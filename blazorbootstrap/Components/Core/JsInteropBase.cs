namespace BlazorBootstrap;

public abstract class JsInteropBase : IAsyncDisposable
{
    #region Fields and Constants

    private readonly Lazy<Task<IJSObjectReference>> moduleTask;
    private bool isJsRuntimeAvailable = true;

    #endregion

    #region Constructors

    protected JsInteropBase(IJSRuntime jsRuntime, string modulePath)
    {
        moduleTask = new Lazy<Task<IJSObjectReference>>(() =>
            jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath).AsTask());
    }

    #endregion

    #region Methods

    public async ValueTask DisposeAsync()
    {
        if (!moduleTask.IsValueCreated)
            return;

        try
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // Circuit is gone; ignore cleanup.
        }
        catch (TaskCanceledException)
        {
            // Circuit is shutting down; ignore cleanup.
        }
    }

    protected async Task SafeInvokeVoidAsync(string identifier, params object?[] args)
    {
        if (!isJsRuntimeAvailable)
            return;

        try
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync(identifier, args);
        }
        catch (JSDisconnectedException)
        {
            isJsRuntimeAvailable = false;
        }
        catch (TaskCanceledException)
        {
            // do nothing
        }
    }

    #endregion
}
