namespace BlazorBootstrap;

public partial class Sidebar2Item : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool navItemGroupExpanded = false;

    /// <summary>
    /// Get nav link style.
    /// Implementation:
    /// Level 0 = 1rem    = 0 + 1 + (0 * 0.5)
    /// Level 1 = 2.5rem  = 1 + 1 + (1 * 0.5)
    /// Level 2 = 4rem    = 2 + 1 + (2 * 0.5)
    /// ...
    /// Level n = .....   = n + 1 + (n * 0.5)
    /// </summary>
    private string navLinkStyle => $"padding-left:{Level + 1 + (Level * 0.5)}rem;";

    #endregion

    #region Methods

    protected override void BuildClasses()
    {
        this.AddClass("nav-item");
        this.AddClass($"nav-item-level-{Level}");
        this.AddClass("nav-item-group", HasChilds);
        this.AddClass("active", navItemGroupExpanded);

        base.BuildClasses();
    }

    protected override void OnParametersSet()
    {
        if (!HasChilds || !(ChildItems?.Any() ?? false))
            return;

        foreach (var childItem in ChildItems)
            if (NavLinkExtensions.ShouldExpand(NavigationManager, childItem.Href!, Match))
            {
                navItemGroupExpanded = true;

                Console.WriteLine($"{Text} - navItemGroupExpanded: {navItemGroupExpanded}");

                // Only on after render
                //if (Rendered && navItemGroupExpanded && OnNavItemGroupExpanded is not null)
                if (navItemGroupExpanded && OnNavItemGroupExpanded is not null)
                {
                    OnNavItemGroupExpanded?.Invoke(true);
                }

                return;
            }
    }

    private void AutoHideNavMenu()
    {
        Root.HideNavMenuOnMobile();
    }

    private void ToggleNavItemGroup() => navItemGroupExpanded = !navItemGroupExpanded;

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

    [Parameter] public int Level { get; set; } = 0;

    /// <summary>
    /// Gets or sets a value representing the URL matching behavior.
    /// </summary>
    [Parameter]
    public NavLinkMatch Match { get; set; }

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [CascadingParameter] public Sidebar2 Root { get; set; } = default!;

    [Parameter] public Target Target { get; set; }

    private string targetString => Target.ToTargetString()!;

    [Parameter] public string? Text { get; set; }

    [Parameter] public Action<bool> OnNavItemGroupExpanded { get; set; } = default!;

    private void HandleNavItemGroupExpanded(bool expanded)
    {
        Console.WriteLine($"{Level}: {Text}");
    }

    #endregion
}
