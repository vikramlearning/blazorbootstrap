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
                case var _ when String.Equals(parameter.Name, nameof(ChildContent), StringComparison.OrdinalIgnoreCase): ChildContent = (RenderFragment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Color), StringComparison.OrdinalIgnoreCase): 
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
                case var _ when String.Equals(parameter.Name, nameof(Heading), StringComparison.OrdinalIgnoreCase): Heading = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(HideHeading), StringComparison.OrdinalIgnoreCase): HideHeading = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                default:
                    AdditionalAttributes[parameter.Name] = parameter.Value;
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
