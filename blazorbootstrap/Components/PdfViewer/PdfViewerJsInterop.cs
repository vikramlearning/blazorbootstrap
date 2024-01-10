namespace BlazorBootstrap;

public class PdfViewerJsInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public PdfViewerJsInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/Blazor.Bootstrap/pdf.viewer.js").AsTask());
    }

    public async ValueTask<string> Prompt(string message)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<string>("showPrompt", message);
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
