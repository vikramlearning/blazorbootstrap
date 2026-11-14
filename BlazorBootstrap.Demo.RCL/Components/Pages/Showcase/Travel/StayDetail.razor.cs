using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// Stay page: photo gallery, rating, description, amenities, a map of the stay, reviews, and a price box whose dates
/// and guests are the current search. An unknown ID renders the site's not-found screen.
/// </summary>
public partial class StayDetail : ComponentBase
{
    #region Methods

    #region Private Methods

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

    /// <summary>An empty or out-of-range guest count is ignored, so the shared search always has 1 to 8 guests.</summary>
    private void OnGuestsChanged(int value)
    {
        if (value is >= 1 and <= TravelSeedData.MaxGuests)
            State.Search.Guests = value;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Rounded to one decimal; the library draws it to the nearest half star.</summary>
    private decimal AverageRating => Stay is null ? 0 : Math.Round((decimal)State.AverageRating(Stay.Id), 1, MidpointRounding.AwayFromZero);

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Search", Href = TravelLayout.Base },
        new() { Text = $"Stays in {Destination.Name}", Href = $"{TravelLayout.Base}/stays?destination={Destination.Id}" },
        new() { Text = Stay?.Name, IsCurrentPage = true },
    };

    private TravelDestination Destination => State.FindDestination(Stay?.DestinationId);

    /// <summary>Gets or sets the stay ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    private IReadOnlyList<ShowcaseLocation> Location => Stay is null
        ? Array.Empty<ShowcaseLocation>()
        : new[] { TravelDisplay.Location(Stay, State.AverageRating(Stay.Id)) };

    private TravelQuote Quote => State.Quote(Stay!, State.Search.CheckIn, State.Search.CheckOut);

    private IReadOnlyList<TravelReview> Reviews => State.ReviewsFor(Id);

    private string? RuleError => Stay is null ? null : TravelState.ValidateStay(Stay, State.Search.CheckIn, State.Search.CheckOut, State.Search.Guests);

    [Inject] private TravelState State { get; set; } = default!;

    private TravelStay? Stay => State.FindStay(Id);

    #endregion
}
