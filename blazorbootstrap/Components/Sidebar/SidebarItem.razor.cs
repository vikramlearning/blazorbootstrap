namespace BlazorBootstrap;

public partial class SidebarItem : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool navitemGroupExpanded = false;

    #endregion

    #region Methods

    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append("nav-item");
        builder.Append("nav-item-group", Item.ChildItems.Any());
        builder.Append("nav-sub-item", IsSubItem);
        builder.Append("active", navitemGroupExpanded);

        base.BuildClasses(builder);
    }

    protected override void OnParametersSet()
    {
        if (!Item.ChildItems.Any())
            return;

        foreach (NavItem childItem in Item.ChildItems)
            if (ShouldExpand(NavigationManager.Uri, childItem.Href))
            {
                navitemGroupExpanded = true;

                return;
            }
    }

    private void AutoHideNavMenu()
    {
        Parent.HideNavMenuOnMobile();
    }

    private bool EqualsHrefExactlyOrIfTrailingSlashAdded(string currentUriAbsolute, string hrefAbsolute)
    {
        if (string.Equals(currentUriAbsolute, hrefAbsolute, StringComparison.OrdinalIgnoreCase)) return true;

        if (currentUriAbsolute.Length == hrefAbsolute.Length - 1)
            // Special case: highlight links to http://host/path/ even if you're
            // at http://host/path (with no trailing slash)
            //
            // This is because the router accepts an absolute URI value of "same
            // as base URI but without trailing slash" as equivalent to "base URI",
            // which in turn is because it's common for servers to return the same page
            // for http://host/vdir as they do for host://host/vdir/ as it's no
            // good to display a blank page in that case.
            if (hrefAbsolute[^1] == '/'
                && hrefAbsolute.StartsWith(currentUriAbsolute, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }

    private static bool IsStrictlyPrefixWithSeparator(string value, string prefix)
    {
        var prefixLength = prefix.Length;

        return value.Length > prefixLength
               && value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
               && (
                      // Only match when there's a separator character either at the end of the
                      // prefix or right after it.
                      // Example: "/abc" is treated as a prefix of "/abc/def" but not "/abcdef"
                      // Example: "/abc/" is treated as a prefix of "/abc/def" but not "/abcdef"
                      prefixLength == 0
                      || !char.IsLetterOrDigit(prefix[prefixLength - 1])
                      || !char.IsLetterOrDigit(value[prefixLength])
                  );
    }

    private bool ShouldExpand(string currentUriAbsolute, string? href)
    {
        var hrefAbsolute = href == null ? null : NavigationManager.ToAbsoluteUri(href).AbsoluteUri;

        return hrefAbsolute != null
               && (EqualsHrefExactlyOrIfTrailingSlashAdded(currentUriAbsolute, hrefAbsolute)
                   || (Match == NavLinkMatch.Prefix && IsStrictlyPrefixWithSeparator(currentUriAbsolute, hrefAbsolute)));
    }

    private void ToggleNavItemGroup() => navitemGroupExpanded = !navitemGroupExpanded;

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    private string iconColorCssClass => BootstrapClassProvider.IconColor(Item.IconColor);

    [CascadingParameter] public bool CollapseSidebar { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="NavItem"/> which should be displayed.
    /// </summary>
    [Parameter, EditorRequired]
    public NavItem Item { get; set; } = new();

    /// <summary>
    /// If set to true then this nav item is marked as nav-sub-item
    /// </summary>
    [Parameter]
    public bool IsSubItem { get; set; }

    /// <summary>
    /// Gets or sets the current level of this <see cref="NavItem"/>
    /// </summary>
    [Parameter] 
    public int SubLevel { get; set; }

    /// <summary>
    /// Gets or sets a value representing the URL matching behavior.
    /// </summary>
    [Parameter]
    public NavLinkMatch Match { get; set; }

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [CascadingParameter] public Sidebar Parent { get; set; } = default!;

    private string targetString => Item.Target.ToTargetString();

   

    #endregion
}
