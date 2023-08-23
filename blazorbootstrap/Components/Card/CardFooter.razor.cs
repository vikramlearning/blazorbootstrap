namespace BlazorBootstrap;

public partial class CardFooter
{
    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(ClassProvider.CardFooter());

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