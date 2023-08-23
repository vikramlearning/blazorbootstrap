namespace BlazorBootstrap;

public partial class Callout : BaseComponent
{
    #region Fields and Constants

    private CalloutType type = CalloutType.Default;

    #endregion

    #region Methods

    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(ClassProvider.Callout());
        builder.Append(ClassProvider.ToCalloutType(Type));

        base.BuildClasses(builder);
    }

    private string GetHeading() =>
        string.IsNullOrWhiteSpace(Heading)
            ? type switch { CalloutType.Default => "NOTE", CalloutType.Info => "INFO", CalloutType.Warning => "WARNING", CalloutType.Danger => "DANGER", CalloutType.Tip => "TIP", _ => "" }
            : Heading;

    private IconName GetIconName() => type switch { CalloutType.Default => IconName.InfoCircleFill, CalloutType.Info => IconName.InfoCircleFill, CalloutType.Warning => IconName.ExclamationTriangleFill, CalloutType.Danger => IconName.Fire, CalloutType.Tip => IconName.Lightbulb, _ => IconName.InfoCircleFill };

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    private string CalloutHeadingCSSClass => ClassProvider.CalloutHeading();

    /// <summary>
    /// Specifies the content to be rendered inside this.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string heading => GetHeading();

    /// <summary>
    /// Gets or sets the callout heading.
    /// </summary>
    [Parameter]
    public string? Heading { get; set; }

    private IconName iconName => GetIconName();

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

    #endregion
}