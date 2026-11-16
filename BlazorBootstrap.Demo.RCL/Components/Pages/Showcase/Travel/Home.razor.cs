using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// Search: destination, dates, and guests, then the results. The form starts from the current search, so going back
/// shows what was searched last. Destination tiles link to the results for that city with the current dates. A signed-in
/// guest sees their next trip, requests waiting for a host, and unread messages; a signed-out visitor is invited to sign
/// in or register.
/// </summary>
public partial class Home : ComponentBase, IDisposable
{
    #region Fields and Constants

    private EditContext editContext = default!;

    private SearchForm form = new();

    private string? ruleError;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        form = new SearchForm
        {
            DestinationId = State.Search.DestinationId,
            CheckIn = State.Search.CheckIn,
            CheckOut = State.Search.CheckOut,
            Guests = State.Search.Guests,
        };
        editContext = new EditContext(form);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void Search()
    {
        ruleError = State.ValidateSearch(form.CheckIn, form.CheckOut, form.Guests);

        if (ruleError is not null)
            return;

        State.Search.DestinationId = State.FindDestination(form.DestinationId).Id;
        State.Search.CheckIn = form.CheckIn;
        State.Search.CheckOut = form.CheckOut;
        State.Search.Guests = form.Guests;

        Navigation.NavigateTo($"{TravelLayout.Base}/stays");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>The signed-in guest's next confirmed trip, if any.</summary>
    private TravelBooking? NextTrip => State.MyBookings()
        .Where(booking => State.Phase(booking) is TravelTripPhase.Upcoming or TravelTripPhase.InProgress)
        .OrderBy(booking => booking.CheckIn)
        .FirstOrDefault();

    /// <summary>The signed-in guest's requests to book that are waiting for the host.</summary>
    private int PendingRequestCount => State.MyBookings().Count(booking => State.Phase(booking) == TravelTripPhase.Requested);

    [CascadingParameter] private ShowcaseSession Session { get; set; } = default!;

    [Inject] private TravelState State { get; set; } = default!;

    #endregion

    #region Nested Types

    private sealed class SearchForm
    {
        public DateOnly CheckIn { get; set; }

        public DateOnly CheckOut { get; set; }

        public string DestinationId { get; set; } = string.Empty;

        [Range(1, TravelSeedData.MaxGuests, ErrorMessage = "Choose from 1 to 8 guests.")]
        public int Guests { get; set; }
    }

    #endregion
}
