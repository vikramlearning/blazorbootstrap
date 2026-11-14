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
