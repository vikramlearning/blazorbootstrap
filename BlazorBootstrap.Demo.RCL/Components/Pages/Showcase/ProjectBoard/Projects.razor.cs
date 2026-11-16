using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// Project boards: a card per project (<see cref="PbState.Projects" />) with its key, name (a link to its board), client,
/// status badge in words, due date, progress (<see cref="PbState.ProjectProgress" /> as a <see cref="Progress" /> and
/// "12 of 20 tasks done"), the next milestone, and the team's initials (names in <c>title</c> and visually hidden text).
/// Toggle buttons (<c>aria-pressed</c>) filter by status; the count shown is announced.
/// </summary>
public partial class Projects : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The status filter, or <see langword="null" /> for every project.</summary>
    private PbProjectStatus? filter;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void SetFilter(PbProjectStatus? status) => filter = status;

    private string ShownText(int shown, int all) => filter is null
        ? $"Showing all {TaskCard.Plural(all, "project")}."
        : $"Showing {shown} {FilterWord} of {TaskCard.Plural(all, "project")}.";

    #endregion

    #endregion

    #region Properties, Indexers

    private static IReadOnlyList<(PbProjectStatus? Status, string Text)> FilterOptions { get; } = new (PbProjectStatus?, string)[]
    {
        (null, "All"),
        (PbProjectStatus.Active, PbDisplay.ProjectStatusLabel(PbProjectStatus.Active)),
        (PbProjectStatus.Planned, PbDisplay.ProjectStatusLabel(PbProjectStatus.Planned)),
        (PbProjectStatus.OnHold, PbDisplay.ProjectStatusLabel(PbProjectStatus.OnHold)),
        (PbProjectStatus.Completed, PbDisplay.ProjectStatusLabel(PbProjectStatus.Completed)),
    };

    private string FilterWord => filter is { } status ? PbDisplay.ProjectStatusLabel(status).ToLowerInvariant() : string.Empty;

    [Inject] private PbState State { get; set; } = default!;

    #endregion
}
