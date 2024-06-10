namespace BlazorBootstrap;

/// <summary>
/// Blazor Bootstrap callout component provides content presentation in a visually distinct manner. <br/> 
/// </summary>
public partial class Callout : BlazorBootstrapComponentBase
{
    #region Methods

    private string GetHeading()
    {
        if (!string.IsNullOrWhiteSpace(Heading))
            return Heading;

        return Color switch
               {
                   CalloutColor.Default => "NOTE",
                   CalloutColor.Info => "INFO",
                   CalloutColor.Warning => "WARNING",
                   CalloutColor.Danger => "DANGER",
                   CalloutColor.Success => "TIP",
                   _ => ""
               };
    }

    private IconName GetIconName() =>
        Color switch
        {
            CalloutColor.Default => IconName.InfoCircleFill,
            CalloutColor.Info => IconName.InfoCircleFill,
            CalloutColor.Warning => IconName.ExclamationTriangleFill,
            CalloutColor.Danger => IconName.Fire,
            CalloutColor.Success => IconName.Lightbulb,
            _ => IconName.InfoCircleFill
        };

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Callout, true),
            (Color.ToCalloutColorClass(), true));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the callout color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CalloutColor.Default" />.
    /// </remarks>
    [Parameter]
    public CalloutColor Color { get; set; } = CalloutColor.Default;

    /// <summary>
    /// Gets or sets the callout heading.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? Heading { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to hide the callout heading.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool HideHeading { get; set; }

    #endregion
}
