namespace BlazorBootstrap;

public partial class PdfViewer : BlazorBootstrapComponentBase
{
    [Inject] PdfViewerJsInterop PdfViewerJsInterop { get; set; } = default!;

    private async Task OnClick(EventArgs args)
    {
        await PdfViewerJsInterop.ShowPdf();
    }
}
