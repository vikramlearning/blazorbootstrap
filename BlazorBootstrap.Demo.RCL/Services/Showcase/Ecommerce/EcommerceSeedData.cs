using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

/// <summary>
/// Fixed, fictitious content for the "Kindred Goods" store and its back office. Every value is a constant or derived from
/// an index, so each load shows the same catalog, a year of orders, customers, coupons, returns, reviews, the audit log,
/// and the demo accounts' notifications (rule WR-001: hundreds of records on the main lists).
/// </summary>
public static partial class EcommerceSeedData
{
    #region Fields and Constants

    /// <summary>The back office's base route (the same as <c>EcommerceAdminLayout.Base</c>).</summary>
    public const string AdminBase = SiteBase + "/admin";

    /// <summary>The role ID of shoppers.</summary>
    public const string CustomerRole = "customer";

    /// <summary>The demo customer's account ID.</summary>
    public const string DemoCustomerId = "maya";

    /// <summary>The demo retailer's account ID.</summary>
    public const string DemoRetailerId = "owen";

    /// <summary>Express shipping price.</summary>
    public const decimal ExpressShipping = 14.95m;

    /// <summary>The ID of the first seeded order; the year of history runs up to <see cref="EcommerceState.FirstNewOrderId" />.</summary>
    public const int FirstHistoryOrderId = 10001;

    /// <summary>Order subtotal (after discount) from which standard shipping is free.</summary>
    public const decimal FreeShippingThreshold = 75m;

    /// <summary>How many orders the year of history holds.</summary>
    public const int HistoryOrderCount = 480;

    /// <summary>Same-day local delivery price.</summary>
    public const decimal LocalDeliveryFee = 9.95m;

    /// <summary>The country of the local delivery area.</summary>
    public const string LocalDeliveryCountry = "United States";

    /// <summary>
    /// The reorder point a product starts with when it has no replenishment settings yet (a product added in the tab):
    /// at or below it, the product counts as low and is flagged for reordering.
    /// </summary>
    public const int LowStockThreshold = 5;

    /// <summary>The reorder quantity a product starts with when it has no replenishment settings yet.</summary>
    public const int DefaultReorderQuantity = 20;

    /// <summary>Highest quantity of one line in the cart.</summary>
    public const int MaxQuantity = 10;

    /// <summary>The most addresses one customer can keep in their address book.</summary>
    public const int MaxSavedAddresses = 10;

    /// <summary>The demo promo code.</summary>
    public const string PromoCode = "WELCOME10";

    /// <summary>The promo code's discount on the subtotal.</summary>
    public const decimal PromoRate = .10m;

    /// <summary>The role ID of the store team.</summary>
    public const string RetailerRole = "retailer";

    /// <summary>How many customers (besides the demo customer) the seeded orders come from.</summary>
    public const int SeededCustomerCount = 220;

    /// <summary>The storefront's base route (the same as <c>EcommerceLayout.Base</c>).</summary>
    public const string SiteBase = DemoRouteConstants.Showcase_URL_Ecommerce;

    /// <summary>Standard shipping price below the free-shipping threshold.</summary>
    public const decimal StandardShipping = 6.95m;

    /// <summary>The fictitious store name.</summary>
    public const string StoreName = "Kindred Goods";

    /// <summary>Who the audit log names for what the store does by itself (deliveries the carrier confirms).</summary>
    public const string SystemActor = "Kindred Goods system";

    /// <summary>Sales tax rate applied after discounts.</summary>
    public const decimal TaxRate = .08m;

    /// <summary>The countries checkout and the address book offer.</summary>
    public static readonly IReadOnlyList<string> Countries = new[] { "United States", "Canada", "United Kingdom", "Germany", "India", "Australia" };

    /// <summary>The cities the store's couriers reach the same day.</summary>
    public static readonly IReadOnlyList<string> LocalDeliveryCities = new[] { "Fairbank", "Brookhaven" };

    /// <summary>The fixed "today" all dates on the site are relative to.</summary>
    public static readonly DateTime Today = new(2026, 11, 9);

    /// <summary>The site clock's start: 10:00 on <see cref="Today" />; the clock then moves with real time.</summary>
    public static readonly DateTime SeedTime = Today.AddHours(10);

    /// <summary>Where the store ships from: the fictitious Kindred Goods workshop in Fairbank.</summary>
    public static readonly (string Name, string Street, double Latitude, double Longitude) Store = ("Kindred Goods workshop", "120 Mill Street, Fairbank", 45.5231, -122.6765);

    private static readonly string[] authors =
    {
        "Maya R.", "Theo L.", "Priya S.", "Jonah W.", "Elena V.", "Sam K.", "Ines M.", "Callum B.", "Noor A.", "Hana T.", "Felix D.", "Rosa G.",
    };

    private static readonly (string Brand, string Last4)[] cards = { ("Visa", "4242"), ("Mastercard", "4444"), ("American Express", "0005") };

    private static readonly (string City, string Country, string Postal)[] cities =
    {
        ("Fairbank", "United States", "104{0:00}"),
        ("Brookhaven", "United States", "209{0:00}"),
        ("Cedar Ridge", "United States", "331{0:00}"),
        ("Port Hollis", "United States", "481{0:00}"),
        ("Millbrook", "United States", "602{0:00}"),
        ("Maple Falls", "Canada", "M4B 1B{0:0}"),
        ("Lakeview", "Canada", "V6K 2L{0:0}"),
        ("Saltmere", "United Kingdom", "SM{0:0} 4AA"),
        ("Northwick", "United Kingdom", "NW{0:0} 2BT"),
        ("Lindenfeld", "Germany", "101{0:00}"),
        ("Coral Bay", "India", "5600{0:00}"),
        ("Clearwater", "Australia", "20{0:00}"),
    };

    private static readonly (string Code, string Name)[] categoryCodes =
    {
        ("kitchen", "KIT"), ("living", "LIV"), ("outdoor", "OUT"), ("workspace", "WRK"), ("bath", "BTH"),
    };

    private static readonly string[] firstNames =
    {
        "Theo", "Priya", "Jonah", "Elena", "Sam", "Ines", "Callum", "Noor", "Hana", "Felix", "Rosa", "Mateo",
        "Aisha", "Lukas", "Chloe", "Ravi", "Freya", "Omar", "Leila", "Henrik", "Sofia", "Tomas", "Yara", "Daniel",
    };

    private static readonly string[] lastNames =
    {
        "Ramsay", "Shah", "Wells", "Vargas", "Kowalski", "Moreau", "Byrne", "Aziz", "Tanaka", "Novak",
        "Okafor", "Lindgren", "Costa", "Brennan", "Iyer", "Holm", "Fischer", "Mensah", "Duarte", "Keller",
    };

    private static readonly int[] ratingPattern = { 5, 4, 5, 3, 5, 4, 2, 5, 4, 5, 4, 3 };

    private static readonly string[] rejectNotes =
    {
        "Outside our 30-day return window.", "The item shows signs of use, so it can't be resold.", "Final-sale items can't be returned.",
    };

    private static readonly (string Reason, string? Comment)[] returnReasons =
    {
        ("Doesn't match the photos", "The color is warmer than on screen."),
        ("Arrived damaged", "One corner was chipped in transit."),
        ("Changed my mind", null),
        ("Wrong size", "Too big for our table."),
        ("Not as described", "The finish is glossier than the description says."),
        ("Arrived late", "It came after the party it was for."),
    };

    private static readonly (string Title, string Body)[] reviewsByRating =
    {
        ("Not for me", "It looked different from the photos and felt lighter than I expected. Returning it was easy, at least."),
        ("Okay, with caveats", "Nice design, but the finish marked quicker than I hoped. Fine for the price."),
        ("Does the job", "Solid and simple. Delivery took a little longer than estimated, but it arrived well packed."),
        ("Really happy with it", "Great quality for the price and it matches the rest of the room. Would buy again."),
        ("Love it", "Beautifully made and it feels like it will last for years. Already ordered a second one as a gift."),
    };

    private static readonly string[] streets =
    {
        "Alder Row", "Willow Court", "Orchard Lane", "Harbor Street", "Cedar Avenue", "Birch Close", "Linden Road", "Heather Way",
        "Juniper Street", "Cypress Lane", "Mill Street", "Kiln Street", "Quarry Road", "Elm Terrace", "Station Road", "Rowan Drive",
    };

    /// <summary>The sizes and editions of the original products, each with its price factor (they share the original's photo).</summary>
    private static readonly (string Suffix, decimal Factor)[] variants = { ("Large", 1.35m), ("Small", .7m), ("Studio Edition", 1.2m) };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the back office's page of an order.</summary>
    public static string AdminOrderPath(int orderId) => string.Create(CultureInfo.InvariantCulture, $"{AdminBase}/orders/{orderId}");

    /// <summary>Returns the back office's editor of a product.</summary>
    public static string AdminProductPath(int productId) => string.Create(CultureInfo.InvariantCulture, $"{AdminBase}/products/{productId}");

    /// <summary>Returns the back office's page of a return.</summary>
    public static string AdminReturnPath(int returnId) => string.Create(CultureInfo.InvariantCulture, $"{AdminBase}/returns/{returnId}");

    /// <summary>
    /// Builds the whole seed once: the catalog, reviews, coupons with their uses, a year of orders, returns (approved ones
    /// refunded on their orders), suppliers, replenishment settings, purchase orders, the audit log, and the demo accounts'
    /// notifications. Every tab starts from it, and the tab snapshot stores only what differs from it.
    /// </summary>
    public static EcommerceSeedSet Build()
    {
        var products = Products();
        var reviews = Reviews(products);
        var customers = Customers();
        var (orders, coupons) = HistoryOrders(products, Coupons(), customers);
        var returns = Returns(orders);

        // An approved return was refunded on its order.
        foreach (var approved in returns.Where(item => item.Status == EcommerceReturnStatus.Approved))
        {
            var index = orders.FindIndex(order => order.Id == approved.OrderId);
            orders[index] = orders[index] with { RefundedAmount = Math.Min(orders[index].Totals.Total, orders[index].RefundedAmount + approved.Refunded) };
        }

        var suppliers = Suppliers();
        var replenishment = Replenishment(products);
        var purchaseOrders = PurchaseOrders(products, suppliers, replenishment);
        var audit = Audit(products, coupons, orders, returns, suppliers, purchaseOrders);
        var notices = Notices(products, reviews, orders, returns, replenishment, suppliers, purchaseOrders);

        return new EcommerceSeedSet(products, reviews, coupons, orders, returns, notices, audit, suppliers, replenishment, purchaseOrders);
    }

    /// <summary>Returns the role IDs of the store team: the retailer and the purchasing manager.</summary>
    public static IReadOnlyList<string> StaffRoles() => new[] { RetailerRole, PurchasingRole };

    /// <summary>
    /// The demo accounts listed on the sign-in page, the back office first: the retailer, the purchasing manager (who approves
    /// large purchase orders), then the customer. Fictitious people with <c>.example</c> addresses.
    /// </summary>
    public static IReadOnlyList<ShowcaseDemoAccount> DemoAccounts() => new List<ShowcaseDemoAccount>
    {
        new(DemoRetailerId, RetailerRole, "Owen Hart", "owen@kindredgoods.example", "store-demo-24", "OH"),
        new(DemoPurchasingId, PurchasingRole, "Lena Brooks", "lena@kindredgoods.example", "buy-demo-24", "LB"),
        new(DemoCustomerId, CustomerRole, "Maya Lindqvist", "maya@kindredgoods.example", "shop-demo-24", "ML"),
    };

    /// <summary>The site's roles, the back office first.</summary>
    public static IReadOnlyList<ShowcaseRole> Roles() => new List<ShowcaseRole>
    {
        new(RetailerRole, "Retailer", AdminBase, $"{AdminBase}/account",
            "Runs the back office: fulfils orders, decides returns, sets prices and offers, manages products, customers, reviews, and stock, raises purchase orders, and reads analytics and the audit log."),
        new(PurchasingRole, "Purchasing manager", AdminBase, $"{AdminBase}/account",
            "Works the whole back office and approves purchase orders over $5,000 that someone else raised; keeps suppliers and reorder points."),
        new(CustomerRole, "Customer", $"{SiteBase}/account", $"{SiteBase}/account", "Shops the storefront, checks out, and tracks orders, returns, and reviews."),
    };

    /// <summary>The product categories.</summary>
    public static IReadOnlyList<EcommerceCategory> Categories() => new List<EcommerceCategory>
    {
        new("kitchen", "Kitchen", "Tools and tableware for slow mornings", IconName.CupHot, "#c2410c"),
        new("living", "Living", "Soft textures and warm light", IconName.Lamp, "#a16207"),
        new("outdoor", "Outdoor", "For porches, parks, and campfires", IconName.Tree, "#15803d"),
        new("workspace", "Workspace", "A calmer, tidier desk", IconName.Laptop, "#1d4ed8"),
        new("bath", "Bath", "Spa days at home", IconName.Droplet, "#0e7490"),
    };

    /// <summary>Returns the discount a coupon gives on a subtotal: a percentage rounded to the cent, or an amount up to the subtotal.</summary>
    public static decimal CouponDiscount(EcommerceCoupon coupon, decimal subtotal) => coupon.Kind == EcommerceCouponKind.Percent
        ? Math.Round(subtotal * coupon.Value / 100m, 2, MidpointRounding.AwayFromZero)
        : Math.Min(coupon.Value, subtotal);

    /// <summary>Returns whether an account ID belongs to one of the seeded registered customers (<c>c-0001</c> to <c>c-0220</c>), who have no sign-in.</summary>
    public static bool IsSeededCustomerId(string? accountId) =>
        accountId is { Length: 6 } && accountId.StartsWith("c-", StringComparison.Ordinal)
        && int.TryParse(accountId.AsSpan(2), NumberStyles.None, CultureInfo.InvariantCulture, out var number) && number is >= 1 and <= SeededCustomerCount;

    /// <summary>Formats an amount in US dollars with cents.</summary>
    public static string Money(decimal value) => value.ToString("$#,0.00", CultureInfo.InvariantCulture);

    /// <summary>Returns what a customer's notice says about their order's new status.</summary>
    public static string OrderNoticeText(EcommerceOrder order) => order.Status switch
    {
        EcommerceOrderStatus.Packed => "It's packed and waiting for the carrier.",
        EcommerceOrderStatus.Shipped => $"It's on its way. Tracking number {order.TrackingNumber}.",
        EcommerceOrderStatus.Delivered => "It was delivered. Enjoy, and tell us what you think in a review.",
        EcommerceOrderStatus.Cancelled => "It was cancelled before it shipped, and the payment was refunded.",
        EcommerceOrderStatus.Refunded => "It was refunded in full to your card.",
        _ => "We've received it.",
    };

    /// <summary>Returns the storefront's page of a customer's order.</summary>
    public static string OrderPath(int orderId) => string.Create(CultureInfo.InvariantCulture, $"{SiteBase}/orders/{orderId}");

    /// <summary>Returns a return's number, like "R-0012".</summary>
    public static string ReturnNumber(int returnId) => string.Create(CultureInfo.InvariantCulture, $"R-{returnId:0000}");

    /// <summary>Returns the words after "Order #10481" in a notice about a status, like "has shipped".</summary>
    public static string StatusVerb(EcommerceOrderStatus status) => status switch
    {
        EcommerceOrderStatus.Packed => "is packed",
        EcommerceOrderStatus.Shipped => "has shipped",
        EcommerceOrderStatus.Delivered => "was delivered",
        EcommerceOrderStatus.Cancelled => "was cancelled",
        EcommerceOrderStatus.Refunded => "was refunded",
        _ => "was placed",
    };

    /// <summary>
    /// The catalog: the 31 original products, then 89 sizes and editions of them (Large, Small, Studio Edition) that share
    /// the original's library photo, 120 in all. A few are out of stock, running low, or archived; each has a SKU like
    /// KG-KIT-0101 and a photo description.
    /// </summary>
    public static IReadOnlyList<EcommerceProduct> Products()
    {
        var rows = new (string Name, string Category, decimal Price, decimal? CompareAt)[]
        {
            ("Oak Edge Cutting Board", "kitchen", 48m, 60m),
            ("Stoneware Pour-Over Set", "kitchen", 64m, null),
            ("Copper Core Skillet", "kitchen", 129m, 159m),
            ("Linen Tea Towels, Set of 3", "kitchen", 24m, null),
            ("Ceramic Canister Trio", "kitchen", 56m, null),
            ("Walnut Salad Servers", "kitchen", 32m, 40m),
            ("Glass Storage Jars", "kitchen", 38m, null),
            ("Boucle Accent Chair", "living", 349m, 429m),
            ("Wool Throw Blanket", "living", 89m, null),
            ("Arc Floor Lamp", "living", 179m, null),
            ("Rattan Side Table", "living", 119m, 149m),
            ("Linen Cushion Cover", "living", 29m, null),
            ("Stoneware Bud Vase", "living", 46m, null),
            ("Folding Teak Chair", "outdoor", 149m, null),
            ("Solar String Lights", "outdoor", 39m, 49m),
            ("Insulated Picnic Tote", "outdoor", 54m, null),
            ("Canvas Hammock", "outdoor", 89m, 109m),
            ("Cast Iron Fire Bowl", "outdoor", 199m, null),
            ("Enamel Camp Mug", "outdoor", 18m, null),
            ("Bamboo Monitor Riser", "workspace", 59m, null),
            ("Felt Desk Mat", "workspace", 34m, 42m),
            ("Ergonomic Task Chair", "workspace", 289m, null),
            ("Brass Desk Lamp", "workspace", 98m, null),
            ("Cork Pinboard", "workspace", 27m, null),
            ("Leather Cable Organizer", "workspace", 22m, 28m),
            ("Waffle Bath Towel", "bath", 36m, null),
            ("Teak Shower Bench", "bath", 129m, 159m),
            ("Ceramic Soap Dispenser", "bath", 26m, null),
            ("Organic Cotton Robe", "bath", 98m, null),
            ("Pebble Bath Mat", "bath", 44m, 55m),
            ("Eucalyptus Candle", "bath", 28m, null),
        };

        var colorsByCategory = new Dictionary<string, string[]>
        {
            ["kitchen"] = new[] { "Natural", "Charcoal" },
            ["living"] = new[] { "Oat", "Sage", "Clay" },
            ["outdoor"] = new[] { "Natural", "Forest" },
            ["workspace"] = new[] { "Black", "Natural" },
            ["bath"] = new[] { "White", "Sand", "Slate" },
        };

        var products = new List<EcommerceProduct>(120);

        for (var index = 0; index < rows.Length; index++)
        {
            var (name, category, price, compareAt) = rows[index];
            var id = 101 + index;
            var colors = index % 3 == 2 ? colorsByCategory[category].Take(1).ToArray() : colorsByCategory[category];
            var stock = index % 13 == 12 ? 0 : index % 9 == 4 ? 3 : 20 + (index * 7 % 30);

            // A third of the originals are new this season; the rest have sold for about a year.
            var addedDaysAgo = 4 + (index * 11 % 90) + (index % 3 == 0 ? 0 : 300);

            products.Add(new EcommerceProduct(
                id,
                name,
                category,
                price,
                compareAt,
                Describe(name, category),
                Highlights(category),
                colors,
                IsFeatured: index % 4 == 0,
                stock,
                Today.AddDays(-addedDaysAgo),
                Sku: SkuFor(category, id),
                ImageAlt: $"{name} in {colors[0]}"));
        }

        for (var index = 0; index < 89; index++)
        {
            var original = products[index % rows.Length];
            var (suffix, factor) = variants[index / rows.Length];
            var id = 132 + index;
            var price = Math.Max(8m, Math.Round(original.Price * factor, 0, MidpointRounding.AwayFromZero));
            decimal? compareAt = original.CompareAtPrice is { } was && suffix != "Small" ? Math.Round(was * factor, 0, MidpointRounding.AwayFromZero) : null;
            var name = $"{original.Name} ({suffix})";
            var stock = index % 17 == 5 ? 0 : index % 11 == 3 ? 2 : 15 + (index * 13 % 40);

            products.Add(new EcommerceProduct(
                id,
                name,
                original.CategoryId,
                price,
                compareAt is { } value && value > price ? value : null,
                Describe(name, original.CategoryId),
                original.Highlights,
                original.Colors,
                IsFeatured: false,
                stock,
                Today.AddDays(-(20 + (index * 37 % 340))),
                IsArchived: index % 29 == 13,
                Sku: SkuFor(original.CategoryId, id),
                ImageId: original.Id,
                ImageAlt: original.ImageAlt));
        }

        return products;
    }

    /// <summary>
    /// Two to five reviews per product, with ratings from a fixed pattern, posted after the product was added. A few are
    /// hidden by the store, and some carry the store's public reply.
    /// </summary>
    public static List<EcommerceReview> Reviews(IEnumerable<EcommerceProduct> products)
    {
        var reviews = new List<EcommerceReview>();

        foreach (var product in products)
        {
            var count = 2 + (product.Id % 4);

            for (var i = 0; i < count; i++)
            {
                var rating = ratingPattern[(product.Id + (i * 5)) % ratingPattern.Length];
                var (title, body) = reviewsByRating[rating - 1];
                var id = reviews.Count + 1;
                var posted = Today.AddDays(-(3 + (i * 17) + (product.Id % 11)));

                if (posted < product.AddedOn.AddDays(2 + i))
                    posted = product.AddedOn.AddDays(2 + i) > Today ? Today : product.AddedOn.AddDays(2 + i);

                var reply = id % 9 != 0 ? null
                    : rating >= 4 ? "Thank you for the kind words! We're glad it fits right in."
                    : rating == 3 ? "Thanks for the honest review. We've passed your notes on to our workshop."
                    : "We're sorry it wasn't right. Our team has emailed you about a free return.";

                reviews.Add(new EcommerceReview(
                    id,
                    product.Id,
                    authors[(product.Id * 3 + i) % authors.Length],
                    rating,
                    title,
                    body,
                    posted,
                    IsFromThisSession: false,
                    Status: id % 31 == 0 ? EcommerceReviewStatus.Hidden : EcommerceReviewStatus.Published,
                    Reply: reply));
            }
        }

        return reviews;
    }

    /// <summary>The demo customer's address book: home (the default) and a studio, both in the local delivery area.</summary>
    public static List<EcommerceSavedAddress> Addresses() => new()
    {
        new(1, DemoCustomerId, "Home", "Maya Lindqvist", "18 Alder Row", "Fairbank", "10482", "United States", "+1 555 0142", IsDefault: true),
        new(2, DemoCustomerId, "Studio", "Maya Lindqvist", "5 Kiln Street, Unit 3", "Brookhaven", "20936", "United States", null, IsDefault: false),
    };

    /// <summary>The demo customer's wishlist: one product in stock and one out of stock, with a back-in-stock notice requested.</summary>
    public static List<EcommerceWishItem> Wishlist() => new()
    {
        new(DemoCustomerId, 108, Today.AddDays(-12), NotifyWhenInStock: false),
        new(DemoCustomerId, 113, Today.AddDays(-5), NotifyWhenInStock: true),
    };

    /// <summary>Returns a map position for an address: the city's center (known cities) plus a small fixed offset from the street.</summary>
    public static (double Latitude, double Longitude) PositionFor(string city, string street)
    {
        var (latitude, longitude) = city.Trim().ToUpperInvariant() switch
        {
            "BROOKHAVEN" => (45.4870, -122.8040),
            _ => (Store.Latitude, Store.Longitude),
        };
        var hash = street.Aggregate(17, (current, ch) => unchecked((current * 31) + char.ToUpperInvariant(ch)));

        return (latitude + ((hash % 100) / 10000.0), longitude + (((hash / 100) % 100) / 10000.0));
    }

    /// <summary>
    /// The store's coupons: a year of seasonal offers (most expired, a few scheduled for the holidays), the welcome code, a
    /// spend-and-save code, codes without dates for loyal customers, and one paused. Uses come from the seeded orders.
    /// </summary>
    public static List<EcommerceCoupon> Coupons()
    {
        static DateTime Day(int year, int month, int day) => new(year, month, day);

        return new List<EcommerceCoupon>
        {
            new(PromoCode, "10% off a first order", EcommerceCouponKind.Percent, 10m, 0m, FirstOrderOnly: true, ExpiresOn: null, IsActive: true, UsageLimit: null, Uses: 0),
            new("TAKE20", "$20 off orders of $150 or more", EcommerceCouponKind.Amount, 20m, 150m, false, Today.AddDays(40), true, 50, 0, Today.AddDays(-20)),
            new("AUTUMN15", "15% off the autumn edit", EcommerceCouponKind.Percent, 15m, 60m, false, Today.AddDays(-3), true, null, 0, Today.AddDays(-60)),
            new("STAFF30", "30% off for the team (paused)", EcommerceCouponKind.Percent, 30m, 0m, false, null, false, 20, 4),
            new("BLACKFRI25", "25% off everything for Black Friday weekend", EcommerceCouponKind.Percent, 25m, 50m, false, Day(2026, 11, 30), true, 500, 0, Day(2026, 11, 27)),
            new("CYBER20", "20% off on Cyber Monday", EcommerceCouponKind.Percent, 20m, 0m, false, Day(2026, 12, 1), true, 300, 0, Day(2026, 11, 30)),
            new("HOLIDAY10", "$10 off holiday orders of $80 or more", EcommerceCouponKind.Amount, 10m, 80m, false, Day(2026, 12, 24), true, null, 0, Day(2026, 12, 1)),
            new("VIP20", "20% off for VIP customers", EcommerceCouponKind.Percent, 20m, 0m, false, null, true, 100, 23),
            new("LOYAL15", "15% thank-you code for repeat customers", EcommerceCouponKind.Percent, 15m, 40m, false, null, true, 200, 61),
            new("BLACKFRI24", "25% off for Black Friday weekend", EcommerceCouponKind.Percent, 25m, 50m, false, Day(2025, 12, 1), true, 500, 0, Day(2025, 11, 28)),
            new("CYBER24", "20% off on Cyber Monday", EcommerceCouponKind.Percent, 20m, 0m, false, Day(2025, 12, 2), true, 300, 0, Day(2025, 12, 1)),
            new("GIFT20", "$20 off gift orders of $120 or more", EcommerceCouponKind.Amount, 20m, 120m, false, Day(2025, 12, 20), true, null, 0, Day(2025, 12, 5)),
            new("WINTER20", "20% off the winter warmers", EcommerceCouponKind.Percent, 20m, 60m, false, Day(2026, 1, 31), true, null, 0, Day(2026, 1, 10)),
            new("NEWYEAR15", "15% off to start the year", EcommerceCouponKind.Percent, 15m, 0m, false, Day(2026, 1, 7), true, 400, 0, Day(2026, 1, 1)),
            new("VALENTINE10", "$10 off orders of $60 or more for Valentine's Day", EcommerceCouponKind.Amount, 10m, 60m, false, Day(2026, 2, 14), true, null, 0, Day(2026, 2, 7)),
            new("REFRESH10", "10% off the spring refresh", EcommerceCouponKind.Percent, 10m, 0m, false, Day(2026, 3, 10), true, null, 0, Day(2026, 2, 20)),
            new("SPRING15", "15% off for the first days of spring", EcommerceCouponKind.Percent, 15m, 50m, false, Day(2026, 4, 10), true, null, 0, Day(2026, 3, 20)),
            new("SPRINGHOME", "$15 off home orders of $90 or more", EcommerceCouponKind.Amount, 15m, 90m, false, Day(2026, 5, 15), true, null, 0, Day(2026, 4, 15)),
            new("MOTHERSDAY", "15% off gifts for Mother's Day", EcommerceCouponKind.Percent, 15m, 0m, false, Day(2026, 5, 10), true, 250, 0, Day(2026, 5, 1)),
            new("BATHWEEK", "15% off for bath week", EcommerceCouponKind.Percent, 15m, 30m, false, Day(2026, 5, 27), true, null, 0, Day(2026, 5, 20)),
            new("PICNIC10", "$10 off picnic season orders of $70 or more", EcommerceCouponKind.Amount, 10m, 70m, false, Day(2026, 6, 30), true, null, 0, Day(2026, 6, 1)),
            new("SUMMER20", "20% off summer orders of $100 or more", EcommerceCouponKind.Percent, 20m, 100m, false, Day(2026, 7, 15), true, null, 0, Day(2026, 6, 15)),
            new("BACK2DESK", "15% off the back-to-desk edit", EcommerceCouponKind.Percent, 15m, 0m, false, Day(2026, 9, 10), true, null, 0, Day(2026, 8, 15)),
            new("FALLSET25", "$25 off orders of $200 or more", EcommerceCouponKind.Amount, 25m, 200m, false, Day(2026, 10, 15), true, 100, 0, Day(2026, 9, 15)),
            new("FLASH30", "30% off for one day only", EcommerceCouponKind.Percent, 30m, 0m, false, Day(2026, 10, 11), true, 100, 0, Day(2026, 10, 10)),
            new("COZY15", "15% off the cozy season", EcommerceCouponKind.Percent, 15m, 40m, false, Day(2026, 10, 31), false, null, 0, Day(2026, 10, 18)),
        };
    }

    /// <summary>Computes the totals of an order the way checkout does: discount, then shipping, then tax on the discounted subtotal.</summary>
    public static EcommerceTotals Totals(int itemCount, decimal subtotal, decimal discount, EcommerceDelivery delivery)
    {
        var afterDiscount = subtotal - discount;
        var shipping = itemCount == 0 ? 0m
            : delivery == EcommerceDelivery.Local ? LocalDeliveryFee
            : delivery == EcommerceDelivery.Express ? ExpressShipping
            : afterDiscount >= FreeShippingThreshold ? 0m
            : StandardShipping;
        var tax = Math.Round(afterDiscount * TaxRate, 2, MidpointRounding.AwayFromZero);

        return new EcommerceTotals(itemCount, subtotal, discount, shipping, tax, afterDiscount + shipping + tax);
    }

    /// <summary>Returns the store's SKU for a product: KG, the category's code, and the product number, like KG-KIT-0101 (KG-NEW for no category yet).</summary>
    public static string SkuFor(string? categoryId, int productId) =>
        string.Create(CultureInfo.InvariantCulture, $"KG-{categoryCodes.FirstOrDefault(code => code.Code == categoryId).Name ?? "NEW"}-{productId:0000}");

    /// <summary>Returns the fictitious carrier tracking number of an order.</summary>
    public static string TrackingNumberFor(int orderId) => FormattableString.Invariant($"KG{orderId}{orderId * 37 % 10000:0000}US");

    #endregion

    #region Private Methods

    /// <summary>
    /// The seeded audit log, oldest first: the last two weeks of orders placed and moved on, cancellations and refunds, the
    /// last month of returns requested and decided, the coupons and products the retailer created in the last weeks, and the
    /// last month of purchase-order steps and the stock their receipts added.
    /// </summary>
    private static List<ShowcaseAuditEntry> Audit(IReadOnlyList<EcommerceProduct> products, IReadOnlyList<EcommerceCoupon> coupons, IReadOnlyList<EcommerceOrder> orders,
        IReadOnlyList<EcommerceReturn> returns, IReadOnlyList<EcommerceSupplier> suppliers, IReadOnlyList<EcommercePurchaseOrder> purchaseOrders)
    {
        var retailer = DemoAccounts().First(account => account.Id == DemoRetailerId).Name;
        var entries = new List<(DateTime At, string Actor, string Action, string Target, string? Url, string? Details)>();
        var ordersById = orders.ToDictionary(order => order.Id);

        foreach (var order in orders.Where(order => order.PlacedOn >= SeedTime.AddDays(-14)))
        {
            var target = $"Order #{order.Id} · {order.ShipTo.FullName}";
            var url = AdminOrderPath(order.Id);
            entries.Add((order.PlacedOn, order.ShipTo.FullName, "Order placed", target, url,
                $"{ItemsText(order.Totals.ItemCount)} · {Money(order.Totals.Total)}{(order.PromoCode is null ? null : $" with {order.PromoCode}")}"));

            foreach (var change in order.History.Skip(1))
            {
                switch (change.Status)
                {
                    case EcommerceOrderStatus.Packed:
                        entries.Add((change.At, retailer, "Order packed", target, url, null));
                        break;
                    case EcommerceOrderStatus.Shipped:
                        entries.Add((change.At, retailer, "Order shipped", target, url, $"Tracking number {order.TrackingNumber}."));
                        break;
                    case EcommerceOrderStatus.Delivered:
                        entries.Add((change.At, SystemActor, "Order delivered", target, url, "The carrier confirmed delivery."));
                        break;
                    case EcommerceOrderStatus.Cancelled:
                        entries.Add((change.At, order.ShipTo.FullName, "Order cancelled", target, url, $"{Money(order.Totals.Total)} refunded; items back in stock."));
                        break;
                }
            }
        }

        foreach (var order in orders.Where(order => order.Status == EcommerceOrderStatus.Refunded && order.History[^1].At >= SeedTime.AddDays(-30)))
            entries.Add((order.History[^1].At, retailer, "Order refunded", $"Order #{order.Id} · {order.ShipTo.FullName}", AdminOrderPath(order.Id),
                $"{Money(order.Totals.Total)} refunded. {order.History[^1].Note}"));

        foreach (var item in returns)
        {
            var order = ordersById[item.OrderId];
            var target = $"Return {ReturnNumber(item.Id)} · order #{item.OrderId}";

            if (item.RequestedOn >= SeedTime.AddDays(-30))
                entries.Add((item.RequestedOn, order.ShipTo.FullName, "Return requested", target, AdminReturnPath(item.Id), $"{ItemsText(item.Lines.Sum(line => line.Quantity))} · {item.Reason}"));

            if (item.DecidedOn is { } decided && decided >= SeedTime.AddDays(-30))
                entries.Add((decided, retailer, item.Status == EcommerceReturnStatus.Approved ? "Return approved" : "Return rejected", target, AdminReturnPath(item.Id),
                    item.Status == EcommerceReturnStatus.Approved ? $"{Money(item.Refunded)} refunded. {item.DecisionNote}" : item.DecisionNote));
        }

        foreach (var coupon in coupons.Where(coupon => coupon.StartsOn is { } starts && starts >= Today.AddDays(-30) && starts <= Today.AddDays(30)))
        {
            var created = coupon.StartsOn!.Value.AddDays(-7).AddHours(11);

            if (created <= SeedTime.AddHours(-1))
                entries.Add((created, retailer, "Coupon created", $"Coupon {coupon.Code}", $"{AdminBase}/offers", coupon.Description));
        }

        foreach (var product in products.Where(product => product.AddedOn >= Today.AddDays(-30)))
            entries.Add((product.AddedOn.AddHours(9), retailer, "Product created", $"Product {product.Name} ({product.Sku})", AdminProductPath(product.Id),
                $"{Money(product.Price)} · {product.Stock} in stock"));

        entries.AddRange(PurchasingAudit(suppliers, purchaseOrders));

        return entries
            .Where(entry => entry.At <= SeedTime)
            .OrderBy(entry => entry.At)
            .ThenBy(entry => entry.Target, StringComparer.Ordinal)
            .Select((entry, index) => new ShowcaseAuditEntry
            {
                Id = index + 1,
                At = entry.At,
                Actor = entry.Actor,
                Action = entry.Action,
                Target = entry.Target.Length <= 120 ? entry.Target : entry.Target[..120],
                TargetUrl = entry.Url,
                Details = entry.Details,
            })
            .ToList();
    }

    /// <summary>
    /// The demo customer (Maya Lindqvist) and 220 fictitious customers, their names from fixed lists in a fixed shuffled
    /// order. About half are registered (an account ID like <c>c-0012</c>, without a sign-in in the demo); the rest
    /// check out as guests.
    /// </summary>
    private static List<SeedCustomer> Customers()
    {
        var customers = new List<SeedCustomer>(SeededCustomerCount + 1)
        {
            new(DemoCustomerId, "Maya Lindqvist", "maya@kindredgoods.example", "18 Alder Row", "Fairbank", "10482", "United States"),
        };

        for (var index = 0; index < SeededCustomerCount; index++)
        {
            var pick = index * 37 % (firstNames.Length * lastNames.Length);
            var first = firstNames[pick % firstNames.Length];
            var last = lastNames[pick / firstNames.Length];
            var (city, country, postal) = cities[index * 7 % cities.Length];
            var registered = index % 5 != 0 && index % 3 != 1;

            customers.Add(new SeedCustomer(
                registered ? string.Create(CultureInfo.InvariantCulture, $"c-{index + 1:0000}") : null,
                $"{first} {last}",
                $"{first}.{last}@example.com".ToLowerInvariant(),
                string.Create(CultureInfo.InvariantCulture, $"{3 + (index * 17 % 180)} {streets[index % streets.Length]}"),
                city,
                string.Format(CultureInfo.InvariantCulture, postal, index % 90 + 10),
                country));
        }

        return customers;
    }

    private static string Describe(string name, string category) => category switch
    {
        "kitchen" => $"The {name} is made in small batches from food-safe materials and finished by hand. It's built for daily use and looks good left out on the counter.",
        "living" => $"The {name} brings texture and warmth to any room. Natural materials, a quiet palette, and proportions that work in small spaces.",
        "outdoor" => $"The {name} is weather-ready and packs down for trips. Sustainably sourced materials, tested through a full season outdoors.",
        "workspace" => $"The {name} helps keep your desk clear and your posture comfortable. Designed with remote teams, for long days at home.",
        _ => $"The {name} turns an ordinary bathroom into a calm corner. Soft, absorbent, and made from certified organic or responsibly sourced materials.",
    };

    private static IReadOnlyList<string> Highlights(string category) => category switch
    {
        "kitchen" => new[] { "Food-safe, plastic-free finish", "Hand wash recommended", "2-year warranty" },
        "living" => new[] { "Natural fibers and solid wood", "Spot clean or dry clean", "Ships fully assembled" },
        "outdoor" => new[] { "Weather-resistant", "Packs down for travel", "Repairable parts available" },
        "workspace" => new[] { "Ergonomically tested", "Recycled packaging", "3-year warranty" },
        _ => new[] { "OEKO-TEX certified fabrics", "Machine washable", "30-night trial" },
    };

    /// <summary>
    /// A year of orders (IDs 10001 to 10480, oldest first), a little more each month, from the seeded customers; the demo
    /// customer placed about a dozen, one packed and one on its way now. Status follows age: today's are placed, then
    /// packed, shipped, and delivered, with a few cancelled and refunded. Some first orders use the welcome code and some
    /// use the seasonal coupon running at the time; prices and totals use the same rules as checkout. Returns the orders
    /// and the coupons with their uses counted.
    /// </summary>
    private static (List<EcommerceOrder> Orders, List<EcommerceCoupon> Coupons) HistoryOrders(IReadOnlyList<EcommerceProduct> products, List<EcommerceCoupon> coupons,
        IReadOnlyList<SeedCustomer> customers)
    {
        var maya = customers[0];
        var shoppers = customers.Skip(1).ToList();
        var byAddedOn = products.OrderBy(product => product.AddedOn).ThenBy(product => product.Id).ToList();
        var uses = new Dictionary<string, int>(StringComparer.Ordinal);
        var ordered = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var welcome = coupons.First(coupon => coupon.Code == PromoCode);
        var seasonal = coupons.Where(coupon => coupon.StartsOn is not null && coupon.IsActive).ToList();

        // Each order's time and customer, by its index; the IDs then follow the time.
        var slots = Enumerable.Range(0, HistoryOrderCount).Select(index =>
        {
            var daysAgo = (int)Math.Round(364 * Math.Pow(1 - (index / (double)(HistoryOrderCount - 1)), 1.15));
            var placed = SeedTime.AddDays(-daysAgo).AddHours(-(index * 5 % 9)).AddMinutes(-(index * 13 % 60));
            var customer = index % 40 == 3 || index is 468 or 474 ? maya
                : index < shoppers.Count ? shoppers[index]
                : shoppers[(int)(shoppers.Count * Math.Pow((index * 61 % 997) / 997.0, 1.7))];

            return (Index: index, DaysAgo: daysAgo, Placed: placed, Customer: customer);
        }).OrderBy(slot => slot.Placed).ThenBy(slot => slot.Index).ToList();

        var orders = new List<EcommerceOrder>(HistoryOrderCount);

        foreach (var (index, daysAgo, placed, customer) in slots)
        {
            var id = FirstHistoryOrderId + orders.Count;
            var available = byAddedOn.Where(product => product.AddedOn <= placed.Date).ToList();

            if (available.Count < 3)
                available = byAddedOn.Take(5).ToList();

            var lines = new List<EcommerceOrderLine>();

            for (var line = 0; line < 1 + (index % 3); line++)
            {
                var product = available[(index * 11 + line * 5) % available.Count];

                if (lines.Any(existing => existing.ProductId == product.Id))
                    continue;

                lines.Add(new EcommerceOrderLine(product.Id, product.Name, product.Colors[(index + line) % product.Colors.Count], 1 + ((index + line) % 2), product.Price));
            }

            var isLocal = LocalDeliveryCities.Contains(customer.City) && customer.Country == LocalDeliveryCountry;
            var delivery = isLocal && index % 4 == 1 ? EcommerceDelivery.Local : index % 5 == 0 ? EcommerceDelivery.Express : EcommerceDelivery.Standard;
            var subtotal = lines.Sum(line => line.UnitPrice * line.Quantity);
            var isFirst = ordered.Add(customer.Email);
            var coupon = isFirst && index % 4 == 0 ? welcome
                : index % 6 == 0 ? seasonal.FirstOrDefault(candidate => candidate.StartsOn!.Value.Date <= placed.Date && (candidate.ExpiresOn is null || placed.Date <= candidate.ExpiresOn.Value.Date)
                    && subtotal >= candidate.MinSubtotal && (candidate.UsageLimit is null || uses.GetValueOrDefault(candidate.Code) < candidate.UsageLimit))
                : null;
            var discount = coupon is null ? 0m : CouponDiscount(coupon, subtotal);
            var totals = Totals(lines.Sum(line => line.Quantity), subtotal, discount, delivery);

            var status = index % 41 == 7 ? EcommerceOrderStatus.Cancelled
                : index % 53 == 9 && daysAgo > 6 ? EcommerceOrderStatus.Refunded
                : daysAgo == 0 ? EcommerceOrderStatus.Placed
                : daysAgo == 1 ? (index % 2 == 0 ? EcommerceOrderStatus.Placed : EcommerceOrderStatus.Packed)
                : daysAgo <= 2 ? EcommerceOrderStatus.Packed
                : daysAgo <= 5 ? EcommerceOrderStatus.Shipped
                : EcommerceOrderStatus.Delivered;

            if (coupon is not null && status != EcommerceOrderStatus.Cancelled)
                uses[coupon.Code] = uses.GetValueOrDefault(coupon.Code) + 1;

            var history = new List<EcommerceStatusChange> { new(EcommerceOrderStatus.Placed, placed, null) };

            if (status == EcommerceOrderStatus.Cancelled)
            {
                history.Add(new(EcommerceOrderStatus.Cancelled, placed.AddHours(3), "Customer asked to cancel before packing."));
            }
            else
            {
                if (status >= EcommerceOrderStatus.Packed)
                    history.Add(new(EcommerceOrderStatus.Packed, placed.AddHours(20), null));

                if (status >= EcommerceOrderStatus.Shipped)
                    history.Add(new(EcommerceOrderStatus.Shipped, placed.AddDays(1).AddHours(6), null));

                if (status >= EcommerceOrderStatus.Delivered)
                    history.Add(new(EcommerceOrderStatus.Delivered, placed.AddDays(delivery == EcommerceDelivery.Express ? 2 : 4), null));

                if (status == EcommerceOrderStatus.Refunded)
                    history.Add(new(EcommerceOrderStatus.Refunded, placed.AddDays(6), "Arrived damaged; refunded in full."));
            }

            var card = cards[index % cards.Length];
            var tracking = status is EcommerceOrderStatus.Shipped or EcommerceOrderStatus.Delivered or EcommerceOrderStatus.Refunded ? TrackingNumberFor(id) : null;

            orders.Add(new EcommerceOrder(
                id,
                placed,
                lines,
                new EcommerceAddress(customer.Name, customer.Email, customer.Street, customer.City, customer.PostalCode, customer.Country),
                delivery,
                coupon?.Code,
                totals,
                card.Brand,
                card.Last4,
                status,
                history,
                tracking,
                customer.AccountId,
                status is EcommerceOrderStatus.Refunded or EcommerceOrderStatus.Cancelled ? totals.Total : 0m));
        }

        var counted = coupons.Select(coupon => coupon.StartsOn is null && coupon.Code != PromoCode ? coupon : coupon with { Uses = uses.GetValueOrDefault(coupon.Code) }).ToList();

        return (orders, counted);
    }

    private static string ItemsText(int count) => count == 1 ? "1 item" : string.Create(CultureInfo.InvariantCulture, $"{count} items");

    /// <summary>
    /// The demo accounts' notifications, oldest first, each account's newest two unread. The retailer's: today's orders to
    /// fulfil, returns waiting for a decision, the newest reviews to moderate, and the purchasing ones (products below their
    /// reorder point, an approved purchase order). The purchasing manager's: products below their reorder point, a purchase
    /// order waiting for approval, and an overdue one. The demo customer's: the last weeks' steps of her orders and her
    /// approved return.
    /// </summary>
    private static List<EcommerceNotice> Notices(IReadOnlyList<EcommerceProduct> products, IReadOnlyList<EcommerceReview> reviews, IReadOnlyList<EcommerceOrder> orders,
        IReadOnlyList<EcommerceReturn> returns, IReadOnlyList<EcommerceReplenishment> replenishment, IReadOnlyList<EcommerceSupplier> suppliers,
        IReadOnlyList<EcommercePurchaseOrder> purchaseOrders)
    {
        var ordersById = orders.ToDictionary(order => order.Id);
        var productsById = products.ToDictionary(product => product.Id);
        var items = new List<(string AccountId, DateTime At, string Title, string Text, string Link)>();

        foreach (var order in orders.Where(order => order.Status == EcommerceOrderStatus.Placed))
            items.Add((DemoRetailerId, order.PlacedOn, $"New order #{order.Id} to fulfil", $"{order.ShipTo.FullName} · {ItemsText(order.Totals.ItemCount)} · {Money(order.Totals.Total)}",
                AdminOrderPath(order.Id)));

        foreach (var item in returns.Where(item => item.Status == EcommerceReturnStatus.Requested))
            items.Add((DemoRetailerId, item.RequestedOn, $"Return {ReturnNumber(item.Id)} requested", $"{ordersById[item.OrderId].ShipTo.FullName} · order #{item.OrderId} · {item.Reason}",
                AdminReturnPath(item.Id)));

        items.AddRange(PurchasingNotices(products, replenishment, suppliers, purchaseOrders));

        // The newest unanswered review of each of the three products reviewed most recently.
        foreach (var review in reviews.Where(review => review.Status == EcommerceReviewStatus.Published && review.Reply is null)
                     .GroupBy(review => review.ProductId)
                     .Select(group => group.OrderByDescending(review => review.PostedOn).ThenByDescending(review => review.Id).First())
                     .OrderByDescending(review => review.PostedOn).ThenByDescending(review => review.Id).Take(3))
            items.Add((DemoRetailerId, review.PostedOn.AddHours(9), "New review to moderate", $"{review.Rating} of 5, “{review.Title}” on {productsById[review.ProductId].Name}", $"{AdminBase}/reviews"));

        foreach (var order in orders.Where(order => order.CustomerId == DemoCustomerId))
        {
            foreach (var change in order.History.Skip(1).Where(change => change.At >= SeedTime.AddDays(-45)))
            {
                var at = order with { Status = change.Status };
                items.Add((DemoCustomerId, change.At, $"Order #{order.Id} {StatusVerb(change.Status)}", OrderNoticeText(at), OrderPath(order.Id)));
            }
        }

        foreach (var item in returns.Where(item => item.Status == EcommerceReturnStatus.Approved && ordersById[item.OrderId].CustomerId == DemoCustomerId))
            items.Add((DemoCustomerId, item.DecidedOn!.Value, $"Return {ReturnNumber(item.Id)} approved", $"{Money(item.Refunded)} is refunded to your card.", OrderPath(item.OrderId)));

        var sorted = items.Where(item => item.At <= SeedTime).OrderBy(item => item.At).ThenBy(item => item.Title, StringComparer.Ordinal).ToList();
        var unread = sorted.GroupBy(item => item.AccountId).SelectMany(group => group.OrderByDescending(item => item.At).Take(2)).ToHashSet();

        return sorted.Select((item, index) => new EcommerceNotice(index + 1, item.AccountId, item.Title, item.Text, item.Link, item.At, IsRead: !unread.Contains(item))).ToList();
    }

    /// <summary>
    /// About fifty return requests on delivered orders, oldest first: the last twelve days' wait for a decision; older ones were
    /// approved (some refunded less a restocking fee, with a note) or rejected with a reason. The demo customer has one
    /// approved return.
    /// </summary>
    private static List<EcommerceReturn> Returns(IReadOnlyList<EcommerceOrder> orders)
    {
        var delivered = orders.Where(order => order.Status == EcommerceOrderStatus.Delivered).ToList();
        var mayaDelivered = delivered.Where(order => order.CustomerId == DemoCustomerId).ToList();
        // About one delivered order in ten over the year, and more of the last three weeks' (those still wait for a decision).
        var picks = delivered
            .Where((order, index) => order.CustomerId != DemoCustomerId && (index % 10 == 3 || (order.PlacedOn >= SeedTime.AddDays(-21) && index % 4 == 1)))
            .Select(order => (Order: order, Approve: false))
            .ToList();

        if (mayaDelivered.Count >= 2)
            picks.Add((mayaDelivered[^2], true));

        var drafts = new List<(EcommerceOrder Order, bool Approve, DateTime Requested)>();

        foreach (var (order, approve) in picks)
        {
            var requested = order.History.Last(change => change.Status == EcommerceOrderStatus.Delivered).At.AddDays(2 + (order.Id % 4)).AddHours(order.Id % 7);

            if (requested <= SeedTime.AddHours(-2))
                drafts.Add((order, approve, requested));
        }

        var returns = new List<EcommerceReturn>();

        foreach (var (order, approve, requested) in drafts.OrderBy(draft => draft.Requested).ThenBy(draft => draft.Order.Id))
        {
            var id = returns.Count + 1;
            var line = order.Lines[order.Id % order.Lines.Count];
            var quantity = line.Quantity > 1 && order.Id % 3 == 0 ? line.Quantity : 1;
            var (reason, comment) = returnReasons[order.Id % returnReasons.Length];
            var decided = requested.AddDays(1).AddHours(order.Id % 5);
            var status = !approve && (requested >= SeedTime.AddDays(-12) || decided > SeedTime) ? EcommerceReturnStatus.Requested
                : approve || order.Id % 10 < 7 ? EcommerceReturnStatus.Approved
                : EcommerceReturnStatus.Rejected;
            var lines = new[] { new EcommerceReturnLine(line.ProductId, line.Name, line.Color, quantity, line.UnitPrice) };
            var amount = line.UnitPrice * quantity;
            var partial = status == EcommerceReturnStatus.Approved && id % 4 == 0 && !approve;

            returns.Add(new EcommerceReturn(
                id,
                order.Id,
                lines,
                reason,
                comment,
                status,
                requested,
                status == EcommerceReturnStatus.Requested ? null : decided,
                status switch
                {
                    EcommerceReturnStatus.Approved when partial => "Refunded less a 15% restocking fee: the item was opened and used.",
                    EcommerceReturnStatus.Approved => "Refunded to the original card; item restocked.",
                    EcommerceReturnStatus.Rejected => rejectNotes[id % rejectNotes.Length],
                    _ => null,
                },
                status == EcommerceReturnStatus.Approved ? partial ? Math.Round(amount * .85m, 2, MidpointRounding.AwayFromZero) : amount : null));
        }

        return returns;
    }

    #endregion

    #endregion

    #region Nested Types

    /// <summary>A seeded customer: their account ID when registered (<see langword="null" /> for a guest), name, email, and address.</summary>
    private sealed record SeedCustomer(string? AccountId, string Name, string Email, string Street, string City, string PostalCode, string Country);

    #endregion
}

/// <summary>
/// The store's seeded lists, built once by <see cref="EcommerceSeedData.Build" />. Records are immutable and shared by every
/// tab; each tab keeps its own lists of them.
/// </summary>
public sealed record EcommerceSeedSet(
    IReadOnlyList<EcommerceProduct> Products,
    IReadOnlyList<EcommerceReview> Reviews,
    IReadOnlyList<EcommerceCoupon> Coupons,
    IReadOnlyList<EcommerceOrder> Orders,
    IReadOnlyList<EcommerceReturn> Returns,
    IReadOnlyList<EcommerceNotice> Notices,
    IReadOnlyList<ShowcaseAuditEntry> Audit,
    IReadOnlyList<EcommerceSupplier> Suppliers,
    IReadOnlyList<EcommerceReplenishment> Replenishment,
    IReadOnlyList<EcommercePurchaseOrder> PurchaseOrders);
