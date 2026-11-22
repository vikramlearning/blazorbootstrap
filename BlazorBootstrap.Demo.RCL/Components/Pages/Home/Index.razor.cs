using System.Text.RegularExpressions;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Home;

/// <summary>
/// The demo site home page: hero, Showcase section, and the component catalog.
/// </summary>
public partial class Index : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private const string CopiedText = "Copied";

    private const string CopyFailedText = "Couldn't copy. Select the command to copy it.";

    private const string GettingStartedUrl = "/demos/getting-started";

    // The same shields.io sources as the production NuGet and GitHub badges, in JSON form.
    // shields.io caches them for 30 minutes and allows cross-origin requests from the WebAssembly host.
    private const string GitHubStarsBadgeUrl = "https://img.shields.io/github/stars/vikramlearning/blazorbootstrap.json";

    private const string InstallCommand = "dotnet add package Blazor.Bootstrap";

    private const string NuGetDownloadsBadgeUrl = "https://img.shields.io/nuget/dt/Blazor.Bootstrap.json";

    private const string NuGetPackageUrl = "https://www.nuget.org/packages/Blazor.Bootstrap/";

    private static readonly TimeSpan LiveStatsTimeout = TimeSpan.FromSeconds(5);

    // A shields.io count such as "2.9M" or "1.2k". Error messages such as "inaccessible" are rejected.
    private static readonly Regex ShieldsCountPattern = new(@"^\d+(\.\d+)?[kMB]?$", RegexOptions.CultureInvariant);

    private readonly CancellationTokenSource disposeCts = new();

    private string copyStatus = string.Empty;

    private string description = "Free, open-source Bootstrap 5.3 components for Blazor Server, WebAssembly and MAUI on .NET 8 to 11: Grid, Modal, Sidebar, forms and more, with live demos.";

    private string filterText = string.Empty;

    private string imageUrl = "https://demos.blazorbootstrap.com/_content/BlazorBootstrap.Demo.RCL/images/social-card.png";

    private DotNetObjectReference<Index>? objRef;

    private string pageUrl = "/";

    private string releaseNotesUrl = default!;

    private string releaseShortDescription = default!;

    private string? githubStars;

    private string? githubUrl;

    private string? nugetDownloads;

    private IReadOnlyList<(string Label, string Value, string? Url)> stats = Array.Empty<(string Label, string Value, string? Url)>();

    private string title = "Bootstrap 5.3 components for Blazor";

    private string version = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            // Cancelled but not disposed, so a fetch still in flight can safely observe it.
            disposeCts.Cancel();
            objRef?.Dispose();
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        // After the first interactive render, so prerendering and first paint never wait on the network.
        if (firstRender)
            await RefreshLiveStatsAsync();
    }

    protected override void OnInitialized()
    {
        version = $"v{Configuration["version"]}"; // example: v4.0.1
        releaseShortDescription = Configuration["release:short_description"]!;
        githubUrl = Configuration["urls:github"];
        releaseNotesUrl = $"{githubUrl}/releases";

        // The configured values show until the live values arrive, and stay if they cannot be fetched.
        nugetDownloads = Configuration["stats:nuget_downloads"];
        githubStars = Configuration["stats:github_stars"];
        BuildStats();

        base.OnInitialized();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Called from JavaScript when the install command could not be copied.
    /// </summary>
    /// <param name="error">The browser's error, which is not used.</param>
    [JSInvokable]
    public void OnCopyFailJS(object? error)
    {
        copyStatus = CopyFailedText;
        StateHasChanged();
    }

    /// <summary>
    /// Called from JavaScript when the install command was copied.
    /// </summary>
    [JSInvokable]
    public void OnCopySuccessJS()
    {
        copyStatus = CopiedText;
        StateHasChanged();
    }

    /// <summary>
    /// Called from JavaScript a moment after a copy attempt, to clear the status message.
    /// </summary>
    [JSInvokable]
    public void ResetCopyStatusJS()
    {
        copyStatus = string.Empty;
        StateHasChanged();
    }

    #endregion

    #region Private Methods

    private void BuildStats()
    {
        // A stat with no value is left out instead of showing an empty box.
        stats = new (string Label, string? Value, string? Url)[]
            {
                ("NuGet downloads", nugetDownloads, NuGetPackageUrl),
                ("GitHub stars", githubStars, string.IsNullOrWhiteSpace(githubUrl) ? null : githubUrl),
                ("Components", Configuration["stats:components"], null)
            }
            .Where(stat => !string.IsNullOrWhiteSpace(stat.Value))
            .Select(stat => (stat.Label, stat.Value!, stat.Url))
            .ToList();
    }

    private async Task CopyInstallCommandAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await SafeInvokeVoidAsync("copyToClipboard", InstallCommand, objRef);
    }

    /// <summary>
    /// Gets the count from a shields.io JSON badge, or <see langword="null" /> when it cannot be fetched or is not a count.
    /// </summary>
    private async Task<string?> GetShieldsCountAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            var badge = await HttpClient.GetFromJsonAsync<ShieldsBadge>(url, cancellationToken);
            var value = badge?.Value?.Trim();

            return value is not null && ShieldsCountPattern.IsMatch(value) ? value : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or System.Text.Json.JsonException or NotSupportedException)
        {
            return null;
        }
    }

    private async Task RefreshLiveStatsAsync()
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(disposeCts.Token);
        timeoutCts.CancelAfter(LiveStatsTimeout);

        var downloadsTask = GetShieldsCountAsync(NuGetDownloadsBadgeUrl, timeoutCts.Token);
        var starsTask = GetShieldsCountAsync(GitHubStarsBadgeUrl, timeoutCts.Token);
        var liveDownloads = await downloadsTask;
        var liveStars = await starsTask;

        if (disposeCts.IsCancellationRequested || (liveDownloads is null && liveStars is null))
            return;

        nugetDownloads = liveDownloads ?? nugetDownloads;
        githubStars = liveStars ?? githubStars;
        BuildStats();
        StateHasChanged();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] public IConfiguration Configuration { get; set; } = default!;

    [Inject] public HttpClient HttpClient { get; set; } = default!;

    private string FilterStatusText
    {
        get
        {
            var shownCount = ShownCategories.Sum(category => category.Items.Count);

            return shownCount == 0 ? "No components match" : $"{shownCount} of {HomeCatalog.TotalCount} shown";
        }
    }

    /// <summary>
    /// Gets the categories and components that match the filter text by name or description, ignoring case.
    /// Categories with no match are left out.
    /// </summary>
    private IEnumerable<HomeCatalogCategory> ShownCategories
    {
        get
        {
            var query = filterText.Trim();

            if (query.Length == 0)
                return HomeCatalog.Categories;

            return HomeCatalog.Categories
                .Select(category => category with
                {
                    Items = category.Items
                        .Where(item => item.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                                       || item.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                        .ToList()
                })
                .Where(category => category.Items.Count > 0)
                .ToList();
        }
    }

    #endregion

    /// <summary>
    /// The part of a shields.io JSON badge this page reads, such as <c>{"value":"2.9M"}</c>.
    /// </summary>
    private sealed record ShieldsBadge(string? Value);
}
