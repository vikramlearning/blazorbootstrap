using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// Layout of the "Beacon Desk" help center: the header with the knowledge base, requests, and the account menu, the page,
/// and the footer. It hosts the tab's demo session for the help center.
/// </summary>
public partial class HelpDeskLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the help desk site.</summary>
    public const string Base = DemoRouteConstants.Showcase_URL_HelpDesk;

    /// <summary>The sign-in page.</summary>
    public const string SignInPath = Base + "/sign-in";

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

    [Inject] private HelpDeskState State { get; set; } = default!;

    #endregion
}
