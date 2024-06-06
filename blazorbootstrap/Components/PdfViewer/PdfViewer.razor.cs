namespace BlazorBootstrap;

/// <summary>
/// The Blazor PDF Viewer component allows users to view PDF files directly in the browser, without relying on third-party browser tools or extensions. <br/>
/// The <see cref="Url"/> element can contain a URL or a base64 string of type "application/pdf".
/// </summary>
public partial class PdfViewer : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private const int DefaultZoomLevel = 8;

    private const int MaxZoomLevel = 17;

    private const int MinZoomLevel = 1;

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

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await PdfViewerJsInterop.InitializeAsync(objRef!, Id!, scale, rotation, Url!);

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        rotation = Orientation == Orientation.Portrait ? 0 : -90;

        await base.OnInitializedAsync();
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (IsRenderComplete)
            if (!Orientation.Equals(oldOrientation))
            {
                oldOrientation = Orientation;
                rotation = Orientation == Orientation.Portrait ? 0 : -90;
                await PdfViewerJsInterop.RotateAsync(objRef!, Id!, rotation);
            }

        await base.OnParametersSetAsync();
    }

    [JSInvokable]
    public void DocumentLoaded(PdfViewerModel? pdfViewerModel)
    {
        if (pdfViewerModel is null) return;

        pageNumber = pdfViewerModel.PageNumber;
        pagesCount = pdfViewerModel.PagesCount;

        StateHasChanged();

        if (OnDocumentLoaded.HasDelegate)
            OnDocumentLoaded.InvokeAsync(new PdfViewerEventArgs(pageNumber, pagesCount));
    }

    [JSInvokable]
    public void SetPdfViewerMetaData(PdfViewerModel? pdfViewerModel)
    {
        if (pdfViewerModel is null) return;

        pageNumber = pdfViewerModel.PageNumber;
        pagesCount = pdfViewerModel.PagesCount;

        if (OnPageChanged.HasDelegate)
            OnPageChanged.InvokeAsync(new PdfViewerEventArgs(pageNumber, pagesCount));
    }

    private Task FirstPageAsync() => PdfViewerJsInterop.FirstPageAsync(objRef!, Id!);

    private static int GetZoomPercentage(int zoomLevel) =>
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

    private Task LastPageAsync() => PdfViewerJsInterop.LastPageAsync(objRef!, Id!);

    private Task NextPageAsync() => PdfViewerJsInterop.NextPageAsync(objRef!, Id!);

    private Task PageNumberChangedAsync(int value)
    {
        if (value < 1 || value > pagesCount)
            pageNumber = 1;
        else
            pageNumber = value;

        return PdfViewerJsInterop.GotoPageAsync(objRef!, Id!, pageNumber);
    }

    private Task PreviousPageAsync() => PdfViewerJsInterop.PreviousPageAsync(objRef!, Id!);

    private Task PrintAsync() => PdfViewerJsInterop.PrintAsync(objRef!, Id!, Url!);

    private async Task ResetZoomAsync()
    {
        zoomLevel = DefaultZoomLevel;
        var zp = GetZoomPercentage(DefaultZoomLevel);
        zoomPercentage = $"{zp}%";
        scale = 0.01 * zp;
        await PdfViewerJsInterop.ZoomInOutAsync(objRef!, Id!, scale);
    }

    private async Task RotateClockwiseAsync()
    {
        rotation += 90;
        rotation = rotation.Equals(360) ? 0 : rotation;
        await PdfViewerJsInterop.RotateAsync(objRef!, Id!, rotation);

        // Orientation
        SetOrientation();
    }

    private async Task RotateCounterclockwiseAsync()
    {
        rotation -= 90;
        rotation = rotation.Equals(-360) ? 0 : rotation;
        await PdfViewerJsInterop.RotateAsync(objRef!, Id!, rotation);

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
        await PdfViewerJsInterop.RotateAsync(objRef!, Id!, rotation);
    }

    private async Task ZoomInAsync()
    {
        if (zoomLevel == MaxZoomLevel)
            return;

        zoomLevel += 1;
        var zp = GetZoomPercentage(zoomLevel);
        zoomPercentage = $"{zp}%";
        scale = 0.01 * zp;
        await PdfViewerJsInterop.ZoomInOutAsync(objRef!, Id!, scale);
    }

    private async Task ZoomOutAsync()
    {
        if (zoomLevel == MinZoomLevel)
            return;

        zoomLevel -= 1;
        var zp = GetZoomPercentage(zoomLevel);
        zoomPercentage = $"{zp}%";
        scale = 0.01 * zp;
        await PdfViewerJsInterop.ZoomInOutAsync(objRef!, Id!, scale);
    }

    #endregion

    #region Properties, Indexers

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
    /// <remarks>
    /// Default value is <see cref="Orientation.Portrait" />.
    /// </remarks>
    [Parameter]
    public Orientation Orientation { get; set; } = Orientation.Portrait;

    /// <summary>
    /// Provides JavaScript interop functionality for the PDF viewer.
    /// </summary>
    [Inject]
    private PdfViewerJsInterop PdfViewerJsInterop { get; set; } = default!;

    /// <summary>
    /// Gets or sets the URL of the PDF document to be displayed.
    /// PDF Viewer component supports base64 string as a URL.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? Url { get; set; }

    #endregion
}
