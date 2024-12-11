namespace BlazorBootstrap;

/// <summary>
/// In this version of the Blazor Bootstrap Placeholders, you can add animated content to the placeholder.
/// </summary>
public partial class PlaceholderContainer : BlazorBootstrapComponentBase
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
                case nameof(Animation): Animation = (PlaceholderAnimation)parameter.Value; break;
                case nameof(ChildContent): ChildContent = (RenderFragment)parameter.Value; break;
                case nameof(Class): Class = (string)parameter.Value!; break;
                case nameof(Id): Id = (string)parameter.Value!; break;
                
                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers
     
    /// <summary>
    /// Gets or sets the placeholder animation.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="PlaceholderAnimation.Glow" />.
    /// </remarks>
    [Parameter] public PlaceholderAnimation Animation { get; set; } = PlaceholderAnimation.Glow;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion
}
