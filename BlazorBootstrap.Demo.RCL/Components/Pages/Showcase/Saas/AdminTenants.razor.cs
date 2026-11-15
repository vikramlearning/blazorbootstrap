using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Tenant list of the operator console: every customer workspace with its owner, plan, seats, status, MRR, and dates,
/// searchable by name or owner, filtered by status and plan, sortable by any column, and paged. Each row links to the
/// tenant's detail screen, and "New tenant" opens the form that creates one. On a phone the table folds most columns into
/// the tenant cell and a "Sort by" list replaces the column headers. <c>?status=Trial</c> (or another status) opens the list
/// filtered.
/// </summary>
public partial class AdminTenants : ComponentBase, IDisposable
{
    #region Fields and Constants

    private static readonly Column[] columns =
    {
        new(SortKey.Name, "Tenant", null),
        new(SortKey.Owner, "Owner", "d-none d-lg-table-cell"),
        new(SortKey.Plan, "Plan", "d-none d-lg-table-cell"),
        new(SortKey.Seats, "Seats", "d-none d-lg-table-cell text-end"),
        new(SortKey.Status, "Status", "d-none d-lg-table-cell"),
        new(SortKey.Mrr, "MRR", "text-end"),
        new(SortKey.Created, "Created", "d-none d-lg-table-cell"),
        new(SortKey.Renewal, "Renews", "d-none d-lg-table-cell"),
    };

    private static readonly int[] pageSizes = { 10, 25, 50 };

    private Button? nextButton;

    private int pageNumber = 1;

    private int pageSize = 10;

    private string planFilter = string.Empty;

    private Button? previousButton;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types (see <see cref="OnSearchInput" />).</summary>
    private string searchBoxValue = string.Empty;

    private TextInput? searchInput;

    private bool sortDescending = true;

    private SortKey sortKey = SortKey.Created;

    private string statusFilter = string.Empty;

    private string? appliedStatusQuery;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    /// <summary>The status filter follows <c>?status=</c>, so the sidebar's plain Tenants link clears a filter set by a link.</summary>
    protected override void OnParametersSet()
    {
        var requested = Enum.TryParse<SaasWorkspaceStatus>(StatusQuery, ignoreCase: true, out var status) && Enum.IsDefined(status) ? status.ToString() : null;

        if (requested != appliedStatusQuery)
        {
            appliedStatusQuery = requested;
            statusFilter = requested ?? string.Empty;
            pageNumber = 1;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static bool DefaultDescending(SortKey key) => key is SortKey.Seats or SortKey.Mrr or SortKey.Created;

    private static DateTime RenewalDate(SaasWorkspace workspace) =>
        workspace is { Status: SaasWorkspaceStatus.Trial, TrialEndsOn: { } endsOn } ? endsOn : workspace.RenewsOn;

    private static string RenewalText(SaasWorkspace workspace) => workspace.Status == SaasWorkspaceStatus.Trial
        ? $"trial ends {SaasDisplay.Date(RenewalDate(workspace))}"
        : $"renews {SaasDisplay.Date(workspace.RenewsOn)}";

    private static string SortValue(SortKey key, bool descending) => $"{key}-{(descending ? "desc" : "asc")}";

    private string? AriaSort(SortKey key) => key != sortKey ? null : sortDescending ? "descending" : "ascending";

    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = statusFilter = planFilter = string.Empty;
        pageNumber = 1;
        StateHasChanged();

        // The button is gone once nothing is filtered, so focus moves to the search box.
        await Task.Yield();
        await SaasDisplay.FocusAsync(searchInput?.Element);
    }

    private IReadOnlyList<TenantRow> FilteredRows()
    {
        IEnumerable<TenantRow> rows = State.Workspaces
            .Select(workspace => new TenantRow(workspace, State.OwnerOf(workspace.Id), State.PlanOf(workspace), State.SeatsUsed(workspace.Id), State.MonthlyRevenueFor(workspace)))
            .ToList();
        var term = search.Trim();

        if (term.Length > 0)
            rows = rows.Where(row =>
                row.Workspace.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (row.Owner?.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                || (row.Owner?.Email.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));

        if (Enum.TryParse<SaasWorkspaceStatus>(statusFilter, out var status))
            rows = rows.Where(row => row.Workspace.Status == status);

        if (planFilter.Length > 0)
            rows = rows.Where(row => row.Workspace.PlanId == planFilter);

        var ordered = sortKey switch
        {
            SortKey.Owner => Sort(rows, row => row.Owner?.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase),
            SortKey.Plan => Sort(rows, row => PlanIndex(row.Plan)),
            SortKey.Seats => Sort(rows, row => row.Workspace.Seats),
            SortKey.Status => Sort(rows, row => row.Workspace.Status),
            SortKey.Mrr => Sort(rows, row => row.Mrr),
            SortKey.Created => Sort(rows, row => row.Workspace.CreatedOn),
            SortKey.Renewal => Sort(rows, row => RenewalDate(row.Workspace)),
            _ => Sort(rows, row => row.Workspace.Name, StringComparer.OrdinalIgnoreCase),
        };

        return ordered.ThenBy(row => row.Workspace.Name, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.Workspace.Id).ToList();
    }

    /// <summary>
    /// Moves to a page. The button just pressed is disabled on the first and last page, so focus moves to the other one
    /// instead of being lost.
    /// </summary>
    private async Task GoToPageAsync(int target, int totalPages)
    {
        target = Math.Clamp(target, 1, totalPages);

        if (target == Math.Clamp(pageNumber, 1, totalPages))
            return;

        pageNumber = target;
        StateHasChanged();

        if (totalPages < 2)
            return;

        await Task.Yield();

        if (target == totalPages)
            await SaasDisplay.FocusAsync(previousButton?.Element);
        else if (target == 1)
            await SaasDisplay.FocusAsync(nextButton?.Element);
    }

    private void OnPageSizeChanged(ChangeEventArgs args)
    {
        if (int.TryParse(args.Value?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var size) && pageSizes.Contains(size))
        {
            pageSize = size;
            pageNumber = 1;
        }
    }

    private void OnPlanChanged(ChangeEventArgs args)
    {
        planFilter = args.Value?.ToString() ?? string.Empty;
        pageNumber = 1;
    }

    private void OnSearchChanged(string value)
    {
        searchBoxValue = value;

        if (search == value)
            return;

        search = value;
        pageNumber = 1;
    }

    /// <summary>
    /// Filters on every keystroke without changing the box's bound value. On Blazor Server a render answering an earlier
    /// keystroke would otherwise write that older text back into the box and drop what was typed since.
    /// </summary>
    private void OnSearchInput(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (search == value)
            return;

        search = value;
        pageNumber = 1;
    }

    private void OnSortChanged(ChangeEventArgs args)
    {
        var parts = (args.Value?.ToString() ?? string.Empty).Split('-');

        if (parts.Length == 2 && Enum.TryParse<SortKey>(parts[0], out var key) && Enum.IsDefined(key))
        {
            sortKey = key;
            sortDescending = parts[1] == "desc";
            pageNumber = 1;
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void OnStatusChanged(ChangeEventArgs args)
    {
        statusFilter = args.Value?.ToString() ?? string.Empty;
        pageNumber = 1;
    }

    private int PlanIndex(SaasPlan plan)
    {
        for (var index = 0; index < State.Plans.Count; index++)
        {
            if (State.Plans[index].Id == plan.Id)
                return index;
        }

        return State.Plans.Count;
    }

    private IOrderedEnumerable<TenantRow> Sort<TKey>(IEnumerable<TenantRow> rows, Func<TenantRow, TKey> key, IComparer<TKey>? comparer = null) =>
        sortDescending ? rows.OrderByDescending(key, comparer) : rows.OrderBy(key, comparer);

    /// <summary>Sorts by a column: the same column again flips the order; another column starts in its natural order.</summary>
    private void SortBy(SortKey key)
    {
        if (key == sortKey)
        {
            sortDescending = !sortDescending;
        }
        else
        {
            sortKey = key;
            sortDescending = DefaultDescending(key);
        }

        pageNumber = 1;
    }

    private IconName SortIcon(SortKey key) => key != sortKey ? IconName.ArrowDownUp : sortDescending ? IconName.ArrowDown : IconName.ArrowUp;

    private string SortLabel(SortKey key, bool descending) => key switch
    {
        SortKey.Name => descending ? "Tenant name, Z to A" : "Tenant name, A to Z",
        SortKey.Owner => descending ? "Owner, Z to A" : "Owner, A to Z",
        SortKey.Plan => $"Plan, {(descending ? State.Plans[^1].Name : State.Plans[0].Name)} first",
        SortKey.Seats => descending ? "Seats, most first" : "Seats, fewest first",
        SortKey.Status => $"Status, {SaasDisplay.StatusName(descending ? Enum.GetValues<SaasWorkspaceStatus>()[^1] : Enum.GetValues<SaasWorkspaceStatus>()[0])} first",
        SortKey.Mrr => descending ? "MRR, highest first" : "MRR, lowest first",
        SortKey.Created => descending ? "Created, newest first" : "Created, oldest first",
        _ => descending ? "Renewal, latest first" : "Renewal, soonest first",
    };

    private string SummaryText(int matches, int page, int shown)
    {
        var total = State.Workspaces.Count;

        if (matches == 0)
            return $"No tenants match these filters (0 of {Count(total)}).";

        var from = ((page - 1) * pageSize) + 1;
        var range = $"{Count(from)}–{Count(from + shown - 1)}";
        var sort = SortLabel(sortKey, sortDescending);

        return matches == total
            ? $"Showing {range} of {Count(total)} tenants · {sort}"
            : $"{Count(matches)} of {Count(total)} tenants match · showing {range} · {sort}";
    }

    private int TotalPages(int count) => Math.Max(1, (int)Math.Ceiling(count / (double)pageSize));

    #endregion

    #endregion

    #region Properties, Indexers

    private bool HasFilters => search.Trim().Length > 0 || statusFilter.Length > 0 || planFilter.Length > 0;

    [Inject] private SaasState State { get; set; } = default!;

    /// <summary>Gets or sets the status the list opens filtered to, such as <c>?status=Trial</c> from the overview.</summary>
    [SupplyParameterFromQuery(Name = "status")] public string? StatusQuery { get; set; }

    #endregion

    #region Nested Types

    /// <summary>The columns of the list, in order.</summary>
    private enum SortKey
    {
        Name,
        Owner,
        Plan,
        Seats,
        Status,
        Mrr,
        Created,
        Renewal,
    }

    private sealed record Column(SortKey Key, string Text, string? Class);

    private sealed record TenantRow(SaasWorkspace Workspace, SaasMember? Owner, SaasPlan Plan, int SeatsUsed, decimal Mrr);

    #endregion
}
