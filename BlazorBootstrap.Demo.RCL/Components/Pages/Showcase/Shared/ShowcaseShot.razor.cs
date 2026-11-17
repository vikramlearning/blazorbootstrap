using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// A site's home-screen screenshot in the current color mode, or a sized placeholder until both screenshots are
/// committed. Decorative: place it next to the site's visible name.
/// </summary>
public partial class ShowcaseShot : ComponentBase
{
    #region Methods

    #region Private Methods

    /// <summary>
    /// A root-relative CSS <c>url()</c> that keeps the app's base path. A relative <c>url()</c> inside a custom
    /// property would resolve against the stylesheet that reads it, and an absolute one would carry the server's
    /// own host name during prerendering behind a proxy.
    /// </summary>
    private string CssUrl(string path) => $"url('{new Uri(Navigation.BaseUri).AbsolutePath}{path}')";

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets extra CSS classes.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the site whose screenshot is shown.
    /// </summary>
    [Parameter, EditorRequired] public ShowcaseSite Site { get; set; } = default!;

    private bool HasScreenshots =>
        ShowcaseCatalog.IsAssetAvailable(Site.ScreenshotLight) && ShowcaseCatalog.IsAssetAvailable(Site.ScreenshotDark);

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    #endregion
}
