using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// Layout of the "Beacon Desk" help desk: the site bar, a Navbar, the page body, one ad slot, the footer, and the toast
/// host for actions.
/// </summary>
public partial class HelpDeskLayout : LayoutComponentBase
{
    #region Fields and Constants

    /// <summary>Base route of the help desk site.</summary>
    public const string Base = DemoRouteConstants.Showcase_URL_HelpDesk;

    private readonly ShowcaseSite site = ShowcaseCatalog.Get("help-desk");

    #endregion
}
