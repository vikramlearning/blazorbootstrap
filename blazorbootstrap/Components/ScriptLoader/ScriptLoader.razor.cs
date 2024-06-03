namespace BlazorBootstrap;

/// <summary>
/// A component for loading scripts dynamically in a Blazor application.
/// </summary>
public partial class ScriptLoader : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    /// <summary>
    /// The default content type for scripts.
    /// </summary>
    private const string Type = "text/javascript";

    /// <summary>
    /// A reference to this component instance for use in JavaScript calls.
    /// </summary>
    private DotNetObjectReference<ScriptLoader> objRef = default!;

    #endregion

    #region Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.scriptLoader.initialize", Id, Async, ScriptId, Source, Type, objRef);

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        await base.OnInitializedAsync();
    }

    protected override void OnParametersSet()
    {
        if (string.IsNullOrWhiteSpace(Source))
            throw new ArgumentNullException(nameof(Source));

        base.OnParametersSet();
    }

    /// <summary>
    /// Handles a script error event from JavaScript.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    [JSInvokable]
    public void OnErrorJS(string errorMessage)
    {
        if (OnError.HasDelegate)
            OnError.InvokeAsync(errorMessage);
    }

    /// <summary>
    /// Handles a script load event from JavaScript.
    /// </summary>
    [JSInvokable]
    public void OnLoadJS()
    {
        if (OnLoad.HasDelegate)
            OnLoad.InvokeAsync();
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets a value indicating whether the script should be loaded asynchronously.
    /// </summary>
    [Parameter]
    public bool Async { get; set; }

    /// <summary>
    /// An event that is fired when a script loading error occurs.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnError { get; set; }

    /// <summary>
    /// An event that is fired when a script has been successfully loaded.
    /// </summary>
    [Parameter]
    public EventCallback OnLoad { get; set; }

    /// <summary>
    /// Gets or sets the ID of the script element.
    /// </summary>
    [Parameter]
    public string? ScriptId { get; set; }

    /// <summary>
    /// Gets or sets the URI of the external script to load.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public string? Source { get; set; } = default!;

    #endregion
}
