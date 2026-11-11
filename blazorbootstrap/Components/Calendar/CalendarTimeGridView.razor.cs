namespace BlazorBootstrap;

/// <summary>
/// The Week, Work week, and Day views of a <see cref="Calendar{TItem}" />: day columns with an all-day row above an hourly grid.
/// The calendar renders it; apps do not use it on its own.
/// </summary>
/// <typeparam name="TItem">The type of the events in the calendar.</typeparam>
[AddedVersion("4.1.0")]
public partial class CalendarTimeGridView<TItem> : BlazorBootstrapComponentBase
{
    #region Methods

    #region Private Methods

    /// <summary>
    /// Gets the classes of a day's cell in the all-day row. A click on it selects the day, as in the Month view.
    /// </summary>
    private string GetAllDayCellClass(DateOnly day)
    {
        var info = Owner.GetDayInfo(day);

        return BuildClassNames(
            ("bb-calendar-allday-cell", true),
            (info.HighlightClass, info.HasHighlight),
            ("bb-calendar-day-selected", Owner.SelectedDay == day));
    }

    private static string GetBlockClass(CalendarEntry<TItem> entry) =>
        BuildClassNames(
            ("bb-calendar-event bb-calendar-timegrid-event d-flex flex-column justify-content-start align-items-stretch border-0 rounded-1 px-1", true),
            (entry.Color.ToCalendarEventColorClass(), true));

    /// <summary>
    /// Places a block by its start and length in slots, and by its column among the blocks it overlaps.
    /// </summary>
    private string GetBlockStyle(CalendarLayout.TimeGridBlock block)
    {
        var slotLength = (double)Owner.SlotLength;
        var top = (block.StartMinutes - Owner.VisibleStartMinutes) / slotLength;
        var height = (block.EndMinutes - block.StartMinutes) / slotLength;
        var width = 100d / block.ColumnCount;

        return string.Create(
            CultureInfo.InvariantCulture,
            $"top: calc(var(--bb-calendar-slot-height) * {top:0.####}); height: calc(var(--bb-calendar-slot-height) * {height:0.####} - 1px); inset-inline-start: {block.Column * width:0.####}%; width: calc({width:0.####}% - 2px);");
    }

    private string GetColumnClass(DateOnly day)
    {
        var info = Owner.GetDayInfo(day);

        return BuildClassNames(
            ("bb-calendar-day-column", true),
            (info.HighlightClass, info.HasHighlight),
            ("bb-calendar-day-today", day == Owner.Today),
            ("bg-primary-subtle", day == Owner.Today && !info.HasHighlight));
    }

    private string GetDayHeaderClass(DateOnly day, CalendarLayout.DayInfo info) =>
        BuildClassNames(
            ("bb-calendar-timegrid-day-header text-center text-truncate", true),
            (info.HighlightClass, info.HasHighlight),
            ("bb-calendar-day-selected", Owner.SelectedDay == day));

    private string GetDayNumberClass(DateOnly day) =>
        BuildClassNames(
            ("bb-calendar-day-number small", true),
            ("badge rounded-pill text-bg-primary", day == Owner.Today));

    private static string GetSegmentClass(CalendarEntry<TItem> entry, CalendarLayout.RowSegment segment) =>
        BuildClassNames(
            ("bb-calendar-event d-flex align-items-center gap-1 border-0 rounded-1 px-1", true),
            (entry.Color.ToCalendarEventColorClass(), true),
            ("bb-calendar-event-continues-before", segment.ContinuesBefore),
            ("bb-calendar-event-continues-after", segment.ContinuesAfter));

    private string GetSlotClass(DateOnly day, int minutes) =>
        BuildClassNames(
            ("bb-calendar-slot", true),
            ("bb-calendar-slot-hour", minutes % 60 == 0),
            ("bb-calendar-slot-nonworking bg-body-tertiary", !Owner.IsWorkingSlot(day, minutes)));

    /// <summary>
    /// Gets the day name of a column header: the full name when one day is shown, the short name otherwise.
    /// </summary>
    private string GetWeekdayName(DateOnly day, int dayCount) =>
        dayCount == 1
            ? Owner.EffectiveCulture.DateTimeFormat.GetDayName(day.DayOfWeek)
            : Owner.EffectiveCulture.DateTimeFormat.GetAbbreviatedDayName(day.DayOfWeek);

    #endregion

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("bb-calendar-timegrid", true),
            ("bb-calendar-timegrid-scrollable", Owner.Height > 0));

    /// <summary>
    /// Gets the place of the current-time line: the time since the first visible slot, in slots.
    /// </summary>
    private string NowLineStyle =>
        Owner.NowMinutes >= Owner.VisibleStartMinutes && Owner.NowMinutes < Owner.VisibleEndMinutes
            ? string.Create(
                CultureInfo.InvariantCulture,
                $"top: calc(var(--bb-calendar-slot-height) * {(Owner.NowMinutes - Owner.VisibleStartMinutes) / Owner.SlotLength:0.####});")
            : "display: none;";

    /// <summary>
    /// Gets or sets the calendar this view belongs to. The calendar cascades itself, so the view re-renders with it.
    /// </summary>
    [CascadingParameter]
    private Calendar<TItem> Owner { get; set; } = default!;

    private string? ScrollStyle =>
        Owner.Height > 0
            ? string.Create(CultureInfo.InvariantCulture, $"max-height: {Owner.Height:0.##}px;")
            : null;

    /// <summary>
    /// Gets the minute of the day each time slot starts at, from the first visible slot to the last.
    /// </summary>
    private IEnumerable<int> SlotStarts
    {
        get
        {
            for (var minutes = Owner.VisibleStartMinutes; minutes < Owner.VisibleEndMinutes; minutes += Owner.SlotLength)
                yield return minutes;
        }
    }

    /// <summary>
    /// Gets the styles of the view. The number of day columns goes to CSS as a custom property: the header, the all-day row,
    /// and the hourly grid share one column template that reads it.
    /// </summary>
    protected override string? StyleNames =>
        BuildStyleNames(Style, ($"--bb-calendar-day-count: {Owner.TimeGridLayout?.Days.Length ?? 1}", true));

    #endregion
}
