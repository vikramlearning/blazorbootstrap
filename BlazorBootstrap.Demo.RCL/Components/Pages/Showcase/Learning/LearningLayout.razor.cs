using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// Layout of the "Quillpath Academy" learning site: the site bar, a Navbar, the page body, one ad slot, and the footer.
/// </summary>
public partial class LearningLayout : LayoutComponentBase
{
    #region Fields and Constants

    /// <summary>Base route of the learning site.</summary>
    public const string Base = DemoRouteConstants.Showcase_URL_Learning;

    private readonly ShowcaseSite site = ShowcaseCatalog.Get("learning");

    #endregion
}
