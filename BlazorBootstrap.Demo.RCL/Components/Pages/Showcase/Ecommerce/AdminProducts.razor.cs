using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The retailer's product list: a sortable, paged grid of every product, archived ones included, filtered by name (live),
/// category, status, and stock level, with a quick "Adjust stock" dialog per row. Names open the product editor.
/// </summary>
public partial class AdminProducts : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Products with this many or fewer in stock count as low (the dashboard uses the same limit).</summary>
    private const int LowStockLimit = 5;

    private const string StatusActive = "active";

    private const string StatusAll = "all";

    private const string StatusArchived = "archived";

    private const string StockAll = "all";

    private const string StockLow = "low";

    private const string StockOut = "out";

    private readonly Dictionary<int, Button> stockButtons = new();

    private EcommerceProduct? adjusting;

    private string categoryFilter = string.Empty;

    private Grid<EcommerceProduct> grid = default!;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types (see <see cref="OnSearchInput" />).</summary>
    private string searchBoxValue = string.Empty;

    private string statusFilter = StatusActive;

    private string? statusMessage;

    private ElementReference statusRegion;

    private int stockChanges;

    private string stockFilter = StockAll;

    private NumberInput<int>? stockInput;

    private Modal stockModal = default!;

    private bool stockSaved;

    private int stockValue;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private async Task AskStockAsync(EcommerceProduct product)
    {
        adjusting = product;
        stockValue = product.Stock;
        stockSaved = false;

        await stockModal.ShowAsync();
    }

    private IconName CategoryIcon(EcommerceProduct product) => State.FindCategory(product.CategoryId)?.IconName ?? IconName.Image;

    private string CategoryName(EcommerceProduct product) => State.FindCategory(product.CategoryId)?.Name ?? product.CategoryId;

    private IEnumerable<EcommerceProduct> FilteredProducts()
    {
        IEnumerable<EcommerceProduct> products = State.AllProducts;
        var term = search.Trim();

        if (term.Length > 0)
            products = products.Where(product => product.Name.Contains(term, StringComparison.OrdinalIgnoreCase));

        if (categoryFilter.Length > 0)
            products = products.Where(product => product.CategoryId == categoryFilter);

        products = statusFilter switch
        {
            StatusArchived => products.Where(product => product.IsArchived),
            StatusAll => products,
            _ => products.Where(product => !product.IsArchived),
        };

        products = stockFilter switch
        {
            StockLow => products.Where(product => product.Stock <= LowStockLimit),
            StockOut => products.Where(product => product.Stock == 0),
            _ => products,
        };

        return products;
    }

    private async Task OnCategoryChanged(ChangeEventArgs args)
    {
        categoryFilter = args.Value?.ToString() ?? string.Empty;
        await RefreshAsync();
    }

    private async Task OnSearchChanged(string value)
    {
        searchBoxValue = value;

        if (search == value)
            return;

        search = value;
        await RefreshAsync();
    }

    /// <summary>
    /// Filters on every keystroke without changing the box's bound value, so a render answering an earlier keystroke
    /// never writes older text back into the box.
    /// </summary>
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
        await grid.RefreshDataAsync();
        StateHasChanged();
    });

    private async Task OnStatusChanged(ChangeEventArgs args)
    {
        statusFilter = args.Value?.ToString() ?? StatusActive;
        await RefreshAsync();
    }

    private async Task OnStockChanged(ChangeEventArgs args)
    {
        stockFilter = args.Value?.ToString() ?? StockAll;
        await RefreshAsync();
    }

    /// <summary>
    /// The dialog opens from code, so Bootstrap doesn't return focus. Focus goes back to the row's button, or to the status
    /// message when the new stock moved the row out of the filtered list.
    /// </summary>
    private async Task OnStockHiddenAsync()
    {
        var productId = adjusting?.Id;
        var saved = stockSaved;

        adjusting = null;
        stockSaved = false;

        if (productId is not int id)
            return;

        var stillListed = FilteredProducts().Any(product => product.Id == id);

        if (stillListed && stockButtons.TryGetValue(id, out var button))
            await EcommerceDisplay.FocusAsync(button.Element);
        else if (saved)
            await EcommerceDisplay.FocusAsync(statusRegion);
    }

    private Task OnStockShownAsync() => EcommerceDisplay.FocusAsync(stockInput?.Element);

    /// <summary>The input raises a change once after its first render with the value it was given; only real changes count.</summary>
    private void OnStockValueChanged(int value)
    {
        if (value != stockValue)
            stockValue = value;
    }

    private Task<GridDataProviderResult<EcommerceProduct>> ProductsDataProvider(GridDataProviderRequest<EcommerceProduct> request) =>
        Task.FromResult(request.ApplyTo(FilteredProducts().ToList()));

    private async Task RefreshAsync()
    {
        await grid.ResetPageNumber();
        StateHasChanged();
    }

    private async Task SaveStockAsync()
    {
        if (adjusting is not null && State.FindAnyProduct(adjusting.Id) is { } product)
        {
            var stock = Math.Clamp(stockValue, 0, 9999);

            stockChanges++;
            stockSaved = true;

            if (stock == product.Stock)
            {
                statusMessage = $"Stock for {product.Name} stays at {stock} (update {stockChanges}).";
            }
            else
            {
                State.SetStock(product.Id, stock);
                statusMessage = $"Stock for {product.Name} set to {stock} (update {stockChanges}).";
            }
        }

        await stockModal.HideAsync();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private EcommerceState State { get; set; } = default!;

    private string SummaryText
    {
        get
        {
            var shown = FilteredProducts().Count();
            var total = State.AllProducts.Count;
            var archived = State.AllProducts.Count(product => product.IsArchived);
            var active = total - archived;
            var outOfStock = State.Products.Count(product => product.Stock == 0);
            var low = State.Products.Count(product => product.Stock > 0 && product.Stock <= LowStockLimit);

            return $"{shown} of {total} {(total == 1 ? "product" : "products")} shown · {active} active, {archived} archived · "
                + $"{low} running low, {outOfStock} out of stock";
        }
    }

    #endregion
}
