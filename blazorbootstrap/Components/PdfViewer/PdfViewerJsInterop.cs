namespace BlazorBootstrap;

public class PdfViewerJsInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public PdfViewerJsInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Blazor.Bootstrap/blazor.bootstrap.pdf.js").AsTask());
    }

    public async Task<PdfViewerModel> InitializeAsync(string elementId, string url)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<PdfViewerModel>("initialize", elementId, url);
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
