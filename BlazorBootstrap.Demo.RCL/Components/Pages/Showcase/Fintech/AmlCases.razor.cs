using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// The AML case list in a <see cref="Grid{TItem}" />: the number (linking to the case; the whole row opens it), the customer,
/// the priority, the status (with Overdue, and the reviewer or disposition), the analyst, the number of alerts, and the
/// opened and due dates, with paging, sorting, the column chooser, and CSV export. It is searched by number or customer and
/// filtered by status (not closed, the default; waiting for my review; overdue; one status; or all), analyst, and priority;
/// the grid's view is saved for the visit and can be reset. <c>?show=review</c> opens it on the cases waiting for the
/// signed-in senior reviewer, for the overview's link.
/// </summary>
public partial class AmlCases : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private const string OpenValue = "open";

    private const string OverdueValue = "overdue";

    private const string ReviewValue = "review";

    private string? appliedShow;

    private string assigneeFilter = string.Empty;

    private ElementReference? countRegion;

    private Grid<CaseRow>? grid;

    private int gridVersion;

    private int pageSize = DefaultPageSize;

    private string priorityFilter = string.Empty;

    private string search = string.Empty;

    private string searchBoxValue = string.Empty;

    private string statusFilter = OpenValue;

    private string? statusMessage;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

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
        view = GridViews.Get(State.SiteSlug, "aml-cases", DefaultPageSize, "due", SortDirection.Ascending);
        State.Changed += OnStateChanged;
    }

    /// <summary>The status filter follows <c>?show=</c>, so the sidebar's plain link clears a filter set by a link.</summary>
    protected override async Task OnParametersSetAsync()
    {
        var requested = Show is ReviewValue or OverdueValue ? Show : null;

        if (requested == appliedShow)
            return;

        appliedShow = requested;
        statusFilter = requested ?? OpenValue;
        await FiltersChangedAsync();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = assigneeFilter = priorityFilter = string.Empty;
        statusFilter = OpenValue;
        await FiltersChangedAsync();
        await FintechDisplay.FocusAsync(countRegion);
    }

    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>Returns whether a case is escalated to the signed-in senior reviewer, and they may decide it (they didn't escalate it).</summary>
    private bool IsMyReview(FintechAmlCase amlCase) =>
        amlCase.Status == FintechAmlCaseStatus.Escalated && amlCase.ReviewerId == State.CurrentStaffId && amlCase.EscalatedById != State.CurrentStaffId;

    private bool Matches(FintechAmlCase amlCase) => statusFilter switch
    {
        OpenValue => amlCase.Status != FintechAmlCaseStatus.Closed,
        ReviewValue => IsMyReview(amlCase),
        OverdueValue => AmlDisplay.IsOverdue(amlCase, State.Today),
        "" => true,
        _ => amlCase.Status.ToString() == statusFilter,
    };

    private void OnSearchChanged(ChangeEventArgs args) => searchBoxValue = search = args.Value?.ToString() ?? string.Empty;

    private async Task OnSearchInputAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (value == search)
            return;

        search = value;
        await FiltersChangedAsync();
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private Task<GridDataProviderResult<CaseRow>> ProvideRowsAsync(GridDataProviderRequest<CaseRow> request) => view.ApplyAsync(request, Rows);

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    private async Task ResetViewAsync()
    {
        const string message = "The view is reset: soonest due first, 25 cases a page, and every column shown.";

        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        statusMessage = statusMessage == message ? message + " (2)" : message;
        StateHasChanged();

        await Task.Yield();
        await FintechDisplay.FocusAsync(countRegion);
    }

    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Returns the status's second line: who it waits for, or how it was closed.</summary>
    private static string StatusDetail(FintechAmlCase amlCase) => amlCase.Status switch
    {
        FintechAmlCaseStatus.Escalated => $"With {AmlDisplay.StaffText(amlCase.ReviewerId)}{(amlCase.ProposedDisposition is { } proposed ? $", proposing {FintechSeedData.DispositionName(proposed)}" : string.Empty)}",
        FintechAmlCaseStatus.Closed when amlCase.Disposition is { } disposition => FintechSeedData.DispositionName(disposition),
        _ => string.Empty,
    };

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || assigneeFilter.Length > 0 || priorityFilter.Length > 0 || statusFilter != OpenValue;

    private int OpenCount => State.AmlCases.Count(amlCase => amlCase.Status != FintechAmlCaseStatus.Closed);

    /// <summary>Gets the cases that pass every filter, as grid rows.</summary>
    private IReadOnlyList<CaseRow> Rows
    {
        get
        {
            var term = search.Trim();
            var today = State.Today;

            return State.AmlCases
                .Where(amlCase => Matches(amlCase) && (assigneeFilter.Length == 0 || amlCase.AssigneeId == assigneeFilter)
                    && (priorityFilter.Length == 0 || amlCase.Priority.ToString() == priorityFilter))
                .Select(amlCase => (Case: amlCase, Customer: State.FindCustomer(amlCase.CustomerId)?.Name ?? amlCase.CustomerId))
                .Where(item => term.Length == 0 || item.Case.Id.ToString(CultureInfo.InvariantCulture).Contains(term, StringComparison.Ordinal)
                    || item.Customer.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(item => new CaseRow(item.Case.Id, item.Customer, item.Case.Priority.ToString(), item.Case.Priority, AmlDisplay.CaseStatusText(item.Case.Status), item.Case.Status,
                    StatusDetail(item.Case), AmlDisplay.IsOverdue(item.Case, today), AmlDisplay.StaffText(item.Case.AssigneeId), item.Case.AlertIds.Count, item.Case.OpenedAt,
                    item.Case.Status == FintechAmlCaseStatus.Closed ? DateOnly.MaxValue : item.Case.DueOn, AmlDisplay.DueText(item.Case, today)))
                .ToList();
        }
    }

    /// <summary>Gets or sets the list to open on, from <c>?show=</c>: <c>review</c> or <c>overdue</c>.</summary>
    [SupplyParameterFromQuery(Name = "show")] public string? Show { get; set; }

    [Inject] private FintechState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One case as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record CaseRow(int Id, string Customer, string Priority, FintechAmlRisk PriorityValue, string Status, FintechAmlCaseStatus StatusValue, string Detail, bool Overdue,
        string Analyst, int Alerts, DateTime Opened, DateOnly DueOn, string Due);

    #endregion
}
