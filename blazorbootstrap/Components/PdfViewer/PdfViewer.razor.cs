namespace BlazorBootstrap;

public partial class PdfViewer : BlazorBootstrapComponentBase
{
    [Inject] PdfViewerJsInterop PdfViewerJsInterop { get; set; } = default!;

    protected void OnClick(EventArgs args)
    {
        PdfViewerJsInterop.Prompt("Hi, ");
    }
}
