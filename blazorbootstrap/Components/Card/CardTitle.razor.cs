namespace BlazorBootstrap;

public partial class CardTitle
{
    #region Events

    #endregion

    #region Members

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.CardTitle());

        base.BuildClasses(builder);
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="ChildContent"/>.
    /// </summary>
    [Parameter] public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the heading size.
    /// </summary>
    [Parameter] public HeadingSize Size { get; set; } = HeadingSize.H5;

    #endregion
}
