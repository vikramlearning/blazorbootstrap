namespace BlazorBootstrap;

public partial class Card : BlazorBootstrapComponentBase
{
    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses()
    {
        this.AddClass(BootstrapClassProvider.Card);
        this.AddClass(BootstrapClassProvider.ToTextAlignment(TextAlignment));
        this.AddClass(BootstrapClassProvider.ToCardColor(Color));

        base.BuildClasses();
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

    /// <summary>
    /// Gets or sets the card color.
    /// </summary>
    [Parameter]
    public CardColor Color { get; set; }

    /// <summary>
    /// Gets or sets the text alignment of the card.
    /// </summary>
    [Parameter]
    public Alignment TextAlignment { get; set; }

    #endregion
}
