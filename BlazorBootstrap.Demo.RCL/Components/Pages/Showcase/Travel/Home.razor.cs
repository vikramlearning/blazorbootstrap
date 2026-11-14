using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// Search: destination, dates, and guests, then the results. The form starts from the current search, so going back
/// shows what was searched last. Destination tiles link to the results for that city with the current dates.
/// </summary>
public partial class Home : ComponentBase
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
    }

    #endregion

    #region Private Methods

    private void Search()
    {
        ruleError = TravelState.ValidateSearch(form.CheckIn, form.CheckOut, form.Guests);

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

    private TravelBooking? NextTrip => State.Bookings
        .Where(booking => booking.Status == TravelBookingStatus.Upcoming)
        .OrderBy(booking => booking.CheckIn)
        .FirstOrDefault();

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
