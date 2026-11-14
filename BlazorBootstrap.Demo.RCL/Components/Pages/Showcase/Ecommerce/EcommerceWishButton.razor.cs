using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// A heart toggle that saves a product to the signed-in customer's wishlist or removes it (<c>aria-pressed</c> tells the
/// state, the accessible name the action), with a visually hidden status that announces the change. Signed out, it opens
/// the sign-in page and comes back to this page; staff see nothing. It follows wishlist changes made elsewhere.
/// </summary>
public partial class EcommerceWishButton : ComponentBase, IDisposable
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

    private void Toggle()
    {
        if (State.Identity.Current is null)
        {
            // Back to this page after signing in; the sign-in page only follows a return address inside the store.
            Navigation.NavigateTo($"{EcommerceLayout.SignInPath}?returnUrl={Uri.EscapeDataString("/" + Navigation.ToBaseRelativePath(Navigation.Uri))}");
            return;
        }

        var wasWished = State.IsWished(Product.Id);
        State.ToggleWish(Product.Id);
        var isWished = State.IsWished(Product.Id);

        if (isWished == wasWished)
            SetStatus(wasWished ? "Couldn't remove it from your wishlist" : "Couldn't save it: your wishlist is full");
        else
            SetStatus(isWished ? "Saved to your wishlist" : "Removed from your wishlist");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string AccessibleName => IsWished
        ? $"Remove {Product.Name} from your wishlist"
        : $"Save {Product.Name} to your wishlist";

    /// <summary>Gets or sets extra CSS classes for the wrapper, such as <c>ec-wish-corner</c> to place it over a card's image.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Gets or sets whether the button is a small round icon-only button (for product cards); otherwise it is a large
    /// pill button with a word next to the heart (for the product page). Default value is <see langword="false" />.
    /// </summary>
    [Parameter] public bool Compact { get; set; }

    private bool IsWished => State.IsWished(Product.Id);

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>Gets or sets the product to save or remove.</summary>
    [Parameter, EditorRequired] public EcommerceProduct Product { get; set; } = default!;

    [Inject] private EcommerceState State { get; set; } = default!;

    #endregion
}
