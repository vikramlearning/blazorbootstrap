using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

/// <summary>
/// Fixed, fictitious content for the "Kindred Goods" storefront. Every value is a constant or derived from the
/// product's index, so each load shows the same catalog, ratings, and reviews.
/// </summary>
public static class EcommerceSeedData
{
    #region Fields and Constants

    /// <summary>Express shipping price.</summary>
    public const decimal ExpressShipping = 14.95m;

    /// <summary>Order subtotal (after discount) from which standard shipping is free.</summary>
    public const decimal FreeShippingThreshold = 75m;

    /// <summary>Highest quantity of one line in the cart.</summary>
    public const int MaxQuantity = 10;

    /// <summary>The demo promo code.</summary>
    public const string PromoCode = "WELCOME10";

    /// <summary>The promo code's discount on the subtotal.</summary>
    public const decimal PromoRate = .10m;

    /// <summary>Standard shipping price below the free-shipping threshold.</summary>
    public const decimal StandardShipping = 6.95m;

    /// <summary>The fictitious store name.</summary>
    public const string StoreName = "Kindred Goods";

    /// <summary>Sales tax rate applied after discounts.</summary>
    public const decimal TaxRate = .08m;

    /// <summary>The role ID of shoppers.</summary>
    public const string CustomerRole = "customer";

    /// <summary>The role ID of the store team.</summary>
    public const string RetailerRole = "retailer";

    /// <summary>The fixed "today" all dates on the site are relative to.</summary>
    public static readonly DateTime Today = new(2026, 11, 9);

    /// <summary>The site clock's start: 10:00 on <see cref="Today" />; the clock then moves with real time.</summary>
    public static readonly DateTime SeedTime = Today.AddHours(10);

    private static readonly string[] authors =
    {
        "Maya R.", "Theo L.", "Priya S.", "Jonah W.", "Elena V.", "Sam K.", "Ines M.", "Callum B.", "Noor A.", "Hana T.", "Felix D.", "Rosa G.",
    };

    private static readonly int[] ratingPattern = { 5, 4, 5, 3, 5, 4, 2, 5, 4, 5, 4, 3 };

    private static readonly (string Title, string Body)[] reviewsByRating =
    {
        ("Not for me", "It looked different from the photos and felt lighter than I expected. Returning it was easy, at least."),
        ("Okay, with caveats", "Nice design, but the finish marked quicker than I hoped. Fine for the price."),
        ("Does the job", "Solid and simple. Delivery took a little longer than estimated, but it arrived well packed."),
        ("Really happy with it", "Great quality for the price and it matches the rest of the room. Would buy again."),
        ("Love it", "Beautifully made and it feels like it will last for years. Already ordered a second one as a gift."),
    };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>The demo accounts listed on the sign-in page, one per role. Fictitious people with <c>.example</c> addresses.</summary>
    public static IReadOnlyList<ShowcaseDemoAccount> DemoAccounts() => new List<ShowcaseDemoAccount>
    {
        new("maya", CustomerRole, "Maya Lindqvist", "maya@kindredgoods.example", "shop-demo-24", "ML"),
        new("owen", RetailerRole, "Owen Hart", "owen@kindredgoods.example", "store-demo-24", "OH"),
    };

    /// <summary>The site's roles.</summary>
    public static IReadOnlyList<ShowcaseRole> Roles() => new List<ShowcaseRole>
    {
        new(CustomerRole, "Customer", $"{DemoRouteConstants.Showcase_URL_Ecommerce}/account", $"{DemoRouteConstants.Showcase_URL_Ecommerce}/account", "Shops, checks out, and tracks orders."),
        new(RetailerRole, "Retailer", $"{DemoRouteConstants.Showcase_URL_Ecommerce}/admin", $"{DemoRouteConstants.Showcase_URL_Ecommerce}/admin/account", "Runs the store from its admin: orders, inventory, pricing, and sales."),
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

    /// <summary>The catalog.</summary>
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

        var products = new List<EcommerceProduct>(rows.Length);

        for (var index = 0; index < rows.Length; index++)
        {
            var (name, category, price, compareAt) = rows[index];
            var id = 101 + index;
            var colors = index % 3 == 2 ? colorsByCategory[category].Take(1).ToArray() : colorsByCategory[category];
            var stock = index % 13 == 12 ? 0 : index % 9 == 4 ? 3 : 20 + (index * 7 % 30);

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
                Today.AddDays(-(index * 11 % 90))));
        }

        return products;
    }

    /// <summary>Two to five reviews per product, with ratings from a fixed pattern.</summary>
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

                reviews.Add(new EcommerceReview(
                    reviews.Count + 1,
                    product.Id,
                    authors[(product.Id * 3 + i) % authors.Length],
                    rating,
                    title,
                    body,
                    Today.AddDays(-(3 + (i * 17) + (product.Id % 11))),
                    IsFromThisSession: false));
            }
        }

        return reviews;
    }

    /// <summary>The store's coupons: the welcome code, a spend-and-save code, an expired seasonal code, and one switched off.</summary>
    public static List<EcommerceCoupon> Coupons() => new()
    {
        new(PromoCode, "10% off a first order", EcommerceCouponKind.Percent, 10m, 0m, FirstOrderOnly: true, ExpiresOn: null, IsActive: true, UsageLimit: null, Uses: 9),
        new("TAKE20", "$20 off orders of $150 or more", EcommerceCouponKind.Amount, 20m, 150m, FirstOrderOnly: false, ExpiresOn: Today.AddDays(40), IsActive: true, UsageLimit: 50, Uses: 12),
        new("AUTUMN15", "15% off the autumn edit", EcommerceCouponKind.Percent, 15m, 60m, FirstOrderOnly: false, ExpiresOn: Today.AddDays(-3), IsActive: true, UsageLimit: null, Uses: 31),
        new("STAFF30", "30% off for the team (paused)", EcommerceCouponKind.Percent, 30m, 0m, FirstOrderOnly: false, ExpiresOn: null, IsActive: false, UsageLimit: 20, Uses: 4),
    };

    /// <summary>
    /// The last four weeks of orders (IDs 10445 to 10480) from fictitious customers, so the dashboard, order list, and
    /// analytics start with history. Status follows age: today's are placed, then packed, shipped, and delivered, with a
    /// few cancelled and refunded. Prices and totals use the same rules as checkout.
    /// </summary>
    public static List<EcommerceOrder> HistoryOrders(IReadOnlyList<EcommerceProduct> products)
    {
        var customers = new (string Name, string Email, string Street, string City, string PostalCode, string Country, string? AccountId)[]
        {
            ("Maya Lindqvist", "maya@kindredgoods.example", "18 Alder Row", "Fairbank", "10482", "United States", "maya"),
            ("Theo Ramsay", "theo.ramsay@example.com", "7 Willow Court", "Brookhaven", "20931", "United States", null),
            ("Priya Shah", "priya.shah@example.com", "52 Orchard Lane", "Maple Falls", "30417", "Canada", null),
            ("Jonah Wells", "jonah.wells@example.com", "3 Harbor Street", "Saltmere", "41820", "United Kingdom", null),
            ("Elena Vargas", "elena.vargas@example.com", "90 Cedar Avenue", "Fairbank", "10488", "United States", null),
            ("Sam Kowalski", "sam.k@example.com", "14 Birch Close", "Northwick", "55102", "Germany", null),
            ("Ines Moreau", "ines.moreau@example.com", "21 Linden Road", "Clearwater", "60231", "Australia", null),
            ("Callum Byrne", "callum.byrne@example.com", "6 Heather Way", "Brookhaven", "20944", "United States", null),
            ("Noor Aziz", "noor.aziz@example.com", "33 Juniper Street", "Maple Falls", "30422", "Canada", null),
            ("Hana Tanaka", "hana.tanaka@example.com", "11 Cypress Lane", "Saltmere", "41833", "India", null),
        };
        var cards = new (string Brand, string Last4)[] { ("Visa", "4242"), ("Mastercard", "4444"), ("American Express", "0005") };
        var orders = new List<EcommerceOrder>();

        for (var index = 0; index < 36; index++)
        {
            var id = 10445 + index;
            var daysAgo = (35 - index) * 4 / 5;
            var placed = SeedTime.AddDays(-daysAgo).AddHours(-(index * 5 % 9)).AddMinutes(index * 13 % 60);
            var customer = customers[index * 7 % customers.Length];
            var delivery = index % 5 == 0 ? EcommerceDelivery.Express : EcommerceDelivery.Standard;
            var promo = index % 9 == 0 ? PromoCode : null;
            var lines = new List<EcommerceOrderLine>();

            for (var line = 0; line < 1 + (index % 3); line++)
            {
                var product = products[(index * 11 + line * 5) % products.Count];

                if (lines.Any(existing => existing.ProductId == product.Id))
                    continue;

                lines.Add(new EcommerceOrderLine(product.Id, product.Name, product.Colors[(index + line) % product.Colors.Count], 1 + ((index + line) % 2), product.Price));
            }

            var subtotal = lines.Sum(line => line.UnitPrice * line.Quantity);
            var discount = promo is null ? 0m : Math.Round(subtotal * PromoRate, 2, MidpointRounding.AwayFromZero);
            var totals = Totals(lines.Sum(line => line.Quantity), subtotal, discount, delivery);

            var status = index % 13 == 7 ? EcommerceOrderStatus.Cancelled
                : index % 17 == 9 ? EcommerceOrderStatus.Refunded
                : daysAgo == 0 ? EcommerceOrderStatus.Placed
                : daysAgo <= 2 ? EcommerceOrderStatus.Packed
                : daysAgo <= 5 ? EcommerceOrderStatus.Shipped
                : EcommerceOrderStatus.Delivered;

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
            var tracking = status is EcommerceOrderStatus.Shipped or EcommerceOrderStatus.Delivered or EcommerceOrderStatus.Refunded
                ? TrackingNumberFor(id)
                : null;

            orders.Add(new EcommerceOrder(
                id,
                placed,
                lines,
                new EcommerceAddress(customer.Name, customer.Email, customer.Street, customer.City, customer.PostalCode, customer.Country),
                delivery,
                promo,
                totals,
                card.Brand,
                card.Last4,
                status,
                history,
                tracking,
                customer.AccountId,
                status is EcommerceOrderStatus.Refunded or EcommerceOrderStatus.Cancelled ? totals.Total : 0m));
        }

        return orders;
    }

    /// <summary>Three return requests on delivered history orders: one waiting, one approved, one declined.</summary>
    public static List<EcommerceReturn> Returns(IReadOnlyList<EcommerceOrder> orders)
    {
        var delivered = orders.Where(order => order.Status == EcommerceOrderStatus.Delivered).OrderBy(order => order.PlacedOn).ToList();
        var returns = new List<EcommerceReturn>();
        var reasons = new[] { ("Doesn't match the photos", "The color is warmer than on screen."), ("Arrived damaged", "One corner was chipped in transit."), ("Changed my mind", null as string) };
        var statuses = new[] { EcommerceReturnStatus.Approved, EcommerceReturnStatus.Rejected, EcommerceReturnStatus.Requested };
        var picks = new[] { delivered[0], delivered[delivered.Count / 2], delivered[^1] };

        for (var index = 0; index < picks.Length; index++)
        {
            var order = picks[index];
            var line = order.Lines[0];
            var status = statuses[index];
            var requested = order.History.Last(change => change.Status == EcommerceOrderStatus.Delivered).At.AddDays(2);

            returns.Add(new EcommerceReturn(
                index + 1,
                order.Id,
                new[] { new EcommerceReturnLine(line.ProductId, line.Name, line.Color, 1, line.UnitPrice) },
                reasons[index].Item1,
                reasons[index].Item2,
                status,
                requested,
                status == EcommerceReturnStatus.Requested ? null : requested.AddDays(1),
                status switch
                {
                    EcommerceReturnStatus.Approved => "Refunded to the original card; item restocked.",
                    EcommerceReturnStatus.Rejected => "Outside our 30-day return window.",
                    _ => null,
                }));
        }

        return returns;
    }

    /// <summary>Computes the totals of an order the way checkout does: discount, then shipping, then tax on the discounted subtotal.</summary>
    public static EcommerceTotals Totals(int itemCount, decimal subtotal, decimal discount, EcommerceDelivery delivery)
    {
        var afterDiscount = subtotal - discount;
        var shipping = itemCount == 0 ? 0m
            : delivery == EcommerceDelivery.Express ? ExpressShipping
            : afterDiscount >= FreeShippingThreshold ? 0m
            : StandardShipping;
        var tax = Math.Round(afterDiscount * TaxRate, 2, MidpointRounding.AwayFromZero);

        return new EcommerceTotals(itemCount, subtotal, discount, shipping, tax, afterDiscount + shipping + tax);
    }

    /// <summary>Returns the fictitious carrier tracking number of an order.</summary>
    public static string TrackingNumberFor(int orderId) => FormattableString.Invariant($"KG{orderId}{orderId * 37 % 10000:0000}US");

    #endregion

    #region Private Methods

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

    #endregion

    #endregion
}
