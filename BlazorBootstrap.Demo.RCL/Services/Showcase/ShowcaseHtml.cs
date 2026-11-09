using System.Net;
using System.Text.RegularExpressions;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase;

/// <summary>
/// Turns user-entered rich text into markup that is safe to render.
/// </summary>
/// <remarks>
/// The whole value is HTML-encoded first, and only exact allow-listed tags are revived afterwards,
/// with every attribute dropped. Links keep only an <c>http</c>, <c>https</c>, or <c>mailto</c>
/// <c>href</c>. Anything else stays visible as plain text.
/// </remarks>
public static partial class ShowcaseHtml
{
    #region Methods

    #region Public Methods

    /// <summary>
    /// Encodes <paramref name="html" /> and revives only the allow-listed formatting tags.
    /// </summary>
    public static MarkupString ToSafeMarkup(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return new MarkupString(string.Empty);

        var encoded = WebUtility.HtmlEncode(html);

        encoded = AllowedLinkOpenTag().Replace(encoded, match => $"<a href=\"{match.Groups["href"].Value}\" target=\"_blank\" rel=\"noopener noreferrer nofollow\">");
        encoded = LinkCloseTag().Replace(encoded, "</a>");
        encoded = AllowedFormattingTag().Replace(encoded, match => $"<{match.Groups["close"].Value}{match.Groups["tag"].Value.ToLowerInvariant()}>");

        return new MarkupString(encoded);
    }

    #endregion

    #region Private Methods

    [GeneratedRegex(@"&lt;a\b(?:(?!&gt;).)*?\bhref=&quot;(?<href>(?:https?://|mailto:)(?:(?!&quot;)[^\s<>])*)&quot;(?:(?!&gt;).)*&gt;", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex AllowedLinkOpenTag();

    [GeneratedRegex(@"&lt;(?<close>/?)(?<tag>p|br|b|strong|i|em|u|s|ul|ol|li|blockquote|code|pre|h[1-6])\b(?:(?!&gt;).)*&gt;", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex AllowedFormattingTag();

    [GeneratedRegex(@"&lt;/a\s*&gt;", RegexOptions.IgnoreCase)]
    private static partial Regex LinkCloseTag();

    #endregion

    #endregion
}
