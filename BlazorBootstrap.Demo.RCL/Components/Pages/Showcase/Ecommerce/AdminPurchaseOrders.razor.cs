using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The purchase orders (AC-7g) in a <see cref="Grid{TItem}" />: number linking to the order, supplier, status (with an
/// overdue flag), lines, units received, total in the supplier's currency, who raised it, and the created and expected
/// dates, with paging, sorting, the column chooser, and CSV export, searched by number, supplier, or product and filtered
/// by status (open and overdue included) and supplier; its view is saved for the visit and can be reset. "New purchase
/// order" asks for the supplier in a dialog and opens the new draft; <c>?status=</c> (a status name, <c>open</c>, or
/// <c>overdue</c>) preselects the filter, as the dashboard's links do.
/// </summary>
public partial class AdminPurchaseOrders : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private const string OpenValue = "open";

    private const string OverdueValue = "overdue";

    private string? appliedStatusQuery;

    private ElementReference? countRegion;

    private Grid<PurchaseOrderRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private string? lastStatus;

    private Button newButton = default!;

    private EditContext newContext = default!;

    private bool newCreated;

    private NewForm newForm = new();

    private Modal newModal = default!;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    private string statusFilter = string.Empty;

    private string? statusMessage;

    private ElementReference statusRegion;

    private int statusRepeats;

    private string supplierFilter = string.Empty;

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
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "purchase-orders", DefaultPageSize, "number", SortDirection.Descending);
        newContext = new EditContext(newForm);
        State.Changed += OnStateChanged;
    }

    /// <summary>Applies <c>?status=</c> when it changes.</summary>
    protected override async Task OnParametersSetAsync()
    {
        if (StatusQuery == appliedStatusQuery)
            return;

        appliedStatusQuery = StatusQuery;
        statusFilter = StatusQuery is OpenValue or OverdueValue ? StatusQuery
            : Enum.TryParse<EcommercePurchaseOrderStatus>(StatusQuery, ignoreCase: true, out var status) && Enum.IsDefined(status) && !int.TryParse(StatusQuery, out _) ? status.ToString()
            : string.Empty;

        if (grid is not null)
            await grid.ResetPageNumber();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void Announce(string message)
    {
        statusRepeats = message == lastStatus ? statusRepeats + 1 : 1;
        lastStatus = message;
        statusMessage = statusRepeats == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusRepeats})");
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = statusFilter = supplierFilter = string.Empty;
        await RefreshAsync();
        await EcommerceDisplay.FocusAsync(countRegion);
    }

    /// <summary>Creates the empty draft and opens it; the state's refusal shows on the supplier field.</summary>
    private async Task CreateAsync()
    {
        var result = State.CreatePurchaseOrder(newForm.SupplierId, out var order);

        if (result != EcommercePurchaseResult.Ok || order is null)
        {
            Announce(result == EcommercePurchaseResult.NotAllowed ? "Only the back office can create purchase orders. Nothing was created." : "That supplier is gone, or the tab holds too many purchase orders. Nothing was created.");
            await newModal.HideAsync();

            return;
        }

        newCreated = true;
        await newModal.HideAsync();
        Navigation.NavigateTo(EcommerceSeedData.AdminPurchaseOrderPath(order.Id));
    }

    private IEnumerable<EcommercePurchaseOrder> Matching()
    {
        IEnumerable<EcommercePurchaseOrder> orders = State.PurchaseOrders;
        var term = search.Trim();

        if (term.Length > 0)
            orders = orders.Where(order => order.Number.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (State.FindSupplier(order.SupplierId)?.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                || order.Lines.Any(line => line.Name.Contains(term, StringComparison.OrdinalIgnoreCase)));

        orders = statusFilter switch
        {
            OpenValue => orders.Where(order => order.IsOpen),
            OverdueValue => orders.Where(State.IsOverdue),
            _ when Enum.TryParse<EcommercePurchaseOrderStatus>(statusFilter, out var status) => orders.Where(order => order.Status == status),
            _ => orders,
        };

        if (supplierFilter.Length > 0)
            orders = orders.Where(order => order.SupplierId == supplierFilter);

        return orders;
    }

    /// <summary>The dialog opens from code, so Bootstrap doesn't return focus: back to "New purchase order" unless a draft opened.</summary>
    private async Task OnNewHiddenAsync()
    {
        var created = newCreated;
        newCreated = false;
        newForm = new NewForm();
        newContext = new EditContext(newForm);

        if (!created)
            await EcommerceDisplay.FocusAsync(newButton.Element);
    }

    private async Task OnSearchChanged(string value)
    {
        searchBoxValue = value;

        if (search == value)
            return;

        search = value;
        await RefreshAsync();
    }

    /// <summary>Filters on every keystroke without changing the box's bound value, so older text is never written back into it.</summary>
    private async Task OnSearchInput(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (search == value)
            return;

        search = value;
        await RefreshAsync();
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnStatusChanged(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        statusFilter = value is OpenValue or OverdueValue ? value : Enum.TryParse<EcommercePurchaseOrderStatus>(value, out var status) && Enum.IsDefined(status) ? status.ToString() : string.Empty;
        await RefreshAsync();
    }

    private async Task OnSupplierChanged(ChangeEventArgs args)
    {
        supplierFilter = State.FindSupplier(args.Value?.ToString()) is { } supplier ? supplier.Id : string.Empty;
        await RefreshAsync();
    }

    private async Task OpenNewAsync()
    {
        newCreated = false;
        newForm = new NewForm();
        newContext = new EditContext(newForm);
        await newModal.ShowAsync();
    }

    /// <summary>The page of purchase orders the grid asked for: the newest first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<PurchaseOrderRow>> ProvideRowsAsync(GridDataProviderRequest<PurchaseOrderRow> request) =>
        view.ApplyAsync(request, Matching().Select(order => new PurchaseOrderRow(
            order.Id,
            order.Number,
            State.FindSupplier(order.SupplierId)?.Name ?? "Removed supplier",
            EcommerceDisplay.PurchaseOrderLabel(order.Status),
            order.Status,
            State.IsOverdue(order),
            order.Lines.Count,
            order.Units == 0 ? "No lines yet" : string.Create(CultureInfo.InvariantCulture, $"{order.UnitsReceived} of {order.Units}"),
            order.Units == 0 ? 0 : order.UnitsReceived / (double)order.Units,
            State.MoneyIn(order),
            State.TotalUsd(order),
            State.Identity.FindAccount(order.CreatedBy)?.Name ?? order.CreatedBy,
            order.CreatedOn,
            order.ExpectedOn)).ToList());

    /// <summary>A filter changed: back to the first page of the filtered purchase orders.</summary>
    private async Task RefreshAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();

        StateHasChanged();
    }

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
        gridVersion++;
        Announce("The view is reset: the newest purchase orders first, 25 a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await EcommerceDisplay.FocusAsync(countRegion);
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
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

    private bool IsFiltered => search.Trim().Length > 0 || statusFilter.Length > 0 || supplierFilter.Length > 0;

    private string LeadText
    {
        get
        {
            var orders = State.PurchaseOrders;
            var waiting = orders.Count(order => order.Status == EcommercePurchaseOrderStatus.Submitted);
            var receipt = orders.Count(order => order.IsOpenForReceipt);
            var overdue = orders.Count(State.IsOverdue);

            return $"Stock bought from {EcommerceDisplay.Plural(State.Suppliers.Count, "supplier")}: {waiting} waiting for approval · {receipt} on the way · {overdue} overdue. "
                + $"Orders over {EcommerceDisplay.Money(EcommerceSeedData.ApprovalThreshold)} need a second approver.";
        }
    }

    private string MatchText
    {
        get
        {
            var count = Matching().Count();

            return count == State.PurchaseOrders.Count
                ? $"Showing all {EcommerceDisplay.Plural(count, "purchase order")}."
                : $"{EcommerceDisplay.Count(count)} of {EcommerceDisplay.Plural(State.PurchaseOrders.Count, "purchase order")} match.";
        }
    }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private EcommerceState State { get; set; } = default!;

    /// <summary>Gets or sets the status the list opens filtered to (<c>?status=submitted</c>, <c>open</c>, or <c>overdue</c>).</summary>
    [SupplyParameterFromQuery(Name = "status")] public string? StatusQuery { get; set; }

    #endregion

    #region Nested Types

    /// <summary>The new-order dialog's one field.</summary>
    private sealed class NewForm
    {
        [Required(ErrorMessage = "Choose the supplier to order from.")]
        public string SupplierId { get; set; } = string.Empty;
    }

    /// <summary>One purchase order as the grid shows and exports it: every exported column is a property.</summary>
    private sealed record PurchaseOrderRow(
        int Id,
        string Number,
        string Supplier,
        string Status,
        EcommercePurchaseOrderStatus StatusValue,
        bool IsOverdue,
        int Lines,
        string ReceivedText,
        double ReceivedShare,
        string TotalText,
        decimal TotalUsd,
        string CreatedBy,
        DateTime CreatedOn,
        DateTime ExpectedOn)
    {
        /// <summary>Gets the creation date as it is exported, like "2026-11-09".</summary>
        public string CreatedText => CreatedOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        /// <summary>Gets the expected date as it is exported, like "2026-11-20".</summary>
        public string ExpectedText => ExpectedOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    #endregion
}
