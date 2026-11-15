using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// The app shell of Lumora's signed-in areas (the customer workspace and the operator console): the site bar, a sidebar with
/// the area's modules, a top bar, the page, and the footer. Its stylesheet styles the pages of both areas.
/// </summary>
public partial class SaasShell : ComponentBase
{
    #region Fields and Constants

    private readonly ShowcaseSite site = ShowcaseCatalog.Get("saas");

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the badge beside the product name in the sidebar, such as the area's name.</summary>
    [Parameter] public string? BadgeText { get; set; }

    /// <summary>Gets or sets the page content.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Gets or sets the sidebar's modules.</summary>
    [Parameter, EditorRequired] public SidebarDataProviderDelegate DataProvider { get; set; } = default!;

    /// <summary>Gets or sets the footer note.</summary>
    [Parameter, EditorRequired] public string FooterText { get; set; } = default!;

    /// <summary>Gets or sets the area's start page, linked from the sidebar brand.</summary>
    [Parameter, EditorRequired] public string Home { get; set; } = default!;

    /// <summary>Gets or sets whether the sidebar shows; it is hidden until the right account is signed in.</summary>
    [Parameter] public bool ShowSidebar { get; set; }

    /// <summary>Gets or sets the actions at the end of the top bar.</summary>
    [Parameter] public RenderFragment? TopbarEnd { get; set; }

    /// <summary>Gets or sets the start of the top bar, such as the workspace name.</summary>
    [Parameter] public RenderFragment? TopbarStart { get; set; }

    [Inject] private SaasState State { get; set; } = default!;

    #endregion
}
