using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// The prompt library of a member in a <see cref="Grid{TItem}" />: their own prompts (<see cref="AiState.MyPrompts" />) and
/// the workspace's published shared prompts (<see cref="AiState.SharedPrompts" />, read-only here), with title (the row opens
/// the prompt: the editor of one's own, the page of a shared one), whose, category, variables, tags, author, uses, the last
/// change, and a "Use" link that starts a chat with it. A saved view, search (title, text, or tag), "whose" and category
/// filters, sorting, paging, the column chooser, and CSV export. The bulk bar deletes the member's own prompts in the
/// selection after a confirmation (<see cref="AiState.BulkDeletePrompts" />). The status region reports every outcome and a
/// save from the editor (<see cref="Saved" />, <see cref="Added" />).
/// </summary>
public partial class Prompts : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The route of the prompt library.</summary>
    public const string ListUrl = AiLayout.Base + "/prompts";

    /// <summary>The route of the new-prompt editor.</summary>
    public const string NewUrl = AiLayout.Base + "/prompts/new";

    private const int DefaultPageSize = 25;

    private const string MineValue = "mine";

    private const string SharedValue = "shared";

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

    private string scopeFilter = string.Empty;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The prompts selected in the grid (on its current page).</summary>
    private List<PromptRow> selected = new();

    private int statusCount;

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
        view = GridViews.Get(State.SiteSlug, "prompt-library", DefaultPageSize, "updated", SortDirection.Descending);
        State.Changed += OnStateChanged;

        if (Saved is { } id && State.FindPrompt(id) is { } prompt)
            SetStatus(Added == 1
                ? $"\"{prompt.Title}\" is saved to your prompts. Use it in chat whenever you need it."
                : $"Your changes to \"{prompt.Title}\" are saved.");
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the chat route that starts with a saved prompt in the composer.</summary>
    internal static string ChatUrl(int promptId) => string.Create(CultureInfo.InvariantCulture, $"{AiLayout.Base}/chat?prompt={promptId}");

    /// <summary>Returns how often a prompt was used, in words.</summary>
    internal static string UseText(int count) => count switch
    {
        0 => "not used yet",
        1 => "used once",
        _ => string.Create(CultureInfo.InvariantCulture, $"used {count:#,0} times"),
    };

    #endregion

    #region Private Methods

    private async Task AskDeleteAsync()
    {
        ClearError();
        pendingDelete = false;
        await deleteModal.ShowAsync();
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = string.Empty;
        scopeFilter = categoryFilter = string.Empty;
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

        return shown == total ? $"{AdminMembers.Plural(total, "prompt")} you can use." : $"Showing {shown.ToString(CultureInfo.InvariantCulture)} of {AdminMembers.Plural(total, "prompt")}.";
    }

    /// <summary>A filter changed: back to the first page of the filtered prompts.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    private async Task OnCategoryFilterChangedAsync(ChangeEventArgs args)
    {
        categoryFilter = Enum.TryParse<AiPromptCategory>(args.Value?.ToString(), out var category) ? category.ToString() : string.Empty;
        await FiltersChangedAsync();
    }

    /// <summary>Deletes the member's own prompts in the selection once the dialog is hidden, and says what happened.</summary>
    private async Task OnDeleteHiddenAsync()
    {
        if (!pendingDelete)
        {
            focusTarget = () => statusRegion;

            return;
        }

        pendingDelete = false;
        var shared = selected.Count(row => !row.IsMine);
        var outcome = State.BulkDeletePrompts(selected.Where(row => row.IsMine).Select(row => row.Id).ToList());

        switch (outcome.Result)
        {
            case AiResult.Ok:
                SetStatus($"Deleted {AdminMembers.Plural(outcome.Changed, "of your prompts", "of your prompts")}."
                    + (outcome.Unchanged > 0 ? $" {AdminMembers.Plural(outcome.Unchanged, "was", "were")} deleted already." : null)
                    + (shared > 0 ? $" {AdminMembers.Plural(shared, "shared prompt stays", "shared prompts stay")}: only their editors can delete them." : null));
                break;

            case AiResult.NoChange:
                SetStatus("Those prompts were deleted already, so nothing changed.");
                break;

            case AiResult.Invalid:
                SetStatus("None of the selected prompts are yours, so nothing was deleted.");
                break;

            case AiResult.NotAllowed:
                ShowError("Your membership isn't active any more, so nothing was deleted.");
                break;

            default:
                ShowError("That didn't work, and nothing was deleted. Try again.");
                break;
        }

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<PromptRow>();
        focusTarget = () => statusRegion;
    }

    private async Task OnScopeFilterChangedAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString();
        scopeFilter = value is MineValue or SharedValue ? value : string.Empty;
        await FiltersChangedAsync();
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

    /// <summary>The page of prompts the grid asked for: the most recently changed first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<PromptRow>> ProvidePromptsAsync(GridDataProviderRequest<PromptRow> request) => view.ApplyAsync(request, Shown);

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

    /// <summary>Gets or sets <c>1</c> when the prompt in <see cref="Saved" /> was just created (rather than edited).</summary>
    [SupplyParameterFromQuery(Name = "added")] public int? Added { get; set; }

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || scopeFilter.Length > 0 || categoryFilter.Length > 0;

    /// <summary>How many of the selected prompts are the member's own.</summary>
    private int OwnSelected => selected.Count(row => row.IsMine);

    /// <summary>Gets or sets the prompt the editor just saved, so the status region says so.</summary>
    [SupplyParameterFromQuery(Name = "saved")] public int? Saved { get; set; }

    /// <summary>The prompts that pass the filters, as grid rows: the member's own, then the shared ones, the most recently changed first.</summary>
    private IReadOnlyList<PromptRow> Shown
    {
        get
        {
            var term = search.Trim();

            return State.MyPrompts.Select(prompt => (Prompt: prompt, IsMine: true))
                .Concat(State.SharedPrompts.Select(prompt => (Prompt: prompt, IsMine: false)))
                .Where(pair => scopeFilter.Length == 0 || pair.IsMine == (scopeFilter == MineValue))
                .Where(pair => categoryFilter.Length == 0 || pair.Prompt.Category.ToString() == categoryFilter)
                .Where(pair => term.Length == 0 || pair.Prompt.Title.Contains(term, StringComparison.OrdinalIgnoreCase) || pair.Prompt.Text.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || pair.Prompt.Tags.Any(tag => tag.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(pair => pair.Prompt.UpdatedAt)
                .Select(pair => new PromptRow(
                    pair.Prompt.Id,
                    pair.Prompt.Title,
                    pair.IsMine,
                    pair.IsMine ? "Mine" : "Shared",
                    pair.Prompt.Category.ToString(),
                    string.Join(", ", pair.Prompt.Variables),
                    string.Join(", ", pair.Prompt.Tags),
                    pair.Prompt.Author,
                    pair.Prompt.UseCount,
                    pair.Prompt.UpdatedAt,
                    AiDisplay.Ago(pair.Prompt.UpdatedAt, State.Clock.Now)))
                .ToList();
        }
    }

    [Inject] private AiState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One prompt as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record PromptRow(
        int Id,
        string Title,
        bool IsMine,
        string Whose,
        string Category,
        string Variables,
        string Tags,
        string Author,
        int Uses,
        DateTime Updated,
        string UpdatedText);

    #endregion
}
