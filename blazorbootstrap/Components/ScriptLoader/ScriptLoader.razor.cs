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
    private const string ScriptType = "text/javascript";

    /// <summary>
    /// A reference to this component instance for use in JavaScript calls.
    /// </summary>
    private DotNetObjectReference<ScriptLoader> objRef = default!;

    #endregion

    #region Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.scriptLoader.initialize", Id, Async, Defer, ScriptId, Source, ScriptType, objRef);

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
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.11.0")]
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether the script should be loaded asynchronously.")]
    [Parameter]
    public bool Async { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the script is meant to be executed 
    /// after the document has been parsed, but before firing DOMContentLoaded event.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.11.0")]
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether the script is meant to be executed after the document has been parsed, but before firing DOMContentLoaded event.")]
    [Parameter]
    public bool Defer { get; set; }

    /// <summary>
    /// An event that is fired when a script loading error occurs.
    /// </summary>
    [AddedVersion("1.11.0")]
    [Description("An event that is fired when a script loading error occurs.")]
    [Parameter]
    public EventCallback<string> OnError { get; set; }

    /// <summary>
    /// An event that is fired when a script has been successfully loaded.
    /// </summary>
    [AddedVersion("1.11.0")]
    [Description("An event that is fired when a script has been successfully loaded.")]
    [Parameter]
    public EventCallback OnLoad { get; set; }

    /// <summary>
    /// Gets or sets the ID of the script element.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.11.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the ID of the script element.")]
    [Parameter]
    public string? ScriptId { get; set; }

    /// <summary>
    /// Gets or sets the URI of the external script to load.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.11.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the URI of the external script to load.")]
    [EditorRequired]
    [Parameter]
    public string? Source { get; set; }

    // NOTE: Hiding base Class and Style properties to prevent their usage in this component.
    private new string? Class { get; set; }

    private new string? Style { get; set; }

    #endregion
}
