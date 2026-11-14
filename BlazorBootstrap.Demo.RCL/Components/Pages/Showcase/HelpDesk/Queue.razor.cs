using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// Queue: totals, search and filters (kept in the site state), and a sortable, paged grid of tickets with priority,
/// status, and SLA badges. The default order is the most urgent SLA first.
/// </summary>
public partial class Queue : ComponentBase, IDisposable
{
    #region Fields and Constants

    private ElementReference countRegion;

    private Grid<HelpDeskTicket> grid = default!;

    /// <summary>The value bound to the search box; it trails the filter while the user types.</summary>
    private string searchBoxValue = string.Empty;

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

    /// <summary>The button disables itself, so focus moves to the ticket count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        State.Filter.Clear();
        searchBoxValue = string.Empty;
        await RefreshAsync();
        await HelpDeskDisplay.FocusAsync(countRegion);
    }

    private async Task OnAssignedToMeChangedAsync(bool value)
    {
        if (State.Filter.AssignedToMe == value)
            return;

        State.Filter.AssignedToMe = value;
        await RefreshAsync();
    }

    private async Task OnPriorityChangedAsync(ChangeEventArgs args)
    {
        State.Filter.Priority = args.Value?.ToString() ?? string.Empty;
        await RefreshAsync();
    }

    private async Task OnSearchChangedAsync(string value)
    {
        searchBoxValue = value;
        await OnSearchValueAsync(value);
    }

    private Task OnSearchInputAsync(ChangeEventArgs args) => OnSearchValueAsync(args.Value?.ToString() ?? string.Empty);

    private async Task OnSearchValueAsync(string value)
    {
        if (State.Filter.Search == value)
            return;

        State.Filter.Search = value;
        await RefreshAsync();
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await grid.RefreshDataAsync();
        StateHasChanged();
    });

    private async Task OnStatusChangedAsync(ChangeEventArgs args)
    {
        State.Filter.Status = args.Value?.ToString() ?? HelpDeskFilter.StatusAllOpen;
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        await grid.ResetPageNumber();
        StateHasChanged();
    }

    /// <summary>The grid sorts by the clicked column; without one, the state's urgency order is kept.</summary>
    private Task<GridDataProviderResult<HelpDeskTicket>> TicketsDataProvider(GridDataProviderRequest<HelpDeskTicket> request) =>
        Task.FromResult(request.ApplyTo(State.Queue().ToList()));

    #endregion

    #endregion

    #region Properties, Indexers

    private string CountText
    {
        get
        {
            var count = State.Queue().Count;

            return count == 1 ? "1 ticket" : $"{count} tickets";
        }
    }

    [Inject] private HelpDeskState State { get; set; } = default!;

    private IEnumerable<(string Label, int Value, IconName Icon)> Summary
    {
        get
        {
            var unresolved = State.Tickets.Where(ticket => ticket.Status != HelpDeskStatus.Resolved).ToList();

            yield return ("Unresolved", unresolved.Count, IconName.Inbox);
            yield return ("Overdue", unresolved.Count(ticket => HelpDeskState.Sla(ticket) == HelpDeskSlaState.Breached), IconName.ExclamationTriangle);
            yield return ("At risk", unresolved.Count(ticket => HelpDeskState.Sla(ticket) == HelpDeskSlaState.AtRisk), IconName.HourglassSplit);
            yield return ("Unassigned", unresolved.Count(ticket => ticket.Assignee is null), IconName.PersonCheck);
        }
    }

    #endregion
}
