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
    /// The list of class names.
    /// </summary>
    private HashSet<string> classList = new();

    /// <summary>
    /// The class names, as a string.
    /// </summary>
    private string? classNames;

    /// <summary>
    /// Whether the class names are dirty and need to be rebuilt.
    /// </summary>
    private bool isClassDirty = true;

    /// <summary>
    /// Whether the styles are dirty and need to be rebuilt.
    /// </summary>
    private bool isStyleDirty = true;

    /// <summary>
    /// The custom styles for the component.
    /// </summary>
    private string? style;

    /// <summary>
    /// The list of styles.
    /// </summary>
    private HashSet<string> styleList = new();

    /// <summary>
    /// The styles, as a string.
    /// </summary>
    private string? styleNames;

    /// <summary>
    /// A stack of functions to execute after the rendering.
    /// </summary>
    private PriorityQueue<Func<Task>, RenderPriority>? renderQueue;

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

    /// <summary>
    /// Appends a string to the class name.
    /// </summary>
    /// <param name="value">The string to append.</param>
    internal void AddClass(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            classList.Add(value);
    }

    /// <summary>
    /// Appends a string to the class name if the specified condition is true.
    /// </summary>
    /// <param name="value">The string to append.</param>
    /// <param name="when">The condition to check.</param>
    internal void AddClass(string value, bool when)
    {
        if (when && !string.IsNullOrWhiteSpace(value))
            classList.Add(value);
    }

    /// <summary>
    /// Appends a list of strings to the class name.
    /// </summary>
    /// <param name="values">The list of strings to append.</param>
    internal void AddClass(IEnumerable<string> values)
    {
        if (values is not null && values.Any())
            classList.UnionWith(values); // TODO: performance check
    }

    /// <summary>
    /// Appends a string to the style.
    /// </summary>
    /// <param name="value">The string to append.</param>
    internal void AddStyle(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            styleList.Add(value);
    }

    /// <summary>
    /// Appends a string to the style if the specified condition is true.
    /// </summary>
    /// <param name="value">The string to append.</param>
    /// <param name="condition">The condition to check.</param>
    internal void AddStyle(string value, bool condition)
    {
        if (condition && !string.IsNullOrWhiteSpace(value))
            styleList.Add(value);
    }

    /// <summary>
    /// Appends a list of strings to the styles.
    /// </summary>
    /// <param name="values">The list of strings to append.</param>
    internal void AddStyle(IEnumerable<string> values)
    {
        if (values is not null && values.Any())
            styleList.UnionWith(values); // TODO: performance check
    }

    /// <summary>
    /// Marks the classList as dirty, so that the class names will be rebuilt the next time they are requested.
    /// </summary>
    public void Dirty() => isClassDirty = true;

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
    protected internal virtual void DirtyClasses() => isClassDirty = true;

    /// <summary>
    /// Builds the class names for this component.
    /// </summary>
    protected virtual void BuildClasses()
    {
        if (Class != null)
            AddClass(Class);
    }

    /// <summary>
    /// Builds the class names for this component.
    /// </summary>
    protected virtual void BuildStyles()
    {
        if (Style != null)
            AddStyle(Style);
    }

    /// <summary>
    /// Marks the styles as dirty, so that they will be regenerated the next time they are requested.
    /// </summary>
    protected virtual void DirtyStyles() => isStyleDirty = true;

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
                classList = null!;
                styleList = null!;
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
                    classList = null!;
                    styleList = null!;
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
    /// <param name="renderPriority"></param>
    protected void QueueAfterRenderAction(Func<Task> action, RenderPriority renderPriority)
    {
        renderQueue ??= new PriorityQueue<Func<Task>, RenderPriority>();
        renderQueue.Enqueue(action, renderPriority);
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
    public Dictionary<string, object> Attributes { get; set; } = default!;

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
    /// Gets the class names.
    /// </summary>
    /// <remarks>
    /// The class names are lazily built, so the first time this property is accessed, the `buildClasses` action will be
    /// called.
    /// </remarks>
    public string? ClassNames
    {
        get
        {
            if (isClassDirty)
            {
                classList = new HashSet<string>();

                BuildClasses();

                classNames = classList.Any() ? string.Join(" ", classList) : null;

                isClassDirty = false;
            }

            return classNames;
        }
    }

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
    /// Gets the styles.
    /// </summary>
    /// <remarks>
    /// The styles are lazily built, so the first time this property is accessed, the `buildStyles` action will be called.
    /// </remarks>
    public string? StyleNames
    {
        get
        {
            if (isStyleDirty)
            {
                styleList = new HashSet<string>();

                BuildStyles();

                styleNames = styleList.Any() ? string.Join(";", styleList) : null;

                isStyleDirty = false;
            }

            return styleNames;
        }
    }
    
    #endregion
}
