using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBootstrap.Demo;
public partial class SectionHeading : ComponentBase
{
    #region Members

    private string link => $"{PageUrl}#{HashTagName}".Trim().ToLower();

    #endregion

    #region Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
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

    [Parameter] public string Text { get; set; }

    [Parameter] public string PageUrl { get; set; }

    [Parameter] public string HashTagName { get; set; }

    [Inject] protected IJSRuntime JS { get; set; }

    #endregion
}
