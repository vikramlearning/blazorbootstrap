using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// My tasks: the signed-in person's tasks (<see cref="PbState.MyTasks" />) grouped into Overdue, Today, This week (to
/// Sunday), Later, and No due date, with counts at the top, and the ones done in the last two weeks in a collapsed
/// <c>details</c>. Each row shows the key, the title (a link to the task), the project (a link to its board), the status
/// and priority in words, the due date in words, and checklist progress. "Mark done" moves a task to the end of Done and
/// "Reopen" moves it back to the end of To do (<see cref="PbState.MoveTask" />); the outcome is announced in the status
/// region, which takes focus because the row moves to another group.
/// </summary>
public partial class MyTasks : ComponentBase, IDisposable
{
    #region Fields and Constants

    private bool focusStatus;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusStatus)
        {
            focusStatus = false;
            await PbDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>", 2 overdue, 1 due today, and 3 due this week" for the groups that have tasks.</summary>
    private static string SummaryText(IReadOnlyList<TaskGroup> groups)
    {
        var parts = groups.Where(group => !group.IsDone && group.Tasks.Count > 0).Select(group => $"{group.Tasks.Count} {group.SummaryWords}").ToList();

        return parts.Count switch
        {
            0 => string.Empty,
            1 => $": {parts[0]}",
            _ => $": {string.Join(", ", parts.Take(parts.Count - 1))}, and {parts[^1]}",
        };
    }

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    /// <summary>The groups, in order; the last one is the done tasks.</summary>
    private IReadOnlyList<TaskGroup> Groups(DateTime today)
    {
        var sunday = PbState.WeekStart(today).AddDays(6);
        var mine = State.MyTasks;
        var open = mine.Where(task => task.Status != PbTaskStatus.Done).ToList();

        return new[]
        {
            new TaskGroup("overdue", "Overdue", "overdue", false, open.Where(task => task.DueDate is { } due && due.Date < today).ToList()),
            new TaskGroup("today", "Today", "due today", false, open.Where(task => task.DueDate is { } due && due.Date == today).ToList()),
            new TaskGroup("week", "This week", "due later this week", false, open.Where(task => task.DueDate is { } due && due.Date > today && due.Date <= sunday).ToList()),
            new TaskGroup("later", "Later", "due later", false, open.Where(task => task.DueDate is { } due && due.Date > sunday).ToList()),
            new TaskGroup("none", "No due date", "without a due date", false, open.Where(task => task.DueDate is null).ToList()),
            new TaskGroup("done", "Done in the last 2 weeks", "done", true, mine.Where(task => task.Status == PbTaskStatus.Done).ToList()),
        };
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Marks an open task done (end of Done) or reopens a done one (end of To do).</summary>
    private void ToggleDone(PbTask task)
    {
        var key = State.TaskKey(task);
        var reopen = task.Status == PbTaskStatus.Done;
        var target = reopen ? PbTaskStatus.Todo : PbTaskStatus.Done;
        var result = State.MoveTask(task.Id, target, State.TasksIn(task.ProjectId, target).Count);

        Announce(result is PbResult.Ok or PbResult.NoChange, result switch
        {
            PbResult.Ok when reopen => $"Reopened {key} “{task.Title}”: it's back in To do, under its due date above.",
            PbResult.Ok => $"Marked {key} “{task.Title}” done. It's under Done in the last 2 weeks, where you can reopen it.",
            PbResult.NoChange => reopen ? $"{key} is already open, so nothing changed." : $"{key} is already done, so nothing changed.",
            PbResult.NotFound => $"{key} was deleted, so nothing changed.",
            PbResult.NotAllowed => "Your sign-in ended. Sign in again to update tasks. Nothing changed.",
            PbResult.WrongState => $"{State.FindProject(task.ProjectId)?.Key ?? "Its project"} is completed, so its tasks are read-only. Nothing changed.",
            _ => "That didn't work, and nothing changed. Try again.",
        });

        focusStatus = true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>A group of tasks on the page.</summary>
    /// <param name="Id">The ID used in element IDs and keys.</param>
    /// <param name="Title">The heading.</param>
    /// <param name="SummaryWords">The words after the count in the summary, like "overdue".</param>
    /// <param name="IsDone">Whether it holds the done tasks.</param>
    /// <param name="Tasks">The tasks.</param>
    private sealed record TaskGroup(string Id, string Title, string SummaryWords, bool IsDone, IReadOnlyList<PbTask> Tasks);

    #endregion
}
