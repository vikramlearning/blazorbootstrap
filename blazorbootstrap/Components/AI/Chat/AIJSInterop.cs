namespace BlazorBootstrap;
internal class AIJSInterop : IAsyncDisposable
{

    #region Fields and Constants

    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    #endregion

    #region Constructors

    public AIJSInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new Lazy<Task<IJSObjectReference>>(()
            => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Blazor.Bootstrap/blazor.bootstrap.ai.js").AsTask());
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

    public async Task CreateChatCompletionsAsync(string key, object message, object objRef)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("createChatCompletions", key, message, objRef);
    }

    public async Task CreateChatCompletions2Async(string key, object message, object objRef)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("createChatCompletions2", key, message, objRef);
    }

    #endregion
}
