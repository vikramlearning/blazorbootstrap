using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// A heart toggle that saves a stay to the signed-in guest's wishlist or removes it. The accessible name says what a
/// press does ("Save Azulejo Loft to your wishlist" / "Remove Azulejo Loft from your wishlist"), and a visually hidden
/// status announces the result. Signed out, it is a link to the sign-in page that comes back to this page; accounts in
/// other roles see nothing. It follows wishlist changes made elsewhere.
/// </summary>
public partial class TravelWishButton : ComponentBase, IDisposable
{
    #region Fields and Constants

    private int statusCount;

    private string? statusMessage;

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

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : $"{message} ({statusCount})";
    }

    /// <summary>Returns the message for the outcome of a toggle.</summary>
    private string StatusFor(TravelResult result, bool isWished) => result switch
    {
        TravelResult.Ok when isWished => $"Saved {Stay.Name} to your wishlist.",
        TravelResult.Ok => $"Removed {Stay.Name} from your wishlist.",
        TravelResult.NotAllowed => "Sign in as a guest to save stays to a wishlist.",
        TravelResult.NotFound => $"{Stay.Name} is no longer listed, so it can't be saved.",
        TravelResult.Invalid => $"Your wishlist is full ({TravelState.MaxWishlist} stays). Remove a stay to save {Stay.Name}.",
        _ => "Your wishlist didn't change. Try again.",
    };

    private async Task ToggleAsync()
    {
        var result = State.ToggleWish(Stay.Id);
        var isWished = State.IsWished(Stay.Id);

        SetStatus(StatusFor(result, isWished));

        if (result == TravelResult.Ok)
            await OnToggled.InvokeAsync(isWished);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string AccessibleName => IsWished
        ? $"Remove {Stay.Name} from your wishlist"
        : $"Save {Stay.Name} to your wishlist";

    /// <summary>
    /// Gets or sets whether the button announces its own result in a status region. A page that removes the button with
    /// the stay (the wishlist) turns it off and announces the change itself. Default value is <see langword="true" />.
    /// </summary>
    [Parameter] public bool Announce { get; set; } = true;

    /// <summary>Gets or sets extra CSS classes for the wrapper.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Gets or sets whether the button is a small round icon-only button (for cards); otherwise it is a pill button with
    /// a word next to the heart (for the stay page). Default value is <see langword="false" />.
    /// </summary>
    [Parameter] public bool Compact { get; set; }

    private bool IsWished => State.IsWished(Stay.Id);

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>Gets or sets a callback raised after the stay was saved (<see langword="true" />) or removed (<see langword="false" />).</summary>
    [Parameter] public EventCallback<bool> OnToggled { get; set; }

    /// <summary>The sign-in page, coming back to this page (path and query) afterwards.</summary>
    private string SignInHref => TravelDisplay.SignInUrl("/" + Navigation.ToBaseRelativePath(Navigation.Uri).Split('#')[0]);

    /// <summary>Starts with the visible word "Save" (WCAG 2.5.3) and says that signing in comes first.</summary>
    private string SignInName => $"Save {Stay.Name} to your wishlist: sign in first";

    [Inject] private TravelState State { get; set; } = default!;

    /// <summary>Gets or sets the stay to save or remove.</summary>
    [Parameter, EditorRequired] public TravelStay Stay { get; set; } = default!;

    #endregion
}
