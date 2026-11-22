using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The retailer's pricing: every product's price, compare-at price, and discount in a <see cref="Grid{TItem}" /> with
/// paging, sorting, the column chooser, and CSV export, searched by name or SKU and filtered by category and sale; its
/// view is saved for the visit and can be reset. A product's row opens a dialog to set its price and compare-at price
/// (above the price: the sale price stays below the regular one) with a reason. Selected products change together from a
/// bulk bar: raise or lower by a percentage or an amount, start a sale, or end one, with a required reason, previewed row
/// by row in a confirmation dialog. Every change goes through <see cref="EcommerceState.UpdatePrices" />, which audits the
/// old and new prices with the reason, so the store shows the new prices right away.
/// </summary>
public partial class AdminPricing : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private const decimal MaxPrice = 100_000m;

    private const decimal MinPrice = .01m;

    private const string SaleOff = "off";

    private const string SaleOn = "on";

    private readonly Dictionary<int, ElementReference> priceButtons = new();

    private bool applied;

    private Modal applyModal = default!;

    private EditContext bulkContext = default!;

    private BulkForm bulkForm = new();

    private string categoryFilter = string.Empty;

    private ElementReference? countRegion;

    private bool focusStatus;

    private Grid<PriceRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private string? lastStatus;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private List<PendingChange> pendingChanges = new();

    private EditContext priceContext = default!;

    private PriceForm priceForm = new();

    private NumberInput<decimal?>? priceInput;

    private Modal priceModal = default!;

    private bool priceSaved;

    /// <summary>The product the price dialog is about.</summary>
    private int pricingId;

    private string saleFilter = string.Empty;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types (see <see cref="OnSearchInput" />).</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The products selected in the grid (on its current page).</summary>
    private List<PriceRow> selected = new();

    private string? statusMessage;

    private ElementReference statusRegion;

    private int statusRepeats;

    /// <summary>How many selected products the reviewed bulk change would leave as they are.</summary>
    private int unchangedCount;

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
        view = GridViews.Get(State.SiteSlug, "pricing", DefaultPageSize, "name", SortDirection.Ascending);
        bulkContext = new EditContext(bulkForm);
        priceContext = new EditContext(priceForm);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static decimal ClampPrice(decimal value) => Math.Clamp(RoundToCent(value), MinPrice, MaxPrice);

    private static string MoneyOrNone(decimal? value) => value is { } amount ? EcommerceDisplay.Money(amount) : "—";

    private static decimal RoundToCent(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private void Announce(string message)
    {
        statusRepeats = message == lastStatus ? statusRepeats + 1 : 1;
        lastStatus = message;
        statusMessage = statusRepeats == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusRepeats})");
    }

    /// <summary>Works out a product's new price and compare-at price; a compare-at at or below the price is dropped, as the state does.</summary>
    private (decimal Price, decimal? CompareAt) Calculate(EcommerceProduct product)
    {
        var value = bulkForm.Value ?? 0m;
        var (price, compareAt) = bulkForm.Action switch
        {
            PriceAction.IncreasePercent => (ClampPrice(product.Price * (100 + value) / 100m), product.CompareAtPrice),
            PriceAction.DecreasePercent => (ClampPrice(product.Price * (100 - value) / 100m), product.CompareAtPrice),
            PriceAction.IncreaseAmount => (ClampPrice(product.Price + value), product.CompareAtPrice),
            PriceAction.DecreaseAmount => (ClampPrice(product.Price - value), product.CompareAtPrice),
            // A sale is always off the regular price: a product already on sale keeps its "was" price, so it never inflates.
            PriceAction.StartSale => (ClampPrice((product.CompareAtPrice ?? product.Price) * (100 - value) / 100m), product.CompareAtPrice ?? product.Price),
            _ => (product.Price, (decimal?)null),
        };

        return (price, compareAt is { } was && was > price ? was : null);
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = categoryFilter = saleFilter = string.Empty;
        await RefreshAsync();
        await EcommerceDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<PriceRow>();
        Announce("Selection cleared.");
        focusStatus = true;
    }

    private async Task ConfirmApplyAsync()
    {
        var reason = bulkForm.Reason.Trim();
        var changed = State.UpdatePrices(pendingChanges.Select(change => (change.Id, change.NewPrice, change.NewCompareAt)), reason);

        Announce(changed == 0
            ? "No prices changed: they were already set to these amounts."
            : $"Updated prices for {EcommerceDisplay.Plural(changed, "product")}. The audit log keeps the old prices and your reason.");
        applied = true;

        await applyModal.HideAsync();
    }

    private IEnumerable<EcommerceProduct> Matching()
    {
        IEnumerable<EcommerceProduct> products = State.AllProducts;
        var term = search.Trim();

        if (term.Length > 0)
            products = products.Where(product => product.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || product.Sku.Contains(term, StringComparison.OrdinalIgnoreCase));

        if (categoryFilter.Length > 0)
            products = products.Where(product => product.CategoryId == categoryFilter);

        if (saleFilter == SaleOn)
            products = products.Where(product => product.IsOnSale);
        else if (saleFilter == SaleOff)
            products = products.Where(product => !product.IsOnSale);

        return products;
    }

    /// <summary>
    /// The dialog opens from code, so Bootstrap doesn't return focus. After an update the selection and its bar are gone,
    /// so focus goes to the status message; otherwise back to the bar.
    /// </summary>
    private async Task OnApplyHiddenAsync()
    {
        var wasApplied = applied;

        applied = false;
        pendingChanges = new List<PendingChange>();

        if (!wasApplied)
            return;

        bulkForm = new BulkForm();
        bulkContext = new EditContext(bulkForm);

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<PriceRow>();
        focusStatus = true;
    }

    private async Task OnCategoryChanged(ChangeEventArgs args)
    {
        categoryFilter = args.Value?.ToString() ?? string.Empty;
        await RefreshAsync();
    }

    /// <summary>The dialog opens from code, so Bootstrap doesn't return focus: it goes back to the product's row, or to the status when the row left the list.</summary>
    private async Task OnPriceHiddenAsync()
    {
        var id = pricingId;
        var saved = priceSaved;

        pricingId = 0;
        priceSaved = false;

        if (Matching().Any(product => product.Id == id) && priceButtons.TryGetValue(id, out var button))
            await EcommerceDisplay.FocusAsync(button);
        else if (saved)
            await EcommerceDisplay.FocusAsync(statusRegion);
    }

    private Task OnPriceShownAsync() => EcommerceDisplay.FocusAsync(priceInput?.Element);

    private async Task OnSaleChanged(ChangeEventArgs args)
    {
        saleFilter = args.Value?.ToString() is SaleOn or SaleOff ? args.Value?.ToString() ?? string.Empty : string.Empty;
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

    /// <summary>Filters on every keystroke without changing the box's bound value, so older text is never written back into it.</summary>
    private async Task OnSearchInput(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (search == value)
            return;

        search = value;
        await RefreshAsync();
    }

    private void OnSelectionChanged(HashSet<PriceRow> items) => selected = items.ToList();

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OpenPriceAsync(int productId)
    {
        if (State.FindAnyProduct(productId) is not { } product)
            return;

        pricingId = productId;
        priceSaved = false;
        priceForm = new PriceForm { Price = product.Price, CompareAt = product.CompareAtPrice };
        priceContext = new EditContext(priceForm);

        await priceModal.ShowAsync();
    }

    /// <summary>The page of products the grid asked for: by name unless a column is sorted.</summary>
    private Task<GridDataProviderResult<PriceRow>> ProvidePricesAsync(GridDataProviderRequest<PriceRow> request) =>
        view.ApplyAsync(request, Matching().Select(product => new PriceRow(
            product.Id,
            product.Name,
            product.Sku,
            State.FindCategory(product.CategoryId)?.Name ?? product.CategoryId,
            product.Price,
            product.IsOnSale ? product.CompareAtPrice : null,
            product.DiscountPercent,
            product.IsArchived ? "Archived" : "Active",
            product.IsArchived)).ToList());

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
        selected = new List<PriceRow>();
        gridVersion++;
        Announce("The view is reset: products by name, 25 a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await EcommerceDisplay.FocusAsync(countRegion);
    }

    /// <summary>
    /// Checks the bulk bar (rule WR-002: a failed submit shows the summary), then works out the selected products' new prices
    /// and opens the confirmation dialog with the ones that change.
    /// </summary>
    private async Task ReviewBulkAsync()
    {
        if (!await bulkContext.ValidateAsync())
            return;

        var changes = new List<PendingChange>();

        foreach (var row in selected)
        {
            if (State.FindAnyProduct(row.Id) is not { } product)
                continue;

            var (price, compareAt) = Calculate(product);

            if (price != product.Price || compareAt != product.CompareAtPrice)
                changes.Add(new PendingChange(product.Id, product.Name, product.Price, price, product.CompareAtPrice, compareAt));
        }

        if (changes.Count == 0)
        {
            Announce(bulkForm.Action == PriceAction.EndSale
                ? "None of the selected products is on sale, so there's no sale to end. Nothing changed."
                : "None of the selected prices would change. Nothing changed.");
            focusStatus = true;

            return;
        }

        pendingChanges = changes.OrderBy(change => change.Name, StringComparer.OrdinalIgnoreCase).ToList();
        unchangedCount = selected.Count - changes.Count;
        applied = false;
        await applyModal.ShowAsync();
    }

    private string RowClass(PriceRow row) => selected.Any(item => item.Id == row.Id) ? "ea-row-selected" : string.Empty;

    private async Task SavePriceAsync()
    {
        if (State.FindAnyProduct(pricingId) is { } product && priceForm.Price is { } typed)
        {
            var price = RoundToCent(typed);
            var compareAt = priceForm.CompareAt is { } was ? RoundToCent(was) : (decimal?)null;
            var changed = State.UpdatePrices(new[] { (product.Id, price, compareAt) }, priceForm.Reason.Trim());

            priceSaved = true;
            Announce(changed > 0 ? $"Saved prices for {product.Name}." : $"Prices for {product.Name} were already set to these amounts.");
        }

        await priceModal.HideAsync();
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

    private string ActionDescription
    {
        get
        {
            var products = EcommerceDisplay.Plural(pendingChanges.Count, "product");
            var value = bulkForm.Value ?? 0m;
            var percent = value.ToString("0.##", CultureInfo.InvariantCulture);

            return bulkForm.Action switch
            {
                PriceAction.IncreasePercent => $"Raise the price of {products} by {percent}%.",
                PriceAction.DecreasePercent => $"Lower the price of {products} by {percent}%.",
                PriceAction.IncreaseAmount => $"Raise the price of {products} by {EcommerceDisplay.Money(value)}.",
                PriceAction.DecreaseAmount => $"Lower the price of {products} by {EcommerceDisplay.Money(value)}.",
                PriceAction.StartSale => $"Put {products} on sale at {percent}% off: each regular price becomes the compare-at price.",
                _ => $"End the sale on {products}: compare-at prices are removed and the current prices stay.",
            };
        }
    }

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || categoryFilter.Length > 0 || saleFilter.Length > 0;

    private string MatchText
    {
        get
        {
            var count = Matching().Count();
            var total = State.AllProducts.Count;
            var onSale = State.AllProducts.Count(product => product.IsOnSale);

            return (count == total ? $"Showing all {EcommerceDisplay.Plural(total, "product")}" : $"{EcommerceDisplay.Count(count)} of {EcommerceDisplay.Plural(total, "product")} match")
                + $" · {onSale} on sale.";
        }
    }

    [Inject] private EcommerceState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The bulk price changes.</summary>
    private enum PriceAction
    {
        /// <summary>Raise prices by a percentage; compare-at prices stay.</summary>
        IncreasePercent,

        /// <summary>Lower prices by a percentage; compare-at prices stay.</summary>
        DecreasePercent,

        /// <summary>Raise prices by an amount; compare-at prices stay.</summary>
        IncreaseAmount,

        /// <summary>Lower prices by an amount; compare-at prices stay.</summary>
        DecreaseAmount,

        /// <summary>The regular price becomes the compare-at price, and the price drops by a percentage.</summary>
        StartSale,

        /// <summary>Remove compare-at prices; prices stay.</summary>
        EndSale,
    }

    /// <summary>The bulk bar's fields: the change, its percentage or amount (the range follows the change), and a reason.</summary>
    private sealed class BulkForm : IValidatableObject
    {
        public PriceAction Action { get; set; } = PriceAction.IncreasePercent;

        [Required(ErrorMessage = "Give a reason for the price change, like “Supplier cost increase”.")]
        [StringLength(EcommerceState.MaxPriceReasonLength, MinimumLength = EcommerceState.MinReasonLength, ErrorMessage = "Use 5 to 200 characters for the reason.")]
        public string Reason { get; set; } = string.Empty;

        /// <summary>Whether the change takes a percentage (otherwise an amount, or nothing to end a sale).</summary>
        public bool UsesPercent => Action is PriceAction.IncreasePercent or PriceAction.DecreasePercent or PriceAction.StartSale;

        public decimal? Value { get; set; } = 10m;

        /// <summary>Runs once the fields are filled in: the value's range depends on the change.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Action == PriceAction.EndSale)
                yield break;

            if (Value is not { } value)
                yield return new ValidationResult(UsesPercent ? "Enter a percentage from 1 to 90." : "Enter an amount from $0.01 to $1,000.", new[] { nameof(Value) });
            else if (UsesPercent && (value < 1 || value > 90 || value != Math.Round(value, 2)))
                yield return new ValidationResult("Enter a percentage from 1 to 90.", new[] { nameof(Value) });
            else if (!UsesPercent && (value < .01m || value > 1000m || value != Math.Round(value, 2)))
                yield return new ValidationResult("Enter an amount from $0.01 to $1,000, in dollars and cents.", new[] { nameof(Value) });
        }
    }

    /// <summary>One product in the bulk change's preview.</summary>
    private sealed record PendingChange(int Id, string Name, decimal OldPrice, decimal NewPrice, decimal? OldCompareAt, decimal? NewCompareAt);

    /// <summary>
    /// The price dialog's fields: a price, an optional compare-at price above it (a cross-field rule: the sale price stays
    /// below the regular price), and a reason.
    /// </summary>
    private sealed class PriceForm : IValidatableObject
    {
        [Range(0.01, 100_000, ErrorMessage = "Enter a compare-at price from $0.01 to $100,000, or leave it empty.")]
        public decimal? CompareAt { get; set; }

        [Required(ErrorMessage = "Enter a price from $0.01 to $100,000.")]
        [Range(0.01, 100_000, ErrorMessage = "Enter a price from $0.01 to $100,000.")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Give a reason for the price change, like “Match the new supplier price”.")]
        [StringLength(EcommerceState.MaxPriceReasonLength, MinimumLength = EcommerceState.MinReasonLength, ErrorMessage = "Use 5 to 200 characters for the reason.")]
        public string Reason { get; set; } = string.Empty;

        /// <summary>Runs once the fields are filled in: a compare-at price must be above the price.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Price is { } price && CompareAt is { } compareAt && Math.Round(compareAt, 2) <= Math.Round(price, 2))
                yield return new ValidationResult("The compare-at price must be above the price, so the sale price stays below the regular one. Leave it empty when the product isn't on sale.",
                    new[] { nameof(CompareAt) });
        }
    }

    /// <summary>One product as the grid shows and exports it: every exported column is a property.</summary>
    private sealed record PriceRow(int Id, string Name, string Sku, string Category, decimal Price, decimal? CompareAt, int Discount, string Status, bool IsArchived)
    {
        /// <summary>Gets the discount as it is exported, like "20% off", or empty.</summary>
        public string DiscountText => Discount > 0 ? string.Create(CultureInfo.InvariantCulture, $"{Discount}% off") : string.Empty;
    }

    #endregion
}
