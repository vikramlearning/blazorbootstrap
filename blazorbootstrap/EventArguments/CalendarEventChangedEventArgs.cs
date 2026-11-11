namespace BlazorBootstrap;

/// <summary>
/// Describes the new times a user gave an event by moving or resizing it in a <see cref="Calendar{TItem}" />.
/// The calendar does not change the item; the app applies or ignores the new times.
/// </summary>
/// <typeparam name="TItem">The type of the events in the calendar.</typeparam>
public sealed class CalendarEventChangedEventArgs<TItem>
{
    #region Constructors and Finalizers

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarEventChangedEventArgs{TItem}" /> class.
    /// </summary>
    /// <param name="item">The item of the moved or resized event.</param>
    /// <param name="newStart">The start the user gave the event.</param>
    /// <param name="newEnd">The end the user gave the event.</param>
    public CalendarEventChangedEventArgs(TItem item, DateTime newStart, DateTime newEnd)
    {
        Item = item;
        NewStart = newStart;
        NewEnd = newEnd;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the item of the moved or resized event.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the item of the moved or resized event.")]
    public TItem Item { get; }

    /// <summary>
    /// Gets the end the user gave the event.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the end the user gave the event.")]
    public DateTime NewEnd { get; }

    /// <summary>
    /// Gets the start the user gave the event.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the start the user gave the event.")]
    public DateTime NewStart { get; }

    #endregion
}
