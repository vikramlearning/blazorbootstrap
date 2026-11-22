using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// Checkout for a stay, for a signed-in guest: the dates and guests (from the query or the current search, editable and
/// re-validated, with the nights' availability), a message to the host (required for a request to book), the test-card
/// payment form, the price breakdown, and the cancellation policy. The card is charged with
/// <see cref="ShowcaseCards.Charge" />, then <see cref="TravelState.Book" /> confirms an instant-book stay or sends a
/// request (card held, not charged); every result gets its own message, and success opens the trip page.
/// </summary>
public partial class Book : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The most taken nights listed by date before the rest are counted.</summary>
    private const int MaxListedNights = 5;

    private const string QueryDateFormat = "yyyy-MM-dd";

    private string? bookError;

    private bool booking;

    private ShowcaseCardInput card = new();

    private DateInput<DateOnly>? checkInInput;

    /// <summary>Whether <see cref="TravelState.Book" /> just said the nights were taken (they were free when the page checked).</summary>
    private bool datesJustTaken;

    private int declines;

    private EditContext editContext = default!;

    private FocusTarget focusTarget;

    private BookingForm form = new();

    private int? loadedId;

    private string? messageError;

    private ElementReference messageInput;

    private string? paymentError;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // After a refused submit, focus moves to the first field to fix (card errors are announced by their messages).
        var target = focusTarget;
        focusTarget = FocusTarget.None;

        if (target == FocusTarget.Dates)
            await TravelDisplay.FocusAsync(checkInInput?.Element);
        else if (target == FocusTarget.Message)
            await TravelDisplay.FocusAsync(messageInput);
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        // Only a different stay starts a fresh form; the dates come from the link's query, else from the current search.
        if (loadedId == Id)
            return;

        loadedId = Id;
        bookError = null;
        paymentError = null;
        messageError = null;
        datesJustTaken = false;
        card = new ShowcaseCardInput();
        form = new BookingForm
        {
            CheckIn = ParseDate(CheckInQuery) ?? State.Search.CheckIn,
            CheckOut = ParseDate(CheckOutQuery) ?? State.Search.CheckOut,
            Guests = GuestsQuery is >= 1 and <= TravelSeedData.MaxGuests ? GuestsQuery.Value : State.Search.Guests,
        };

        if (form.CheckOut <= form.CheckIn)
            form.CheckOut = form.CheckIn.AddDays(1);

        editContext = new EditContext(form);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParseExact(value, QueryDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;

    /// <summary>Returns why the message can't be sent, or <see langword="null" />: a request to book needs one.</summary>
    private string? MessageProblem()
    {
        var length = form.Message?.Trim().Length ?? 0;

        if (Stay is { InstantBook: false } && length == 0)
            return $"Write a message to {State.HostOf(Stay).Name}: hosts read it to decide on a request.";

        return length > TravelState.MaxMessageLength ? $"Use {TravelState.MaxMessageLength} characters or fewer." : null;
    }

    /// <summary>Moving check-in keeps the same number of nights (at least one), so check-out never ends up before it.</summary>
    private void OnCheckInChanged(DateOnly value)
    {
        var nights = Math.Max(1, form.CheckOut.DayNumber - form.CheckIn.DayNumber);

        form.CheckIn = value;

        if (form.CheckOut <= value)
            form.CheckOut = value.AddDays(nights);

        OnTripChanged();
    }

    /// <summary>A check-out on or before check-in is ignored; the input's minimum is the day after check-in.</summary>
    private void OnCheckOutChanged(DateOnly value)
    {
        if (value > form.CheckIn)
            form.CheckOut = value;

        OnTripChanged();
    }

    /// <summary>A native select, so the count changes only when the guest picks one; anything outside 1 to 8 is ignored.</summary>
    private void OnGuestsChanged(ChangeEventArgs args)
    {
        if (int.TryParse(args.Value?.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var guests) && guests is >= 1 and <= TravelSeedData.MaxGuests)
            form.Guests = guests;

        OnTripChanged();
    }

    /// <summary>A shown message error is re-checked as the guest types, so it goes away once fixed.</summary>
    private void OnMessageInput()
    {
        if (messageError is not null)
            messageError = MessageProblem();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>New dates or guests: the old booking error no longer applies, and valid choices become the current search.</summary>
    private void OnTripChanged()
    {
        datesJustTaken = false;
        bookError = null;

        if (Stay is not null && RuleError is null)
        {
            // The stay page reads the search, so "Pick other dates" opens it on these dates.
            State.Search.CheckIn = form.CheckIn;
            State.Search.CheckOut = form.CheckOut;
            State.Search.Guests = form.Guests;
        }
    }

    /// <summary>
    /// Checks everything at once (dates, availability, message, card), then charges the test card and books. The state
    /// checks availability before the card, so taken nights are reported even when the card would be declined.
    /// </summary>
    private async Task Submit()
    {
        // Guards a double submit: the first successful click navigates away.
        if (booking || Stay is null)
            return;

        bookError = null;
        paymentError = null;

        // Validate() runs the card form's checks; the dates and the message are checked here.
        var cardValid = await editContext.ValidateAsync();
        messageError = MessageProblem();
        var datesValid = RuleError is null && IsAvailable;

        if (!datesValid || messageError is not null || !cardValid)
        {
            focusTarget = !datesValid ? FocusTarget.Dates : messageError is not null ? FocusTarget.Message : FocusTarget.None;
            return;
        }

        var charge = ShowcaseCards.Charge(card);
        var result = State.Book(Stay.Id, form.CheckIn, form.CheckOut, form.Guests, form.Message, charge, out var made);

        switch (result)
        {
            case TravelResult.Ok when made is not null:
                booking = true;
                card = new ShowcaseCardInput();
                Navigation.NavigateTo(TravelDisplay.TripUrl(made.Reference));
                break;

            case TravelResult.CardDeclined:
                // A different text each attempt, so the alert is announced again.
                declines++;
                var failure = charge.FailureMessage ?? "Your card was declined. Try another card.";
                paymentError = declines == 1 ? $"{failure} Nothing was booked." : $"{failure} Nothing was booked. (attempt {declines})";
                break;

            case TravelResult.Unavailable:
                datesJustTaken = true;
                bookError = "Someone else just booked some of these nights, so nothing was booked or charged. Choose other dates above, or pick them on the stay page.";
                focusTarget = FocusTarget.Dates;
                break;

            case TravelResult.Invalid:
                bookError = RuleError ?? MessageProblem() ?? "Something in the booking isn't valid, so nothing was booked or charged. Check the dates, guests, message, and card, then try again.";
                break;

            case TravelResult.NotAllowed:
                bookError = "Only a signed-in guest account can book stays. Switch to a guest account and try again.";
                break;

            case TravelResult.NotFound:
                bookError = "This stay is no longer listed, so it can't be booked. Nothing was charged.";
                break;

            default:
                bookError = "The booking couldn't be made, and nothing was charged. Try again.";
                break;
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = $"Stays in {Destination.Name}", Href = $"{TravelLayout.Base}/stays?destination={Destination.Id}" },
        new() { Text = Stay?.Name, Href = Stay is null ? null : TravelDisplay.StayUrl(Stay.Id) },
        new() { Text = "Book", IsCurrentPage = true },
    };

    /// <summary>Gets or sets the check-in date from the query (yyyy-MM-dd); the current search is used without it.</summary>
    [SupplyParameterFromQuery(Name = "checkIn")] public string? CheckInQuery { get; set; }

    /// <summary>Gets or sets the check-out date from the query (yyyy-MM-dd); the current search is used without it.</summary>
    [SupplyParameterFromQuery(Name = "checkOut")] public string? CheckOutQuery { get; set; }

    private TravelDestination Destination => State.FindDestination(Stay?.DestinationId);

    /// <summary>Gets or sets the number of guests from the query; the current search is used without it.</summary>
    [SupplyParameterFromQuery(Name = "guests")] public int? GuestsQuery { get; set; }

    private string Heading => Stay is { InstantBook: true } ? "Confirm and pay" : "Request to book";

    /// <summary>Gets or sets the stay ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    private bool IsAvailable => Stay is not null && State.IsAvailable(Stay.Id, form.CheckIn, form.CheckOut);

    private int MessageLength => form.Message?.Length ?? 0;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private TravelQuote Quote => State.Quote(Stay!, form.CheckIn, form.CheckOut);

    private string? RuleError => Stay is null ? null : State.ValidateStay(Stay, form.CheckIn, form.CheckOut, form.Guests);

    [Inject] private TravelState State { get; set; } = default!;

    /// <summary>The stay, when it is listed; an unlisted stay can't be booked.</summary>
    private TravelStay? Stay => State.FindStay(Id) is { IsListed: true } stay ? stay : null;

    /// <summary>Gets the taken nights in the chosen range, by date, for the availability message.</summary>
    private string TakenNightsText
    {
        get
        {
            var nights = State.UnavailableNights(Stay!.Id, form.CheckIn, form.CheckOut).Order().ToList();

            if (nights.Count == 0)
                return "Choose other dates.";

            var listed = string.Join(", ", nights.Take(MaxListedNights).Select(TravelDisplay.Day));
            var more = nights.Count > MaxListedNights ? $" and {nights.Count - MaxListedNights} more" : string.Empty;

            return $"Already booked or blocked: the {(nights.Count == 1 ? "night" : "nights")} of {listed}{more}. Check-out day itself doesn't count.";
        }
    }

    #endregion

    #region Nested Types

    /// <summary>Where focus goes after a refused submit.</summary>
    private enum FocusTarget
    {
        None,
        Dates,
        Message,
    }

    /// <summary>The trip being booked and the message to the host; the card has its own model.</summary>
    private sealed class BookingForm
    {
        public DateOnly CheckIn { get; set; }

        public DateOnly CheckOut { get; set; }

        public int Guests { get; set; } = 2;

        public string? Message { get; set; }
    }

    #endregion
}
