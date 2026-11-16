using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;
using static BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel.HostReservations;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// The host's Today dashboard: a greeting by first name; figures for requests waiting (with the soonest answer deadline),
/// check-ins today and tomorrow, guests staying now, earnings this month (payouts paying this month), occupancy of the
/// next 30 days (booked nights of the listed listings, with blocked nights left out of the open nights), and the average
/// rating; the requests that need an answer, with the time left; the guests arriving and leaving in the next 7 days; and
/// links to the unread conversations and the reviews without a reply. Every list has an empty state.
/// </summary>
public partial class HostToday : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How far ahead "Arriving soon" and "Leaving soon" look, in days.</summary>
    private const int SoonDays = 7;

    /// <summary>How far ahead occupancy is worked out, in days.</summary>
    private const int OccupancyDays = 30;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Count(int count, string one, string many) => count == 1 ? $"1 {one}" : FormattableString.Invariant($"{count} {many}");

    /// <summary>Returns a day relative to today: "Today", "Tomorrow", or the weekday and date, like "Mon, Nov 30".</summary>
    private string RelativeDay(DateOnly date) =>
        date == State.Today ? "Today"
        : date == State.Today.AddDays(1) ? "Tomorrow"
        : date.ToString("ddd, MMM d", CultureInfo.InvariantCulture);

    /// <summary>Returns the greeting for the time of day on the site's clock.</summary>
    private string Greeting(string name)
    {
        var hour = State.Clock.Now.Hour;
        var part = hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening";

        return $"{part}, {FirstName(name)}";
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>
    /// Works out occupancy of the next <see cref="OccupancyDays" /> days over the listed listings: nights of confirmed
    /// bookings, and blocked nights, which are left out of the open nights because they can't be booked.
    /// </summary>
    private (int Booked, int Blocked, int Open, int Listings) Occupancy(IReadOnlyList<TravelBooking> bookings)
    {
        var from = State.Today;
        var to = from.AddDays(OccupancyDays);
        var listed = State.HostStays().Where(stay => stay.IsListed).ToList();
        int booked = 0, blocked = 0;

        foreach (var stay in listed)
        {
            var bookedNights = new HashSet<DateOnly>();

            foreach (var booking in bookings.Where(booking => booking.StayId == stay.Id && booking.Status == TravelBookingStatus.Confirmed))
            {
                for (var night = booking.CheckIn < from ? from : booking.CheckIn; night < booking.CheckOut && night < to; night = night.AddDays(1))
                    bookedNights.Add(night);
            }

            var blockedNights = new HashSet<DateOnly>();

            foreach (var block in State.BlocksFor(stay.Id))
            {
                for (var night = block.Start < from ? from : block.Start; night < block.End && night < to; night = night.AddDays(1))
                {
                    if (!bookedNights.Contains(night))
                        blockedNights.Add(night);
                }
            }

            booked += bookedNights.Count;
            blocked += blockedNights.Count;
        }

        return (booked, blocked, (listed.Count * OccupancyDays) - blocked, listed.Count);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private TravelState State { get; set; } = default!;

    #endregion
}
