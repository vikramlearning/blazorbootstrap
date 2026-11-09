using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Marketing;

/// <summary>
/// Layout of the "Lumora" SaaS marketing site.
/// </summary>
public partial class MarketingLayout : LayoutComponentBase
{
    #region Fields and Constants

    /// <summary>Base route of the marketing site.</summary>
    public const string Base = DemoRouteConstants.Showcase_URL_Marketing;

    private readonly (string Path, string Text)[] links =
    {
        ("features", "Features"),
        ("pricing", "Pricing"),
        ("customers", "Customers"),
        ("faq", "FAQ"),
    };

    private readonly ShowcaseSite site = ShowcaseCatalog.Get("marketing");

    private bool menuOpen;

    #endregion

    #region Methods

    #region Private Methods

    private void CloseMenu() => menuOpen = false;

    private void ToggleMenu() => menuOpen = !menuOpen;

    #endregion

    #endregion
}
