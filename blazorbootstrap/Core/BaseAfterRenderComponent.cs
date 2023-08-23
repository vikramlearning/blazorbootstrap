namespace BlazorBootstrap.Core;

/// <summary>
/// Base render component that implements render-queue logic.
/// </summary>
public abstract class BaseAfterRenderComponent : ComponentBase, IDisposable, IAsyncDisposable
{
    #region Fields and Constants

    /// <summary>
    /// A stack of functions to execute after the rendering.
    /// </summary>
    private Queue<Func<Task>>? executeAfterRenderQueue;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        Rendered = true;

        if (executeAfterRenderQueue?.Count > 0)
            while (executeAfterRenderQueue.Count > 0)
            {
                var action = executeAfterRenderQueue.Dequeue();

                await action();
            }

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc />
    public void Dispose() => Dispose(true);

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);

        Dispose(false);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="BaseComponent" /> and optionally releases the managed
    /// resources.
    /// </summary>
    /// <param name="disposing">True if the component is in the disposing process.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            Disposed = true;

            if (disposing) executeAfterRenderQueue?.Clear();
        }
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="BaseComponent" /> and optionally releases the managed
    /// resources.
    /// </summary>
    /// <param name="disposing">True if the component is in the disposing process.</param>
    protected virtual ValueTask DisposeAsync(bool disposing)
    {
        try
        {
            if (!AsyncDisposed) AsyncDisposed = true;

            return default;
        }
        catch (Exception exc)
        {
            return new ValueTask(Task.FromException(exc));
        }
    }

    /// <summary>
    /// Pushes an action to the stack to be executed after the rendering is done.
    /// </summary>
    /// <param name="action"></param>
    protected void ExecuteAfterRender(Func<Task> action)
    {
        executeAfterRenderQueue ??= new Queue<Func<Task>>();

        executeAfterRenderQueue.Enqueue(action);
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Indicates if the component is already fully disposed (asynchronously).
    /// </summary>
    protected bool AsyncDisposed { get; private set; }

    /// <summary>
    /// Indicates if the component is already fully disposed.
    /// </summary>
    protected bool Disposed { get; private set; }

    /// <summary>
    /// Indicates if component has been rendered in the browser.
    /// </summary>
    protected bool Rendered { get; private set; }

    #endregion
}