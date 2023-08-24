namespace BlazorBootstrap;

public partial class Badge
{
    #region Fields and Constants

    private RenderFragment? childContent;

    private BadgeColor color = BadgeColor.Secondary;

    private BadgeIndicatorType indicatorType = BadgeIndicatorType.None;

    private BadgePlacement placement = BadgePlacement.None;

    private Position position;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Badge());
        builder.Append(BootstrapClassProvider.BadgeColor(Color), Color != BadgeColor.None);
        builder.Append(BootstrapClassProvider.ToBadgeIndicator(IndicatorType), IndicatorType != BadgeIndicatorType.None);
        builder.Append(BootstrapClassProvider.ToPosition(Position), Position != Position.None);
        builder.Append(BootstrapClassProvider.ToBadgePlacement(Placement), Placement != BadgePlacement.None);
        builder.Append("p-2", childContent is null);

        base.BuildClasses(builder);
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="Badge" />.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent
    {
        get => childContent;
        set
        {
            childContent = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Gets or sets the badge color.
    /// </summary>
    [Parameter]
    [EditorRequired]
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
    /// Gets or sets the badge position.
    /// </summary>
    [Parameter]
    public Position Position
    {
        get => position;
        set
        {
            position = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Gets or sets the visually hidden text.
    /// </summary>
    [Parameter]
    public string VisuallyHiddenText { get; set; } = default!;

    #endregion
}