namespace BlazorBootstrap;

public partial class Accordion
{
    #region Events

    #endregion

    #region Members

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Accordion());
        builder.Append(BootstrapClassProvider.AccordionFlush(), Flush);
        base.BuildClasses(builder);
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="Collapse"/>.
    /// </summary>
    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the flush.
    /// Removes borders and rounded corners to render accordions edge-to-edge with their parent container.
    /// </summary>
    [Parameter] public bool Flush { get; set; }

    /// <summary>
    /// Gets or sets the AlwaysOpen.
    /// It makes accordion items stay open when another item is opened.
    /// </summary>
    [Parameter] public bool AlwaysOpen { get; set; }

    #endregion
}

