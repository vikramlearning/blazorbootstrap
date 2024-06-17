namespace BlazorBootstrap;

/// <summary>
/// Blazor Bootstrap callout component provides content presentation in a visually distinct manner. <br/> 
/// </summary>
public partial class Callout : BlazorBootstrapComponentBase
{
    #region Methods

    /// <summary>
    /// Parameters are loaded manually for sake of performance.
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/performance#implement-setparametersasync-manually"/>
    /// </summary> 
    public override Task SetParametersAsync(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(ChildContent): ChildContent = (RenderFragment)parameter.Value; break;
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(Color): 
                    Color = (CalloutColor)parameter.Value;
                    IconName = Color switch
                    {
                        CalloutColor.Default => IconName.InfoCircleFill,
                        CalloutColor.Info => IconName.InfoCircleFill,
                        CalloutColor.Warning => IconName.ExclamationTriangleFill,
                        CalloutColor.Danger => IconName.Fire,
                        CalloutColor.Success => IconName.Lightbulb,
                        _ => IconName.InfoCircleFill
                    };
                    break;
                case nameof(Heading): Heading = (string)parameter.Value; break;
                case nameof(HideHeading): HideHeading = (bool)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value; break;
                case nameof(Style): Style = (string)parameter.Value; break;
                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
                    break;
            }
        }

        if (String.IsNullOrWhiteSpace(Heading))
        {
            Heading = Color switch
            {
                CalloutColor.Default => "NOTE",
                CalloutColor.Info => "INFO",
                CalloutColor.Warning => "WARNING",
                CalloutColor.Danger => "DANGER",
                CalloutColor.Success => "TIP",
                _ => ""
            };
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

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

    private IconName IconName { get; set; } = IconName.InfoCircleFill;

    #endregion
}
