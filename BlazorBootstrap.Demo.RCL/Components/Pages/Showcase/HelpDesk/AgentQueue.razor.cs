using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// The agent queue: totals, search and filters (kept in the site state), and a sortable, paged table of tickets with
/// priority, status, SLA, and "Customer replied" badges. The default order is the most urgent SLA first. Rows can be
/// selected (with a "select all shown" checkbox that shows a mixed state) and changed together from a bulk bar: assignee,
/// status, and priority. The selection only ever holds tickets on the current page.
/// </summary>
public partial class AgentQueue : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int PageSize = 10;

    /// <summary>The bulk "Assign to" value that unassigns; no agent can have this name.</summary>
    private const string UnassignValue = "__unassign";

    private static readonly Column[] columns =
    {
        new(SortKey.Ticket, "Ticket"),
        new(SortKey.Subject, "Subject"),
        new(SortKey.Priority, "Priority"),
        new(SortKey.Status, "Status"),
        new(SortKey.Sla, "SLA"),
        new(SortKey.Assignee, "Assignee"),
        new(SortKey.Updated, "Updated"),
    };

    private string bulkAssignee = string.Empty;

    private string? bulkError;

    private int bulkErrors;

    private string bulkPriority = string.Empty;

    private string bulkStatus = string.Empty;

    private ElementReference countRegion;

    private Button? nextButton;

    private int pageNumber = 1;

    private Button? previousButton;

    /// <summary>The value bound to the search box; it trails the filter while the user types.</summary>
    private string searchBoxValue = string.Empty;

    private ElementReference selectAllButton;

    private readonly HashSet<int> selectedIds = new();

    private bool sortDescending;

    /// <summary>The column the table is sorted by, or <see langword="null" /> for the state's urgency order.</summary>
    private SortKey? sortKey;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        searchBoxValue = State.Filter.Search;
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>
    /// Whether the customer wrote last, after an agent's public reply: a reply or a reopen waiting for the agent. Internal
    /// notes are skipped, since the customer never sees them; a new ticket with only its description is not marked.
    /// </summary>
    private static bool CustomerReplied(HelpDeskTicket ticket)
    {
        if (ticket.Status == HelpDeskStatus.Resolved)
            return false;

        var visible = ticket.Messages.Where(message => !message.IsInternalNote).ToList();

        return visible.Count > 1 && !visible[^1].IsAgent && visible.Take(visible.Count - 1).Any(message => message.IsAgent);
    }

    private static int TotalPages(int count) => Math.Max(1, (int)Math.Ceiling(count / (double)PageSize));

    /// <summary>
    /// Applies the bulk bar's choices to the selected tickets in one <see cref="HelpDeskState.BulkUpdate" /> call. Focus
    /// stays on Apply while the bar is open; when the changed tickets leave the queue (for example resolved under "All
    /// unresolved") the bar closes, so focus moves to the announcement.
    /// </summary>
    private async Task ApplyBulkAsync(IReadOnlyList<HelpDeskTicket> selected)
    {
        var unassign = bulkAssignee == UnassignValue;
        var assignee = unassign || bulkAssignee.Length == 0 ? null : bulkAssignee;
        HelpDeskStatus? status = Enum.TryParse<HelpDeskStatus>(bulkStatus, out var parsedStatus) && Enum.IsDefined(parsedStatus) ? parsedStatus : null;
        HelpDeskPriority? priority = Enum.TryParse<HelpDeskPriority>(bulkPriority, out var parsedPriority) && Enum.IsDefined(parsedPriority) ? parsedPriority : null;

        if (!unassign && assignee is null && status is null && priority is null)
        {
            bulkErrors++;
            const string message = "Choose an assignee, a status, or a priority to apply.";
            bulkError = bulkErrors == 1 ? message : $"{message} ({bulkErrors})";

            return;
        }

        bulkError = null;
        bulkErrors = 0;

        var ids = selected.Select(ticket => ticket.Id).ToList();
        var changed = State.BulkUpdate(ids, assignee, unassign, status, priority);

        SetStatus(changed switch
        {
            0 => "No tickets changed: the selected tickets already had those values.",
            1 => "Updated 1 ticket.",
            _ => string.Create(CultureInfo.InvariantCulture, $"Updated {changed} tickets."),
        });

        bulkAssignee = string.Empty;
        bulkStatus = string.Empty;
        bulkPriority = string.Empty;
        PruneSelection();
        StateHasChanged();

        if (selectedIds.Count == 0)
        {
            await Task.Yield();
            await HelpDeskDisplay.FocusAsync(statusRegion);
        }
    }

    private string? AriaSort(SortKey key) => key != sortKey ? null : sortDescending ? "descending" : "ascending";

    /// <summary>The button disables itself, so focus moves to the ticket count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        State.Filter.Clear();
        searchBoxValue = string.Empty;
        Refresh();
        await HelpDeskDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the "select all shown" checkbox.</summary>
    private async Task ClearSelectionAsync()
    {
        selectedIds.Clear();
        bulkError = null;
        SetStatus("Selection cleared.");
        StateHasChanged();

        await Task.Yield();
        await HelpDeskDisplay.FocusAsync(selectAllButton);
    }

    private string CountText(int matches, int page, int shown)
    {
        if (matches == 0)
            return "0 tickets";

        if (matches <= PageSize)
            return matches == 1 ? "1 ticket" : string.Create(CultureInfo.InvariantCulture, $"{matches} tickets");

        var from = ((page - 1) * PageSize) + 1;

        return string.Create(CultureInfo.InvariantCulture, $"{matches} tickets · showing {from}–{from + shown - 1}");
    }

    /// <summary>
    /// Moves to a page; the selection is dropped, since it only holds tickets that are shown. The button just pressed is
    /// disabled on the first and last page, so focus moves to the other one instead of being lost.
    /// </summary>
    private async Task GoToPageAsync(int target, int totalPages)
    {
        target = Math.Clamp(target, 1, totalPages);

        if (target == Math.Clamp(pageNumber, 1, totalPages))
            return;

        pageNumber = target;
        PruneSelection();
        StateHasChanged();

        await Task.Yield();

        if (target == totalPages)
            await HelpDeskDisplay.FocusAsync(previousButton?.Element);
        else if (target == 1)
            await HelpDeskDisplay.FocusAsync(nextButton?.Element);
    }

    private void OnAssignedToMeChanged(bool value)
    {
        // The Switch raises ValueChanged on its first render too.
        if (State.Filter.AssignedToMe == value)
            return;

        State.Filter.AssignedToMe = value;
        Refresh();
    }

    private void OnBulkAssigneeChanged(ChangeEventArgs args) => bulkAssignee = args.Value?.ToString() ?? string.Empty;

    private void OnBulkPriorityChanged(ChangeEventArgs args) => bulkPriority = args.Value?.ToString() ?? string.Empty;

    private void OnBulkStatusChanged(ChangeEventArgs args) => bulkStatus = args.Value?.ToString() ?? string.Empty;

    private void OnPriorityChanged(ChangeEventArgs args)
    {
        State.Filter.Priority = args.Value?.ToString() ?? string.Empty;
        Refresh();
    }

    private void OnTeamChanged(ChangeEventArgs args)
    {
        State.Filter.TeamId = args.Value?.ToString() ?? string.Empty;
        Refresh();
    }

    private void OnSearchChanged(string value)
    {
        searchBoxValue = value;
        OnSearchValue(value);
    }

    private void OnSearchInput(ChangeEventArgs args) => OnSearchValue(args.Value?.ToString() ?? string.Empty);

    private void OnSearchValue(string value)
    {
        if (State.Filter.Search == value)
            return;

        State.Filter.Search = value;
        Refresh();
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        PruneSelection();
        StateHasChanged();
    });

    private void OnStatusChanged(ChangeEventArgs args)
    {
        State.Filter.Status = args.Value?.ToString() ?? HelpDeskFilter.StatusAllOpen;
        Refresh();
    }

    /// <summary>Keeps only the selected tickets that are still on the current page.</summary>
    private void PruneSelection()
    {
        if (selectedIds.Count == 0)
            return;

        var shown = ShownTickets().Select(ticket => ticket.Id).ToHashSet();
        selectedIds.RemoveWhere(id => !shown.Contains(id));
    }

    /// <summary>A filter changed: back to the first page, keeping only selected tickets that are still shown.</summary>
    private void Refresh()
    {
        pageNumber = 1;
        PruneSelection();
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>The tickets on the current page, in the order the table shows them.</summary>
    private List<HelpDeskTicket> ShownTickets()
    {
        var rows = SortedTickets();
        var page = Math.Clamp(pageNumber, 1, TotalPages(rows.Count));

        return rows.Skip((page - 1) * PageSize).Take(PageSize).ToList();
    }

    /// <summary>Sorts by a column: ascending, then descending, then back to the urgency order.</summary>
    private void SortBy(SortKey key)
    {
        if (key != sortKey)
        {
            sortKey = key;
            sortDescending = false;
        }
        else if (!sortDescending)
        {
            sortDescending = true;
        }
        else
        {
            sortKey = null;
            sortDescending = false;
        }

        pageNumber = 1;
        PruneSelection();
    }

    /// <summary>The button goes away, so focus moves to the ticket count.</summary>
    private async Task SortByUrgencyAsync()
    {
        sortKey = null;
        sortDescending = false;
        pageNumber = 1;
        PruneSelection();
        StateHasChanged();

        await Task.Yield();
        await HelpDeskDisplay.FocusAsync(countRegion);
    }

    /// <summary>The queue for the current filter, in the state's urgency order or by the chosen column (ticket number breaks ties).</summary>
    private List<HelpDeskTicket> SortedTickets()
    {
        var queue = State.Queue();

        if (sortKey is not { } key)
            return queue.ToList();

        IOrderedEnumerable<HelpDeskTicket> ordered = key switch
        {
            SortKey.Ticket => Sort(queue, ticket => ticket.Id),
            SortKey.Subject => Sort(queue, ticket => ticket.Subject, StringComparer.OrdinalIgnoreCase),
            SortKey.Priority => Sort(queue, ticket => ticket.Priority),
            SortKey.Status => Sort(queue, ticket => ticket.Status),
            SortKey.Sla => Sort(queue, ticket => State.DueAt(ticket)),
            SortKey.Assignee => Sort(queue, ticket => ticket.Assignee ?? string.Empty, StringComparer.OrdinalIgnoreCase),
            _ => Sort(queue, ticket => ticket.UpdatedAt),
        };

        return ordered.ThenBy(ticket => ticket.Id).ToList();
    }

    private IOrderedEnumerable<HelpDeskTicket> Sort<TKey>(IEnumerable<HelpDeskTicket> tickets, Func<HelpDeskTicket, TKey> key, IComparer<TKey>? comparer = null) =>
        sortDescending ? tickets.OrderByDescending(key, comparer) : tickets.OrderBy(key, comparer);

    private IconName SortIcon(SortKey key) => key != sortKey ? IconName.ArrowDownUp : sortDescending ? IconName.ArrowDown : IconName.ArrowUp;

    /// <summary>Selects every ticket on the page, or clears them when all are selected already.</summary>
    private void ToggleAllShown(IReadOnlyList<HelpDeskTicket> shown, string state)
    {
        if (state == "true")
            selectedIds.Clear();
        else
            selectedIds.UnionWith(shown.Select(ticket => ticket.Id));

        bulkError = null;
    }

    private void ToggleTicket(int ticketId, ChangeEventArgs args)
    {
        if (args.Value is true)
            selectedIds.Add(ticketId);
        else
            selectedIds.Remove(ticketId);

        bulkError = null;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string SortLabel => sortKey switch
    {
        null => "most urgent first",
        { } key => $"sorted by {columns.First(column => column.Key == key).Text}, {(sortDescending ? "descending" : "ascending")}",
    };

    [Inject] private HelpDeskState State { get; set; } = default!;

    private IEnumerable<(string Label, int Value, IconName Icon)> Summary
    {
        get
        {
            var unresolved = State.Tickets.Where(ticket => ticket.Status != HelpDeskStatus.Resolved).ToList();

            yield return ("Unresolved", unresolved.Count, IconName.Inbox);
            yield return ("Overdue", unresolved.Count(ticket => State.Sla(ticket) == HelpDeskSlaState.Breached), IconName.ExclamationTriangle);
            yield return ("At risk", unresolved.Count(ticket => State.Sla(ticket) == HelpDeskSlaState.AtRisk), IconName.HourglassSplit);
            yield return ("Unassigned", unresolved.Count(ticket => ticket.Assignee is null), IconName.PersonCheck);
        }
    }

    #endregion

    #region Nested Types

    private enum SortKey
    {
        Ticket,
        Subject,
        Priority,
        Status,
        Sla,
        Assignee,
        Updated,
    }

    private sealed record Column(SortKey Key, string Text);

    #endregion
}
