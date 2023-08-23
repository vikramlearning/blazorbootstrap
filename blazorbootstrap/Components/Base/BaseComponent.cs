namespace BlazorBootstrap;

/// <summary>
/// The base class for all BlazorBootstrap components.
/// </summary>
/// <remarks>
/// This class provides common functionality for all BlazorBootstrap components, such as
/// generating an ID for the component, building the class names and styles, and handling
/// custom attributes.
/// </remarks>
public abstract class BaseComponent : ComponentBase, IDisposable, IAsyncDisposable
{
    #region Fields and Constants

    /// <summary>
    /// The custom class names for the component.
    /// </summary>
    private string? customClass;

    /// <summary>
    /// The custom styles for the component.
    /// </summary>
    private string? customStyle;

    /// <summary>
    /// A stack of functions to execute after the rendering.
    /// </summary>
    private Queue<Func<Task>>? executeAfterRenderQueue;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseComponent" /> class.
    /// </summary>
    public BaseComponent()
    {
        ClassBuilder = new CssClassBuilder(BuildClasses);
        StyleBuilder = new CssStyleBuilder(BuildStyles);
    }

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

    /// <summary>
    /// Called when the component is initialized.
    /// </summary>
    protected override void OnInitialized()
    {
        if (ShouldAutoGenerateId && ElementId == null) ElementId = IdGenerator.Generate;

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
    protected internal virtual void DirtyClasses() => ClassBuilder?.Dirty();

    /// <summary>
    /// Builds the class names for this component.
    /// </summary>
    /// <param name="builder">The class builder used to append the class names.</param>
    protected virtual void BuildClasses(CssClassBuilder builder)
    {
        if (Class != null)
            builder.Append(Class);

        // TODO: update this
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
    protected virtual void DirtyStyles() => StyleBuilder?.Dirty();

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
                ClassBuilder = null;
                StyleBuilder = null;
            }

            if (disposing && executeAfterRenderQueue != null)
            {
                executeAfterRenderQueue.Clear();
                executeAfterRenderQueue = null;
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
                    ClassBuilder = null;
                    StyleBuilder = null;
                }

                AsyncDisposed = true;
            }

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
        get => customClass;
        set
        {
            customClass = value;

            DirtyClasses();
        }
    }

    /// <summary>
    /// Gets the class builder.
    /// </summary>
    protected CssClassBuilder? ClassBuilder { get; private set; }

    /// <summary>
    /// Gets the built class-names based on all the rules set by the component parameters.
    /// </summary>
    public string? ClassNames => ClassBuilder.Class;

    /// <summary>
    /// Gets or sets the classname provider.
    /// </summary>
    [Inject]
    protected ClassProvider ClassProvider { get; set; }

    /// <summary>
    /// Indicates if the component is already fully disposed.
    /// </summary>
    protected bool Disposed { get; private set; }

    /// <summary>
    /// Gets or sets the unique id of the element.
    /// </summary>
    /// <remarks>
    /// Note that this ID is not defined for the component but instead for the underlined element that it represents.
    /// eg: for the TextEdit the ID will be set on the input element.
    /// </remarks>
    [Parameter]
    public string? ElementId { get; set; }

    /// <summary>
    /// Gets or sets the reference to the rendered element.
    /// </summary>
    public ElementReference ElementRef { get; set; }

    [Inject] protected IIdGenerator IdGenerator { get; set; }

    [Inject] protected IJSRuntime JS { get; set; }

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
        get => customStyle;
        set
        {
            customStyle = value;

            DirtyStyles();
        }
    }

    /// <summary>
    /// Gets the style mapper.
    /// </summary>
    protected CssStyleBuilder? StyleBuilder { get; private set; }

    /// <summary>
    /// Gets the built styles based on all the rules set by the component parameters.
    /// </summary>
    public string? StyleNames => StyleBuilder.Styles;

    #endregion
}