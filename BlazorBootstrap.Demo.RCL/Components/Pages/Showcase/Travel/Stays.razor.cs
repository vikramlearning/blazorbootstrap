using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// Results: stays in the searched destination that fit the guests, filtered by price, type, and free cancellation,
/// beside a map (or its placeholder) whose numbered pins match the numbered results. Filters live in the site state.
/// </summary>
public partial class Stays : ComponentBase
{
    #region Fields and Constants

    private ElementReference resultsCount;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>A destination in the query (from the destination tiles) becomes the searched destination; unknown IDs are ignored.</summary>
    protected override void OnParametersSet()
    {
        if (DestinationQuery is not null && State.Destinations.Any(destination => destination.Id == DestinationQuery))
            State.Search.DestinationId = DestinationQuery;
    }

    #endregion

    #region Private Methods

    /// <summary>The button disables itself, so focus moves to the result count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        State.Filter.Clear();
        StateHasChanged();

        await TravelDisplay.FocusAsync(resultsCount);
    }

    private void OnFreeCancellationChanged(bool value) => State.Filter.FreeCancellationOnly = value;

    /// <summary>The range input reports its value on first render too; only a real change updates the filter.</summary>
    private void OnMaxPriceChanged(int value)
    {
        if (State.Filter.MaxPrice != value)
            State.Filter.MaxPrice = value;
    }

    private void OnSortChanged(TravelSort sort) => State.Filter.Sort = sort;

    private void OnTypeChanged(string type) => State.Filter.Type = type;

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the destination ID from the query string.</summary>
    [SupplyParameterFromQuery(Name = "destination")] public string? DestinationQuery { get; set; }

    private TravelDestination Destination => State.FindDestination(State.Search.DestinationId);

    private IReadOnlyList<ShowcaseLocation> Locations => Results.Select(stay => TravelDisplay.Location(stay, State.AverageRating(stay.Id))).ToList();

    private int Nights => Math.Max(0, State.Search.CheckOut.DayNumber - State.Search.CheckIn.DayNumber);

    private IReadOnlyList<TravelStay> Results => State.Results();

    private string ResultsText => Results.Count switch
    {
        0 => "No stays found",
        1 => "1 stay",
        var count => $"{count} stays",
    };

    private string? SearchError => TravelState.ValidateSearch(State.Search.CheckIn, State.Search.CheckOut, State.Search.Guests);

    [Inject] private TravelState State { get; set; } = default!;

    #endregion
}
