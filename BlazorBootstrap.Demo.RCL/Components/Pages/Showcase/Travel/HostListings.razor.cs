using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// The signed-in host's listings, listed and unlisted, as cards: the cover photo, name, place and type, nightly rate,
/// guests, instant book or request, cancellation policy, a Listed or Unlisted badge in words, the upcoming bookings and
/// waiting requests, and the average rating. Each card links to the editor, the listing's calendar, and its public page
/// (which the host sees even while it's unlisted). Unlisting asks first (it leaves search; trips go on); listing again
/// doesn't. Deleting asks first; a listing with bookings, conversations, or reviews can't be deleted, so the dialog says
/// so and offers to unlist it. Every <see cref="TravelResult" /> maps to a message in the status region, which also
/// reports a save from the editor (<see cref="Saved" />).
/// </summary>
public partial class HostListings : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<int, ElementReference> deleteButtons = new();

    private readonly Dictionary<int, ElementReference> listButtons = new();

    /// <summary>Set when a dialog's action ran, so focus moves to the status message once it is hidden.</summary>
    private bool actionDone;

    private Modal deleteModal = default!;

    private string? dialogError;

    /// <summary>The dialog shown now.</summary>
    private Modal? openModal;

    /// <summary>The button that opened the dialog shown now, so focus can go back to it.</summary>
    private ElementReference? opener;

    /// <summary>The listing a dialog is about.</summary>
    private TravelStay? pending;

    /// <summary>Whether the listing in the delete dialog has bookings, conversations, or reviews.</summary>
    private bool pendingInUse;

    /// <summary>The save message from the editor, shown after the first render so the status region announces it.</summary>
    private string? savedMessage;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    private Modal unlistModal = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnAfterRender(bool firstRender)
    {
        // The router focuses the heading after navigation; a message added now is announced by the status region.
        if (firstRender && savedMessage is not null)
        {
            SetStatus(savedMessage);
            savedMessage = null;
            StateHasChanged();
        }
    }

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;

        if (Saved is int id && State.HostStays().FirstOrDefault(stay => stay.Id == id) is { } stay)
        {
            savedMessage = Added == true
                ? $"{stay.Name} is added{(stay.IsListed ? " and on the site: guests can find and book it now" : string.Empty)}."
                : $"{stay.Name} is saved. The site shows the changes now.";
        }
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string BedroomsText(int bedrooms) => bedrooms switch
    {
        0 => "studio",
        1 => "1 bedroom",
        _ => string.Create(CultureInfo.InvariantCulture, $"{bedrooms} bedrooms"),
    };

    /// <summary>Says what happens to a listing's trips and requests when it's unlisted.</summary>
    private static string ExistingTripsText((int Upcoming, int Requests, int Reviews) counts)
    {
        if (counts.Upcoming == 0 && counts.Requests == 0)
            return "It has no upcoming trips or waiting requests.";

        var parts = new List<string>(2);

        if (counts.Upcoming > 0)
            parts.Add(counts.Upcoming == 1 ? "The upcoming trip goes on as booked." : string.Create(CultureInfo.InvariantCulture, $"The {counts.Upcoming} upcoming trips go on as booked."));

        if (counts.Requests > 0)
            parts.Add(counts.Requests == 1 ? "The waiting request stays open for your answer." : string.Create(CultureInfo.InvariantCulture, $"The {counts.Requests} waiting requests stay open for your answer."));

        return string.Join(' ', parts);
    }

    private static string ListingCountText(IReadOnlyList<TravelStay> listings)
    {
        var listed = listings.Count(stay => stay.IsListed);
        var total = listings.Count == 1 ? "1 listing" : string.Create(CultureInfo.InvariantCulture, $"{listings.Count} listings");

        return string.Create(CultureInfo.InvariantCulture, $"{total}: {listed} listed, {listings.Count - listed} unlisted.");
    }

    private static string ReviewsText(int count) => count == 1 ? "1 review" : string.Create(CultureInfo.InvariantCulture, $"{count} reviews");

    private static string UpcomingText((int Upcoming, int Requests, int Reviews) counts)
    {
        var bookings = counts.Upcoming == 1 ? "1 booking" : string.Create(CultureInfo.InvariantCulture, $"{counts.Upcoming} bookings");

        return counts.Requests switch
        {
            0 => bookings,
            1 => $"{bookings}, 1 request waiting",
            _ => string.Create(CultureInfo.InvariantCulture, $"{bookings}, {counts.Requests} requests waiting"),
        };
    }

    private async Task AskDeleteAsync(TravelStay stay)
    {
        pending = stay;
        pendingInUse = IsInUse(stay.Id);
        dialogError = null;
        actionDone = false;
        opener = deleteButtons.TryGetValue(stay.Id, out var button) ? button : null;
        openModal = deleteModal;

        await deleteModal.ShowAsync();
    }

    /// <summary>Returns a listing's upcoming (or current) stays, its requests still waiting for an answer, and its reviews.</summary>
    private (int Upcoming, int Requests, int Reviews) Counts(int stayId)
    {
        var bookings = State.HostBookings().Where(booking => booking.StayId == stayId).Select(State.Phase).ToList();

        return (bookings.Count(phase => phase is TravelTripPhase.Upcoming or TravelTripPhase.InProgress),
            bookings.Count(phase => phase == TravelTripPhase.Requested),
            State.HostReviews().Count(review => review.StayId == stayId));
    }

    private async Task ConfirmDeleteAsync()
    {
        if (pending is not { } stay)
            return;

        var result = State.DeleteStay(stay.Id);

        switch (result)
        {
            case TravelResult.Ok:
                actionDone = true;
                SetStatus($"{stay.Name} is deleted. It's no longer on {TravelSeedData.SiteName}.");
                await deleteModal.HideAsync();
                break;

            case TravelResult.InUse:
                pendingInUse = true;
                dialogError = null;
                break;

            default:
                dialogError = ResultText(result);
                break;
        }
    }

    /// <summary>Unlists the listing in the dialog (from the unlist dialog, or "Unlist instead" in the delete dialog).</summary>
    private async Task ConfirmUnlistAsync()
    {
        if (pending is not { } stay)
            return;

        var result = State.SetListed(stay.Id, false);

        if (result is not (TravelResult.Ok or TravelResult.NoChange))
        {
            dialogError = ResultText(result);
            return;
        }

        actionDone = true;
        SetStatus(result == TravelResult.NoChange
            ? $"{stay.Name} was already unlisted."
            : $"{stay.Name} is unlisted. It's out of search and can't be booked; existing trips go on.");

        await (openModal ?? unlistModal).HideAsync();
    }

    /// <summary>Whether a listing has bookings, conversations, or reviews, which keep it from being deleted.</summary>
    private bool IsInUse(int stayId) =>
        State.HostBookings().Any(booking => booking.StayId == stayId) || State.HostThreads().Any(thread => thread.StayId == stayId)
        || State.HostReviews().Any(review => review.StayId == stayId);

    /// <summary>
    /// The dialogs open from code, so Bootstrap does not return focus. After the action, focus goes to the status message
    /// (the opener may be gone); otherwise back to the button that opened the dialog.
    /// </summary>
    private async Task OnDialogHiddenAsync()
    {
        dialogError = null;

        if (actionDone)
        {
            actionDone = false;
            pending = null;
            StateHasChanged();
            await TravelDisplay.FocusAsync(statusRegion);
        }
        else
        {
            await TravelDisplay.FocusAsync(opener);
        }

        opener = null;
        openModal = null;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private static string ResultText(TravelResult result) => result switch
    {
        TravelResult.NotFound => "This listing isn't one of yours any more, so nothing changed.",
        TravelResult.NotAllowed => "Sign in as the host of this listing to do that. Nothing changed.",
        TravelResult.InUse => "This listing has bookings, conversations, or reviews, so it can only be unlisted.",
        _ => "That didn't work, and nothing changed. Try again.",
    };

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>Unlisting asks first; listing again happens at once and is reported in the status message.</summary>
    private async Task ToggleListedAsync(TravelStay stay)
    {
        if (stay.IsListed)
        {
            pending = stay;
            dialogError = null;
            actionDone = false;
            opener = listButtons.TryGetValue(stay.Id, out var button) ? button : null;
            openModal = unlistModal;

            await unlistModal.ShowAsync();

            return;
        }

        var result = State.SetListed(stay.Id, true);

        SetStatus(result switch
        {
            TravelResult.Ok => $"{stay.Name} is listed again: guests can find and book it.",
            TravelResult.NoChange => $"{stay.Name} was already listed.",
            _ => ResultText(result),
        });
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets whether the listing in <see cref="Saved" /> was just added (from the query string).</summary>
    [SupplyParameterFromQuery(Name = "added")] public bool? Added { get; set; }

    private string InUseText =>
        $"{pending?.Name} has bookings, conversations, or reviews. Guests' trips, messages, and reviews need the listing, so it can only be unlisted, not deleted.";

    /// <summary>Gets or sets the listing the editor just saved (from the query string).</summary>
    [SupplyParameterFromQuery(Name = "saved")] public int? Saved { get; set; }

    [Inject] private TravelState State { get; set; } = default!;

    #endregion
}
