using System.Text.Json;

namespace BlazorBootstrap;
/// <summary>
/// Code element for PrismJS highlighting
/// </summary>
public partial class Code : BlazorBootstrapComponentBase
{

    /// <summary>
    /// Current markup string.
    /// </summary>
    [Parameter] public MarkupString CodeString { get; set; } = new(String.Empty);

    /// <summary>
    /// The language of the code.
    /// </summary>
    [Parameter] [EditorRequired] public LanguageCode Language { get; set; }

    /// <summary>
    /// How the text will be structured inside the code element.
    /// </summary>
    [Parameter] [EditorRequired] public LanguageCodeStyles CodeStyle { get; set; }

    /// <summary>
    /// Set the code to be displayed in the element.
    /// </summary>
    /// <param name="code">Code to be shown</param>
    /// <returns><see cref="Task"/> that is being executed and awaits completion, running on the main UI thread.</returns>
    public Task SetCode(string code)
    {
        return Task.Run(async () =>
        {
            var htmlCodeData = await JsRuntime
                .InvokeAsync<string>(
                    "window.blazorBootstrap.code.highlight",
                    code,
                    CodeExtensions.SupportedCodeStyles[CodeStyle],
                    CodeExtensions.SupportedCodeLanguages[Language])
                .ConfigureAwait(false);
            CodeString = new MarkupString(htmlCodeData);
            await InvokeAsync(StateHasChanged);
        });
    }

    /// <summary>
    /// Dependency injected Javascript Runtime
    /// </summary>
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
}
