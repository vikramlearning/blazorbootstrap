namespace BlazorBootstrap.Demo.RCL;

public partial class WebsiteNavbar : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool navbarMenuActive;

    private string version = default!;
    private string dotNetVersion = default!;
    private string docsUrl = default!;
    private string blogUrl = default!;
    private string githubUrl = default!;
    private string nugetUrl = default!;
    private string twitterUrl = default!;
    private string linkedInUrl = default!;
    private string openCollectiveUrl = default!;
    private string githubIssuesUrl = default!;
    private string githubDiscussionsUrl = default!;
    private string stackoverflowUrl = default!;

    #endregion

    #region Methods

    protected override void OnInitialized()
    {
        version = $"v{Configuration["version"]}"; // example: v0.6.1
        dotNetVersion = $".NET {Configuration["dotNetVersion"]}"; // example: 9.0.0
        docsUrl = $"{Configuration["urls:docs"]}";
        blogUrl = $"{Configuration["urls:blog"]}";
        githubUrl = $"{Configuration["urls:github"]}";
        nugetUrl = $"{Configuration["urls:nuget"]}";
        twitterUrl = $"{Configuration["urls:twitter"]}";
        linkedInUrl = $"{Configuration["urls:linkedin"]}";
        openCollectiveUrl = $"{Configuration["urls:opencollective"]}";
        githubIssuesUrl = $"{Configuration["urls:github_issues"]}";
        githubDiscussionsUrl = $"{Configuration["urls:github_discussions"]}";
        stackoverflowUrl = $"{Configuration["urls:stackoverflow"]}";
    }

    private Task SetAutoTheme() => SetTheme("system");

    private Task SetDarkTheme() => SetTheme("dark");

    private Task SetLightTheme() => SetTheme("light");

    private async Task SetTheme(string themeName)
    {
        await JSRuntime.InvokeVoidAsync("setTheme", themeName);
        navbarMenuActive = false;
    }

    private void ToggleNavbarMenu(bool isActive)
    {
        navbarMenuActive = isActive;
    }

    private async ValueTask OnThemeChanged(string themeName)
        => await JSRuntime.InvokeVoidAsync("updateDemoCodeThemeCss", themeName);

    #endregion

    #region Properties, Indexers

    [Inject] public IConfiguration Configuration { get; set; } = default!;

    [Parameter] public string? ApplicationName { get; set; } = null;

    [Parameter] public string? BrandImgAltText { get; set; } = null;

    [Parameter] public string? BrandImgSrc { get; set; } = null;

    public string Version => version;
    public string DotNetVersion => dotNetVersion;
    public string DocsUrl => docsUrl;
    public string BlogUrl => blogUrl;
    public string GithubUrl => githubUrl;
    public string NuGetUrl => nugetUrl;
    public string TwitterUrl => twitterUrl;
    public string LinkedInUrl => linkedInUrl;
    public string OpenCollectiveUrl => openCollectiveUrl;
    public string GithubIssuesUrl => githubIssuesUrl;
    public string GithubDiscussionsUrl => githubDiscussionsUrl;
    public string StackoverflowUrl => stackoverflowUrl;

    #endregion
}
