namespace BlazorBootstrap;

/// <summary>
/// The base class for all BlazorBootstrap components.
/// </summary>
/// <remarks>
/// This class provides common functionality for all BlazorBootstrap components, such as
/// generating an ID for the component, building the class names and styles, and handling
/// custom attributes.
/// </remarks>
public abstract class BlazorBootstrapComponentBase : ComponentBase, IDisposable, IAsyncDisposable
{
    #region Fields and Constants

    /// <summary>
    /// The custom class names for the component.
    /// </summary>
    private string? @class;

    /// <summary>
    /// A stack of functions to execute after the rendering.
    /// </summary>
    private Queue<Func<Task>>? renderQueue;

    /// <summary>
    /// The custom styles for the component.
    /// </summary>
    private string? style;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="BlazorBootstrapComponentBase" /> class.
    /// </summary>
#pragma warning disable CS8618
    public BlazorBootstrapComponentBase()
#pragma warning restore CS8618
    {
        CssClassBuilder = new CssClassBuilder(BuildClasses);
        CssStyleBuilder = new CssStyleBuilder(BuildStyles);
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        Rendered = true;

        if (renderQueue?.Count > 0)
            while (renderQueue.Count > 0)
            {
                var action = renderQueue.Dequeue();
                await action();
            }

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <summary>
    /// Called when the component is initialized.
    /// </summary>
    protected override void OnInitialized()
    {
        if (ShouldAutoGenerateId && ElementId == null) ElementId = IdGenerator.GetNextId();

        base.OnInitialized();
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
    /// Marks the class names as dirty, so that they will be regenerated the next time they are requested.
    /// </summary>
    protected internal virtual void DirtyClasses() => CssClassBuilder?.Dirty();

    /// <summary>
    /// Builds the class names for this component.
    /// </summary>
    /// <param name="builder">The class builder used to append the class names.</param>
    protected virtual void BuildClasses(CssClassBuilder builder)
    {
        if (Class != null)
            builder.Append(Class);
    }

    /// <summary>
    /// Builds the styles for this component.
    /// </summary>
    /// <param name="builder">The style builder used to append the styles.</param>
    protected virtual void BuildStyles(CssStyleBuilder builder)
    {
        if (Style != null)
            builder.Append(Style);
    }

    /// <summary>
    /// Marks the styles as dirty, so that they will be regenerated the next time they are requested.
    /// </summary>
    protected virtual void DirtyStyles() => CssStyleBuilder?.Dirty();

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {
                CssClassBuilder = null;
                CssStyleBuilder = null;
            }

            if (disposing && renderQueue != null)
            {
                renderQueue.Clear();
                renderQueue = null;
            }

            Disposed = true;
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual ValueTask DisposeAsync(bool disposing)
    {
        try
        {
            if (!AsyncDisposed)
            {
                if (disposing)
                {
                    CssClassBuilder = null;
                    CssStyleBuilder = null;
                }

                AsyncDisposed = true;
            }

            return default;
        }
        catch (Exception ex)
        {
            return new ValueTask(Task.FromException(ex));
        }
    }

    /// <summary>
    /// Pushes an action to the stack to be executed after the rendering is done.
    /// </summary>
    /// <param name="action"></param>
    protected void QueueAfterRenderAction(Func<Task> action)
    {
        renderQueue ??= new Queue<Func<Task>>();
        renderQueue.Enqueue(action);
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Indicates if the component is already fully disposed (asynchronously).
    /// </summary>
    protected bool AsyncDisposed { get; private set; }

    /// <summary>
    /// Captures all the custom attributes that are not part of BlazorBootstrap component.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>
    /// Optional CSS class names. If given, these will be included in the class attribute of the component.
    /// </summary>
    [Parameter]
    public string? Class
    {
        get => @class;
        set
        {
            @class = value;

            DirtyClasses();
        }
    }

    /// <summary>
    /// Gets the built class-names based on all the rules set by the component parameters.
    /// </summary>
    public string? ClassNames => CssClassBuilder!.ClassNames;

    /// <summary>
    /// Gets the class builder.
    /// </summary>
    protected CssClassBuilder? CssClassBuilder { get; private set; }

    /// <summary>
    /// Gets the style mapper.
    /// </summary>
    protected CssStyleBuilder? CssStyleBuilder { get; private set; }

    /// <summary>
    /// Indicates if the component is already fully disposed.
    /// </summary>
    protected bool Disposed { get; private set; }

    /// <summary>
    /// Gets or sets the unique id of the element.
    /// </summary>
    /// <remarks>
    /// Note that this ID is not defined for the component but instead for the underlined element that it represents.
    /// </remarks>
    [Parameter]
    public string? ElementId { get; set; }

    /// <summary>
    /// Gets or sets the reference to the rendered element.
    /// </summary>
    public ElementReference ElementRef { get; set; }

    [Inject] protected IIdGenerator IdGenerator { get; set; } = default!;

    [Inject] protected IJSRuntime JS { get; set; } = default!;

    /// <summary>
    /// Indicates if component has been rendered in the browser.
    /// </summary>
    protected bool Rendered { get; private set; }

    /// <summary>
    /// If true, <see cref="ElementId" /> will be auto-generated on component initialize.
    /// </summary>
    /// <remarks>
    /// Override this in components that need to have an id defined before calling JSInterop.
    /// </remarks>
    protected virtual bool ShouldAutoGenerateId => false;

    /// <summary>
    /// Optional in-line styles. If given, these will be included in the style attribute of the component.
    /// </summary>
    [Parameter]
    public string? Style
    {
        get => style;
        set
        {
            style = value;

            DirtyStyles();
        }
    }

    /// <summary>
    /// Gets the built styles based on all the rules set by the component parameters.
    /// </summary>
    public string? StyleNames => CssStyleBuilder!.Styles;

    #endregion
}
