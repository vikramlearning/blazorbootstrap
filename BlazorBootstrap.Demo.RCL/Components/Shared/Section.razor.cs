namespace BlazorBootstrap.Demo.RCL;

public partial class Section : BlazorBootstrapComponentBase
{
    #region Members

    private string? headerClassNames => "mt-4 mb-3";

    private string link => $"{PageUrl}#{Link}".Trim().ToLower();

    #endregion

    #region Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && IsRenderComplete)
        {
            await Task.Delay(200);
            await JsRuntime.InvokeVoidAsync("navigateToHeading");
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task OnClick()
    {
        await JsRuntime.InvokeVoidAsync("navigateToHeading");
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the child content.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null"/>.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool IsMobile { get; set; } = true;

    [Parameter] public string? Link { get; set; }

    [Parameter] public string? Name { get; set; }

    [Parameter] public string? PageUrl { get; set; }

    [Parameter] public HeadingSize Size { get; set; }

    #endregion
}
