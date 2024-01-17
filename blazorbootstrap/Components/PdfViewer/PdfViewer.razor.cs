namespace BlazorBootstrap;

public partial class PdfViewer : BlazorBootstrapComponentBase
{
    private int pageNumber = 0;
    private int pagesCount = 0;
    private double defaultScale = 1.0;
    private double minScale = 0.1;
    private double maxScale = 10.0;
    private int zoomLevel = 1;
    private double currentRotationDegrees = 90;

    private DotNetObjectReference<PdfViewer>? objRef;
    [Inject] PdfViewerJsInterop PdfViewerJsInterop { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await PdfViewerJsInterop.InitializeAsync(objRef, ElementId, Url);
        await base.OnInitializedAsync();
    }

    [JSInvokable]
    public void Set(PdfViewerModel pdfViewerModel)
    {
        if (pdfViewerModel is null) return;

        pageNumber = pdfViewerModel.PageNumber;
        pagesCount = pdfViewerModel.PagesCount;

        Console.WriteLine($"pageNumber: {pdfViewerModel.PageNumber}, pagesCount: {pdfViewerModel.PagesCount}");
    }

    private async Task PreviousPageAsync() =>
        await PdfViewerJsInterop.PreviousPageAsync(objRef, ElementId);

    private async Task NextPageAsync() =>
        await PdfViewerJsInterop.NextPageAsync(objRef, ElementId);

    private async Task FirstPageAsync() =>
        await PdfViewerJsInterop.FirstPageAsync(objRef, ElementId);

    private async Task LastPageAsync() =>
        await PdfViewerJsInterop.LastPageAsync(objRef, ElementId);

    private void PageNumberChanged(int value)
    {
        // TODO: update
    }

    private async Task ZoomOutAsync() => 
        await PdfViewerJsInterop.ZoomInOutAsync(objRef, ElementId, 0.5);

    private async Task ZoomInAsync() =>
        await PdfViewerJsInterop.ZoomInOutAsync(objRef, ElementId, 1);

    private async Task RotateClockwiseAsync() =>
        await PdfViewerJsInterop.RotateAsync(objRef, ElementId, 90);

    private async Task RotateCounterclockwiseAsync() =>
        await PdfViewerJsInterop.RotateAsync(objRef, ElementId, -90);

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    [Parameter]
    public string? Url { get; set; }

    #endregion
}
