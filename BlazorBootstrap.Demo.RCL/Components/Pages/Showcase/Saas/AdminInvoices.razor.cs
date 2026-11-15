using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Invoice list of the operator console: every invoice of every tenant from <see cref="SaasState.AllInvoices" />, newest
/// first, filtered by status (paid, partially refunded, refunded, open) and searched by invoice number or tenant name, with
/// totals for the current filter and 25 invoices per page. Each number links to the invoice's detail screen, where it can be
/// refunded or its payment retried. <c>?status=open</c> (or <c>paid</c>, <c>partial</c>, <c>refunded</c>) opens the list
/// filtered.
/// </summary>
public partial class AdminInvoices : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int PageSize = 25;

    private StatusFilter? appliedStatusQuery;

    private Button? nextButton;

    private int pageNumber = 1;

    private Button? previousButton;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types (see <see cref="OnSearchInput" />).</summary>
    private string searchBoxValue = string.Empty;

    private TextInput? searchInput;

    private StatusFilter statusFilter = StatusFilter.All;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    /// <summary>The status filter follows <c>?status=</c>, so the sidebar's plain Invoices link clears a filter set by a link.</summary>
    protected override void OnParametersSet()
    {
        StatusFilter? requested = StatusQuery?.Trim().ToLowerInvariant() switch
        {
            "paid" => StatusFilter.Paid,
            "partial" or "partiallyrefunded" => StatusFilter.PartiallyRefunded,
            "refunded" => StatusFilter.Refunded,
            "open" => StatusFilter.Open,
            _ => null,
        };

        if (requested != appliedStatusQuery)
        {
            appliedStatusQuery = requested;
            statusFilter = requested ?? StatusFilter.All;
            pageNumber = 1;
        }
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

    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = string.Empty;
        statusFilter = StatusFilter.All;
        pageNumber = 1;
        StateHasChanged();

        // The button is gone once nothing is filtered, so focus moves to the search box.
        await Task.Yield();
        await SaasDisplay.FocusAsync(searchInput?.Element);
    }

    private IReadOnlyList<SaasInvoice> FilteredRows(IReadOnlyList<SaasInvoice> all)
    {
        var term = search.Trim();
        IEnumerable<SaasInvoice> rows = all.Where(invoice => Matches(invoice, statusFilter));

        if (term.Length > 0)
            rows = rows.Where(invoice =>
                invoice.Number.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (State.FindWorkspace(invoice.WorkspaceId)?.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));

        return rows.ToList();
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

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void SelectStatus(StatusFilter filter)
    {
        if (statusFilter == filter)
            return;

        statusFilter = filter;
        pageNumber = 1;
    }

    private string SummaryText(int matches, int total, int page, int shown)
    {
        if (matches == 0)
            return $"No invoices match these filters (0 of {Count(total)}).";

        var from = ((page - 1) * PageSize) + 1;
        var range = $"{Count(from)}–{Count(from + shown - 1)}";

        return matches == total
            ? $"Showing {range} of {Count(total)} invoices, newest first."
            : $"{Count(matches)} of {Count(total)} invoices match · showing {range}, newest first.";
    }

    private static int TotalPages(int count) => Math.Max(1, (int)Math.Ceiling(count / (double)PageSize));

    #endregion

    #endregion

    #region Properties, Indexers

    private bool HasFilters => search.Trim().Length > 0 || statusFilter != StatusFilter.All;

    [Inject] private SaasState State { get; set; } = default!;

    /// <summary>Gets or sets the status the list opens filtered to, such as <c>?status=open</c>.</summary>
    [SupplyParameterFromQuery(Name = "status")] public string? StatusQuery { get; set; }

    #endregion

    #region Nested Types

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
