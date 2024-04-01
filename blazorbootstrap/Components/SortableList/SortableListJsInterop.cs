namespace BlazorBootstrap;

public class SortableListJsInterop : IAsyncDisposable
{
    #region Fields and Constants

    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    #endregion

    #region Constructors

    public SortableListJsInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new Lazy<Task<IJSObjectReference>>(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Blazor.Bootstrap/blazor.bootstrap.sortable-list.js").AsTask());
    }

    #endregion

    #region Methods

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }

    public async Task InitializeAsync(string elementId, string elementName, string handle, string group, bool allowSorting, object pull, object put, string filter, object objRef)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("initialize", elementId, elementName, handle, group, allowSorting, pull, put, filter, objRef);
    }

    #endregion
}
