using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The retailer's product list: a <see cref="Grid{TItem}" /> of every product, archived ones included (image, name linking
/// to the editor, SKU, category, price, stock, status, featured, and added), with paging, sorting, the column chooser, and
/// CSV export, searched by name or SKU and filtered by category, status, and stock level; its view is saved for the visit
/// and can be reset. Each row has a quick "Adjust stock" dialog. Selected products can be published, unpublished
/// (archived), or moved to another category together from a bulk bar.
/// </summary>
public partial class AdminProducts : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private const string StatusActive = "active";

    private const string StatusAll = "all";

    private const string StatusArchived = "archived";

    private const string StockAll = "all";

    private const string StockLow = "low";

    private const string StockOut = "out";

    private readonly Dictionary<int, Button> stockButtons = new();

    /// <summary>The product the stock dialog is about.</summary>
    private int adjustingId;

    private string bulkCategory = string.Empty;

    private string? bulkError;

    private int bulkErrors;

    private string categoryFilter = string.Empty;

    private ElementReference? countRegion;

    private bool focusStatus;

    private Grid<ProductRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private string? lastStatus;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types (see <see cref="OnSearchInput" />).</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The products selected in the grid (on its current page).</summary>
    private List<ProductRow> selected = new();

    private string statusFilter = StatusActive;

    private string? statusMessage;

    private int statusRepeats;

    private ElementReference statusRegion;

    private EditContext stockContext = default!;

    private string stockFilter = StockAll;

    private StockForm stockForm = new();

    private NumberInput<int?>? stockInput;

    private Modal stockModal = default!;

    private bool stockSaved;

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
        view = GridViews.Get(State.SiteSlug, "products", DefaultPageSize, "name", SortDirection.Ascending);
        stockContext = new EditContext(stockForm);
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

    /// <summary>Announces a bulk action's outcome, refreshes the grid, and clears the selection; the bar closes, so focus moves to the announcement.</summary>
    private async Task AfterBulkAsync(EcommerceBulkOutcome outcome, string done, string noChange)
    {
        bulkError = null;
        bulkErrors = 0;
        Announce(outcome.Result switch
        {
            EcommerceBulkResult.Ok => $"{done.Replace("{0}", EcommerceDisplay.Plural(outcome.Changed, "product"), StringComparison.Ordinal)}"
                + (outcome.Unchanged > 0 ? $" {EcommerceDisplay.Plural(outcome.Unchanged, "product was", "products were")} already like that." : string.Empty),
            EcommerceBulkResult.NoChange => noChange,
            EcommerceBulkResult.NotAllowed => "Only the store team can change products. Nothing changed.",
            _ => "Those changes couldn't be saved. Nothing changed.",
        });

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<ProductRow>();
        focusStatus = true;
        StateHasChanged();
    }

    private async Task AskStockAsync(int productId)
    {
        if (State.FindAnyProduct(productId) is not { } product)
            return;

        adjustingId = productId;
        stockForm = new StockForm { Stock = product.Stock };
        stockContext = new EditContext(stockForm);
        stockSaved = false;

        await stockModal.ShowAsync();
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = categoryFilter = string.Empty;
        statusFilter = StatusActive;
        stockFilter = StockAll;
        await RefreshAsync();
        await EcommerceDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<ProductRow>();
        bulkError = null;
        Announce("Selection cleared.");
        focusStatus = true;
    }

    private IEnumerable<EcommerceProduct> FilteredProducts()
    {
        IEnumerable<EcommerceProduct> products = State.AllProducts;
        var term = search.Trim();

        if (term.Length > 0)
            products = products.Where(product => product.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || product.Sku.Contains(term, StringComparison.OrdinalIgnoreCase));

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
            StockLow => products.Where(product => State.IsLowStock(product)),
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

    private void OnSelectionChanged(HashSet<ProductRow> items)
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
        var id = adjustingId;
        var saved = stockSaved;

        adjustingId = 0;
        stockSaved = false;

        if (id == 0)
            return;

        var stillListed = FilteredProducts().Any(product => product.Id == id);

        if (stillListed && stockButtons.TryGetValue(id, out var button))
            await EcommerceDisplay.FocusAsync(button.Element);
        else if (saved)
            await EcommerceDisplay.FocusAsync(statusRegion);
    }

    private Task OnStockShownAsync() => EcommerceDisplay.FocusAsync(stockInput?.Element);

    /// <summary>The page of products the grid asked for: by name unless a column is sorted.</summary>
    private Task<GridDataProviderResult<ProductRow>> ProvideProductsAsync(GridDataProviderRequest<ProductRow> request) =>
        view.ApplyAsync(request, FilteredProducts().Select(ToRow).ToList());

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
        selected = new List<ProductRow>();
        gridVersion++;
        Announce("The view is reset: products by name, 25 a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await EcommerceDisplay.FocusAsync(countRegion);
    }

    private string RowClass(ProductRow row) => selected.Any(item => item.Id == row.Id) ? "ea-row-selected" : string.Empty;

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    private async Task SaveStockAsync()
    {
        if (State.FindAnyProduct(adjustingId) is { } product && stockForm.Stock is { } stock)
        {
            stockSaved = true;

            if (stock == product.Stock)
            {
                Announce($"Stock for {product.Name} stays at {stock.ToString(CultureInfo.InvariantCulture)}.");
            }
            else
            {
                State.SetStock(product.Id, stock);
                Announce($"Stock for {product.Name} set to {stock.ToString(CultureInfo.InvariantCulture)}.");
            }
        }

        await stockModal.HideAsync();
    }

    /// <summary>Publishes or unpublishes the selected products with <see cref="EcommerceState.BulkSetArchived" />.</summary>
    private async Task SetArchivedAsync(bool archived)
    {
        var outcome = State.BulkSetArchived(selected.Select(row => row.Id).ToList(), archived);

        await AfterBulkAsync(outcome,
            archived ? "Unpublished {0}: they're archived, out of the store and shoppers' carts." : "Published {0}: they're back in the store.",
            archived ? "No products changed: the selected products were already unpublished." : "No products changed: the selected products were already published.");
    }

    /// <summary>Moves the selected products to the chosen category with <see cref="EcommerceState.BulkSetCategory" />.</summary>
    private async Task SetCategoryAsync()
    {
        if (State.FindCategory(bulkCategory) is not { } category)
        {
            bulkErrors++;
            const string problem = "Choose the category to move the selected products to.";
            bulkError = bulkErrors == 1 ? problem : string.Create(CultureInfo.InvariantCulture, $"{problem} ({bulkErrors})");

            return;
        }

        var outcome = State.BulkSetCategory(selected.Select(row => row.Id).ToList(), category.Id);
        bulkCategory = string.Empty;

        await AfterBulkAsync(outcome, $"Moved {{0}} to {category.Name}.", $"No products changed: the selected products are already in {category.Name}.");
    }

    private ProductRow ToRow(EcommerceProduct product)
    {
        var category = State.FindCategory(product.CategoryId);

        return new ProductRow(
            product.Id,
            product.Name,
            product.Sku,
            category?.Name ?? product.CategoryId,
            product.Price,
            product.IsOnSale ? product.CompareAtPrice : null,
            product.Stock,
            product.IsArchived ? "Archived" : "Active",
            product.IsArchived,
            product.IsFeatured ? "Yes" : "No",
            product.IsFeatured,
            product.AddedOn,
            product.ImagePath,
            category?.Accent,
            category?.IconName ?? IconName.Image,
            State.IsLowStock(product),
            State.OnOrderFor(product.Id));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || categoryFilter.Length > 0 || statusFilter != StatusActive || stockFilter != StockAll;

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
            var low = State.Products.Count(product => product.Stock > 0 && State.IsLowStock(product));

            return $"{EcommerceDisplay.Count(shown)} of {EcommerceDisplay.Plural(total, "product")} shown · {active} active, {archived} archived · "
                + $"{low} running low, {outOfStock} out of stock";
        }
    }

    #endregion

    #region Nested Types

    /// <summary>One product as the grid shows and exports it: every exported column is a property.</summary>
    private sealed record ProductRow(
        int Id,
        string Name,
        string Sku,
        string Category,
        decimal Price,
        decimal? CompareAt,
        int Stock,
        string Status,
        bool IsArchived,
        string Featured,
        bool IsFeatured,
        DateTime AddedOn,
        string ImagePath,
        string? Accent,
        IconName Icon,
        bool IsLow,
        int OnOrder)
    {
        /// <summary>Gets the date the product was added, as it is exported, like "2026-11-09".</summary>
        public string AddedText => AddedOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    /// <summary>The stock dialog's one field.</summary>
    private sealed class StockForm
    {
        [Required(ErrorMessage = "Enter the units in stock, from 0 to 9,999.")]
        [Range(0, 9999, ErrorMessage = "Enter the units in stock, from 0 to 9,999.")]
        public int? Stock { get; set; }
    }

    #endregion
}
