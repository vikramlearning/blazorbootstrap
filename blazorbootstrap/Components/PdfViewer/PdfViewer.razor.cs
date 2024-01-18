namespace BlazorBootstrap;

public partial class PdfViewer : BlazorBootstrapComponentBase
{
    private int pageNumber = 0;
    private int pagesCount = 0;

    private int minZoomLevel = 1;
    private int maxZoomLevel = 17;
    private int zoomLevel = 8;
    private string zoomPercentage = "100%";

    private double scale = 1.0;
    private double minScale = 0.25;
    private double maxScale = 5;

    private double rotation = 0;

    private DotNetObjectReference<PdfViewer>? objRef;

    [Inject] PdfViewerJsInterop PdfViewerJsInterop { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await PdfViewerJsInterop.InitializeAsync(objRef, ElementId, scale, rotation, Url);
        await base.OnInitializedAsync();
    }

    [JSInvokable]
    public void Set(PdfViewerModel pdfViewerModel)
    {
        if (pdfViewerModel is null) return;

        pageNumber = pdfViewerModel.PageNumber;
        pagesCount = pdfViewerModel.PagesCount;
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
        if (value < 1 || value > pagesCount)
            pageNumber = 1;

        // TODO: call generic page render method
    }

    private async Task ZoomOutAsync()
    {
        if (zoomLevel == minZoomLevel) 
            return;

        zoomLevel -= 1;
        var zp = GetZoomPercentage(zoomLevel);
        zoomPercentage = $"{zp}%";
        scale = 0.01 * zp;
        await PdfViewerJsInterop.ZoomInOutAsync(objRef, ElementId, scale);
    }

    private async Task ZoomInAsync()
    {
        if (zoomLevel == maxZoomLevel) 
            return;

        zoomLevel += 1;
        var zp = GetZoomPercentage(zoomLevel);
        zoomPercentage = $"{zp}%";
        scale = 0.01 * zp;
        await PdfViewerJsInterop.ZoomInOutAsync(objRef, ElementId, scale);
    }

    private async Task RotateClockwiseAsync()
    {
        rotation += 90;
        rotation = (rotation == 360) ? 0 : rotation;
        await PdfViewerJsInterop.RotateAsync(objRef, ElementId, rotation);
    }

    private async Task RotateCounterclockwiseAsync()
    {
        rotation -= 90;
        rotation = (rotation == -360) ? 0 : rotation;
        await PdfViewerJsInterop.RotateAsync(objRef, ElementId, rotation);
    }

    public int GetZoomPercentage(int zoomLevel) =>
        zoomLevel switch
        {
            1 => 25,
            2 => 33,
            3 => 50,
            4 => 67,
            5 => 75,
            6 => 80,
            7 => 90,
            8 => 100,
            9 => 110,
            10 => 125,
            11 => 150,
            12 => 175,
            13 => 200,
            14 => 250,
            15 => 300,
            16 => 400,
            17 => 500,
            _ => 100
        };

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    [Parameter]
    public string? Url { get; set; }

    #endregion
}
