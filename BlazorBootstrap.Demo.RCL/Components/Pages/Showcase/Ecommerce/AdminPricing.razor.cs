using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The retailer's pricing tools: a bulk change (raise or lower by a percentage, start a sale, or end one) for the active
/// products of a category, previewed row by row and applied after a confirmation dialog; and a searchable list where each
/// product's price and compare-at price can be set on its own. Every change goes through
/// <see cref="EcommerceState.UpdatePrices" />, so the store shows the new prices right away.
/// </summary>
public partial class AdminPricing : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const decimal MaxPrice = 100_000m;

    private const decimal MinPrice = .01m;

    private readonly Dictionary<int, PriceRow> rows = new();

    private Button applyButton = default!;

    private bool applied;

    private Modal applyModal = default!;

    private BulkAction bulkAction = BulkAction.Increase;

    private string bulkCategory = string.Empty;

    private string? lastStatus;

    private bool onSaleOnly;

    private List<(int ProductId, decimal Price, decimal? CompareAtPrice)> pendingChanges = new();

    private int percent = 10;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types (see <see cref="OnSearchInput" />).</summary>
    private string searchBoxValue = string.Empty;

    private string? statusMessage;

    private ElementReference statusRegion;

    private int statusRepeats;

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

    private static decimal ClampPrice(decimal value) => Math.Clamp(RoundToCent(value), MinPrice, MaxPrice);

    private static string MoneyOrNone(decimal? value) => value is { } amount ? EcommerceDisplay.Money(amount) : "—";

    private static string ProductCountText(int count) => count == 1 ? "1 product" : $"{count} products";

    private static decimal RoundToCent(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private async Task AskApplyAsync()
    {
        if (PercentError is not null)
            return;

        pendingChanges = Preview.Select(row => (row.Product.Id, row.NewPrice, row.NewCompareAt)).ToList();

        if (pendingChanges.Count == 0)
            return;

        applied = false;
        await applyModal.ShowAsync();
    }

    /// <summary>Works out the new price and compare-at price; a compare-at at or below the price is dropped, as the state does.</summary>
    private (decimal Price, decimal? CompareAt) Calculate(EcommerceProduct product)
    {
        var (price, compareAt) = bulkAction switch
        {
            BulkAction.Increase => (ClampPrice(product.Price * (100 + percent) / 100m), product.CompareAtPrice),
            BulkAction.Decrease => (ClampPrice(product.Price * (100 - percent) / 100m), product.CompareAtPrice),
            // A sale is always off the regular price: a product already on sale keeps its "was" price, so it never inflates.
            BulkAction.StartSale => (ClampPrice((product.CompareAtPrice ?? product.Price) * (100 - percent) / 100m), product.CompareAtPrice ?? product.Price),
            _ => (product.Price, (decimal?)null),
        };

        return (price, compareAt is { } was && was > price ? was : null);
    }

    private async Task ConfirmApplyAsync()
    {
        var changed = State.UpdatePrices(pendingChanges);

        // Rows for these products reload from the store, so the list below shows the new prices.
        foreach (var change in pendingChanges)
            rows.Remove(change.ProductId);

        SetStatus($"Updated prices for {ProductCountText(changed)}.");
        applied = true;

        await applyModal.HideAsync();
    }

    private void OnBulkActionChanged(ChangeEventArgs args)
    {
        if (Enum.TryParse<BulkAction>(args.Value?.ToString(), out var action))
            bulkAction = action;
    }

    private void OnBulkCategoryChanged(ChangeEventArgs args) => bulkCategory = args.Value?.ToString() ?? string.Empty;

    /// <summary>
    /// The dialog opens from code, so Bootstrap doesn't return focus. After an update focus goes to the status message
    /// (the Apply button may now be disabled); otherwise back to the Apply button.
    /// </summary>
    private async Task OnApplyHiddenAsync()
    {
        var wasApplied = applied;

        applied = false;
        pendingChanges = new();

        if (wasApplied)
            await EcommerceDisplay.FocusAsync(statusRegion);
        else
            await EcommerceDisplay.FocusAsync(applyButton.Element);
    }

    /// <summary>The input raises a change once after its first render with the value it was given; only real changes count.</summary>
    private void OnPercentChanged(int value)
    {
        if (value != percent)
            percent = value;
    }

    private void OnRowCompareAtChanged(PriceRow row, decimal? value)
    {
        if (value == row.CompareAt)
            return;

        row.CompareAt = value;
        row.Error = null;
    }

    private void OnRowPriceChanged(PriceRow row, decimal? value)
    {
        if (value == row.Price)
            return;

        row.Price = value;
        row.Error = null;
    }

    private void OnSearchChanged(string value)
    {
        searchBoxValue = value;
        search = value;
    }

    /// <summary>Filters on every keystroke without changing the box's bound value, so older text is never written back into it.</summary>
    private void OnSearchInput(ChangeEventArgs args) => search = args.Value?.ToString() ?? string.Empty;

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>
    /// Returns the editable row of a product. A row nobody has edited follows the store, so a bulk change or a change
    /// elsewhere shows up; a row with unsaved edits keeps them.
    /// </summary>
    private PriceRow RowFor(EcommerceProduct product)
    {
        if (!rows.TryGetValue(product.Id, out var row))
        {
            row = new PriceRow(product);
            rows[product.Id] = row;
        }
        else if (!row.IsEdited && (row.SavedPrice != product.Price || row.SavedCompareAt != product.CompareAtPrice))
        {
            row.Reset(product);
        }

        return row;
    }

    private void SaveRow(int productId)
    {
        if (State.FindAnyProduct(productId) is not { } product || !rows.TryGetValue(productId, out var row))
            return;

        var price = row.Price is { } typed ? RoundToCent(typed) : (decimal?)null;
        var compareAt = row.CompareAt is { } was ? RoundToCent(was) : (decimal?)null;

        row.Error = price switch
        {
            null => "Enter a price.",
            < MinPrice or > MaxPrice => "Enter a price from $0.01 to $100,000.",
            _ when compareAt is > MaxPrice => "Enter a compare-at price up to $100,000, or leave it empty.",
            _ when compareAt is { } value && value <= price => "The compare-at price must be more than the price. Leave it empty when the product isn't on sale.",
            _ => null,
        };

        if (row.Error is not null)
            return;

        var changed = State.UpdatePrices(new[] { (productId, price!.Value, compareAt) });

        rows.Remove(productId);
        SetStatus(changed > 0 ? $"Saved prices for {product.Name}." : $"Prices for {product.Name} were already set to these amounts.");
    }

    /// <summary>Shows a status message; a repeat carries a count, so it is announced again.</summary>
    private void SetStatus(string message)
    {
        statusRepeats = message == lastStatus ? statusRepeats + 1 : 1;
        lastStatus = message;
        statusMessage = statusRepeats == 1 ? message : $"{message} ({statusRepeats} times)";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string ActionDescription
    {
        get
        {
            var products = ProductCountText(pendingChanges.Count);
            var scope = State.FindCategory(bulkCategory) is { } category ? $" in {category.Name}" : string.Empty;
            var change = bulkAction switch
            {
                BulkAction.Increase => $"Raise the price of {products}{scope} by {percent}%.",
                BulkAction.Decrease => $"Lower the price of {products}{scope} by {percent}%.",
                BulkAction.StartSale => $"Put {products}{scope} on sale at {percent}% off: each current price becomes the compare-at price.",
                _ => $"End the sale on {products}{scope}: compare-at prices are removed and the current prices stay.",
            };

            return bulkAction == BulkAction.EndSale ? change : $"{change} New prices are rounded to the cent and stay at $0.01 or more.";
        }
    }

    private string ApplyText => $"Apply to {ProductCountText(Preview.Count)}";

    /// <summary>Gets the active products the bulk change is aimed at: the chosen category, and only those on sale when asked.</summary>
    private IEnumerable<EcommerceProduct> Candidates => State.Products
        .Where(product => bulkCategory.Length == 0 || product.CategoryId == bulkCategory)
        .Where(product => !onSaleOnly || product.IsOnSale);

    private string EmptyPreviewText
    {
        get
        {
            if (PercentError is not null)
                return "Enter a percentage from 1 to 90 to see which prices change.";

            if (!Candidates.Any())
                return onSaleOnly ? "No active products in this selection are on sale." : "No active products in this category.";

            return bulkAction == BulkAction.EndSale
                ? "None of these products is on sale, so there's no sale to end."
                : "None of these prices would change.";
        }
    }

    private IReadOnlyList<EcommerceProduct> Listed
    {
        get
        {
            var term = search.Trim();

            return State.AllProducts
                .Where(product => term.Length == 0 || product.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                .OrderBy(product => product.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }

    private string ListSummary => Listed.Count == State.AllProducts.Count
        ? $"Showing all {ProductCountText(Listed.Count)}"
        : $"{Listed.Count} of {ProductCountText(State.AllProducts.Count)} match";

    private string? PercentError => UsesPercent && percent is < 1 or > 90 ? "Enter a percentage from 1 to 90." : null;

    /// <summary>Gets the products the bulk change affects, with their new prices; products it wouldn't change are left out.</summary>
    private IReadOnlyList<PreviewRow> Preview
    {
        get
        {
            if (PercentError is not null)
                return Array.Empty<PreviewRow>();

            return Candidates
                .Select(product => (Product: product, Change: Calculate(product)))
                .Where(entry => entry.Change.Price != entry.Product.Price || entry.Change.CompareAt != entry.Product.CompareAtPrice)
                .OrderBy(entry => entry.Product.Name, StringComparer.OrdinalIgnoreCase)
                .Select(entry => new PreviewRow(entry.Product, entry.Change.Price, entry.Change.CompareAt))
                .ToList();
        }
    }

    [Inject] private EcommerceState State { get; set; } = default!;

    private bool UsesPercent => bulkAction != BulkAction.EndSale;

    #endregion

    #region Nested Types

    /// <summary>The bulk adjustments.</summary>
    private enum BulkAction
    {
        /// <summary>Raise prices by a percentage; compare-at prices stay.</summary>
        Increase,

        /// <summary>Lower prices by a percentage; compare-at prices stay.</summary>
        Decrease,

        /// <summary>The current price becomes the compare-at price, and the price drops by a percentage.</summary>
        StartSale,

        /// <summary>Remove compare-at prices; prices stay.</summary>
        EndSale,
    }

    /// <summary>One product in the bulk preview.</summary>
    private sealed record PreviewRow(EcommerceProduct Product, decimal NewPrice, decimal? NewCompareAt);

    /// <summary>The editable prices of one product in the list, with the stored prices they started from.</summary>
    private sealed class PriceRow
    {
        public PriceRow(EcommerceProduct product) => Reset(product);

        public decimal? CompareAt { get; set; }

        public string? Error { get; set; }

        public bool IsEdited => Price != SavedPrice || CompareAt != SavedCompareAt;

        public decimal? Price { get; set; }

        public decimal? SavedCompareAt { get; private set; }

        public decimal SavedPrice { get; private set; }

        public void Reset(EcommerceProduct product)
        {
            SavedPrice = product.Price;
            SavedCompareAt = product.CompareAtPrice;
            Price = product.Price;
            CompareAt = product.CompareAtPrice;
            Error = null;
        }
    }

    #endregion
}
