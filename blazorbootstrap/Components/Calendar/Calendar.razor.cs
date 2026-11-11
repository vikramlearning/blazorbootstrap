namespace BlazorBootstrap;

/// <summary>
/// An event calendar with Month, Week, Work week, Day, and Agenda views.
/// The app binds a list of events and reacts to callbacks; the calendar never changes an event itself.
/// </summary>
/// <remarks>
/// Bootstrap 5.3 has no calendar component; the calendar is built from Bootstrap classes and a small set of <c>--bb-calendar-*</c> CSS properties.
/// </remarks>
/// <typeparam name="TItem">The type of the events in the calendar.</typeparam>
[AddedVersion("4.1.0")]
public partial class Calendar<TItem> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    /// <summary>
    /// The largest difference between the server's clock and a device's clock that can be a time zone, in hours.
    /// </summary>
    private const int MaxClockOffsetHours = 27;

    private const int MinutesInDay = 24 * 60;

    /// <summary>
    /// The number of event rows a Month day shows before the rest collapse into "+N more".
    /// </summary>
    private const int MonthVisibleLanes = 3;

    private static readonly IReadOnlyList<DayOfWeek> defaultWorkDays =
        new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

    private List<CalendarLayout.AgendaDay> agendaDays = new();

    /// <summary>
    /// How far the clock of the user's device is ahead of the clock of the machine the calendar's code runs on.
    /// It is zero in WebAssembly, and in a Server app until the browser script reports the device's time.
    /// </summary>
    private TimeSpan clockOffset;

    /// <summary>
    /// Counts the changes of <see cref="clockOffset" />, so the hourly grid scrolls to its opening time again when "now" changes.
    /// </summary>
    private int clockVersion;

    private DateOnly currentDate;

    private CalendarView currentView;

    /// <summary>
    /// What each day shows besides its events, worked out once per render.
    /// </summary>
    private readonly Dictionary<DateOnly, CalendarLayout.DayInfo> dayInfos = new();

    private List<CalendarEntry<TItem>> entries = new();

    private bool hasDeviceClock;

    private bool hasParameterSnapshot;

    private HashSet<DayOfWeek> hiddenDays = new();

    private Dictionary<DateOnly, string> holidayNames = new();

    private bool isScriptInitialized;

    private List<CalendarLayout.MonthWeek> monthWeeks = new();

    private (DateOnly Start, DateOnly End, CalendarView View)? notifiedRange;

    /// <summary>
    /// The reference the browser script calls back through after a drag.
    /// </summary>
    private DotNetObjectReference<Calendar<TItem>>? objRef;

    private DateOnly previousDateParameter;

    private DateOnly? previousSelectedDateParameter;

    private CalendarView previousViewParameter;

    /// <summary>
    /// The settings last sent to the browser script, so it is called only when one of them changes.
    /// </summary>
    private (bool HasGrid, int Start, int End, int Slot, string ScrollKey, bool Select, bool Move, bool Resize)? scriptState;

    private DateOnly? selectedDate;

    private CalendarLayout.TimeGrid? timeGrid;

    private string title = "";

    private (DateOnly Start, DateOnly End) visibleRange;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            if (isScriptInitialized)
                await SafeInvokeVoidAsync("window.blazorBootstrap.calendar.dispose", Id);

            objRef?.Dispose();
            objRef = null;
        }

        await base.DisposeAsyncCore(disposing);
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        await SyncScriptAsync();

        // Report the visible range on the first render and whenever navigation or a view switch changes it.
        var range = (visibleRange.Start, visibleRange.End, View: currentView);

        if (notifiedRange.HasValue && notifiedRange.Value == range)
            return;

        notifiedRange = range;

        if (OnVisibleRangeChanged.HasDelegate)
            await OnVisibleRangeChanged.InvokeAsync(new CalendarVisibleRangeChangedEventArgs(range.Start, range.End, range.View));
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        // The calendar keeps its own date and view so navigation works when the app does not bind them;
        // a parameter replaces them only when the app passes a new value.
        if (!hasParameterSnapshot || Date != previousDateParameter)
            currentDate = Date;

        if (!hasParameterSnapshot || View != previousViewParameter)
            currentView = View;

        if (!hasParameterSnapshot || SelectedDate != previousSelectedDateParameter)
            selectedDate = SelectedDate;

        previousDateParameter = Date;
        previousSelectedDateParameter = SelectedDate;
        previousViewParameter = View;
        hasParameterSnapshot = true;

        UpdateLayout();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Called by the calendar's browser script with the date and time of the user's device, when the calendar starts and when the day changes. Not for use by apps.
    /// </summary>
    /// <param name="now">The device's local date and time, in the script's date and time form.</param>
    [AddedVersion("4.1.0")]
    [Description("Called by the calendar's browser script with the date and time of the user's device, when the calendar starts and when the day changes. Not for use by apps.")]
    [JSInvokable]
    public async Task OnClockJS(string now)
    {
        if (!TryParseScriptDateTime(now, out var deviceNow))
            return;

        var offset = deviceNow - DateTime.Now;

        // Clocks less than a minute apart are one clock: WebAssembly, or a server in the user's time zone.
        // No two time zones are more than 26 hours apart, so a larger difference is not a time zone and is ignored.
        if (offset.Duration() < TimeSpan.FromMinutes(1))
            offset = TimeSpan.Zero;
        else if (offset.Duration() > TimeSpan.FromHours(MaxClockOffsetHours))
            return;

        var isFirstReport = !hasDeviceClock;
        var offsetChanged = (offset - clockOffset).Duration() >= TimeSpan.FromMinutes(1);
        var previousToday = Today;

        hasDeviceClock = true;

        // The first report of a device on the server's clock changes nothing. A later report means the day has changed.
        if (isFirstReport && !offsetChanged)
            return;

        if (offsetChanged)
        {
            clockOffset = offset;
            clockVersion++;
        }

        // A calendar that opened on the server's today moves to the user's today.
        if (isFirstReport && currentDate == previousToday && Today != previousToday)
            await SetDateAsync(Today);

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Called by the calendar's browser script when the user moved an event by dragging. Not for use by apps.
    /// </summary>
    /// <param name="key">The key of the event in the render the drag started in.</param>
    /// <param name="originalStart">The start of the event as the script read it when the drag started.</param>
    /// <param name="originalEnd">The end of the event as the script read it when the drag started.</param>
    /// <param name="start">The new start, in the script's date and time form.</param>
    [AddedVersion("4.1.0")]
    [Description("Called by the calendar's browser script when the user moved an event by dragging. Not for use by apps.")]
    [JSInvokable]
    public async Task OnEventMovedJS(int key, string originalStart, string originalEnd, string start)
    {
        // The values come from the browser, so they are checked like any other input: the event must be the one
        // the drag started on, the date must parse, and the change must be one a drag can make.
        if (!AllowEventMove
            || !OnEventMoved.HasDelegate
            || FindDraggedEntry(key, originalStart, originalEnd) is not { } entry
            || !TryParseScriptDateTime(start, out var reportedStart)
            || !TryGetDragChange(entry.Start, reportedStart, out var newStart))
            return;

        // A move keeps the length of the event.
        var length = entry.End - entry.Start;

        if (newStart > DateTime.MaxValue - length)
            return;

        await OnEventMoved.InvokeAsync(new CalendarEventChangedEventArgs<TItem>(entry.Item, newStart, newStart + length));
    }

    /// <summary>
    /// Called by the calendar's browser script when the user resized an event by dragging one of its ends. Not for use by apps.
    /// </summary>
    /// <param name="key">The key of the event in the render the drag started in.</param>
    /// <param name="originalStart">The start of the event as the script read it when the drag started.</param>
    /// <param name="originalEnd">The end of the event as the script read it when the drag started.</param>
    /// <param name="start">The new start, in the script's date and time form.</param>
    /// <param name="end">The new end, in the script's date and time form.</param>
    [AddedVersion("4.1.0")]
    [Description("Called by the calendar's browser script when the user resized an event by dragging one of its ends. Not for use by apps.")]
    [JSInvokable]
    public async Task OnEventResizedJS(int key, string originalStart, string originalEnd, string start, string end)
    {
        if (!AllowEventResize
            || !OnEventResized.HasDelegate
            || FindDraggedEntry(key, originalStart, originalEnd) is not { } entry
            || !TryParseScriptDateTime(start, out var reportedStart)
            || !TryParseScriptDateTime(end, out var reportedEnd))
            return;

        // A resize changes exactly one end. The other comes back as the text the script read, so the texts are compared:
        // an event's own times can be finer than the seconds the script works in.
        var startChanged = !string.Equals(start, entry.StartText, StringComparison.Ordinal);
        var endChanged = !string.Equals(end, entry.EndText, StringComparison.Ordinal);

        if (startChanged == endChanged)
            return;

        var newStart = entry.Start;
        var newEnd = entry.ResizeEnd;

        if (startChanged)
        {
            if (!TryGetDragChange(entry.Start, reportedStart, out newStart))
                return;
        }
        else
        {
            // The script measured the change from the end it read; the event's end for a resize can be a day later.
            var resizeOffset = entry.ResizeEnd - entry.End;

            if (!TryGetDragChange(entry.End, reportedEnd, out var changedEnd) || changedEnd > DateTime.MaxValue - resizeOffset)
                return;

            newEnd = changedEnd + resizeOffset;
        }

        if (newEnd <= newStart)
            return;

        await OnEventResized.InvokeAsync(new CalendarEventChangedEventArgs<TItem>(entry.Item, newStart, newEnd));
    }

    /// <summary>
    /// Called by the calendar's browser script when the user selected a range of days or time slots by dragging. Not for use by apps.
    /// </summary>
    /// <param name="start">The start of the range, in the script's date and time form.</param>
    /// <param name="end">The end of the range, in the script's date and time form.</param>
    /// <param name="isAllDay">Whether whole days were selected rather than time slots.</param>
    [AddedVersion("4.1.0")]
    [Description("Called by the calendar's browser script when the user selected a range of days or time slots by dragging. Not for use by apps.")]
    [JSInvokable]
    public async Task OnRangeSelectedJS(string start, string end, bool isAllDay)
    {
        // The values come from the browser, so they are checked like any other input:
        // the dates must parse, lie inside what the calendar shows, and sit on whole days or whole slots.
        if (!AllowRangeSelection
            || !OnRangeSelected.HasDelegate
            || !TryParseScriptDateTime(start, out var rangeStart)
            || !TryParseScriptDateTime(end, out var rangeEnd)
            || rangeEnd <= rangeStart
            || DateOnly.FromDateTime(rangeStart) < visibleRange.Start
            || rangeEnd > visibleRange.End.ToDateTime(TimeOnly.MinValue).AddDays(1))
            return;

        var step = isAllDay ? MinutesInDay : SlotLength;

        if (rangeStart.TimeOfDay.TotalMinutes % step != 0 || rangeEnd.TimeOfDay.TotalMinutes % step != 0)
            return;

        await OnRangeSelected.InvokeAsync(new CalendarRangeSelectedEventArgs(rangeStart, rangeEnd, isAllDay));
    }

    #endregion

    #region Internal Methods

    /// <summary>
    /// Formats the label of an hour line: "9 AM" on a 12-hour clock, "09:00" on a 24-hour clock.
    /// <see cref="TimeFormat" /> chooses the clock; by default the culture does.
    /// </summary>
    internal string FormatHour(int minutes)
    {
        var culture = EffectiveCulture;
        var time = DateTime.MinValue.AddMinutes(minutes);

        if (TimeFormat == CalendarTimeFormat.TwentyFourHour)
            return time.ToString("HH:mm", culture);

        var usesTwelveHourClock = TimeFormat == CalendarTimeFormat.TwelveHour || culture.DateTimeFormat.ShortTimePattern.Contains('h');

        return usesTwelveHourClock
            ? time.ToString(time.Minute == 0 ? "h tt" : "h:mm tt", TwelveHourCulture)
            : time.ToString("t", culture);
    }

    /// <summary>
    /// Formats a time of day: in the culture's own format, or on the clock <see cref="TimeFormat" /> chooses.
    /// </summary>
    internal string FormatTime(DateTime dateTime) =>
        TimeFormat switch
        {
            CalendarTimeFormat.TwelveHour => dateTime.ToString("h:mm tt", TwelveHourCulture),
            CalendarTimeFormat.TwentyFourHour => dateTime.ToString("HH:mm", EffectiveCulture),
            _ => dateTime.ToString("t", EffectiveCulture)
        };

    /// <summary>
    /// Gets the accessible name of a day: its full date, and its label when a day mark or a holiday gives it one.
    /// </summary>
    internal string GetDayAriaLabel(DateOnly date)
    {
        var label = GetDayInfo(date).Label;
        var text = date.ToString("D", EffectiveCulture);

        return label is null ? text : $"{text}, {label}";
    }

    /// <summary>
    /// Gets what a day shows besides its events: the label, highlight, and shading that the app's day marks and the holiday list give it.
    /// </summary>
    internal CalendarLayout.DayInfo GetDayInfo(DateOnly date)
    {
        if (!dayInfos.TryGetValue(date, out var info))
            dayInfos[date] = info = BuildDayInfo(date);

        return info;
    }

    /// <summary>
    /// Gets the color class of the dot shown beside an event in a list.
    /// </summary>
    internal static string GetDotClass(CalendarEntry<TItem> entry) =>
        entry.Color == CalendarColor.None ? "bg-secondary" : entry.Color.ToCalendarEventColorClass();

    /// <summary>
    /// Gets the accessible name of an event: its title, its date or dates, and its times or the all-day text.
    /// A screen reader reads it in place of the event's visible content, which is often cut short.
    /// </summary>
    internal string GetEventAriaLabel(CalendarEntry<TItem> entry)
    {
        var culture = EffectiveCulture;
        var firstDay = entry.FirstDay.ToString("D", culture);
        string when;

        if (entry.FirstDay == entry.LastDay)
        {
            when = $"{firstDay}, {GetEventTimeText(entry, entry.FirstDay)}";
        }
        else
        {
            var lastDay = entry.LastDay.ToString("D", culture);

            when = entry.IsAllDay
                ? $"{firstDay} – {lastDay}, {AllDayText}"
                : $"{firstDay}, {FormatTime(entry.Start)} – {lastDay}, {FormatTime(entry.End)}";
        }

        return string.IsNullOrWhiteSpace(entry.Title) ? when : $"{entry.Title}, {when}";
    }

    /// <summary>
    /// Gets the time shown beside an event in the list of one day: "All day", its start and end,
    /// or for a timed event that covers several days, its start on the first day and its end on the last.
    /// </summary>
    internal string GetEventTimeText(CalendarEntry<TItem> entry, DateOnly day)
    {
        if (entry.IsAllDay)
            return AllDayText;

        if (entry.FirstDay == entry.LastDay)
            return entry.End > entry.Start ? $"{FormatTime(entry.Start)} \u2013 {FormatTime(entry.End)}" : FormatTime(entry.Start);

        if (day == entry.FirstDay)
            return $"{FormatTime(entry.Start)} \u2013";

        return day == entry.LastDay ? $"\u2013 {FormatTime(entry.End)}" : AllDayText;
    }

    /// <summary>
    /// Gets the "+N more" text for a day with hidden events.
    /// </summary>
    internal string GetMoreText(int count) => string.Format(EffectiveCulture, MoreTextFormat, count);

    /// <summary>
    /// Gets the accessible name of a time slot: its full date and its start and end times.
    /// </summary>
    internal string GetSlotAriaLabel(DateOnly date, int minutes)
    {
        var start = date.ToDateTime(TimeOnly.MinValue).AddMinutes(minutes);

        return $"{date.ToString("D", EffectiveCulture)}, {FormatTime(start)} – {FormatTime(start.AddMinutes(SlotLength))}";
    }

    /// <summary>
    /// Gets the number of the week that starts on a day, by the rule the app chose.
    /// </summary>
    internal int GetWeekNumber(DateOnly weekStart) =>
        CalendarLayout.GetWeekNumber(weekStart, EffectiveCulture, EffectiveFirstDayOfWeek, WeekNumberRule);

    /// <summary>
    /// Selects a day and tells the app it was clicked.
    /// </summary>
    internal async Task HandleDayClickAsync(DateOnly date)
    {
        if (selectedDate != date)
        {
            selectedDate = date;

            if (SelectedDateChanged.HasDelegate)
                await SelectedDateChanged.InvokeAsync(date);
        }

        if (OnDayClick.HasDelegate)
            await OnDayClick.InvokeAsync(new CalendarDayClickEventArgs(date));
    }

    /// <summary>
    /// Tells the app an event was clicked. The calendar shows no form of its own.
    /// </summary>
    internal Task HandleEventClickAsync(CalendarEntry<TItem> entry) =>
        OnEventClick.HasDelegate
            ? OnEventClick.InvokeAsync(new CalendarEventClickEventArgs<TItem>(entry.Item))
            : Task.CompletedTask;

    /// <summary>
    /// Tells the app an empty time slot was clicked.
    /// </summary>
    internal Task HandleSlotClickAsync(DateOnly date, int minutes)
    {
        if (!OnSlotClick.HasDelegate)
            return Task.CompletedTask;

        var start = date.ToDateTime(TimeOnly.MinValue).AddMinutes(minutes);

        return OnSlotClick.InvokeAsync(new CalendarSlotClickEventArgs(start, start.AddMinutes(SlotLength)));
    }

    /// <summary>
    /// Tells whether a time slot lies inside working hours on a working day that is not marked as non-working or as a holiday.
    /// </summary>
    internal bool IsWorkingSlot(DateOnly date, int minutes) =>
        !GetDayInfo(date).IsNonWorking
        && (WorkDays ?? defaultWorkDays).Contains(date.DayOfWeek)
        && minutes >= ToMinutes(WorkDayStart)
        && minutes < (WorkDayEnd > WorkDayStart ? ToMinutes(WorkDayEnd) : MinutesInDay);

    /// <summary>
    /// Renders the app's template for an event. Call it only when <see cref="EventTemplate" /> is set.
    /// </summary>
    internal RenderFragment RenderEventTemplate(CalendarEntry<TItem> entry) =>
        EventTemplate!(new CalendarEventContext<TItem>(entry.Item, entry.Title, entry.Start, entry.End, entry.IsAllDay, currentView));

    #endregion

    #region Private Methods

    private CalendarLayout.DayInfo BuildDayInfo(DateOnly date)
    {
        var mark = DayMarkProvider?.Invoke(date);
        var hasHoliday = holidayNames.TryGetValue(date, out var holiday);

        if (mark is null && !hasHoliday)
            return CalendarLayout.DayInfo.None;

        // A holiday is labelled with its name and shaded; the app's own mark for the same day adds to it.
        var label = hasHoliday
            ? (string.IsNullOrWhiteSpace(mark?.Label) ? holiday : $"{holiday}, {mark!.Label}")
            : mark!.Label;
        var color = mark is { Color: not CalendarColor.None } ? mark.Color : hasHoliday ? CalendarColor.Danger : CalendarColor.None;

        return new CalendarLayout.DayInfo(
            string.IsNullOrWhiteSpace(label) ? null : label,
            color.ToCalendarDayHighlightClass(),
            color.ToCalendarDayLabelClass(),
            hasHoliday || (mark?.IsNonWorking ?? false));
    }

    /// <summary>
    /// Finds the event a drag started on. The key is the event's place in the list, and the app can replace the list
    /// while the user drags, so the times the script read when the drag started must still be the event's times.
    /// </summary>
    private CalendarEntry<TItem>? FindDraggedEntry(int key, string? originalStart, string? originalEnd)
    {
        if (key < 0 || key >= entries.Count)
            return null;

        var entry = entries[key];

        return string.Equals(originalStart, entry.StartText, StringComparison.Ordinal)
               && string.Equals(originalEnd, entry.EndText, StringComparison.Ordinal)
            ? entry
            : null;
    }

    /// <summary>
    /// Gets the time the hourly grid opens at: an hour before now when today is shown, otherwise the start of the working day.
    /// </summary>
    private int GetOpeningMinutes()
    {
        var showsToday = timeGrid is not null && timeGrid.Days.Contains(Today);
        var minutes = showsToday ? (int)NowMinutes - 60 : ToMinutes(WorkDayStart);

        return Math.Clamp(minutes - minutes % SlotLength, VisibleStartMinutes, Math.Max(VisibleStartMinutes, VisibleEndMinutes - SlotLength));
    }

    private Task GoToTodayAsync() => SetDateAsync(Today);

    private Task NavigateAsync(int direction) =>
        SetDateAsync(CalendarLayout.AddPeriod(currentView, currentDate, direction, EffectiveCulture, AgendaDays));

    private async Task SetDateAsync(DateOnly date)
    {
        if (date == currentDate)
            return;

        currentDate = date;
        UpdateLayout();

        if (DateChanged.HasDelegate)
            await DateChanged.InvokeAsync(date);
    }

    private async Task SetViewAsync(CalendarView view)
    {
        if (view == currentView)
            return;

        currentView = view;
        UpdateLayout();

        if (ViewChanged.HasDelegate)
            await ViewChanged.InvokeAsync(view);
    }

    /// <summary>
    /// Sends the calendar's settings to the browser script when one of them changed. The script handles dragging,
    /// keeps the current-time line moving, and scrolls the hourly grid to its opening time once per visible period.
    /// </summary>
    private async Task SyncScriptAsync()
    {
        var hasGrid = timeGrid is not null;
        var scrollKey = hasGrid ? $"{currentView}:{visibleRange.Start.DayNumber}:{clockVersion}" : "";
        var state = (hasGrid, VisibleStartMinutes, VisibleEndMinutes, SlotLength, scrollKey, AllowRangeSelection, AllowEventMove, AllowEventResize);

        if (scriptState.HasValue && scriptState.Value == state)
            return;

        scriptState = state;

        var options = new
        {
            hasGrid,
            dayStart = VisibleStartMinutes,
            dayEnd = VisibleEndMinutes,
            slotMinutes = SlotLength,
            scrollKey,
            scrollToMinutes = hasGrid ? GetOpeningMinutes() : 0,
            allowRangeSelection = AllowRangeSelection,
            allowEventMove = AllowEventMove,
            allowEventResize = AllowEventResize
        };

        if (isScriptInitialized)
        {
            await SafeInvokeVoidAsync("window.blazorBootstrap.calendar.update", Id, options);

            return;
        }

        // Set before the call, so a calendar removed while the call is on its way still disposes its script.
        isScriptInitialized = true;
        objRef ??= DotNetObjectReference.Create(this);
        await SafeInvokeVoidAsync("window.blazorBootstrap.calendar.initialize", Id, objRef, options);
    }

    private static int ToMinutes(TimeOnly time) => time.Hour * 60 + time.Minute;

    /// <summary>
    /// Works out the new value of one end of an event from what the browser script reports after a drag.
    /// A change by whole days keeps the event's exact time of day (Month, and the all-day row);
    /// any other change must end on a slot of the hourly grid. Agenda has no drag.
    /// </summary>
    private bool TryGetDragChange(DateTime original, DateTime reported, out DateTime result)
    {
        result = original;

        // The script works in whole seconds.
        var change = reported - original.AddTicks(-(original.Ticks % TimeSpan.TicksPerSecond));

        if (change == TimeSpan.Zero
            || currentView == CalendarView.Agenda
            || Math.Abs(change.TotalDays) > VisibleDayCount
            || original > DateTime.MaxValue - change.Duration()
            || original < DateTime.MinValue + change.Duration())
            return false;

        if (change.Ticks % TimeSpan.TicksPerDay == 0)
        {
            result = original + change;

            return true;
        }

        if (timeGrid is null || reported.TimeOfDay.Ticks % TimeSpan.FromMinutes(SlotLength).Ticks != 0)
            return false;

        result = reported;

        return true;
    }

    /// <summary>
    /// Reads a date and time sent by the browser script. Only the exact culture-free form is accepted.
    /// </summary>
    private static bool TryParseScriptDateTime(string? value, out DateTime result) =>
        DateTime.TryParseExact(value, CalendarEntry<TItem>.ScriptDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);

    private void UpdateLayout()
    {
        var culture = EffectiveCulture;
        var firstDayOfWeek = EffectiveFirstDayOfWeek;

        visibleRange = CalendarLayout.GetVisibleRange(currentView, currentDate, culture, firstDayOfWeek, WorkDays ?? defaultWorkDays, AgendaDays);
        title = CalendarLayout.GetTitle(currentView, currentDate, visibleRange.Start, visibleRange.End, culture);
        entries = CalendarLayout.BuildEntries(Data, StartField, EndField, TitleField, AllDayField, ColorField);

        // Hiding every weekday would leave nothing to show, so it hides none.
        hiddenDays = new HashSet<DayOfWeek>(HiddenDays ?? Array.Empty<DayOfWeek>());

        if (hiddenDays.Count >= 7)
            hiddenDays.Clear();

        // Two holidays on one day share the label.
        holidayNames = (Holidays ?? Array.Empty<CalendarHoliday>())
            .Where(item => item is not null && !string.IsNullOrWhiteSpace(item.Name))
            .GroupBy(item => item.Date)
            .ToDictionary(group => group.Key, group => string.Join(", ", group.Select(item => item.Name)));

        dayInfos.Clear();

        monthWeeks = currentView == CalendarView.Month
            ? CalendarLayout.BuildMonthWeeks(visibleRange.Start, visibleRange.End, entries, MonthVisibleLanes, hiddenDays)
            : new List<CalendarLayout.MonthWeek>();

        agendaDays = currentView == CalendarView.Agenda
            ? CalendarLayout.BuildAgenda(visibleRange.Start, visibleRange.End, entries, hiddenDays, holidayNames.ContainsKey)
            : new List<CalendarLayout.AgendaDay>();

        timeGrid = currentView is CalendarView.Week or CalendarView.WorkWeek or CalendarView.Day
            ? CalendarLayout.BuildTimeGrid(
                CalendarLayout.GetViewDays(currentView, visibleRange.Start, visibleRange.End, WorkDays ?? defaultWorkDays, hiddenDays),
                entries,
                VisibleStartMinutes,
                VisibleEndMinutes,
                SlotLength)
            : null;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the number of days the Agenda view lists.
    /// <para>
    /// Default value is 7.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(7)]
    [Description("Gets or sets the number of days the Agenda view lists.")]
    [Parameter]
    public int AgendaDays { get; set; } = 7;

    internal IReadOnlyList<CalendarLayout.AgendaDay> AgendaLayout => agendaDays;

    /// <summary>
    /// Gets or sets the text of the Agenda view button.
    /// <para>
    /// Default value is 'Agenda'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Agenda")]
    [Description("Gets or sets the text of the Agenda view button.")]
    [Parameter]
    public string AgendaText { get; set; } = "Agenda";

    /// <summary>
    /// Gets or sets a function that tells whether an event is an all-day event.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a function that tells whether an event is an all-day event.")]
    [Parameter]
    public Func<TItem, bool>? AllDayField { get; set; }

    /// <summary>
    /// Gets or sets the label of the all-day row in the Week, Work week, and Day views.
    /// <para>
    /// Default value is 'All day'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("All day")]
    [Description("Gets or sets the label of the all-day row in the Week, Work week, and Day views.")]
    [Parameter]
    public string AllDayText { get; set; } = "All day";

    /// <summary>
    /// Gets or sets whether the user can move an event by dragging it. The calendar reports the new times through
    /// <see cref="OnEventMoved" />; the event moves only when the app updates its list.
    /// <para>
    /// Default value is <see langword="true" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(true)]
    [Description("Gets or sets whether the user can move an event by dragging it. The calendar reports the new times through OnEventMoved; the event moves only when the app updates its list.")]
    [Parameter]
    public bool AllowEventMove { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the user can resize an event by dragging one of its ends. The calendar reports the new times through
    /// <see cref="OnEventResized" />; the event changes only when the app updates its list.
    /// <para>
    /// Default value is <see langword="true" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(true)]
    [Description("Gets or sets whether the user can resize an event by dragging one of its ends. The calendar reports the new times through OnEventResized; the event changes only when the app updates its list.")]
    [Parameter]
    public bool AllowEventResize { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the user can select a range of days or time slots by dragging. The calendar reports the range through
    /// <see cref="OnRangeSelected" />.
    /// <para>
    /// Default value is <see langword="true" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(true)]
    [Description("Gets or sets whether the user can select a range of days or time slots by dragging. The calendar reports the range through OnRangeSelected.")]
    [Parameter]
    public bool AllowRangeSelection { get; set; } = true;

    /// <summary>
    /// Gets or sets the accessible name of the calendar, read by screen readers.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the accessible name of the calendar, read by screen readers.")]
    [Parameter]
    public string? AriaLabel { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("bb-calendar", true),
            (CompactBreakpoint.ToCalendarCompactClass(), CompactBreakpoint != CalendarCompactBreakpoint.None),
            ("bb-calendar-selectable", AllowRangeSelection),
            ("bb-calendar-movable", AllowEventMove),
            ("bb-calendar-resizable", AllowEventResize));

    /// <summary>
    /// Gets or sets the accessible name of the button that closes a day's full event list.
    /// <para>
    /// Default value is 'Close'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Close")]
    [Description("Gets or sets the accessible name of the button that closes a day's full event list.")]
    [Parameter]
    public string CloseText { get; set; } = "Close";

    /// <summary>
    /// Gets or sets a function that returns the color of an event.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a function that returns the color of an event.")]
    [Parameter]
    public Func<TItem, CalendarColor>? ColorField { get; set; }

    /// <summary>
    /// Gets or sets the width below which the calendar shows its compact layout: the view switch becomes a list,
    /// Month shows a marker on each day that has events and lists the selected day's events below the grid,
    /// and Week and Work week keep readable columns and scroll sideways inside the calendar.
    /// <para>
    /// Default value is <see cref="CalendarCompactBreakpoint.Medium" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(CalendarCompactBreakpoint.Medium)]
    [Description("Gets or sets the width below which the calendar shows its compact layout.")]
    [Parameter]
    public CalendarCompactBreakpoint CompactBreakpoint { get; set; } = CalendarCompactBreakpoint.Medium;

    /// <summary>
    /// Gets or sets the culture that supplies month and day names, date and time formats, the first day of the week, and the calendar system.
    /// <para>
    /// Default value is <see langword="null" />, which uses <see cref="CultureInfo.CurrentCulture" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the culture that supplies month and day names, date and time formats, the first day of the week, and the calendar system. When null, the current culture is used.")]
    [Parameter]
    public CultureInfo? Culture { get; set; }

    internal DateOnly CurrentDate => currentDate;

    internal CalendarView CurrentView => currentView;

    /// <summary>
    /// Gets or sets the events.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the events.")]
    [Parameter]
    public IEnumerable<TItem>? Data { get; set; }

    /// <summary>
    /// Gets or sets the date the calendar shows: the visible period is the one that contains it.
    /// <para>
    /// Default value is today.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Gets or sets the date the calendar shows: the visible period is the one that contains it. Default value is today.")]
    [Parameter]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// This event fires when navigation changes the date the calendar shows.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when navigation changes the date the calendar shows.")]
    [Parameter]
    public EventCallback<DateOnly> DateChanged { get; set; }

    /// <summary>
    /// Gets or sets the time the hourly grid of the Week, Work week, and Day views ends at.
    /// A value that is not after <see cref="DayStart" /> means the end of the day.
    /// <para>
    /// Default value is 00:00, the end of the day.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Gets or sets the time the hourly grid of the Week, Work week, and Day views ends at. A value that is not after DayStart means the end of the day. Default value is 00:00, the end of the day.")]
    [Parameter]
    public TimeOnly DayEnd { get; set; } = TimeOnly.MinValue;

    /// <summary>
    /// Gets or sets a function that marks a day: a label such as "Office closed", a highlight color, and non-working shading.
    /// It is called for each day the calendar shows; return <see langword="null" /> for a day with no mark.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a function that marks a day: a label, a highlight color, and non-working shading. It is called for each day the calendar shows; return null for a day with no mark.")]
    [Parameter]
    public Func<DateOnly, CalendarDayMark?>? DayMarkProvider { get; set; }

    /// <summary>
    /// Gets or sets the time the hourly grid of the Week, Work week, and Day views starts at.
    /// <para>
    /// Default value is 00:00.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Gets or sets the time the hourly grid of the Week, Work week, and Day views starts at. Default value is 00:00.")]
    [Parameter]
    public TimeOnly DayStart { get; set; } = TimeOnly.MinValue;

    /// <summary>
    /// Gets or sets the text of the Day view button.
    /// <para>
    /// Default value is 'Day'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Day")]
    [Description("Gets or sets the text of the Day view button.")]
    [Parameter]
    public string DayText { get; set; } = "Day";

    internal CultureInfo EffectiveCulture => Culture ?? CultureInfo.CurrentCulture;

    /// <summary>
    /// Gets the first day of the week: <see cref="FirstDayOfWeek" /> when set, otherwise the culture's.
    /// </summary>
    internal DayOfWeek EffectiveFirstDayOfWeek => CalendarLayout.GetFirstDayOfWeek(EffectiveCulture, FirstDayOfWeek);

    /// <summary>
    /// Gets or sets the text shown where there are no events: in the Agenda view for a period without events,
    /// and in the compact Month view for a selected day without events.
    /// <para>
    /// Default value is 'No events'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("No events")]
    [Description("Gets or sets the text shown where there are no events: in the Agenda view for a period without events, and in the compact Month view for a selected day without events.")]
    [Parameter]
    public string EmptyText { get; set; } = "No events";

    /// <summary>
    /// Gets or sets a function that returns the end of an event. An end at midnight is exclusive:
    /// an event from 1 November 00:00 to 3 November 00:00 covers 1 and 2 November.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a function that returns the end of an event. An end at midnight is exclusive: an event from 1 November 00:00 to 3 November 00:00 covers 1 and 2 November.")]
    [Parameter]
    public Func<TItem, DateTime>? EndField { get; set; }

    internal IReadOnlyList<CalendarEntry<TItem>> Entries => entries;

    /// <summary>
    /// Gets or sets a template that replaces the content of every event. Its context carries the event's item, title, start, end, and the view it is drawn in.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a template that replaces the content of every event. Its context carries the event's item, title, start, end, and the view it is drawn in.")]
    [Parameter]
    public RenderFragment<CalendarEventContext<TItem>>? EventTemplate { get; set; }

    /// <summary>
    /// Gets or sets the first day of the week. It overrides the culture's first day without changing the culture.
    /// <para>
    /// Default value is <see langword="null" />, which uses the culture's first day.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the first day of the week. It overrides the culture's first day without changing the culture. When null, the culture's first day is used.")]
    [Parameter]
    public DayOfWeek? FirstDayOfWeek { get; set; }

    /// <summary>
    /// Gets or sets the height in pixels of the hourly grid of the Week, Work week, and Day views. The grid scrolls inside it.
    /// A value of 0 or less shows the whole grid.
    /// <para>
    /// Default value is 600.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(600d)]
    [Description("Gets or sets the height in pixels of the hourly grid of the Week, Work week, and Day views. The grid scrolls inside it. A value of 0 or less shows the whole grid.")]
    [Parameter]
    public double Height { get; set; } = 600;

    /// <summary>
    /// Gets or sets the weekdays the calendar does not show, for example Saturday and Sunday. An event on a hidden day is not drawn;
    /// an event that covers several days keeps its part on the days that are shown. The Day view shows its day whatever is hidden.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the weekdays the calendar does not show, for example Saturday and Sunday. An event on a hidden day is not drawn; an event that covers several days keeps its part on the days that are shown.")]
    [Parameter]
    public IReadOnlyList<DayOfWeek>? HiddenDays { get; set; }

    /// <summary>
    /// Gets or sets the holidays. The calendar labels each with its name and shades it as a non-working day in every view.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the holidays. The calendar labels each with its name and shades it as a non-working day in every view.")]
    [Parameter]
    public IReadOnlyList<CalendarHoliday>? Holidays { get; set; }

    internal int MonthLaneCount => MonthVisibleLanes;

    /// <summary>
    /// Gets or sets the text of the Month view button.
    /// <para>
    /// Default value is 'Month'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Month")]
    [Description("Gets or sets the text of the Month view button.")]
    [Parameter]
    public string MonthText { get; set; } = "Month";

    internal IReadOnlyList<CalendarLayout.MonthWeek> MonthWeeks => monthWeeks;

    /// <summary>
    /// Gets or sets the format of the text shown on a Month day with more events than fit. <c>{0}</c> is the number of hidden events.
    /// <para>
    /// Default value is '+{0} more'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("+{0} more")]
    [Description("Gets or sets the format of the text shown on a Month day with more events than fit. {0} is the number of hidden events.")]
    [Parameter]
    public string MoreTextFormat { get; set; } = "+{0} more";

    /// <summary>
    /// Gets or sets the accessible name of the next button.
    /// <para>
    /// Default value is 'Next'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Next")]
    [Description("Gets or sets the accessible name of the next button.")]
    [Parameter]
    public string NextText { get; set; } = "Next";

    /// <summary>
    /// Gets the current date and time on the clock of the user's device: the browser's clock, in WebAssembly and in a Server app alike.
    /// </summary>
    private DateTime Now => DateTime.Now + clockOffset;

    /// <summary>
    /// Gets the current time as minutes of the day, on the clock of the user's device.
    /// </summary>
    internal double NowMinutes => Now.TimeOfDay.TotalMinutes;

    /// <summary>
    /// This event fires when the user clicks a day.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user clicks a day.")]
    [Parameter]
    public EventCallback<CalendarDayClickEventArgs> OnDayClick { get; set; }

    /// <summary>
    /// This event fires when the user clicks an event. Open your own form here; the calendar shows none.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user clicks an event. Open your own form here; the calendar shows none.")]
    [Parameter]
    public EventCallback<CalendarEventClickEventArgs<TItem>> OnEventClick { get; set; }

    /// <summary>
    /// This event fires when the user moved an event by dragging it. It carries the event and its new start and end.
    /// Update your list to apply the move; if you do not, the event stays where it was.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user moved an event by dragging it. It carries the event and its new start and end. Update your list to apply the move; if you do not, the event stays where it was.")]
    [Parameter]
    public EventCallback<CalendarEventChangedEventArgs<TItem>> OnEventMoved { get; set; }

    /// <summary>
    /// This event fires when the user resized an event by dragging one of its ends. It carries the event and its new start and end.
    /// Update your list to apply the change; if you do not, the event stays as it was.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user resized an event by dragging one of its ends. It carries the event and its new start and end. Update your list to apply the change; if you do not, the event stays as it was.")]
    [Parameter]
    public EventCallback<CalendarEventChangedEventArgs<TItem>> OnEventResized { get; set; }

    /// <summary>
    /// This event fires when the user selected a range of days or time slots by dragging. Open your own form here to create an event in the range.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user selected a range of days or time slots by dragging. Open your own form here to create an event in the range.")]
    [Parameter]
    public EventCallback<CalendarRangeSelectedEventArgs> OnRangeSelected { get; set; }

    /// <summary>
    /// This event fires when the user clicks an empty time slot in the Week, Work week, or Day view.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user clicks an empty time slot in the Week, Work week, or Day view.")]
    [Parameter]
    public EventCallback<CalendarSlotClickEventArgs> OnSlotClick { get; set; }

    /// <summary>
    /// This event fires after the first render and whenever navigation or a view switch changes the dates the calendar shows.
    /// Use it to load the events of the visible range.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires after the first render and whenever navigation or a view switch changes the dates the calendar shows. Use it to load the events of the visible range.")]
    [Parameter]
    public EventCallback<CalendarVisibleRangeChangedEventArgs> OnVisibleRangeChanged { get; set; }

    /// <summary>
    /// Gets the time the hourly grid opens at. The slot at this time is also where the Tab key enters the grid.
    /// </summary>
    internal int OpeningMinutes => GetOpeningMinutes();

    /// <summary>
    /// Gets or sets the accessible name of the previous button.
    /// <para>
    /// Default value is 'Previous'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Previous")]
    [Description("Gets or sets the accessible name of the previous button.")]
    [Parameter]
    public string PreviousText { get; set; } = "Previous";

    /// <summary>
    /// Gets or sets the selected day. Clicking a day selects it.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the selected day. Clicking a day selects it.")]
    [Parameter]
    public DateOnly? SelectedDate { get; set; }

    /// <summary>
    /// This event fires when the user selects a day.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user selects a day.")]
    [Parameter]
    public EventCallback<DateOnly?> SelectedDateChanged { get; set; }

    internal DateOnly? SelectedDay => selectedDate;

    /// <summary>
    /// Gets the length of a time slot in minutes: <see cref="SlotMinutes" /> when it is 15, 30, or 60, otherwise 30.
    /// </summary>
    /// <summary>
    /// Gets or sets whether the calendar shows week numbers: beside each week in the Month view, and in the corner of the Week, Work week, and Day views.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the calendar shows week numbers: beside each week in the Month view, and in the corner of the Week, Work week, and Day views.")]
    [Parameter]
    public bool ShowWeekNumbers { get; set; }

    internal int SlotLength => SlotMinutes is 15 or 60 ? SlotMinutes : 30;

    /// <summary>
    /// Gets or sets the length of a time slot in the Week, Work week, and Day views: 15, 30, or 60 minutes. Any other value means 30.
    /// <para>
    /// Default value is 30.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(30)]
    [Description("Gets or sets the length of a time slot in the Week, Work week, and Day views: 15, 30, or 60 minutes. Any other value means 30.")]
    [Parameter]
    public int SlotMinutes { get; set; } = 30;

    /// <summary>
    /// Gets or sets a function that returns the start of an event.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a function that returns the start of an event.")]
    [Parameter]
    public Func<TItem, DateTime>? StartField { get; set; }

    /// <summary>
    /// Gets or sets how times of day are shown: in the culture's format, on a 12-hour clock, or on a 24-hour clock.
    /// It overrides the culture's time format without changing the culture.
    /// <para>
    /// Default value is <see cref="CalendarTimeFormat.Culture" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(CalendarTimeFormat.Culture)]
    [Description("Gets or sets how times of day are shown: in the culture's format, on a 12-hour clock, or on a 24-hour clock. It overrides the culture's time format without changing the culture.")]
    [Parameter]
    public CalendarTimeFormat TimeFormat { get; set; } = CalendarTimeFormat.Culture;

    internal CalendarLayout.TimeGrid? TimeGridLayout => timeGrid;

    /// <summary>
    /// Gets or sets a function that returns the title of an event.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a function that returns the title of an event.")]
    [Parameter]
    public Func<TItem, string?>? TitleField { get; set; }

    /// <summary>
    /// Gets today's date on the clock of the user's device.
    /// </summary>
    internal DateOnly Today => DateOnly.FromDateTime(Now);

    /// <summary>
    /// Gets or sets the text of the Today button.
    /// <para>
    /// Default value is 'Today'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Today")]
    [Description("Gets or sets the text of the Today button.")]
    [Parameter]
    public string TodayText { get; set; } = "Today";

    /// <summary>
    /// Gets the culture whose AM and PM texts a 12-hour clock uses. A culture that has none cannot tell 9 in the morning
    /// from 9 at night on a 12-hour clock, so the culture-free "AM" and "PM" are used for it.
    /// </summary>
    private CultureInfo TwelveHourCulture =>
        string.IsNullOrEmpty(EffectiveCulture.DateTimeFormat.AMDesignator) ? CultureInfo.InvariantCulture : EffectiveCulture;

    /// <summary>
    /// Gets or sets the view the calendar shows.
    /// <para>
    /// Default value is <see cref="CalendarView.Month" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(CalendarView.Month)]
    [Description("Gets or sets the view the calendar shows.")]
    [Parameter]
    public CalendarView View { get; set; } = CalendarView.Month;

    /// <summary>
    /// This event fires when the user switches the view.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user switches the view.")]
    [Parameter]
    public EventCallback<CalendarView> ViewChanged { get; set; }

    /// <summary>
    /// Gets or sets the accessible name of the view switch.
    /// <para>
    /// Default value is 'View'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("View")]
    [Description("Gets or sets the accessible name of the view switch.")]
    [Parameter]
    public string ViewText { get; set; } = "View";

    /// <summary>
    /// Gets the number of days the calendar shows. A drag cannot move an event further than this.
    /// </summary>
    private int VisibleDayCount => visibleRange.End.DayNumber - visibleRange.Start.DayNumber + 1;

    /// <summary>
    /// Gets the minute of the day the time grid ends at: <see cref="DayEnd" /> rounded up to a whole slot, or the end of the day.
    /// </summary>
    internal int VisibleEndMinutes
    {
        get
        {
            if (DayEnd <= DayStart)
                return MinutesInDay;

            var minutes = ToMinutes(DayEnd);
            var remainder = (minutes - VisibleStartMinutes) % SlotLength;

            return Math.Min(MinutesInDay, remainder == 0 ? minutes : minutes + SlotLength - remainder);
        }
    }

    /// <summary>
    /// Gets the minute of the day the time grid starts at: <see cref="DayStart" /> rounded down to a whole slot.
    /// </summary>
    internal int VisibleStartMinutes => ToMinutes(DayStart) - ToMinutes(DayStart) % SlotLength;

    /// <summary>
    /// Gets or sets how weeks are numbered: by the culture's rule, or by ISO 8601.
    /// <para>
    /// Default value is <see cref="CalendarWeekNumberRule.Culture" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(CalendarWeekNumberRule.Culture)]
    [Description("Gets or sets how weeks are numbered: by the culture's rule, or by ISO 8601.")]
    [Parameter]
    public CalendarWeekNumberRule WeekNumberRule { get; set; } = CalendarWeekNumberRule.Culture;

    /// <summary>
    /// Gets or sets the short text that heads the week numbers.
    /// <para>
    /// Default value is 'Wk'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Wk")]
    [Description("Gets or sets the short text that heads the week numbers.")]
    [Parameter]
    public string WeekNumberText { get; set; } = "Wk";

    /// <summary>
    /// Gets or sets the text of the Week view button.
    /// <para>
    /// Default value is 'Week'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Week")]
    [Description("Gets or sets the text of the Week view button.")]
    [Parameter]
    public string WeekText { get; set; } = "Week";

    /// <summary>
    /// Gets or sets the time the working day ends at. Time after it is shaded in the Week, Work week, and Day views.
    /// <para>
    /// Default value is 17:00.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Gets or sets the time the working day ends at. Time after it is shaded in the Week, Work week, and Day views. Default value is 17:00.")]
    [Parameter]
    public TimeOnly WorkDayEnd { get; set; } = new(17, 0);

    /// <summary>
    /// Gets or sets the working days the Work week view shows.
    /// <para>
    /// Default value is <see langword="null" />, which means Monday to Friday.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the working days the Work week view shows. When null, Monday to Friday.")]
    [Parameter]
    public IReadOnlyList<DayOfWeek>? WorkDays { get; set; }

    /// <summary>
    /// Gets or sets the time the working day starts at. Time before it is shaded in the Week, Work week, and Day views,
    /// and the hourly grid opens there when today is not shown.
    /// <para>
    /// Default value is 09:00.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Gets or sets the time the working day starts at. Time before it is shaded in the Week, Work week, and Day views, and the hourly grid opens there when today is not shown. Default value is 09:00.")]
    [Parameter]
    public TimeOnly WorkDayStart { get; set; } = new(9, 0);

    /// <summary>
    /// Gets or sets the text of the Work week view button.
    /// <para>
    /// Default value is 'Work week'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Work week")]
    [Description("Gets or sets the text of the Work week view button.")]
    [Parameter]
    public string WorkWeekText { get; set; } = "Work week";

    #endregion
}
