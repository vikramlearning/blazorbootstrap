namespace BlazorBootstrap.Demo.RCL;

public class Snippet : ComponentBase
{
    #region Members

    private string? snippet;

    #endregion

    #region Methods

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(300, "div");
        builder.AddAttribute(301, "class", "highlight show-code-only");

        builder.OpenElement(400, "pre");

        builder.OpenElement(500, "code");
        builder.AddAttribute(501, "class", LanguageCode.ToLanguageCssClass());

        if (snippet != null)
            builder.AddContent(600, snippet.Trim());

        builder.CloseElement(); // end: code
        builder.CloseElement(); // end: pre
        builder.CloseElement(); // end: div
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await JS.InvokeVoidAsync("highlightCode");

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnParametersSetAsync()
    {
        if (snippet is null)
        {
            if (!string.IsNullOrWhiteSpace(FilePath))
            {
                var resourceName = FilePath.Replace("~", typeof(Snippet).Assembly.GetName().Name).Replace("/", ".").Replace("\\", ".");

                using (var stream = typeof(Snippet).Assembly.GetManifestResourceStream(resourceName)!)
                {
                    try
                    {
                        if (stream is null)
                            return;

                        using (var reader = new StreamReader(stream))
                        {
                            snippet = await reader.ReadToEndAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }

    #endregion

    #region Properties

    [Inject] protected IJSRuntime JS { get; set; } = null!;

    [Parameter] public LanguageCode LanguageCode { get; set; } = LanguageCode.Razor;

    [Parameter] public string? FilePath { get; set; }

    #endregion
}
