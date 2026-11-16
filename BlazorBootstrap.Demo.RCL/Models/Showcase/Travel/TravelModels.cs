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

/// <summary>A listing's cancellation policy: how much a guest gets back when they cancel (see <c>TravelState.RefundFor</c>).</summary>
public enum TravelCancellationPolicy
{
    /// <summary>Full refund until 1 day before check-in; after that, the first night is kept.</summary>
    Flexible,

    /// <summary>Full refund until 5 days before check-in; after that, half of the nights are refunded.</summary>
    Moderate,

    /// <summary>Half refund until 7 days before check-in; nothing after that.</summary>
    Strict,
}

/// <summary>Where a booking is in its life. Whether a confirmed stay is upcoming, in progress, or completed follows from the clock.</summary>
public enum TravelBookingStatus
{
    /// <summary>A request to book, waiting for the host. The card is held, not charged.</summary>
    Requested,

    /// <summary>Booked and paid.</summary>
    Confirmed,

    /// <summary>The host declined the request; nothing was charged.</summary>
    Declined,

    /// <summary>Cancelled by the guest (a confirmed booking, refunded by the policy, or a withdrawn request).</summary>
    Cancelled,
}

/// <summary>A destination people can search.</summary>
public sealed record TravelDestination(string Id, string Name, string Country, string Tagline, IconName IconName, string Accent);

/// <summary>A host. Names are fictitious.</summary>
public sealed record TravelHost(string Id, string Name, string Initials, int HostingSince, string ResponseTime, string About);

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
    TravelCancellationPolicy Policy,
    double Latitude,
    double Longitude,
    string HostId,
    bool InstantBook,
    string Address)
{
    #region Fields and Constants

    /// <summary>The number of gallery photos per stay.</summary>
    public const int PhotoCount = 3;

    #endregion

    #region Properties, Indexers

    /// <summary>Gets whether a guest can cancel for a full refund until shortly before check-in (Flexible or Moderate).</summary>
    public bool FreeCancellation => Policy != TravelCancellationPolicy.Strict;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Gets the path of a gallery photo (1 to <see cref="PhotoCount" />); <c>ShowcaseImage</c> shows a placeholder until the file is supplied.</summary>
    public string PhotoPath(int number) => $"_content/BlazorBootstrap.Demo.RCL/showcase/travel/stays/{Id}-{number}.webp";

    #endregion

    #endregion
}

/// <summary>
/// A guest review of a stay. Seeded reviews have no booking; a signed-in guest's review belongs to one completed booking
/// (<see cref="BookingReference" />) and can be edited. <see cref="HostReply" /> is the host's public answer.
/// </summary>
public sealed record TravelReview(int Id, int StayId, string Author, int Rating, string Body, DateOnly PostedOn)
{
    #region Properties, Indexers

    /// <summary>Gets the reviewer's demo account, or <see langword="null" /> for a seeded review.</summary>
    public string? AccountId { get; init; }

    /// <summary>Gets the booking the review is about, or <see langword="null" /> for a seeded review.</summary>
    public string? BookingReference { get; init; }

    /// <summary>Gets the host's public reply, if any.</summary>
    public string? HostReply { get; init; }

    #endregion
}

/// <summary>The price of a stay for some nights.</summary>
public sealed record TravelQuote(int Nights, decimal NightlyRate, decimal Subtotal, decimal CleaningFee, decimal Taxes)
{
    #region Properties, Indexers

    /// <summary>Gets the total to pay.</summary>
    public decimal Total => Subtotal + CleaningFee + Taxes;

    #endregion
}

/// <summary>What a guest would get back if they cancelled now, and the policy line that decides it.</summary>
public sealed record TravelRefund(decimal Amount, decimal Kept, string Rule);

/// <summary>One money movement on a booking: the payment, a date-change difference, or a refund (negative).</summary>
public sealed record TravelCharge(DateTime At, string Description, decimal Amount);

/// <summary>A booking: a guest's stay, its price, its payment, and its history.</summary>
public sealed class TravelBooking
{
    #region Properties, Indexers

    /// <summary>Gets or sets the card brand (test cards only, D-026).</summary>
    public string CardBrand { get; set; } = string.Empty;

    /// <summary>Gets or sets the card's last four digits.</summary>
    public string CardLast4 { get; set; } = string.Empty;

    /// <summary>Gets the payment, date-change differences, and refunds (negative), oldest first. Empty while requested.</summary>
    public List<TravelCharge> Charges { get; init; } = new();

    /// <summary>Gets or sets the check-in date.</summary>
    public DateOnly CheckIn { get; set; }

    /// <summary>Gets or sets the check-out date.</summary>
    public DateOnly CheckOut { get; set; }

    /// <summary>Gets or sets the cleaning fee.</summary>
    public decimal CleaningFee { get; set; }

    /// <summary>Gets or sets when the booking was cancelled or declined.</summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>Gets when the booking or request was made.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets the guest's demo account, or <see langword="null" /> for a guest who booked elsewhere (seeded).</summary>
    public string? GuestAccountId { get; init; }

    /// <summary>Gets or sets the number of guests.</summary>
    public int Guests { get; set; }

    /// <summary>Gets the name on the booking.</summary>
    public string GuestName { get; init; } = string.Empty;

    /// <summary>Gets or sets the nightly rate when booked.</summary>
    public decimal NightlyRate { get; set; }

    /// <summary>Gets the booking reference shown to the guest.</summary>
    public string Reference { get; init; } = string.Empty;

    /// <summary>Gets the guest's message to the host with a request to book.</summary>
    public string? RequestMessage { get; init; }

    /// <summary>Gets the stay.</summary>
    public int StayId { get; init; }

    /// <summary>Gets or sets the booking state.</summary>
    public TravelBookingStatus Status { get; set; }

    /// <summary>Gets or sets the taxes and service fees.</summary>
    public decimal Taxes { get; set; }

    /// <summary>Gets the number of nights.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public int Nights => CheckOut.DayNumber - CheckIn.DayNumber;

    /// <summary>Gets what the guest has paid, net of refunds.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public decimal Paid => Charges.Sum(charge => charge.Amount);

    /// <summary>Gets the nights' subtotal.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public decimal Subtotal => NightlyRate * Nights;

    /// <summary>Gets the price of the stay as booked now (nights, cleaning, taxes).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public decimal Total => Subtotal + CleaningFee + Taxes;

    #endregion
}

/// <summary>A message between a guest and a host. Plain text.</summary>
public sealed record TravelMessage(bool FromHost, string Author, string Body, DateTime SentAt);

/// <summary>A conversation between a guest and the host of one stay, optionally about a booking.</summary>
public sealed class TravelThread
{
    #region Properties, Indexers

    /// <summary>Gets or sets the booking it is about, if any.</summary>
    public string? BookingReference { get; set; }

    /// <summary>Gets the guest's demo account.</summary>
    public string GuestAccountId { get; init; } = string.Empty;

    /// <summary>Gets the guest's name.</summary>
    public string GuestName { get; init; } = string.Empty;

    /// <summary>Gets or sets whether the guest has a host message they haven't read.</summary>
    public bool GuestUnread { get; set; }

    /// <summary>Gets or sets whether the host has a guest message they haven't read.</summary>
    public bool HostUnread { get; set; }

    /// <summary>Gets the thread ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets the messages, oldest first.</summary>
    public List<TravelMessage> Messages { get; init; } = new();

    /// <summary>Gets the stay.</summary>
    public int StayId { get; init; }

    /// <summary>Gets when the last message was sent.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public DateTime UpdatedAt => Messages.Count == 0 ? DateTime.MinValue : Messages[^1].SentAt;

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

    /// <summary>Gets or sets whether only stays that can be booked instantly are shown.</summary>
    public bool InstantBookOnly { get; set; }

    /// <summary>Gets or sets the highest nightly rate shown.</summary>
    public int MaxPrice { get; set; } = TravelFilterLimits.MaxPrice;

    /// <summary>Gets or sets the sort order.</summary>
    public TravelSort Sort { get; set; } = TravelSort.Rating;

    /// <summary>Gets or sets the stay type name; empty means every type.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets whether any filter narrows the results.</summary>
    public bool IsActive => FreeCancellationOnly || InstantBookOnly || MaxPrice < TravelFilterLimits.MaxPrice || Type.Length > 0;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Clears every filter; the sort order stays.</summary>
    public void Clear()
    {
        FreeCancellationOnly = false;
        InstantBookOnly = false;
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

/// <summary>Outcome of a guest action.</summary>
public enum TravelResult
{
    /// <summary>Done.</summary>
    Ok,

    /// <summary>Sign in as a guest first, or it isn't the guest's booking.</summary>
    NotAllowed,

    /// <summary>The booking, stay, or thread doesn't exist.</summary>
    NotFound,

    /// <summary>A value is missing, too long, or out of range.</summary>
    Invalid,

    /// <summary>Some of the nights are already booked or blocked.</summary>
    Unavailable,

    /// <summary>The booking's state doesn't allow it (for example, cancelling a past stay).</summary>
    WrongState,

    /// <summary>The test card was declined.</summary>
    CardDeclined,

    /// <summary>Nothing changed.</summary>
    NoChange,
}

/// <summary>Dates a host blocked on a stay's calendar, from <see cref="Start" /> up to (not including) <see cref="End" />.</summary>
public sealed record TravelBlock(int StayId, DateOnly Start, DateOnly End, string Note);

/// <summary>Where a trip stands for the guest, from its status and the clock.</summary>
public enum TravelTripPhase
{
    /// <summary>Waiting for the host to accept.</summary>
    Requested,

    /// <summary>Confirmed; check-in is ahead.</summary>
    Upcoming,

    /// <summary>Confirmed; the guest is staying now.</summary>
    InProgress,

    /// <summary>Confirmed; the stay is over.</summary>
    Completed,

    /// <summary>Cancelled by the guest.</summary>
    Cancelled,

    /// <summary>Declined by the host.</summary>
    Declined,
}
