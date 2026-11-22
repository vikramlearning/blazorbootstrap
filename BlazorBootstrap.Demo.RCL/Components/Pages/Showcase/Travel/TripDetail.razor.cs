using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// One of the signed-in guest's trips: a one-time confirmation right after booking, the phase, dates and check-in time,
/// guests, the stay and its host (with a link to message them), the address once confirmed, the price breakdown with every
/// payment and refund, the receipt link, and the cancellation policy. The actions follow the phase: withdraw a request;
/// change the dates (with the availability calendar, the new price, and the difference) or cancel an upcoming trip (with
/// the refund the policy gives); write or edit a review of a completed stay. Each action is confirmed in a
/// <see cref="Modal" /> that names the trip and the money involved; focus returns to the opener, or moves to the status
/// message after the action. Someone else's or an unknown reference shows a not-found screen.
/// </summary>
public partial class TripDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private ElementReference banner;

    /// <summary>The phase the trip was booked in, while its one-time confirmation shows; <see langword="null" /> without one.</summary>
    private TravelTripPhase? bannerPhase;

    private Button? cancelButton;

    private Modal cancelModal = default!;

    /// <summary>The refund shown in the cancel dialog, worked out when it opens.</summary>
    private TravelRefund? cancelPreview;

    private Button? changeButton;

    private int changeGuests;

    private DateOnly changeIn;

    private DateInput<DateOnly>? changeInInput;

    private bool changeDone;

    private Modal changeModal = default!;

    private DateOnly changeOut;

    /// <summary>Whether the change-dates dialog is open; its body renders only then.</summary>
    private bool changing;

    private string? dialogError;

    private Button? editReviewButton;

    private bool editingReview;

    private bool focusBanner;

    private string? loadedReference;

    /// <summary>The button that opened the dialog shown now, so focus can go back to it.</summary>
    private Button? opener;

    private FocusTarget pendingFocus;

    private string? reviewBody;

    private string? reviewBodyError;

    private ElementReference reviewBodyInput;

    private ElementReference reviewHeading;

    private int reviewRating;

    private string? reviewRatingError;

    private string? statusMessage;

    private ElementReference statusRegion;

    private Button? withdrawButton;

    private Modal withdrawModal = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // A booking just made: focus moves to its confirmation once it is on the page, so a screen reader reads it.
        if (focusBanner && banner.Id is not null)
        {
            focusBanner = false;
            await TravelDisplay.FocusAsync(banner);
        }

        var target = pendingFocus;
        pendingFocus = FocusTarget.None;

        switch (target)
        {
            case FocusTarget.Status:
                await TravelDisplay.FocusAsync(statusRegion);
                break;

            case FocusTarget.ReviewBody:
                await TravelDisplay.FocusAsync(reviewBodyInput);
                break;

            case FocusTarget.EditReview:
                await TravelDisplay.FocusAsync(editReviewButton?.Element ?? reviewHeading);
                break;
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (loadedReference == Reference)
            return;

        loadedReference = Reference;
        statusMessage = null;
        bannerPhase = null;
        editingReview = false;
        reviewRating = 0;
        reviewBody = string.Empty;
        reviewRatingError = null;
        reviewBodyError = null;

        // The confirmation shows once: only for the booking just made, and it is forgotten as soon as it is shown.
        if (State.LastBookingReference == Reference && Booking is { } trip)
        {
            State.TakeLastBookingReference();
            bannerPhase = State.Phase(trip);
            focusBanner = true;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string CardText(TravelBooking trip) => $"{trip.CardBrand} ending {trip.CardLast4}";

    /// <summary>Returns the check-in time, like "3 pm".</summary>
    private static string CheckInTime(TravelBooking trip) => TravelState.CheckInAt(trip).ToString("h tt", CultureInfo.InvariantCulture).ToLowerInvariant();

    /// <summary>Formats a date with its weekday, like "Sat, Dec 12, 2026".</summary>
    private static string LongDay(DateOnly date) => date.ToString("ddd, MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>Returns what was refunded to the card in all (refunds and change-downs).</summary>
    private static decimal Refunded(TravelBooking trip) => -trip.Charges.Where(charge => charge.Amount < 0).Sum(charge => charge.Amount);

    /// <summary>Formats a charge; a refund gets a minus sign, which screen readers read.</summary>
    private static string SignedMoney(decimal amount) => amount < 0 ? $"−{TravelDisplay.Money(-amount)}" : TravelDisplay.Money(amount);

    private async Task AskCancelAsync()
    {
        if (Booking is not { } trip)
            return;

        cancelPreview = State.RefundFor(trip);
        dialogError = null;
        changeDone = false;
        opener = cancelButton;

        await cancelModal.ShowAsync();
    }

    private async Task AskChangeAsync()
    {
        if (Booking is not { } trip)
            return;

        changeIn = trip.CheckIn;
        changeOut = trip.CheckOut;
        changeGuests = trip.Guests;
        changing = true;
        changeDone = false;
        dialogError = null;
        opener = changeButton;

        await changeModal.ShowAsync();
    }

    private async Task AskWithdrawAsync()
    {
        dialogError = null;
        changeDone = false;
        opener = withdrawButton;

        await withdrawModal.ShowAsync();
    }

    private void CancelReviewEdit()
    {
        editingReview = false;
        reviewRatingError = null;
        reviewBodyError = null;
        pendingFocus = FocusTarget.EditReview;
    }

    /// <summary>Returns the confirm button's text in the change dialog: what the change costs or gives back, when it can be made.</summary>
    private string ChangeButtonText(TravelBooking trip, TravelStay stay)
    {
        if (!changing || State.ValidateStay(stay, changeIn, changeOut, changeGuests) is not null || !State.IsAvailable(stay.Id, changeIn, changeOut, trip.Reference))
            return "Change dates";

        var difference = State.ChangeAmount(trip, State.QuoteChange(trip, changeIn, changeOut));

        return difference > 0 ? $"Change and pay {TravelDisplay.Money(difference)}"
            : difference < 0 ? $"Change and get {TravelDisplay.Money(-difference)} back"
            : "Change dates";
    }

    private async Task ConfirmCancelAsync()
    {
        var card = Booking is { } trip ? CardText(trip) : "card";
        var result = State.Cancel(Reference, out var refund);

        if (result != TravelResult.Ok)
        {
            dialogError = ResultText(result, "This trip can't be cancelled any more: check-in has started, or it was already cancelled.");
            return;
        }

        bannerPhase = null;
        changeDone = true;
        statusMessage = refund is { Amount: > 0 }
            ? $"Trip cancelled. {TravelDisplay.Money(refund.Amount)} is refunded to your {card}."
            : "Trip cancelled. By the cancellation policy, nothing is refunded.";

        await cancelModal.HideAsync();
    }

    private async Task ConfirmChangeAsync()
    {
        if (Booking is not { } trip || State.FindStay(trip.StayId) is not { } stay)
            return;

        var result = State.ChangeDates(Reference, changeIn, changeOut, changeGuests, out var difference);

        switch (result)
        {
            case TravelResult.Ok:
                bannerPhase = null;
                changeDone = true;
                statusMessage = $"Dates changed to {TravelDisplay.Dates(trip.CheckIn, trip.CheckOut)}, {TravelDisplay.Guests(trip.Guests)}. "
                    + (difference > 0 ? $"{TravelDisplay.Money(difference)} was charged to your {CardText(trip)}."
                        : difference < 0 ? $"{TravelDisplay.Money(-difference)} was refunded to your {CardText(trip)}."
                        : "The price stayed the same.");
                await changeModal.HideAsync();
                break;

            case TravelResult.NoChange:
                dialogError = "These are already your trip's dates and guests. Choose different ones, or keep the current dates.";
                break;

            case TravelResult.Unavailable:
                dialogError = "Some of those nights were just booked by someone else, so nothing changed. Choose other dates.";
                break;

            case TravelResult.Invalid:
                dialogError = State.ValidateStay(stay, changeIn, changeOut, changeGuests) ?? "Those dates or guests can't be booked, so nothing changed.";
                break;

            default:
                dialogError = ResultText(result, "This trip can't be changed any more: check-in has started, or it was cancelled.");
                break;
        }
    }

    private async Task ConfirmWithdrawAsync()
    {
        var card = Booking is { } trip ? CardText(trip) : "card";
        var result = State.Cancel(Reference, out _);

        if (result != TravelResult.Ok)
        {
            dialogError = ResultText(result, "This request can't be withdrawn any more: the host has already answered it.");
            return;
        }

        bannerPhase = null;
        changeDone = true;
        statusMessage = $"Request withdrawn. Nothing was charged, and your {card} is no longer held.";

        await withdrawModal.HideAsync();
    }

    /// <summary>Moving check-in keeps the same number of nights (at least one), so check-out never ends up before it.</summary>
    private void OnChangeInChanged(DateOnly value)
    {
        var nights = Math.Max(1, changeOut.DayNumber - changeIn.DayNumber);

        changeIn = value;

        if (changeOut <= value)
            changeOut = value.AddDays(nights);

        dialogError = null;
    }

    /// <summary>A check-out on or before check-in is ignored; the input's minimum is the day after check-in.</summary>
    private void OnChangeOutChanged(DateOnly value)
    {
        if (value > changeIn)
            changeOut = value;

        dialogError = null;
    }

    /// <summary>A native select, so the count changes only when the guest picks one.</summary>
    private void OnChangeGuestsChanged(ChangeEventArgs args)
    {
        if (int.TryParse(args.Value?.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var guests) && guests is >= 1 and <= TravelSeedData.MaxGuests)
            changeGuests = guests;

        dialogError = null;
    }

    private Task OnChangeShownAsync() => TravelDisplay.FocusAsync(changeInInput?.Element);

    /// <summary>
    /// The dialogs open from code, so Bootstrap does not return focus. After the action, focus goes to the status message
    /// (the opener may be gone); otherwise back to the button that opened the dialog.
    /// </summary>
    private async Task OnDialogHiddenAsync()
    {
        changing = false;
        cancelPreview = null;
        dialogError = null;

        if (changeDone)
        {
            changeDone = false;
            await TravelDisplay.FocusAsync(statusRegion);
        }
        else
        {
            await TravelDisplay.FocusAsync(opener?.Element);
        }

        opener = null;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Returns when a request runs out (see <see cref="TravelState.AnswerBy" />).</summary>
    private static DateTime AnswerBy(TravelBooking trip) => TravelState.AnswerBy(trip);

    /// <summary>Returns the sentence beside the phase badge: where the trip stands and what happens next.</summary>
    private string PhaseSentence(TravelBooking trip, TravelTripPhase phase, TravelHost host)
    {
        var closed = TravelDisplay.Stamp(trip.ClosedAt ?? trip.CreatedAt);

        return phase switch
        {
            TravelTripPhase.Requested => $"Waiting for {host.Name} to answer, by {TravelDisplay.Stamp(AnswerBy(trip))} at the latest. Your card is held, not charged.",
            TravelTripPhase.Expired => $"{host.Name} didn't answer in time, so the request expired. Nothing was charged.",
            TravelTripPhase.Upcoming => trip.CheckIn.DayNumber - State.Today.DayNumber == 1
                ? "Check-in is tomorrow."
                : $"Check-in is in {trip.CheckIn.DayNumber - State.Today.DayNumber} days.",
            TravelTripPhase.InProgress => $"You're staying now. Check-out is {LongDay(trip.CheckOut)}.",
            TravelTripPhase.Completed => $"You stayed {TravelDisplay.Dates(trip.CheckIn, trip.CheckOut)}.",
            TravelTripPhase.Cancelled when trip.Charges.Count == 0 => $"You withdrew this request on {closed}. Nothing was charged.",
            TravelTripPhase.Cancelled when Refunded(trip) == 0 => $"You cancelled on {closed}. By the policy, nothing was refunded.",
            TravelTripPhase.Cancelled => $"You cancelled on {closed}. {TravelDisplay.Money(Refunded(trip))} was refunded.",
            _ => $"{host.Name} declined on {closed}. Nothing was charged.",
        };
    }

    /// <summary>Returns the message for a refused withdraw or cancel; <paramref name="wrongState" /> explains a trip past that point.</summary>
    private static string ResultText(TravelResult result, string wrongState) => result switch
    {
        TravelResult.WrongState => wrongState,
        TravelResult.NotFound => "This trip isn't on your account any more, so nothing changed.",
        TravelResult.NotAllowed => "Sign in as the guest who booked this trip to do that. Nothing changed.",
        _ => "That didn't work, and nothing changed. Try again.",
    };

    /// <summary>Checks the stars and the text, then posts or updates the review; each result gets its own message.</summary>
    private void SaveReview()
    {
        if (Booking is not { } trip || State.FindStay(trip.StayId) is not { } stay)
            return;

        var body = reviewBody?.Trim() ?? string.Empty;
        reviewRatingError = reviewRating is < 1 or > 5 ? "Choose from 1 to 5 stars." : null;
        reviewBodyError = body.Length == 0 ? "Write a few words about your stay."
            : body.Length > TravelState.MaxReviewLength ? $"Use {TravelState.MaxReviewLength} characters or fewer."
            : null;

        if (reviewRatingError is not null || reviewBodyError is not null)
        {
            if (reviewBodyError is not null)
                pendingFocus = FocusTarget.ReviewBody;

            return;
        }

        var hadReview = State.ReviewFor(trip) is not null;
        var result = State.SaveReview(Reference, reviewRating, body, out _);

        switch (result)
        {
            case TravelResult.Ok:
                editingReview = false;
                statusMessage = hadReview ? $"Review updated. The {stay.Name} page shows the new version." : $"Thanks! Your review is posted on the {stay.Name} page.";
                pendingFocus = FocusTarget.Status;
                break;

            case TravelResult.NoChange:
                editingReview = false;
                statusMessage = "Nothing changed in your review.";
                pendingFocus = FocusTarget.Status;
                break;

            case TravelResult.Invalid:
                reviewBodyError = $"Choose 1 to 5 stars and write up to {TravelState.MaxReviewLength} characters.";
                break;

            case TravelResult.WrongState:
                reviewBodyError = "You can review a stay once it's over.";
                break;

            default:
                reviewBodyError = ResultText(result, "You can review a stay once it's over.");
                break;
        }
    }

    private void StartReview(TravelReview review)
    {
        editingReview = true;
        reviewRating = review.Rating;
        reviewBody = review.Body;
        reviewRatingError = null;
        reviewBodyError = null;
        pendingFocus = FocusTarget.ReviewBody;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private TravelBooking? Booking => State.FindBooking(Reference);

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Your trips", Href = TripsPath },
        new() { Text = $"Trip {Reference}", IsCurrentPage = true },
    };

    /// <summary>Gets or sets the booking reference from the route.</summary>
    [Parameter] public string Reference { get; set; } = string.Empty;

    [Inject] private TravelState State { get; set; } = default!;

    private static string TripsPath => $"{TravelLayout.Base}/trips";

    #endregion

    #region Nested Types

    /// <summary>Where focus goes after the next render.</summary>
    private enum FocusTarget
    {
        None,
        Status,
        ReviewBody,
        EditReview,
    }

    #endregion
}
