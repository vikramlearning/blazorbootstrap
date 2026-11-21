using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// The shared prompt library of the Parley back office (prompt editors, Admins, and the Owner;
/// <see cref="AiState.AllSharedPrompts" />) in a <see cref="Grid{TItem}" />: title (the row opens the editor,
/// <see cref="AdminPromptEdit" />), published or draft, category, variables, tags, author, uses, and the last change, the
/// most recently changed first, with a saved view, search (title, author, or tag), status and category filters, sorting,
/// paging, the column chooser, and CSV export. The bulk bar publishes or unpublishes the selection
/// (<see cref="AiState.BulkSetPublished" />) or deletes it after a confirmation (<see cref="AiState.BulkDeletePrompts" />).
/// Outcomes are announced in the status region, which takes focus when the bar closes. The routes of the editor are shared
/// from here.
/// </summary>
public partial class AdminPrompts : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private const string DraftValue = "draft";

    private const string PublishedValue = "published";

    private string categoryFilter = string.Empty;

    private ElementReference? countRegion;

    private Modal deleteModal = default!;

    private int errorCount;

    private string? errorMessage;

    private string? errorText;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private Grid<PromptRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>Whether Delete was confirmed; it runs once the dialog is hidden.</summary>
    private bool pendingDelete;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The prompts selected in the grid (on its current page).</summary>
    private List<PromptRow> selected = new();

    private int statusCount;

    private string statusFilter = string.Empty;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive (it lives in the tab's session storage), then shows the grid.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await view.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }

        if (focusTarget is { } target)
        {
            focusTarget = null;
            await AiDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "shared-prompts", DefaultPageSize, "updated", SortDirection.Descending);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the route of a shared prompt's editor.</summary>
    internal static string EditUrl(int promptId) => AiSeedData.SharedPromptPath(promptId);

    #endregion

    #region Private Methods

    private async Task AskDeleteAsync()
    {
        ClearError();
        pendingDelete = false;
        await deleteModal.ShowAsync();
    }

    /// <summary>Says what a bulk action did.</summary>
    private void AnnounceBulk(AiBulkOutcome outcome, string done, string same)
    {
        var unchanged = outcome.Unchanged > 0 ? $" {AdminMembers.Plural(outcome.Unchanged, "prompt was", "prompts were")} {same}, and left as they were." : null;

        switch (outcome.Result)
        {
            case AiResult.Ok:
                SetStatus(done + unchanged);
                break;

            case AiResult.NoChange:
                SetStatus("No prompts changed." + unchanged);
                break;

            case AiResult.NotAllowed:
                ShowError("Only prompt editors, Admins, and the Owner manage shared prompts. Nothing changed.");
                break;

            default:
                ShowError("That didn't work, and nothing changed. Try again.");
                break;
        }
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = string.Empty;
        statusFilter = categoryFilter = string.Empty;
        await FiltersChangedAsync();
        await AiDisplay.FocusAsync(countRegion);
    }

    private void ClearError()
    {
        errorMessage = errorText = null;
        errorCount = 0;
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<PromptRow>();
        ClearError();
        SetStatus("Selection cleared.");
        focusTarget = () => statusRegion;
    }

    private async Task ConfirmDeleteAsync()
    {
        pendingDelete = true;
        await deleteModal.HideAsync();
    }

    private string CountText(int total)
    {
        var shown = Shown.Count;

        return shown == total ? $"{AdminMembers.Plural(total, "shared prompt")}." : $"Showing {shown.ToString(CultureInfo.InvariantCulture)} of {AdminMembers.Plural(total, "shared prompt")}.";
    }

    /// <summary>A filter changed: back to the first page of the filtered prompts.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>A bulk action ended: the selection is cleared, and focus moves to the announcement.</summary>
    private async Task FinishBulkAsync()
    {
        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<PromptRow>();
        focusTarget = () => statusRegion;
        StateHasChanged();
    }

    private async Task OnCategoryFilterChangedAsync(ChangeEventArgs args)
    {
        categoryFilter = Enum.TryParse<AiPromptCategory>(args.Value?.ToString(), out var category) ? category.ToString() : string.Empty;
        await FiltersChangedAsync();
    }

    /// <summary>Deletes the selection once the dialog is hidden (or returns focus to the bar when cancelled).</summary>
    private async Task OnDeleteHiddenAsync()
    {
        if (!pendingDelete)
        {
            focusTarget = () => statusRegion;

            return;
        }

        pendingDelete = false;
        var outcome = State.BulkDeletePrompts(selected.Select(row => row.Id).ToList());
        AnnounceBulk(outcome, $"Deleted {AdminMembers.Plural(outcome.Changed, "shared prompt")}. Members no longer see {(outcome.Changed == 1 ? "it" : "them")}.", "deleted meanwhile");
        await FinishBulkAsync();
    }

    private void OnSearchChanged(ChangeEventArgs args) => searchBoxValue = search = args.Value?.ToString() ?? string.Empty;

    /// <summary>Filters on every keystroke without changing the box's bound value, so a slower render never overwrites newer typing.</summary>
    private async Task OnSearchInputAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (value == search)
            return;

        search = value;
        await FiltersChangedAsync();
    }

    private void OnSelectionChanged(HashSet<PromptRow> items)
    {
        selected = items.ToList();

        if (selected.Count > 0)
            ClearError();
    }

    /// <summary>Another screen or a change here changed the state: the grid reads the prompts again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnStatusFilterChangedAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString();
        statusFilter = value is PublishedValue or DraftValue ? value : string.Empty;
        await FiltersChangedAsync();
    }

    /// <summary>The page of prompts the grid asked for: the most recently changed first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<PromptRow>> ProvidePromptsAsync(GridDataProviderRequest<PromptRow> request) => view.ApplyAsync(request, Shown);

    /// <summary>Publishes or unpublishes the selection in one <see cref="AiState.BulkSetPublished" /> call.</summary>
    private async Task PublishAsync(bool publish)
    {
        ClearError();
        var outcome = State.BulkSetPublished(selected.Select(row => row.Id).ToList(), publish);
        AnnounceBulk(outcome,
            publish
                ? $"Published {AdminMembers.Plural(outcome.Changed, "shared prompt")}: every member sees {(outcome.Changed == 1 ? "it" : "them")} in their prompt library, and hears about it."
                : $"Unpublished {AdminMembers.Plural(outcome.Changed, "shared prompt")}: {(outcome.Changed == 1 ? "it's a draft" : "they're drafts")} that only editors see.",
            publish ? "already published" : "already drafts");
        await FinishBulkAsync();
    }

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        selected = new List<PromptRow>();
        gridVersion++;
        SetStatus("The view is reset: the most recently changed first, 25 a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await AiDisplay.FocusAsync(countRegion);
    }

    private string RowClass(PromptRow row) => selected.Any(item => item.Id == row.Id) ? "pa-row-selected" : string.Empty;

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

    /// <summary>Shows a refusal in the page's alert; the same one again gets a counter, so it is announced again.</summary>
    private void ShowError(string message)
    {
        errorCount = message == errorText ? errorCount + 1 : 1;
        errorText = message;
        errorMessage = errorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({errorCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || statusFilter.Length > 0 || categoryFilter.Length > 0;

    /// <summary>Gets the route of the shared prompt list.</summary>
    internal static string ListUrl => $"{AiAdminLayout.Base}/prompts";

    /// <summary>Gets the route of the new shared prompt editor.</summary>
    internal static string NewUrl => $"{ListUrl}/new";

    /// <summary>The shared prompts that pass the filters, as grid rows, the most recently changed first.</summary>
    private IReadOnlyList<PromptRow> Shown
    {
        get
        {
            var term = search.Trim();

            return State.AllSharedPrompts
                .Where(prompt => statusFilter.Length == 0 || prompt.IsPublished == (statusFilter == PublishedValue))
                .Where(prompt => categoryFilter.Length == 0 || prompt.Category.ToString() == categoryFilter)
                .Where(prompt => term.Length == 0 || prompt.Title.Contains(term, StringComparison.OrdinalIgnoreCase) || prompt.Author.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || prompt.Tags.Any(tag => tag.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .Select(prompt => new PromptRow(
                    prompt.Id,
                    prompt.Title,
                    prompt.IsPublished,
                    prompt.IsPublished ? "Published" : "Draft",
                    prompt.Category.ToString(),
                    string.Join(", ", prompt.Variables),
                    string.Join(", ", prompt.Tags),
                    prompt.Author,
                    prompt.UseCount,
                    prompt.UpdatedAt,
                    AiDisplay.Ago(prompt.UpdatedAt, State.Clock.Now)))
                .ToList();
        }
    }

    [Inject] private AiState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One shared prompt as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record PromptRow(
        int Id,
        string Title,
        bool IsPublished,
        string Status,
        string Category,
        string Variables,
        string Tags,
        string Author,
        int Uses,
        DateTime Updated,
        string UpdatedText);

    #endregion
}
