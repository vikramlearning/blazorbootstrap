namespace BlazorBootstrap;

public partial class ScriptLoader : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private const string type = "text/javascript";

    #endregion

    #region Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await JS.InvokeVoidAsync("window.blazorBootstrap.scriptLoader.load", ElementId, Async, ScriptId, Source, type);

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnParametersSet()
    {
        if (string.IsNullOrWhiteSpace(Source))
            throw new ArgumentNullException(nameof(Source));

        base.OnParametersSet();
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets the async.
    /// </summary>
    [Parameter]
    public bool Async { get; set; }

    /// <summary>
    /// Gets or set the script id.
    /// </summary>
    [Parameter]
    public string? ScriptId { get; set; }

    /// <summary>
    /// This parameter specifies the URI of an external script; this can be used as an alternative to embedding a script
    /// directly within a document.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public string? Source { get; set; } = default!;

    #endregion
}
