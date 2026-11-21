using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// Approvals (project managers, AC-7f): one row per project in each submitted, approved, or returned week with hours on a
/// project the signed-in manager approves (<see cref="PbState.ApprovalRows" />), in a <see cref="Grid{TItem}" /> (week,
/// person, project, hours, billable hours, submitted, status) with a saved view, the column chooser, and CSV export,
/// filtered by status (Waiting for you by default) and searched by person or project. The row opens the week's review
/// (<see cref="ApprovalDetail" />). Selected rows can be approved together (<see cref="PbState.BulkApproveHours" />), or
/// their weeks returned with one reason of 10 to 300 characters (<see cref="PbState.BulkReturnWeeks" />).
/// </summary>
public partial class Approvals : ComponentBase, IDisposable
{
    #region Fields and Constants

    internal const string ListUrl = PbLayout.Base + "/approvals";

    private const string ApprovedFilter = "approved";

    private const int DefaultPageSize = 25;

    private const string ReturnedFilter = "returned";

    private const string WaitingFilter = "waiting";

    private string? bulkError;

    private int bulkErrors;

    private string bulkReason = string.Empty;

    private ElementReference? countRegion;

    private bool focusStatus;

    private Grid<ApprovalRowView>? grid;

    private int gridVersion;

    private int pageSize = DefaultPageSize;

    private string search = string.Empty;

    private string searchBoxValue = string.Empty;

    private List<ApprovalRowView> selected = new();

    private int statusCount;

    private string statusFilter = WaitingFilter;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

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
        view = GridViews.Get(State.SiteSlug, "approvals", DefaultPageSize, "week", SortDirection.Descending);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the route of a week's review.</summary>
    internal static string DetailUrl(int timesheetId) => FormattableString.Invariant($"{ListUrl}/{timesheetId}");

    #endregion

    #region Private Methods

    private static string Plural(int count, string one, string many) => count == 1 ? $"1 {one}" : string.Create(CultureInfo.InvariantCulture, $"{count} {many}");

    private static string RowStatus(PbApprovalRow row) => row.Timesheet.Status == PbTimesheetStatus.Returned ? ReturnedFilter : row.IsApproved ? ApprovedFilter : WaitingFilter;

    /// <summary>Approves the selected rows' hours in one call; the bar closes, so focus moves to the announcement.</summary>
    private async Task BulkApproveAsync()
    {
        var outcome = State.BulkApproveHours(selected.Select(row => (row.TimesheetId, row.ProjectId)).ToList());

        await FinishAsync(outcome.Result switch
        {
            PbResult.Ok => $"Approved {Plural(outcome.Changed, "project week", "project weeks")}." + (outcome.Unchanged > 0 ? $" {Plural(outcome.Unchanged, "was", "were")} already approved or returned." : string.Empty),
            PbResult.NoChange => "Nothing changed: the selected hours are already approved, or their weeks were returned.",
            PbResult.NotAllowed => "Only project managers can approve hours. Nothing changed.",
            _ => "Those hours couldn't be approved. Nothing changed.",
        }, outcome.Result is not (PbResult.Ok or PbResult.NoChange));
    }

    /// <summary>Returns the selected rows' weeks with the reason in one call.</summary>
    private async Task BulkReturnAsync()
    {
        var reason = bulkReason.Trim();

        if (reason.Length is < PbState.MinReturnReasonLength or > PbState.MaxReturnReasonLength)
        {
            bulkErrors++;
            var problem = $"Write why you return the weeks, {PbState.MinReturnReasonLength} to {PbState.MaxReturnReasonLength} characters (now {reason.Length}).";
            bulkError = bulkErrors == 1 ? problem : string.Create(CultureInfo.InvariantCulture, $"{problem} ({bulkErrors})");

            return;
        }

        var outcome = State.BulkReturnWeeks(selected.Select(row => row.TimesheetId).Distinct().ToList(), reason);
        bulkReason = string.Empty;

        await FinishAsync(outcome.Result switch
        {
            PbResult.Ok => $"Returned {Plural(outcome.Changed, "week", "weeks")} with your reason; the people are told." + (outcome.Unchanged > 0 ? $" {Plural(outcome.Unchanged, "week was", "weeks were")} already approved or returned." : string.Empty),
            PbResult.NoChange => "Nothing changed: the selected weeks are already approved or returned.",
            PbResult.NotAllowed => "Only project managers can return weeks. Nothing changed.",
            _ => "Those weeks couldn't be returned. Nothing changed.",
        }, outcome.Result is not (PbResult.Ok or PbResult.NoChange));
    }

    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = string.Empty;
        statusFilter = WaitingFilter;
        await FiltersChangedAsync();
        await PbDisplay.FocusAsync(countRegion);
    }

    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<ApprovalRowView>();
        bulkError = null;
        SetStatus("Selection cleared.", false);
        focusStatus = true;
    }

    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    private async Task FinishAsync(string message, bool isError)
    {
        bulkError = null;
        bulkErrors = 0;
        SetStatus(message, isError);

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<ApprovalRowView>();
        focusStatus = true;
        StateHasChanged();
    }

    private IEnumerable<ApprovalRowView> Matching()
    {
        var term = search.Trim();

        foreach (var row in State.ApprovalRows())
        {
            var status = RowStatus(row);

            if (statusFilter.Length > 0 && status != statusFilter)
                continue;

            var person = State.FindPerson(row.Timesheet.PersonId)?.Name ?? "A former teammate";
            var project = $"{row.Project.Key} · {row.Project.Name}";

            if (term.Length > 0 && !(person.Contains(term, StringComparison.OrdinalIgnoreCase) || project.Contains(term, StringComparison.OrdinalIgnoreCase)))
                continue;

            yield return new ApprovalRowView($"{row.Timesheet.Id}-{row.Project.Id}", row.Timesheet.Id, row.Project.Id, row.Timesheet.WeekStart, PbDisplay.Date(row.Timesheet.WeekStart), person, project,
                row.Hours, row.BillableHours, row.Timesheet.SubmittedAt, row.Timesheet.SubmittedAt is { } at ? PbDisplay.Date(at) : "Not submitted",
                status == WaitingFilter ? "Waiting" : status == ReturnedFilter ? "Returned" : "Approved");
        }
    }

    private void OnSearchChanged(ChangeEventArgs args) => searchBoxValue = search = args.Value?.ToString() ?? string.Empty;

    private async Task OnSearchInputAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (value == search)
            return;

        search = value;
        await FiltersChangedAsync();
    }

    private void OnSelectionChanged(HashSet<ApprovalRowView> items)
    {
        selected = items.ToList();

        if (selected.Count > 0)
            bulkError = null;
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private Task<GridDataProviderResult<ApprovalRowView>> ProvideRowsAsync(GridDataProviderRequest<ApprovalRowView> request) => view.ApplyAsync(request, Shown);

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        selected = new List<ApprovalRowView>();
        gridVersion++;
        SetStatus("The view is reset: newest week first, 25 a page, and every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await PbDisplay.FocusAsync(countRegion);
    }

    private string RowClass(ApprovalRowView row) => selected.Any(item => item.Key == row.Key) ? "tb-row-selected" : string.Empty;

    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    private void SetStatus(string message, bool isError)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = isError;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || statusFilter != WaitingFilter;

    private IReadOnlyList<ApprovalRowView> Shown => Matching().ToList();

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One project's hours in a week as the grid shows and exports it.</summary>
    private sealed record ApprovalRowView(
        string Key,
        int TimesheetId,
        int ProjectId,
        DateTime WeekStart,
        string WeekText,
        string Person,
        string Project,
        decimal Hours,
        decimal Billable,
        DateTime? Submitted,
        string SubmittedText,
        string Status);

    #endregion
}
