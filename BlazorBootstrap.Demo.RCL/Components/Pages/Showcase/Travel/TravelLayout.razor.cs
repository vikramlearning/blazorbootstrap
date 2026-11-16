using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// Layout of the "Driftwood Stays" travel site: the site bar, a Navbar with the guest's modules and the account menu, the
/// page body inside the showcase session (per-tab data and the demo sign-in), one ad slot, the footer, and the toasts.
/// </summary>
public partial class TravelLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the travel site.</summary>
    public const string Base = DemoRouteConstants.Showcase_URL_Travel;

    /// <summary>Route of the demo sign-in page.</summary>
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

    [Inject] private TravelState State { get; set; } = default!;

    #endregion
}
