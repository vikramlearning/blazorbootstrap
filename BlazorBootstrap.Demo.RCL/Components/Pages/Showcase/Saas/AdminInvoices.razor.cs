using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Invoice list of the operator console: every invoice of every tenant from <see cref="SaasState.AllInvoices" /> in a
/// <see cref="Grid{TItem}" /> (number linking to the invoice, tenant, issue date, description, amount, refunded, status in
/// words, card) with paging, sorting, the column chooser, and CSV export, filtered by status (paid, partially refunded,
/// refunded, open; each with its count) and searched by invoice number or tenant name, with totals for the filter. The
/// grid's view is saved for the visit and can be reset. Selected open invoices can have their payment retried together (the
/// card on file is charged, as on the invoice's page). <c>?status=open</c> (or <c>paid</c>, <c>partial</c>, <c>refunded</c>)
/// opens the list filtered.
/// </summary>
public partial class AdminInvoices : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private StatusFilter? appliedStatusQuery;

    private ElementReference? countRegion;

    private bool focusStatus;

    private Grid<InvoiceRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    private ElementReference searchInput;

    /// <summary>The invoices selected in the grid (on its current page).</summary>
    private List<InvoiceRow> selected = new();

    private int statusCount;

    private StatusFilter statusFilter = StatusFilter.All;

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
        view = GridViews.Get(State.SiteSlug, "invoices", DefaultPageSize, "issued", SortDirection.Descending);
        State.Changed += OnStateChanged;
    }

    /// <summary>The status filter follows <c>?status=</c>, so the sidebar's plain Invoices link clears a filter set by a link.</summary>
    protected override async Task OnParametersSetAsync()
    {
        StatusFilter? requested = StatusQuery?.Trim().ToLowerInvariant() switch
        {
            "paid" => StatusFilter.Paid,
            "partial" or "partiallyrefunded" => StatusFilter.PartiallyRefunded,
            "refunded" => StatusFilter.Refunded,
            "open" => StatusFilter.Open,
            _ => null,
        };

        if (requested == appliedStatusQuery)
            return;

        appliedStatusQuery = requested;
        statusFilter = requested ?? StatusFilter.All;
        await FiltersChangedAsync();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the console path of an invoice.</summary>
    internal static string DetailPath(string number) => $"{SaasAdminLayout.Base}/invoices/{Uri.EscapeDataString(number)}";

    /// <summary>Returns the badge color of an invoice: paid, partially refunded, refunded, or open.</summary>
    internal static BadgeColor StatusColor(SaasInvoice invoice) => invoice.Status switch
    {
        SaasInvoiceStatus.Open => BadgeColor.Warning,
        SaasInvoiceStatus.Refunded => BadgeColor.Secondary,
        _ when invoice.RefundedAmount > 0 => BadgeColor.Info,
        _ => BadgeColor.Success,
    };

    /// <summary>Returns "Paid", "Partially refunded", "Refunded", or "Open".</summary>
    internal static string StatusName(SaasInvoice invoice) => invoice.Status switch
    {
        SaasInvoiceStatus.Open => "Open",
        SaasInvoiceStatus.Refunded => "Refunded",
        _ when invoice.RefundedAmount > 0 => "Partially refunded",
        _ => "Paid",
    };

    #endregion

    #region Private Methods

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static string FilterName(StatusFilter filter) => filter switch
    {
        StatusFilter.Paid => "Paid",
        StatusFilter.PartiallyRefunded => "Partially refunded",
        StatusFilter.Refunded => "Refunded",
        StatusFilter.Open => "Open",
        _ => "All",
    };

    private static bool Matches(SaasInvoice invoice, StatusFilter filter) => filter switch
    {
        // Paid and partially refunded don't overlap, so each row's badge matches the filter it is shown under.
        StatusFilter.Paid => invoice is { Status: SaasInvoiceStatus.Paid, RefundedAmount: 0 },
        StatusFilter.PartiallyRefunded => invoice is { Status: SaasInvoiceStatus.Paid, RefundedAmount: > 0 },
        StatusFilter.Refunded => invoice.Status == SaasInvoiceStatus.Refunded,
        StatusFilter.Open => invoice.Status == SaasInvoiceStatus.Open,
        _ => true,
    };

    private static string Plural(int count, string one, string many) => count == 1 ? $"1 {one}" : $"{Count(count)} {many}";

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = string.Empty;
        statusFilter = StatusFilter.All;
        await FiltersChangedAsync();
        await SaasDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<InvoiceRow>();
        SetStatus("Selection cleared.", false);
        focusStatus = true;
    }

    /// <summary>A filter changed: back to the first page of the filtered invoices.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>Returns the invoices that pass the search and (unless <paramref name="byStatus" /> is off) the status filter, as grid rows.</summary>
    private IEnumerable<InvoiceRow> Matching(bool byStatus = true)
    {
        var names = State.Workspaces.ToDictionary(workspace => workspace.Id, workspace => workspace.Name);
        var term = search.Trim();

        foreach (var invoice in State.AllInvoices)
        {
            var tenant = names.GetValueOrDefault(invoice.WorkspaceId) ?? "Unknown tenant";

            if (byStatus && !Matches(invoice, statusFilter))
                continue;

            if (term.Length > 0 && !invoice.Number.Contains(term, StringComparison.OrdinalIgnoreCase) && !tenant.Contains(term, StringComparison.OrdinalIgnoreCase))
                continue;

            yield return new InvoiceRow(invoice, invoice.Number, tenant, invoice.IssuedOn, invoice.Description, invoice.Amount, invoice.RefundedAmount, StatusName(invoice),
                AppBilling.CardText(invoice.CardBrand, invoice.CardLast4) ?? string.Empty);
        }
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

    private void OnSelectionChanged(HashSet<InvoiceRow> items) => selected = items.ToList();

    /// <summary>Another screen changed the state: the grid reads the invoices again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>The page of invoices the grid asked for, from the filtered list, newest first unless a column is sorted.</summary>
    private Task<GridDataProviderResult<InvoiceRow>> ProvideInvoicesAsync(GridDataProviderRequest<InvoiceRow> request) => view.ApplyAsync(request, Shown);

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
        selected = new List<InvoiceRow>();
        gridVersion++;
        SetStatus("The view is reset: newest first, 25 invoices a page, and every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await SaasDisplay.FocusAsync(countRegion);
    }

    /// <summary>
    /// Retries the payment of each selected open invoice (<see cref="SaasState.CollectInvoice" />), then clears the selection;
    /// the bar closes, so focus moves to the announcement, which says how many were paid and why the others weren't.
    /// </summary>
    private async Task RetrySelectedAsync()
    {
        int paid = 0, noCard = 0, declined = 0, other = 0;

        foreach (var row in selected.Where(row => row.Invoice.Status == SaasInvoiceStatus.Open))
        {
            switch (State.CollectInvoice(row.Number))
            {
                case SaasResult.Ok:
                    paid++;
                    break;
                case SaasResult.CardRequired:
                    noCard++;
                    break;
                case SaasResult.CardDeclined:
                    declined++;
                    break;
                default:
                    other++;
                    break;
            }
        }

        var parts = new List<string>
        {
            paid == 0 ? "No payment went through." : $"{Plural(paid, "invoice is", "invoices are")} paid with the card on file.",
        };

        if (noCard > 0)
            parts.Add($"{Plural(noCard, "tenant has", "tenants have")} no card on file, so {(noCard == 1 ? "its invoice stays" : "their invoices stay")} open until the customer adds one.");

        if (declined > 0)
            parts.Add($"{Plural(declined, "card was", "cards were")} declined again, so {(declined == 1 ? "that tenant stays" : "those tenants stay")} in dunning until the customer replaces the card.");

        if (other > 0)
            parts.Add($"{Plural(other, "invoice", "invoices")} couldn't be retried: already paid, or no longer in this tab.");

        SetStatus(string.Join(" ", parts), paid == 0);

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<InvoiceRow>();
        focusStatus = true;
        StateHasChanged();
    }

    private string RowClass(InvoiceRow row) => selected.Any(item => item.Number == row.Number) ? "sd-row-selected" : string.Empty;

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    private async Task SelectStatusAsync(StatusFilter filter)
    {
        statusFilter = filter;
        await FiltersChangedAsync();
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message, bool isError)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = isError;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    private int StatusCount(StatusFilter filter) => Matching(byStatus: false).Count(row => Matches(row.Invoice, filter));

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || statusFilter != StatusFilter.All;

    private IReadOnlyList<InvoiceRow> Shown => Matching().ToList();

    [Inject] private SaasState State { get; set; } = default!;

    /// <summary>Gets or sets the status the list opens filtered to, such as <c>?status=open</c>.</summary>
    [SupplyParameterFromQuery(Name = "status")] public string? StatusQuery { get; set; }

    #endregion

    #region Nested Types

    /// <summary>One invoice as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record InvoiceRow(SaasInvoice Invoice, string Number, string Tenant, DateTime Issued, string Description, decimal Amount, decimal Refunded, string Status, string Card);

    /// <summary>The status filters, in the order the buttons show them.</summary>
    private enum StatusFilter
    {
        All,
        Paid,
        PartiallyRefunded,
        Refunded,
        Open,
    }

    #endregion
}
