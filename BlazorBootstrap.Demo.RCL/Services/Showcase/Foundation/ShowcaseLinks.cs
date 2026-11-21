namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>
/// The check every site applies to a link it stores in a notification, search result, or audit entry, so a stored link
/// can only lead to a page of the same site.
/// </summary>
public static class ShowcaseLinks
{
    #region Methods

    /// <summary>
    /// Determines whether <paramref name="url" /> is empty or a path inside the site: under <paramref name="siteBase" />
    /// (the start page itself only when <paramref name="allowSiteBase" /> is <see langword="true" />), at most 200 characters,
    /// and free of <c>//</c>, white space, control characters, quotes, angle brackets, and backslashes.
    /// </summary>
    public static bool IsSiteLink(string? url, string siteBase, bool allowSiteBase = true) =>
        url is null
        || (url.Length <= 200
            && ((allowSiteBase && url == siteBase) || url.StartsWith(siteBase + "/", StringComparison.Ordinal))
            && !url.Contains("//", StringComparison.Ordinal)
            && url.All(character => !char.IsWhiteSpace(character) && !char.IsControl(character) && character is not ('"' or '\'' or '<' or '>' or '\\')));

    #endregion
}
