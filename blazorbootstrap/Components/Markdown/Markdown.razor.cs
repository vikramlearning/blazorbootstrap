using Microsoft.AspNetCore.Components.Rendering;
using System.Text.RegularExpressions;

namespace BlazorBootstrap;

public partial class Markdown : BlazorBootstrapComponentBase
{
    private string? html;

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? TableCssClass { get; set; } = "table";

    #endregion

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

    private void ParseMarkdown()
    {
        var lines = GetLines();
        if (lines.Any())
        {
            // remove start and end blank lines
            if (lines[0] == "")
                lines.RemoveAt(0);

            if (lines[lines.Count - 1] == "")
                lines.RemoveAt(lines.Count - 1);
        }

        var markup = ApplyRules(lines);
        markup = ConvertMarkdownTableToHtml(markup);
        markup = ConvertMarkdownListToHtml(markup);
        html = ApplyFullMarkupRules(markup);
    }

    private string ApplyRules(List<string> lines)
    {
        var patterns = GetRules();
        foreach (var pattern in patterns)
        {
            for (var i = 0; i < lines.Count; i++)
                lines[i] = Regex.Replace(lines[i], pattern.Rule, pattern.Template);
        }

        return string.Join("\n", lines);
    }

    private string ApplyFullMarkupRules(string markup)
    {
        var patterns = GetFullMarkupRules();

        foreach (var pattern in patterns)
            markup = Regex.Replace(markup, pattern.Rule, pattern.Template);

        return markup;
    }    

    List<string> GetLines()
    {
        var inputs = new List<string>();

        if (ChildContent is not null)
        {
            var builder = new RenderTreeBuilder();
            ChildContent.Invoke(builder);

            var frames = builder.GetFrames().Array;
            foreach (var frame in frames)
            {
                if (frame.FrameType == Microsoft.AspNetCore.Components.RenderTree.RenderTreeFrameType.Markup) // .MarkupContent is not null)
                {
                    var lines = frame.MarkupContent.Split("\r\n");
                    foreach (var line in lines)
                        inputs.Add(line.Trim());
                }
            }
        }

        return inputs;
    }

    private List<MarkdownPattern> GetRules()
    {
        return new List<MarkdownPattern>
        {
            // Headers
            new(@"^#{6}\s?([^\n]+)", "<h6>$1</h6>"),
            new(@"^#{5}\s?([^\n]+)", "<h5>$1</h5>"),
            new(@"^#{4}\s?([^\n]+)", "<h4>$1</h4>"),
            new(@"^#{3}\s?([^\n]+)", "<h3>$1</h3>"),
            new(@"^#{2}\s?([^\n]+)", "<h2>$1</h2>"),
            new(@"^#{1}\s?([^\n]+)", "<h1>$1</h1>"),

            // Blockquotes
            new(@"^>{1}\s(.*)$", "<blockquote>$1</blockquote>"),
            //new(@"^(>>)+ (.*)$", "<blockquote><p>$2</p></blockquote>"),

            // Horizontal rules
            new(@"^\-{3,}$", "<hr />"),

             // Emphasis (bold, italics, strikethrough)
            new(@"\*\*(.*?)\*\*", "<b>$1</b>"),
            new(@"__(.*?)__", "<b>$1</b>"),
            new(@"\*(.*?)\*", "<i>$1</i>"),
            new(@"_(.*?)_", "<i>$1</i>"),
            new(@"~~(.*?)~~", "<s>$1</s>"),

            // Code highlighting
            new(@"\```(\w+)", "<pre><code class=\"lang-$1\">"),
            new(@"```", "</code></pre>"),

            // Tables
            // done

            // Lists
            // Ordered or numbered lists

            // Bulleted lists

            // Nested lists

            // Links

            // Anchor links

            // Images

            // Checklist or task list

            // Emoji

            // Mathematical notation and characters

            // Mermaid diagrams
        };
    }

    private List<MarkdownPattern> GetFullMarkupRules()
    {
        return new List<MarkdownPattern>
        {
            // Paragraphs and line breaks
            new(@"(?<!\n)\n(?!\n)", "<br />"),
            new(@"([^\n\n]+\n?)", "<p>$1</p>"),
        };
    }

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

    private string ConvertMarkdownListToHtml(string markup)
    {
        var lines = markup.Split("\n");
        var htmlLines = new List<string>();
        var listStack = new Stack<string>();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (Regex.IsMatch(trimmedLine, @"^\d+\.\s"))
            {
                // Ordered list
                if (listStack.Count == 0 || listStack.Peek() != "ol")
                {
                    if (listStack.Count > 0)
                        htmlLines.Add($"</{listStack.Pop()}>");

                    htmlLines.Add("<ol>");
                    listStack.Push("ol");
                }
                htmlLines.Add($"<li>{Regex.Replace(trimmedLine, @"^\d+\.\s", "")}</li>");
            }
            else if (Regex.IsMatch(trimmedLine, @"^\-\s"))
            {
                // Unordered list
                if (listStack.Count == 0 || listStack.Peek() != "ul")
                {
                    if (listStack.Count > 0)
                        htmlLines.Add($"</{listStack.Pop()}>");

                    htmlLines.Add("<ul>");
                    listStack.Push("ul");
                }
                htmlLines.Add($"<li>{Regex.Replace(trimmedLine, @"^\-\s", "")}</li>");
            }
            else
            {
                // Close any open lists
                while (listStack.Count > 0)
                    htmlLines.Add($"</{listStack.Pop()}>");

                htmlLines.Add(line);
            }
        }

        // Close any remaining open lists
        while (listStack.Count > 0)
            htmlLines.Add($"</{listStack.Pop()}>");

        return string.Join("", htmlLines);
    }

}
