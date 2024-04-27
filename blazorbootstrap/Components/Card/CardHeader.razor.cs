namespace BlazorBootstrap;

public partial class CardHeader : BlazorBootstrapComponentBase
{
    #region Methods

    protected string? ClassNames => new CssClassBuilder(Class)
        .AddClass(BootstrapClass.CardHeader)
        .AddClass(Color.ToCardColorClass())
        .Build();

    protected string? StyleNames => new CssStyleBuilder(Style).Build();

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the card header color.
    /// </summary>
    [Parameter]
    public CardColor Color { get; set; }

    #endregion
}
