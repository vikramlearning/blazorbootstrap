namespace BlazorBootstrap;

public partial class PdfViewer : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private int defaultZoomLevel = 8;

    private int maxZoomLevel = 17;

    private int minZoomLevel = 1;

    private DotNetObjectReference<PdfViewer>? objRef;

    private Orientation? oldOrientation;

    private int pageNumber = 0;

    private int pagesCount = 0;

    private double rotation = 0;

    private double scale = 1.0;
    private int zoomLevel = 8;

    private string zoomPercentage = "100%";

    #endregion

    #region Methods

    protected override async Task OnInitializedAsync()
    {
        rotation = Orientation == Orientation.Portrait ? 0 : -90;
        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();

        QueueAfterRenderAction(async () => await PdfViewerJsInterop.InitializeAsync(objRef!, ElementId!, scale, rotation, Url!), new RenderPriority());
    }

    protected override async Task OnParametersSetAsync()
    {
        if (Rendered)
            if (!Orientation.Equals(oldOrientation))
            {
                oldOrientation = Orientation;
                rotation = Orientation == Orientation.Portrait ? 0 : -90;
                await PdfViewerJsInterop.RotateAsync(objRef!, ElementId!, rotation);
            }

        await base.OnParametersSetAsync();
    }

    [JSInvokable]
    public void DocumentLoaded(PdfViewerModel pdfViewerModel)
    {
        if (pdfViewerModel is null) return;

        pageNumber = pdfViewerModel.PageNumber;
        pagesCount = pdfViewerModel.PagesCount;

        StateHasChanged();

        if (OnDocumentLoaded.HasDelegate)
            OnDocumentLoaded.InvokeAsync(new PdfViewerEventArgs(pageNumber, pagesCount));
    }

    [JSInvokable]
    public void SetPdfViewerMetaData(PdfViewerModel pdfViewerModel)
    {
        if (pdfViewerModel is null) return;

        pageNumber = pdfViewerModel.PageNumber;
        pagesCount = pdfViewerModel.PagesCount;

        if (OnPageChanged.HasDelegate)
            OnPageChanged.InvokeAsync(new PdfViewerEventArgs(pageNumber, pagesCount));
    }

    private async Task FirstPageAsync() => await PdfViewerJsInterop.FirstPageAsync(objRef!, ElementId!);

    private int GetZoomPercentage(int zoomLevel) =>
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

    private async Task LastPageAsync() => await PdfViewerJsInterop.LastPageAsync(objRef!, ElementId!);

    private async Task NextPageAsync() => await PdfViewerJsInterop.NextPageAsync(objRef!, ElementId!);

    private async Task PageNumberChangedAsync(int value)
    {
        if (value < 1 || value > pagesCount)
            pageNumber = 1;
        else
            pageNumber = value;

        await PdfViewerJsInterop.GotoPageAsync(objRef!, ElementId!, pageNumber);
    }

    private async Task PreviousPageAsync() => await PdfViewerJsInterop.PreviousPageAsync(objRef!, ElementId!);

    private async Task PrintAsync() => await PdfViewerJsInterop.PrintAsync(objRef!, ElementId!, Url!);

    private async Task ResetZoomAsync()
    {
        zoomLevel = defaultZoomLevel;
        var zp = GetZoomPercentage(defaultZoomLevel);
        zoomPercentage = $"{zp}%";
        scale = 0.01 * zp;
        await PdfViewerJsInterop.ZoomInOutAsync(objRef!, ElementId!, scale);
    }

    private async Task RotateClockwiseAsync()
    {
        rotation += 90;
        rotation = rotation.Equals(360) ? 0 : rotation;
        await PdfViewerJsInterop.RotateAsync(objRef!, ElementId!, rotation);

        // Orientation
        SetOrientation();
    }

    private async Task RotateCounterclockwiseAsync()
    {
        rotation -= 90;
        rotation = rotation.Equals(-360) ? 0 : rotation;
        await PdfViewerJsInterop.RotateAsync(objRef!, ElementId!, rotation);

        // Orientation
        SetOrientation();
    }

    private void SetOrientation()
    {
        if (rotation == 0)
            oldOrientation = Orientation = Orientation.Portrait;
        else if (rotation == -90)
            oldOrientation = Orientation = Orientation.Landscape;
    }

    private async Task SwitchOrientationAsync()
    {
        oldOrientation = Orientation;
        Orientation = Orientation == Orientation.Portrait ? Orientation.Landscape : Orientation.Portrait;
        rotation = Orientation == Orientation.Portrait ? 0 : -90;
        await PdfViewerJsInterop.RotateAsync(objRef!, ElementId!, rotation);
    }

    private async Task ZoomInAsync()
    {
        if (zoomLevel == maxZoomLevel)
            return;

        zoomLevel += 1;
        var zp = GetZoomPercentage(zoomLevel);
        zoomPercentage = $"{zp}%";
        scale = 0.01 * zp;
        await PdfViewerJsInterop.ZoomInOutAsync(objRef!, ElementId!, scale);
    }

    private async Task ZoomOutAsync()
    {
        if (zoomLevel == minZoomLevel)
            return;

        zoomLevel -= 1;
        var zp = GetZoomPercentage(zoomLevel);
        zoomPercentage = $"{zp}%";
        scale = 0.01 * zp;
        await PdfViewerJsInterop.ZoomInOutAsync(objRef!, ElementId!, scale);
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// This event fires immediately after the PDF document is loaded.
    /// </summary>
    [Parameter]
    public EventCallback<PdfViewerEventArgs> OnDocumentLoaded { get; set; }

    /// <summary>
    /// This event fires immediately after the page is changed.
    /// </summary>
    [Parameter]
    public EventCallback<PdfViewerEventArgs> OnPageChanged { get; set; }

    /// <summary>
    /// Gets or sets the preferred orientation for the PDF viewer.
    /// </summary>
    [Parameter]
    public Orientation Orientation { get; set; }

    /// <summary>
    /// Provides JavaScript interop functionality for the PDF viewer.
    /// </summary>
    [Inject]
    private PdfViewerJsInterop PdfViewerJsInterop { get; set; } = default!;

    /// <summary>
    /// Gets or sets the URL of the PDF document to be displayed.
    /// </summary>
    [Parameter]
    public string? Url { get; set; }

    #endregion
}
