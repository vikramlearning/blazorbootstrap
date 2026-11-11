namespace BlazorBootstrap;

/// <summary>
/// Describes the event a user clicked in a <see cref="Calendar{TItem}" />.
/// </summary>
/// <typeparam name="TItem">The type of the events in the calendar.</typeparam>
public sealed class CalendarEventClickEventArgs<TItem>
{
    #region Constructors and Finalizers

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarEventClickEventArgs{TItem}" /> class.
    /// </summary>
    /// <param name="item">The item of the clicked event.</param>
    public CalendarEventClickEventArgs(TItem item)
    {
        Item = item;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the item of the clicked event.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the item of the clicked event.")]
    public TItem Item { get; }

    #endregion
}
