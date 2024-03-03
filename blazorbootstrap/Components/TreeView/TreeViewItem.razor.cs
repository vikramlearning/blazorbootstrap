namespace BlazorBootstrap;

public partial class TreeViewItem : BlazorBootstrapComponentBase
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
            if (ShouldExpand(NavigationManager.Uri, childItem.Href!))
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

    private bool ShouldExpand(string currentUriAbsolute, string href)
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
