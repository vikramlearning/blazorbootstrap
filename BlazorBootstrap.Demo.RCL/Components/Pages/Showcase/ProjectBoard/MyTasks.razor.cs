using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// My tasks: the signed-in person's tasks (<see cref="PbState.MyTasks" />) in a <see cref="Grid{TItem}" /> (key, title
/// linking to the task, project, status and priority in words, due date in words with overdue marked, estimate, checklist
/// progress, and Mark done or Reopen) with paging, sorting, the column chooser, and CSV export. A summary at the top counts
/// what is overdue, due today, this week, later, and without a date; the Due filter shows one of those groups, all open
/// tasks (the default), or the ones done in the last two weeks, and a search narrows by key, title, or project. The grid's
/// view is saved for the visit and can be reset. "Mark done" moves a task to the end of Done and "Reopen" back to the end of
/// To do (<see cref="PbState.MoveTask" />). Selected tasks can be marked done, given a new due date (or none), or reassigned
/// together (<see cref="PbState.BulkCompleteTasks" />, <see cref="PbState.BulkSetDueDate" />,
/// <see cref="PbState.BulkReassignTasks" />). Outcomes are announced in the status region, which takes focus because rows
/// can leave the list.
/// </summary>
public partial class MyTasks : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    /// <summary>The Due filter's value for every open task.</summary>
    private const string OpenFilter = "open";

    /// <summary>The bulk reassign value that unassigns the tasks.</summary>
    private const string UnassignValue = "nobody";

    private string bulkAssignee = string.Empty;

    private string bulkDueText = string.Empty;

    private string? bulkError;

    private int bulkErrors;

    private ElementReference? countRegion;

    private string dueFilter = OpenFilter;

    private bool focusStatus;

    private Grid<TaskRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The tasks selected in the grid (on its current page).</summary>
    private List<TaskRow> selected = new();

    private int statusCount;

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

        if (focusStatus)
        {
            focusStatus = false;
            await PbDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "my-tasks", DefaultPageSize, null, SortDirection.None);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Plural(int count, string one, string many) => count == 1 ? $"1 {one}" : string.Create(CultureInfo.InvariantCulture, $"{count} {many}");

    /// <summary>", 2 overdue, 1 due today, and 3 due this week" for the groups that have tasks.</summary>
    private static string SummaryText(IReadOnlyList<TaskGroup> groups)
    {
        var parts = groups.Where(group => !group.IsDone && group.Count > 0).Select(group => $"{group.Count} {group.SummaryWords}").ToList();

        return parts.Count switch
        {
            0 => string.Empty,
            1 => $": {parts[0]}",
            _ => $": {string.Join(", ", parts.Take(parts.Count - 1))}, and {parts[^1]}",
        };
    }

    /// <summary>Marks the selected tasks done in one <see cref="PbState.BulkCompleteTasks" /> call.</summary>
    private async Task BulkCompleteAsync()
    {
        var outcome = State.BulkCompleteTasks(selected.Select(row => row.Id).ToList());

        await FinishBulkAsync(outcome, $"Marked {Plural(outcome.Changed, "task", "tasks")} done. They're under Done in the last 2 weeks, where you can reopen them.",
            "No tasks changed: the selected tasks are already done.", "already done, or on a completed project");
    }

    /// <summary>Sets or clears the selected tasks' due date in one <see cref="PbState.BulkSetDueDate" /> call.</summary>
    private async Task BulkDueAsync()
    {
        DateTime? due = null;

        if (bulkDueText.Trim().Length > 0)
        {
            if (!DateTime.TryParseExact(bulkDueText.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) || parsed < MinDate || parsed > MaxDate)
            {
                ShowBulkError($"Choose a due date from {PbDisplay.Date(MinDate)} to {PbDisplay.Date(MaxDate)}, or leave it empty to clear the due dates.");

                return;
            }

            due = parsed;
        }

        var outcome = State.BulkSetDueDate(selected.Select(row => row.Id).ToList(), due);
        bulkDueText = string.Empty;

        await FinishBulkAsync(outcome,
            due is { } date ? $"Moved {Plural(outcome.Changed, "task", "tasks")} to {PbDisplay.Date(date)}." : $"Cleared the due date of {Plural(outcome.Changed, "task", "tasks")}.",
            "No tasks changed: the selected tasks already have that due date, are done, or their project's dates don't include it.",
            "already due then, done, or outside their project's dates");
    }

    /// <summary>Reassigns the selected tasks in one <see cref="PbState.BulkReassignTasks" /> call.</summary>
    private async Task BulkReassignAsync()
    {
        if (bulkAssignee.Length == 0)
        {
            ShowBulkError("Choose who to reassign the selected tasks to, or Nobody to unassign them.");

            return;
        }

        string? personId = bulkAssignee == UnassignValue ? null : bulkAssignee;
        var name = personId is null ? null : State.FindPerson(personId)?.Name ?? "that person";
        var outcome = State.BulkReassignTasks(selected.Select(row => row.Id).ToList(), personId);
        bulkAssignee = string.Empty;

        await FinishBulkAsync(outcome,
            name is null ? $"Unassigned {Plural(outcome.Changed, "task", "tasks")}." : $"Reassigned {Plural(outcome.Changed, "task", "tasks")} to {name}.",
            name is null ? "No tasks changed: the selected tasks are done or read-only." : $"No tasks changed: {name} isn't on those projects, or the tasks are done.",
            name is null ? "done or read-only" : $"on projects {name} isn't on, or done");
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = string.Empty;
        dueFilter = OpenFilter;
        await FiltersChangedAsync();
        await PbDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<TaskRow>();
        bulkError = null;
        SetStatus("Selection cleared.", false);
        focusStatus = true;
    }

    /// <summary>A filter changed: back to the first page of the filtered tasks.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>Announces a bulk change, refreshes the grid, and clears the selection; the bar closes, so focus moves to the announcement.</summary>
    private async Task FinishBulkAsync(PbBulkOutcome outcome, string changedText, string noChangeText, string unchangedReason)
    {
        bulkError = null;
        bulkErrors = 0;

        var message = outcome.Result switch
        {
            PbResult.Ok => changedText + (outcome.Unchanged > 0 ? $" {Plural(outcome.Unchanged, "task was", "tasks were")} left as they were: {unchangedReason}." : string.Empty),
            PbResult.NoChange => noChangeText,
            PbResult.NotAllowed => "Your sign-in ended. Sign in again to update tasks. Nothing changed.",
            PbResult.NotFound => "That person isn't on the team any more. Nothing changed.",
            _ => "Those changes couldn't be saved. Nothing changed.",
        };

        SetStatus(message, outcome.Result is not (PbResult.Ok or PbResult.NoChange));

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<TaskRow>();
        focusStatus = true;
        StateHasChanged();
    }

    /// <summary>The groups the Due filter offers, in order, with their counts; the last one is the tasks done in the last two weeks.</summary>
    private IReadOnlyList<TaskGroup> Groups(DateTime today)
    {
        var mine = State.MyTasks;

        return new[]
        {
            new TaskGroup("overdue", "Overdue", "overdue", false, mine.Count(task => GroupOf(task, today) == "overdue")),
            new TaskGroup("today", "Today", "due today", false, mine.Count(task => GroupOf(task, today) == "today")),
            new TaskGroup("week", "This week", "due later this week", false, mine.Count(task => GroupOf(task, today) == "week")),
            new TaskGroup("later", "Later", "due later", false, mine.Count(task => GroupOf(task, today) == "later")),
            new TaskGroup("none", "No due date", "without a due date", false, mine.Count(task => GroupOf(task, today) == "none")),
            new TaskGroup("done", "Done in the last 2 weeks", "done", true, mine.Count(task => task.Status == PbTaskStatus.Done)),
        };
    }

    /// <summary>Which Due group a task is in: overdue, today, this week (to Sunday), later, none, or done.</summary>
    private static string GroupOf(PbTask task, DateTime today)
    {
        if (task.Status == PbTaskStatus.Done)
            return "done";

        var sunday = PbState.WeekStart(today).AddDays(6);

        return task.DueDate is not { } due ? "none" : due.Date < today ? "overdue" : due.Date == today ? "today" : due.Date <= sunday ? "week" : "later";
    }

    /// <summary>Returns the person's tasks that pass the filters, as grid rows, in the order <see cref="PbState.MyTasks" /> gives them.</summary>
    private IEnumerable<TaskRow> Matching()
    {
        var today = State.Clock.Today;
        var term = search.Trim();
        var projects = State.Projects.ToDictionary(project => project.Id);

        foreach (var task in State.MyTasks)
        {
            var group = GroupOf(task, today);

            if (dueFilter == OpenFilter ? group == "done" : group != dueFilter)
                continue;

            var project = projects.GetValueOrDefault(task.ProjectId);
            var key = string.Create(CultureInfo.InvariantCulture, $"{project?.Key ?? "?"}-{task.Number}");

            if (term.Length > 0 && !(key.Contains(term, StringComparison.OrdinalIgnoreCase) || task.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || (project?.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)))
                continue;

            var isDone = task.Status == PbTaskStatus.Done;

            yield return new TaskRow(
                task.Id,
                key,
                task.Title,
                project?.Name ?? "Unknown project",
                PbDisplay.StatusLabel(task.Status),
                task.Status,
                task.Priority.ToString(),
                task.Priority,
                task.DueDate,
                isDone && task.CompletedAt is { } completed ? $"Done {completed.ToString("MMM d", CultureInfo.InvariantCulture)}" : PbDisplay.Due(task.DueDate, today),
                PbDisplay.IsOverdue(task, today),
                task.EstimateHours,
                task.Checklist.Count == 0 ? "None" : string.Create(CultureInfo.InvariantCulture, $"{task.Checklist.Count(item => item.Done)} of {task.Checklist.Count} done"),
                isDone);
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

    private void OnSelectionChanged(HashSet<TaskRow> items)
    {
        selected = items.ToList();

        if (selected.Count > 0)
            bulkError = null;
    }

    /// <summary>Another screen or a change here changed the state: the grid reads the tasks again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>The page of tasks the grid asked for: open ones by due date, then the done ones, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<TaskRow>> ProvideTasksAsync(GridDataProviderRequest<TaskRow> request) => view.ApplyAsync(request, Shown);

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
        selected = new List<TaskRow>();
        gridVersion++;
        SetStatus("The view is reset: by due date, 25 tasks a page, and every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await PbDisplay.FocusAsync(countRegion);
    }

    private string RowClass(TaskRow row) => selected.Any(item => item.Id == row.Id) ? "tb-row-selected" : string.Empty;

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message, bool isError)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = isError;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>Shows a bulk bar problem; the same one again gets a counter, so it is announced again.</summary>
    private void ShowBulkError(string problem)
    {
        bulkErrors = bulkError?.StartsWith(problem, StringComparison.Ordinal) == true ? bulkErrors + 1 : 1;
        bulkError = bulkErrors == 1 ? problem : string.Create(CultureInfo.InvariantCulture, $"{problem} ({bulkErrors})");
    }

    /// <summary>Marks an open task done (end of Done) or reopens a done one (end of To do).</summary>
    private async Task ToggleDoneAsync(int taskId)
    {
        if (State.FindTask(taskId) is not { } task)
        {
            SetStatus("That task was deleted, so nothing changed.", true);
            focusStatus = true;

            return;
        }

        var key = State.TaskKey(task);
        var reopen = task.Status == PbTaskStatus.Done;
        var target = reopen ? PbTaskStatus.Todo : PbTaskStatus.Done;
        var result = State.MoveTask(task.Id, target, State.TasksIn(task.ProjectId, target).Count);

        SetStatus(result switch
        {
            PbResult.Ok when reopen => $"Reopened {key} “{task.Title}”: it's back in To do, with your open tasks.",
            PbResult.Ok => $"Marked {key} “{task.Title}” done. It's under Done in the last 2 weeks, where you can reopen it.",
            PbResult.NoChange => reopen ? $"{key} is already open, so nothing changed." : $"{key} is already done, so nothing changed.",
            PbResult.NotFound => $"{key} was deleted, so nothing changed.",
            PbResult.NotAllowed => "Your sign-in ended. Sign in again to update tasks. Nothing changed.",
            PbResult.WrongState => $"{State.FindProject(task.ProjectId)?.Key ?? "Its project"} is completed, so its tasks are read-only. Nothing changed.",
            _ => "That didn't work, and nothing changed. Try again.",
        }, result is not (PbResult.Ok or PbResult.NoChange));

        await RefreshGridAsync();
        focusStatus = true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || dueFilter != OpenFilter;

    /// <summary>The latest due date the state accepts (three years after the seed time).</summary>
    private static DateTime MaxDate => PbSeedData.Now.AddYears(3).Date;

    /// <summary>The earliest due date the state accepts (three years before the seed time, rounded up to a whole day).</summary>
    private static DateTime MinDate => PbSeedData.Now.AddYears(-3).Date.AddDays(1);

    private IReadOnlyList<TaskRow> Shown => Matching().ToList();

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>A group of the Due filter.</summary>
    /// <param name="Id">The filter's value.</param>
    /// <param name="Title">The option's text.</param>
    /// <param name="SummaryWords">The words after the count in the summary, like "overdue".</param>
    /// <param name="IsDone">Whether it holds the done tasks.</param>
    /// <param name="Count">How many tasks it has.</param>
    private sealed record TaskGroup(string Id, string Title, string SummaryWords, bool IsDone, int Count);

    /// <summary>One task as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record TaskRow(
        int Id,
        string Key,
        string Title,
        string Project,
        string Status,
        PbTaskStatus StatusValue,
        string Priority,
        PbTaskPriority PriorityValue,
        DateTime? Due,
        string DueText,
        bool IsOverdue,
        int Estimate,
        string ChecklistText,
        bool IsDone)
    {
        /// <summary>Gets the estimate in words.</summary>
        public string EstimateText => Estimate == 0 ? "None" : string.Create(CultureInfo.InvariantCulture, $"{Estimate} h");
    }

    #endregion
}
