using static System.Net.Mime.MediaTypeNames;

namespace BlazorBootstrap;

/// <summary>
/// Use Blazor Bootstrap loading placeholders for your components or pages to indicate something may still be loading.
/// </summary>
public partial class Placeholder : BlazorBootstrapComponentBase
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
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Color), StringComparison.OrdinalIgnoreCase): Color = (PlaceholderColor)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Size), StringComparison.OrdinalIgnoreCase): Size = (PlaceholderSize)parameter.Value; break;
                
                case var _ when String.Equals(parameter.Name, nameof(Width), StringComparison.OrdinalIgnoreCase): Width = (PlaceholderWidth)parameter.Value; break;

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
    /// Gets or sets the placeholder color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="PlaceholderColor.None" />.
    /// </remarks>
    [Parameter] public PlaceholderColor Color { get; set; } = PlaceholderColor.None;

    /// <summary>
    /// Gets or sets the placeholder size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="PlaceholderSize.None" />.
    /// </remarks>
    [Parameter] public PlaceholderSize Size { get; set; } = PlaceholderSize.None;

    /// <summary>
    /// Gets or sets the placeholder width.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="PlaceholderWidth.Col1" />.
    /// </remarks>
    [Parameter] public PlaceholderWidth Width { get; set; } = PlaceholderWidth.Col1;

    #endregion
}
