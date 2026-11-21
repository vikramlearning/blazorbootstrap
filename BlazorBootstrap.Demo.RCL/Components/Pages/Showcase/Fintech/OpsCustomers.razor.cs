using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Operations' customer list in a <see cref="Grid{TItem}" />, by name, with paging, sorting, the column chooser, and CSV
/// export: searched by name, customer ID, email, or city, and filtered by status (active or on watch), segment, and whether
/// anything is waiting on operations. Each row has the name (linking to the customer's profile; the whole row opens it), the
/// ID, email, city, segment, customer-since date, the status in text, the number of accounts, the total balance (live for
/// the demo customer), and the open items: held transfers and open disputes, counted and named. The grid's view is saved
/// for the visit and can be reset. The status and path helpers are shared with <see cref="OpsCustomer" />.
/// </summary>
public partial class OpsCustomers : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private const string WithOpenValue = "open";

    private ElementReference? countRegion;

    private Grid<CustomerRow>? grid;

    private int gridVersion;

    private string openFilter = string.Empty;

    private int pageSize = DefaultPageSize;

    private string search = string.Empty;

    private string searchBoxValue = string.Empty;

    private string segmentFilter = string.Empty;

    private string statusFilter = string.Empty;

    private string? statusMessage;

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
        view = GridViews.Get(State.SiteSlug, "ops-customers", DefaultPageSize, "name", SortDirection.Ascending);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the operations route of a customer's profile.</summary>
    internal static string CustomerPath(string customerId) => $"{FintechOpsLayout.Base}/customers/{Uri.EscapeDataString(customerId)}";

    /// <summary>Formats a customer-since date, like "Apr 2, 2019".</summary>
    internal static string SinceText(DateOnly date) => date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>Returns the badge color of a customer status; the badge always carries the status text too.</summary>
    internal static BadgeColor StatusColor(FintechCustomerStatus status) => status == FintechCustomerStatus.Watch ? BadgeColor.Warning : BadgeColor.Success;

    /// <summary>Returns a customer status in words.</summary>
    internal static string StatusText(FintechCustomerStatus status) => status == FintechCustomerStatus.Watch ? "Watch" : "Active";

    #endregion

    #region Private Methods

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = statusFilter = segmentFilter = openFilter = string.Empty;
        await FiltersChangedAsync();
        await FintechDisplay.FocusAsync(countRegion);
    }

    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    private static string OpenItemsText(int held, int disputes)
    {
        var parts = new List<string>();

        if (held > 0)
            parts.Add(held == 1 ? "1 held transfer" : $"{Count(held)} held transfers");

        if (disputes > 0)
            parts.Add(disputes == 1 ? "1 open dispute" : $"{Count(disputes)} open disputes");

        return string.Join(", ", parts);
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

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private Task<GridDataProviderResult<CustomerRow>> ProvideRowsAsync(GridDataProviderRequest<CustomerRow> request) => view.ApplyAsync(request, Shown);

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        statusMessage = statusMessage == "The view is reset: by name, 25 customers a page, and every column shown."
            ? "The view is reset again: by name, 25 customers a page, and every column shown."
            : "The view is reset: by name, 25 customers a page, and every column shown.";
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

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || statusFilter.Length > 0 || segmentFilter.Length > 0 || openFilter.Length > 0;

    private IReadOnlyList<string> Segments => State.Customers.Select(customer => customer.Segment).Distinct().Order(StringComparer.Ordinal).ToList();

    /// <summary>Gets the customers that pass every filter, as grid rows, with their open items counted in one pass.</summary>
    private IReadOnlyList<CustomerRow> Shown
    {
        get
        {
            var term = search.Trim();
            var held = State.AllTransfers.Where(transfer => transfer.Status == FintechTransferStatus.Held).GroupBy(transfer => transfer.CustomerId)
                .ToDictionary(group => group.Key, group => group.Count());
            var open = State.AllDisputes.Where(OpsDisputes.IsOpen).GroupBy(dispute => dispute.CustomerId).ToDictionary(group => group.Key, group => group.Count());

            return State.Customers
                .Where(customer => (statusFilter.Length == 0 || customer.Status.ToString() == statusFilter) && (segmentFilter.Length == 0 || customer.Segment == segmentFilter)
                    && (term.Length == 0 || customer.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || customer.Id.Contains(term, StringComparison.OrdinalIgnoreCase)
                        || customer.Email.Contains(term, StringComparison.OrdinalIgnoreCase) || customer.City.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .Select(customer =>
                {
                    var heldCount = held.GetValueOrDefault(customer.Id);
                    var disputeCount = open.GetValueOrDefault(customer.Id);

                    return new CustomerRow(customer.Id, customer.Name, customer.Email, customer.City, customer.Segment, customer.Since, StatusText(customer.Status), customer.Status,
                        customer.AccountCount, customer.TotalBalance, heldCount + disputeCount, OpenItemsText(heldCount, disputeCount));
                })
                .Where(row => openFilter.Length == 0 || row.OpenCount > 0)
                .OrderBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }

    [Inject] private FintechState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One customer as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record CustomerRow(string Id, string Name, string Email, string City, string Segment, DateOnly Since, string Status, FintechCustomerStatus StatusValue,
        int Accounts, decimal TotalBalance, int OpenCount, string OpenItems);

    #endregion
}
