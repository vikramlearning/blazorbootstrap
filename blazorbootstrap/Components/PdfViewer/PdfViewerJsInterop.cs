namespace BlazorBootstrap;

public class PdfViewerJsInterop : JsInteropBase
{
    #region Constructors

    public PdfViewerJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, "./_content/Blazor.Bootstrap/blazor.bootstrap.pdf.js")
    {
    }

    #endregion

    #region Methods

    public async Task FirstPageAsync(object objRef, string elementId)
    {
        await SafeInvokeVoidAsync("firstPage", objRef, elementId);
    }

    public async Task GotoPageAsync(object objRef, string elementId, int gotoPageNum)
    {
        await SafeInvokeVoidAsync("gotoPage", objRef, elementId, gotoPageNum);
    }

    public async Task InitializeAsync(object objRef, string elementId, double scale, double rotation, string url, string password)
    {
        await SafeInvokeVoidAsync("initialize", objRef, elementId, scale, rotation, url, password);
    }

    public async Task LastPageAsync(object objRef, string elementId)
    {
        await SafeInvokeVoidAsync("lastPage", objRef, elementId);
    }

    public async Task NextPageAsync(object objRef, string elementId)
    {
        await SafeInvokeVoidAsync("nextPage", objRef, elementId);
    }

    public async Task PreviousPageAsync(object objRef, string elementId)
    {
        await SafeInvokeVoidAsync("previousPage", objRef, elementId);
    }

    public async Task PrintAsync(object objRef, string elementId, string url)
    {
        await SafeInvokeVoidAsync("print", objRef, elementId, url);
    }

    public async Task RotateAsync(object objRef, string elementId, double rotation)
    {
        await SafeInvokeVoidAsync("rotate", objRef, elementId, rotation);
    }

    public async Task ZoomInOutAsync(object objRef, string elementId, double scale)
    {
        await SafeInvokeVoidAsync("zoomInOut", objRef, elementId, scale);
    }

    #endregion
}
