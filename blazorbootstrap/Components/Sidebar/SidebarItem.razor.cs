namespace BlazorBootstrap;

/// <summary>
/// Represents a sidebar item within a <see cref="Sidebar"/> component.
/// </summary>
public partial class SidebarItem : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool navitemGroupExpanded = false;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (!HasChildren || !(ChildItems?.Any() ?? false))
            return;

        foreach (var childItem in ChildItems)
            if (NavLinkExtensions.ShouldExpand(NavigationManager, childItem.Href!, Match))
            {
                navitemGroupExpanded = true;

                return;
            }
    }

    private void AutoHideNavMenu()
    {
        Parent.HideNavMenuOnMobile();
    }

    private void ToggleNavItemGroup() => navitemGroupExpanded = !navitemGroupExpanded;


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
                case nameof(ChildItems): ChildItems = (IReadOnlyCollection<NavItem>)parameter.Value; break;
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(CollapseSidebar): CollapseSidebar = (bool)parameter.Value; break;
                case nameof(CustomIconName): CustomIconName = (string)parameter.Value; break;
                case nameof(HasChildren): HasChildren = (bool)parameter.Value; break;
                case nameof(Href): Href = (string)parameter.Value; break;
                case nameof(IconColor): 
                    IconColor = (IconColor)parameter.Value;
                    IconColorCssClass = EnumExtensions.IconColorClassMap[IconColor];
                    break;
                case nameof(IconName): IconName = (IconName)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value!; break;
                case nameof(Match): Match = (NavLinkMatch)parameter.Value; break;
                case nameof(Parent): Parent = (Sidebar)parameter.Value!; break;

                case nameof(Target): Target = (Target)parameter.Value; break;
                case nameof(Text): Text = (string)parameter.Value; break;
                
                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
                    break;
            }
        } 

        return base.SetParametersAsync(ParameterView.Empty);
    }


    #endregion

    #region Properties, Indexer

    /// <summary>
    /// Gets or sets the child items.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public IReadOnlyCollection<NavItem>? ChildItems { get; set; }

    /// <summary>
    /// The <see cref="Sidebar"/> sets if this sidebar item should be collapsed.
    /// </summary>
    [CascadingParameter] public bool CollapseSidebar { get; set; }

    /// <summary>
    /// Gets or sets the custom icon name.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string? CustomIconName { get; set; }

    /// <summary>
    /// Gets or sets the has child items state.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool HasChildren { get; set; }

    /// <summary>
    /// Gets or sets the link href attribute.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string? Href { get; set; }

    /// <summary>
    /// Gets or sets the icon color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconColor.None" />.
    /// </remarks>
    [Parameter] public IconColor IconColor { get; set; } = IconColor.None;

    private string IconColorCssClass { get; set; } = "";

    /// <summary>
    /// Gets or sets the icon name.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconName.None" />.
    /// </remarks>
    [Parameter] public IconName IconName { get; set; } = IconName.None;

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
    /// Gets or sets the parent.
    /// </summary>
    [CascadingParameter]
    public Sidebar Parent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the link target.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Target.None" />.
    /// </remarks>
    [Parameter]
    public Target Target { get; set; } = Target.None; 

    /// <summary>
    /// Gets or sets the sidebar item text.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? Text { get; set; }

    #endregion
}
