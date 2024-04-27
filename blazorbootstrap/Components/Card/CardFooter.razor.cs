namespace BlazorBootstrap;

public partial class CardFooter : BlazorBootstrapComponentBase
{
    #region Methods

    protected string? ClassNames => new CssClassBuilder(Class)
        .AddClass(BootstrapClass.CardFooter)
        .Build();

    protected string? StyleNames => new CssStyleBuilder(Style).Build();

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    #endregion
}
