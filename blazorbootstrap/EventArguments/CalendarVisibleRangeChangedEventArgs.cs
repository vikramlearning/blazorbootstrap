namespace BlazorBootstrap;

/// <summary>
/// Describes the dates a <see cref="Calendar{TItem}" /> shows after its first render, a navigation, or a view switch.
/// </summary>
public sealed class CalendarVisibleRangeChangedEventArgs
{
    #region Constructors and Finalizers

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarVisibleRangeChangedEventArgs" /> class.
    /// </summary>
    /// <param name="start">The first visible day.</param>
    /// <param name="end">The last visible day.</param>
    /// <param name="view">The view that shows the range.</param>
    public CalendarVisibleRangeChangedEventArgs(DateOnly start, DateOnly end, CalendarView view)
    {
        Start = start;
        End = end;
        View = view;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the last visible day.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the last visible day.")]
    public DateOnly End { get; }

    /// <summary>
    /// Gets the first visible day.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the first visible day.")]
    public DateOnly Start { get; }

    /// <summary>
    /// Gets the view that shows the range.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(CalendarView.Month)]
    [Description("Gets the view that shows the range.")]
    public CalendarView View { get; }

    #endregion
}
