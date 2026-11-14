using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// Layout of the Kindred Goods back office for the retailer (D-031): a sidebar with the admin modules, a top bar with
/// "View store" and the account menu, and the page body behind a retailer-only guard. The sidebar appears only to a
/// signed-in retailer, so a customer or a signed-out visitor sees the guard's screens without the admin chrome.
/// </summary>
public partial class EcommerceAdminLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the back office.</summary>
    public const string Base = EcommerceLayout.Base + "/admin";

    private readonly ShowcaseSite site = ShowcaseCatalog.Get("ecommerce");

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

    /// <summary>The admin modules. Each appears here once its screen exists, so the sidebar never links to a missing page.</summary>
    private Task<SidebarDataProviderResult> SidebarDataProvider(SidebarDataProviderRequest request)
    {
        var items = new List<NavItem>
        {
            new() { Id = "dashboard", Href = Base, IconName = IconName.Speedometer2, Text = "Dashboard", Match = NavLinkMatch.All },
            new() { Id = "account", Href = $"{Base}/account", IconName = IconName.PersonGear, Text = "Account" },
        };

        return Task.FromResult(request.ApplyTo(items));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private EcommerceState State { get; set; } = default!;

    #endregion
}
