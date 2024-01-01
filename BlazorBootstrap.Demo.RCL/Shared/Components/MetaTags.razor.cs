namespace BlazorBootstrap.Demo.RCL;

public partial class MetaTags : ComponentBase
{
    #region Members

    private string siteName => "Blazor Bootstrap";

    private string title => $"{Title} | {siteName}";

    private string url => $"https://demos.blazorbootstrap.com{PageUrl}";

    #endregion

    #region Methods

    #endregion

    #region Properties

    /// <summary>
    /// Meta Url.
    /// Example: /alerts, /modal
    /// </summary>
    [Parameter] public string PageUrl { get; set; } = null!;

    /// <summary>
    /// Page Title / Meta Title.
    /// </summary>
    [Parameter] public string Title { get; set; } = null!;

    /// <summary>
    /// Meta Description.
    /// </summary>
    [Parameter] public string Description { get; set; } = null!;

    /// <summary>
    /// Meta Image Url.
    /// </summary>
    [Parameter] public string ImageUrl { get; set; } = null!;

    #endregion
}
