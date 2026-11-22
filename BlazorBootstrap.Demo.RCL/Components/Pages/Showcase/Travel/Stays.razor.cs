using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// Results: stays in the searched destination that fit the guests and are free on the searched dates, filtered by price,
/// type, free cancellation, and instant book, beside a map (or its placeholder) whose numbered pins match the numbered
/// results. Each card says whether the stay books instantly or on request, names its cancellation policy, and has the
/// wishlist heart. Stays that match but are booked on the dates are listed below, with a link to pick other dates.
/// Filters live in the site state.
/// </summary>
public partial class Stays : ComponentBase, IDisposable
{
    #region Fields and Constants

    private ElementReference resultsCount;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    /// <summary>A destination in the query (from the destination tiles) becomes the searched destination; unknown IDs are ignored.</summary>
    protected override void OnParametersSet()
    {
        if (DestinationQuery is not null && State.Destinations.Any(destination => destination.Id == DestinationQuery))
            State.Search.DestinationId = DestinationQuery;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Returns a policy's name, like "Moderate cancellation".</summary>
    private static string PolicyName(TravelCancellationPolicy policy) => $"{policy} cancellation";

    private static string StaysText(int count) => count == 1 ? "1 stay" : $"{count} stays";

    /// <summary>The button disables itself, so focus moves to the result count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        State.Filter.Clear();

        await TravelDisplay.FocusAsync(resultsCount);
    }

    /// <summary>The library Switch can report its value without a change, so only a real change updates the filter.</summary>
    private void OnFreeCancellationChanged(bool value)
    {
        if (State.Filter.FreeCancellationOnly != value)
            State.Filter.FreeCancellationOnly = value;
    }

    /// <summary>The library Switch can report its value without a change, so only a real change updates the filter.</summary>
    private void OnInstantBookChanged(bool value)
    {
        if (State.Filter.InstantBookOnly != value)
            State.Filter.InstantBookOnly = value;
    }

    /// <summary>The range input reports its value on first render too; only a real change updates the filter.</summary>
    private void OnMaxPriceChanged(int value)
    {
        if (State.Filter.MaxPrice != value)
            State.Filter.MaxPrice = value;
    }

    private void OnSortChanged(TravelSort sort) => State.Filter.Sort = sort;

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void OnTypeChanged(string type) => State.Filter.Type = type;

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>The stays that match but are booked, requested, or blocked on some of the searched nights.</summary>
    private IReadOnlyList<TravelStay> Booked => State.UnavailableResults();

    /// <summary>Gets or sets the destination ID from the query string.</summary>
    [SupplyParameterFromQuery(Name = "destination")] public string? DestinationQuery { get; set; }

    private TravelDestination Destination => State.FindDestination(State.Search.DestinationId);

    /// <summary>
    /// The numbered results: the stays free on the dates. Without valid dates availability can't be checked, so every
    /// matching stay is listed.
    /// </summary>
    private IReadOnlyList<TravelStay> Listed => SearchError is null ? State.Results() : State.Results().Concat(State.UnavailableResults()).ToList();

    private IReadOnlyList<ShowcaseLocation> Locations => Listed.Select(stay => TravelDisplay.Location(stay, State.AverageRating(stay.Id))).ToList();

    private int Nights => Math.Max(0, State.Search.CheckOut.DayNumber - State.Search.CheckIn.DayNumber);

    /// <summary>The result count, including how many matching stays are booked on the dates.</summary>
    private string ResultsText
    {
        get
        {
            if (SearchError is not null)
                return Listed.Count == 0 ? "No stays match" : $"{StaysText(Listed.Count)} match. Fix the dates to check availability.";

            var listed = Listed.Count;
            var booked = Booked.Count;
            var available = listed == 0 ? "No stays available on your dates" : $"{StaysText(listed)} available on your dates";

            if (booked == 0)
                return available;

            var bookedText = $"{StaysText(booked)} {(booked == 1 ? "is" : "are")} booked on those dates";

            return listed == 0 ? $"{available}: {bookedText}" : $"{available}; {(booked == 1 ? "1 more" : $"{booked} more")} {(booked == 1 ? "is" : "are")} booked on those dates";
        }
    }

    private string? SearchError => State.ValidateSearch(State.Search.CheckIn, State.Search.CheckOut, State.Search.Guests);

    [Inject] private TravelState State { get; set; } = default!;

    #endregion
}
