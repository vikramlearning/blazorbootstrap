namespace BlazorBootstrap.Demo.RCL;

public partial class Demo : ComponentBase
{
    #region Fields and Constants

    private string? codeSnippet;

    /// <summary>
    /// A reference to this component instance for use in JavaScript calls.
    /// </summary>
    private DotNetObjectReference<Demo> objRef = default!;

    #endregion

    #region Methods

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await JS.InvokeVoidAsync("highlightCode");

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnParametersSetAsync()
    {
        if (codeSnippet is null)
        {
            var resourceName = Type.FullName + ".razor";

            using (var stream = Type.Assembly.GetManifestResourceStream(resourceName)!)
            {
                try
                {
                    if (stream is null)
                        return;

                    using (var reader = new StreamReader(stream))
                    {
                        codeSnippet = await reader.ReadToEndAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }

    private async Task CopyToClipboardAsync()
    {
        await JS.InvokeVoidAsync("copyToClipboard", codeSnippet, objRef);
    }

    /// <summary>
    /// Handles a script error event from JavaScript.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    [JSInvokable]
    public void OnCopyFailJS(string errorMessage)
    {
        Console.WriteLine($"OnCopyFailJS called, errorMessage: {errorMessage}");
    }

    /// <summary>
    /// Handles a script load event from JavaScript.
    /// </summary>
    [JSInvokable]
    public void OnCopySuccessJS()
    {
        Console.WriteLine($"OnCopySuccessJS called.");
    }

    [JSInvokable]
    public void ResetCopyStatusJS()
    {
        Console.WriteLine($"ResetCopyStatusJS called.");
    }

    #endregion

    #region Properties, Indexers

    [Inject] protected IJSRuntime JS { get; set; } = default!;

    [Parameter] public string LanguageCssClass { get; set; } = "language-cshtml";

    [Parameter] public bool ShowCodeOnly { get; set; }

    [Parameter] public bool Tabs { get; set; } = false;

    [Parameter] public Type Type { get; set; } = default!;

    #endregion
}
