namespace BlazorBootstrap;

public partial class PdfViewer : BlazorBootstrapComponentBase
{
    private PdfViewerModel model;
    [Inject] PdfViewerJsInterop PdfViewerJsInterop { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        model = await PdfViewerJsInterop.InitializeAsync(ElementId, Url);
        await base.OnInitializedAsync();
    }

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    [Parameter]
    public string? Url { get; set; }

    #endregion
}
