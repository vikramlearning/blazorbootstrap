namespace BlazorBootstrap;

public abstract class BlazorBootstrapComponentBase : ComponentBase, IDisposable, IAsyncDisposable
{
    #region Fields and Constants

    private bool isAsyncDisposed = false;

    private bool isDisposed = false;

    #endregion

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
        Id ??= IdGenerator.GetNextId();

        base.OnInitialized();
    }

    /// <inheritdoc />
    /// <seealso cref="https://learn.microsoft.com/en-us/dotnet/api/system.idisposable?view=net-6.0" />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    /// <seealso cref="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#implement-both-dispose-and-async-dispose-patterns" />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore(true).ConfigureAwait(false);

        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                // cleanup
            }

            isDisposed = true;
        }
    }

    protected virtual ValueTask DisposeAsyncCore(bool disposing)
    {
        if (!isAsyncDisposed)
        {
            if (disposing)
            {
                // cleanup
            }

            isAsyncDisposed = true;
        }

        return ValueTask.CompletedTask;
    }

    ~BlazorBootstrapComponentBase()
    {
        Dispose(false);
    }

    #endregion

    #region Properties, Indexers

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = default!;

    [Parameter]
    public string? Class { get; set; }

    public ElementReference Element { get; set; }

    [Parameter]
    public string? Id { get; set; }

    [Inject] protected IIdGenerator IdGenerator { get; set; } = default!;

    protected bool IsRenderComplete { get; private set; }

    [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter]
    public string? Style { get; set; }

    #endregion
}
