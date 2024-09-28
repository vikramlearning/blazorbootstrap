using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
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
        if (ChildContent is not null)
        {
            var builder = new RenderTreeBuilder();
            ChildContent.Invoke(builder);
            var frames = builder.GetFrames().Array;
            var lines = GetLines(frames);
            var patterns = GetRules();
            foreach (var pattern in patterns)
            {
                for (var i = 0; i < lines.Count; i++)
                    lines[i] = Regex.Replace(lines[i], pattern.Rule, pattern.Template);
            }

            html = string.Join("\n", lines);
        }

        base.OnInitialized();
    }
    List<string> GetLines(RenderTreeFrame[] frames)
    {
        var inputs = new List<string>();
        foreach (var frame in frames)
        {
            if (frame.MarkupContent is not null)
            {
                var lines = frame.MarkupContent.Split("\r\n");
                foreach (var line in lines)
                    inputs.Add(line.Trim());
            }
        }

        return inputs;
    }

    private List<MarkdownPattern> GetRules()
    {
        return new List<MarkdownPattern>{
            // header rules
            new(@"^#{6}\s?([^\n]+)", "<h6>$1</h6>"),
            new(@"^#{5}\s?([^\n]+)", "<h5>$1</h5>"),
            new(@"^#{4}\s?([^\n]+)", "<h4>$1</h4>"),
            new(@"^#{3}\s?([^\n]+)", "<h3>$1</h3>"),
            new(@"^#{2}\s?([^\n]+)", "<h2>$1</h2>"),
            new(@"^#{1}\s?([^\n]+)", "<h1>$1</h1>"),
        };
    }
}

public record MarkdownPattern(string Rule, string Template);