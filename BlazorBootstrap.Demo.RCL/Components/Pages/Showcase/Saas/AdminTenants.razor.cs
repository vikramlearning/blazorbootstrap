using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// The operator's tenant queue: every customer workspace in a <see cref="Grid{TItem}" /> (name and subdomain linking to the
/// tenant, owner, plan and billing period, seats used, status, MRR, account manager, country, and dates) with paging,
/// sorting, the column chooser, and CSV export, searched by name, subdomain, or owner and filtered by status, plan, and
/// account manager. The grid's view (page size, sort, columns) is saved for the visit and can be reset. Rows can be selected
/// with the grid's checkboxes and changed together from a bulk bar: move to a plan, suspend or reactivate, and assign an
/// account manager (<see cref="SaasState.BulkUpdateTenants" />). <c>?status=Trial</c> (or another status) opens the list
/// filtered, for the overview's links.
/// </summary>
public partial class AdminTenants : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    /// <summary>The manager filter's value for tenants without an account manager.</summary>
    private const string NoManagerValue = "none";

    /// <summary>The bulk bar's account manager value that leaves it as it is (an empty value removes it).</summary>
    private const string NoChangeValue = "keep";

    private string? appliedStatusQuery;

    private string? bulkError;

    private int bulkErrors;

    private string bulkManager = NoChangeValue;

    private string bulkPlan = string.Empty;

    private string bulkStatus = string.Empty;

    private ElementReference? countRegion;

    private bool focusStatus;

    private Grid<TenantRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private string managerFilter = string.Empty;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private string planFilter = string.Empty;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    private ElementReference searchInput;

    /// <summary>The tenants selected in the grid (on its current page).</summary>
    private List<TenantRow> selected = new();

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

        if (focusStatus)
        {
            focusStatus = false;
            await SaasDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "tenants", DefaultPageSize, "created", SortDirection.Descending);
        State.Changed += OnStateChanged;
    }

    /// <summary>The status filter follows <c>?status=</c>, so the sidebar's plain Tenants link clears a filter set by a link.</summary>
    protected override async Task OnParametersSetAsync()
    {
        var requested = Enum.TryParse<SaasWorkspaceStatus>(StatusQuery, ignoreCase: true, out var status) && Enum.IsDefined(status) ? status.ToString() : null;

        if (requested == appliedStatusQuery)
            return;

        appliedStatusQuery = requested;
        statusFilter = requested ?? string.Empty;
        await FiltersChangedAsync();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static string StatusOption(SaasWorkspaceStatus status) => status == SaasWorkspaceStatus.Archived ? "Cancelled (archived)" : SaasDisplay.StatusName(status);

    private static string TenantPath(int id) => string.Create(CultureInfo.InvariantCulture, $"{SaasAdminLayout.Base}/tenants/{id}");

    /// <summary>
    /// Applies the bulk bar's choices to the selected tenants in one <see cref="SaasState.BulkUpdateTenants" /> call, then
    /// refreshes the grid and clears the selection; the bar closes, so focus moves to the announcement.
    /// </summary>
    private async Task ApplyBulkAsync()
    {
        var planId = State.FindPlan(bulkPlan)?.Id;
        SaasWorkspaceStatus? status = Enum.TryParse<SaasWorkspaceStatus>(bulkStatus, out var parsed) && parsed is SaasWorkspaceStatus.Suspended or SaasWorkspaceStatus.Active ? parsed : null;
        string? managerId = bulkManager == NoChangeValue ? null : SaasSeedData.ManagerName(bulkManager) is null ? string.Empty : bulkManager;

        if (planId is null && status is null && managerId is null)
        {
            bulkErrors++;
            const string problem = "Choose a plan, a status, or an account manager to apply.";
            bulkError = bulkErrors == 1 ? problem : string.Create(CultureInfo.InvariantCulture, $"{problem} ({bulkErrors})");

            return;
        }

        bulkError = null;
        bulkErrors = 0;

        var outcome = State.BulkUpdateTenants(selected.Select(row => row.Id).ToList(), planId, status, managerId);
        var message = outcome.Result switch
        {
            SaasResult.Ok => $"Updated {Plural(outcome.Changed, "tenant", "tenants")}."
                + (outcome.Unchanged > 0 ? $" {Plural(outcome.Unchanged, "tenant was", "tenants were")} left as they were: cancelled, too many members for the plan, or already like that." : string.Empty),
            SaasResult.NoChange => "No tenants changed: the selected tenants already had those values, are cancelled, or have more members than the plan allows.",
            SaasResult.NotAllowed => "Sign in as a Lumora operator to change tenants. Nothing changed.",
            SaasResult.NotFound => "That plan or account manager is no longer available. Nothing changed.",
            _ => "Those changes couldn't be saved. Nothing changed.",
        };

        SetStatus(message, outcome.Result is not (SaasResult.Ok or SaasResult.NoChange));
        bulkPlan = bulkStatus = string.Empty;
        bulkManager = NoChangeValue;

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<TenantRow>();
        focusStatus = true;
        StateHasChanged();
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = statusFilter = planFilter = managerFilter = string.Empty;
        await FiltersChangedAsync();
        await SaasDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<TenantRow>();
        bulkError = null;
        SetStatus("Selection cleared.", false);
        focusStatus = true;
    }

    /// <summary>A filter changed: back to the first page of the filtered tenants.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>Returns the tenants that pass every filter (but the status filter when <paramref name="byStatus" /> is off), as grid rows.</summary>
    private IEnumerable<TenantRow> Matching(bool byStatus = true)
    {
        var owners = State.OwnersByWorkspace();
        var seats = State.SeatsUsedByWorkspace();
        var term = search.Trim();
        var planOrder = State.Plans.Select((plan, index) => (plan.Id, index)).ToDictionary(pair => pair.Id, pair => pair.index);

        foreach (var workspace in State.Workspaces)
        {
            var owner = owners.GetValueOrDefault(workspace.Id);

            if (term.Length > 0 && !(workspace.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || workspace.Subdomain.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || (owner?.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) || (owner?.Email.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)))
                continue;

            if (byStatus && statusFilter.Length > 0 && workspace.Status.ToString() != statusFilter)
                continue;

            if (planFilter.Length > 0 && workspace.PlanId != planFilter)
                continue;

            if (managerFilter.Length > 0 && (managerFilter == NoManagerValue ? workspace.ManagerId is not null : workspace.ManagerId != managerFilter))
                continue;

            var plan = State.PlanOf(workspace);
            var used = seats.GetValueOrDefault(workspace.Id);

            yield return new TenantRow(
                workspace.Id,
                workspace.Name,
                workspace.Subdomain,
                owner?.Name ?? "None",
                owner?.Email ?? string.Empty,
                plan.Name,
                plan.Id,
                planOrder.GetValueOrDefault(plan.Id),
                plan.MonthlyPricePerSeat == 0 ? "Free" : workspace.Period == SaasBillingPeriod.Annual ? "Annual" : "Monthly",
                used,
                workspace.Seats,
                SaasDisplay.StatusName(workspace.Status),
                workspace.Status,
                State.MonthlyRevenueFor(workspace),
                SaasSeedData.ManagerName(workspace.ManagerId) ?? string.Empty,
                workspace.Country,
                workspace.CreatedOn,
                workspace is { Status: SaasWorkspaceStatus.Trial, TrialEndsOn: { } trialEndsOn } ? trialEndsOn : workspace.RenewsOn);
        }
    }

    private void OnSelectionChanged(HashSet<TenantRow> items)
    {
        selected = items.ToList();

        if (selected.Count > 0)
            bulkError = null;
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

    /// <summary>Another screen or a bulk change changed the state: the grid reads the tenants again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private static string Plural(int count, string one, string many) => count == 1 ? $"1 {one}" : $"{Count(count)} {many}";

    /// <summary>The page of tenants the grid asked for, from the filtered list, newest first unless a column is sorted.</summary>
    private Task<GridDataProviderResult<TenantRow>> ProvideTenantsAsync(GridDataProviderRequest<TenantRow> request) => view.ApplyAsync(request, Shown);

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
        selected = new List<TenantRow>();
        gridVersion++;
        SetStatus("The view is reset: newest first, 25 tenants a page, and every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await SaasDisplay.FocusAsync(countRegion);
    }

    private string RowClass(TenantRow row) => selected.Any(item => item.Id == row.Id) ? "sd-row-selected" : string.Empty;

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

    private int StatusCount(SaasWorkspaceStatus? status) => Matching(byStatus: false).Count(row => status is null || row.StatusValue == status);

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || statusFilter.Length > 0 || planFilter.Length > 0 || managerFilter.Length > 0;

    private static string NewTenantPath => $"{SaasAdminLayout.Base}/tenants/new";

    private int PastDueCount => State.Workspaces.Count(workspace => workspace.Status == SaasWorkspaceStatus.PastDue);

    private IReadOnlyList<TenantRow> Shown => Matching().ToList();

    [Inject] private SaasState State { get; set; } = default!;

    /// <summary>Gets or sets the status the list opens filtered to, such as <c>?status=Trial</c> from the overview.</summary>
    [SupplyParameterFromQuery(Name = "status")] public string? StatusQuery { get; set; }

    #endregion

    #region Nested Types

    /// <summary>One tenant as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record TenantRow(
        int Id,
        string Name,
        string Subdomain,
        string Owner,
        string OwnerEmail,
        string Plan,
        string PlanId,
        int PlanOrder,
        string Billing,
        int SeatsUsed,
        int Seats,
        string Status,
        SaasWorkspaceStatus StatusValue,
        decimal Mrr,
        string Manager,
        string Country,
        DateTime Created,
        DateTime Renews)
    {
        /// <summary>Gets the owner with their email, as exported.</summary>
        public string OwnerText => OwnerEmail.Length == 0 ? Owner : $"{Owner} <{OwnerEmail}>";

        /// <summary>Gets the seats used of the seats bought, such as "9/12".</summary>
        public string SeatsText => string.Create(CultureInfo.InvariantCulture, $"{SeatsUsed}/{Seats}");
    }

    #endregion
}
