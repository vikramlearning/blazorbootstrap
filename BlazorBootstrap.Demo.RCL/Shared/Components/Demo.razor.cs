namespace BlazorBootstrap.Demo.RCL;

public partial class Demo : ComponentBase
{
    #region Members

    private string? codeSnippet;

    #endregion

    #region Methods

    protected override async Task OnParametersSetAsync()
    {
        if (codeSnippet is null)
        {
            var resourceName = Type.FullName + ".razor";
            using (Stream stream = Type.Assembly.GetManifestResourceStream(resourceName)!)
            {
                try
                {
                    if (stream is null)
                        return;

                    using (StreamReader reader = new StreamReader(stream))
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        await JS.InvokeVoidAsync("highlightCode");
    }

    #endregion

    #region Properties

    [Inject] protected IJSRuntime JS { get; set; } = default!;

    [Parameter] public string LanguageCssClass { get; set; } = "language-cshtml";

    [Parameter] public bool ShowCodeOnly { get; set; }

    [Parameter] public bool Tabs { get; set; } = false;

    [Parameter] public Type Type { get; set; } = default!;

    #endregion
}
