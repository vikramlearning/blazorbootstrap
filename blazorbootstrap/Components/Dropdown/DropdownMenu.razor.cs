namespace BlazorBootstrap;

/// <summary>
/// Represents the menu section in a <see cref="Dropdown"/> component.
/// </summary>
public partial class DropdownMenu : BlazorBootstrapComponentBase
{
    #region Properties, Indexers
     
    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }  

    /// <summary>
    /// Gets or sets the dropdown menu position.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="DropdownMenuPosition.Start" />.
    /// </remarks>
    [Parameter] public DropdownMenuPosition Position { get; set; } = DropdownMenuPosition.Start;

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
                case var _ when String.Equals(parameter.Name, nameof(ChildContent), StringComparison.OrdinalIgnoreCase): ChildContent = (RenderFragment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Position), StringComparison.OrdinalIgnoreCase): Position = (DropdownMenuPosition)parameter.Value; break;
                default: AdditionalAttributes[parameter.Name] = parameter.Value; break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }
    
    #endregion
}
