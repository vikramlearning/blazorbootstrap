using BlazorBootstrap.Demo.RCL.Models.Showcase.Restaurant;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Restaurant;

/// <summary>
/// Fixed, fictitious content for the "Saffron Table" restaurant site: the menu, delivery addresses, and one past order.
/// Every time is relative to a fixed "now", so each load starts the same way.
/// </summary>
public static class RestaurantSeedData
{
    #region Fields and Constants

    /// <summary>Orders below this subtotal pay the delivery fee.</summary>
    public const decimal FreeDeliveryFrom = 40m;

    /// <summary>The delivery fee for smaller orders.</summary>
    public const decimal DeliveryFee = 3.99m;

    /// <summary>The most of one item a cart can hold.</summary>
    public const int MaxQuantity = 10;

    /// <summary>The restaurant's latitude, for the map.</summary>
    public const double RestaurantLatitude = 51.5205;

    /// <summary>The restaurant's longitude, for the map.</summary>
    public const double RestaurantLongitude = -0.0790;

    /// <summary>The fictitious restaurant name.</summary>
    public const string RestaurantName = "Saffron Table";

    /// <summary>Sales tax on food and delivery.</summary>
    public const decimal TaxRate = 0.085m;

    /// <summary>The fixed "now" all times on the site are relative to.</summary>
    public static readonly DateTime Now = new(2026, 11, 24, 18, 30, 0);

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Fictitious delivery addresses around the restaurant, with travel times.</summary>
    public static IReadOnlyList<RestaurantAddress> Addresses() => new List<RestaurantAddress>
    {
        new("home", "Home", "14 Wren Street", "Shoreditch", 51.5262, -0.0781, 18),
        new("work", "Work", "2 Finsbury Court, floor 5", "Moorgate", 51.5185, -0.0884, 14),
        new("studio", "Studio", "88 Canal Walk", "Hoxton", 51.5341, -0.0815, 24),
        new("parents", "Parents", "31 Fournier Gardens", "Spitalfields", 51.5196, -0.0736, 12),
    };

    /// <summary>The menu categories.</summary>
    public static IReadOnlyList<RestaurantCategory> Categories() => new List<RestaurantCategory>
    {
        new("starters", "Starters", IconName.Egg, "#b45309"),
        new("mains", "Mains", IconName.Fire, "#b91c1c"),
        new("pizza", "Pizza", IconName.CircleHalf, "#c2410c"),
        new("salads", "Salads & bowls", IconName.Flower2, "#15803d"),
        new("desserts", "Desserts", IconName.Cake2, "#be185d"),
        new("drinks", "Drinks", IconName.CupStraw, "#1d4ed8"),
    };

    /// <summary>The menu.</summary>
    public static IReadOnlyList<RestaurantMenuItem> Menu() => new List<RestaurantMenuItem>
    {
        new(401, "Charred corn ribs", "starters", "Smoked paprika butter, lime, and crumbled feta.", 7.50m, RestaurantDiet.Vegetarian | RestaurantDiet.GlutenFree, 320, 10, true),
        new(402, "Crispy calamari", "starters", "Lightly fried squid with saffron aioli and lemon.", 9.00m, RestaurantDiet.None, 410, 12, false),
        new(403, "Spiced lentil soup", "starters", "Red lentils, cumin, and a swirl of chili oil.", 6.50m, RestaurantDiet.Vegan | RestaurantDiet.GlutenFree | RestaurantDiet.Spicy, 260, 8, false),
        new(404, "Whipped feta & flatbread", "starters", "Honey, thyme, and warm stone-baked flatbread.", 8.00m, RestaurantDiet.Vegetarian, 450, 8, false),
        new(405, "Saffron chicken", "mains", "Roast half chicken, saffron yogurt, charred lemon, and herbed rice.", 18.50m, RestaurantDiet.GlutenFree, 780, 25, true),
        new(406, "Slow-cooked lamb shoulder", "mains", "Seven-hour lamb with pomegranate and freekeh.", 22.00m, RestaurantDiet.None, 920, 20, false),
        new(407, "Miso-glazed salmon", "mains", "Sesame greens, pickled ginger, and jasmine rice.", 19.00m, RestaurantDiet.GlutenFree, 640, 18, false),
        new(408, "Mushroom & chestnut ragù", "mains", "Pappardelle with a rich wild mushroom sauce.", 15.50m, RestaurantDiet.Vegan, 690, 16, false),
        new(409, "Margherita", "pizza", "San Marzano tomato, fior di latte, and basil.", 12.00m, RestaurantDiet.Vegetarian, 820, 12, true),
        new(410, "Nduja & honey", "pizza", "Spicy nduja, mozzarella, hot honey, and oregano.", 14.50m, RestaurantDiet.Spicy, 940, 12, false),
        new(411, "Wild mushroom & truffle", "pizza", "Roasted mushrooms, fontina, and truffle oil.", 15.00m, RestaurantDiet.Vegetarian, 880, 13, false),
        new(412, "Roast squash bowl", "salads", "Squash, quinoa, kale, pumpkin seeds, and tahini.", 11.50m, RestaurantDiet.Vegan | RestaurantDiet.GlutenFree, 520, 10, false),
        new(413, "Chicken Caesar", "salads", "Grilled chicken, gem lettuce, parmesan, and sourdough croutons.", 12.50m, RestaurantDiet.None, 610, 10, false),
        new(414, "Beet & burrata", "salads", "Roast beets, burrata, walnuts, and aged balsamic.", 11.00m, RestaurantDiet.Vegetarian | RestaurantDiet.GlutenFree, 430, 8, false),
        new(415, "Pistachio cheesecake", "desserts", "Baked cheesecake with a pistachio crumb.", 7.00m, RestaurantDiet.Vegetarian, 510, 5, true),
        new(416, "Dark chocolate mousse", "desserts", "Sea salt, olive oil, and cocoa nibs.", 6.50m, RestaurantDiet.Vegan | RestaurantDiet.GlutenFree, 380, 5, false),
        new(417, "Saffron rice pudding", "desserts", "Cardamom, rose, and toasted almonds.", 6.00m, RestaurantDiet.Vegetarian | RestaurantDiet.GlutenFree, 340, 5, false),
        new(418, "Fresh mint lemonade", "drinks", "Pressed lemons, mint, and sparkling water.", 4.00m, RestaurantDiet.Vegan | RestaurantDiet.GlutenFree, 120, 3, false),
        new(419, "Iced hibiscus tea", "drinks", "Lightly sweetened, with orange peel.", 3.50m, RestaurantDiet.Vegan | RestaurantDiet.GlutenFree, 90, 3, false),
        new(420, "Sparkling water", "drinks", "750 ml bottle.", 2.50m, RestaurantDiet.Vegan | RestaurantDiet.GlutenFree, 0, 1, false),
    };

    #endregion

    #endregion
}
