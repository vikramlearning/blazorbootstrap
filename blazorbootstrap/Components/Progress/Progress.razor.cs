namespace BlazorBootstrap;

/// <summary>
/// Documentation and examples for using the Blazor Bootstrap progress component featuring support for stacked bars, animated backgrounds, and text labels. <br/>
/// For more information, visit the <see href="https://getbootstrap.com/docs/5.0/components/progress/">Bootstrap Progress</see> documentation.
/// </summary>
public partial class Progress : BlazorBootstrapComponentBase
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
                case var _ when String.Equals(parameter.Name, nameof(Height), StringComparison.OrdinalIgnoreCase): Height = (double)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case "style": Style = (string)parameter.Value; break;

                default:
                    AdditionalAttributes[parameter.Name] = parameter.Value;
                    break;
            }
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
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the height of the Progress. Height is measured in pixels.
    /// </summary>
    /// <remarks>
    /// Default value is 16.
    /// </remarks>
    [Parameter] public double Height { get; set; } = 16;
    private string Style { get; set; } = "";
    
    #endregion
}
