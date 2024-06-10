namespace BlazorBootstrap;

/// <summary>
/// Bootstrap's cards provide a flexible and extensible content container with multiple variants and options. <br/>
/// This component is based on the <see href="https://getbootstrap.com/docs/5.0/components/card/">Bootstrap Card</see> component.
/// </summary>
public partial class Card : BlazorBootstrapComponentBase
{
    #region Properties, Indexers
    
    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Card, true),
            (TextAlignment.ToTextAlignmentClass(), true),
            (Color.ToCardColorClass(), true));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the card color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CardColor.None" />.
    /// </remarks>
    [Parameter]
    public CardColor Color { get; set; } = CardColor.None;

    /// <summary>
    /// Gets or sets the text alignment of the card.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Alignment.None" />.
    /// </remarks>
    [Parameter]
    public Alignment TextAlignment { get; set; } = Alignment.None;

    #endregion
}
