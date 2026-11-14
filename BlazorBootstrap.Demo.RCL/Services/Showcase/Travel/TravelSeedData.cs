using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

/// <summary>
/// Fixed, fictitious content for the "Driftwood Stays" travel site. The cities are real, but every stay, host, review,
/// and booking is made up, so each load shows the same data.
/// </summary>
public static class TravelSeedData
{
    #region Fields and Constants

    /// <summary>The most guests a search can ask for.</summary>
    public const int MaxGuests = 8;

    /// <summary>The longest stay that can be booked, in nights.</summary>
    public const int MaxNights = 14;

    /// <summary>The fictitious brand name.</summary>
    public const string SiteName = "Driftwood Stays";

    /// <summary>Taxes and service fees, as a share of the nightly subtotal.</summary>
    public const decimal TaxRate = 0.12m;

    /// <summary>The fictitious traveler shown as signed in.</summary>
    public const string TravelerName = "Jordan Ashdown";

    /// <summary>The fixed "today" all dates on the site are relative to.</summary>
    public static readonly DateOnly Today = new(2026, 11, 24);

    private static readonly string[] reviewers = { "Amara O.", "Lucas P.", "Hana T.", "Mateo R.", "Freya N.", "Omar K.", "Zoe L.", "Ravi M." };

    private static readonly int[] ratingPattern = { 5, 4, 5, 3, 5, 4, 4, 5, 2, 5 };

    /// <summary>Two review texts per star rating, from 1 to 5 stars.</summary>
    private static readonly string[][] reviewBodies =
    {
        new[] { "Not as described, and the street noise kept us up every night.", "Check-in took over an hour. We would not stay again." },
        new[] { "Great location, but the place needed a proper clean.", "Smaller than the photos suggest, and the wifi kept dropping." },
        new[] { "Fine for a short stay. The bed was comfortable; the kitchen was basic.", "Good value, though it was hard to find late at night." },
        new[] { "Lovely stay. Quiet at night and close to everything we wanted.", "Spotless and well equipped. The host's tips were spot on." },
        new[] { "Perfect from start to finish. We're already planning to come back.", "Beautiful place, easy check-in, and the best view of the trip." },
    };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Bookings in a fresh session: one upcoming trip and two past ones.</summary>
    public static List<TravelBooking> Bookings() => new()
    {
        new TravelBooking { Reference = "DS-48213", StayId = 306, CheckIn = Today.AddDays(18), CheckOut = Today.AddDays(22), Guests = 2, GuestName = TravelerName, Total = 1_081.44m, Status = TravelBookingStatus.Upcoming },
        new TravelBooking { Reference = "DS-41877", StayId = 302, CheckIn = Today.AddDays(-61), CheckOut = Today.AddDays(-57), Guests = 2, GuestName = TravelerName, Total = 779.72m, Status = TravelBookingStatus.Completed },
        new TravelBooking { Reference = "DS-39542", StayId = 314, CheckIn = Today.AddDays(-140), CheckOut = Today.AddDays(-133), Guests = 3, GuestName = TravelerName, Total = 793.32m, Status = TravelBookingStatus.Completed },
    };

    /// <summary>The searchable destinations.</summary>
    public static IReadOnlyList<TravelDestination> Destinations() => new List<TravelDestination>
    {
        new("lisbon", "Lisbon", "Portugal", "Tiled streets, river light, and late dinners", IconName.Sun, "#c2410c"),
        new("kyoto", "Kyoto", "Japan", "Temples, gardens, and quiet lanes", IconName.Tree, "#b91c1c"),
        new("cape-town", "Cape Town", "South Africa", "Mountain views and long beaches", IconName.Water, "#0f766e"),
        new("vancouver", "Vancouver", "Canada", "Forests and the ocean, minutes apart", IconName.Snow, "#1d4ed8"),
    };

    /// <summary>Three to five reviews per stay, with ratings from a fixed pattern.</summary>
    public static List<TravelReview> Reviews(IEnumerable<TravelStay> stays)
    {
        var reviews = new List<TravelReview>();

        foreach (var stay in stays)
        {
            var count = 3 + (stay.Id % 3);

            for (var i = 0; i < count; i++)
            {
                var rating = ratingPattern[(stay.Id + (i * 3)) % ratingPattern.Length];
                reviews.Add(new TravelReview(stay.Id, reviewers[(stay.Id + i) % reviewers.Length], rating, reviewBodies[rating - 1][(stay.Id + i) % 2], Today.AddDays(-(4 + (i * 13) + (stay.Id % 9)))));
            }
        }

        return reviews;
    }

    /// <summary>The places to stay, four or five per destination.</summary>
    public static IReadOnlyList<TravelStay> Stays()
    {
        string[] city = { "Wifi", "Air conditioning", "Kitchen", "Washer" };
        string[] hotel = { "Wifi", "Breakfast included", "24-hour front desk", "Air conditioning" };
        string[] nature = { "Wifi", "Free parking", "Fireplace", "Garden" };
        string[] luxury = { "Wifi", "Pool", "Sea view", "Free parking", "Kitchen" };

        return new List<TravelStay>
        {
            new(301, "Azulejo Loft", "lisbon", TravelStayType.Apartment, "Alfama", "A bright top-floor loft above a tiled stairway, with a balcony over the rooftops and the river.", 128m, 35m, 3, 1, city, true, 38.7114, -9.1305),
            new(302, "Casa do Miradouro", "lisbon", TravelStayType.Apartment, "Bairro Alto", "Two bedrooms a short walk from the viewpoints, quiet once the evening crowds head home.", 164m, 45m, 4, 2, city, false, 38.7130, -9.1460),
            new(303, "Hotel Tejo Azul", "lisbon", TravelStayType.Hotel, "Cais do Sodré", "A small hotel by the river with ferries, trams, and the food market at the door.", 142m, 0m, 2, 1, hotel, true, 38.7060, -9.1440),
            new(304, "Jardim Townhouse", "lisbon", TravelStayType.Villa, "Príncipe Real", "A three-floor townhouse with a walled garden, steps from the antique shops and a leafy square.", 315m, 80m, 6, 3, luxury, true, 38.7170, -9.1500),
            new(305, "Belém Riverside Studio", "lisbon", TravelStayType.Apartment, "Belém", "A compact studio near the monuments and the riverside cycle path.", 92m, 25m, 2, 1, city, true, 38.6970, -9.2060),

            new(306, "Machiya Kotone", "kyoto", TravelStayType.Villa, "Higashiyama", "A restored wooden townhouse with tatami rooms, a cedar bath, and a moss garden.", 228m, 60m, 4, 2, nature, true, 34.9967, 135.7850),
            new(307, "Gion Lantern Inn", "kyoto", TravelStayType.Hotel, "Gion", "A ten-room inn on a lantern-lit lane, with a seasonal breakfast served in your room.", 196m, 0m, 2, 1, hotel, false, 35.0037, 135.7788),
            new(308, "Bamboo Grove Cabin", "kyoto", TravelStayType.Cabin, "Arashiyama", "A cabin at the edge of the hills, a short walk from the bamboo paths and the river.", 154m, 40m, 3, 1, nature, true, 35.0094, 135.6668),
            new(309, "Nishiki Market Flat", "kyoto", TravelStayType.Apartment, "Nakagyo", "A modern flat above a side street of the food market, on the subway line.", 118m, 30m, 3, 1, city, true, 35.0100, 135.7600),

            new(310, "Camps Bay Glass House", "cape-town", TravelStayType.Villa, "Camps Bay", "Floor-to-ceiling windows, a heated pool, and the sunset over the bay from every room.", 420m, 95m, 8, 4, luxury, false, -33.9510, 18.3770),
            new(311, "Bo-Kaap Colour Cottage", "cape-town", TravelStayType.Cabin, "Bo-Kaap", "A small cottage on a bright cobbled street, with a roof deck facing the mountain.", 104m, 30m, 2, 1, nature, true, -33.9200, 18.4150),
            new(312, "The Gardens Hotel", "cape-town", TravelStayType.Hotel, "Gardens", "A calm hotel near the city's green heart, with a pool and a hearty breakfast.", 138m, 0m, 2, 1, hotel, true, -33.9330, 18.4120),
            new(313, "Sea Point Promenade Flat", "cape-town", TravelStayType.Apartment, "Sea Point", "Two bedrooms above the seafront promenade, ideal for morning walks and runs.", 126m, 35m, 4, 2, city, true, -33.9150, 18.3890),
            new(314, "Muizenberg Surf Shack", "cape-town", TravelStayType.Cabin, "Muizenberg", "A relaxed beach house a block from the colorful huts and the surf school.", 98m, 25m, 5, 2, nature, false, -34.1080, 18.4700),

            new(315, "Gastown Brick Loft", "vancouver", TravelStayType.Apartment, "Gastown", "A warehouse loft with exposed brick, near cafés and the waterfront.", 176m, 45m, 3, 1, city, true, 49.2840, -123.1090),
            new(316, "Kitsilano Beach House", "vancouver", TravelStayType.Villa, "Kitsilano", "A family home two streets from the beach, with a deck and a barbecue.", 298m, 75m, 6, 3, luxury, true, 49.2680, -123.1680),
            new(317, "North Shore Cedar Cabin", "vancouver", TravelStayType.Cabin, "North Vancouver", "A cedar cabin among tall trees, close to the trails and the suspension bridges.", 184m, 50m, 4, 2, nature, false, 49.3200, -123.0720),
            new(318, "Harbourlight Hotel", "vancouver", TravelStayType.Hotel, "Yaletown", "A modern hotel by the seawall, with bikes to borrow and harbor views.", 212m, 0m, 2, 1, hotel, true, 49.2750, -123.1210),
        };
    }

    #endregion

    #endregion
}
