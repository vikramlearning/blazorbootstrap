using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// A project's board: the header (name, key, client, status, dates, next milestone, progress, team), a Board / List view
/// toggle kept in <c>?view=list</c>, and filters (assignee with Me and Unassigned, priority, text, and an "Only my tasks"
/// toggle).
/// <para>
/// The board has four <see cref="SortableList{TItem}" /> columns in one group. A drag in a column calls
/// <see cref="PbState.MoveTask" /> with the new index (<c>OnUpdate</c>); a drag to another column is handled by the source
/// column (<c>OnRemove</c>: the task is the source column's item at the old index, moved to the target column at the new
/// index). After every move the columns are rebuilt from the state and <see cref="boardVersion" /> in each list's
/// <c>@key</c> recreates the lists, so Blazor never diffs against DOM that SortableJS moved. A card is dragged by
/// everything but its move controls (the list's <c>Handle</c>), so pressing them never starts a drag. Every card also has a
/// "Move to" select and Move up/down (<see cref="TaskCard" />), focus returns to the moved card's control, and each move is
/// announced in the status region. With a filter on, dragging and Move up/down are off (reordering part of a column is
/// ambiguous) and the page says so; "Move to" still works.
/// </para>
/// <para>
/// The list is a table with sortable column headings (<c>aria-sort</c>), a Status select per row, and, in board order with
/// no filter, Move up/down within the task's column. A completed project is read-only. New tasks open the shared
/// <see cref="TaskEditor" />. An unknown ID shows <see cref="PbNotFound" />.
/// </para>
/// </summary>
public partial class ProjectBoard : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The assignee filter value for the signed-in person.</summary>
    private const string MeFilter = "@me";

    /// <summary>The assignee filter value for unassigned tasks.</summary>
    private const string UnassignedFilter = "@none";

    /// <summary>The per-column "Add task" buttons, to focus when the dialog is cancelled.</summary>
    private readonly Dictionary<PbTaskStatus, ElementReference> addButtons = new();

    /// <summary>The cards on the board by task ID, to focus a moved card's control.</summary>
    private readonly Dictionary<int, TaskCard> cards = new();

    private readonly Dictionary<int, ElementReference> rowDowns = new();

    private readonly Dictionary<int, ElementReference> rowSelects = new();

    private readonly Dictionary<int, ElementReference> rowUps = new();

    /// <summary>The <c>?deleted=</c> value already announced.</summary>
    private string? appliedDeleted;

    /// <summary>The project the columns were built for.</summary>
    private int builtFor;

    /// <summary>Bumped on every rebuild: part of each column's <c>@key</c>, so the lists are recreated after a move.</summary>
    private int boardVersion;

    /// <summary>The columns as rendered (filtered), rebuilt from the state; never changed after they are passed as data.</summary>
    private Dictionary<PbTaskStatus, List<PbTask>> columns = new();

    private TaskEditor editor = default!;

    /// <summary>The control that opened the task dialog, to focus when it is cancelled.</summary>
    private ElementReference? editorOpener;

    private string filterAssignee = string.Empty;

    private string filterPriority = string.Empty;

    /// <summary>The task and control to focus after the next render (a card on the board, or a row in the list).</summary>
    private (int TaskId, TaskCardControl Control)? focusMoved;

    private bool focusStatus;

    private ElementReference newTaskButton;

    private string search = string.Empty;

    private ListSort sort = ListSort.Board;

    private bool sortDescending;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusMoved is { } moved)
        {
            focusMoved = null;

            if (!IsList && cards.TryGetValue(moved.TaskId, out var card) && State.FindTask(moved.TaskId) is { } task && columns.TryGetValue(task.Status, out var list) && list.Contains(task))
                await card.FocusAsync(moved.Control);
            else if (IsList && RowControl(moved.TaskId, moved.Control) is { } element)
                await PbDisplay.FocusAsync(element);
            else
                await PbDisplay.FocusAsync(statusRegion);
        }

        if (focusStatus)
        {
            focusStatus = false;
            await PbDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (builtFor != Id)
        {
            builtFor = Id;
            statusMessage = null;
            filterAssignee = filterPriority = search = string.Empty;
            cards.Clear();
            Rebuild();
        }

        if (Deleted != appliedDeleted)
        {
            appliedDeleted = Deleted;

            if (Deleted is { } key && DeletedKeyPattern().IsMatch(key) && State.FindProject(Id) is { } project && key.StartsWith(project.Key + "-", StringComparison.Ordinal))
                Announce(true, $"{key} is deleted, with its checklist and comments.");
        }
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    [GeneratedRegex("^[A-Z]{2,5}-[0-9]{1,6}$")]
    private static partial Regex DeletedKeyPattern();

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    private string? AriaSort(ListSort column) => sort == column ? (sortDescending ? "descending" : "ascending") : null;

    /// <summary>The assignee filter's people: the project's team, and anyone else who has a task on it.</summary>
    private IReadOnlyList<PbPerson> AssigneeOptions(PbProject project)
    {
        var ids = project.MemberIds.Concat(State.TasksOf(project.Id).Select(task => task.AssigneeId).OfType<string>()).ToHashSet();

        return State.People.Where(person => ids.Contains(person.Id)).ToList();
    }

    private void ClearFilters()
    {
        filterAssignee = filterPriority = search = string.Empty;
        OnFiltersChanged();
        Announce(true, "Filters cleared: all tasks are shown, and dragging is on again.");
    }

    /// <summary>The list rows: filtered, in board order or by the chosen column.</summary>
    private List<PbTask> ListRows()
    {
        var rows = State.TasksOf(Id).Where(Matches).ToList();

        IEnumerable<PbTask> ordered = sort switch
        {
            ListSort.Key => rows.OrderBy(task => task.Number),
            ListSort.Title => rows.OrderBy(task => task.Title, StringComparer.OrdinalIgnoreCase),
            ListSort.Status => rows.OrderBy(task => task.Status).ThenBy(task => task.Position),
            ListSort.Assignee => rows.OrderBy(task => task.AssigneeId is null).ThenBy(task => State.FindPerson(task.AssigneeId)?.Name, StringComparer.OrdinalIgnoreCase),
            ListSort.Priority => rows.OrderBy(task => task.Priority),
            ListSort.Due => rows.OrderBy(task => task.DueDate is null).ThenBy(task => task.DueDate),
            ListSort.Estimate => rows.OrderBy(task => task.EstimateHours),
            _ => rows,
        };

        var list = ordered.ToList();

        if (sortDescending && sort != ListSort.Board)
            list.Reverse();

        return list;
    }

    private bool Matches(PbTask task)
    {
        if (filterAssignee == MeFilter ? task.AssigneeId is null || task.AssigneeId != State.CurrentPerson?.Id
            : filterAssignee == UnassignedFilter ? task.AssigneeId is not null
            : filterAssignee.Length > 0 && task.AssigneeId != filterAssignee)
            return false;

        if (Enum.TryParse<PbTaskPriority>(filterPriority, out var priority) && task.Priority != priority)
            return false;

        var text = search.Trim();

        return text.Length == 0 || task.Title.Contains(text, StringComparison.OrdinalIgnoreCase) || State.TaskKey(task).Contains(text, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Moves a task with <see cref="PbState.MoveTask" />, announces the outcome, and rebuilds the board.</summary>
    private void Move(PbTask task, PbTaskStatus status, int index)
    {
        var key = State.TaskKey(task);
        var from = task.Status;
        var label = PbDisplay.StatusLabel(status);
        var result = State.MoveTask(task.Id, status, index);

        switch (result)
        {
            case PbResult.Ok when from == status:
                var column = State.TasksIn(task.ProjectId, status).ToList();
                Announce(true, $"Moved {key} to place {column.IndexOf(task) + 1} of {column.Count} in {label}.");
                break;

            case PbResult.Ok:
                Announce(true, $"Moved {key} to {label}{(status == PbTaskStatus.Done ? ": it counts as done" : null)}.");
                break;

            case PbResult.NoChange:
                Announce(true, $"{key} is already there, so nothing changed.");
                break;

            case PbResult.NotFound:
                Announce(false, $"{key} was deleted, so it couldn't be moved.");
                break;

            case PbResult.NotAllowed:
                Announce(false, "Your sign-in ended. Sign in again to move tasks. Nothing moved.");
                break;

            case PbResult.Invalid:
                Announce(false, "That column doesn't exist, so nothing moved.");
                break;

            default:
                Announce(false, "That didn't work, and nothing moved. Try again.");
                break;
        }

        Rebuild();
    }

    private void OnCardMoveTo((PbTask Task, PbTaskStatus Status) move)
    {
        Move(move.Task, move.Status, State.TasksIn(move.Task.ProjectId, move.Status).Count);
        focusMoved = (move.Task.Id, TaskCardControl.MoveTo);
    }

    private void OnCardReorder((PbTask Task, int Offset) move) => Reorder(move.Task, move.Offset, move.Offset < 0 ? TaskCardControl.Up : TaskCardControl.Down);

    /// <summary>A drag to another column, reported by the source column: the task is its item at the old index.</summary>
    private void OnColumnRemove(SortableListEventArgs args)
    {
        if (!Enum.TryParse<PbTaskStatus>(args.FromListName, out var from) || !Enum.TryParse<PbTaskStatus>(args.ToListName, out var to)
            || !columns.TryGetValue(from, out var list) || args.OldIndex < 0 || args.OldIndex >= list.Count)
        {
            Announce(false, "That card couldn't be moved. The board is shown as it is now.");
            Rebuild();

            return;
        }

        Move(list[args.OldIndex], to, args.NewIndex);
    }

    /// <summary>A drag within a column.</summary>
    private void OnColumnUpdate(PbTaskStatus status, SortableListEventArgs args)
    {
        if (!columns.TryGetValue(status, out var list) || args.OldIndex < 0 || args.OldIndex >= list.Count)
        {
            Announce(false, "That card couldn't be moved. The board is shown as it is now.");
            Rebuild();

            return;
        }

        Move(list[args.OldIndex], status, args.NewIndex);
    }

    /// <summary>Announces what the task dialog did, and moves focus to the status message or back to the button that opened it.</summary>
    private void OnEditorClosed(TaskEditorResult result)
    {
        if (result.Message is { } message)
        {
            Announce(true, result.Task is { } task && !Matches(task) ? $"{message} The filters hide it." : message);
            focusStatus = true;
        }
        else if (editorOpener is { } opener)
        {
            _ = PbDisplay.FocusAsync(opener);
        }

        StateHasChanged();
    }

    private void OnFiltersChanged() => Rebuild();

    private void OnRowStatusChanged(PbTask task, ChangeEventArgs args)
    {
        if (!Enum.TryParse<PbTaskStatus>(args.Value?.ToString(), out var status))
            return;

        Move(task, status, State.TasksIn(task.ProjectId, status).Count);
        focusMoved = (task.Id, TaskCardControl.MoveTo);
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        Rebuild(force: false);
        StateHasChanged();
    });

    private async Task OpenCreateAsync(PbTaskStatus status, ElementReference? opener)
    {
        editorOpener = opener;

        await editor.OpenCreateAsync(Id, status);
    }

    /// <summary>
    /// Rebuilds the columns from the state (filtered) as new lists and recreates the SortableLists. Without
    /// <paramref name="force" /> (a change from elsewhere) it keeps the lists when they still hold the same tasks in the
    /// same order: the state's change notice after a move arrives after the move's own rebuild on the Server host, and
    /// recreating the lists again would lose the focus just moved to the card.
    /// </summary>
    private void Rebuild(bool force = true)
    {
        var next = Statuses.ToDictionary(status => status, status => State.TasksIn(Id, status).Where(Matches).ToList());

        if (!force && next.Count == columns.Count && next.All(pair => columns.TryGetValue(pair.Key, out var current) && current.SequenceEqual(pair.Value)))
            return;

        columns = next;
        boardVersion++;
    }

    /// <summary>Moves a task up or down one place in its column.</summary>
    private void Reorder(PbTask task, int offset, TaskCardControl control)
    {
        var column = State.TasksIn(task.ProjectId, task.Status).ToList();
        var index = column.IndexOf(task) + offset;

        if (index < 0 || index >= column.Count)
            Announce(true, $"{State.TaskKey(task)} is already {(offset < 0 ? "first" : "last")} in {PbDisplay.StatusLabel(task.Status)}.");
        else
            Move(task, task.Status, index);

        focusMoved = (task.Id, control);
    }

    private Task ReorderAsync(PbTask task, int offset, TaskCardControl control)
    {
        Reorder(task, offset, control);

        return Task.CompletedTask;
    }

    /// <summary>A list row's control; a disabled Move up/down falls back to the other one, then to the Status select.</summary>
    private ElementReference? RowControl(int taskId, TaskCardControl control)
    {
        if (State.FindTask(taskId) is not { } task)
            return null;

        var column = State.TasksIn(task.ProjectId, task.Status).ToList();
        var index = column.IndexOf(task);
        var canUp = index > 0 && rowUps.ContainsKey(taskId);
        var canDown = index < column.Count - 1 && rowDowns.ContainsKey(taskId);

        return control switch
        {
            TaskCardControl.Up when canUp => rowUps[taskId],
            TaskCardControl.Up when canDown => rowDowns[taskId],
            TaskCardControl.Down when canDown => rowDowns[taskId],
            TaskCardControl.Down when canUp => rowUps[taskId],
            _ => rowSelects.TryGetValue(taskId, out var select) ? select : null,
        };
    }

    private void SetView(bool list)
    {
        if (list == IsList)
            return;

        statusMessage = null;
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(new Dictionary<string, object?> { ["view"] = list ? "list" : null, ["deleted"] = null }), replace: true);
    }

    private void SortBy(ListSort column)
    {
        if (sort == column && column != ListSort.Board)
            sortDescending = !sortDescending;
        else
        {
            sort = column;
            sortDescending = false;
        }

        Announce(true, $"Sorted {SortText}.");

        // "Back to board order" is gone after this render.
        focusStatus = column == ListSort.Board;
    }

    private IconName SortIcon(ListSort column) => sort != column ? IconName.ArrowDownUp : sortDescending ? IconName.SortDown : IconName.SortUp;

    private void ToggleMine()
    {
        filterAssignee = filterAssignee == MeFilter ? string.Empty : MeFilter;
        OnFiltersChanged();
        Announce(true, filterAssignee == MeFilter ? "Showing only your tasks. Dragging is off while a filter is on." : "Showing everyone's tasks.");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the key of a task just deleted on its page (<c>?deleted=WEB-12</c>), to announce.</summary>
    [SupplyParameterFromQuery(Name = "deleted")] public string? Deleted { get; set; }

    /// <summary>Gets or sets the project ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    /// <summary>Gets or sets the view from the query: <c>list</c> for the table, anything else for the board.</summary>
    [SupplyParameterFromQuery(Name = "view")] public string? View { get; set; }

    private bool FiltersOn => filterAssignee.Length > 0 || filterPriority.Length > 0 || search.Trim().Length > 0;

    private bool IsList => string.Equals(View, "list", StringComparison.OrdinalIgnoreCase);

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private static IReadOnlyList<(ListSort Sort, string Text)> SortColumns { get; } = new (ListSort, string)[]
    {
        (ListSort.Key, "Key"),
        (ListSort.Title, "Title"),
        (ListSort.Status, "Status"),
        (ListSort.Assignee, "Assignee"),
        (ListSort.Priority, "Priority"),
        (ListSort.Due, "Due"),
        (ListSort.Estimate, "Estimate"),
    };

    private string SortText => sort switch
    {
        ListSort.Board => "in board order (by column, then place)",
        _ => $"by {SortColumns.First(column => column.Sort == sort).Text.ToLowerInvariant()}, {(sortDescending ? "descending" : "ascending")}",
    };

    [Inject] private PbState State { get; set; } = default!;

    private static IReadOnlyList<PbTaskStatus> Statuses { get; } = Enum.GetValues<PbTaskStatus>();

    #endregion

    #region Nested Types

    /// <summary>What the list is sorted by.</summary>
    private enum ListSort
    {
        Board,
        Key,
        Title,
        Status,
        Assignee,
        Priority,
        Due,
        Estimate,
    }

    #endregion
}
