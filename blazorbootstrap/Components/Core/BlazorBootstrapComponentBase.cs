
namespace BlazorBootstrap;

/// <summary>
/// Root component for all Blazor Bootstrap components.
/// </summary>
public abstract class BlazorBootstrapComponentBase : ComponentBase, IDisposable, IAsyncDisposable
{
    #region Methods

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        IsRenderComplete = true;

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        Id ??= IdUtility.GetNextId();

        base.OnInitialized();
    } 
    
    /// <inheritdoc />
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.idisposable" />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    /// <seealso
    ///     href="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#implement-both-dispose-and-async-dispose-patterns" />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore(true).ConfigureAwait(false);
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// This method executes disposing in two distinct scenarios.
    /// If <paramref name="disposing" /> is <see langword="true" />, the method has been called directly
    /// or indirectly by a user's code. This means managed and unmanaged resources can be disposed.
    /// If <paramref name="disposing"/> is <see langword="false" />, the method has been called by the runtime
    /// from inside the finalizer, and you should not reference other objects. Only unmanaged resources can be disposed.
    /// </summary>
    /// <param name="disposing">User-invoked disposing</param>
    protected virtual void Dispose(bool disposing)
    {
        DisposeAsyncCore(disposing).AsTask().Wait(); 
    }

    /// <summary>
    /// Virtual method to clean-up resources asynchronously.
    /// By default, no resources are cleaned up, unless overridden in a derived class.
    /// If <paramref name="disposing" /> is <see langword="true" />, the method has been called directly
    /// or indirectly by a user's code. This means managed and unmanaged resources can be disposed.
    /// If <paramref name="disposing"/> is <see langword="false" />, the method has been called by the runtime
    /// from inside the finalizer, and you should not reference other objects. Only unmanaged resources can be disposed.
    /// </summary>
    /// <param name="disposing">User-invoked disposing</param>
    /// <returns></returns>
    protected virtual ValueTask DisposeAsyncCore(bool disposing)
    {
        return ValueTask.CompletedTask;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Additional HTML attributes to be added to the component.
    /// </summary>
    protected Dictionary<string, object> AdditionalAttributes { get; } = new();

    /// <summary>
    /// class attribute to be applied in the Html render, for usages such as CSS.
    /// </summary>
    [Parameter] public string Class { get; set; } = "";
      
    /// <summary>
    /// Element reference for the component, to use in code when interfacing with the component.
    /// </summary>
    public ElementReference Element { get; protected set; }

    /// <summary>
    /// The HTML id of the component. If left empty, a unique id will be generated. <br/>
    /// </summary>
    /// <remarks>
    /// Ensure the id of the component is unique throughout the entire HTML page! It is not enough
    /// to have the id be unique within a razor page or set of components, the layout of the page counts as well!
    /// </remarks>
    [Parameter] public string? Id { get; set; }

    /// <summary>
    /// Determines if the element had been rendered for the first time (assigned true in <see cref="OnAfterRenderAsync(bool)"/>). <br/>
    /// This is useful for Javascript interop, to ensure the element is rendered before attempting to interact with it.
    /// </summary>
    protected bool IsRenderComplete { get; private set; }

    #endregion

    #region Other

    /// <summary>
    /// Default destructor, to ensure <see cref="IDisposable"/> is executed properly.
    /// </summary>
    ~BlazorBootstrapComponentBase()
    {
        Dispose(false);
    }

    #endregion
}
