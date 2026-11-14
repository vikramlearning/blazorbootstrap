namespace BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;

/// <summary>Delivery speed chosen at checkout.</summary>
public enum EcommerceDelivery
{
    /// <summary>3–5 business days; free over the free-shipping threshold.</summary>
    Standard,

    /// <summary>1–2 business days; always charged.</summary>
    Express,
}

/// <summary>Catalog sort order.</summary>
public enum EcommerceSort
{
    /// <summary>Featured products first, then by name.</summary>
    Featured,

    /// <summary>Lowest price first.</summary>
    PriceLowToHigh,

    /// <summary>Highest price first.</summary>
    PriceHighToLow,

    /// <summary>Highest average rating first.</summary>
    TopRated,

    /// <summary>Most recently added first.</summary>
    Newest,
}

/// <summary>A product category.</summary>
public sealed record EcommerceCategory(string Id, string Name, string Tagline, IconName IconName, string Accent);

/// <summary>A product in the catalog.</summary>
public sealed record EcommerceProduct(
    int Id,
    string Name,
    string CategoryId,
    decimal Price,
    decimal? CompareAtPrice,
    string Description,
    IReadOnlyList<string> Highlights,
    IReadOnlyList<string> Colors,
    bool IsFeatured,
    int Stock,
    DateTime AddedOn)
{
    #region Properties, Indexers

    /// <summary>Gets the discount against the compare-at price, in whole percent (0 when not on sale).</summary>
    public int DiscountPercent => IsOnSale ? (int)Math.Round((1 - (Price / CompareAtPrice!.Value)) * 100) : 0;

    /// <summary>Gets whether the product is cheaper than its compare-at price.</summary>
    public bool IsOnSale => CompareAtPrice is decimal compareAt && compareAt > Price;

    /// <summary>Gets the image path; <c>ShowcaseImage</c> shows a placeholder until the developer supplies the file.</summary>
    public string ImagePath => $"_content/BlazorBootstrap.Demo.RCL/showcase/ecommerce/products/{Id}.webp";

    #endregion
}

/// <summary>A customer review of a product.</summary>
public sealed record EcommerceReview(int ProductId, string Author, int Rating, string Title, string Body, DateTime PostedOn, bool IsFromThisSession);

/// <summary>One product and color in the cart.</summary>
public sealed record EcommerceCartLine(int ProductId, string Color, int Quantity);

/// <summary>Where an order ships, as typed at checkout. Kept only in memory.</summary>
public sealed record EcommerceAddress(string FullName, string Email, string Street, string City, string PostalCode, string Country);

/// <summary>One line of a placed order, with the price paid.</summary>
public sealed record EcommerceOrderLine(int ProductId, string Name, string Color, int Quantity, decimal UnitPrice);

/// <summary>Money totals of a cart or order.</summary>
public sealed record EcommerceTotals(int ItemCount, decimal Subtotal, decimal Discount, decimal Shipping, decimal Tax, decimal Total);

/// <summary>An order placed in this session. Only the card's brand and last four digits are kept.</summary>
public sealed record EcommerceOrder(
    int Id,
    DateTime PlacedOn,
    IReadOnlyList<EcommerceOrderLine> Lines,
    EcommerceAddress ShipTo,
    EcommerceDelivery Delivery,
    string? PromoCode,
    EcommerceTotals Totals,
    string CardBrand,
    string CardLast4);

/// <summary>The catalog filters, kept in the site state so they survive a visit to a product page.</summary>
public sealed class EcommerceCatalogFilter
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the last <c>?category=</c> / <c>?sale=</c> query applied to the filter. Kept in the state, so coming back
    /// to the same URL (browser Back, the breadcrumb) keeps the filters set since; only a different query starts fresh.
    /// </summary>
    public string? AppliedQuery { get; set; }

    /// <summary>Gets the selected category IDs; empty means all categories.</summary>
    public HashSet<string> CategoryIds { get; } = new();

    /// <summary>Gets or sets the highest price shown.</summary>
    public int MaxPrice { get; set; } = 400;

    /// <summary>Gets or sets the lowest average rating shown (0 for any).</summary>
    public int MinRating { get; set; }

    /// <summary>Gets or sets whether only discounted products are shown.</summary>
    public bool OnSaleOnly { get; set; }

    /// <summary>Gets or sets the current page, starting at 1.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Gets or sets the search text.</summary>
    public string Search { get; set; } = string.Empty;

    /// <summary>Gets or sets the sort order.</summary>
    public EcommerceSort Sort { get; set; } = EcommerceSort.Featured;

    #endregion
}
