namespace BlazorBootstrap;

/// <summary>
/// The Month view of a <see cref="Calendar{TItem}" />: a grid of weeks with day numbers, event bars, and "+N more".
/// The calendar renders it; apps do not use it on its own.
/// </summary>
/// <typeparam name="TItem">The type of the events in the calendar.</typeparam>
[AddedVersion("4.1.0")]
public partial class CalendarMonthView<TItem> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    /// <summary>
    /// The day whose full event list is open, or <see langword="null" /> when none is.
    /// </summary>
    private DateOnly? moreListDate;

    /// <summary>
    /// The element inside this view to move keyboard focus to after the next render, as a CSS selector, or <see langword="null" />.
    /// </summary>
    private string? pendingFocusSelector;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (pendingFocusSelector is null)
            return;

        var selector = pendingFocusSelector;
        pendingFocusSelector = null;

        await SafeInvokeVoidAsync("window.blazorBootstrap.calendar.focusIn", Id, selector);
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        // Navigation can remove the day whose list is open.
        if (moreListDate is { } date && !Owner.MonthWeeks.Any(week => week.Days.Contains(date)))
            moreListDate = null;
    }

    #endregion

    #region Private Methods

    private void CloseMoreList() => moreListDate = null;

    /// <summary>
    /// Closes the day's event list and returns keyboard focus to the "+N more" button that opened it.
    /// </summary>
    private void CloseMoreListAndFocus()
    {
        if (moreListDate is { } date)
            pendingFocusSelector = $".bb-calendar-more[data-bb-more-date='{date.ToString(CalendarEntry<TItem>.ScriptDateFormat, CultureInfo.InvariantCulture)}']";

        CloseMoreList();
    }

    /// <summary>
    /// Gets the classes of a day cell. One background wins, in this order: the app's highlight or a holiday,
    /// today, non-working shading, then the dimming of a day outside the month.
    /// </summary>
    private string GetDayClass(DateOnly date, CalendarLayout.DayInfo info)
    {
        var isToday = date == Owner.Today;
        var isOutside = !CalendarLayout.IsSameMonth(date, Owner.CurrentDate, Owner.EffectiveCulture);

        return BuildClassNames(
            ("bb-calendar-day", true),
            (info.HighlightClass, info.HasHighlight),
            ("bb-calendar-day-today", isToday),
            ("bg-primary-subtle", isToday && !info.HasHighlight),
            ("bb-calendar-day-nonworking", info.IsNonWorking),
            ("bb-calendar-day-outside", isOutside),
            ("bg-body-tertiary", !info.HasHighlight && !isToday && (info.IsNonWorking || isOutside)),
            ("bb-calendar-day-selected", Owner.SelectedDay == date));
    }

    private string GetDayNumberClass(DateOnly date)
    {
        var isToday = date == Owner.Today;
        var isOutside = !CalendarLayout.IsSameMonth(date, Owner.CurrentDate, Owner.EffectiveCulture);

        return BuildClassNames(
            ("bb-calendar-day-number small", true),
            ("badge rounded-pill text-bg-primary", isToday),
            ("text-body-tertiary", isOutside && !isToday));
    }

    private static string GetDotClass(CalendarEntry<TItem> entry) =>
        entry.Color == CalendarColor.None ? "bg-secondary" : entry.Color.ToCalendarEventColorClass();

    private static string GetEventClass(CalendarEntry<TItem> entry, bool continuesBefore, bool continuesAfter) =>
        BuildClassNames(
            ("bb-calendar-event d-flex align-items-center gap-1 border-0 rounded-1 px-1", true),
            (entry.Color.ToCalendarEventColorClass(), entry.IsBar),
            ("bg-transparent text-body", !entry.IsBar),
            ("bb-calendar-event-continues-before", continuesBefore),
            ("bb-calendar-event-continues-after", continuesAfter));

    private string GetWeekdayName(DateOnly date) =>
        Owner.EffectiveCulture.DateTimeFormat.GetAbbreviatedDayName(date.DayOfWeek);

    /// <summary>
    /// Tells whether a day of a week row has any event, shown or hidden behind "+N more".
    /// </summary>
    private static bool HasEvents(CalendarLayout.MonthWeek week, int column) =>
        week.HiddenCounts[column] > 0
        || week.Segments.Any(segment => column >= segment.Column && column < segment.Column + segment.Span);

    private void ToggleMoreList(DateOnly date)
    {
        moreListDate = moreListDate == date ? null : date;

        // Focus moves into the list, so the keyboard reaches its events and Escape closes it.
        if (moreListDate is not null)
            pendingFocusSelector = ".bb-calendar-more-list .btn-close";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("bb-calendar-month", true),
            ("bb-calendar-month-week-numbers", Owner.ShowWeekNumbers));

    /// <summary>
    /// Gets the one day the Tab key stops at: the selected day, or the calendar's date, or the first day shown.
    /// The arrow keys move focus from it to the other days.
    /// </summary>
    private DateOnly FocusDay
    {
        get
        {
            var days = Owner.MonthWeeks.SelectMany(week => week.Days).ToList();

            if (Owner.SelectedDay is { } selected && days.Contains(selected))
                return selected;

            return days.Contains(Owner.CurrentDate) || days.Count == 0 ? Owner.CurrentDate : days[0];
        }
    }

    /// <summary>
    /// Gets the day whose events the compact layout lists below the grid: the selected day, or the calendar's date when none is selected.
    /// </summary>
    private DateOnly ListDay => Owner.SelectedDay ?? Owner.CurrentDate;

    /// <summary>
    /// Gets or sets the calendar this view belongs to. The calendar cascades itself, so the view re-renders with it.
    /// </summary>
    [CascadingParameter]
    private Calendar<TItem> Owner { get; set; } = default!;

    /// <summary>
    /// Gets the styles of the view. The number of day columns goes to CSS as a custom property, so hiding weekdays narrows the grid.
    /// </summary>
    protected override string? StyleNames =>
        BuildStyleNames(Style, ($"--bb-calendar-month-columns: {(Owner.MonthWeeks.Count > 0 ? Owner.MonthWeeks[0].Days.Length : 7)}", true));

    #endregion
}
