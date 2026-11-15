using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Layout of Lumora's public site (visitors): the header with the product pages and sign-in or the account menu, the page,
/// and the footer. It hosts the tab's demo session for the whole site.
/// </summary>
public partial class SaasLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the site.</summary>
    public const string Base = DemoRouteConstants.Showcase_URL_Saas;

    /// <summary>The customer's notifications page.</summary>
    public const string NoticesPath = Base + "/app/notifications";

    /// <summary>The sign-in page.</summary>
    public const string SignInPath = Base + "/sign-in";

    private readonly (string Path, string Text)[] links =
    {
        ("features", "Features"),
        ("pricing", "Pricing"),
        ("customers", "Customers"),
        ("faq", "FAQ"),
        ("contact", "Contact sales"),
    };

    private readonly ShowcaseSite site = ShowcaseCatalog.Get("saas");

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

    [Inject] private SaasState State { get; set; } = default!;

    #endregion
}
