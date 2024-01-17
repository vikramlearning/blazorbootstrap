namespace BlazorBootstrap;

public class PdfViewerJsInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public PdfViewerJsInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Blazor.Bootstrap/blazor.bootstrap.pdf.js").AsTask());
    }

    public async Task InitializeAsync(object objRef, string elementId, string url)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("initialize", objRef, elementId, url);
    }

    public async Task PreviousPageAsync(object objRef, string elementId)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("previous", objRef, elementId);
    }

    public async Task NextPageAsync(object objRef, string elementId)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("next", objRef, elementId);
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
