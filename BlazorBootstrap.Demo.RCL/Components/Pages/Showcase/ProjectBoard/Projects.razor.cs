using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// Project boards: every project (<see cref="PbState.Projects" />) in a <see cref="Grid{TItem}" /> with its key, name (a
/// stretched link to its board, so the whole row opens it), client, status in words, project manager, due date, progress
/// (<see cref="PbState.ProjectProgress" /> as a <see cref="Progress" /> and "12 of 20 tasks done"), the signed-in person's
/// open tasks there, the next milestone, and the team's initials (names in <c>title</c> and visually hidden text). Paging,
/// sorting, the column chooser, and CSV export; searched by key, name, or client, and filtered by status and by the
/// projects the person is on. The grid's view is saved for the visit and can be reset.
/// </summary>
public partial class Projects : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private const string ResetText = "The view is reset: active projects first, then by due date, 25 a page, and every column shown.";

    private ElementReference? countRegion;

    private Grid<BoardRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>Whether only the projects the signed-in person is on are shown.</summary>
    private bool mineOnly;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    private string statusFilter = string.Empty;

    private string? statusMessage;

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
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "projects", DefaultPageSize, null, SortDirection.None);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = statusFilter = string.Empty;
        mineOnly = false;
        await FiltersChangedAsync();
        await PbDisplay.FocusAsync(countRegion);
    }

    /// <summary>A filter changed: back to the first page of the filtered projects.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>Returns the projects that pass every filter (but the status filter when <paramref name="byStatus" /> is off), as grid rows.</summary>
    private IEnumerable<BoardRow> Matching(bool byStatus = true)
    {
        var term = search.Trim();
        var today = State.Clock.Today;
        var me = State.CurrentPerson;
        var progress = State.ProgressByProject();
        var clients = State.Clients.ToDictionary(client => client.Id, client => client.Name);
        var people = State.People.ToDictionary(person => person.Id);
        var mine = me is null
            ? new Dictionary<int, int>()
            : State.AllTasks.Where(task => task.AssigneeId == me.Id && task.Status != PbTaskStatus.Done).GroupBy(task => task.ProjectId).ToDictionary(group => group.Key, group => group.Count());

        foreach (var project in State.Projects)
        {
            var client = project.ClientId is { } clientId ? clients.GetValueOrDefault(clientId) ?? "Unknown client" : "Internal project";

            if (term.Length > 0 && !(project.Key.Contains(term, StringComparison.OrdinalIgnoreCase) || project.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || client.Contains(term, StringComparison.OrdinalIgnoreCase)))
                continue;

            if (byStatus && statusFilter.Length > 0 && project.Status.ToString() != statusFilter)
                continue;

            if (mineOnly && (me is null || !(project.MemberIds.Contains(me.Id) || project.OwnerId == me.Id)))
                continue;

            var (done, total) = progress.GetValueOrDefault(project.Id);
            var next = project.Milestones.Where(milestone => milestone.Due.Date >= today).OrderBy(milestone => milestone.Due).FirstOrDefault();
            var team = project.MemberIds.Select(id => people.GetValueOrDefault(id)).OfType<PbPerson>().OrderBy(person => person.Name, StringComparer.OrdinalIgnoreCase).ToList();

            yield return new BoardRow(
                project.Id,
                project.Key,
                project.Name,
                client,
                PbDisplay.ProjectStatusLabel(project.Status),
                project.Status,
                project.Status switch { PbProjectStatus.Active => 0, PbProjectStatus.Planned => 1, PbProjectStatus.OnHold => 2, _ => 3 },
                people.GetValueOrDefault(project.OwnerId)?.Name ?? "Nobody",
                project.DueDate,
                ManageProjects.Percent(done, total),
                total == 0 ? "No tasks yet" : $"{done} of {TaskCard.Plural(total, "task")} done",
                mine.GetValueOrDefault(project.Id),
                next is null ? "None coming up" : $"{next.Title}, {PbDisplay.Date(next.Due)}",
                next?.Due,
                team);
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

    /// <summary>Another screen changed the state: the grid reads the projects again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>The page of projects the grid asked for: active first, then planned, on hold, and completed, by due date, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<BoardRow>> ProvideProjectsAsync(GridDataProviderRequest<BoardRow> request) => view.ApplyAsync(request, Shown);

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
        gridVersion++;
        statusMessage = statusMessage == ResetText ? ResetText + " (2)" : ResetText;
        StateHasChanged();

        await Task.Yield();
        await PbDisplay.FocusAsync(countRegion);
    }

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

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || statusFilter.Length > 0 || mineOnly;

    private IReadOnlyList<BoardRow> Shown => Matching().ToList();

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One project as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record BoardRow(
        int Id,
        string Key,
        string Name,
        string Client,
        string Status,
        PbProjectStatus StatusValue,
        int StatusOrder,
        string Manager,
        DateTime Due,
        double Percent,
        string ProgressText,
        int MyOpenTasks,
        string NextMilestone,
        DateTime? NextMilestoneDue,
        IReadOnlyList<PbPerson> Team)
    {
        /// <summary>Gets the due date in words.</summary>
        public string DueText => PbDisplay.Date(Due);

        /// <summary>Gets the number of people on the team.</summary>
        public int TeamCount => Team.Count;

        /// <summary>Gets the team's names, as exported.</summary>
        public string TeamText => string.Join(", ", Team.Select(person => person.Name));
    }

    #endregion
}
