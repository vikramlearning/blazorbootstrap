namespace BlazorBootstrap;

/// <summary>
/// Describes the empty time slot a user clicked in a <see cref="Calendar{TItem}" />.
/// </summary>
public sealed class CalendarSlotClickEventArgs
{
    #region Constructors and Finalizers

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarSlotClickEventArgs" /> class.
    /// </summary>
    /// <param name="start">The start of the clicked slot.</param>
    /// <param name="end">The end of the clicked slot.</param>
    public CalendarSlotClickEventArgs(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the end of the clicked slot.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the end of the clicked slot.")]
    public DateTime End { get; }

    /// <summary>
    /// Gets the start of the clicked slot.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the start of the clicked slot.")]
    public DateTime Start { get; }

    #endregion
}
