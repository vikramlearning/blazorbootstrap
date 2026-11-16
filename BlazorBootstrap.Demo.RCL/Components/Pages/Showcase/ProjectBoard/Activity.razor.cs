using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// The activity timeline of every client (account managers): calls, emails, meetings, and notes, and the deal and project
/// changes Tandem records, newest first and grouped by day ("Today", "Yesterday", then dates), each entry an
/// <see cref="ActivityEntry" /> naming and linking its client. Filters by client and by kind; a status line says how many
/// entries are shown. Logging and deleting happen on the client's page.
/// </summary>
public partial class Activity : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The client to show, or 0 for every client.</summary>
    private int clientFilter;

    /// <summary>The kind to show, or <see langword="null" /> for every kind.</summary>
    private PbActivityKind? kindFilter;

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

    #endregion

    #endregion

    #region Properties, Indexers

    private IReadOnlyList<PbActivity> Entries =>
        State.Activities.Where(entry => (clientFilter == 0 || entry.ClientId == clientFilter) && (kindFilter is not { } kind || entry.Kind == kind)).ToList();

    [Inject] private PbState State { get; set; } = default!;

    #endregion
}
