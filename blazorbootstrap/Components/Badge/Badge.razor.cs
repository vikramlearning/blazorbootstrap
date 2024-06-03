namespace BlazorBootstrap;

/// <summary>
/// The Blazor Bootstrap Badge component shows the small count and labels. <br/>
/// See <see href="https://getbootstrap.com/docs/5.0/components/badge/">Bootstrap Badge</see> for more information.
/// </summary>
public partial class Badge : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.Badge)
            .AddClass(Color.ToBadgeColorClass(), Color != BadgeColor.None)
            .AddClass(IndicatorType.ToBadgeIndicatorClass(), IndicatorType != BadgeIndicatorType.None)
            .AddClass(Position.ToPositionClass(), Position != Position.None)
            .AddClass(Placement.ToBadgePlacementClass(), Placement != BadgePlacement.None)
            .AddClass("p-2", ChildContent is null)
            .Build();

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the badge color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="BadgeColor.Secondary" />.
    /// </remarks>
    [Parameter]
    public BadgeColor Color { get; set; } = BadgeColor.Secondary;

    /// <summary>
    /// Gets or sets the badge indicator.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="BadgeIndicatorType.None" />.
    /// </remarks>
    [Parameter]
    public BadgeIndicatorType IndicatorType { get; set; } = BadgeIndicatorType.None;

    /// <summary>
    /// Gets or sets the badge placement.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="BadgePlacement.None" />.
    /// </remarks>
    [Parameter]
    public BadgePlacement Placement { get; set; } = BadgePlacement.None;

    /// <summary>
    /// Gets or sets the badge position.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Position.None" />.
    /// </remarks>
    [Parameter]
    public Position Position { get; set; } = Position.None;

    /// <summary>
    /// Gets or sets the visually hidden text.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string VisuallyHiddenText { get; set; } = default!;

    #endregion
}
