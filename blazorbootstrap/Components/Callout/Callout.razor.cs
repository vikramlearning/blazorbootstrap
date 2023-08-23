namespace BlazorBootstrap;

public partial class Callout : BaseComponent
{
    #region Members

    private CalloutType type = CalloutType.Default;

    private string CalloutHeadingCSSClass => ClassProvider.CalloutHeading();

    private string heading => GetHeading();

    private IconName iconName => GetIconName();

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(ClassProvider.Callout());
        builder.Append(ClassProvider.ToCalloutType(Type));

        base.BuildClasses(builder);
    }

    private string GetHeading() => string.IsNullOrWhiteSpace(Heading)
            ? type switch
            {
                CalloutType.Default => "NOTE",
                CalloutType.Info => "INFO",
                CalloutType.Warning => "WARNING",
                CalloutType.Danger => "DANGER",
                CalloutType.Tip => "TIP",
                _ => "",
            }
            : Heading;

    private IconName GetIconName() => type switch
    {
        CalloutType.Default => IconName.InfoCircleFill,
        CalloutType.Info => IconName.InfoCircleFill,
        CalloutType.Warning => IconName.ExclamationTriangleFill,
        CalloutType.Danger => IconName.Fire,
        CalloutType.Tip => IconName.Lightbulb,
        _ => IconName.InfoCircleFill,
    };

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside this.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the callout heading.
    /// </summary>
    [Parameter] public string? Heading { get; set; }

    /// <summary>
    /// Gets or sets the callout color.
    /// </summary>
    [Parameter]
    public CalloutType Type
    {
        get => type;
        set
        {
            type = value;
            DirtyClasses();
        }
    }

    #endregion Properties
}
