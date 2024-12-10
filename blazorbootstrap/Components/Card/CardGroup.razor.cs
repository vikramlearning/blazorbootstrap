namespace BlazorBootstrap;

/// <summary>
/// Represents a Bootstrap card group.
/// For more information, see <see href="https://getbootstrap.com/docs/5.0/components/card/#card-groups">Bootstrap Card Groups</see>.
/// </summary>
public partial class CardGroup : BlazorBootstrapComponentBase
{
    #region Properties, Indexers
     
    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }  

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
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(ChildContent): ChildContent = (RenderFragment)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value; break;
                
                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }
    
    #endregion
}
