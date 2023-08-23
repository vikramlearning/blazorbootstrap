using BlazorBootstrap.Core;

namespace BlazorBootstrap;

public abstract class BaseComponent : BaseAfterRenderComponent
{
    #region Fields and Constants

    private string customClass;

    private string customStyle;

    #endregion

    #region Constructors

    public BaseComponent()
    {
        ClassBuilder = new ClassBuilder(BuildClasses);
        StyleBuilder = new StyleBuilder(BuildStyles);
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ClassBuilder = null;
            StyleBuilder = null;
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc />
    protected override ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            ClassBuilder = null;
            StyleBuilder = null;
        }

        return base.DisposeAsync(disposing);
    }

    protected override void OnInitialized()
    {
        if (ShouldAutoGenerateId && ElementId == null) ElementId = IdGenerator.Generate;

        base.OnInitialized();
    }

    /// <summary>
    /// Clears the class-names and mark them to be regenerated.
    /// </summary>
    protected internal virtual void DirtyClasses() => ClassBuilder?.Dirty();

    /// <summary>
    /// Builds a list of classnames for this component.
    /// </summary>
    /// <param name="builder">Class builder used to append the classnames.</param>
    protected virtual void BuildClasses(ClassBuilder builder)
    {
        if (Class != null)
            builder.Append(Class);

        // TODO: update this
    }

    /// <summary>
    /// Builds a list of styles for this component.
    /// </summary>
    /// <param name="builder">Style builder used to append the styles.</param>
    protected virtual void BuildStyles(StyleBuilder builder)
    {
        if (Style != null)
            builder.Append(Style);
    }

    /// <summary>
    /// Clears the styles-names and mark them to be regenerated.
    /// </summary>
    protected virtual void DirtyStyles() => StyleBuilder?.Dirty();

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Captures all the custom attribute that are not part of BlazorBootstrap component.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>
    /// Gets or sets the classname provider.
    /// </summary>
    [Inject]
    protected ClassProvider ClassProvider { get; set; }

    /// <summary>
    /// Custom css classname.
    /// </summary>
    [Parameter]
    public string Class
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
    protected ClassBuilder? ClassBuilder { get; private set; }

    /// <summary>
    /// Gets the built class-names based on all the rules set by the component parameters.
    /// </summary>
    public string ClassNames => ClassBuilder.Class;

    /// <summary>
    /// Gets or sets the unique id of the element.
    /// </summary>
    /// <remarks>
    /// Note that this ID is not defined for the component but instead for the underlined element that it represents.
    /// eg: for the TextEdit the ID will be set on the input element.
    /// </remarks>
    [Parameter]
    public string ElementId { get; set; }

    /// <summary>
    /// Gets or sets the reference to the rendered element.
    /// </summary>
    public ElementReference ElementRef { get; set; }

    /// <summary>
    /// Gets or set the javascript runner.
    /// </summary>
    [Inject]
    protected IIdGenerator IdGenerator { get; set; }

    [Inject] protected IJSRuntime JS { get; set; }

    /// <summary>
    /// If true, <see cref="ElementId" /> will be auto-generated on component initialize.
    /// </summary>
    /// <remarks>
    /// Override this in components that need to have an id defined before calling JSInterop.
    /// </remarks>
    protected virtual bool ShouldAutoGenerateId => false;

    /// <summary>
    /// Custom html style.
    /// </summary>
    [Parameter]
    public string Style
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
    protected StyleBuilder? StyleBuilder { get; private set; }

    /// <summary>
    /// Gets the built styles based on all the rules set by the component parameters.
    /// </summary>
    public string StyleNames => StyleBuilder.Styles;

    #endregion

    // TODO: update this
}