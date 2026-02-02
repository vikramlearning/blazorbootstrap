namespace BlazorBootstrap;

public partial class CardHeader : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.CardHeader, true),
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
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the card header color.
    /// <para>
    /// Default value is <see cref="CardColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(CardColor.None)]
    [Description("Gets or sets the card header color.")]
    [Parameter]
    public CardColor Color { get; set; } = CardColor.None;

    #endregion
}
