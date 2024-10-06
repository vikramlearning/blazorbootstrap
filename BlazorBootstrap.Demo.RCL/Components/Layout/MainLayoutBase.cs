namespace BlazorBootstrap.Demo.RCL;

public class MainLayoutBase : LayoutComponentBase
{
    internal string version = default!;
    internal string docsUrl = default!;
    internal string blogUrl = default!;
    internal string githubUrl = default!;
    internal string twitterUrl = default!;
    internal string linkedInUrl = default!;
    internal string openCollectiveUrl = default!;
    internal string githubIssuesUrl = default!;
    internal string githubDiscussionsUrl = default!;
    internal string stackoverflowUrl = default!;

    internal Sidebar2 sidebar2 = default!;
    internal IEnumerable<NavItem> navItems = default!;

    [Inject] public IConfiguration Configuration { get; set; } = default!;

    [Inject] protected IJSRuntime JS { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await JS.InvokeVoidAsync("initializeTheme");

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        version = $"v{Configuration["version"]}"; // example: v0.6.1
        docsUrl = $"{Configuration["urls:docs"]}";
        blogUrl = $"{Configuration["urls:blog"]}";
        githubUrl = $"{Configuration["urls:github"]}";
        twitterUrl = $"{Configuration["urls:twitter"]}";
        linkedInUrl = $"{Configuration["urls:linkedin"]}";
        openCollectiveUrl = $"{Configuration["urls:opencollective"]}";
        githubIssuesUrl = $"{Configuration["urls:github_issues"]}";
        githubDiscussionsUrl = $"{Configuration["urls:github_discussions"]}";
        stackoverflowUrl = $"{Configuration["urls:stackoverflow"]}";
        base.OnInitialized();
    }

    internal Task SetAutoTheme() => SetTheme("system");

    internal Task SetDarkTheme() => SetTheme("dark");

    internal Task SetLightTheme() => SetTheme("light");

    internal async Task SetTheme(string themeName) => await JS.InvokeVoidAsync("setTheme", themeName);

    internal virtual async Task<Sidebar2DataProviderResult> Sidebar2DataProvider(Sidebar2DataProviderRequest request)
    {
        if (navItems is null)
            navItems = GetNavItems();

        return await Task.FromResult(request.ApplyTo(navItems));
    }

    internal virtual IEnumerable<NavItem> GetNavItems() => new List<NavItem>();

    public string Version => version;
    public string DocsUrl => docsUrl;
    public string BlogUrl => blogUrl;
    public string GithubUrl => githubUrl;
    public string TwitterUrl => twitterUrl;
    public string LinkedInUrl => linkedInUrl;
    public string OpenCollectiveUrl => openCollectiveUrl;
    public string GithubIssuesUrl => githubIssuesUrl;
    public string GithubDiscussionsUrl => githubDiscussionsUrl;
    public string StackoverflowUrl => stackoverflowUrl;
}
