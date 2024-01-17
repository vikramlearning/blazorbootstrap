namespace BlazorBootstrap;

public partial class PdfViewer : BlazorBootstrapComponentBase
{
    private PdfViewerModel model = new();
    private DotNetObjectReference<PdfViewer>? objRef;
    [Inject] PdfViewerJsInterop PdfViewerJsInterop { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await PdfViewerJsInterop.InitializeAsync(objRef, ElementId, Url );
        await base.OnInitializedAsync();
    }

    [JSInvokable]
    public void Set(PdfViewerModel pdfViewerModel)
    {
        model = pdfViewerModel;
    }

    private async Task PreviousPageAsync() =>
        await PdfViewerJsInterop.PreviousPageAsync(objRef, ElementId);

    private async Task NextPageAsync() =>
        await PdfViewerJsInterop.NextPageAsync(objRef, ElementId);

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    [Parameter]
    public string? Url { get; set; }

    #endregion
}
