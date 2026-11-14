using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

/// <summary>
/// In-memory state of the travel site for the current session: the search, the result filters, and the bookings.
/// Nothing is persisted or sent.
/// </summary>
public sealed class TravelState
{
    #region Fields and Constants

    private readonly List<TravelBooking> bookings = TravelSeedData.Bookings();

    private readonly List<TravelReview> reviews;

    private int nextReference = 52_604;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh session.</summary>
    public TravelState()
    {
        reviews = TravelSeedData.Reviews(Stays);

        Search = new TravelSearch
        {
            DestinationId = Destinations[0].Id,
            CheckIn = TravelSeedData.Today.AddDays(14),
            CheckOut = TravelSeedData.Today.AddDays(18),
            Guests = 2,
        };
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the average rating of a stay, or 0 without reviews.</summary>
    public double AverageRating(int stayId)
    {
        var ratings = reviews.Where(review => review.StayId == stayId).Select(review => review.Rating).ToList();

        return ratings.Count == 0 ? 0 : ratings.Average();
    }

    /// <summary>Books a stay and returns the booking; callers check the dates and guests first with <see cref="ValidateStay" />.</summary>
    public TravelBooking Book(TravelStay stay, DateOnly checkIn, DateOnly checkOut, int guests, string guestName)
    {
        var booking = new TravelBooking
        {
            Reference = string.Create(CultureInfo.InvariantCulture, $"DS-{nextReference++}"),
            StayId = stay.Id,
            CheckIn = checkIn,
            CheckOut = checkOut,
            Guests = guests,
            GuestName = guestName.Trim(),
            Total = Quote(stay, checkIn, checkOut).Total,
            Status = TravelBookingStatus.Upcoming,
        };

        bookings.Insert(0, booking);
        LastBookingReference = booking.Reference;
        Changed?.Invoke();

        return booking;
    }

    /// <summary>Cancels an upcoming booking; other bookings are left as they are.</summary>
    public bool Cancel(string reference)
    {
        var booking = bookings.FirstOrDefault(item => item.Reference == reference);

        if (booking is null || booking.Status != TravelBookingStatus.Upcoming)
            return false;

        booking.Status = TravelBookingStatus.Cancelled;
        Changed?.Invoke();

        return true;
    }

    /// <summary>Finds a destination by its ID; unknown IDs fall back to the first destination.</summary>
    public TravelDestination FindDestination(string? destinationId) =>
        Destinations.FirstOrDefault(destination => destination.Id == destinationId) ?? Destinations[0];

    /// <summary>Finds a stay by its ID, or returns <see langword="null" />.</summary>
    public TravelStay? FindStay(int stayId) => Stays.FirstOrDefault(stay => stay.Id == stayId);

    /// <summary>Prices a stay; fewer than one night prices as zero nights.</summary>
    public TravelQuote Quote(TravelStay stay, DateOnly checkIn, DateOnly checkOut)
    {
        var nights = Math.Max(0, checkOut.DayNumber - checkIn.DayNumber);
        var subtotal = stay.NightlyRate * nights;
        var cleaning = nights == 0 ? 0 : stay.CleaningFee;
        var taxes = Math.Round(subtotal * TravelSeedData.TaxRate, 2, MidpointRounding.AwayFromZero);

        return new TravelQuote(nights, stay.NightlyRate, subtotal, cleaning, taxes);
    }

    /// <summary>Returns the reviews of a stay, newest first.</summary>
    public IReadOnlyList<TravelReview> ReviewsFor(int stayId) =>
        reviews.Where(review => review.StayId == stayId).OrderByDescending(review => review.PostedOn).ToList();

    /// <summary>Returns the stays in the searched destination that fit the guests and pass the filters, in the chosen order.</summary>
    public IReadOnlyList<TravelStay> Results()
    {
        var results = Stays
            .Where(stay => stay.DestinationId == Search.DestinationId)
            .Where(stay => stay.MaxGuests >= Search.Guests)
            .Where(stay => stay.NightlyRate <= Filter.MaxPrice)
            .Where(stay => Filter.Type.Length == 0 || stay.Type.ToString() == Filter.Type)
            .Where(stay => !Filter.FreeCancellationOnly || stay.FreeCancellation);

        return (Filter.Sort switch
        {
            TravelSort.PriceLow => results.OrderBy(stay => stay.NightlyRate),
            TravelSort.PriceHigh => results.OrderByDescending(stay => stay.NightlyRate),
            _ => results.OrderByDescending(stay => AverageRating(stay.Id)).ThenBy(stay => stay.NightlyRate),
        }).ToList();
    }

    /// <summary>Returns the reference of the booking made last and forgets it, so the confirmation shows only once.</summary>
    public string? TakeLastBookingReference()
    {
        var reference = LastBookingReference;
        LastBookingReference = null;

        return reference;
    }

    /// <summary>
    /// Returns why a search can't run, or <see langword="null" /> when it can: check-in from today to a year ahead,
    /// 1 to <see cref="TravelSeedData.MaxNights" /> nights, and 1 to <see cref="TravelSeedData.MaxGuests" /> guests.
    /// </summary>
    public static string? ValidateSearch(DateOnly checkIn, DateOnly checkOut, int guests)
    {
        var nights = checkOut.DayNumber - checkIn.DayNumber;

        if (checkIn < TravelSeedData.Today)
            return "Check-in can't be in the past.";

        if (checkIn > TravelSeedData.Today.AddDays(365))
            return "Check-in must be within the next year.";

        if (nights < 1)
            return "Check-out must be at least one day after check-in.";

        if (nights > TravelSeedData.MaxNights)
            return $"Stays are limited to {TravelSeedData.MaxNights} nights.";

        if (guests < 1 || guests > TravelSeedData.MaxGuests)
            return $"Choose from 1 to {TravelSeedData.MaxGuests} guests.";

        return null;
    }

    /// <summary>Returns why dates and guests can't be booked for a stay, or <see langword="null" /> when they can.</summary>
    public static string? ValidateStay(TravelStay stay, DateOnly checkIn, DateOnly checkOut, int guests) =>
        ValidateSearch(checkIn, checkOut, guests)
        ?? (guests > stay.MaxGuests ? $"{stay.Name} sleeps up to {stay.MaxGuests} {(stay.MaxGuests == 1 ? "guest" : "guests")}." : null);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the bookings, newest first.</summary>
    public IReadOnlyList<TravelBooking> Bookings => bookings;

    /// <summary>Gets the searchable destinations.</summary>
    public IReadOnlyList<TravelDestination> Destinations { get; } = TravelSeedData.Destinations();

    /// <summary>Gets the result filters.</summary>
    public TravelFilter Filter { get; } = new();

    /// <summary>Gets the reference of the booking made last in this session, so the trips page can confirm it.</summary>
    public string? LastBookingReference { get; private set; }

    /// <summary>Gets the current search.</summary>
    public TravelSearch Search { get; }

    /// <summary>Gets every stay.</summary>
    public IReadOnlyList<TravelStay> Stays { get; } = TravelSeedData.Stays();

    #endregion

    #region Events

    /// <summary>Raised when bookings change.</summary>
    public event Action? Changed;

    #endregion
}
