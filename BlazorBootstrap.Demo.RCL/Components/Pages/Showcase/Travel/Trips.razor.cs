using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// The signed-in guest's trips in three groups: upcoming (requests, confirmed stays, and a stay in progress), past
/// (completed), and cancelled (cancelled by the guest or declined by the host). Each trip shows its photo, stay,
/// destination, dates, guests, phase, and what was paid, and links to the trip page; each group has its own empty state.
/// </summary>
public partial class Trips : ComponentBase, IDisposable
{
    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Count(int count, string one, string many) => count == 1 ? $"1 {one}" : $"{count} {many}";

    /// <summary>Returns what the trip cost the guest so far: the total, nothing for a request or a declined one, or what a cancellation kept.</summary>
    private static string MoneyLine(TravelBooking trip, TravelTripPhase phase)
    {
        if (trip.Charges.Count == 0)
            return phase == TravelTripPhase.Requested ? $"Nothing charged yet · {TravelDisplay.Money(trip.Total)} if accepted" : "Nothing charged";

        if (phase == TravelTripPhase.Cancelled)
            return trip.Paid == 0 ? "Fully refunded" : $"{TravelDisplay.Money(trip.Paid)} kept after the refund";

        return $"Total {TravelDisplay.Money(trip.Paid)}";
    }

    private static string Summary(IReadOnlyList<TripGroup> groups) =>
        $"{Count(groups[0].Trips.Count, "upcoming trip", "upcoming trips")}, {Count(groups[1].Trips.Count, "past stay", "past stays")}, and {groups[2].Trips.Count} cancelled.";

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the guest's trips by group, each in the order <see cref="TravelState.MyBookings" /> returns them.</summary>
    private IReadOnlyList<TripGroup> Groups
    {
        get
        {
            var trips = State.MyBookings();

            List<TravelBooking> In(params TravelTripPhase[] phases) => trips.Where(trip => phases.Contains(State.Phase(trip))).ToList();

            return new[]
            {
                new TripGroup("ds-trips-upcoming", "Upcoming", "No upcoming trips. Search for a stay to plan your next one.",
                    In(TravelTripPhase.InProgress, TravelTripPhase.Requested, TravelTripPhase.Upcoming)),
                new TripGroup("ds-trips-past", "Past", "No past stays yet. Trips show up here after check-out.", In(TravelTripPhase.Completed)),
                new TripGroup("ds-trips-cancelled", "Cancelled", "Nothing cancelled. Cancelled trips and declined requests show up here.",
                    In(TravelTripPhase.Cancelled, TravelTripPhase.Declined)),
            };
        }
    }

    [Inject] private TravelState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One group of trips: its heading ID, title, empty-state text, and trips.</summary>
    private sealed record TripGroup(string Id, string Title, string Empty, IReadOnlyList<TravelBooking> Trips);

    #endregion
}
