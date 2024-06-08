﻿namespace BlazorBootstrap;

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

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Callout, true),
            (Color.ToCalloutColorClass(), true));

    private string CalloutHeadingCssClass => BootstrapClass.CalloutHeading;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is null.
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

    private string heading => GetHeading();

    /// <summary>
    /// Gets or sets the callout heading.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string? Heading { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to hide the callout heading.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool HideHeading { get; set; }

    private IconName iconName => GetIconName();

    #endregion
}
