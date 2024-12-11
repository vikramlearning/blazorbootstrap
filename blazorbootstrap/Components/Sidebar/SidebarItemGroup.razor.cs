using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;

namespace BlazorBootstrap;

/// <summary>
/// Represents a group of <see cref="SidebarItem"/>s within a <see cref="Sidebar"/> component.
/// </summary>
public partial class SidebarItemGroup : BlazorBootstrapComponentBase
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
                case var _ when String.Equals(parameter.Name, nameof(CollapseSidebar), StringComparison.OrdinalIgnoreCase): CollapseSidebar = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(NavItems), StringComparison.OrdinalIgnoreCase): NavItems = (IReadOnlyCollection<NavItem>)parameter.Value; break;
 

                default:
                    AdditionalAttributes[parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers
      
    [CascadingParameter] public bool CollapseSidebar { get; set; }

    /// <summary>
    /// Gets or sets the nav items.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public IReadOnlyCollection<NavItem>? NavItems { get; set; }

    #endregion
}
