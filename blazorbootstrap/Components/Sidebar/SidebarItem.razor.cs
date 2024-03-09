namespace BlazorBootstrap;

public partial class SidebarItem : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool navitemGroupExpanded = false;

    #endregion

    #region Methods

    protected override void BuildClasses()
    {
        this.AddClass("nav-item");
        this.AddClass("nav-item-group", HasChilds);
        this.AddClass("active", navitemGroupExpanded);

        base.BuildClasses();
    }

    protected override void OnParametersSet()
    {
        if (!HasChilds || !(ChildItems?.Any() ?? false))
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

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    [Parameter] public IEnumerable<NavItem>? ChildItems { get; set; }

    [CascadingParameter] public bool CollapseSidebar { get; set; }

    [Parameter] public string? CustomIconName { get; set; }

    [Parameter] public bool HasChilds { get; set; }

    [Parameter] public string? Href { get; set; }

    [Parameter] public IconColor IconColor { get; set; }

    private string iconColorCssClass => BootstrapClassProvider.IconColor(IconColor);

    [Parameter] public IconName IconName { get; set; }

    /// <summary>
    /// Gets or sets a value representing the URL matching behavior.
    /// </summary>
    [Parameter]
    public NavLinkMatch Match { get; set; }

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [CascadingParameter] public Sidebar Parent { get; set; } = default!;

    [Parameter] public Target Target { get; set; }

    private string targetString => Target.ToTargetString()!;

    [Parameter] public string? Text { get; set; }

    #endregion
}
