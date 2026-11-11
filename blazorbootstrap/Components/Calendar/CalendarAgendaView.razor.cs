namespace BlazorBootstrap;

/// <summary>
/// The Agenda view of a <see cref="Calendar{TItem}" />: the events of a period as a list grouped by day.
/// The calendar renders it; apps do not use it on its own.
/// </summary>
/// <typeparam name="TItem">The type of the events in the calendar.</typeparam>
[AddedVersion("4.1.0")]
public partial class CalendarAgendaView<TItem> : BlazorBootstrapComponentBase
{
    #region Methods

    #region Private Methods

    private string GetDateClass(DateOnly date, CalendarLayout.DayInfo info) =>
        BuildClassNames(
            ("bb-calendar-agenda-date small fw-semibold mb-0 px-2 py-1", true),
            (info.HighlightClass, info.HasHighlight),
            ("bg-body-tertiary", !info.HasHighlight),
            ("text-primary", date == Owner.Today));

    #endregion

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames => BuildClassNames(Class, ("bb-calendar-agenda", true));

    /// <summary>
    /// Gets or sets the calendar this view belongs to. The calendar cascades itself, so the view re-renders with it.
    /// </summary>
    [CascadingParameter]
    private Calendar<TItem> Owner { get; set; } = default!;

    #endregion
}
