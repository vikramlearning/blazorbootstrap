namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>
/// Checks the address a sign-in page returns to. Only a relative path inside the same site is accepted, so a crafted
/// <c>?returnUrl=</c> can never send someone to another site or another origin (open redirect).
/// </summary>
public static class ShowcaseReturnUrl
{
    #region Methods

    #region Public Methods

    /// <summary>Returns <paramref name="returnUrl" /> when it is a safe path inside the site, otherwise <see langword="null" />.</summary>
    /// <param name="returnUrl">The requested address, from the query string.</param>
    /// <param name="siteBase">The site's base path, such as <c>/showcase/ecommerce</c>.</param>
    public static string? Resolve(string? returnUrl, string siteBase)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) || returnUrl.Length > 512)
            return null;

        // Paths only: no scheme, no protocol-relative or backslash tricks, no control characters.
        if (!returnUrl.StartsWith('/') || returnUrl.StartsWith("//", StringComparison.Ordinal) || returnUrl.Contains('\\') || returnUrl.Contains("..", StringComparison.Ordinal) || returnUrl.Contains("%2e", StringComparison.OrdinalIgnoreCase) || returnUrl.Any(char.IsControl))
            return null;

        var path = returnUrl.Split('?', '#')[0];

        return string.Equals(path, siteBase, StringComparison.OrdinalIgnoreCase) || path.StartsWith(siteBase + "/", StringComparison.OrdinalIgnoreCase)
            ? returnUrl
            : null;
    }

    #endregion

    #endregion
}
