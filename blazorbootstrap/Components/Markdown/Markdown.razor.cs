﻿using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorBootstrap;

public partial class Markdown : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private const string CODE_HIGHLIGHTING_LINE_SEPERATOR = " $$CHLS$$ ";
    private const string PATTERN_ORDERED_LIST = @"^\s*\d+\.\s";
    private const string PATTERN_UNORDERED_LIST = @"^\s*\-\s";
    private const string PATTERN_HORIZONTAL_RULES = @"^\-{3,}$";
    private const string PATTERN_BLOCKQUOTES = @"^>{1,}\s(.*)";
    private string? html;

    #endregion

    #region Methods

    protected override void OnInitialized()
    {
        ParseMarkdown();

        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        if (IsRenderComplete)
            ParseMarkdown();

        base.OnParametersSet();
    }

    // Headers
    private string ConvertMakdownHeadersToHtml(string markup)
    {
        var lines = markup.Split("\n");
        var parsedLines = new List<string>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                parsedLines.Add(line);
                parsedLines.Add("\n");

                continue;
            }

            if (Regex.IsMatch(line.Trim(), @"^#{6}\s?([^\n]+)"))
            {
                parsedLines.Add(Regex.Replace(line.Trim(), @"^#{6}\s?([^\n]+)", "<h6>$1</h6>"));
            }
            else if (Regex.IsMatch(line.Trim(), @"^#{5}\s?([^\n]+)"))
            {
                parsedLines.Add(Regex.Replace(line.Trim(), @"^#{5}\s?([^\n]+)", "<h5>$1</h5>"));
            }
            else if (Regex.IsMatch(line.Trim(), @"^#{4}\s?([^\n]+)"))
            {
                parsedLines.Add(Regex.Replace(line.Trim(), @"^#{4}\s?([^\n]+)", "<h4>$1</h4>"));
            }
            else if (Regex.IsMatch(line.Trim(), @"^#{3}\s?([^\n]+)"))
            {
                parsedLines.Add(Regex.Replace(line.Trim(), @"^#{3}\s?([^\n]+)", "<h3>$1</h3>"));
            }
            else if (Regex.IsMatch(line.Trim(), @"^#{2}\s?([^\n]+)"))
            {
                parsedLines.Add(Regex.Replace(line.Trim(), @"^#{2}\s?([^\n]+)", "<h2>$1</h2>"));
            }
            else if (Regex.IsMatch(line.Trim(), @"^#{1}\s?([^\n]+)"))
            {
                parsedLines.Add(Regex.Replace(line.Trim(), @"^#{1}\s?([^\n]+)", "<h1>$1</h1>"));
            }
            else
            {
                parsedLines.Add(line);
                parsedLines.Add("\n");
            }
        }

        RemoveLastLineBreak(parsedLines);

        return string.Join("", parsedLines);
    }

    // Blockquotes
    private string ConvertMarkdownBlockquotesToHtml(string markup)
    {
        var lines = markup.Split("\n");
        var htmlLines = new List<string>();
        var listStack = new Stack<string>();
        var indentStack = new Stack<int>();

        foreach (var line in lines)
        {
            var trimmerLine = line.Trim();
            var indentLevel = trimmerLine.TakeWhile(c => c == '>').Count();

            if (Regex.IsMatch(trimmerLine, PATTERN_BLOCKQUOTES))
            {
                if (listStack.Count == 0 || indentStack.Peek() < indentLevel)
                {
                    if (listStack.Count > 0 && indentStack.Peek() < indentLevel)
                    {
                        // close the `p` tag
                        if (listStack.Peek() == "p")
                            htmlLines.Add($"</{listStack.Pop()}>");

                        htmlLines.Add($"<blockquote class=\"{BlockquotesCssClass}\">");
                        listStack.Push("blockquote");
                        indentStack.Push(indentLevel);
                    }
                    else
                    {
                        while (listStack.Count > 0 && indentStack.Peek() > indentLevel)
                        {
                            htmlLines.Add($"</{listStack.Pop()}>");
                            indentStack.Pop();
                        }

                        if (listStack.Count == 0 || listStack.Peek() != "blockquote")
                        {
                            htmlLines.Add($"<blockquote class=\"{BlockquotesCssClass}\">");
                            listStack.Push("blockquote");
                            indentStack.Push(indentLevel);
                        }
                    }

                    htmlLines.Add($"<p>{Regex.Replace(trimmerLine, PATTERN_BLOCKQUOTES, "$1")}");
                    listStack.Push("p");
                }
                else if (indentStack.Peek() >= indentLevel)
                {
                    htmlLines.Add($"<br />{Regex.Replace(trimmerLine, PATTERN_BLOCKQUOTES, "$1")}");
                }
            }
            else
            {
                // Close any open lists
                while (listStack.Count > 0)
                {
                    htmlLines.Add($"</{listStack.Pop()}>");

                    if (indentStack.Count > 0)
                        indentStack.Pop();
                }

                htmlLines.Add(line);
                htmlLines.Add("\n");
            }
        }

        // Close any remaining open blockquotes
        while (listStack.Count > 0)
        {
            htmlLines.Add($"</{listStack.Pop()}>");

            if (indentStack.Count > 0)
                indentStack.Pop();
        }

        return string.Join("", htmlLines);
    }

    // Code Highlighting
    private string ConvertMarkdownCodeHighlightingToHtml(string markup)
    {
        var lines = markup.Split("\n");
        var parsedLines = new List<string>();
        var isCodeBlockInprogress = false;

        for (var i = 0; i < lines.Count(); i++)
            if (Regex.IsMatch(lines[i].Trim(), @"\```(\w+)"))
            {
                if (!isCodeBlockInprogress)
                    isCodeBlockInprogress = true;

                lines[i] = Regex.Replace(lines[i].Trim(), @"\```(\w+)", "<pre><code class=\"lang-$1\">");
                parsedLines.Add(lines[i]);
            }
            else if (Regex.IsMatch(lines[i].Trim().Trim(), @"```"))
            {
                if (isCodeBlockInprogress)
                    isCodeBlockInprogress = false;

                lines[i] = Regex.Replace(lines[i].Trim(), @"```", "</code></pre>");
                parsedLines.Add(lines[i]);
            }
            else if (isCodeBlockInprogress)
            {
                parsedLines.Add(lines[i]);
                parsedLines.Add($"{CODE_HIGHLIGHTING_LINE_SEPERATOR}");
            }
            else
            {
                parsedLines.Add(lines[i]);
                parsedLines.Add("\n");
            }

        RemoveLastLineBreak(parsedLines);

        return string.Join("", parsedLines);
    }

    // Emphasis
    private string ConvertMarkdownEmphasisToHtml(string markup)
    {
        var lines = markup.Split("\n");
        var parsedLines = new List<string>();

        for (var i = 0; i < lines.Count(); i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                parsedLines.Add(lines[i]);

                continue;
            }

            if (Regex.IsMatch(lines[i].Trim(), @"\*\*(.*?)\*\*")
                || Regex.IsMatch(lines[i].Trim().Trim(), @"__(.*?)__")
                || Regex.IsMatch(lines[i].Trim(), @"\*(.*?)\*")
                || Regex.IsMatch(lines[i].Trim(), @"_(.*?)_")
                || Regex.IsMatch(lines[i].Trim(), @"~~(.*?)~~"))
            {
                lines[i] = Regex.Replace(lines[i].Trim(), @"\*\*(.*?)\*\*", "<b>$1</b>");
                lines[i] = Regex.Replace(lines[i].Trim(), @"__(.*?)__", "<b>$1</b>");
                lines[i] = Regex.Replace(lines[i].Trim(), @"\*(.*?)\*", "<i>$1</i>");
                lines[i] = Regex.Replace(lines[i].Trim(), @"_(.*?)_", "<i>$1</i>");
                lines[i] = Regex.Replace(lines[i].Trim(), @"~~(.*?)~~", "<s>$1</s>");
            }

            parsedLines.Add(lines[i]);
        }

        return string.Join("\n", parsedLines);
    }

    // HorizontalRules
    private string ConvertMarkdownHorizontalRulesToHtml(string markup)
    {
        var lines = markup.Split("\n");
        var parsedLines = new List<string>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                parsedLines.Add(line);
                parsedLines.Add("\n");

                continue;
            }

            if (Regex.IsMatch(line.Trim(), PATTERN_HORIZONTAL_RULES))
            {
                RemoveLastLineBreak(parsedLines);
                parsedLines.Add(Regex.Replace(line.Trim(), PATTERN_HORIZONTAL_RULES, "<hr />"));
            }
            else
            {
                parsedLines.Add(line);
                parsedLines.Add("\n");
            }
        }

        RemoveLastLineBreak(parsedLines);

        return string.Join("", parsedLines);
    }

    // Image
    private string ConvertMarkdownImageToHtml(string markup)
    {
        // Pattern to match Markdown image syntax: ![alt text](url "optional title" =WIDTHxHEIGHT)
        var pattern = @"!\[(.*?)\]\((.*?)(\s*""[^""]*"")?(\s*=\s*(\d*)x?(\d*))?\)";

        // Replace Markdown image syntax with HTML <img> tag
        var html = Regex.Replace(
            markup,
            pattern,
            match =>
            {
                var altText = match.Groups[1].Value;
                var url = match.Groups[2].Value;
                var title = match.Groups[3].Value;
                var width = match.Groups[5].Value;
                var height = match.Groups[6].Value;

                var imgTag = $"<img src=\"{url}\" alt=\"{altText}\"";

                if (!string.IsNullOrEmpty(title)) imgTag += $" title={title}";

                if (!string.IsNullOrEmpty(width)) imgTag += $" width=\"{width}\"";

                if (!string.IsNullOrEmpty(height)) imgTag += $" height=\"{height}\"";

                imgTag += " />";

                return imgTag;
            }
        );

        return html;
    }

    // Inline code
    private string ConvertMarkdownInlineCodeToHtml(string markup)
    {
        // Pattern to match inline Markdown code syntax: `code`
        var pattern = @"`([^`]+)`";

        // Replace inline Markdown code syntax with HTML <code> tag
        var html = Regex.Replace(
            markup,
            pattern,
            match =>
            {
                var codeText = match.Groups[1].Value;

                return $"<code>{codeText}</code>";
            }
        );

        return html;
    }

    // Line breaks
    private string ConvertMarkdownLineBreaksToHtml(string markup) => markup.Replace("\n", "<br />");

    // Links
    private string ConvertMarkdownLinksToHtml(string markup)
    {
        // Pattern to match Markdown link syntax: [Link Text](Link URL)
        var pattern = @"\[(.*?)\]\((.*?)\)";

        // Replace Markdown link syntax with HTML <a> tag
        var html = Regex.Replace(
            markup,
            pattern,
            match =>
            {
                var linkText = match.Groups[1].Value;
                var linkUrl = match.Groups[2].Value;

                return $"<a href=\"{linkUrl}\">{linkText}</a>";
            }
        );

        return html;
    }

    // Lists
    private string ConvertMarkdownListToHtml(string markup)
    {
        var lines = markup.Split("\n");
        var htmlLines = new List<string>();
        var listStack = new Stack<string>();
        var indentStack = new Stack<int>();

        foreach (var line in lines)
        {
            var indentLevel = line.TakeWhile(char.IsWhiteSpace).Count();

            if (Regex.IsMatch(line, PATTERN_ORDERED_LIST))
            {
                // Ordered list
                if (listStack.Count == 0 || listStack.Peek() != "ol" || indentStack.Peek() < indentLevel)
                {
                    if (listStack.Count > 0 && indentStack.Peek() < indentLevel)
                    {
                        htmlLines.Add("<ol>");
                        listStack.Push("ol");
                        indentStack.Push(indentLevel);
                    }
                    else
                    {
                        while (listStack.Count > 0 && indentStack.Peek() > indentLevel)
                        {
                            htmlLines.Add($"</{listStack.Pop()}>");
                            indentStack.Pop();
                        }

                        if (listStack.Count == 0 || listStack.Peek() != "ol")
                        {
                            htmlLines.Add("<ol>");
                            listStack.Push("ol");
                            indentStack.Push(indentLevel);
                        }
                    }

                    htmlLines.Add($"<li>{Regex.Replace(line, PATTERN_ORDERED_LIST, "")}");
                }
                else if (indentStack.Peek() > indentLevel)
                {
                    htmlLines.Add($"</{listStack.Pop()}>");
                    indentStack.Pop();

                    htmlLines.Add($"<li>{Regex.Replace(line, PATTERN_ORDERED_LIST, "")}");
                }
                else if (indentStack.Peek() == indentLevel)
                {
                    htmlLines.Add($"<li>{Regex.Replace(line, PATTERN_ORDERED_LIST, "")}");
                }
            }
            else if (Regex.IsMatch(line, PATTERN_UNORDERED_LIST))
            {
                // Unordered list
                if (listStack.Count == 0 || listStack.Peek() != "ul" || indentStack.Peek() < indentLevel)
                {
                    if (listStack.Count > 0 && indentStack.Peek() < indentLevel)
                    {
                        htmlLines.Add("<ul>");
                        listStack.Push("ul");
                        indentStack.Push(indentLevel);
                    }
                    else
                    {
                        while (listStack.Count > 0 && indentStack.Peek() > indentLevel)
                        {
                            htmlLines.Add($"</{listStack.Pop()}>");
                            indentStack.Pop();
                        }

                        if (listStack.Count == 0 || listStack.Peek() != "ul")
                        {
                            htmlLines.Add("<ul>");
                            listStack.Push("ul");
                            indentStack.Push(indentLevel);
                        }
                    }

                    htmlLines.Add($"<li>{Regex.Replace(line, PATTERN_UNORDERED_LIST, "")}");
                }
                else if (indentStack.Peek() > indentLevel)
                {
                    htmlLines.Add($"</{listStack.Pop()}>");
                    indentStack.Pop();

                    htmlLines.Add($"<li>{Regex.Replace(line, PATTERN_UNORDERED_LIST, "")}");
                }
                else if (indentStack.Peek() == indentLevel)
                {
                    htmlLines.Add($"<li>{Regex.Replace(line, PATTERN_UNORDERED_LIST, "")}");
                }
            }
            else
            {
                // Close any open lists
                while (listStack.Count > 0)
                {
                    htmlLines.Add($"</{listStack.Pop()}>");
                    indentStack.Pop();
                }

                htmlLines.Add(line);
                htmlLines.Add("\n");
            }
        }

        // Close any remaining open lists
        while (listStack.Count > 0)
        {
            htmlLines.Add($"</{listStack.Pop()}>");
            indentStack.Pop();
        }

        // Close any open list items
        for (var i = 0; i < htmlLines.Count; i++)
            if (htmlLines[i].StartsWith("<li>") && (i == htmlLines.Count - 1 || htmlLines[i + 1].StartsWith("<li>") || htmlLines[i + 1].StartsWith("</")))
                htmlLines[i] += "</li>";

        RemoveLastLineBreak(htmlLines);

        return string.Join("", htmlLines);
    }

    // Paragraphs
    private string ConvertMarkdownParagraphsToHtml(string markup)
    {
        var lines = markup.Split("\n\n\n");
        var parsedLines = new List<string>();

        if (lines.Length == 1)
            return markup;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                parsedLines.Add(line);

                continue;
            }

            parsedLines.Add($"<p>{line}</p>");
        }

        return string.Join("", parsedLines);
    }

    // Tables
    private string ConvertMarkdownTableToHtml(string markup)
    {
        var lines = markup.Split("\n");
        var parsedLines = new List<string>();
        var htmlLines = new List<string>();

        var isTableStart = false;
        var isTableHeadingAdded = false;

        // Read lines starting with '|'
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                parsedLines.Add(line);

                continue;
            }

            // Trim row with spaces
            var trimmedLine = line.Trim();

            if (trimmedLine.StartsWith("| "))
            {
                if (!isTableStart)
                    isTableStart = true;

                // Remove '|' from the start and end of the line
                trimmedLine = trimmedLine.TrimStart('|').TrimEnd('|');

                // Convert markdown syntax to HTML
                var cells = trimmedLine.Split("|", StringSplitOptions.RemoveEmptyEntries);
                var tableRow = "<tr>";

                foreach (var cell in cells)
                {
                    var tableCellTagName = !isTableHeadingAdded ? "th" : "td";
                    var tableCell = $"<{tableCellTagName}>{cell.Trim()}</{tableCellTagName}>";
                    tableRow += tableCell;
                }

                tableRow += "</tr>";
                htmlLines.Add(tableRow);
            }
            else if (trimmedLine.StartsWith("|--") || trimmedLine.StartsWith("|:--"))
            {
                // Table heading row is over
                if (!isTableHeadingAdded)
                {
                    isTableHeadingAdded = true;
                    htmlLines.Add("</thead>");
                    htmlLines.Add("<tbody>");
                }
            }
            else
            {
                if (isTableStart)
                {
                    isTableStart = false;
                    parsedLines.Add($"<table class=\"{TableCssClass}\"><thead>{string.Join("", htmlLines)}</tbody></table>");
                    htmlLines.Clear();
                }
                else
                {
                    parsedLines.Add(line);
                }
            }
        }

        if (isTableStart && htmlLines.Any())
        {
            isTableStart = false;
            parsedLines.Add($"<table class=\"{TableCssClass}\"><thead>{string.Join("", htmlLines)}</tbody></table>");
            htmlLines.Clear();
        }

        return string.Join("\n", parsedLines);
    }

    private List<string> GetLines()
    {
        var inputs = new List<string>();

        if (ChildContent is not null)
        {
            var builder = new RenderTreeBuilder();
            ChildContent.Invoke(builder);

            var frames = builder.GetFrames().Array;

            foreach (var frame in frames)
                if (frame.MarkupContent is not null)
                {
                    var lines = frame.MarkupContent.Split("\r\n").ToList();

                    if (lines.Any())
                        inputs.AddRange(lines);
                }
        }

        if (inputs.Any())
        {
            // remove first blank line
            if (string.IsNullOrWhiteSpace(inputs[0]))
                inputs.RemoveAt(0);

            // remove last blank line
            if (string.IsNullOrWhiteSpace(inputs[^1]))
                inputs.RemoveAt(inputs.Count - 1);
        }

        return inputs;
    }

    private void ParseMarkdown()
    {
        var lines = GetLines();

        if (lines is null)
            return;

        // NOTE: do not change the sequence of these two lines
        var markup = string.Join("\n", lines);
        markup = ConvertMakdownHeadersToHtml(markup);
        markup = ConvertMarkdownBlockquotesToHtml(markup);
        markup = ConvertMarkdownHorizontalRulesToHtml(markup);
        markup = ConvertMarkdownEmphasisToHtml(markup);
        markup = ConvertMarkdownCodeHighlightingToHtml(markup);
        markup = ConvertMarkdownListToHtml(markup);
        markup = ConvertMarkdownTableToHtml(markup);
        markup = ConvertMarkdownParagraphsToHtml(markup);
        markup = ConvertMarkdownLineBreaksToHtml(markup);
        markup = ConvertMarkdownImageToHtml(markup);
        markup = ConvertMarkdownLinksToHtml(markup);
        markup = ConvertMarkdownInlineCodeToHtml(markup);
        html = markup.Replace(CODE_HIGHLIGHTING_LINE_SEPERATOR, "\n");
    }

    //private string ConvertMarkdownChecklistToHtml(string markup)
    //{
    //    // Pattern to match Markdown checklist syntax: - [ ] or - [x] or 1. [ ] or 1. [x]
    //    var pattern = @"^(\s*[-\d]+\.\s*\[([ xX])\])\s*(.*)";

    //    // Replace Markdown checklist syntax with HTML <input type="checkbox"> tag
    //    var html = Regex.Replace(markup, pattern, match =>
    //    {
    //        var checkbox = match.Groups[2].Value.Trim().ToLower() == "x" ? "checked" : "";
    //        var content = match.Groups[3].Value;

    //        return $"<li><input type=\"checkbox\" {checkbox} disabled> {content}</li>";
    //    }, RegexOptions.Multiline);

    //    // Wrap the checkboxes in a <ul> or <ol> tag
    //    html = Regex.Replace(html, @"((<li>.*?</li>\s*)+)", "<ul>$1</ul>", RegexOptions.Singleline);

    //    return html;
    //}

    // Emoji

    // Mathematical notation and characters

    // Mermaid diagrams

    private static void RemoveLastLineBreak(List<string> htmlLines)
    {
        // remove last line break
        if (htmlLines.Any() && htmlLines[^1] == "\n")
            htmlLines.RemoveAt(htmlLines.Count - 1);
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the CSS class for blockquotes.
    /// </summary>
    [Parameter]
    public string? BlockquotesCssClass { get; set; } = "blockquote";

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for table.
    /// </summary>
    [Parameter]
    public string? TableCssClass { get; set; } = "table";

    #endregion
}
