namespace BlazorBootstrap;

public partial class PdfViewer : BlazorBootstrapComponentBase
{
    private int pageNumber = 0;
    private int pageCount = 0;

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
        pageCount = pdfViewerModel.PageCount;
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

    }

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    [Parameter]
    public string? Url { get; set; }

    #endregion
}
