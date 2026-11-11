namespace BlazorBootstrap;

/// <summary>
/// Describes the range a user selected by dragging in a <see cref="Calendar{TItem}" />.
/// </summary>
public sealed class CalendarRangeSelectedEventArgs
{
    #region Constructors and Finalizers

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarRangeSelectedEventArgs" /> class.
    /// </summary>
    /// <param name="start">The start of the selected range.</param>
    /// <param name="end">The end of the selected range.</param>
    /// <param name="isAllDay">Whether whole days were selected rather than time slots.</param>
    public CalendarRangeSelectedEventArgs(DateTime start, DateTime end, bool isAllDay)
    {
        Start = start;
        End = end;
        IsAllDay = isAllDay;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the end of the selected range. For whole days it is the start of the day after the last selected day.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the end of the selected range. For whole days it is the start of the day after the last selected day.")]
    public DateTime End { get; }

    /// <summary>
    /// Gets whether whole days were selected rather than time slots.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets whether whole days were selected rather than time slots.")]
    public bool IsAllDay { get; }

    /// <summary>
    /// Gets the start of the selected range.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the start of the selected range.")]
    public DateTime Start { get; }

    #endregion
}
