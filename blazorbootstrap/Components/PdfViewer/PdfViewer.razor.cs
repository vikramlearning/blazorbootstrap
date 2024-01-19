﻿namespace BlazorBootstrap;

public partial class PdfViewer : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private double maxScale = 5;
    private int maxZoomLevel = 17;
    private double minScale = 0.25;

    private int minZoomLevel = 1;

    private DotNetObjectReference<PdfViewer>? objRef;
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
        objRef ??= DotNetObjectReference.Create(this);
        await PdfViewerJsInterop.InitializeAsync(objRef, ElementId, scale, rotation, Url);
        await base.OnInitializedAsync();
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

    private async Task FirstPageAsync() => await PdfViewerJsInterop.FirstPageAsync(objRef, ElementId);

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

    private async Task LastPageAsync() => await PdfViewerJsInterop.LastPageAsync(objRef, ElementId);

    private async Task NextPageAsync() => await PdfViewerJsInterop.NextPageAsync(objRef, ElementId);

    private async Task PageNumberChangedAsync(int value)
    {
        if (value < 1 || value > pagesCount)
            pageNumber = 1;
        else
            pageNumber = value;

        await PdfViewerJsInterop.GotoPageAsync(objRef, ElementId, pageNumber);
    }

    private async Task PreviousPageAsync() => await PdfViewerJsInterop.PreviousPageAsync(objRef, ElementId);

    private async Task PrintAsync() => await PdfViewerJsInterop.PrintAsync(objRef, ElementId);

    private async Task RotateClockwiseAsync()
    {
        rotation += 90;
        rotation = rotation == 360 ? 0 : rotation;
        await PdfViewerJsInterop.RotateAsync(objRef, ElementId, rotation);
    }

    private async Task RotateCounterclockwiseAsync()
    {
        rotation -= 90;
        rotation = rotation == -360 ? 0 : rotation;
        await PdfViewerJsInterop.RotateAsync(objRef, ElementId, rotation);
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

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    [Parameter] public EventCallback<PdfViewerEventArgs> OnDocumentLoaded { get; set; }

    [Parameter] public EventCallback<PdfViewerEventArgs> OnPageChanged { get; set; }

    [Inject] private PdfViewerJsInterop PdfViewerJsInterop { get; set; } = default!;

    [Parameter] public string? Url { get; set; }

    #endregion
}
