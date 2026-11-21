namespace BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;

/// <summary>Delivery speed chosen at checkout.</summary>
public enum EcommerceDelivery
{
    /// <summary>3–5 business days; free over the free-shipping threshold.</summary>
    Standard,

    /// <summary>1–2 business days; always charged.</summary>
    Express,

    /// <summary>Same day by courier from the store, for addresses in the local delivery area; always charged.</summary>
    Local,
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

/// <summary>
/// A product in the catalog. <paramref name="Sku" /> is the stock-keeping unit, unique in the store (like KG-KIT-0101);
/// <paramref name="ImageId" /> names the library photo a size or edition shares with its original product (its own ID when
/// <see langword="null" />), and <paramref name="ImageAlt" /> is the photo's text alternative.
/// </summary>
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
    DateTime AddedOn,
    bool IsArchived = false,
    string Sku = "",
    int? ImageId = null,
    string? ImageAlt = null)
{
    #region Fields and Constants

    /// <summary>The first product photo in the store's licensed photo library.</summary>
    public const int FirstLibraryPhotoId = 101;

    /// <summary>The last product photo in the store's licensed photo library.</summary>
    public const int LastLibraryPhotoId = 131;

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the discount against the compare-at price, in whole percent (0 when not on sale).</summary>
    public int DiscountPercent => IsOnSale ? (int)Math.Round((1 - (Price / CompareAtPrice!.Value)) * 100) : 0;

    /// <summary>Gets whether the product has a photo from the library (a product added in the tab shows a placeholder).</summary>
    public bool HasPhoto => (ImageId ?? Id) is >= FirstLibraryPhotoId and <= LastLibraryPhotoId;

    /// <summary>Gets whether the product is cheaper than its compare-at price.</summary>
    public bool IsOnSale => CompareAtPrice is decimal compareAt && compareAt > Price;

    /// <summary>Gets the image path; <c>ShowcaseImage</c> shows a placeholder until the developer supplies the file.</summary>
    public string ImagePath => $"_content/BlazorBootstrap.Demo.RCL/showcase/ecommerce/products/{ImageId ?? Id}.webp";

    #endregion
}

/// <summary>A customer review of a product, with the store's moderation status and optional public reply.</summary>
public sealed record EcommerceReview(int Id, int ProductId, string Author, int Rating, string Title, string Body, DateTime PostedOn, bool IsFromThisSession, EcommerceReviewStatus Status = EcommerceReviewStatus.Published, string? Reply = null, string? AuthorAccountId = null);

/// <summary>One product and color in the cart.</summary>
public sealed record EcommerceCartLine(int ProductId, string Color, int Quantity);

/// <summary>Where an order ships, as typed at checkout. Kept in the tab's state and its <c>sessionStorage</c> snapshot only.</summary>
public sealed record EcommerceAddress(string FullName, string Email, string Street, string City, string PostalCode, string Country);

/// <summary>One line of a placed order, with the price paid.</summary>
public sealed record EcommerceOrderLine(int ProductId, string Name, string Color, int Quantity, decimal UnitPrice);

/// <summary>Money totals of a cart or order.</summary>
public sealed record EcommerceTotals(int ItemCount, decimal Subtotal, decimal Discount, decimal Shipping, decimal Tax, decimal Total);

/// <summary>An order with its fulfilment status and history. Only the card's brand and last four digits are kept.</summary>
public sealed record EcommerceOrder(
    int Id,
    DateTime PlacedOn,
    IReadOnlyList<EcommerceOrderLine> Lines,
    EcommerceAddress ShipTo,
    EcommerceDelivery Delivery,
    string? PromoCode,
    EcommerceTotals Totals,
    string CardBrand,
    string CardLast4,
    EcommerceOrderStatus Status,
    IReadOnlyList<EcommerceStatusChange> History,
    string? TrackingNumber = null,
    string? CustomerId = null,
    decimal RefundedAmount = 0m);

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

/// <summary>Where an order is in fulfilment. Placed → Packed → Shipped → Delivered; Cancelled before shipping; Refunded after.</summary>
public enum EcommerceOrderStatus
{
    /// <summary>Paid and waiting to be packed.</summary>
    Placed,

    /// <summary>Packed and waiting for the carrier.</summary>
    Packed,

    /// <summary>With the carrier, with a tracking number.</summary>
    Shipped,

    /// <summary>Delivered to the customer.</summary>
    Delivered,

    /// <summary>Cancelled before it shipped; stock went back.</summary>
    Cancelled,

    /// <summary>Refunded in full after it shipped.</summary>
    Refunded,
}

/// <summary>One step in an order's history.</summary>
public sealed record EcommerceStatusChange(EcommerceOrderStatus Status, DateTime At, string? Note);

/// <summary>Whether a review shows on the product page.</summary>
public enum EcommerceReviewStatus
{
    /// <summary>Shown to shoppers and counted in the rating.</summary>
    Published,

    /// <summary>Hidden by the store; not counted in the rating.</summary>
    Hidden,
}

/// <summary>How a coupon discounts the subtotal.</summary>
public enum EcommerceCouponKind
{
    /// <summary>A percentage of the subtotal.</summary>
    Percent,

    /// <summary>A fixed amount, at most the subtotal.</summary>
    Amount,
}

/// <summary>A coupon the store offers; it works from <paramref name="StartsOn" /> (any time when empty) through the end of <paramref name="ExpiresOn" />.</summary>
public sealed record EcommerceCoupon(
    string Code,
    string Description,
    EcommerceCouponKind Kind,
    decimal Value,
    decimal MinSubtotal,
    bool FirstOrderOnly,
    DateTime? ExpiresOn,
    bool IsActive,
    int? UsageLimit,
    int Uses,
    DateTime? StartsOn = null);

/// <summary>The outcome of checking a coupon code against the cart.</summary>
public enum EcommerceCouponCheck
{
    /// <summary>The coupon applies.</summary>
    Applied,

    /// <summary>No coupon has this code.</summary>
    NotFound,

    /// <summary>The store turned the coupon off.</summary>
    Inactive,

    /// <summary>The coupon has expired.</summary>
    Expired,

    /// <summary>The coupon's start date hasn't come yet.</summary>
    NotStarted,

    /// <summary>The coupon has been used as many times as allowed.</summary>
    UsedUp,

    /// <summary>The cart subtotal is below the coupon's minimum.</summary>
    BelowMinimum,

    /// <summary>The coupon is for a first order, and this customer has ordered before.</summary>
    NotFirstOrder,
}

/// <summary>Where a return request is.</summary>
public enum EcommerceReturnStatus
{
    /// <summary>Waiting for the store.</summary>
    Requested,

    /// <summary>Approved: the items went back into stock and the refund was issued.</summary>
    Approved,

    /// <summary>Declined by the store, with a note.</summary>
    Rejected,
}

/// <summary>One line of a return request.</summary>
public sealed record EcommerceReturnLine(int ProductId, string Name, string Color, int Quantity, decimal UnitPrice);

/// <summary>
/// A customer's request to return items from a delivered order. An approved return refunds
/// <paramref name="RefundAmount" />: the lines' total, or less (a restocking fee, for example) with a note saying why.
/// </summary>
public sealed record EcommerceReturn(
    int Id,
    int OrderId,
    IReadOnlyList<EcommerceReturnLine> Lines,
    string Reason,
    string? Comment,
    EcommerceReturnStatus Status,
    DateTime RequestedOn,
    DateTime? DecidedOn = null,
    string? DecisionNote = null,
    decimal? RefundAmount = null)
{
    #region Properties, Indexers

    /// <summary>Gets the lines' total: the most an approval can refund.</summary>
    public decimal Amount => Lines.Sum(line => line.UnitPrice * line.Quantity);

    /// <summary>Gets the amount refunded: the approved refund (the lines' total unless the store set less), 0 otherwise.</summary>
    public decimal Refunded => Status == EcommerceReturnStatus.Approved ? RefundAmount ?? Amount : 0m;

    #endregion
}

/// <summary>A customer as the store sees one: from accounts and from orders placed as a guest.</summary>
public sealed record EcommerceCustomerSummary(string Email, string Name, string? AccountId, int OrderCount, decimal Spent, DateTime? LastOrderOn, string? Country = null, DateTime? FirstOrderOn = null);

/// <summary>The outcome of a bulk action on a list's selected records.</summary>
public enum EcommerceBulkResult
{
    /// <summary>At least one record changed.</summary>
    Ok,

    /// <summary>No record changed: each was already like that, or isn't in a state the action applies to.</summary>
    NoChange,

    /// <summary>Only the retailer may do this.</summary>
    NotAllowed,

    /// <summary>Nothing was selected, or a value (a reason, a category) is missing or out of range.</summary>
    Invalid,
}

/// <summary>What a bulk action did: its result, how many records changed, and how many were left as they were.</summary>
public sealed record EcommerceBulkOutcome(EcommerceBulkResult Result, int Changed, int Unchanged);

/// <summary>The product editor's form: everything the retailer can change about a product.</summary>
public sealed class EcommerceProductDraft
{
    #region Properties, Indexers

    /// <summary>Gets or sets the category ID.</summary>
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>Gets or sets the colors, comma-separated.</summary>
    public string Colors { get; set; } = string.Empty;

    /// <summary>Gets or sets the compare-at ("was") price, or <see langword="null" /> when not on sale.</summary>
    public decimal? CompareAtPrice { get; set; }

    /// <summary>Gets or sets the description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the highlights, one per line.</summary>
    public string Highlights { get; set; } = string.Empty;

    /// <summary>Gets or sets the photo's text alternative (kept only for a product with a library photo).</summary>
    public string? ImageAlt { get; set; }

    /// <summary>Gets or sets whether the product is featured on the home page.</summary>
    public bool IsFeatured { get; set; }

    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the price.</summary>
    public decimal Price { get; set; }

    /// <summary>Gets or sets the stock-keeping unit, unique in the store.</summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>Gets or sets the units in stock.</summary>
    public int Stock { get; set; }

    #endregion
}

/// <summary>An address in a customer's address book.</summary>
public sealed record EcommerceSavedAddress(int Id, string AccountId, string Label, string FullName, string Street, string City, string PostalCode, string Country, string? Phone, bool IsDefault);

/// <summary>A product on a customer's wishlist; <c>NotifyWhenInStock</c> asks for a notice when an out-of-stock product returns.</summary>
public sealed record EcommerceWishItem(string AccountId, int ProductId, DateTime AddedOn, bool NotifyWhenInStock);

/// <summary>
/// A notice for an account that can sign in, shown in the header's bell (and, for a customer, on the account overview):
/// for a customer, an order moved on, a return was decided, or a wished-for product is back in stock; for the retailer,
/// something to act on (an order to fulfil, a return to decide, low stock, a review to moderate).
/// </summary>
public sealed record EcommerceNotice(int Id, string AccountId, string Title, string Text, string? Link, DateTime CreatedOn, bool IsRead);