using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// Stay page: photo gallery, rating, the wishlist heart, description, amenities, a map of the area (the address is only
/// on a confirmed trip), the host with "Contact host" (a message dialog that opens the conversation), the cancellation
/// policy, an availability calendar, and reviews with the host's replies. The booking box uses the current search's dates
/// and guests, checks the rules and the availability of those nights as they change, prices the stay, and leads to the
/// booking step: "Reserve" for an instant-book stay, "Request to book" otherwise. Signed-out visitors can use it all;
/// contacting the host asks them to sign in, and the booking step handles its own sign-in. An unknown ID renders the
/// site's not-found screen.
/// </summary>
public partial class StayDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private Button? contactButton;

    private string? contactError;

    private ElementReference contactInput;

    private Modal contactModal = default!;

    private string contactText = string.Empty;

    private bool sendingContact;

    /// <summary>The conversation to open once the dialog has closed after sending.</summary>
    private int? sentThreadId;

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

    /// <summary>Returns a host's first name, like "Aiko".</summary>
    private static string FirstName(TravelHost host) => host.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? host.Name;

    /// <summary>Moving check-in keeps the same number of nights (at least one), so check-out never ends up before it.</summary>
    private void OnCheckInChanged(DateOnly value)
    {
        var nights = Math.Max(1, State.Search.CheckOut.DayNumber - State.Search.CheckIn.DayNumber);

        State.Search.CheckIn = value;

        if (State.Search.CheckOut <= value)
            State.Search.CheckOut = value.AddDays(nights);
    }

    /// <summary>A check-out on or before check-in is ignored; the input's minimum is the day after check-in.</summary>
    private void OnCheckOutChanged(DateOnly value)
    {
        if (value > State.Search.CheckIn)
            State.Search.CheckOut = value;
    }

    /// <summary>
    /// The dialog opens from code, so Bootstrap does not return focus: after sending, the conversation opens; after
    /// Cancel or Escape, focus goes back to "Contact host".
    /// </summary>
    private async Task OnContactHiddenAsync()
    {
        if (sentThreadId is { } threadId)
        {
            sentThreadId = null;
            Navigation.NavigateTo(TravelDisplay.ThreadUrl(threadId));

            return;
        }

        await TravelDisplay.FocusAsync(contactButton?.Element);
    }

    private Task OnContactShownAsync() => TravelDisplay.FocusAsync(contactInput);

    /// <summary>An empty or out-of-range guest count is ignored, so the shared search always has 1 to 8 guests.</summary>
    private void OnGuestsChanged(int value)
    {
        if (value is >= 1 and <= TravelSeedData.MaxGuests && value != State.Search.Guests)
            State.Search.Guests = value;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Opens the message dialog with an empty message.</summary>
    private async Task OpenContactAsync()
    {
        sendingContact = false;
        contactText = string.Empty;
        contactError = null;
        sentThreadId = null;

        await contactModal.ShowAsync();
    }

    /// <summary>Sends the message into the guest's conversation about this stay, then closes the dialog and opens it.</summary>
    private async Task SendContactAsync()
    {
        if (Stay is null || sendingContact)
            return;

        var text = contactText.Trim();
        var host = State.HostOf(Stay);

        if (text.Length == 0)
            contactError = $"Write a message to {FirstName(host)} first.";
        else if (text.Length > TravelState.MaxMessageLength)
            contactError = $"Shorten the message to {TravelState.MaxMessageLength} characters or fewer.";
        else if (!State.IsGuest)
            contactError = "You've been signed out. Sign in as a guest to send the message.";
        else if (State.ContactHost(Stay.Id, text) is { } thread)
        {
            // Guards a second click or Enter while the dialog closes, so the message isn't sent twice.
            sendingContact = true;
            contactError = null;
            contactText = string.Empty;
            sentThreadId = thread.Id;
            await contactModal.HideAsync();

            return;
        }
        else
            contactError = "The message couldn't be sent: this conversation is full. Open it from Messages to see the earlier messages.";

        await TravelDisplay.FocusAsync(contactInput);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Rounded to one decimal; the library draws it to the nearest half star.</summary>
    private decimal AverageRating => Stay is null ? 0 : Math.Round((decimal)State.AverageRating(Stay.Id), 1, MidpointRounding.AwayFromZero);

    /// <summary>
    /// The booking step with the box's dates (yyyy-MM-dd) and guests in the query; the box edits the shared search too,
    /// which the booking step falls back to.
    /// </summary>
    private string BookUrl => string.Create(CultureInfo.InvariantCulture,
        $"{TravelDisplay.StayUrl(Id)}/book?checkIn={State.Search.CheckIn:yyyy-MM-dd}&checkOut={State.Search.CheckOut:yyyy-MM-dd}&guests={State.Search.Guests}");

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Search", Href = TravelLayout.Base },
        new() { Text = $"Stays in {Destination.Name}", Href = $"{TravelLayout.Base}/stays?destination={Destination.Id}" },
        new() { Text = Stay?.Name, IsCurrentPage = true },
    };

    private bool CanBook => RuleError is null && IsAvailable;

    private TravelDestination Destination => State.FindDestination(Stay?.DestinationId);

    /// <summary>Gets or sets the stay ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    /// <summary>Whether every night from check-in up to check-out is free (only checked once the dates follow the rules).</summary>
    private bool IsAvailable => Stay is not null && State.IsAvailable(Stay.Id, State.Search.CheckIn, State.Search.CheckOut);

    private IReadOnlyList<ShowcaseLocation> Location => Stay is null
        ? Array.Empty<ShowcaseLocation>()
        : new[] { TravelDisplay.Location(Stay, State.AverageRating(Stay.Id)) };

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>The policy's name, like "Moderate cancellation".</summary>
    private string PolicyName => $"{Stay?.Policy} cancellation";

    private TravelQuote Quote => State.Quote(Stay!, State.Search.CheckIn, State.Search.CheckOut);

    private string ReviewCountText => Reviews.Count == 1 ? "1 review" : $"{Reviews.Count} reviews";

    private IReadOnlyList<TravelReview> Reviews => State.ReviewsFor(Id);

    private string? RuleError => Stay is null ? null : State.ValidateStay(Stay, State.Search.CheckIn, State.Search.CheckOut, State.Search.Guests);

    /// <summary>The sign-in page, coming back to this stay.</summary>
    private string SignInHere => TravelDisplay.SignInUrl(TravelDisplay.StayUrl(Id));

    [Inject] private TravelState State { get; set; } = default!;

    /// <summary>The stay, when it is listed, or unlisted for its host (a preview) and for guests with a trip there.</summary>
    private TravelStay? Stay => State.FindStay(Id) is { } stay
        && (stay.IsListed || stay.HostId == State.CurrentHost?.Id || State.MyBookings().Any(booking => booking.StayId == stay.Id)) ? stay : null;

    /// <summary>Names the taken nights, like "The nights of Dec 8 and Dec 9 are taken."</summary>
    private string TakenText
    {
        get
        {
            if (Stay is null)
                return string.Empty;

            var nights = State.UnavailableNights(Stay.Id, State.Search.CheckIn, State.Search.CheckOut).Order().Select(TravelDisplay.Day).ToList();

            return nights.Count switch
            {
                0 => "Some of these nights are taken.",
                1 => $"The night of {nights[0]} is taken.",
                _ => $"The nights of {string.Join(", ", nights.Take(nights.Count - 1))} and {nights[^1]} are taken.",
            };
        }
    }

    #endregion
}
