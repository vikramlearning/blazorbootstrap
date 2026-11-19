namespace BlazorBootstrap.Demo.RCL;

public partial class MainLayoutBaseFooter : ComponentBase
{
    #region Properties, Indexers

    [Parameter] public string? BlogUrl { get; set; }
    [Parameter] public string? DemosUrl { get; set; }
    [Parameter] public string? DocsUrl { get; set; }
    [Parameter] public string? DotNetVersion { get; set; }
    [Parameter] public string? GithubDiscussionsUrl { get; set; }
    [Parameter] public string? GithubIssuesUrl { get; set; }
    [Parameter] public string? GithubUrl { get; set; }
    [Parameter] public string? LinkedInUrl { get; set; }

    /// <summary>
    /// Optional statement shown above the footer's bottom bar, such as the Showcase gallery's fictitious-data note.
    /// </summary>
    [Parameter] public RenderFragment? Note { get; set; }

    [Parameter] public string? OpenCollectiveUrl { get; set; }
    [Parameter] public string? StackoverflowUrl { get; set; }
    [Parameter] public string? TwitterUrl { get; set; }
    [Parameter] public string? Version { get; set; }

    #endregion
}
