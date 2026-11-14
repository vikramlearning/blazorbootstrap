namespace BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;

/// <summary>Kind of place to stay.</summary>
public enum TravelStayType
{
    /// <summary>A hotel room.</summary>
    Hotel,

    /// <summary>A whole apartment.</summary>
    Apartment,

    /// <summary>A cabin or cottage.</summary>
    Cabin,

    /// <summary>A villa or house.</summary>
    Villa,
}

/// <summary>Sort order of the results.</summary>
public enum TravelSort
{
    /// <summary>Highest rated first.</summary>
    Rating,

    /// <summary>Lowest nightly rate first.</summary>
    PriceLow,

    /// <summary>Highest nightly rate first.</summary>
    PriceHigh,
}

/// <summary>State of a booking.</summary>
public enum TravelBookingStatus
{
    /// <summary>Check-in is today or later.</summary>
    Upcoming,

    /// <summary>The stay is over.</summary>
    Completed,

    /// <summary>Cancelled by the guest.</summary>
    Cancelled,
}

/// <summary>A destination people can search.</summary>
public sealed record TravelDestination(string Id, string Name, string Country, string Tagline, IconName IconName, string Accent);

/// <summary>A place to stay. Names, hosts, and descriptions are fictitious; coordinates are near the destination.</summary>
public sealed record TravelStay(
    int Id,
    string Name,
    string DestinationId,
    TravelStayType Type,
    string Neighborhood,
    string Description,
    decimal NightlyRate,
    decimal CleaningFee,
    int MaxGuests,
    int Bedrooms,
    IReadOnlyList<string> Amenities,
    bool FreeCancellation,
    double Latitude,
    double Longitude)
{
    #region Fields and Constants

    /// <summary>The number of gallery photos per stay.</summary>
    public const int PhotoCount = 3;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Gets the path of a gallery photo (1 to <see cref="PhotoCount" />); <c>ShowcaseImage</c> shows a placeholder until the file is supplied.</summary>
    public string PhotoPath(int number) => $"_content/BlazorBootstrap.Demo.RCL/showcase/travel/stays/{Id}-{number}.webp";

    #endregion

    #endregion
}

/// <summary>A guest review of a stay.</summary>
public sealed record TravelReview(int StayId, string Author, int Rating, string Body, DateOnly PostedOn);

/// <summary>The price of a stay for some nights.</summary>
public sealed record TravelQuote(int Nights, decimal NightlyRate, decimal Subtotal, decimal CleaningFee, decimal Taxes)
{
    #region Properties, Indexers

    /// <summary>Gets the total to pay.</summary>
    public decimal Total => Subtotal + CleaningFee + Taxes;

    #endregion
}

/// <summary>A booking made by the fictitious signed-in traveler.</summary>
public sealed class TravelBooking
{
    #region Properties, Indexers

    /// <summary>Gets or sets the check-in date.</summary>
    public DateOnly CheckIn { get; init; }

    /// <summary>Gets or sets the check-out date.</summary>
    public DateOnly CheckOut { get; init; }

    /// <summary>Gets or sets the number of guests.</summary>
    public int Guests { get; init; }

    /// <summary>Gets or sets the name on the booking.</summary>
    public string GuestName { get; init; } = string.Empty;

    /// <summary>Gets or sets the booking reference shown to the traveler.</summary>
    public string Reference { get; init; } = string.Empty;

    /// <summary>Gets or sets the stay.</summary>
    public int StayId { get; init; }

    /// <summary>Gets or sets the booking state.</summary>
    public TravelBookingStatus Status { get; set; }

    /// <summary>Gets or sets the total price.</summary>
    public decimal Total { get; init; }

    #endregion
}

/// <summary>What the traveler searched for; kept in the site state so it carries across screens.</summary>
public sealed class TravelSearch
{
    #region Properties, Indexers

    /// <summary>Gets or sets the check-in date.</summary>
    public DateOnly CheckIn { get; set; }

    /// <summary>Gets or sets the check-out date.</summary>
    public DateOnly CheckOut { get; set; }

    /// <summary>Gets or sets the destination ID.</summary>
    public string DestinationId { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of guests.</summary>
    public int Guests { get; set; } = 2;

    #endregion
}

/// <summary>Result filters; kept in the site state so they survive opening a stay and coming back.</summary>
public sealed class TravelFilter
{
    #region Properties, Indexers

    /// <summary>Gets or sets whether only stays with free cancellation are shown.</summary>
    public bool FreeCancellationOnly { get; set; }

    /// <summary>Gets or sets the highest nightly rate shown.</summary>
    public int MaxPrice { get; set; } = TravelFilterLimits.MaxPrice;

    /// <summary>Gets or sets the sort order.</summary>
    public TravelSort Sort { get; set; } = TravelSort.Rating;

    /// <summary>Gets or sets the stay type name; empty means every type.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets whether any filter narrows the results.</summary>
    public bool IsActive => FreeCancellationOnly || MaxPrice < TravelFilterLimits.MaxPrice || Type.Length > 0;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Clears every filter; the sort order stays.</summary>
    public void Clear()
    {
        FreeCancellationOnly = false;
        MaxPrice = TravelFilterLimits.MaxPrice;
        Type = string.Empty;
    }

    #endregion

    #endregion
}

/// <summary>Limits of the price filter.</summary>
public static class TravelFilterLimits
{
    #region Fields and Constants

    /// <summary>The highest value of the price slider.</summary>
    public const int MaxPrice = 500;

    /// <summary>The lowest value of the price slider.</summary>
    public const int MinPrice = 50;

    #endregion
}
