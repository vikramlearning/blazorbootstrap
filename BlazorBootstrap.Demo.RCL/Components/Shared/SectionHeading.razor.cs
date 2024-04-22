namespace BlazorBootstrap.Demo.RCL;
public partial class SectionHeading : ComponentBase
{
    #region Members

    private string link => $"{PageUrl}#{HashTagName}".Trim().ToLower();

    #endregion

    #region Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await Task.Delay(200);
        await JS.InvokeVoidAsync("navigateToHeading");
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task OnClick()
    {
        await JS.InvokeVoidAsync("navigateToHeading");
    }

    #endregion

    #region Properties

    [Parameter] public HeadingSize Size { get; set; }

    [Parameter] public string Text { get; set; } = null!;

    [Parameter] public string PageUrl { get; set; } = null!;

    [Parameter] public string HashTagName { get; set; } = null!;

    [Inject] protected IJSRuntime JS { get; set; } = null!;

    #endregion
}
