namespace BlazorBootstrap;

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
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.7.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [ParameterTypeName("RenderFragment?")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the badge color.
    /// <para>
    /// Default value is <see cref="BadgeColor.Secondary" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.7.0")]
    [DefaultValue(BadgeColor.Secondary)]
    [Description("Gets or sets the badge color.")]
    [Parameter]
    public BadgeColor Color { get; set; } = BadgeColor.Secondary;

    /// <summary>
    /// Gets or sets the badge indicator.
    /// <para>
    /// Default value is <see cref="BadgeIndicatorType.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.7.0")]
    [DefaultValue(BadgeIndicatorType.None)]
    [Description("Gets or sets the badge indicator.")]
    [Parameter]
    public BadgeIndicatorType IndicatorType { get; set; } = BadgeIndicatorType.None;

    /// <summary>
    /// Gets or sets the badge placement.
    /// <para>
    /// Default value is <see cref="BadgePlacement.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.7.0")]
    [DefaultValue(BadgePlacement.None)]
    [Description("Gets or sets the badge placement.")]
    [Parameter]
    public BadgePlacement Placement { get; set; } = BadgePlacement.None;

    /// <summary>
    /// Gets or sets the badge position.
    /// <para>
    /// Default value is <see cref="Position.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.7.0")]
    [DefaultValue(Position.None)]
    [Description("Gets or sets the badge position.")]
    [Parameter]
    public Position Position { get; set; } = Position.None;

    /// <summary>
    /// Gets or sets the visually hidden text.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.7.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the visually hidden text.")]
    [ParameterTypeName("string?")]
    [Parameter]
    public string? VisuallyHiddenText { get; set; }

    #endregion
}
