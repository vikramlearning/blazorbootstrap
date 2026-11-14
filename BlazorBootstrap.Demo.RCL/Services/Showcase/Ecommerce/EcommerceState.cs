using System.Text.Json;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

/// <summary>
/// State of the storefront for the current browser tab: catalog filters, cart, reviews written here, placed orders, and
/// the demo sign-in. It is kept in the tab's <c>sessionStorage</c> through the showcase foundation, so it survives a
/// reload in the tab; nothing is sent or charged.
/// </summary>
public sealed class EcommerceState : IShowcaseSiteState
{
    #region Fields and Constants

    private const int FirstOrderId = 10481;

    private const int MaxOrders = 200;

    private const int MaxSessionReviews = 200;

    private static readonly string[] cardBrands = ShowcaseCards.TestCards.Select(card => card.Brand).Distinct().ToArray();

    private readonly List<EcommerceCartLine> cart = new();

    private readonly List<EcommerceOrder> orders = new();

    private readonly List<EcommerceReview> reviews;

    private int nextOrderId = FirstOrderId;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh session, signed out.</summary>
    public EcommerceState()
    {
        reviews = EcommerceSeedData.Reviews(Products);
        Identity = new ShowcaseIdentity(EcommerceSeedData.Roles(), EcommerceSeedData.DemoAccounts());
        Identity.Changed += () => Changed?.Invoke();
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Adds a quantity of a product in one color, merging with an existing line and capping at stock and <see cref="EcommerceSeedData.MaxQuantity" />.</summary>
    /// <returns>The quantity actually added (0 when the product is unknown or out of stock).</returns>
    public int AddToCart(int productId, string color, int quantity)
    {
        if (FindProduct(productId) is not { } product || product.Stock == 0 || quantity <= 0)
            return 0;

        var index = cart.FindIndex(line => line.ProductId == productId && line.Color == color);
        var current = index < 0 ? 0 : cart[index].Quantity;
        var updated = Math.Min(current + quantity, MaxQuantityFor(product, color));

        if (updated == current)
            return 0;

        if (index < 0)
            cart.Add(new EcommerceCartLine(productId, color, updated));
        else
            cart[index] = cart[index] with { Quantity = updated };

        Changed?.Invoke();

        return updated - current;
    }

    /// <summary>Adds a review written in this session and returns it.</summary>
    public EcommerceReview AddReview(int productId, string author, int rating, string title, string body)
    {
        var review = new EcommerceReview(productId, author.Trim(), Math.Clamp(rating, 1, 5), title.Trim(), body.Trim(), Clock.Today, IsFromThisSession: true);
        reviews.Add(review);
        Changed?.Invoke();

        return review;
    }

    /// <summary>Applies a promo code; returns whether it was accepted.</summary>
    public bool ApplyPromoCode(string? code)
    {
        var accepted = string.Equals(code?.Trim(), EcommerceSeedData.PromoCode, StringComparison.OrdinalIgnoreCase);

        if (accepted && AppliedPromoCode is null)
        {
            AppliedPromoCode = EcommerceSeedData.PromoCode;
            Changed?.Invoke();
        }

        return accepted;
    }

    /// <summary>Returns the average rating of a product, or 0 without reviews.</summary>
    public double AverageRating(int productId)
    {
        var ratings = reviews.Where(review => review.ProductId == productId).Select(review => review.Rating).ToList();

        return ratings.Count == 0 ? 0 : ratings.Average();
    }

    /// <summary>Returns the snapshot of the tab's changes: cart, promo code, orders, and reviews written here.</summary>
    public object CaptureData() => new EcommerceSnapshot
    {
        Cart = cart.ToList(),
        PromoCode = AppliedPromoCode,
        Orders = orders.ToList(),
        Reviews = reviews.Where(review => review.IsFromThisSession).ToList(),
        NextOrderId = nextOrderId,
    };

    /// <summary>Returns the totals of the current cart for a delivery speed.</summary>
    public EcommerceTotals CartTotals(EcommerceDelivery delivery = EcommerceDelivery.Standard) =>
        Totals(cart.Select(line => (FindProduct(line.ProductId)!.Price, line.Quantity)), AppliedPromoCode is not null, delivery);

    /// <summary>Returns the products that match the catalog filters, sorted.</summary>
    public IReadOnlyList<EcommerceProduct> FilteredProducts()
    {
        var filter = Filter;
        var term = filter.Search.Trim();

        IEnumerable<EcommerceProduct> query = Products.Where(product =>
            (filter.CategoryIds.Count == 0 || filter.CategoryIds.Contains(product.CategoryId))
            && product.Price <= filter.MaxPrice
            && (!filter.OnSaleOnly || product.IsOnSale)
            && (filter.MinRating == 0 || AverageRating(product.Id) >= filter.MinRating)
            && (term.Length == 0 || product.Name.Contains(term, StringComparison.OrdinalIgnoreCase)));

        query = filter.Sort switch
        {
            EcommerceSort.PriceLowToHigh => query.OrderBy(product => product.Price).ThenBy(product => product.Name),
            EcommerceSort.PriceHighToLow => query.OrderByDescending(product => product.Price).ThenBy(product => product.Name),
            EcommerceSort.TopRated => query.OrderByDescending(product => AverageRating(product.Id)).ThenBy(product => product.Name),
            EcommerceSort.Newest => query.OrderByDescending(product => product.AddedOn).ThenBy(product => product.Name),
            _ => query.OrderByDescending(product => product.IsFeatured).ThenBy(product => product.Name),
        };

        return query.ToList();
    }

    /// <summary>Finds a category by its ID, or returns <see langword="null" />.</summary>
    public EcommerceCategory? FindCategory(string? categoryId) =>
        Categories.FirstOrDefault(category => string.Equals(category.Id, categoryId, StringComparison.OrdinalIgnoreCase));

    /// <summary>Finds an order placed in this session, or returns <see langword="null" />.</summary>
    public EcommerceOrder? FindOrder(int orderId) => orders.FirstOrDefault(order => order.Id == orderId);

    /// <summary>Finds a product by its ID, or returns <see langword="null" />.</summary>
    public EcommerceProduct? FindProduct(int productId) => Products.FirstOrDefault(product => product.Id == productId);

    /// <summary>
    /// Returns the highest quantity one cart line (a product in one color) may hold: the stock left after the product's
    /// other color lines, capped at <see cref="EcommerceSeedData.MaxQuantity" />. Stock is per product, not per color.
    /// </summary>
    public int MaxQuantityFor(EcommerceProduct product, string color)
    {
        var inOtherLines = cart.Where(line => line.ProductId == product.Id && line.Color != color).Sum(line => line.Quantity);

        return Math.Max(0, Math.Min(product.Stock - inOtherLines, EcommerceSeedData.MaxQuantity));
    }

    /// <summary>Raises <see cref="Changed" /> after the catalog filter was edited in place.</summary>
    public void NotifyFilterChanged() => Changed?.Invoke();

    /// <summary>Raises <see cref="Changed" /> after a stored snapshot was applied.</summary>
    public void NotifyRestored() => Changed?.Invoke();

    /// <summary>Turns the cart into an order paid with an approved test card, empties the cart, and returns the order.</summary>
    public EcommerceOrder PlaceOrder(EcommerceAddress shipTo, EcommerceDelivery delivery, ShowcaseCardCharge payment)
    {
        var lines = new List<EcommerceOrderLine>(cart.Count);

        foreach (var line in cart)
        {
            var product = FindProduct(line.ProductId)!;
            lines.Add(new EcommerceOrderLine(product.Id, product.Name, line.Color, line.Quantity, product.Price));
        }

        var order = new EcommerceOrder(nextOrderId++, Clock.Now, lines, shipTo, delivery, AppliedPromoCode, CartTotals(delivery), payment.Brand, payment.Last4);

        orders.Add(order);
        cart.Clear();
        AppliedPromoCode = null;
        Changed?.Invoke();

        return order;
    }

    /// <summary>Returns how many more of a product, in any color, the cart can take before stock runs out.</summary>
    public int RemainingStockFor(EcommerceProduct product) =>
        Math.Max(0, product.Stock - cart.Where(line => line.ProductId == product.Id).Sum(line => line.Quantity));

    /// <summary>Removes the applied promo code.</summary>
    public void RemovePromoCode()
    {
        if (AppliedPromoCode is null)
            return;

        AppliedPromoCode = null;
        Changed?.Invoke();
    }

    /// <summary>Returns the reviews of a product, newest first.</summary>
    public IReadOnlyList<EcommerceReview> ReviewsFor(int productId) =>
        reviews.Where(review => review.ProductId == productId).OrderByDescending(review => review.PostedOn).ThenByDescending(review => review.IsFromThisSession).ToList();

    /// <summary>Sets the quantity of a cart line; zero or less removes it.</summary>
    public void SetQuantity(int productId, string color, int quantity)
    {
        var index = cart.FindIndex(line => line.ProductId == productId && line.Color == color);

        if (index < 0 || FindProduct(productId) is not { } product)
            return;

        if (quantity <= 0)
            cart.RemoveAt(index);
        else
            cart[index] = cart[index] with { Quantity = Math.Min(quantity, MaxQuantityFor(product, cart[index].Color)) };

        Changed?.Invoke();
    }

    /// <summary>Validates a stored snapshot and applies it only when every part is valid.</summary>
    public bool TryRestoreData(JsonElement data)
    {
        EcommerceSnapshot? snapshot;

        try
        {
            snapshot = data.Deserialize<EcommerceSnapshot>(new JsonSerializerOptions(JsonSerializerDefaults.Web) { MaxDepth = 32 });
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException)
        {
            return false;
        }

        if (snapshot?.Cart is not { Count: <= 50 } cartLines
            || snapshot.Orders is not { Count: <= MaxOrders } placed
            || snapshot.Reviews is not { Count: <= MaxSessionReviews } written
            || snapshot.NextOrderId is < FirstOrderId or > FirstOrderId + 100_000
            || (snapshot.PromoCode is not null && snapshot.PromoCode != EcommerceSeedData.PromoCode))
            return false;

        // Cart: known products and colors, sane quantities, no duplicate lines, within stock per product.
        if (cartLines.Any(line => line is null || FindProduct(line.ProductId) is not { } product || line.Color is null || !product.Colors.Contains(line.Color) || line.Quantity is < 1 or > EcommerceSeedData.MaxQuantity)
            || cartLines.GroupBy(line => (line.ProductId, line.Color)).Any(group => group.Count() > 1)
            || cartLines.GroupBy(line => line.ProductId).Any(group => group.Sum(line => line.Quantity) > FindProduct(group.Key)!.Stock))
            return false;

        if (placed.Any(order => !IsValidOrder(order, snapshot.NextOrderId)) || placed.Select(order => order.Id).Distinct().Count() != placed.Count)
            return false;

        if (written.Any(review => review is null || FindProduct(review.ProductId) is null || !review.IsFromThisSession || review.Rating is < 1 or > 5
                || !IsText(review.Author, 60) || !IsText(review.Title, 120) || !IsText(review.Body, 2000) || !IsPlausibleDate(review.PostedOn)))
            return false;

        cart.Clear();
        cart.AddRange(cartLines);
        AppliedPromoCode = snapshot.PromoCode;
        orders.Clear();
        orders.AddRange(placed);
        reviews.RemoveAll(review => review.IsFromThisSession);
        reviews.AddRange(written);
        nextOrderId = snapshot.NextOrderId;

        return true;
    }

    #endregion

    #region Private Methods

    private static bool IsPlausibleDate(DateTime value) => value >= EcommerceSeedData.Today.AddYears(-1) && value <= EcommerceSeedData.Today.AddYears(2);

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

    private static bool IsMoney(decimal value) => value is >= 0 and <= 1_000_000;

    private bool IsValidOrder(EcommerceOrder? order, int nextId) =>
        order is not null
        && order.Id >= FirstOrderId && order.Id < nextId
        && IsPlausibleDate(order.PlacedOn)
        && Enum.IsDefined(order.Delivery)
        && (order.PromoCode is null || order.PromoCode == EcommerceSeedData.PromoCode)
        && order.CardBrand is not null && cardBrands.Contains(order.CardBrand)
        && order.CardLast4 is { Length: 4 } last4 && last4.All(char.IsAsciiDigit)
        && order.Lines is { Count: >= 1 and <= 50 }
        && order.Lines.All(line => line is not null && FindProduct(line.ProductId) is not null && IsText(line.Name, 120) && IsText(line.Color, 40)
            && line.Quantity is >= 1 and <= EcommerceSeedData.MaxQuantity && IsMoney(line.UnitPrice))
        && order.ShipTo is { } address
        && IsText(address.FullName, 60) && IsText(address.Email, 120) && IsText(address.Street, 100) && IsText(address.City, 60) && IsText(address.PostalCode, 10) && IsText(address.Country, 60)
        && order.Totals is { } totals
        && totals.ItemCount is >= 1 and <= 500 && IsMoney(totals.Subtotal) && IsMoney(totals.Discount) && IsMoney(totals.Shipping) && IsMoney(totals.Tax) && IsMoney(totals.Total);

    private static EcommerceTotals Totals(IEnumerable<(decimal Price, int Quantity)> lines, bool hasPromo, EcommerceDelivery delivery)
    {
        var list = lines.ToList();
        var subtotal = list.Sum(line => line.Price * line.Quantity);
        var discount = hasPromo ? Math.Round(subtotal * EcommerceSeedData.PromoRate, 2, MidpointRounding.AwayFromZero) : 0m;
        var afterDiscount = subtotal - discount;

        var shipping = list.Count == 0 ? 0m
            : delivery == EcommerceDelivery.Express ? EcommerceSeedData.ExpressShipping
            : afterDiscount >= EcommerceSeedData.FreeShippingThreshold ? 0m
            : EcommerceSeedData.StandardShipping;

        var tax = Math.Round(afterDiscount * EcommerceSeedData.TaxRate, 2, MidpointRounding.AwayFromZero);

        return new EcommerceTotals(list.Sum(line => line.Quantity), subtotal, discount, shipping, tax, afterDiscount + shipping + tax);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the applied promo code, or <see langword="null" />.</summary>
    public string? AppliedPromoCode { get; private set; }

    /// <summary>Gets the cart lines in the order they were added.</summary>
    public IReadOnlyList<EcommerceCartLine> Cart => cart;

    /// <summary>Gets the number of items in the cart.</summary>
    public int CartCount => cart.Sum(line => line.Quantity);

    /// <summary>Gets the product categories.</summary>
    public IReadOnlyList<EcommerceCategory> Categories { get; } = EcommerceSeedData.Categories();

    /// <summary>Gets the site clock, which starts at <see cref="EcommerceSeedData.SeedTime" />.</summary>
    public ShowcaseClock Clock { get; } = new(EcommerceSeedData.SeedTime);

    /// <summary>Gets the catalog filters.</summary>
    public EcommerceCatalogFilter Filter { get; } = new();

    /// <summary>Gets the demo sign-in: a customer and a retailer account.</summary>
    public ShowcaseIdentity Identity { get; }

    /// <summary>Gets the catalog.</summary>
    public IReadOnlyList<EcommerceProduct> Products { get; } = EcommerceSeedData.Products();

    /// <summary>Gets the site slug used in the storage key.</summary>
    public string SiteSlug => "ecommerce";

    /// <summary>Gets the snapshot format version.</summary>
    public int SnapshotVersion => 1;

    #endregion

    #region Events

    /// <summary>Raised when the cart, filters, reviews, orders, or sign-in change.</summary>
    public event Action? Changed;

    #endregion

    #region Nested Types

    /// <summary>The storefront's part of the tab snapshot.</summary>
    private sealed class EcommerceSnapshot
    {
        #region Properties, Indexers

        public List<EcommerceCartLine>? Cart { get; set; }

        public int NextOrderId { get; set; }

        public List<EcommerceOrder>? Orders { get; set; }

        public string? PromoCode { get; set; }

        public List<EcommerceReview>? Reviews { get; set; }

        #endregion
    }

    #endregion
}
