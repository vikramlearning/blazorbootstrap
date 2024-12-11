namespace BlazorBootstrap;

/// <summary>
/// Represents a group of <see cref="Sidebar2Item"/>s within a <see cref="Sidebar2"/> component.
/// </summary>
public partial class Sidebar2ItemGroup : BlazorBootstrapComponentBase
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
                case nameof(CollapseSidebar): CollapseSidebar = (bool)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value!; break;
                 case nameof(Match): Match = (NavLinkMatch)parameter.Value; break;
                case nameof(NavItems): NavItems = (IReadOnlyCollection<NavItem>)parameter.Value; break;
 

                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion


    #region Properties, Indexers 

    [CascadingParameter] public bool CollapseSidebar { get; set; }

    /// <summary>
    /// Gets or sets a value representing the URL matching behavior.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="NavLinkMatch.Prefix" />.
    /// </remarks>
    [Parameter]
    public NavLinkMatch Match { get; set; } = NavLinkMatch.Prefix;

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// Gets or sets the nav items.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public IReadOnlyCollection<NavItem>? NavItems { get; set; }

    #endregion
}
