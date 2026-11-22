using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The reorder list (AC-7g): active products at or below their reorder point in a <see cref="Grid{TItem}" /> (product
/// linking to its editor, SKU, stock, reorder point, on order, reorder quantity, preferred supplier, and the line's total)
/// with paging, sorting, the column chooser, and CSV export, searched by name or SKU and filtered by supplier, by default
/// to the products with nothing on order yet; its view is saved for the visit and can be reset. Selected products become
/// draft purchase orders, one per preferred supplier (<see cref="EcommerceState.CreatePurchaseOrdersFromLowStock" />), and
/// the announcement links to each new draft.
/// </summary>
public partial class AdminReorder : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private const string ShowAll = "all";

    private const string ShowNeeds = "needs";

    private ElementReference? countRegion;

    /// <summary>The drafts the last "Create purchase orders" made, linked under the announcement.</summary>
    private List<EcommercePurchaseOrder> created = new();

    private string? errorMessage;

    private bool focusStatus;

    private Grid<ReorderRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private string? lastStatus;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The products selected in the grid (on its current page).</summary>
    private List<ReorderRow> selected = new();

    private string showFilter = ShowNeeds;

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

        if (focusStatus)
        {
            focusStatus = false;
            await EcommerceDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "reorder", DefaultPageSize, "stock", SortDirection.Ascending);
        State.Changed += OnStateChanged;
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

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<ReorderRow>();
        Announce("Selection cleared.");
        focusStatus = true;
    }

    /// <summary>
    /// Turns the selected products into draft purchase orders, then refreshes the grid (they're on order now) and clears the
    /// selection; the bar closes, so focus moves to the announcement, which the links to the drafts follow.
    /// </summary>
    private async Task CreateOrdersAsync()
    {
        errorMessage = null;
        var (result, orders, noSupplier, onOrder, notLow) = State.CreatePurchaseOrdersFromLowStock(selected.Select(row => row.Id).ToList());
        var skipped = new List<string>();

        if (onOrder > 0)
            skipped.Add($"{EcommerceDisplay.Plural(onOrder, "product was", "products were")} on order already");

        if (noSupplier > 0)
            skipped.Add($"{EcommerceDisplay.Plural(noSupplier, "product has", "products have")} no preferred supplier");

        if (notLow > 0)
            skipped.Add($"{EcommerceDisplay.Plural(notLow, "product is", "products are")} no longer below the reorder point");

        var skippedText = skipped.Count == 0 ? string.Empty : $" Left out: {string.Join("; ", skipped)}.";

        switch (result)
        {
            case EcommercePurchaseResult.Ok:
                created = orders.ToList();
                Announce($"Created {EcommerceDisplay.Plural(orders.Count, "draft purchase order")} for {EcommerceDisplay.Plural(orders.Sum(order => order.Lines.Count), "product")}: "
                    + $"{string.Join(", ", orders.Select(order => $"{order.Number} to {State.FindSupplier(order.SupplierId)?.Name}"))}. Open each to check it and submit it for approval.{skippedText}");
                break;
            case EcommercePurchaseResult.NotAllowed:
                errorMessage = "Only the back office can create purchase orders. Nothing was created.";
                break;
            default:
                created = new List<EcommercePurchaseOrder>();
                Announce($"No purchase orders were created.{skippedText}");
                break;
        }

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<ReorderRow>();
        focusStatus = true;
    }

    private IEnumerable<EcommerceProduct> Matching()
    {
        IEnumerable<EcommerceProduct> products = State.ReorderList();
        var term = search.Trim();

        if (term.Length > 0)
            products = products.Where(product => product.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || product.Sku.Contains(term, StringComparison.OrdinalIgnoreCase));

        if (supplierFilter.Length > 0)
            products = products.Where(product => State.ReplenishmentFor(product.Id).SupplierId == supplierFilter);

        if (showFilter == ShowNeeds)
            products = products.Where(product => State.OnOrderFor(product.Id) == 0);

        return products;
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

    private void OnSelectionChanged(HashSet<ReorderRow> items) => selected = items.ToList();

    private async Task OnShowChanged(ChangeEventArgs args)
    {
        showFilter = args.Value?.ToString() == ShowAll ? ShowAll : ShowNeeds;
        await RefreshAsync();
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnSupplierChanged(ChangeEventArgs args)
    {
        supplierFilter = State.FindSupplier(args.Value?.ToString()) is { } supplier ? supplier.Id : string.Empty;
        await RefreshAsync();
    }

    /// <summary>The page of products the grid asked for: the lowest stock first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<ReorderRow>> ProvideRowsAsync(GridDataProviderRequest<ReorderRow> request) =>
        view.ApplyAsync(request, Matching().Select(product =>
        {
            var setting = State.ReplenishmentFor(product.Id);
            var supplier = State.FindSupplier(setting.SupplierId);
            var total = setting.ReorderQuantity * setting.UnitCost;

            return new ReorderRow(
                product.Id,
                product.Name,
                product.Sku,
                product.Stock,
                setting.ReorderPoint,
                State.OnOrderFor(product.Id),
                setting.ReorderQuantity,
                supplier?.Id,
                supplier?.Name ?? "None",
                supplier is null ? "—" : EcommerceSeedData.MoneyIn(total, supplier.Currency),
                EcommerceSeedData.ToUsd(total, supplier?.Currency));
        }).ToList());

    /// <summary>A filter changed: back to the first page of the filtered products.</summary>
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
        selected = new List<ReorderRow>();
        gridVersion++;
        Announce("The view is reset: the lowest stock first, 25 a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await EcommerceDisplay.FocusAsync(countRegion);
    }

    private string RowClass(ReorderRow row) => selected.Any(item => item.Id == row.Id) ? "ea-row-selected" : string.Empty;

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

    private string MatchText
    {
        get
        {
            var all = State.ReorderList();
            var count = Matching().Count();

            return $"{EcommerceDisplay.Count(count)} shown · {EcommerceDisplay.Plural(all.Count, "product")} below the reorder point, {NeedsCount} with nothing on order.";
        }
    }

    private int NeedsCount => State.ReorderList().Count(product => State.OnOrderFor(product.Id) == 0);

    [Inject] private EcommerceState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One product to reorder as the grid shows and exports it: every exported column is a property.</summary>
    private sealed record ReorderRow(
        int Id,
        string Name,
        string Sku,
        int Stock,
        int ReorderPoint,
        int OnOrder,
        int ReorderQuantity,
        string? SupplierId,
        string Supplier,
        string LineTotalText,
        decimal LineTotalUsd);

    #endregion
}
