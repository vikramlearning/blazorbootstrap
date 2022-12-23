namespace BlazorBootstrap;

public partial class SidebarItem : BaseComponent
{
    #region Events

    private bool expanded = false;

    private string navLinkCssClass => (HasChilds & expanded) ? "nav-link link-dark fw-semibold" : "nav-link link-dark";

    private string liCssClass => (HasChilds & expanded) ? "nav-item nav-group expanded" : "nav-item";

    #endregion Events

    #region Members

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        base.BuildClasses(builder);
    }

    protected override async Task OnInitializedAsync()
    {
        Attributes ??= new Dictionary<string, object>();

        await base.OnInitializedAsync();
    }

    protected override void OnParametersSet()
    {
        if (!HasChilds || ChildItems is null || !ChildItems.Any()) return;

        foreach (var childItem in ChildItems)
        {
            Console.WriteLine($"Uri: {NavigationManager.Uri}, Href: {childItem.Href}");
            if (ShouldExpand(NavigationManager.Uri, childItem.Href))
            {
                expanded = true;
                return;
            }
        }
    }

    private bool ShouldExpand(string currentUriAbsolute, string href)
    {
        var hrefAbsolute = (href == null) ? null : NavigationManager.ToAbsoluteUri(href).AbsoluteUri;

        if (hrefAbsolute == null)
        {
            return false;
        }

        if (EqualsHrefExactlyOrIfTrailingSlashAdded(currentUriAbsolute, hrefAbsolute))
        {
            return true;
        }

        if (Match == NavLinkMatch.Prefix && IsStrictlyPrefixWithSeparator(currentUriAbsolute, hrefAbsolute))
        {
            return true;
        }

        return false;
    }

    private bool EqualsHrefExactlyOrIfTrailingSlashAdded(string currentUriAbsolute, string hrefAbsolute)
    {
        if (string.Equals(currentUriAbsolute, hrefAbsolute, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (currentUriAbsolute.Length == hrefAbsolute.Length - 1)
        {
            // Special case: highlight links to http://host/path/ even if you're
            // at http://host/path (with no trailing slash)
            //
            // This is because the router accepts an absolute URI value of "same
            // as base URI but without trailing slash" as equivalent to "base URI",
            // which in turn is because it's common for servers to return the same page
            // for http://host/vdir as they do for host://host/vdir/ as it's no
            // good to display a blank page in that case.
            if (hrefAbsolute[hrefAbsolute.Length - 1] == '/'
                && hrefAbsolute.StartsWith(currentUriAbsolute, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsStrictlyPrefixWithSeparator(string value, string prefix)
    {
        var prefixLength = prefix.Length;
        if (value.Length > prefixLength)
        {
            return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
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
        else
        {
            return false;
        }
    }

    private void OnNavLinkClick()
    {
        expanded = !expanded;
    }

    #endregion Methods

    #region Properties

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [Parameter] public IReadOnlyList<NavItem>? ChildItems { get; set; }

    [Parameter] public bool HasChilds { get; set; }

    [Parameter] public string Href { get; set; }

    /// <summary>
    /// Gets or sets a value representing the URL matching behavior.
    /// </summary>
    [Parameter] public NavLinkMatch Match { get; set; }

    [Parameter] public IconName PrefixIconName { get; set; }

    [Parameter] public string Text { get; set; }

    // TODO: add target support

    #endregion Properties
}

public class NavItem
{
    public string Id { get; set; }
    public IReadOnlyList<NavItem>? ChildItems { get; set; }
    public bool HasChilds { get; set; }
    public string Href { get; set; }
    public NavLinkMatch Match { get; set; }
    public string ParentId { get; set; }
    public IconName PrefixIconName { get; set; }
    public int Sequence { get; set; }
    public string Text { get; set; }
}
