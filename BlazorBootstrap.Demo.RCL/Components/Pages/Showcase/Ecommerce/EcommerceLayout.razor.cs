using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// Layout of the "Kindred Goods" storefront: announcement bar, Navbar with the cart button, the cart drawer, toasts, and footer.
/// </summary>
public partial class EcommerceLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the storefront.</summary>
    public const string Base = DemoRouteConstants.Showcase_URL_Ecommerce;

    /// <summary>The storefront's sign-in page.</summary>
    public const string SignInPath = Base + "/sign-in";

    private Button cartButton = default!;

    private Offcanvas cartDrawer = default!;

    private bool closingForNavigation;

    private bool isCartOpen;

    private readonly ShowcaseSite site = ShowcaseCatalog.Get("ecommerce");

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;
        Navigation.LocationChanged += OnLocationChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and navigation changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        Navigation.LocationChanged -= OnLocationChanged;
    }

    #endregion

    #region Private Methods

    /// <summary>The drawer opens from code, so focus goes back to the cart button when it closes, unless a link in it navigated away.</summary>
    private async Task OnCartHiddenAsync()
    {
        isCartOpen = false;

        if (closingForNavigation)
        {
            closingForNavigation = false;
            return;
        }

        await EcommerceDisplay.FocusAsync(cartButton.Element);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        if (!isCartOpen)
            return;

        closingForNavigation = true;
        _ = InvokeAsync(cartDrawer.HideAsync);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenCartAsync()
    {
        isCartOpen = true;
        await cartDrawer.ShowAsync();
    }

    private async Task RemoveLineAsync(EcommerceCartLine line)
    {
        State.SetQuantity(line.ProductId, line.Color, 0);

        // The removed line's button is gone; keep keyboard users inside the drawer.
        await Task.Yield();
        await EcommerceDisplay.FocusAsync(cartDrawer.Element);
    }

    private static string ShippingNote(EcommerceTotals totals)
    {
        var remaining = EcommerceSeedData.FreeShippingThreshold - (totals.Subtotal - totals.Discount);

        return remaining > 0
            ? $"Add {EcommerceDisplay.Money(remaining)} more for free standard shipping. Tax is calculated at checkout."
            : "You've unlocked free standard shipping. Tax is calculated at checkout.";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string CartLabel => State.CartCount switch
    {
        0 => "Cart, empty",
        1 => "Cart, 1 item",
        var count => $"Cart, {count} items",
    };

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private EcommerceState State { get; set; } = default!;

    #endregion
}
