namespace BlazorBootstrap;

public abstract class BlazorBootstrapComponentBase : ComponentBase, IDisposable, IAsyncDisposable
{
    #region Fields and Constants

    private bool isAsyncDisposed;

    private bool isDisposed;

    internal Queue<Func<Task>> queuedTasks = new();

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // process queued tasks
        while (queuedTasks.TryDequeue(out var taskToExecute))
            await taskToExecute.Invoke();

        IsRenderComplete = true;
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        Id ??= IdUtility.GetNextId();
    }

    public static string BuildClassNames(params (string? cssClass, bool when)[] cssClassList)
    {
        var list = new HashSet<string>();

        if (cssClassList is not null && cssClassList.Any())
            foreach (var (cssClass, when) in cssClassList)
            {
                if (!string.IsNullOrWhiteSpace(cssClass) && when)
                    list.Add(cssClass);
            }

        if (list.Any())
            return string.Join(" ", list);
        else
            return string.Empty;
    }

    public static string BuildClassNames(string? userDefinedCssClass, params (string? cssClass, bool when)[] cssClassList)
    {
        var list = new HashSet<string>();

        if (cssClassList is not null && cssClassList.Any())
            foreach (var (cssClass, when) in cssClassList)
            {
                if (!string.IsNullOrWhiteSpace(cssClass) && when)
                    list.Add(cssClass);
            }

        if (!string.IsNullOrWhiteSpace(userDefinedCssClass))
            list.Add(userDefinedCssClass.Trim());

        if (list.Any())
            return string.Join(" ", list);
        else
            return string.Empty;
    }

    public static string BuildStyleNames(string? userDefinedCssStyle, params (string? cssStyle, bool when)[] cssStyleList)
    {
        var list = new HashSet<string>();

        if (cssStyleList is not null && cssStyleList.Any())
            foreach (var (cssStyle, when) in cssStyleList)
            {
                if (!string.IsNullOrWhiteSpace(cssStyle) && when)
                    list.Add(cssStyle);
            }

        if (!string.IsNullOrWhiteSpace(userDefinedCssStyle))
            list.Add(userDefinedCssStyle.Trim());

        if (list.Any())
            return string.Join(';', list);
        else
            return string.Empty;
    }

    /// <inheritdoc />
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.idisposable?view=net-6.0" />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    /// <see href="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#implement-both-dispose-and-async-dispose-patterns" />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore(true).ConfigureAwait(false);

        Dispose(false);
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

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets additional attributes that will be applied to the component.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets additional attributes that will be applied to the component.")]
    [ParameterTypeName("Dictionary<string, object>")]
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = default!;

    /// <summary>
    /// Gets or sets the CSS class name(s) to apply to the component.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the CSS class name(s) to apply to the component.")]
    [ParameterTypeName("string?")]
    [Parameter]
    public string? Class { get; set; }

    protected virtual string? ClassNames => Class;

    /// <summary>
    /// Gets or sets the associated <see cref="ElementReference" />.
    /// <para>
    /// May be <see langword="null" />, if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull]
    public ElementReference Element { get; set; }

    /// <summary>
    /// Gets or sets the ID. If not set, a unique ID will be generated.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the ID. If not set, a unique ID will be generated.")]
    [ParameterTypeName("string?")]
    [Parameter] 
    public string? Id { get; set; }

    protected bool IsRenderComplete { get; private set; }

    [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Gets or sets the CSS style string that defines the inline styles for the component.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the CSS style string that defines the inline styles for the component.")]
    [ParameterTypeName("string?")]
    [Parameter]
    public string? Style { get; set; }

    protected virtual string? StyleNames => Style;

    #endregion

    #region Other

    ~BlazorBootstrapComponentBase()
    {
        Dispose(false);
    }

    #endregion
}
