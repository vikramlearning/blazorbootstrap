namespace BlazorBootstrap.Demo.RCL.Models.Showcase;

/// <summary>
/// One sample website listed in the Showcase gallery.
/// </summary>
/// <param name="Slug">Stable kebab-case route segment under <c>/showcase</c>.</param>
/// <param name="Name">Display name.</param>
/// <param name="Tagline">One-line description shown on the gallery card.</param>
/// <param name="Category">Gallery grouping, such as "SaaS" or "Industry".</param>
/// <param name="IconName">Bootstrap icon shown when no screenshot is available.</param>
/// <param name="Accent">CSS color used for the card accent and placeholder.</param>
/// <param name="SourceFolder">Folder name under <c>Components/Pages/Showcase</c>.</param>
/// <param name="Components">Blazor Bootstrap components the site demonstrates.</param>
public sealed record ShowcaseSite(
    string Slug,
    string Name,
    string Tagline,
    string Category,
    IconName IconName,
    string Accent,
    string SourceFolder,
    IReadOnlyList<string> Components)
{
    #region Properties, Indexers

    /// <summary>
    /// Gets the docs page URL for this site.
    /// </summary>
    public string DocsUrl => $"{DemoRouteConstants.Showcase_Docs_URL_Prefix}/{Slug}";

    /// <summary>
    /// Gets the dark-mode gallery screenshot path.
    /// </summary>
    public string ScreenshotDark => $"_content/BlazorBootstrap.Demo.RCL/showcase/gallery/{Slug}-dark.webp";

    /// <summary>
    /// Gets the light-mode gallery screenshot path.
    /// </summary>
    public string ScreenshotLight => $"_content/BlazorBootstrap.Demo.RCL/showcase/gallery/{Slug}-light.webp";

    /// <summary>
    /// Gets the GitHub URL of this site's source folder.
    /// </summary>
    public string SourceUrl => $"{DemoRouteConstants.Showcase_Source_URL_Prefix}/{SourceFolder}";

    /// <summary>
    /// Gets the site home route.
    /// </summary>
    public string Url => $"{DemoRouteConstants.Showcase_URL_Prefix}/{Slug}";

    #endregion
}
