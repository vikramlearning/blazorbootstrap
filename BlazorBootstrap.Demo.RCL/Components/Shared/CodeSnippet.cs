namespace BlazorBootstrap.Demo.RCL;

public class CodeSnippet : ComponentBase
{
    #region Members

    private string? code;

    #endregion

    #region Methods

    protected override async Task OnParametersSetAsync()
    {
        if (code is null)
        {
            if (!string.IsNullOrWhiteSpace(File))
            {
                var resourceName = File.Replace("~", typeof(CodeSnippet).Assembly.GetName().Name).Replace("/", ".").Replace("\\", ".");

                using (Stream stream = typeof(CodeSnippet).Assembly.GetManifestResourceStream(resourceName)!)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        code = await reader.ReadToEndAsync();
                    }
                }
            }
        }
    }

    private string? GetLanguageFromFileExtension()
    {
        if (string.IsNullOrWhiteSpace(File))
            return null;

        return Path.GetExtension(File).ToLower() switch
        {
            ".razor" => "cshtml",
            ".cshtml" => "cshtml",
            ".html" => "html",
            ".css" => "css",
            ".cs" => "csharp",
            ".txt" => "none",
            ".js" => "js",
            _ => null
        };
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // no base call
        builder.AddMarkupContent(0, "<!--googleoff: index-->");

        builder.OpenElement(300, "div");
        builder.AddAttribute(301, "class", "highlight show-code-only");
        builder.OpenElement(400, "pre");
        builder.OpenElement(401, "code");
        builder.AddAttribute(402, "class", $"language-{Language ?? GetLanguageFromFileExtension() ?? "cshtml"}");
        if (code != null)
        {
            builder.AddContent(403, code.Trim());
        }
        builder.CloseElement(); // end: code
        builder.CloseElement(); // end: pre
        builder.CloseElement();

        builder.AddMarkupContent(700, "<!--googleon: index-->");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(firstRender)
            await JS.InvokeVoidAsync("highlightCode");

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region Properties

    [Inject] protected IJSRuntime JS { get; set; } = null!;

    [Parameter] public string? Language { get; set; }

    [Parameter] public string? File { get; set; }

    #endregion
}
