﻿namespace BlazorBootstrap;

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
        await module.InvokeVoidAsync("previousPage", objRef, elementId);
    }

    public async Task NextPageAsync(object objRef, string elementId)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("nextPage", objRef, elementId);
    }

    public async Task FirstPageAsync(object objRef, string elementId)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("firstPage", objRef, elementId);
    }

    public async Task LastPageAsync(object objRef, string elementId)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("lastPage", objRef, elementId);
    }

    public async Task ZoomInOutAsync(object objRef, string elementId, double scale)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("zoomInOut", objRef, elementId, scale);
    }

    public async Task RotateAsync(object objRef, string elementId, double rotationDegrees)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("rotate", objRef, elementId, rotationDegrees);
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
