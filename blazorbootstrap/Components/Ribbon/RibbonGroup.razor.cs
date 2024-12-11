namespace BlazorBootstrap;

/// <summary>
/// Represents a group of <see cref="RibbonTab"/>'s within a <see cref="Ribbon"/> component.
/// </summary>
public partial class RibbonGroup : BlazorBootstrapComponentBase
{
    #region Properties, Indexers
     
    /// <summary>
    /// Gets or sets the content to be rendered inside this component.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

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
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(ChildContent), StringComparison.OrdinalIgnoreCase):  ChildContent = (RenderFragment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                
                default:
                    AdditionalAttributes[parameter.Name] = parameter.Value;
                    break;
            }
        } 
        return base.SetParametersAsync(ParameterView.Empty);
    }
    
    #endregion
}
