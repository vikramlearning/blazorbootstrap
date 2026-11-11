namespace BlazorBootstrap;

/// <summary>
/// Describes the day a user clicked in a <see cref="Calendar{TItem}" />.
/// </summary>
public sealed class CalendarDayClickEventArgs
{
    #region Constructors and Finalizers

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarDayClickEventArgs" /> class.
    /// </summary>
    /// <param name="date">The clicked day.</param>
    public CalendarDayClickEventArgs(DateOnly date)
    {
        Date = date;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the clicked day.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the clicked day.")]
    public DateOnly Date { get; }

    #endregion
}
