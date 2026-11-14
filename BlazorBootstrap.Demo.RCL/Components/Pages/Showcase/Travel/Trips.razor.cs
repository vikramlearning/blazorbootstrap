using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// Trips: a confirmation for a booking just made, then upcoming, past, and cancelled bookings. An upcoming booking can be
/// cancelled after a confirmation in a <see cref="Modal" />; focus then moves to the status message, or back to the
/// "Cancel booking" button when the traveler keeps the booking.
/// </summary>
public partial class Trips : ComponentBase, IDisposable
{
    #region Fields and Constants

    private static readonly (TravelBookingStatus Status, string Title)[] sections =
    {
        (TravelBookingStatus.Upcoming, "Upcoming"),
        (TravelBookingStatus.Completed, "Past trips"),
        (TravelBookingStatus.Cancelled, "Cancelled"),
    };

    private readonly Dictionary<string, Button> cancelButtons = new();

    private Modal cancelModal = default!;

    private bool cancelled;

    private TravelBooking? cancelling;

    private TravelBooking? confirmed;

    private ElementReference confirmation;

    private ElementReference statusRegion;

    private string? statusMessage;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // A booking just made: focus moves to its confirmation, which a screen reader then reads.
        if (firstRender && confirmed is not null)
            await TravelDisplay.FocusAsync(confirmation);
    }

    protected override void OnInitialized()
    {
        var reference = State.TakeLastBookingReference();
        confirmed = State.Bookings.FirstOrDefault(booking => booking.Reference == reference);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static BadgeColor StatusColor(TravelBookingStatus status) => status switch
    {
        TravelBookingStatus.Upcoming => BadgeColor.Primary,
        TravelBookingStatus.Completed => BadgeColor.Secondary,
        _ => BadgeColor.Danger,
    };

    private static string StatusLabel(TravelBookingStatus status) => status switch
    {
        TravelBookingStatus.Upcoming => "Upcoming",
        TravelBookingStatus.Completed => "Completed",
        _ => "Cancelled",
    };

    private async Task AskCancelAsync(TravelBooking trip)
    {
        cancelling = trip;
        cancelled = false;

        await cancelModal.ShowAsync();
    }

    private async Task ConfirmCancelAsync()
    {
        if (cancelling is not null && State.Cancel(cancelling.Reference))
        {
            cancelled = true;
            statusMessage = $"Booking {cancelling.Reference} at {State.FindStay(cancelling.StayId)?.Name} is cancelled.";

            if (confirmed?.Reference == cancelling.Reference)
                confirmed = null;
        }

        await cancelModal.HideAsync();
    }

    /// <summary>
    /// The dialog opens from code, so Bootstrap does not return focus. After a cancellation the button is gone, so focus
    /// goes to the status message; otherwise back to the button that opened the dialog.
    /// </summary>
    private async Task OnCancelHiddenAsync()
    {
        var reference = cancelling?.Reference;
        cancelling = null;

        if (cancelled)
        {
            cancelled = false;
            await TravelDisplay.FocusAsync(statusRegion);
        }
        else if (reference is not null && cancelButtons.TryGetValue(reference, out var button))
        {
            await TravelDisplay.FocusAsync(button.Element);
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private TravelState State { get; set; } = default!;

    private string Summary
    {
        get
        {
            var upcoming = State.Bookings.Count(booking => booking.Status == TravelBookingStatus.Upcoming);
            var past = State.Bookings.Count(booking => booking.Status == TravelBookingStatus.Completed);

            return $"{upcoming} upcoming {(upcoming == 1 ? "trip" : "trips")} and {past} past {(past == 1 ? "stay" : "stays")}.";
        }
    }

    #endregion
}
