namespace BlazorBootstrap;

public partial class CardBody
{
    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.CardBody());

        base.BuildClasses(builder);
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="ChildContent" />.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    #endregion
}