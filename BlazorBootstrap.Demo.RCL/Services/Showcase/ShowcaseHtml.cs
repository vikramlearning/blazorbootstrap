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

    /// <summary>
    /// Renders HTML read back from the library's <c>RichTextEditor</c>, in which typed text is already entity-encoded.
    /// First the editor's real markup is normalized: comments (the editor's value carries Blazor's
    /// <c>&lt;!--!--&gt;</c> render markers) and <c>script</c> / <c>style</c> blocks are removed; <c>div</c> lines and
    /// headings become paragraphs; underline and line-through spans become <c>u</c> and <c>s</c>; other tags that are not
    /// on the allowlist (pasted spans, images, tables) are dropped and their text kept. Then the entities are decoded once
    /// and the result goes through <see cref="ToSafeMarkup" />, which encodes everything again and revives only the
    /// allow-listed tags. So typed text such as <c>&lt;img onerror=…&gt;</c> shows as written, while typed or applied
    /// allow-listed formatting (for example <c>&lt;b&gt;</c>) is kept.
    /// </summary>
    public static MarkupString FromEditorHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return new MarkupString(string.Empty);

        // Real tags only: typed text is still entity-encoded here, so a "<span>" or "<div>" the user typed is not touched.
        var cleaned = HtmlComment().Replace(html, string.Empty);
        cleaned = ScriptOrStyleBlock().Replace(cleaned, string.Empty);
        cleaned = NormalizeEditorTags(cleaned);

        return ToSafeMarkup(WebUtility.HtmlDecode(cleaned));
    }

    #endregion

    #region Private Methods

    [GeneratedRegex(@"&lt;a\b(?:(?!&gt;).)*?\bhref=&quot;(?<href>(?:https?://|mailto:)(?:(?!&quot;)[^\s<>])*)&quot;(?:(?!&gt;).)*&gt;", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex AllowedLinkOpenTag();

    [GeneratedRegex(@"&lt;(?<close>/?)(?<tag>p|br|b|strong|i|em|u|s|ul|ol|li|blockquote|code|pre|h[1-6])\b(?:(?!&gt;).)*&gt;", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex AllowedFormattingTag();

    [GeneratedRegex(@"<(?<close>/?)(?<name>[a-zA-Z][a-zA-Z0-9-]*)(?<attributes>[^>]*)>")]
    private static partial Regex EditorTag();

    [GeneratedRegex(@"<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex HtmlComment();

    /// <summary>
    /// Rewrites the editor's real tags (see <see cref="FromEditorHtml" />). Span replacements are tracked on a stack, so
    /// each closing span gets the closing tag its opening span became.
    /// </summary>
    private static string NormalizeEditorTags(string html)
    {
        var spans = new Stack<string>();

        return EditorTag().Replace(html, match =>
        {
            var isClose = match.Groups["close"].Value.Length > 0;
            var name = match.Groups["name"].Value.ToLowerInvariant();
            var attributes = match.Groups["attributes"].Value;

            switch (name)
            {
                case "div":
                case "h1" or "h2" or "h3" or "h4" or "h5" or "h6":
                    return isClose ? "</p>" : "<p>";

                case "span" when isClose:
                    return spans.Count > 0 ? spans.Pop() : string.Empty;

                case "span":
                    var (open, close) = attributes.Contains("underline", StringComparison.OrdinalIgnoreCase) ? ("<u>", "</u>")
                        : attributes.Contains("line-through", StringComparison.OrdinalIgnoreCase) ? ("<s>", "</s>")
                        : (string.Empty, string.Empty);
                    spans.Push(close);
                    return open;

                case "p" or "br" or "b" or "strong" or "i" or "em" or "u" or "s" or "ul" or "ol" or "li" or "blockquote" or "code" or "pre" or "a":
                    return match.Value;

                default:
                    return string.Empty;
            }
        });
    }

    [GeneratedRegex(@"<(script|style)\b[^>]*>.*?</\1\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptOrStyleBlock();

    [GeneratedRegex(@"&lt;/a\s*&gt;", RegexOptions.IgnoreCase)]
    private static partial Regex LinkCloseTag();

    #endregion

    #endregion
}
