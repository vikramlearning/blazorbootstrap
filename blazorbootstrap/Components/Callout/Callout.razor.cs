namespace BlazorBootstrap;

public partial class Callout : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private CalloutType type = CalloutType.Default;

    #endregion

    #region Methods

    protected override void BuildClasses()
    {
        this.AddClass(BootstrapClassProvider.Callout);
        this.AddClass(BootstrapClassProvider.ToCalloutType(Type));

        base.BuildClasses();
    }

    private string GetHeading()
    {
        if (!string.IsNullOrWhiteSpace(Heading))
            return Heading;

        return type switch
        {
            CalloutType.Default => "NOTE",
            CalloutType.Info => "INFO",
            CalloutType.Warning => "WARNING",
            CalloutType.Danger => "DANGER",
            CalloutType.Tip or CalloutType.Success => "TIP",
            _ => ""
        };
    }

    private IconName GetIconName() =>
        type switch
        {
            CalloutType.Default => IconName.InfoCircleFill,
            CalloutType.Info => IconName.InfoCircleFill,
            CalloutType.Warning => IconName.ExclamationTriangleFill,
            CalloutType.Danger => IconName.Fire,
            CalloutType.Tip or CalloutType.Success => IconName.Lightbulb,
            _ => IconName.InfoCircleFill
        };

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    private string CalloutHeadingCssClass => BootstrapClassProvider.CalloutHeading;

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

    /// <summary>
    /// Gets or sets a value indicating whether to hide the callout heading.
    /// </summary>
    [Parameter]
    public bool HideHeading { get; set; }

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
