using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

/// <summary>
/// State of the "Driftwood Stays" travel site for the current browser tab: bookings (the demo guest's and other guests'
/// that take dates), host calendar blocks, reviews, guest–host messages, wishlists, the search, and the demo sign-in. It is
/// kept in the tab's <c>sessionStorage</c> through the showcase foundation, and its clock moves on from the seed time
/// (D-029), so trips become in progress and completed and refund rules follow the time to check-in; nothing is sent.
/// </summary>
public sealed partial class TravelState : IShowcaseSiteState
{
    #region Fields and Constants

    /// <summary>The hour check-in starts, local time at the stay; cancellation rules count down to it.</summary>
    public const int CheckInHour = 15;

    /// <summary>The longest message to a host.</summary>
    public const int MaxMessageLength = 1000;

    /// <summary>The longest review.</summary>
    public const int MaxReviewLength = 1000;

    /// <summary>The most stays on a wishlist.</summary>
    public const int MaxWishlist = 50;

    private const int MaxBlocks = 500;

    private const int MaxBookings = 500;

    private const int MaxChargesPerBooking = 50;

    private const decimal MaxMoney = 1_000_000m;

    /// <summary>The most messages in one conversation.</summary>
    public const int MaxMessagesPerThread = 200;

    private const int MaxReviews = 1000;

    private const int MaxThreads = 200;

    private static readonly string[] cardBrands = ShowcaseCards.TestCards.Select(card => card.Brand).Distinct().ToArray();

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<TravelBlock> blocks = TravelSeedData.Blocks();

    private readonly List<TravelBooking> bookings = TravelSeedData.Bookings();

    private readonly List<TravelReview> reviews = TravelSeedData.Reviews();

    private readonly List<TravelThread> threads = TravelSeedData.Threads();

    private readonly Dictionary<string, List<int>> wishlists = TravelSeedData.Wishlists();

    private int nextReference = 52_604;

    private int nextReviewId;

    private int nextThreadId;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out.</summary>
    public TravelState()
    {
        nextReviewId = reviews.Max(review => review.Id) + 1;
        nextThreadId = threads.Max(thread => thread.Id) + 1;
        Identity = new ShowcaseIdentity(TravelSeedData.Roles(), TravelSeedData.DemoAccounts());
        Identity.Changed += () => Changed?.Invoke();

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

    /// <summary>
    /// Books a stay as the signed-in guest, after the page charged the test card (D-026). An instant-book stay is confirmed
    /// and paid; another stay becomes a request the host answers, with the card held but not charged, and needs a message
    /// to the host. The nights must be free: no other booking or request and no host block overlaps them.
    /// </summary>
    public TravelResult Book(int stayId, DateOnly checkIn, DateOnly checkOut, int guests, string? message, ShowcaseCardCharge payment, out TravelBooking? booking)
    {
        booking = null;

        if (Identity.Current is not { RoleId: TravelSeedData.GuestRole } account)
            return TravelResult.NotAllowed;

        if (FindStay(stayId) is not { } stay)
            return TravelResult.NotFound;

        message = string.IsNullOrWhiteSpace(message) ? null : message.Trim();

        if (ValidateStay(stay, checkIn, checkOut, guests) is not null || (!stay.InstantBook && message is null) || message is { Length: > MaxMessageLength }
            || bookings.Count >= MaxBookings)
            return TravelResult.Invalid;

        if (!IsAvailable(stay.Id, checkIn, checkOut))
            return TravelResult.Unavailable;

        if (!payment.IsApproved)
            return TravelResult.CardDeclined;

        if (!cardBrands.Contains(payment.Brand) || payment.Last4 is not { Length: 4 } || !payment.Last4.All(char.IsAsciiDigit))
            return TravelResult.Invalid;

        var quote = Quote(stay, checkIn, checkOut);
        booking = new TravelBooking
        {
            Reference = string.Create(CultureInfo.InvariantCulture, $"DS-{nextReference++}"),
            StayId = stay.Id,
            GuestAccountId = account.Id,
            GuestName = account.Name,
            CheckIn = checkIn,
            CheckOut = checkOut,
            Guests = guests,
            NightlyRate = quote.NightlyRate,
            CleaningFee = quote.CleaningFee,
            Taxes = quote.Taxes,
            Status = stay.InstantBook ? TravelBookingStatus.Confirmed : TravelBookingStatus.Requested,
            CreatedAt = Clock.Now,
            CardBrand = payment.Brand,
            CardLast4 = payment.Last4,
            RequestMessage = stay.InstantBook ? null : message,
        };

        if (stay.InstantBook)
            booking.Charges.Add(new TravelCharge(Clock.Now, $"Payment, {CardText(booking)}", booking.Total));

        bookings.Insert(0, booking);

        // The message also goes into the conversation with the host, when it has room; a request keeps it either way.
        if (message is not null && ThreadFor(account, stay.Id, create: true) is { Messages.Count: < MaxMessagesPerThread } thread)
            AddGuestMessage(thread, message, booking.Reference);

        LastBookingReference = booking.Reference;
        Changed?.Invoke();

        return TravelResult.Ok;
    }

    /// <summary>
    /// Cancels one of the signed-in guest's bookings before check-in: a request is withdrawn (nothing was charged); a
    /// confirmed stay is refunded to the card by the listing's policy (<see cref="RefundFor" />).
    /// </summary>
    public TravelResult Cancel(string reference, out TravelRefund? refund)
    {
        refund = null;

        if (FindBooking(reference) is not { } booking)
            return IsGuest ? TravelResult.NotFound : TravelResult.NotAllowed;

        if (Phase(booking) is not (TravelTripPhase.Requested or TravelTripPhase.Upcoming))
            return TravelResult.WrongState;

        refund = RefundFor(booking);

        if (refund.Amount > 0)
            booking.Charges.Add(new TravelCharge(Clock.Now, $"Refund to {CardText(booking)}", -refund.Amount));

        booking.Status = TravelBookingStatus.Cancelled;
        booking.ClosedAt = Clock.Now;
        Changed?.Invoke();

        return TravelResult.Ok;
    }

    /// <summary>
    /// Changes the dates or guests of one of the signed-in guest's confirmed, upcoming stays while <see cref="CanChangeDates" />.
    /// The new nights must be free (the booking's own nights don't count against it). The price is worked out again: a higher
    /// total is charged to the card on file, a lower one refunded by <see cref="ChangeAmount" />. <paramref name="difference" />
    /// is the amount charged (negative: refunded).
    /// </summary>
    public TravelResult ChangeDates(string reference, DateOnly checkIn, DateOnly checkOut, int guests, out decimal difference)
    {
        difference = 0;

        if (FindBooking(reference) is not { } booking)
            return IsGuest ? TravelResult.NotFound : TravelResult.NotAllowed;

        // A slot always stays free for a cancellation's refund (M3).
        if (!CanChangeDates(booking) || FindStay(booking.StayId) is not { } stay)
            return TravelResult.WrongState;

        if (ValidateStay(stay, checkIn, checkOut, guests) is not null)
            return TravelResult.Invalid;

        if (booking.CheckIn == checkIn && booking.CheckOut == checkOut && booking.Guests == guests)
            return TravelResult.NoChange;

        if (!IsAvailable(stay.Id, checkIn, checkOut, booking.Reference))
            return TravelResult.Unavailable;

        var quote = Quote(stay, checkIn, checkOut);
        difference = ChangeAmount(booking, quote);
        var was = TravelDates(booking.CheckIn, booking.CheckOut);

        booking.CheckIn = checkIn;
        booking.CheckOut = checkOut;
        booking.Guests = guests;
        booking.NightlyRate = quote.NightlyRate;
        booking.CleaningFee = quote.CleaningFee;
        booking.Taxes = quote.Taxes;

        if (difference != 0)
        {
            booking.Charges.Add(new TravelCharge(Clock.Now,
                difference > 0 ? $"Change from {was}, charged to {CardText(booking)}" : $"Change from {was}, refunded to {CardText(booking)}", difference));
        }

        Changed?.Invoke();

        return TravelResult.Ok;
    }

    /// <summary>Returns when a booking's check-in starts.</summary>
    public static DateTime CheckInAt(TravelBooking booking) => booking.CheckIn.ToDateTime(new TimeOnly(CheckInHour, 0));

    /// <summary>
    /// Returns how long before check-in date changes close under a policy: as long as a cancellation would still be refunded
    /// at its best rate (Flexible 24 hours, Moderate 5 days, Strict 7 days), so a change can't get around the policy.
    /// </summary>
    public static int ChangeCutoffHours(TravelCancellationPolicy policy) => policy switch
    {
        TravelCancellationPolicy.Flexible => 24,
        TravelCancellationPolicy.Moderate => 5 * 24,
        _ => 7 * 24,
    };

    /// <summary>
    /// Returns what a date change costs (positive, charged) or gives back (negative, refunded) for a new quote. A cheaper
    /// stay is refunded by the policy's best rate: in full, or half under Strict (the rest is kept, like a cancellation).
    /// </summary>
    public decimal ChangeAmount(TravelBooking booking, TravelQuote quote)
    {
        var difference = quote.Total - booking.Total;

        if (difference >= 0)
            return difference;

        return FindStay(booking.StayId)?.Policy == TravelCancellationPolicy.Strict ? -Round(-difference / 2) : difference;
    }

    /// <summary>Returns whether one of the guest's trips can still have its dates changed: confirmed, before check-in, and before the policy's cutoff.</summary>
    public bool CanChangeDates(TravelBooking booking) =>
        Phase(booking) == TravelTripPhase.Upcoming && FindStay(booking.StayId) is { } stay
        && (CheckInAt(booking) - Clock.Now).TotalHours >= ChangeCutoffHours(stay.Policy)
        && booking.Charges.Count < MaxChargesPerBooking - 1;

    /// <summary>
    /// Sends a message to the host of a stay as the signed-in guest: into the guest's conversation about that stay, or a
    /// new one. Returns the conversation, or <see langword="null" /> when the message can't be sent.
    /// </summary>
    public TravelThread? ContactHost(int stayId, string text)
    {
        if (Identity.Current is not { RoleId: TravelSeedData.GuestRole } account || FindStay(stayId) is null || !IsText(text?.Trim(), MaxMessageLength))
            return null;

        var thread = ThreadFor(account, stayId, create: true);

        if (thread is null || thread.Messages.Count >= MaxMessagesPerThread)
            return null;

        AddGuestMessage(thread, text!.Trim(), null);
        Changed?.Invoke();

        return thread;
    }

    /// <summary>Returns one of the signed-in guest's bookings by reference, or <see langword="null" />.</summary>
    public TravelBooking? FindBooking(string? reference) =>
        Identity.Current is { RoleId: TravelSeedData.GuestRole } account
            ? bookings.FirstOrDefault(booking => booking.Reference == reference && booking.GuestAccountId == account.Id)
            : null;

    /// <summary>Finds a destination by its ID; unknown IDs fall back to the first destination.</summary>
    public TravelDestination FindDestination(string? destinationId) =>
        Destinations.FirstOrDefault(destination => destination.Id == destinationId) ?? Destinations[0];

    /// <summary>Returns a host by ID, or <see langword="null" />.</summary>
    public TravelHost? FindHost(string? hostId) => Hosts.FirstOrDefault(host => host.Id == hostId);

    /// <summary>Finds a stay by its ID, or returns <see langword="null" />.</summary>
    public TravelStay? FindStay(int stayId) => Stays.FirstOrDefault(stay => stay.Id == stayId);

    /// <summary>Returns the host of a stay.</summary>
    public TravelHost HostOf(TravelStay stay) => FindHost(stay.HostId) ?? Hosts[0];

    /// <summary>Returns whether the nights from check-in up to check-out are free: no booking or request and no host block overlaps them.</summary>
    public bool IsAvailable(int stayId, DateOnly checkIn, DateOnly checkOut, string? exceptReference = null) =>
        checkOut > checkIn
        && !bookings.Any(booking => booking.StayId == stayId && booking.Reference != exceptReference && HoldsDates(booking)
            && booking.CheckIn < checkOut && checkIn < booking.CheckOut)
        && !blocks.Any(block => block.StayId == stayId && block.Start < checkOut && checkIn < block.End);

    /// <summary>Returns whether the signed-in guest saved a stay.</summary>
    public bool IsWished(int stayId) => Identity.Current is { } account && wishlists.TryGetValue(account.Id, out var saved) && saved.Contains(stayId);

    /// <summary>Returns the signed-in guest's bookings: requests and upcoming stays by check-in, then the rest, newest first.</summary>
    public IReadOnlyList<TravelBooking> MyBookings() =>
        Identity.Current is { RoleId: TravelSeedData.GuestRole } account
            ? bookings.Where(booking => booking.GuestAccountId == account.Id)
                .OrderBy(booking => Phase(booking) is TravelTripPhase.Requested or TravelTripPhase.Upcoming or TravelTripPhase.InProgress ? 0 : 1)
                .ThenBy(booking => Phase(booking) is TravelTripPhase.Requested or TravelTripPhase.Upcoming or TravelTripPhase.InProgress ? booking.CheckIn.DayNumber : -booking.CheckIn.DayNumber)
                .ToList()
            : Array.Empty<TravelBooking>();

    /// <summary>Returns the signed-in guest's conversations, the most recent first.</summary>
    public IReadOnlyList<TravelThread> MyThreads() =>
        Identity.Current is { RoleId: TravelSeedData.GuestRole } account
            ? threads.Where(thread => thread.GuestAccountId == account.Id).OrderByDescending(thread => thread.UpdatedAt).ToList()
            : Array.Empty<TravelThread>();

    /// <summary>Raises <see cref="Changed" /> after a snapshot was restored.</summary>
    public void NotifyRestored() => Changed?.Invoke();

    /// <summary>Returns one of the signed-in guest's conversations and marks the host's messages as read; <see langword="null" /> for anyone else's.</summary>
    public TravelThread? OpenThread(int threadId)
    {
        if (Identity.Current is not { RoleId: TravelSeedData.GuestRole } account
            || threads.FirstOrDefault(thread => thread.Id == threadId && thread.GuestAccountId == account.Id) is not { } thread)
            return null;

        if (thread.GuestUnread)
        {
            thread.GuestUnread = false;
            Changed?.Invoke();
        }

        return thread;
    }

    /// <summary>Returns where a trip stands on the site's clock.</summary>
    public TravelTripPhase Phase(TravelBooking booking) => booking.Status switch
    {
        TravelBookingStatus.Requested => TravelTripPhase.Requested,
        TravelBookingStatus.Declined => TravelTripPhase.Declined,
        TravelBookingStatus.Cancelled => TravelTripPhase.Cancelled,
        _ when Clock.Now < CheckInAt(booking) => TravelTripPhase.Upcoming,
        _ when Today < booking.CheckOut => TravelTripPhase.InProgress,
        _ => TravelTripPhase.Completed,
    };

    /// <summary>Prices a stay; fewer than one night prices as zero nights.</summary>
    public TravelQuote Quote(TravelStay stay, DateOnly checkIn, DateOnly checkOut)
    {
        var nights = Math.Max(0, checkOut.DayNumber - checkIn.DayNumber);
        var subtotal = stay.NightlyRate * nights;
        var cleaning = nights == 0 ? 0 : stay.CleaningFee;

        return new TravelQuote(nights, stay.NightlyRate, subtotal, cleaning, Tax(subtotal));
    }

    /// <summary>
    /// Returns what the guest gets back if they cancel now, by the listing's policy and the time left to check-in
    /// (<see cref="CheckInHour" /> on the check-in day). The cleaning fee always comes back before check-in. A request
    /// returns nothing because nothing was charged.
    /// </summary>
    public TravelRefund RefundFor(TravelBooking booking)
    {
        if (booking.Status == TravelBookingStatus.Requested)
            return new TravelRefund(0, 0, "Nothing was charged for the request, so nothing needs refunding.");

        var stay = FindStay(booking.StayId);
        var hours = (CheckInAt(booking) - Clock.Now).TotalHours;
        var nights = booking.Subtotal + booking.Taxes;
        var firstNight = booking.NightlyRate + Tax(booking.NightlyRate);

        var (kept, rule) = stay?.Policy switch
        {
            TravelCancellationPolicy.Flexible when hours >= 24 => (0m, "Flexible: full refund until 24 hours before check-in."),
            TravelCancellationPolicy.Flexible => (Math.Min(firstNight, nights), "Flexible: less than 24 hours before check-in, the first night is kept."),
            TravelCancellationPolicy.Moderate when hours >= 5 * 24 => (0m, "Moderate: full refund until 5 days before check-in."),
            TravelCancellationPolicy.Moderate => (Round(nights / 2), "Moderate: less than 5 days before check-in, half of the nights are refunded."),
            TravelCancellationPolicy.Strict when hours >= 7 * 24 => (Round(nights / 2), "Strict: until 7 days before check-in, half of the nights are refunded."),
            _ => (nights, "Strict: less than 7 days before check-in, the nights aren't refunded."),
        };

        // The refund comes from the stay's current price; anything an earlier change kept stays kept.
        var amount = Math.Clamp(booking.Total - kept, 0, Math.Max(0, booking.Paid));

        return new TravelRefund(amount, booking.Paid - amount, rule);
    }

    /// <summary>Returns the stays in the searched destination that fit the guests, are free on the searched dates, and pass the filters, in the chosen order.</summary>
    public IReadOnlyList<TravelStay> Results() => Sort(Matching().Where(stay => IsAvailable(stay.Id, Search.CheckIn, Search.CheckOut)));

    /// <summary>Returns the signed-in guest's review of a booking, or <see langword="null" />.</summary>
    public TravelReview? ReviewFor(TravelBooking booking) =>
        reviews.FirstOrDefault(review => review.BookingReference == booking.Reference && review.AccountId == booking.GuestAccountId);

    /// <summary>Returns the reviews of a stay, newest first.</summary>
    public IReadOnlyList<TravelReview> ReviewsFor(int stayId) =>
        reviews.Where(review => review.StayId == stayId).OrderByDescending(review => review.PostedOn).ThenByDescending(review => review.Id).ToList();

    /// <summary>
    /// Writes or edits the signed-in guest's review of a completed stay: 1–5 stars and up to <see cref="MaxReviewLength" />
    /// characters. One review per booking; an edit keeps the host's reply.
    /// </summary>
    public TravelResult SaveReview(string reference, int rating, string body, out TravelReview? review)
    {
        review = null;

        if (FindBooking(reference) is not { } booking)
            return IsGuest ? TravelResult.NotFound : TravelResult.NotAllowed;

        if (Phase(booking) != TravelTripPhase.Completed)
            return TravelResult.WrongState;

        body = body?.Trim() ?? string.Empty;

        if (rating is < 1 or > 5 || !IsText(body, MaxReviewLength))
            return TravelResult.Invalid;

        var existing = ReviewFor(booking);

        if (existing is not null)
        {
            if (existing.Rating == rating && existing.Body == body)
                return TravelResult.NoChange;

            review = existing with { Rating = rating, Body = body };
            reviews[reviews.IndexOf(existing)] = review;
        }
        else
        {
            if (reviews.Count >= MaxReviews)
                return TravelResult.Invalid;

            review = new TravelReview(nextReviewId++, booking.StayId, ReviewerName(Identity.Current!.Name), rating, body, Today)
            {
                AccountId = booking.GuestAccountId,
                BookingReference = booking.Reference,
            };
            reviews.Add(review);
        }

        Changed?.Invoke();

        return TravelResult.Ok;
    }

    /// <summary>Sends the signed-in guest's message in one of their conversations.</summary>
    public TravelResult SendMessage(int threadId, string text)
    {
        if (Identity.Current is not { RoleId: TravelSeedData.GuestRole } account)
            return TravelResult.NotAllowed;

        if (threads.FirstOrDefault(thread => thread.Id == threadId && thread.GuestAccountId == account.Id) is not { } thread)
            return TravelResult.NotFound;

        if (!IsText(text?.Trim(), MaxMessageLength) || thread.Messages.Count >= MaxMessagesPerThread)
            return TravelResult.Invalid;

        AddGuestMessage(thread, text!.Trim(), null);
        Changed?.Invoke();

        return TravelResult.Ok;
    }

    /// <summary>Returns the signed-in guest's conversation about a stay, or <see langword="null" />.</summary>
    public TravelThread? ThreadAbout(int stayId) =>
        Identity.Current is { RoleId: TravelSeedData.GuestRole } account ? ThreadFor(account, stayId, create: false) : null;

    /// <summary>Returns the reference of the booking made last and forgets it, so the confirmation shows only once.</summary>
    public string? TakeLastBookingReference()
    {
        var reference = LastBookingReference;
        LastBookingReference = null;

        return reference;
    }

    /// <summary>Saves or unsaves a stay on the signed-in guest's wishlist.</summary>
    public TravelResult ToggleWish(int stayId)
    {
        if (Identity.Current is not { RoleId: TravelSeedData.GuestRole } account)
            return TravelResult.NotAllowed;

        if (FindStay(stayId) is null)
            return TravelResult.NotFound;

        if (!wishlists.TryGetValue(account.Id, out var saved))
            wishlists[account.Id] = saved = new List<int>();

        if (!saved.Remove(stayId))
        {
            if (saved.Count >= MaxWishlist)
                return TravelResult.Invalid;

            saved.Insert(0, stayId);
        }

        Changed?.Invoke();

        return TravelResult.Ok;
    }

    /// <summary>Validates a stored snapshot and applies it only when every part is valid.</summary>
    public bool TryRestoreData(JsonElement data)
    {
        TravelSnapshot? snapshot;

        try
        {
            snapshot = data.Deserialize<TravelSnapshot>(snapshotOptions);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException)
        {
            return false;
        }

        if (snapshot is not
            {
                Bookings: { Count: <= MaxBookings } storedBookings,
                Reviews: { Count: <= MaxReviews } storedReviews,
                Threads: { Count: <= MaxThreads } storedThreads,
                Wishlists: { Count: <= ShowcaseIdentity.MaxAccounts } storedWishlists,
                Blocks: { Count: <= MaxBlocks } storedBlocks,
            }
            || snapshot.NextReference is < 1000 or > 9_999_999 - MaxBookings
            || snapshot.NextReviewId is < 1 or > 1_000_000 - MaxReviews
            || snapshot.NextThreadId is < 1 or > 1_000_000 - MaxThreads)
            return false;

        if (storedBookings.Any(booking => !ValidBooking(booking, snapshot.NextReference)) || !Unique(storedBookings.Select(booking => booking.Reference)))
            return false;

        var references = storedBookings.ToDictionary(booking => booking.Reference, booking => booking.GuestAccountId, StringComparer.Ordinal);

        if (storedBlocks.Any(block => block is null || FindStay(block.StayId) is null || block.Start >= block.End || !IsPlausibleDay(block.Start) || !IsPlausibleDay(block.End)
                || !IsText(block.Note, 80)))
            return false;

        if (storedReviews.Any(review => review is null || review.Id < 1 || review.Id >= snapshot.NextReviewId || FindStay(review.StayId) is null
                || !IsText(review.Author, ShowcaseIdentity.MaxNameLength) || review.Rating is < 1 or > 5 || !IsText(review.Body, MaxReviewLength) || !IsPlausibleDay(review.PostedOn)
                || (review.HostReply is not null && !IsText(review.HostReply, MaxReviewLength))
                || (review.AccountId is null) != (review.BookingReference is null)
                || (review.AccountId is not null && (!IsAccountId(review.AccountId) || !references.TryGetValue(review.BookingReference!, out var owner) || owner != review.AccountId)))
            || !Unique(storedReviews.Select(review => review.Id))
            || !Unique(storedReviews.Where(review => review.BookingReference is not null).Select(review => review.BookingReference)))
            return false;

        if (storedThreads.Any(thread => thread is null || thread.Id < 1 || thread.Id >= snapshot.NextThreadId || FindStay(thread.StayId) is null || !IsAccountId(thread.GuestAccountId)
                || !IsText(thread.GuestName, ShowcaseIdentity.MaxNameLength) || (thread.BookingReference is not null && !references.ContainsKey(thread.BookingReference))
                || thread.Messages is not { Count: >= 1 and <= MaxMessagesPerThread }
                || thread.Messages.Any(message => message is null || !IsText(message.Author, ShowcaseIdentity.MaxNameLength) || !IsText(message.Body, MaxMessageLength) || !IsPlausible(message.SentAt)))
            || !Unique(storedThreads.Select(thread => thread.Id))
            || !Unique(storedThreads.Select(thread => (thread.GuestAccountId, thread.StayId))))
            return false;

        if (storedWishlists.Any(entry => entry is null || !IsAccountId(entry.AccountId) || entry.StayIds is not { Count: <= MaxWishlist } ids || ids.Any(id => FindStay(id) is null) || !Unique(ids))
            || !Unique(storedWishlists.Select(entry => entry.AccountId)))
            return false;

        bookings.Clear();
        bookings.AddRange(storedBookings);
        blocks.Clear();
        blocks.AddRange(storedBlocks);
        reviews.Clear();
        reviews.AddRange(storedReviews);
        threads.Clear();
        threads.AddRange(storedThreads);
        wishlists.Clear();

        foreach (var entry in storedWishlists)
            wishlists[entry.AccountId] = entry.StayIds!.ToList();

        nextReference = snapshot.NextReference;
        nextReviewId = snapshot.NextReviewId;
        nextThreadId = snapshot.NextThreadId;

        return true;
    }

    /// <summary>Returns the nights of a stay that can't be booked between two dates (booked, requested, or blocked), for a calendar.</summary>
    public IReadOnlySet<DateOnly> UnavailableNights(int stayId, DateOnly from, DateOnly to, string? exceptReference = null)
    {
        var nights = new HashSet<DateOnly>();

        foreach (var (start, end) in bookings.Where(booking => booking.StayId == stayId && booking.Reference != exceptReference && HoldsDates(booking))
                     .Select(booking => (booking.CheckIn, booking.CheckOut))
                     .Concat(blocks.Where(block => block.StayId == stayId).Select(block => (block.Start, block.End))))
        {
            for (var night = start < from ? from : start; night < end && night < to; night = night.AddDays(1))
                nights.Add(night);
        }

        return nights;
    }

    /// <summary>Returns the stays that match the search and filters but are booked on the searched dates.</summary>
    public IReadOnlyList<TravelStay> UnavailableResults() => Sort(Matching().Where(stay => !IsAvailable(stay.Id, Search.CheckIn, Search.CheckOut)));

    /// <summary>
    /// Returns why a search can't run, or <see langword="null" /> when it can: check-in from today to a year ahead,
    /// 1 to <see cref="TravelSeedData.MaxNights" /> nights, and 1 to <see cref="TravelSeedData.MaxGuests" /> guests.
    /// </summary>
    public string? ValidateSearch(DateOnly checkIn, DateOnly checkOut, int guests)
    {
        var nights = checkOut.DayNumber - checkIn.DayNumber;

        if (checkIn < Today)
            return "Check-in can't be in the past.";

        if (checkIn > Today.AddDays(365))
            return "Check-in must be within the next year.";

        if (nights < 1)
            return "Check-out must be at least one day after check-in.";

        if (nights > TravelSeedData.MaxNights)
            return $"Stays are limited to {TravelSeedData.MaxNights} nights.";

        if (guests < 1 || guests > TravelSeedData.MaxGuests)
            return $"Choose from 1 to {TravelSeedData.MaxGuests} guests.";

        return null;
    }

    /// <summary>Returns why dates and guests can't be booked for a stay, or <see langword="null" /> when they can (availability is checked separately).</summary>
    public string? ValidateStay(TravelStay stay, DateOnly checkIn, DateOnly checkOut, int guests) =>
        ValidateSearch(checkIn, checkOut, guests)
        ?? (guests > stay.MaxGuests ? $"{stay.Name} sleeps up to {stay.MaxGuests} {(stay.MaxGuests == 1 ? "guest" : "guests")}." : null);

    /// <summary>Returns the signed-in guest's saved stays, the most recently saved first.</summary>
    public IReadOnlyList<TravelStay> Wishlist() =>
        Identity.Current is { } account && wishlists.TryGetValue(account.Id, out var saved)
            ? saved.Select(FindStay).OfType<TravelStay>().ToList()
            : Array.Empty<TravelStay>();

    /// <summary>Captures the tab's data: bookings, blocks, reviews, conversations, wishlists, and the ID counters.</summary>
    public object CaptureData() => new TravelSnapshot
    {
        Blocks = blocks.ToList(),
        Bookings = bookings.ToList(),
        NextReference = nextReference,
        NextReviewId = nextReviewId,
        NextThreadId = nextThreadId,
        Reviews = reviews.ToList(),
        Threads = threads.ToList(),
        Wishlists = wishlists.Select(pair => new TravelWishlist { AccountId = pair.Key, StayIds = pair.Value.ToList() }).ToList(),
    };

    #endregion

    #region Private Methods

    private static string CardText(TravelBooking booking) => $"{booking.CardBrand} ending {booking.CardLast4}";

    private static bool HoldsDates(TravelBooking booking) => booking.Status is TravelBookingStatus.Requested or TravelBookingStatus.Confirmed;

    private static bool IsAccountId(string? accountId) =>
        accountId is not null && (TravelSeedData.DemoAccounts().Any(demo => demo.Id == accountId) || (accountId.StartsWith("u-", StringComparison.Ordinal) && accountId.Length <= 12));

    private static bool IsMoney(decimal value) => value is >= -MaxMoney and <= MaxMoney && decimal.Round(value, 2) == value;

    private static bool IsPlausible(DateTime value) => value >= TravelSeedData.Now.AddYears(-2) && value <= TravelSeedData.Now.AddYears(3);

    private static bool IsPlausibleDay(DateOnly value) => value >= TravelSeedData.Today.AddYears(-2) && value <= TravelSeedData.Today.AddYears(3);

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

    [GeneratedRegex(@"^DS-(?<number>\d{4,7})$")]
    private static partial Regex ReferencePattern();

    /// <summary>Returns a reviewer's public name: their first name and last initial, like "Jordan A.".</summary>
    private static string ReviewerName(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length < 2 ? name : $"{parts[0]} {char.ToUpperInvariant(parts[^1][0])}.";
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static decimal Tax(decimal subtotal) => Round(subtotal * TravelSeedData.TaxRate);

    private static string TravelDates(DateOnly checkIn, DateOnly checkOut) =>
        $"{checkIn.ToString("MMM d", CultureInfo.InvariantCulture)} – {checkOut.ToString("MMM d", CultureInfo.InvariantCulture)}";

    private static bool Unique<T>(IEnumerable<T> values)
    {
        var list = values.ToList();

        return list.Distinct().Count() == list.Count;
    }

    private void AddGuestMessage(TravelThread thread, string text, string? reference)
    {
        if (reference is not null)
            thread.BookingReference = reference;

        thread.Messages.Add(new TravelMessage(false, Identity.Current?.Name ?? thread.GuestName, text, Clock.Now));
        thread.HostUnread = true;
    }

    private IEnumerable<TravelStay> Matching() => Stays
        .Where(stay => stay.DestinationId == Search.DestinationId)
        .Where(stay => stay.MaxGuests >= Search.Guests)
        .Where(stay => stay.NightlyRate <= Filter.MaxPrice)
        .Where(stay => Filter.Type.Length == 0 || stay.Type.ToString() == Filter.Type)
        .Where(stay => !Filter.FreeCancellationOnly || stay.FreeCancellation)
        .Where(stay => !Filter.InstantBookOnly || stay.InstantBook);

    private IReadOnlyList<TravelStay> Sort(IEnumerable<TravelStay> stays) => (Filter.Sort switch
    {
        TravelSort.PriceLow => stays.OrderBy(stay => stay.NightlyRate),
        TravelSort.PriceHigh => stays.OrderByDescending(stay => stay.NightlyRate),
        _ => stays.OrderByDescending(stay => AverageRating(stay.Id)).ThenBy(stay => stay.NightlyRate),
    }).ToList();

    private TravelThread? ThreadFor(ShowcaseAccount account, int stayId, bool create)
    {
        var thread = threads.FirstOrDefault(candidate => candidate.GuestAccountId == account.Id && candidate.StayId == stayId);

        if (thread is null && create && threads.Count < MaxThreads)
        {
            thread = new TravelThread { Id = nextThreadId++, StayId = stayId, GuestAccountId = account.Id, GuestName = account.Name };
            threads.Add(thread);
        }

        return thread;
    }

    private bool ValidBooking(TravelBooking? booking, int nextReference)
    {
        if (booking is null || ReferencePattern().Match(booking.Reference ?? string.Empty) is not { Success: true } match
            || !int.TryParse(match.Groups["number"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var number) || number >= nextReference
            || FindStay(booking.StayId) is not { } stay)
            return false;

        var closed = booking.Status is TravelBookingStatus.Cancelled or TravelBookingStatus.Declined;

        return (booking.GuestAccountId is null || IsAccountId(booking.GuestAccountId)) && IsText(booking.GuestName, ShowcaseIdentity.MaxNameLength)
            && booking.Guests >= 1 && booking.Guests <= stay.MaxGuests
            && booking.Nights is >= 1 and <= TravelSeedData.MaxNights && IsPlausibleDay(booking.CheckIn) && IsPlausible(booking.CreatedAt)
            && booking.NightlyRate is > 0 and <= 100_000 && IsMoney(booking.NightlyRate) && booking.CleaningFee is >= 0 and <= 100_000 && IsMoney(booking.CleaningFee)
            && booking.Taxes is >= 0 and <= MaxMoney && IsMoney(booking.Taxes)
            && Enum.IsDefined(booking.Status) && closed == booking.ClosedAt is not null && (booking.ClosedAt is null || IsPlausible(booking.ClosedAt.Value))
            && cardBrands.Contains(booking.CardBrand) && booking.CardLast4 is { Length: 4 } && booking.CardLast4.All(char.IsAsciiDigit)
            && (booking.RequestMessage is null || IsText(booking.RequestMessage, MaxMessageLength))
            && booking.Charges is { Count: <= MaxChargesPerBooking }
            && booking.Charges.All(charge => charge is not null && IsPlausible(charge.At) && IsText(charge.Description, 160) && IsMoney(charge.Amount))
            && (booking.Status != TravelBookingStatus.Confirmed || (booking.Charges.Count >= 1 && booking.Paid >= booking.Total))
            && (booking.Status is not (TravelBookingStatus.Requested or TravelBookingStatus.Declined) || booking.Charges.Count == 0)
            && booking.Paid >= 0 && booking.Paid <= booking.Charges.Where(charge => charge.Amount > 0).Sum(charge => charge.Amount);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the site's clock, which moves on from the seed time (D-029).</summary>
    public ShowcaseClock Clock { get; } = new(TravelSeedData.Now);

    /// <summary>Gets the searchable destinations.</summary>
    public IReadOnlyList<TravelDestination> Destinations { get; } = TravelSeedData.Destinations();

    /// <summary>Gets the result filters.</summary>
    public TravelFilter Filter { get; } = new();

    /// <summary>Gets the hosts.</summary>
    public IReadOnlyList<TravelHost> Hosts { get; } = TravelSeedData.Hosts();

    /// <summary>Gets the site's demo sign-in.</summary>
    public ShowcaseIdentity Identity { get; }

    /// <summary>Gets whether the signed-in account is a guest.</summary>
    public bool IsGuest => Identity.Current?.RoleId == TravelSeedData.GuestRole;

    /// <summary>Gets the reference of the booking made last in this tab, so the trip page can confirm it.</summary>
    public string? LastBookingReference { get; private set; }

    /// <summary>Gets the current search.</summary>
    public TravelSearch Search { get; }

    /// <summary>Gets the site's slug, used in the storage key.</summary>
    public string SiteSlug => "travel";

    /// <summary>Gets the snapshot format version.</summary>
    public int SnapshotVersion => 1;

    /// <summary>Gets every stay.</summary>
    public IReadOnlyList<TravelStay> Stays { get; } = TravelSeedData.Stays();

    /// <summary>Gets the site's today, on its clock.</summary>
    public DateOnly Today => DateOnly.FromDateTime(Clock.Now);

    /// <summary>Gets how many of the signed-in guest's conversations have a host message they haven't read.</summary>
    public int UnreadThreadCount => Identity.Current is { RoleId: TravelSeedData.GuestRole } account ? threads.Count(thread => thread.GuestAccountId == account.Id && thread.GuestUnread) : 0;

    #endregion

    #region Events

    /// <summary>Raised when anything in the state changes, including sign-in.</summary>
    public event Action? Changed;

    #endregion

    #region Nested Types

    /// <summary>The travel site's part of the tab snapshot.</summary>
    private sealed class TravelSnapshot
    {
        #region Properties, Indexers

        public List<TravelBlock>? Blocks { get; set; }

        public List<TravelBooking>? Bookings { get; set; }

        public int NextReference { get; set; }

        public int NextReviewId { get; set; }

        public int NextThreadId { get; set; }

        public List<TravelReview>? Reviews { get; set; }

        public List<TravelThread>? Threads { get; set; }

        public List<TravelWishlist>? Wishlists { get; set; }

        #endregion
    }

    /// <summary>One account's saved stays, in the snapshot.</summary>
    private sealed class TravelWishlist
    {
        #region Properties, Indexers

        public string AccountId { get; set; } = string.Empty;

        public List<int>? StayIds { get; set; }

        #endregion
    }

    #endregion
}
