namespace BlazorBootstrap.Demo.RCL.Components.Pages.Home;

/// <summary>
/// The demo site home page: hero, Showcase section, and the component catalog.
/// </summary>
public partial class Index : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private const string CopiedText = "Copied";

    private const string CopyFailedText = "Couldn't copy. Select the command to copy it.";

    private const string GettingStartedUrl = "/getting-started/blazor-webassembly";

    private const string InstallCommand = "dotnet add package Blazor.Bootstrap";

    private string copyStatus = string.Empty;

    private string description = "An Enterprise-class Blazor Bootstrap Component library built on the Blazor and Bootstrap CSS frameworks.";

    private string filterText = string.Empty;

    private string imageUrl = "https://i.imgur.com/HEJciCp.png";

    private DotNetObjectReference<Index>? objRef;

    private string pageUrl = "/";

    private string releaseNotesUrl = default!;

    private string releaseShortDescription = default!;

    private IReadOnlyList<(string Label, string Value)> stats = Array.Empty<(string Label, string Value)>();

    private string title = "Blazor Bootstrap Component Library";

    private string version = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
            objRef?.Dispose();

        await base.DisposeAsyncCore(disposing);
    }

    protected override void OnInitialized()
    {
        version = $"v{Configuration["version"]}"; // example: v4.0.1
        releaseShortDescription = Configuration["release:short_description"]!;
        releaseNotesUrl = $"{Configuration["urls:github"]}/releases";

        // A stat with no configured value is left out instead of showing an empty box.
        stats = new (string Label, string? Value)[]
            {
                ("NuGet downloads", Configuration["stats:nuget_downloads"]),
                ("GitHub stars", Configuration["stats:github_stars"]),
                ("Components", Configuration["stats:components"])
            }
            .Where(stat => !string.IsNullOrWhiteSpace(stat.Value))
            .Select(stat => (stat.Label, stat.Value!))
            .ToList();

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

    private async Task CopyInstallCommandAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await SafeInvokeVoidAsync("copyToClipboard", InstallCommand, objRef);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] public IConfiguration Configuration { get; set; } = default!;

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
}
