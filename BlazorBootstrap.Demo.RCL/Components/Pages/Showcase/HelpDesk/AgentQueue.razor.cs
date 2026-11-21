using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// The agent queue: totals, search and filters (kept in the site state), and a <see cref="Grid{TItem}" /> of tickets with
/// priority, status, tier (escalated tickets flagged), SLA, and "Customer replied" badges, paging, sorting, the column chooser, and CSV export. With no column
/// sorted, the rows keep the state's urgency order (the most urgent SLA first). The grid's view (page size, sort, columns)
/// is saved for the visit and can be reset. Rows can be selected with the grid's checkboxes and changed together from a bulk
/// bar: assignee, status, and priority. The grid keeps the selection to the current page.
/// </summary>
public partial class AgentQueue : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 10;

    /// <summary>The bulk "Assign to" value that unassigns; no agent can have this name.</summary>
    private const string UnassignValue = "__unassign";

    private string bulkAssignee = string.Empty;

    private string? bulkError;

    private int bulkErrors;

    private string bulkPriority = string.Empty;

    private string bulkStatus = string.Empty;

    private ElementReference countRegion;

    private Grid<HelpDeskTicket>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>The value bound to the search box; it trails the filter while the user types.</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The tickets selected in the grid (on its current page).</summary>
    private List<HelpDeskTicket> selected = new();

    private int statusCount;

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
        searchBoxValue = State.Filter.Search;
        view = GridViews.Get(State.SiteSlug, "ticket-queue", DefaultPageSize);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string CountText(int matches) =>
        matches == 1 ? "1 ticket" : string.Create(CultureInfo.InvariantCulture, $"{matches} tickets");

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

    /// <summary>
    /// Applies the bulk bar's choices to the selected tickets in one <see cref="HelpDeskState.BulkUpdate" /> call, then
    /// refreshes the grid and clears the selection; the bar closes, so focus moves to the announcement.
    /// </summary>
    private async Task ApplyBulkAsync()
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

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<HelpDeskTicket>();
        StateHasChanged();

        await Task.Yield();
        await HelpDeskDisplay.FocusAsync(statusRegion);
    }

    /// <summary>The button disables itself, so focus moves to the ticket count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        State.Filter.Clear();
        searchBoxValue = string.Empty;
        await FiltersChangedAsync();
        await HelpDeskDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<HelpDeskTicket>();
        bulkError = null;
        SetStatus("Selection cleared.");
        StateHasChanged();

        await Task.Yield();
        await HelpDeskDisplay.FocusAsync(statusRegion);
    }

    /// <summary>A filter changed: back to the first page of the filtered queue.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    private async Task OnAssignedToMeChanged(bool value)
    {
        // The Switch raises ValueChanged on its first render too.
        if (State.Filter.AssignedToMe == value)
            return;

        State.Filter.AssignedToMe = value;
        await FiltersChangedAsync();
    }

    private async Task OnEscalatedOnlyChanged(bool value)
    {
        // The Switch raises ValueChanged on its first render too.
        if (State.Filter.EscalatedOnly == value)
            return;

        State.Filter.EscalatedOnly = value;
        await FiltersChangedAsync();
    }

    private void OnBulkAssigneeChanged(ChangeEventArgs args) => bulkAssignee = args.Value?.ToString() ?? string.Empty;

    private void OnBulkPriorityChanged(ChangeEventArgs args) => bulkPriority = args.Value?.ToString() ?? string.Empty;

    private void OnBulkStatusChanged(ChangeEventArgs args) => bulkStatus = args.Value?.ToString() ?? string.Empty;

    private async Task OnPriorityChanged(ChangeEventArgs args)
    {
        State.Filter.Priority = args.Value?.ToString() ?? string.Empty;
        await FiltersChangedAsync();
    }

    private async Task OnSearchChanged(string value)
    {
        searchBoxValue = value;
        await OnSearchValueAsync(value);
    }

    private Task OnSearchInput(ChangeEventArgs args) => OnSearchValueAsync(args.Value?.ToString() ?? string.Empty);

    private async Task OnSearchValueAsync(string value)
    {
        if (State.Filter.Search == value)
            return;

        State.Filter.Search = value;
        await FiltersChangedAsync();
    }

    private void OnSelectionChanged(HashSet<HelpDeskTicket> items)
    {
        selected = items.ToList();

        if (selected.Count > 0)
            bulkError = null;
    }

    /// <summary>Another screen or tab action changed the state: the grid reads the queue again (the selection keeps the tickets still shown).</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnStatusChanged(ChangeEventArgs args)
    {
        State.Filter.Status = args.Value?.ToString() ?? HelpDeskFilter.StatusAllOpen;
        await FiltersChangedAsync();
    }

    private async Task OnTeamChanged(ChangeEventArgs args)
    {
        State.Filter.TeamId = args.Value?.ToString() ?? string.Empty;
        await FiltersChangedAsync();
    }

    /// <summary>The page of tickets the grid asked for, from the filtered queue in urgency order (or the column sort the grid asks for).</summary>
    private Task<GridDataProviderResult<HelpDeskTicket>> ProvideTicketsAsync(GridDataProviderRequest<HelpDeskTicket> request) => view.ApplyAsync(request, State.Queue());

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the ticket count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        selected = new List<HelpDeskTicket>();
        gridVersion++;
        SetStatus("The view is reset: most urgent first, 10 tickets a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await HelpDeskDisplay.FocusAsync(countRegion);
    }

    private string RowClass(HelpDeskTicket ticket) => selected.Contains(ticket) ? "hd-row-selected" : string.Empty;

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

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

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
            yield return ("Escalated", unresolved.Count(ticket => ticket.IsEscalated), IconName.ArrowUpCircle);
        }
    }

    #endregion
}
