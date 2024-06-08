﻿namespace BlazorBootstrap;

public partial class Badge : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Badge, true),
            (Color.ToBadgeColorClass(), Color != BadgeColor.None),
            (IndicatorType.ToBadgeIndicatorClass(), IndicatorType != BadgeIndicatorType.None),
            (Position.ToPositionClass(), Position != Position.None),
            (Placement.ToBadgePlacementClass(), Placement != BadgePlacement.None),
            ("p-2", ChildContent is null));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is null.
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
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string VisuallyHiddenText { get; set; } = default!;

    #endregion
}
