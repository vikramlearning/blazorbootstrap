namespace BlazorBootstrap.Demo.RCL;

public partial class PageMetaTags : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

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
    /// Meta Url.
    /// Example: /alerts, /modal
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

    private string url => $"https://demos.blazorbootstrap.com{PageUrl}";

    #endregion
}
