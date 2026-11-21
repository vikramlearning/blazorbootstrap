using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// The agents' knowledge base list: every article, drafts included, with its category, status, last update, views, and
/// helpful votes, in a <see cref="Grid{TItem}" /> with paging, sorting, the column chooser, and CSV export, and a saved view
/// that can be reset. Filters by text, category, and status; each row opens the editor, and can be edited, published or
/// unpublished, viewed in the help center when published, or deleted after a confirmation that names the article and what
/// is lost.
/// </summary>
public partial class AgentArticles : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 10;

    private const string StatusAll = "all";

    private const string StatusDrafts = "drafts";

    private const string StatusPublished = "published";

    private string categoryId = string.Empty;

    private ElementReference countRegion;

    private bool deleted;

    private readonly Dictionary<int, Button> deleteButtons = new();

    private HelpDeskArticle? deleting;

    private Modal deleteModal = default!;

    private Grid<HelpDeskArticle>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private string? pageError;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private string search = string.Empty;

    private int statusCount;

    private string statusFilter = StatusAll;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive (it lives in the tab's session storage), then shows the grid.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        await view.LoadAsync();
        pageSize = (await view.GetSettingsAsync()).PageSize;
        viewLoaded = true;
        StateHasChanged();
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "kb-articles", DefaultPageSize, "updated", SortDirection.Descending);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string CountText(int shown, int total) =>
        total == 0 ? string.Empty
        : shown == total ? (total == 1 ? "1 article" : string.Create(CultureInfo.InvariantCulture, $"{total} articles"))
        : string.Create(CultureInfo.InvariantCulture, $"Showing {shown} of {total} articles");

    private async Task AskDeleteAsync(HelpDeskArticle article)
    {
        deleting = article;
        deleted = false;
        pageError = null;

        await deleteModal.ShowAsync();
    }

    private async Task ClearFiltersAsync()
    {
        search = string.Empty;
        categoryId = string.Empty;
        statusFilter = StatusAll;

        await FiltersChangedAsync();
        await HelpDeskDisplay.FocusAsync(countRegion);
    }

    private async Task ConfirmDeleteAsync()
    {
        if (deleting is { } article)
        {
            var result = State.DeleteArticle(article.Id);

            if (result == HelpDeskAdminResult.Ok)
            {
                deleted = true;
                SetStatus($"The article \"{article.Title}\" is deleted.");
            }
            else
            {
                pageError = result switch
                {
                    HelpDeskAdminResult.NotFound => $"The article \"{article.Title}\" couldn't be deleted: it's no longer in the knowledge base.",
                    HelpDeskAdminResult.NotAllowed => "Only agents and admins can delete articles. Sign in with a staff account.",
                    _ => $"The article \"{article.Title}\" couldn't be deleted. Try again.",
                };
            }
        }

        await deleteModal.HideAsync();
    }

    /// <summary>Says what a delete takes with it: the help center page (when published) and the article's votes.</summary>
    private string DeleteConsequence(HelpDeskArticle? article)
    {
        if (article is null)
            return string.Empty;

        var (helpful, notHelpful) = State.Votes(article.Id);
        var place = article.IsPublished
            ? "It's removed from the knowledge base and leaves the help center, so links to it stop working."
            : "The draft is removed from the knowledge base.";
        var votes = helpful + notHelpful == 0
            ? "Its \"Was this helpful?\" votes go with it (none so far)."
            : string.Create(CultureInfo.InvariantCulture, $"Its \"Was this helpful?\" votes are lost too: {helpful} yes, {notHelpful} no.");

        return $"{place} {votes} This can't be undone.";
    }

    private IReadOnlyList<HelpDeskArticle> Filtered(IReadOnlyList<HelpDeskArticle> articles)
    {
        var text = search.Trim();

        return articles
            .Where(article => text.Length == 0
                || article.Title.Contains(text, StringComparison.OrdinalIgnoreCase)
                || article.Summary.Contains(text, StringComparison.OrdinalIgnoreCase))
            .Where(article => categoryId.Length == 0 || article.CategoryId == categoryId)
            .Where(article => statusFilter switch
            {
                StatusPublished => article.IsPublished,
                StatusDrafts => !article.IsPublished,
                _ => true,
            })
            .ToList();
    }

    /// <summary>A filter changed: back to the first page of the filtered list.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    private async Task OnCategoryChanged(ChangeEventArgs args)
    {
        categoryId = args.Value?.ToString() ?? string.Empty;
        await FiltersChangedAsync();
    }

    /// <summary>The dialog opens from code, so Bootstrap does not return focus: after a delete the row is gone, so focus goes to the status.</summary>
    private async Task OnDeleteHiddenAsync()
    {
        var article = deleting;
        deleting = null;

        if (deleted)
        {
            deleted = false;
            await HelpDeskDisplay.FocusAsync(statusRegion);
        }
        else if (article is not null && deleteButtons.TryGetValue(article.Id, out var button))
        {
            await HelpDeskDisplay.FocusAsync(button.Element);
        }
    }

    private async Task OnSearchInput(ChangeEventArgs args)
    {
        search = args.Value?.ToString() ?? string.Empty;
        await FiltersChangedAsync();
    }

    /// <summary>An article changed (here or elsewhere): the grid reads the list again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnStatusFilterChanged(ChangeEventArgs args)
    {
        statusFilter = args.Value?.ToString() is StatusPublished or StatusDrafts ? args.Value.ToString()! : StatusAll;
        await FiltersChangedAsync();
    }

    /// <summary>The page of articles the grid asked for, from the filtered list (most recently updated first unless a column is sorted).</summary>
    private Task<GridDataProviderResult<HelpDeskArticle>> ProvideArticlesAsync(GridDataProviderRequest<HelpDeskArticle> request) =>
        view.ApplyAsync(request, Filtered(State.AllArticles));

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the article count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        SetStatus("The view is reset: most recently updated first, 10 articles a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await HelpDeskDisplay.FocusAsync(countRegion);
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    private void TogglePublished(HelpDeskArticle article)
    {
        var publish = !article.IsPublished;
        var result = State.SetArticlePublished(article.Id, publish);
        pageError = null;

        switch (result)
        {
            case HelpDeskAdminResult.Ok:
                SetStatus(publish
                    ? $"The article \"{article.Title}\" is published. Customers can find it in the help center."
                    : $"The article \"{article.Title}\" is unpublished. It's a draft again and no longer in the help center.");
                break;

            case HelpDeskAdminResult.NoChange:
                SetStatus(publish ? $"The article \"{article.Title}\" is already published." : $"The article \"{article.Title}\" is already a draft.");
                break;

            case HelpDeskAdminResult.NotFound:
                pageError = $"The article \"{article.Title}\" couldn't be changed: it's no longer in the knowledge base.";
                break;

            case HelpDeskAdminResult.NotAllowed:
                pageError = "Only agents and admins can publish articles. Sign in with a staff account.";
                break;

            default:
                pageError = $"The article \"{article.Title}\" couldn't be changed. Try again.";
                break;
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private bool FiltersActive => search.Trim().Length > 0 || categoryId.Length > 0 || statusFilter != StatusAll;

    private static string NewArticleUrl => $"{HelpDeskAgentLayout.Base}/kb/new";

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    [Inject] private HelpDeskState State { get; set; } = default!;

    #endregion
}
