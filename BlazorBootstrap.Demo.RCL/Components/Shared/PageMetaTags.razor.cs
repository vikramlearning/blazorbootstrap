using System.Text.Json.Nodes;

namespace BlazorBootstrap.Demo.RCL;

public partial class PageMetaTags : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private string? breadcrumbJson;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnParametersSet()
    {
        breadcrumbJson = BuildBreadcrumbJson();

        base.OnParametersSet();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// BreadcrumbList JSON-LD for demo and docs pages: the section's start page, then this page.
    /// System.Text.Json's default encoder escapes &lt;, &gt; and &amp;, so no title can end the script element.
    /// </summary>
    private string? BuildBreadcrumbJson()
    {
        var path = SeoRoutes.Normalize(PageUrl);

        (string Name, string StartPage)? section = null;

        if (path.StartsWith(DemoRouteConstants.Demos_URL_Prefix + "/", StringComparison.OrdinalIgnoreCase))
            section = ("Demos", DemoRouteConstants.Demos_URL_GettingStarted);
        else if (path.StartsWith(DemoRouteConstants.Docs_URL_Prefix + "/", StringComparison.OrdinalIgnoreCase))
            section = ("Docs", DemoRouteConstants.Docs_URL_GettingStarted);

        if (section is null)
            return null;

        var items = new JsonArray { CreateListItem(1, section.Value.Name, SeoRoutes.DemosSiteUrl + section.Value.StartPage) };

        if (!string.Equals(path, section.Value.StartPage, StringComparison.OrdinalIgnoreCase))
            items.Add(CreateListItem(2, Title, SeoRoutes.GetCanonicalUrl(path))); // the docs-site page for a mapped /docs page

        var breadcrumb = new JsonObject
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "BreadcrumbList",
            ["itemListElement"] = items
        };

        return breadcrumb.ToJsonString();
    }

    private static JsonObject CreateListItem(int position, string name, string url) =>
        new()
        {
            ["@type"] = "ListItem",
            ["position"] = position,
            ["name"] = name,
            ["item"] = url
        };

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// The page's canonical URL: the matching docs-site page for a demos docs page that has one, otherwise this page.
    /// </summary>
    private string canonicalUrl => SeoRoutes.GetCanonicalUrl(PageUrl);

    /// <summary>
    /// Meta Description.
    /// </summary>
    [Parameter]
    public string Description { get; set; } = null!;

    /// <summary>
    /// Meta Image Url.
    /// </summary>
    [Parameter]
    public string ImageUrl { get; set; } = null!;

    /// <summary>
    /// Whether to set the page title. Showcase landing pages keep their own in-app title and pass false.
    /// </summary>
    [Parameter]
    public bool IncludePageTitle { get; set; } = true;

    /// <summary>
    /// Meta Url.
    /// Example: /demos/alerts, /demos/modals
    /// </summary>
    [Parameter]
    public string PageUrl { get; set; } = null!;

    private string siteName => "Blazor Bootstrap";

    private string title => $"{Title} | {siteName}";

    /// <summary>
    /// Page Title / Meta Title.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = null!;

    #endregion
}
