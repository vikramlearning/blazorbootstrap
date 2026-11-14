using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Layout of the "Stackwatch" monitoring site: the site bar (whose theme changes feed the chart colors), a Navbar, the
/// page body, one ad slot, and the footer.
/// </summary>
public partial class MonitoringLayout : LayoutComponentBase
{
    #region Fields and Constants

    /// <summary>Base route of the monitoring site.</summary>
    public const string Base = DemoRouteConstants.Showcase_URL_Monitoring;

    private readonly ShowcaseSite site = ShowcaseCatalog.Get("monitoring");

    #endregion

    #region Methods

    #region Private Methods

    private void OnThemeChanged(string themeName) => State.SetTheme(themeName);

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion
}
