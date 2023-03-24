namespace BlazorBootstrap;

public partial class Badge
{
    #region Events

    #endregion

    #region Members

    private BadgeColor color = BadgeColor.Secondary;

    private BadgePlacement placement = BadgePlacement.None;

    private BadgeIndicatorType indicatorType = BadgeIndicatorType.None;

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Badge());
        builder.Append(BootstrapClassProvider.BadgeColor(color), color != BadgeColor.None);
        builder.Append(BootstrapClassProvider.ToBadgePlacement(placement), placement != BadgePlacement.None);
        builder.Append(BootstrapClassProvider.ToBadgeIndicator(indicatorType), IndicatorType != BadgeIndicatorType.None);

        base.BuildClasses(builder);
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets the badge color.
    /// </summary>
    [Parameter]
    public BadgeColor Color
    {
        get => color;
        set
        {
            color = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Gets or sets the badge placement.
    /// </summary>
    [Parameter]
    public BadgePlacement Placement
    {
        get => placement;
        set
        {
            placement = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Gets or sets the badge indicator.
    /// </summary>
    [Parameter]
    public BadgeIndicatorType IndicatorType
    {
        get => indicatorType;
        set
        {
            indicatorType = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="Badge"/>.
    /// </summary>
    [Parameter] public RenderFragment ChildContent { get; set; } = default!;

    #endregion
}
