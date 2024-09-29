using Microsoft.AspNetCore.Components.Forms;
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

    #endregion

    protected override void OnInitialized()
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
        html = ApplyFullMarkupRules(markup);
        base.OnInitialized();
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
                if (frame.MarkupContent is not null)
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
}
