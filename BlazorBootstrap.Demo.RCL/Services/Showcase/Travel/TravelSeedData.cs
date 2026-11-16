using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

/// <summary>
/// Fixed, fictitious content for the "Driftwood Stays" travel site. The cities are real, but every stay, host, guest,
/// address, review, and booking is made up. Times are relative to a fixed start, so a fresh tab always shows the same data.
/// </summary>
public static class TravelSeedData
{
    #region Fields and Constants

    /// <summary>The role of a guest (searches, books, and manages trips).</summary>
    public const string GuestRole = "guest";

    /// <summary>The most guests a search can ask for.</summary>
    public const int MaxGuests = 8;

    /// <summary>The longest stay that can be booked, in nights.</summary>
    public const int MaxNights = 14;

    /// <summary>The ID of the seeded guest account.</summary>
    public const string SeededGuestId = "jordan";

    /// <summary>The fictitious brand name.</summary>
    public const string SiteName = "Driftwood Stays";

    /// <summary>Taxes and service fees, as a share of the nightly subtotal.</summary>
    public const decimal TaxRate = 0.12m;

    /// <summary>The seeded guest's name.</summary>
    public const string TravelerName = "Jordan Ashdown";

    /// <summary>When a fresh tab's clock starts; the clock moves on from there (D-029).</summary>
    public static readonly DateTime Now = new(2026, 11, 24, 10, 0, 0);

    /// <summary>The seed's "today".</summary>
    public static readonly DateOnly Today = DateOnly.FromDateTime(Now);

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

    /// <summary>
    /// The bookings of a fresh tab: the demo guest's trips (an upcoming stay, a request waiting for the host, two completed
    /// stays) and other guests' bookings, which take dates on the calendars.
    /// </summary>
    public static List<TravelBooking> Bookings()
    {
        var stays = Stays().ToDictionary(stay => stay.Id);
        var list = new List<TravelBooking>
        {
            Booking(stays[306], "DS-48213", SeededGuestId, TravelerName, 18, 22, 2, TravelBookingStatus.Confirmed, -9, "Visa", "4242"),
            Booking(stays[307], "DS-50112", SeededGuestId, TravelerName, 40, 43, 2, TravelBookingStatus.Requested, -1, "Mastercard", "4444",
                "Hello! We're visiting Kyoto for my partner's birthday and would love to stay at the inn. We'll arrive around 4 pm."),
            Booking(stays[302], "DS-41877", SeededGuestId, TravelerName, -61, -57, 2, TravelBookingStatus.Confirmed, -90, "Visa", "4242"),
            Booking(stays[314], "DS-39542", SeededGuestId, TravelerName, -140, -133, 3, TravelBookingStatus.Confirmed, -170, "Visa", "4242"),
        };

        var others = new (int Stay, string Guest, int In, int Out, int Guests)[]
        {
            (301, "Mia Laurent", 3, 7, 2), (301, "Kofi Mensah", 15, 19, 2), (303, "Elena Petrova", 10, 12, 1), (304, "The Okafor family", 14, 18, 5),
            (305, "Sven Larsen", 1, 4, 2), (306, "Priya Shah", 5, 9, 3), (308, "Tom Becker", 14, 16, 2), (310, "Lina Haddad", 20, 27, 6),
            (311, "Diego Alvarez", 14, 17, 2), (315, "Ayumi Sato", 14, 20, 2), (316, "Noah Fischer", 2, 6, 4), (318, "Grace Kim", 30, 33, 2),
            (309, "Ben Carter", -6, -2, 2), (312, "Iris Bauer", -3, 2, 1),
        };

        for (var index = 0; index < others.Length; index++)
        {
            var row = others[index];
            list.Add(Booking(stays[row.Stay], FormattableString.Invariant($"DS-{47_100 + (index * 37)}"), null, row.Guest, row.In, row.Out, row.Guests, TravelBookingStatus.Confirmed,
                row.In - 30, index % 2 == 0 ? "Visa" : "Mastercard", index % 2 == 0 ? "1881" : "5100"));
        }

        return list;
    }

    /// <summary>Dates hosts have blocked on their calendars (no bookings possible).</summary>
    public static List<TravelBlock> Blocks() => new()
    {
        new(301, Today.AddDays(30), Today.AddDays(33), "Family visit"),
        new(309, Today.AddDays(14), Today.AddDays(16), "Repairs"),
        new(317, Today.AddDays(7), Today.AddDays(14), "Host holiday"),
    };

    /// <summary>The demo accounts: the guest with trips of their own.</summary>
    public static IReadOnlyList<ShowcaseDemoAccount> DemoAccounts() => new List<ShowcaseDemoAccount>
    {
        new(SeededGuestId, GuestRole, TravelerName, "jordan.ashdown@example.com", "stay-demo-26", "JA"),
    };

    /// <summary>The searchable destinations.</summary>
    public static IReadOnlyList<TravelDestination> Destinations() => new List<TravelDestination>
    {
        new("lisbon", "Lisbon", "Portugal", "Tiled streets, river light, and late dinners", IconName.Sun, "#c2410c"),
        new("kyoto", "Kyoto", "Japan", "Temples, gardens, and quiet lanes", IconName.Tree, "#b91c1c"),
        new("cape-town", "Cape Town", "South Africa", "Mountain views and long beaches", IconName.Water, "#0f766e"),
        new("vancouver", "Vancouver", "Canada", "Forests and the ocean, minutes apart", IconName.Snow, "#1d4ed8"),
    };

    /// <summary>The hosts.</summary>
    public static IReadOnlyList<TravelHost> Hosts() => new List<TravelHost>
    {
        new("ines", "Inês Moreira", "IM", 2017, "within an hour", "Born in Alfama. I love sharing the city's quiet corners and the best custard tarts."),
        new("rui", "Rui Santos", "RS", 2019, "within a few hours", "Architect and amateur gardener. Ask me about the walking routes."),
        new("aiko", "Aiko Tanaka", "AT", 2016, "within an hour", "I restored our family machiya and run a small inn in Gion."),
        new("thabo", "Thabo Nkosi", "TN", 2018, "within a day", "Surfer, cook, and Cape Town local. I'll send you my favourite beaches."),
        new("claire", "Claire Dubois", "CD", 2020, "within a few hours", "Moved to Vancouver for the mountains and stayed for the ocean."),
    };

    /// <summary>The roles of the site.</summary>
    public static IReadOnlyList<ShowcaseRole> Roles() => new List<ShowcaseRole>
    {
        new(GuestRole, "Guest", "/showcase/travel/trips", "/showcase/travel/account", "Searches stays, books trips, and messages hosts."),
    };

    /// <summary>Three to five reviews per stay, with ratings from a fixed pattern, plus the demo guest's review of a past trip.</summary>
    public static List<TravelReview> Reviews()
    {
        var reviews = new List<TravelReview>();
        var id = 1;

        foreach (var stay in Stays())
        {
            var count = 3 + (stay.Id % 3);

            for (var i = 0; i < count; i++)
            {
                var rating = ratingPattern[(stay.Id + (i * 3)) % ratingPattern.Length];
                var reply = rating <= 3 && i == 0 ? "Thank you for the honest feedback. We've fixed this since your stay and hope to welcome you again." : null;
                reviews.Add(new TravelReview(id++, stay.Id, reviewers[(stay.Id + i) % reviewers.Length], rating, reviewBodies[rating - 1][(stay.Id + i) % 2], Today.AddDays(-(4 + (i * 13) + (stay.Id % 9))))
                {
                    HostReply = reply,
                });
            }
        }

        reviews.Add(new TravelReview(id, 314, "Jordan A.", 4, "A relaxed base for surfing. The kitchen was small, but the beach is right there.", Today.AddDays(-131))
        {
            AccountId = SeededGuestId,
            BookingReference = "DS-39542",
        });

        return reviews;
    }

    /// <summary>The places to stay, four or five per destination.</summary>
    public static IReadOnlyList<TravelStay> Stays()
    {
        string[] city = { "Wifi", "Air conditioning", "Kitchen", "Washer" };
        string[] hotel = { "Wifi", "Breakfast included", "24-hour front desk", "Air conditioning" };
        string[] nature = { "Wifi", "Free parking", "Fireplace", "Garden" };
        string[] luxury = { "Wifi", "Pool", "Sea view", "Free parking", "Kitchen" };
        const TravelCancellationPolicy flexible = TravelCancellationPolicy.Flexible;
        const TravelCancellationPolicy moderate = TravelCancellationPolicy.Moderate;
        const TravelCancellationPolicy strict = TravelCancellationPolicy.Strict;

        return new List<TravelStay>
        {
            new(301, "Azulejo Loft", "lisbon", TravelStayType.Apartment, "Alfama", "A bright top-floor loft above a tiled stairway, with a balcony over the rooftops and the river.", 128m, 35m, 3, 1, city, flexible, 38.7114, -9.1305, "ines", true, "Beco das Flores 12, 4th floor, Lisbon"),
            new(302, "Casa do Miradouro", "lisbon", TravelStayType.Apartment, "Bairro Alto", "Two bedrooms a short walk from the viewpoints, quiet once the evening crowds head home.", 164m, 45m, 4, 2, city, strict, 38.7130, -9.1460, "ines", false, "Travessa da Luz 7, 2nd floor, Lisbon"),
            new(303, "Hotel Tejo Azul", "lisbon", TravelStayType.Hotel, "Cais do Sodré", "A small hotel by the river with ferries, trams, and the food market at the door.", 142m, 0m, 2, 1, hotel, flexible, 38.7060, -9.1440, "ines", true, "Rua do Cais 31, Lisbon"),
            new(304, "Jardim Townhouse", "lisbon", TravelStayType.Villa, "Príncipe Real", "A three-floor townhouse with a walled garden, steps from the antique shops and a leafy square.", 315m, 80m, 6, 3, luxury, moderate, 38.7170, -9.1500, "rui", true, "Rua do Jardim 44, Lisbon"),
            new(305, "Belém Riverside Studio", "lisbon", TravelStayType.Apartment, "Belém", "A compact studio near the monuments and the riverside cycle path.", 92m, 25m, 2, 1, city, flexible, 38.6970, -9.2060, "rui", true, "Rua da Ribeira 5, ground floor, Lisbon"),

            new(306, "Machiya Kotone", "kyoto", TravelStayType.Villa, "Higashiyama", "A restored wooden townhouse with tatami rooms, a cedar bath, and a moss garden.", 228m, 60m, 4, 2, nature, moderate, 34.9967, 135.7850, "aiko", true, "2-14 Kotone-cho, Higashiyama, Kyoto"),
            new(307, "Gion Lantern Inn", "kyoto", TravelStayType.Hotel, "Gion", "A ten-room inn on a lantern-lit lane, with a seasonal breakfast served in your room.", 196m, 0m, 2, 1, hotel, strict, 35.0037, 135.7788, "aiko", false, "5-3 Chochin-dori, Gion, Kyoto"),
            new(308, "Bamboo Grove Cabin", "kyoto", TravelStayType.Cabin, "Arashiyama", "A cabin at the edge of the hills, a short walk from the bamboo paths and the river.", 154m, 40m, 3, 1, nature, flexible, 35.0094, 135.6668, "aiko", true, "8 Takebayashi, Arashiyama, Kyoto"),
            new(309, "Nishiki Market Flat", "kyoto", TravelStayType.Apartment, "Nakagyo", "A modern flat above a side street of the food market, on the subway line.", 118m, 30m, 3, 1, city, moderate, 35.0100, 135.7600, "aiko", true, "3-21 Ichiba-machi, Nakagyo, Kyoto"),

            new(310, "Camps Bay Glass House", "cape-town", TravelStayType.Villa, "Camps Bay", "Floor-to-ceiling windows, a heated pool, and the sunset over the bay from every room.", 420m, 95m, 8, 4, luxury, strict, -33.9510, 18.3770, "thabo", false, "18 Sunset Ridge, Camps Bay, Cape Town"),
            new(311, "Bo-Kaap Colour Cottage", "cape-town", TravelStayType.Cabin, "Bo-Kaap", "A small cottage on a bright cobbled street, with a roof deck facing the mountain.", 104m, 30m, 2, 1, nature, flexible, -33.9200, 18.4150, "thabo", true, "9 Rainbow Lane, Bo-Kaap, Cape Town"),
            new(312, "The Gardens Hotel", "cape-town", TravelStayType.Hotel, "Gardens", "A calm hotel near the city's green heart, with a pool and a hearty breakfast.", 138m, 0m, 2, 1, hotel, moderate, -33.9330, 18.4120, "thabo", true, "60 Palm Avenue, Gardens, Cape Town"),
            new(313, "Sea Point Promenade Flat", "cape-town", TravelStayType.Apartment, "Sea Point", "Two bedrooms above the seafront promenade, ideal for morning walks and runs.", 126m, 35m, 4, 2, city, flexible, -33.9150, 18.3890, "thabo", true, "Promenade Court 6B, Sea Point, Cape Town"),
            new(314, "Muizenberg Surf Shack", "cape-town", TravelStayType.Cabin, "Muizenberg", "A relaxed beach house a block from the colorful huts and the surf school.", 98m, 25m, 5, 2, nature, strict, -34.1080, 18.4700, "thabo", true, "3 Wave Street, Muizenberg, Cape Town"),

            new(315, "Gastown Brick Loft", "vancouver", TravelStayType.Apartment, "Gastown", "A warehouse loft with exposed brick, near cafés and the waterfront.", 176m, 45m, 3, 1, city, moderate, 49.2840, -123.1090, "claire", true, "220 Cobble Row, Unit 5, Vancouver"),
            new(316, "Kitsilano Beach House", "vancouver", TravelStayType.Villa, "Kitsilano", "A family home two streets from the beach, with a deck and a barbecue.", 298m, 75m, 6, 3, luxury, flexible, 49.2680, -123.1680, "claire", true, "1810 Driftwood Crescent, Vancouver"),
            new(317, "North Shore Cedar Cabin", "vancouver", TravelStayType.Cabin, "North Vancouver", "A cedar cabin among tall trees, close to the trails and the suspension bridges.", 184m, 50m, 4, 2, nature, strict, 49.3200, -123.0720, "claire", false, "45 Fern Trail, North Vancouver"),
            new(318, "Harbourlight Hotel", "vancouver", TravelStayType.Hotel, "Yaletown", "A modern hotel by the seawall, with bikes to borrow and harbor views.", 212m, 0m, 2, 1, hotel, moderate, 49.2750, -123.1210, "claire", true, "900 Seawall Way, Vancouver"),
        };
    }

    /// <summary>The demo guest's conversations with hosts.</summary>
    public static List<TravelThread> Threads() => new()
    {
        new TravelThread
        {
            Id = 1,
            StayId = 306,
            BookingReference = "DS-48213",
            GuestAccountId = SeededGuestId,
            GuestName = TravelerName,
            GuestUnread = true,
            Messages =
            {
                new TravelMessage(false, TravelerName, "Hi Aiko, we're so looking forward to the stay. Is there somewhere to leave bags before check-in?", Now.AddDays(-8)),
                new TravelMessage(true, "Aiko Tanaka", "Welcome! Yes, you can leave bags in the entrance hall from 10 am. I'll send the door code three days before you arrive.", Now.AddDays(-8).AddHours(1)),
                new TravelMessage(true, "Aiko Tanaka", "Also, the cedar bath takes about 20 minutes to fill. Enjoy it after the temple walks!", Now.AddHours(-3)),
            },
        },
        new TravelThread
        {
            Id = 2,
            StayId = 307,
            BookingReference = "DS-50112",
            GuestAccountId = SeededGuestId,
            GuestName = TravelerName,
            HostUnread = true,
            Messages =
            {
                new TravelMessage(false, TravelerName, "Hello! We're visiting Kyoto for my partner's birthday and would love to stay at the inn. We'll arrive around 4 pm.", Now.AddDays(-1)),
            },
        },
    };

    /// <summary>The demo guest's saved stays.</summary>
    public static Dictionary<string, List<int>> Wishlists() => new() { [SeededGuestId] = new() { 304, 310 } };

    #endregion

    #region Private Methods

    private static TravelBooking Booking(TravelStay stay, string reference, string? accountId, string guest, int inDays, int outDays, int guests, TravelBookingStatus status,
        int bookedDaysAgo, string brand, string last4, string? message = null)
    {
        var nights = outDays - inDays;
        var subtotal = stay.NightlyRate * nights;
        var booking = new TravelBooking
        {
            Reference = reference,
            StayId = stay.Id,
            GuestAccountId = accountId,
            GuestName = guest,
            CheckIn = Today.AddDays(inDays),
            CheckOut = Today.AddDays(outDays),
            Guests = guests,
            NightlyRate = stay.NightlyRate,
            CleaningFee = stay.CleaningFee,
            Taxes = Math.Round(subtotal * TaxRate, 2, MidpointRounding.AwayFromZero),
            Status = status,
            CreatedAt = Now.AddDays(bookedDaysAgo).AddHours(-2),
            CardBrand = brand,
            CardLast4 = last4,
            RequestMessage = message,
        };

        if (status == TravelBookingStatus.Confirmed)
            booking.Charges.Add(new TravelCharge(booking.CreatedAt, string.Create(CultureInfo.InvariantCulture, $"Payment, {brand} ending {last4}"), booking.Total));

        return booking;
    }

    #endregion

    #endregion
}
