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

    #endregion
}

