namespace BlazorBootstrap;

/// <summary>
/// The context passed to the <see cref="Calendar{TItem}.EventTemplate" /> of a <see cref="Calendar{TItem}" /> event.
/// </summary>
/// <typeparam name="TItem">The type of the events in the calendar.</typeparam>
public sealed class CalendarEventContext<TItem>
{
    #region Constructors and Finalizers

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarEventContext{TItem}" /> class.
    /// </summary>
    /// <param name="item">The item the event represents.</param>
    /// <param name="title">The title of the event.</param>
    /// <param name="start">The start of the event.</param>
    /// <param name="end">The end of the event.</param>
    /// <param name="isAllDay">Whether the event is an all-day event.</param>
    /// <param name="view">The view the event is drawn in.</param>
    public CalendarEventContext(TItem item, string? title, DateTime start, DateTime end, bool isAllDay, CalendarView view)
    {
        Item = item;
        Title = title;
        Start = start;
        End = end;
        IsAllDay = isAllDay;
        View = view;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the end of the event.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the end of the event.")]
    public DateTime End { get; }

    /// <summary>
    /// Gets whether the event is an all-day event.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets whether the event is an all-day event.")]
    public bool IsAllDay { get; }

    /// <summary>
    /// Gets the item the event represents.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the item the event represents.")]
    public TItem Item { get; }

    /// <summary>
    /// Gets the start of the event.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the start of the event.")]
    public DateTime Start { get; }

    /// <summary>
    /// Gets the title of the event.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the title of the event.")]
    public string? Title { get; }

    /// <summary>
    /// Gets the view the event is drawn in.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(CalendarView.Month)]
    [Description("Gets the view the event is drawn in.")]
    public CalendarView View { get; }

    #endregion
}
