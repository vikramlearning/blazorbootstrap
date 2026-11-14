using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Layout of the "Tallymint" personal finance site: the site bar, a Navbar, the page body, one ad slot, and the footer.
/// </summary>
public partial class FintechLayout : LayoutComponentBase
{
    #region Fields and Constants

    /// <summary>Base route of the personal finance site.</summary>
    public const string Base = DemoRouteConstants.Showcase_URL_Fintech;

    private readonly ShowcaseSite site = ShowcaseCatalog.Get("fintech");

    #endregion

    #region Methods

    #region Private Methods

    private void OnThemeChanged(string themeName) => State.SetTheme(themeName);

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private FintechState State { get; set; } = default!;

    #endregion
}
