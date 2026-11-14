using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

/// <summary>
/// In-memory state of the storefront for the current session: catalog filters, cart, reviews written here, and placed orders.
/// Nothing is persisted, sent, or charged.
/// </summary>
public sealed class EcommerceState
{
    #region Fields and Constants

    private readonly List<EcommerceCartLine> cart = new();

    private readonly List<EcommerceOrder> orders = new();

    private readonly List<EcommerceReview> reviews;

    private int nextOrderId = 10481;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh session.</summary>
    public EcommerceState() => reviews = EcommerceSeedData.Reviews(Products);

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
        var updated = Math.Min(current + quantity, MaxQuantityFor(product));

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
        var review = new EcommerceReview(productId, author.Trim(), Math.Clamp(rating, 1, 5), title.Trim(), body.Trim(), EcommerceSeedData.Today, IsFromThisSession: true);
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

    /// <summary>Returns the highest quantity of one cart line for a product.</summary>
    public int MaxQuantityFor(EcommerceProduct product) => Math.Min(product.Stock, EcommerceSeedData.MaxQuantity);

    /// <summary>Raises <see cref="Changed" /> after the catalog filter was edited in place.</summary>
    public void NotifyFilterChanged() => Changed?.Invoke();

    /// <summary>Turns the cart into an order, empties the cart, and returns the order.</summary>
    public EcommerceOrder PlaceOrder(EcommerceAddress shipTo, EcommerceDelivery delivery)
    {
        var lines = new List<EcommerceOrderLine>(cart.Count);

        foreach (var line in cart)
        {
            var product = FindProduct(line.ProductId)!;
            lines.Add(new EcommerceOrderLine(product.Id, product.Name, line.Color, line.Quantity, product.Price));
        }

        var order = new EcommerceOrder(
            nextOrderId++,
            EcommerceSeedData.Today.AddHours(10).AddMinutes(orders.Count * 7),
            lines,
            shipTo,
            delivery,
            AppliedPromoCode,
            CartTotals(delivery));

        orders.Add(order);
        cart.Clear();
        AppliedPromoCode = null;
        Changed?.Invoke();

        return order;
    }

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
            cart[index] = cart[index] with { Quantity = Math.Min(quantity, MaxQuantityFor(product)) };

        Changed?.Invoke();
    }

    #endregion

    #region Private Methods

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

    /// <summary>Gets the catalog filters.</summary>
    public EcommerceCatalogFilter Filter { get; } = new();

    /// <summary>Gets the catalog.</summary>
    public IReadOnlyList<EcommerceProduct> Products { get; } = EcommerceSeedData.Products();

    #endregion

    #region Events

    /// <summary>Raised when the cart, filters, reviews, or orders change.</summary>
    public event Action? Changed;

    #endregion
}
