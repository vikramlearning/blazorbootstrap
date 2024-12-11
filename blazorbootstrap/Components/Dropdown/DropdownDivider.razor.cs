using System.Reflection.Metadata.Ecma335;

namespace BlazorBootstrap;

/// <summary>
/// Renders a divider within a <see cref="Dropdown"/> component, to separate different sections.
/// </summary> 
public partial class DropdownDivider : BlazorBootstrapComponentBase
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
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value!; break;
                default: AdditionalAttributes[parameter.Name] = parameter.Value; break;
            }
        }
        return base.SetParametersAsync(ParameterView.Empty);
    }


    #endregion
}
