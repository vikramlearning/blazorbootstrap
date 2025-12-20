namespace BlazorBootstrap;

public partial class Card : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Card, true),
            (TextAlignment.ToTextAlignmentClass(), true),
            (Color.ToCardColorClass(), true));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [ParameterTypeName("RenderFragment?")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the card color.
    /// <para>
    /// Default value is <see cref="CardColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(CardColor.None)]
    [Description("Gets or sets the card color.")]
    [Parameter]
    public CardColor Color { get; set; } = CardColor.None;

    /// <summary>
    /// Gets or sets the text alignment of the card.
    /// <para>
    /// Default value is <see cref="Alignment.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(Alignment.None)]
    [Description("Gets or sets the text alignment of the card.")]
    [Parameter]
    public Alignment TextAlignment { get; set; } = Alignment.None;

    #endregion
}
