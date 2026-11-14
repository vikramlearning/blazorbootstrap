using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// Booking form: dates and guests (starting from the search), the guest's details, and the house rules, beside a live
/// price summary. A valid booking is added to the trips, and the trips page confirms it. No payment details are asked.
/// </summary>
public partial class Book : ComponentBase
{
    #region Fields and Constants

    private EditContext editContext = default!;

    private BookingForm form = new();

    private int? loadedId;

    private string? ruleError;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnParametersSet()
    {
        // Only a different stay starts a fresh form.
        if (loadedId == Id)
            return;

        loadedId = Id;
        ruleError = null;
        form = new BookingForm
        {
            CheckIn = State.Search.CheckIn,
            CheckOut = State.Search.CheckOut,
            Guests = Stay is null ? State.Search.Guests : Math.Clamp(State.Search.Guests, 1, Stay.MaxGuests),
        };
        editContext = new EditContext(form);
    }

    #endregion

    #region Private Methods

    private void Submit()
    {
        if (Stay is null)
            return;

        ruleError = TravelState.ValidateStay(Stay, form.CheckIn, form.CheckOut, form.Guests);

        if (ruleError is not null)
            return;

        State.Search.CheckIn = form.CheckIn;
        State.Search.CheckOut = form.CheckOut;
        State.Search.Guests = form.Guests;
        State.Book(Stay, form.CheckIn, form.CheckOut, form.Guests, form.FullName);

        Navigation.NavigateTo($"{TravelLayout.Base}/trips");
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

    private TravelDestination Destination => State.FindDestination(Stay?.DestinationId);

    /// <summary>Gets or sets the stay ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private TravelQuote Quote => State.Quote(Stay!, form.CheckIn, form.CheckOut);

    [Inject] private TravelState State { get; set; } = default!;

    private TravelStay? Stay => State.FindStay(Id);

    #endregion

    #region Nested Types

    private sealed class BookingForm
    {
        [Range(typeof(bool), "true", "true", ErrorMessage = "Agree to the house rules to book.")]
        public bool AcceptsRules { get; set; }

        [Required(ErrorMessage = "Choose when you expect to arrive.")]
        public string Arrival { get; set; } = string.Empty;

        public DateOnly CheckIn { get; set; }

        public DateOnly CheckOut { get; set; }

        [Required(ErrorMessage = "Enter an email address.")]
        [EmailAddress(ErrorMessage = "Enter an email address like name@example.com.")]
        [StringLength(80, ErrorMessage = "Use 80 characters or fewer.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the name for the booking.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "Use 2 to 60 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Range(1, TravelSeedData.MaxGuests, ErrorMessage = "Choose at least 1 guest.")]
        public int Guests { get; set; }

        [StringLength(300, ErrorMessage = "Use 300 characters or fewer.")]
        public string? Note { get; set; }
    }

    #endregion
}
