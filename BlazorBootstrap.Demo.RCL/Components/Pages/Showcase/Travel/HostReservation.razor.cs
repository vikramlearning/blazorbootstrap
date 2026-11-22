using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;
using static BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel.HostReservations;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// One booking of the signed-in host's listings: the guest (with a link to their conversation), the listing (with a link
/// to its public page), the dates with the check-in time, the guests, the guest's request message, the price breakdown,
/// every charge and refund, the payout (amount, pay date, paid or upcoming, with the taxes and the 3% host fee explained),
/// and the cancellation policy. A request shows its answer deadline and two actions, each confirmed in a
/// <see cref="Modal" /> that names the guest, the dates, and the money: accept (the held card is charged and the dates are
/// confirmed, with an optional note to the guest) or decline (nothing is charged and the dates are freed, with an optional
/// reason). Focus returns to the opener, or moves to the status message after the action. An expired request explains it
/// ran out unanswered. An unknown reference, or one of another host's listings, shows a not-found screen.
/// </summary>
public partial class HostReservation : ComponentBase, IDisposable
{
    #region Fields and Constants

    private Button? acceptButton;

    private Modal acceptModal = default!;

    private bool actionDone;

    private Button? declineButton;

    private Modal declineModal = default!;

    private string? dialogError;

    private string? loadedReference;

    private string note = string.Empty;

    private ElementReference noteInput;

    /// <summary>The button that opened the dialog shown now, so focus can go back to it.</summary>
    private Button? opener;

    private string reason = string.Empty;

    private ElementReference reasonInput;

    private string? statusMessage;

    private ElementReference statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (loadedReference == Reference)
            return;

        loadedReference = Reference;
        statusMessage = null;
        dialogError = null;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Returns the check-in time, like "3 pm".</summary>
    private static string CheckInTime(TravelBooking booking) => TravelState.CheckInAt(booking).ToString("h tt", CultureInfo.InvariantCulture).ToLowerInvariant();

    /// <summary>Returns a guest's initials for the avatar, like "ML" for "Mia Laurent".</summary>
    private static string Initials(string name) =>
        string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(part => char.IsLetter(part[0])).Take(2).Select(part => char.ToUpperInvariant(part[0])));

    /// <summary>Formats a date with its weekday, like "Sat, Dec 12, 2026".</summary>
    private static string LongDay(DateOnly date) => date.ToString("ddd, MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>Returns the host's share of an amount the guest paid: without its taxes, less the host fee (as <see cref="TravelState.PayoutFor" /> works it out).</summary>
    private static decimal HostShare(TravelBooking booking, decimal paid) =>
        booking.Total <= 0 ? 0 : Math.Round(paid * (1 - (booking.Taxes / booking.Total)) * (1 - TravelState.HostFeeRate), 2, MidpointRounding.AwayFromZero);

    /// <summary>Returns the taxes inside an amount the guest paid, in proportion to the booking's taxes.</summary>
    private static decimal TaxShare(TravelBooking booking, decimal paid) =>
        booking.Total <= 0 ? 0 : Math.Round(paid * (booking.Taxes / booking.Total), 2, MidpointRounding.AwayFromZero);

    /// <summary>Returns what was refunded to the guest's card in all.</summary>
    private static decimal Refunded(TravelBooking booking) => -booking.Charges.Where(charge => charge.Amount < 0).Sum(charge => charge.Amount);

    /// <summary>Formats a charge; a refund gets a minus sign, which screen readers read.</summary>
    private static string SignedMoney(decimal amount) => amount < 0 ? $"−{TravelDisplay.Money(-amount)}" : TravelDisplay.Money(amount);

    private async Task AskAcceptAsync()
    {
        note = string.Empty;
        dialogError = null;
        actionDone = false;
        opener = acceptButton;

        await acceptModal.ShowAsync();
    }

    private async Task AskDeclineAsync()
    {
        reason = string.Empty;
        dialogError = null;
        actionDone = false;
        opener = declineButton;

        await declineModal.ShowAsync();
    }

    private async Task ConfirmAcceptAsync()
    {
        if (Booking is not { } booking)
            return;

        var text = note.Trim();

        if (text.Length > TravelState.MaxMessageLength)
        {
            dialogError = $"Your note is {text.Length} characters. Use {TravelState.MaxMessageLength} or fewer.";
            await TravelDisplay.FocusAsync(noteInput);
            return;
        }

        var result = State.Accept(booking.Reference, text);

        if (result != TravelResult.Ok)
        {
            dialogError = ResultText(result, booking);
            return;
        }

        var notified = booking.GuestAccountId is null
            ? $"{booking.GuestName} booked without a {TravelSeedData.SiteName} account, so there's no conversation to notify them in."
            : $"{booking.GuestName} was notified in your conversation{(text.Length > 0 ? " with your note" : null)}, and the trip shows as confirmed for them.";

        statusMessage = $"Request accepted. {booking.GuestName}'s {CardText(booking)} was charged {TravelDisplay.Money(booking.Paid)}, and {TravelDisplay.Dates(booking.CheckIn, booking.CheckOut)} are confirmed. {notified}";
        actionDone = true;

        await acceptModal.HideAsync();
    }

    private async Task ConfirmDeclineAsync()
    {
        if (Booking is not { } booking)
            return;

        var text = reason.Trim();

        if (text.Length > TravelState.MaxMessageLength)
        {
            dialogError = $"Your reason is {text.Length} characters. Use {TravelState.MaxMessageLength} or fewer.";
            await TravelDisplay.FocusAsync(reasonInput);
            return;
        }

        var result = State.Decline(booking.Reference, text);

        if (result != TravelResult.Ok)
        {
            dialogError = ResultText(result, booking);
            return;
        }

        var notified = booking.GuestAccountId is null
            ? $"{booking.GuestName} booked without a {TravelSeedData.SiteName} account, so there's no conversation to notify them in."
            : $"{booking.GuestName} was notified in your conversation{(text.Length > 0 ? " with your reason" : null)}.";

        statusMessage = $"Request declined. Nothing was charged to {booking.GuestName}'s {CardText(booking)}, and {TravelDisplay.Dates(booking.CheckIn, booking.CheckOut)} are free again. {notified}";
        actionDone = true;

        await declineModal.HideAsync();
    }

    /// <summary>
    /// The dialogs open from code, so Bootstrap does not return focus. After the action, focus goes to the status message
    /// (the buttons are gone); otherwise back to the button that opened the dialog.
    /// </summary>
    private async Task OnDialogHiddenAsync()
    {
        dialogError = null;

        if (actionDone)
        {
            actionDone = false;
            await TravelDisplay.FocusAsync(statusRegion);
        }
        else
        {
            await TravelDisplay.FocusAsync(opener?.Element);
        }

        opener = null;
    }

    private Task OnAcceptShownAsync() => TravelDisplay.FocusAsync(noteInput);

    private Task OnDeclineShownAsync() => TravelDisplay.FocusAsync(reasonInput);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Returns the sentence beside the phase badge: where the reservation stands for the host.</summary>
    private string PhaseSentence(TravelBooking booking, TravelTripPhase phase)
    {
        var closed = TravelDisplay.Stamp(booking.ClosedAt ?? booking.CreatedAt);
        var days = booking.CheckIn.DayNumber - State.Today.DayNumber;

        return phase switch
        {
            TravelTripPhase.Requested => $"Waiting for your answer, by {TravelDisplay.Stamp(AnswerBy(booking))} ({TimeLeft(AnswerBy(booking), State.Clock.Now)}).",
            TravelTripPhase.Expired => $"Nobody answered by {TravelDisplay.Stamp(AnswerBy(booking))}, so the request expired. Nothing was charged.",
            TravelTripPhase.Upcoming => days <= 0 ? $"Checks in today from {CheckInTime(booking)}." : days == 1 ? "Checks in tomorrow." : $"Checks in in {days} days.",
            TravelTripPhase.InProgress => $"Staying now. Checks out {LongDay(booking.CheckOut)}.",
            TravelTripPhase.Completed => $"Stayed {TravelDisplay.Dates(booking.CheckIn, booking.CheckOut)}.",
            TravelTripPhase.Cancelled when booking.Charges.Count == 0 => $"{booking.GuestName} withdrew the request on {closed}. Nothing was charged.",
            TravelTripPhase.Cancelled => $"{booking.GuestName} cancelled on {closed}. {TravelDisplay.Money(Refunded(booking))} was refunded by the policy.",
            _ => $"You declined on {closed}. Nothing was charged.",
        };
    }

    /// <summary>Returns the message for a refused accept or decline; every result gets its own explanation.</summary>
    private string ResultText(TravelResult result, TravelBooking booking) => result switch
    {
        TravelResult.WrongState => State.Phase(booking) switch
        {
            TravelTripPhase.Expired => $"The request expired at {TravelDisplay.Stamp(AnswerBy(booking))} before you answered, so nothing changed and nothing was charged.",
            TravelTripPhase.Cancelled => $"{booking.GuestName} withdrew this request, so there's nothing to answer.",
            _ => "This request was already answered, so nothing changed.",
        },
        TravelResult.NotFound => "This reservation isn't on your listings any more, so nothing changed.",
        TravelResult.NotAllowed => "Sign in as the host of this listing to answer the request. Nothing changed.",
        TravelResult.Invalid => $"The message is too long. Use {TravelState.MaxMessageLength} characters or fewer.",
        TravelResult.Unavailable => "Those nights are no longer free, so nothing changed.",
        TravelResult.CardDeclined => $"{booking.GuestName}'s card was declined, so nothing was charged and the request is still open.",
        TravelResult.NoChange => "Nothing changed.",
        TravelResult.InUse or TravelResult.Duplicate => "That can't be done for this reservation. Nothing changed.",
        _ => "That didn't work, and nothing changed. Try again.",
    };

    #endregion

    #endregion

    #region Properties, Indexers

    private TravelBooking? Booking => State.FindHostBooking(Reference);

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Reservations", Href = ReservationsPath },
        new() { Text = $"Reservation {Reference}", IsCurrentPage = true },
    };

    /// <summary>Gets or sets the booking reference from the route.</summary>
    [Parameter] public string Reference { get; set; } = string.Empty;

    [Inject] private TravelState State { get; set; } = default!;

    #endregion
}
