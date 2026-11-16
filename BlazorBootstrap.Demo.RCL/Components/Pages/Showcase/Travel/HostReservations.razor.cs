using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// The host's reservations: every booking of their listings in five groups with counts (requests, upcoming, staying now,
/// past, and cancelled, declined, or expired). A listing filter (kept in the <c>?stay=</c> query, so a link can open one
/// listing's reservations) and a search by guest name or reference narrow every group. Each group is a plain table with a
/// caption: guest, listing, dates and nights, guests, the phase as text, the total (or "Nothing charged"), and for a
/// request the time left to answer; each row links to the reservation. Also holds the host-side reservation helpers the
/// Today and reservation pages share.
/// </summary>
public partial class HostReservations : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The <c>?stay=</c> value applied last, so a re-render with the same query keeps the selection.</summary>
    private string? appliedStayQuery;

    private string search = string.Empty;

    /// <summary>The selected listing; 0 means every listing.</summary>
    private int stayFilter;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (StayQuery == appliedStayQuery)
            return;

        appliedStayQuery = StayQuery;
        stayFilter = int.TryParse(StayQuery, NumberStyles.None, CultureInfo.InvariantCulture, out var stayId) && State.HostStays().Any(stay => stay.Id == stayId) ? stayId : 0;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns when a request runs out (see <see cref="TravelState.AnswerBy" />).</summary>
    internal static DateTime AnswerBy(TravelBooking booking) => TravelState.AnswerBy(booking);

    /// <summary>Returns the card on a booking, like "Visa ending 4242".</summary>
    internal static string CardText(TravelBooking booking) => $"{booking.CardBrand} ending {booking.CardLast4}";

    /// <summary>Returns the host's first name, for a greeting.</summary>
    internal static string FirstName(string name) => name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? name;

    /// <summary>Returns the host-facing label of a phase (the badge text; the color only repeats it).</summary>
    internal static string PhaseText(TravelTripPhase phase) => phase switch
    {
        TravelTripPhase.Requested => "Request",
        TravelTripPhase.Upcoming => "Confirmed",
        TravelTripPhase.InProgress => "Staying now",
        TravelTripPhase.Completed => "Completed",
        TravelTripPhase.Cancelled => "Cancelled by guest",
        TravelTripPhase.Expired => "Expired",
        _ => "Declined",
    };

    /// <summary>Returns the route of one of the host's reservations.</summary>
    internal static string ReservationUrl(string reference) => $"{ReservationsPath}/{Uri.EscapeDataString(reference)}";

    /// <summary>Returns the route of a conversation in the host's inbox.</summary>
    internal static string InboxThreadUrl(int threadId) => FormattableString.Invariant($"{HostLayout.Base}/inbox/{threadId}");

    /// <summary>
    /// Returns the conversation with the guest of a booking: the one about this booking, else the guest's conversation about
    /// the same listing; <see langword="null" /> for a guest without an account or without a conversation.
    /// </summary>
    internal static TravelThread? ThreadFor(TravelState state, TravelBooking booking)
    {
        var threads = state.HostThreads().Where(thread => thread.StayId == booking.StayId).ToList();

        return threads.FirstOrDefault(thread => thread.BookingReference == booking.Reference)
            ?? (booking.GuestAccountId is { } accountId ? threads.FirstOrDefault(thread => thread.GuestAccountId == accountId) : null);
    }

    /// <summary>Returns the time left until a deadline, like "5 h 20 min left" or "40 min left".</summary>
    internal static string TimeLeft(DateTime deadline, DateTime now)
    {
        var left = deadline - now;

        if (left <= TimeSpan.Zero)
            return "Time is up";

        var minutes = Math.Max(1, (int)Math.Ceiling(left.TotalMinutes));

        return minutes < 60
            ? FormattableString.Invariant($"{minutes} min left")
            : FormattableString.Invariant($"{minutes / 60} h {minutes % 60} min left");
    }

    /// <summary>Returns what a booking is worth to show in a list: the total, or "Nothing charged" for a request, a declined or expired one, or a withdrawn request.</summary>
    internal static string TotalText(TravelBooking booking, TravelTripPhase phase)
    {
        if (booking.Charges.Count == 0)
            return phase == TravelTripPhase.Requested ? $"{TravelDisplay.Money(booking.Total)} if accepted" : "Nothing charged";

        if (phase == TravelTripPhase.Cancelled)
            return booking.Paid == 0 ? "Fully refunded" : $"{TravelDisplay.Money(booking.Paid)} kept";

        return TravelDisplay.Money(booking.Paid);
    }

    #endregion

    #region Private Methods

    private bool Matches(TravelBooking booking)
    {
        if (stayFilter != 0 && booking.StayId != stayFilter)
            return false;

        var text = search.Trim();

        return text.Length == 0
            || booking.GuestName.Contains(text, StringComparison.OrdinalIgnoreCase)
            || booking.Reference.Contains(text, StringComparison.OrdinalIgnoreCase);
    }

    private void ClearFilters()
    {
        search = string.Empty;
        SetStayFilter(0);
    }

    /// <summary>A native select, so the filter changes only when the host picks a listing; the choice goes into the query.</summary>
    private void OnStayChanged(ChangeEventArgs args) =>
        SetStayFilter(int.TryParse(args.Value?.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var stayId) ? stayId : 0);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void SetStayFilter(int stayId)
    {
        stayFilter = State.HostStays().Any(stay => stay.Id == stayId) ? stayId : 0;
        appliedStayQuery = stayFilter == 0 ? null : stayFilter.ToString(CultureInfo.InvariantCulture);
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameter("stay", stayFilter == 0 ? (int?)null : stayFilter), replace: true);
    }

    /// <summary>Returns the line under the title: how many reservations match, and of which listing.</summary>
    private string Summary(IReadOnlyList<ReservationGroup> groups)
    {
        var shown = groups.Sum(group => group.Bookings.Count);
        var count = shown == 1 ? "1 reservation" : FormattableString.Invariant($"{shown} reservations");
        var listing = State.FindStay(stayFilter) is { } stay && stayFilter != 0 ? $" at {stay.Name}" : " across your listings";
        var searched = search.Trim().Length > 0 ? $" matching \"{search.Trim()}\"" : string.Empty;

        return $"{count}{listing}{searched}. {groups[0].Bookings.Count} waiting for your answer.";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the matching reservations by group: requests by answer deadline, upcoming and staying by check-in, the rest newest first.</summary>
    private IReadOnlyList<ReservationGroup> Groups
    {
        get
        {
            var bookings = State.HostBookings().Where(Matches).ToList();

            List<TravelBooking> In(params TravelTripPhase[] phases) => bookings.Where(booking => phases.Contains(State.Phase(booking))).ToList();

            return new[]
            {
                new ReservationGroup("ds-res-requests", "Requests", "No requests waiting. Requests for listings without instant booking show up here.",
                    In(TravelTripPhase.Requested).OrderBy(AnswerBy).ToList()),
                new ReservationGroup("ds-res-upcoming", "Upcoming", "No upcoming stays.", In(TravelTripPhase.Upcoming)),
                new ReservationGroup("ds-res-staying", "Staying now", "No one is staying right now.", In(TravelTripPhase.InProgress)),
                new ReservationGroup("ds-res-past", "Past", "No past stays yet. Stays show up here after check-out.",
                    In(TravelTripPhase.Completed).OrderByDescending(booking => booking.CheckIn).ToList()),
                new ReservationGroup("ds-res-closed", "Cancelled, declined, or expired", "Nothing cancelled, declined, or expired.",
                    In(TravelTripPhase.Cancelled, TravelTripPhase.Declined, TravelTripPhase.Expired).OrderByDescending(booking => booking.CheckIn).ToList()),
            };
        }
    }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>Gets the reservations route.</summary>
    internal static string ReservationsPath => $"{HostLayout.Base}/reservations";

    [Inject] private TravelState State { get; set; } = default!;

    /// <summary>Gets or sets the listing filter from the query (a listing ID).</summary>
    [SupplyParameterFromQuery(Name = "stay")] public string? StayQuery { get; set; }

    #endregion

    #region Nested Types

    /// <summary>One group of reservations: its heading ID, title, empty-state text, and bookings.</summary>
    private sealed record ReservationGroup(string Id, string Title, string Empty, IReadOnlyList<TravelBooking> Bookings);

    #endregion
}
