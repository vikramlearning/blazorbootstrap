using System.Runtime.InteropServices;

namespace BlazorBootstrap.Demo.RCL;

public class MainLayoutBase : LayoutComponentBase
{
    internal Sidebar2 sidebar2 = default!;
    internal IReadOnlyCollection<NavItem>? NavItems;

    [Inject] public IConfiguration Configuration { get; set; } = default!;

    [Inject] protected IJSRuntime JS { get; set; } = default!;

    protected override void OnInitialized()
    {
        Version = $"v{Configuration["version"]}"; // example: v0.6.1
        DocsUrl = $"{Configuration["urls:docs"]}";
        BlogUrl = $"{Configuration["urls:blog"]}";
        GithubUrl = $"{Configuration["urls:github"]}";
        TwitterUrl = $"{Configuration["urls:twitter"]}";
        LinkedInUrl = $"{Configuration["urls:linkedin"]}";
        OpenCollectiveUrl = $"{Configuration["urls:opencollective"]}";
        GithubIssuesUrl = $"{Configuration["urls:github_issues"]}";
        GithubDiscussionsUrl = $"{Configuration["urls:github_discussions"]}";
        StackoverflowUrl = $"{Configuration["urls:stackoverflow"]}";
        base.OnInitialized();
    }

    internal virtual Task<Sidebar2DataProviderResult> Sidebar2DataProvider()
    {
        return Task.FromResult(Sidebar2DataProviderRequest.ApplyTo(NavItems));
    }
    internal virtual IReadOnlyCollection<NavItem> GetNavItems() => new List<NavItem>();

    public string Version { get; private set; } = default!;

    public string DocsUrl { get; private set; } = default!;

    public string BlogUrl { get; private set; } = default!;

    public string GithubUrl { get; private set; } = default!;

    public string TwitterUrl { get; private set; } = default!;

    public string LinkedInUrl { get; private set; } = default!;

    public string OpenCollectiveUrl { get; private set; } = default!;

    public string GithubIssuesUrl { get; private set; } = default!;

    public string GithubDiscussionsUrl { get; private set; } = default!;

    public string StackoverflowUrl { get; private set; } = default!;
    
    public string DotNetVersion => RuntimeInformation.FrameworkDescription;
}
