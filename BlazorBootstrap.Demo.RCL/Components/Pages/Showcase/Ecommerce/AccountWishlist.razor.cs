using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The customer's wishlist, newest first: each product with its image, price, and stock in words. A product in stock can
/// go into the cart (first color, one item); one out of stock has a "Tell me when it's back" switch for a back-in-stock
/// notice; one the store no longer sells says so. Any product can be removed from the list.
/// </summary>
public partial class AccountWishlist : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>At or below this stock, the page says how many are left.</summary>
    private const int LowStock = 5;

    private bool showCartLink;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

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

    private static string SavedOn(EcommerceWishItem item) => item.AddedOn.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    private static (string Text, IconName Icon, string Css) StockOf(EcommerceProduct product) => product switch
    {
        { IsArchived: true } => ("No longer sold", IconName.Archive, "gone"),
        { Stock: 0 } => ("Out of stock", IconName.XCircle, "out"),
        { Stock: <= LowStock } => ($"Only {product.Stock} left", IconName.ExclamationCircle, "low"),
        _ => ("In stock", IconName.CheckCircle, "in"),
    };

    /// <summary>Adds one item in the product's first color; the button stays in place, so focus stays on it.</summary>
    private void AddToCart(EcommerceProduct product)
    {
        var color = product.Colors[0];
        var added = State.AddToCart(product.Id, color, 1);

        showCartLink = State.CartCount > 0;
        SetStatus(added > 0
            ? $"{product.Name} ({color}) is in your cart."
            : $"{product.Name} couldn't be added: your cart already holds the most we can sell of it.");
    }

    /// <summary>Returns the wishlist with each product, skipping any product the store has deleted.</summary>
    private IReadOnlyList<(EcommerceWishItem Item, EcommerceProduct Product)> Entries(string accountId) => State.WishlistFor(accountId)
        .Select(item => (Item: item, Product: State.FindAnyProduct(item.ProductId)))
        .Where(entry => entry.Product is not null)
        .Select(entry => (Item: entry.Item, Product: entry.Product!))
        .ToList();

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>The removed row and its buttons are gone, so focus moves to the status message.</summary>
    private async Task RemoveAsync(EcommerceProduct product)
    {
        if (!State.IsWished(product.Id))
            return;

        State.ToggleWish(product.Id);
        showCartLink = false;
        SetStatus($"{product.Name} is removed from your wishlist.");
        StateHasChanged();

        await Task.Yield();
        await EcommerceDisplay.FocusAsync(statusRegion);
    }

    /// <summary>
    /// Turns the back-in-stock notice on or off. The library Switch also raises its change when the value is set from
    /// outside, so a value that already matches the state is ignored.
    /// </summary>
    private void SetNotify(EcommerceProduct product, bool notify)
    {
        if (State.Identity.Current is not { } account
            || State.WishlistFor(account.Id).FirstOrDefault(item => item.ProductId == product.Id) is not { } item
            || item.NotifyWhenInStock == notify)
            return;

        if (!State.SetWishNotify(product.Id, notify))
            return;

        showCartLink = false;
        SetStatus(notify
            ? $"We'll add a notice to your account when {product.Name} is back in stock."
            : $"You won't get a notice when {product.Name} is back in stock.");
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : $"{message} ({statusCount})";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private EcommerceState State { get; set; } = default!;

    #endregion
}
