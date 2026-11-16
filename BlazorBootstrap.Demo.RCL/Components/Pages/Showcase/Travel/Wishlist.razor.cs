using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// The signed-in guest's saved stays, the most recently saved first: each with its photo, name, destination, rating,
/// booking type (instant or on request), cancellation policy, nightly rate, and the heart to remove it. A removal is
/// announced in a status message with "Undo", and focus moves there because the card is gone. With nothing saved, the
/// page links to the search.
/// </summary>
public partial class Wishlist : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The stay removed last, which "Undo" saves again.</summary>
    private TravelStay? removed;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

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

    private static string SavedText(int count) => count switch
    {
        0 => "Stays you save appear here.",
        1 => "1 stay saved. Prices are for one night; open a stay to check your dates.",
        _ => $"{count} stays saved. Prices are for one night; open a stay to check your dates.",
    };

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>The removed card and its heart are gone, so focus moves to the status message, which offers "Undo".</summary>
    private async Task OnToggledAsync(TravelStay stay, bool isWished)
    {
        if (isWished)
            return;

        removed = stay;
        SetStatus($"Removed {stay.Name} from your wishlist.");
        StateHasChanged();

        await Task.Yield();
        await TravelDisplay.FocusAsync(statusRegion);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : $"{message} ({statusCount})";
    }

    /// <summary>Saves the removed stay again; the Undo button goes away, so focus stays on the status message.</summary>
    private async Task UndoAsync()
    {
        if (removed is not { } stay)
            return;

        removed = null;
        SetStatus(State.ToggleWish(stay.Id) switch
        {
            TravelResult.Ok => $"Saved {stay.Name} to your wishlist again.",
            TravelResult.Invalid => $"Your wishlist is full ({TravelState.MaxWishlist} stays), so {stay.Name} couldn't be saved again.",
            TravelResult.NotFound => $"{stay.Name} is no longer listed, so it couldn't be saved again.",
            _ => "Sign in as a guest to save stays.",
        });
        StateHasChanged();

        await Task.Yield();
        await TravelDisplay.FocusAsync(statusRegion);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private TravelState State { get; set; } = default!;

    #endregion
}
