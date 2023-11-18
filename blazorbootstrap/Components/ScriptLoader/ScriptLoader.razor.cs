namespace BlazorBootstrap;

public partial class ScriptLoader : BlazorBootstrapComponentBase
{
    #region Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await JS.InvokeVoidAsync("window.blazorBootstrap.scriptLoader.load", ElementId, Async, ScriptId, Source, Type);
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
    public string? Source { get; set; }

    /// <summary>
    /// This parameter indicates the type of script represented.
    /// </summary>
    [Parameter]
    public string? Type { get; set; }

    #endregion
}
