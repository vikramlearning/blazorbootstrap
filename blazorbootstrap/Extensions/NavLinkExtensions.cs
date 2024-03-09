namespace BlazorBootstrap;

public static class NavLinkExtensions
{
    private static bool EqualsHrefExactlyOrIfTrailingSlashAdded(string currentUriAbsolute, string hrefAbsolute)
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

    public static bool ShouldExpand(NavigationManager navigationManager, string href, NavLinkMatch match)
    {
        var hrefAbsolute = href is null ? null : navigationManager.ToAbsoluteUri(href).AbsoluteUri;
        return hrefAbsolute is not null
               && (EqualsHrefExactlyOrIfTrailingSlashAdded(navigationManager.Uri, hrefAbsolute)
                   || (match == NavLinkMatch.Prefix && IsStrictlyPrefixWithSeparator(navigationManager.Uri, hrefAbsolute)));
    }

    public static bool ShouldExpand(NavigationManager navigationManager, IEnumerable<NavItem> navItems, NavLinkMatch match, int currentLevel = 0)
    {
        if(currentLevel > 16)
            return false;

        if (navItems?.Any() ?? false)
        {
            foreach (var item in navItems)
            {
                if (ShouldExpand(navigationManager, item.Href!, match))
                    return true;

                if (item?.HasChildItems ?? false)
                    if (ShouldExpand(navigationManager, item.ChildItems!, match, currentLevel + 1))
                        return true;
            }
        }

        return false;
    }
}
