namespace BlazorBootstrap;

public partial class Badge : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

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
    /// Gets or sets the badge color.
    /// </summary>
    [Parameter]
    //[EditorRequired]
    public BadgeColor Color { get; set; } = BadgeColor.Secondary;

    /// <summary>
    /// Gets or sets the badge indicator.
    /// </summary>
    [Parameter]
    public BadgeIndicatorType IndicatorType { get; set; } = BadgeIndicatorType.None;

    /// <summary>
    /// Gets or sets the badge placement.
    /// </summary>
    [Parameter]
    public BadgePlacement Placement { get; set; } = BadgePlacement.None;

    /// <summary>
    /// Gets or sets the badge position.
    /// </summary>
    [Parameter]
    public Position Position { get; set; }

    /// <summary>
    /// Gets or sets the visually hidden text.
    /// </summary>
    [Parameter]
    public string VisuallyHiddenText { get; set; } = default!;

    #endregion
}
