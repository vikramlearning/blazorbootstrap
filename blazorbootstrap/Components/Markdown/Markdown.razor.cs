using Microsoft.AspNetCore.Components.Rendering;
using System.Text.RegularExpressions;

namespace BlazorBootstrap;

public partial class Markdown : BlazorBootstrapComponentBase
{
    private string? html;

    private const string CODE_HIGHLIGHTING_LINE_SEPERATOR = " $$CHLS$$ ";
    private const string Pattern_Ordered_List = @"^\s*\d+\.\s";
    private const string Pattern_Unordered_List = @"^\s*\-\s";
    private const string Pattern_Horizontal_Rules = @"^\-{3,}$";

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
        html = markup.Replace(CODE_HIGHLIGHTING_LINE_SEPERATOR, "\n");
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
                    var lines = frame.MarkupContent.Split("\r\n").ToList();

                    if (lines.Any())
                        inputs.AddRange(lines);
                }
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

    private List<MarkdownPattern> GetRules()
    {
        return new List<MarkdownPattern>
        {
            // Headers
            // done

            // Blockquotes
            // done

            // Horizontal rules
            // done

            // Emphasis (bold, italics, strikethrough)
            // done

            // Code highlighting
            // done

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

    // TODO: fix this method
    private string ConvertMarkdownBlockquotesToHtml(string markup)
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

            if (Regex.IsMatch(line.Trim(), @"^>{1}\s(.*)"))
            {
                parsedLines.Add(Regex.Replace(line.Trim(), @"^>{1}\s(.*)", "<blockquote>$1</blockquote>"));
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

            if (Regex.IsMatch(line.Trim(), Pattern_Horizontal_Rules))
            {
                RemoveLastLineBreak(parsedLines);
                parsedLines.Add(Regex.Replace(line.Trim(), Pattern_Horizontal_Rules, "<hr />"));
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

    private string ConvertMarkdownCodeHighlightingToHtml(string markup)
    {
        var lines = markup.Split("\n");
        var parsedLines = new List<string>();
        var isCodeBlockInprogress = false;

        for (var i = 0; i < lines.Count(); i++)
        {
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
        }

        RemoveLastLineBreak(parsedLines);

        return string.Join("", parsedLines);
    }

    private string ConvertMarkdownListToHtml(string markup)
    {
        var lines = markup.Split("\n");
        var htmlLines = new List<string>();
        var listStack = new Stack<string>();
        var indentStack = new Stack<int>();

        foreach (var line in lines)
        {
            var indentLevel = line.TakeWhile(char.IsWhiteSpace).Count();

            if (Regex.IsMatch(line, Pattern_Ordered_List))
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
                    htmlLines.Add($"<li>{Regex.Replace(line, Pattern_Ordered_List, "")}");
                }
                else if (indentStack.Peek() > indentLevel)
                {
                    htmlLines.Add($"</{listStack.Pop()}>");
                    indentStack.Pop();

                    htmlLines.Add($"<li>{Regex.Replace(line, Pattern_Ordered_List, "")}");
                }
                else if (indentStack.Peek() == indentLevel)
                {
                    htmlLines.Add($"<li>{Regex.Replace(line, Pattern_Ordered_List, "")}");
                }
            }
            else if (Regex.IsMatch(line, Pattern_Unordered_List))
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
                    htmlLines.Add($"<li>{Regex.Replace(line, Pattern_Unordered_List, "")}");
                }
                else if (indentStack.Peek() > indentLevel)
                {
                    htmlLines.Add($"</{listStack.Pop()}>");
                    indentStack.Pop();

                    htmlLines.Add($"<li>{Regex.Replace(line, Pattern_Unordered_List, "")}");
                }
                else if (indentStack.Peek() == indentLevel)
                {
                    htmlLines.Add($"<li>{Regex.Replace(line, Pattern_Unordered_List, "")}");
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
        for (int i = 0; i < htmlLines.Count; i++)
        {
            if (htmlLines[i].StartsWith("<li>") && (i == htmlLines.Count - 1 || htmlLines[i + 1].StartsWith("<li>") || htmlLines[i + 1].StartsWith("</")))
            {
                htmlLines[i] += "</li>";
            }
        }

        RemoveLastLineBreak(htmlLines);

        return string.Join("", htmlLines);
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

    private string ConvertMarkdownLineBreaksToHtml(string markup) => markup.Replace("\n", "<br />");

    private static void RemoveLastLineBreak(List<string> htmlLines)
    {
        // remove last line break
        if (htmlLines.Any() && htmlLines[^1] == "\n")
            htmlLines.RemoveAt(htmlLines.Count - 1);
    }
}
