namespace BlazorBootstrap;

public partial class Sidebar2Item : BlazorBootstrapComponentBase
{
    #region Methods

    protected override void OnInitialized()
    {
        if (NavLinkExtensions.ShouldExpand(NavigationManager, ChildItems!, Match))
            NavItemGroupExpanded = true;

        base.OnInitialized();
    }

    private void AutoHideNavMenu()
    {
        Root.HideNavMenuOnMobile();
    }

    private string GetNavLinkStyle()
    {
        // Implementation:
        // Level 0 = 1rem    = 0 + 1 + (0 * 0.5)
        // Level 1 = 2.5rem  = 1 + 1 + (1 * 0.5)
        // Level 2 = 4rem    = 2 + 1 + (2 * 0.5)
        // ...
        // Level n = .....   = n + 1 + (n * 0.5)

        var level = Level + 1 + Level * 0.5;

        if (HasChilds && !NavItemGroupExpanded)
            level += 0.25;
        else if (!HasChilds && Level == 0)
            level += 0.25;

        return $"padding-left:{level}rem;";
    }

    private void ToggleNavItemGroup() => NavItemGroupExpanded = !NavItemGroupExpanded;

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass("nav-item")
            .AddClass($"nav-item-level-{Level}")
            .AddClass("nav-item-group", HasChilds)
            .AddClass("active", NavItemGroupExpanded)
            .Build();

    /// <summary>
    /// Gets or sets the child items.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public IEnumerable<NavItem>? ChildItems { get; set; }

    [CascadingParameter] public bool CollapseSidebar { get; set; }

    /// <summary>
    /// Gets or sets the custom icon name.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string? CustomIconName { get; set; }

    /// <summary>
    /// Gets or sets the has child items state.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool HasChilds { get; set; }

    /// <summary>
    /// Gets or sets the link href attribute.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// Gets or sets the icon color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconColor.None" />.
    /// </remarks>
    [Parameter]
    public IconColor IconColor { get; set; }

    private string iconColorCssClass => IconColor.ToIconColorClass();

    /// <summary>
    /// Gets or sets the icon name.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconName.None" />.
    /// </remarks>
    [Parameter]
    public IconName IconName { get; set; }

    /// <summary>
    /// Gets or sets the nested level.
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    [Parameter]
    public int Level { get; set; } = 0;

    /// <summary>
    /// Gets or sets a value representing the URL matching behavior.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="NavLinkMatch.Prefix" />.
    /// </remarks>
    [Parameter]
    public NavLinkMatch Match { get; set; } = NavLinkMatch.Prefix;

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter] public bool NavItemGroupExpanded { get; set; } = false;

    /// <summary>
    /// Get nav link style.
    /// </summary>
    private string navLinkStyle => GetNavLinkStyle();

    [Parameter] public Action<bool> OnNavItemGroupExpanded { get; set; } = default!;

    /// <summary>
    /// Gets or sets the root.
    /// </summary>
    [CascadingParameter]
    public Sidebar2 Root { get; set; } = default!;

    /// <summary>
    /// Gets or sets the link target.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Target.None" />.
    /// </remarks>
    [Parameter]
    public Target Target { get; set; } = Target.None;

    private string targetString => Target.ToTargetString()!;

    /// <summary>
    /// Gets or sets the sidebar item text.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string? Text { get; set; }

    #endregion
}
