namespace BlazorBootstrap;

/// <summary>
/// This component represents the header of a <see cref="Card"/>. <br/>
/// If no header is required, it can be omitted from the card implementation.
/// </summary>
public partial class CardHeader : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.CardHeader, true),
            (Color.ToCardColorClass(), true));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the card header color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CardColor.None" />.
    /// </remarks>
    [Parameter]
    public CardColor Color { get; set; } = CardColor.None;

    #endregion
}
