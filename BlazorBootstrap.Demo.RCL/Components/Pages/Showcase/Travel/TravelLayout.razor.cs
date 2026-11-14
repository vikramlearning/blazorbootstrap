using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// Layout of the "Driftwood Stays" travel site: the site bar, a Navbar, the page body, one ad slot, and the footer.
/// </summary>
public partial class TravelLayout : LayoutComponentBase
{
    #region Fields and Constants

    /// <summary>Base route of the travel site.</summary>
    public const string Base = DemoRouteConstants.Showcase_URL_Travel;

    private readonly ShowcaseSite site = ShowcaseCatalog.Get("travel");

    #endregion
}
