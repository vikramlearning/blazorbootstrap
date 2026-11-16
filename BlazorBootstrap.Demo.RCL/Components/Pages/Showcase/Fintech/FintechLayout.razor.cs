using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Layout of the "Tallymint" bank: the site bar, a Navbar with the customer's modules and the account menu, the page body
/// inside the showcase session (per-tab data and the demo sign-in) behind the customer guard (password, then a one-time
/// code), one ad slot, the footer, and the toasts. It also posts scheduled transfers that have come due as pages load.
/// </summary>
public partial class FintechLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the bank.</summary>
    public const string Base = DemoRouteConstants.Showcase_URL_Fintech;

    /// <summary>Route of the demo sign-in page.</summary>
    public const string SignInPath = Base + "/sign-in";

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;
        Navigation.LocationChanged += OnLocationChanged;
    }

    protected override void OnParametersSet() => State.CatchUp();

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and navigation changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        Navigation.LocationChanged -= OnLocationChanged;
    }

    #endregion

    #region Private Methods

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e) => InvokeAsync(StateHasChanged);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets whether the current page is the sign-in page, the only one without the customer guard.</summary>
    private bool IsSignInPage => new Uri(Navigation.Uri).AbsolutePath.TrimEnd('/').EndsWith("/fintech/sign-in", StringComparison.OrdinalIgnoreCase);

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private FintechState State { get; set; } = default!;

    #endregion
}
