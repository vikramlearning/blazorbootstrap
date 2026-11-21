using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// Manage projects (project managers): every project in a <see cref="Grid{TItem}" /> (key, name linking to its editor,
/// client, status in words, project manager, start and due dates, team size, budget, and tasks done as "x of y") with
/// paging, sorting, the column chooser, and CSV export, searched by key, name, or client and filtered by status (with its
/// counts) and project manager; Open board and Delete in each row. The grid's view is saved for the visit and can be reset.
/// Rows can be selected and changed together from a bulk bar: set a status, hand them to a project manager, or both
/// (<see cref="PbState.BulkUpdateProjects" />). A callout at the top lists the projects that came from a won deal and are
/// still planned with nobody on the team ("New from a won deal"), each with a "Plan it" link, and their rows say so too.
/// Delete opens a <see cref="Modal" /> that names the project and how many tasks go with it (the deal it came from stays
/// won); the deletion runs once the dialog is hidden, the outcome is announced in an always-rendered status region, and
/// focus returns to the Delete button, or to the status message when the row is gone.
/// </summary>
public partial class ManageProjects : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The route of this page.</summary>
    internal const string ListUrl = PbLayout.Base + "/manage/projects";

    /// <summary>The route of the new-project editor.</summary>
    internal const string NewUrl = ListUrl + "/new";

    private const int DefaultPageSize = 25;

    /// <summary>The Delete buttons by project ID, for focus after the dialog.</summary>
    private readonly Dictionary<int, ElementReference> deleteButtons = new();

    private string? bulkError;

    private int bulkErrors;

    private string bulkManager = string.Empty;

    private string bulkStatus = string.Empty;

    private ElementReference? countRegion;

    private Modal dialogModal = default!;

    /// <summary>The project the dialog is about.</summary>
    private int dialogProjectId;

    /// <summary>The project whose Delete button takes focus after the next render.</summary>
    private int? focusDeleteId;

    private bool focusStatus;

    private Grid<ProjectRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private string managerFilter = string.Empty;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>Whether Delete was confirmed; it runs once the dialog is hidden.</summary>
    private bool pendingDelete;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The projects selected in the grid (on its current page).</summary>
    private List<ProjectRow> selected = new();

    private int statusCount;

    private string statusFilter = string.Empty;

    private bool statusIsError;

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

        if (focusDeleteId is { } id)
        {
            focusDeleteId = null;

            if (deleteButtons.TryGetValue(id, out var button) && State.FindProject(id) is not null)
                await PbDisplay.FocusAsync(button);
            else
                focusStatus = true;
        }

        if (focusStatus)
        {
            focusStatus = false;
            await PbDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "manage-projects", DefaultPageSize, null, SortDirection.None);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns a project's client name, or "Internal (no client)".</summary>
    internal static string ClientName(PbState state, int? clientId) => state.FindClient(clientId)?.Name ?? (clientId is null ? "Internal (no client)" : "Unknown client");

    /// <summary>Returns the route of a project's editor.</summary>
    internal static string EditUrl(int projectId) => FormattableString.Invariant($"{ListUrl}/{projectId}/edit");

    /// <summary>
    /// Returns the projects a won deal created that still need planning: planned, with nobody on the team. These are what
    /// an account manager hands to the project managers.
    /// </summary>
    internal static IReadOnlyList<PbProject> NewFromDeals(PbState state) =>
        state.Projects.Where(project => project.FromDealId is not null && project.Status == PbProjectStatus.Planned && project.MemberIds.Count == 0).ToList();

    /// <summary>Returns a share as a percentage from 0 to 100 (0 when there is nothing to count).</summary>
    internal static double Percent(int part, int whole) => whole <= 0 ? 0 : Math.Clamp(part * 100.0 / whole, 0, 100);

    /// <summary>Returns a count with its noun, like "1 task" or "3 tasks".</summary>
    internal static string Plural(int count, string noun, string? plural = null) =>
        string.Create(CultureInfo.InvariantCulture, $"{count} {(count == 1 ? noun : plural ?? noun + "s")}");

    /// <summary>Returns tasks done in words, like "3 of 5 tasks done" or "No tasks yet".</summary>
    internal static string ProgressText(int done, int total) =>
        total == 0 ? "No tasks yet" : string.Create(CultureInfo.InvariantCulture, $"{done} of {Plural(total, "task")} done");

    /// <summary>Returns the badge color of a project status (always shown with its name).</summary>
    internal static BadgeColor StatusBadge(PbProjectStatus status) => PbDisplay.ProjectStatusColor(status) switch
    {
        "success" => BadgeColor.Success,
        "info" => BadgeColor.Info,
        "warning" => BadgeColor.Warning,
        _ => BadgeColor.Secondary,
    };

    #endregion

    #region Private Methods

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private void Announce(bool ok, string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = !ok;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>
    /// Applies the bulk bar's choices to the selected projects in one <see cref="PbState.BulkUpdateProjects" /> call, then
    /// refreshes the grid and clears the selection; the bar closes, so focus moves to the announcement.
    /// </summary>
    private async Task ApplyBulkAsync()
    {
        PbProjectStatus? status = Enum.TryParse<PbProjectStatus>(bulkStatus, out var parsed) && Enum.IsDefined(parsed) ? parsed : null;
        string? managerId = bulkManager.Length == 0 ? null : bulkManager;

        if (status is null && managerId is null)
        {
            bulkErrors++;
            const string problem = "Choose a status, a project manager, or both to apply.";
            bulkError = bulkErrors == 1 ? problem : string.Create(CultureInfo.InvariantCulture, $"{problem} ({bulkErrors})");

            return;
        }

        bulkError = null;
        bulkErrors = 0;

        var outcome = State.BulkUpdateProjects(selected.Select(row => row.Id).ToList(), status, managerId);
        var message = outcome.Result switch
        {
            PbResult.Ok => $"Updated {Plural(outcome.Changed, "project")}."
                + (outcome.Unchanged > 0 ? $" {Plural(outcome.Unchanged, "project was", "projects were")} already like that, or no longer there." : string.Empty),
            PbResult.NoChange => "No projects changed: the selected projects already had those values.",
            PbResult.NotAllowed => "Only project managers can change projects. Nothing changed.",
            PbResult.NotFound => "That person no longer runs projects. Nothing changed.",
            _ => "Those changes couldn't be saved. Nothing changed.",
        };

        Announce(outcome.Result is PbResult.Ok or PbResult.NoChange, message);
        bulkStatus = bulkManager = string.Empty;

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<ProjectRow>();
        focusStatus = true;
        StateHasChanged();
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = statusFilter = managerFilter = string.Empty;
        await FiltersChangedAsync();
        await PbDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<ProjectRow>();
        bulkError = null;
        Announce(true, "Selection cleared.");
        focusStatus = true;
    }

    private async Task ConfirmDeleteAsync()
    {
        pendingDelete = true;
        await dialogModal.HideAsync();
    }

    /// <summary>Says what deleting a project takes with it.</summary>
    private string DeleteConsequence(PbProject project)
    {
        var tasks = State.TasksOf(project.Id);
        var open = tasks.Count(task => task.Status != PbTaskStatus.Done);
        var deal = State.FindDeal(project.FromDealId);
        var taskText = tasks.Count == 0
            ? "It has no tasks."
            : $"Its {Plural(tasks.Count, "task")} {(tasks.Count == 1 ? "is" : "are")} deleted too ({open} open), with their comments and checklists.";
        var milestoneText = project.Milestones.Count == 0 ? null : $" Its {Plural(project.Milestones.Count, "milestone")} {(project.Milestones.Count == 1 ? "goes" : "go")} with it.";
        var dealText = deal is null ? null : $" The deal “{deal.Title}” it came from stays won.";

        return taskText + milestoneText + dealText;
    }

    /// <summary>A filter changed: back to the first page of the filtered projects.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>Returns the projects that pass every filter (but the status filter when <paramref name="byStatus" /> is off), as grid rows.</summary>
    private IEnumerable<ProjectRow> Matching(bool byStatus = true)
    {
        var term = search.Trim();
        var fresh = NewFromDeals(State).Select(project => project.Id).ToHashSet();
        var progress = State.ProgressByProject();
        var clients = State.Clients.ToDictionary(client => client.Id);
        var names = State.People.ToDictionary(person => person.Id, person => person.Name);

        foreach (var project in State.Projects)
        {
            var client = project.ClientId is { } clientId ? clients.GetValueOrDefault(clientId) : null;
            var clientName = client?.Name ?? (project.ClientId is null ? "Internal (no client)" : "Unknown client");

            if (term.Length > 0 && !(project.Key.Contains(term, StringComparison.OrdinalIgnoreCase) || project.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || clientName.Contains(term, StringComparison.OrdinalIgnoreCase)))
                continue;

            if (byStatus && statusFilter.Length > 0 && project.Status.ToString() != statusFilter)
                continue;

            if (managerFilter.Length > 0 && project.OwnerId != managerFilter)
                continue;

            var (done, total) = progress.GetValueOrDefault(project.Id);
            var currency = client?.BillingCurrency ?? "USD";

            yield return new ProjectRow(
                project.Id,
                project.Key,
                project.Name,
                clientName,
                PbDisplay.ProjectStatusLabel(project.Status),
                project.Status,
                project.Status switch { PbProjectStatus.Active => 0, PbProjectStatus.Planned => 1, PbProjectStatus.OnHold => 2, _ => 3 },
                project.OwnerId,
                names.GetValueOrDefault(project.OwnerId) ?? "Nobody",
                project.StartDate,
                project.DueDate,
                project.MemberIds.Count,
                project.BudgetHours,
                project.BudgetHours == 0 && project.BudgetAmount == 0
                    ? "No budget"
                    : string.Create(CultureInfo.InvariantCulture, $"{project.BudgetHours:#,0} h · {currency} {project.BudgetAmount:#,0}"),
                Percent(done, total),
                ProgressText(done, total),
                fresh.Contains(project.Id));
        }
    }

    private void OnDialogHidden()
    {
        var id = dialogProjectId;

        if (!pendingDelete)
        {
            focusDeleteId = id;

            return;
        }

        pendingDelete = false;

        if (State.FindProject(id) is not { } project)
        {
            Announce(false, "That project was already deleted, so nothing changed.");
            focusStatus = true;

            return;
        }

        var taskCount = State.TasksOf(id).Count;
        var deal = State.FindDeal(project.FromDealId);
        var label = $"{project.Key} · {project.Name}";
        var result = State.DeleteProject(id);

        switch (result)
        {
            case PbResult.Ok:
                Announce(true, $"Deleted {label} and its {Plural(taskCount, "task")}." + (deal is null ? null : $" The deal “{deal.Title}” stays won."));
                deleteButtons.Remove(id);
                focusStatus = true;

                break;

            case PbResult.NotFound:
                Announce(false, "That project was already deleted, so nothing changed.");
                focusStatus = true;

                break;

            case PbResult.NotAllowed:
                Announce(false, "Only project managers can delete projects. Nothing was deleted.");
                focusStatus = true;

                break;

            case PbResult.WrongState:
                Announce(false, $"{label} has logged hours or invoices, so it keeps its history and can't be deleted. Set it to Completed instead.");
                focusDeleteId = id;

                break;

            default:
                Announce(false, $"{label} couldn't be deleted. Try again.");
                focusDeleteId = id;

                break;
        }
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

    private void OnSelectionChanged(HashSet<ProjectRow> items)
    {
        selected = items.ToList();

        if (selected.Count > 0)
            bulkError = null;
    }

    /// <summary>Another screen or a change here changed the state: the grid reads the projects again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OpenDeleteAsync(int projectId)
    {
        dialogProjectId = projectId;
        pendingDelete = false;
        await dialogModal.ShowAsync();
    }

    /// <summary>The page of projects the grid asked for: active first, then planned, on hold, and completed, by due date, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<ProjectRow>> ProvideProjectsAsync(GridDataProviderRequest<ProjectRow> request) => view.ApplyAsync(request, Shown);

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
        selected = new List<ProjectRow>();
        gridVersion++;
        Announce(true, "The view is reset: active projects first, then by due date, 25 a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await PbDisplay.FocusAsync(countRegion);
    }

    private string RowClass(ProjectRow row) => selected.Any(item => item.Id == row.Id) ? "tb-row-selected" : string.Empty;

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    private int StatusCount(PbProjectStatus? status) => Matching(byStatus: false).Count(row => status is null || row.StatusValue == status);

    #endregion

    #endregion

    #region Properties, Indexers

    private PbProject? DialogProject => State.FindProject(dialogProjectId);

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || statusFilter.Length > 0 || managerFilter.Length > 0;

    private IReadOnlyList<ProjectRow> Shown => Matching().ToList();

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One project as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record ProjectRow(
        int Id,
        string Key,
        string Name,
        string Client,
        string Status,
        PbProjectStatus StatusValue,
        int StatusOrder,
        string ManagerId,
        string Manager,
        DateTime Start,
        DateTime Due,
        int Team,
        int BudgetHours,
        string BudgetText,
        double Percent,
        string ProgressText,
        bool IsFresh)
    {
        /// <summary>Gets the due date in words.</summary>
        public string DueText => PbDisplay.Date(Due);

        /// <summary>Gets the start date in words.</summary>
        public string StartText => PbDisplay.Date(Start);
    }

    #endregion
}
