using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Restaurant;
using BlazorBootstrap.Demo.RCL.Services.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Restaurant;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Restaurant;

/// <summary>
/// Layout of the "Saffron Table" restaurant site: the site bar, a Navbar with the cart button, the page body, one ad
/// slot, the footer, the cart drawer (<see cref="Offcanvas" />), and the toast host.
/// </summary>
public partial class RestaurantLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the restaurant site.</summary>
    public const string Base = DemoRouteConstants.Showcase_URL_Restaurant;

    private readonly ShowcaseSite site = ShowcaseCatalog.Get("restaurant");

    private Button cartButton = default!;

    private Offcanvas cartDrawer = default!;

    private bool closingForNavigation;

    private bool isCartOpen;

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

        await RestaurantDisplay.FocusAsync(cartButton.Element);
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

    /// <summary>The removed line's button is gone; focus stays inside the drawer.</summary>
    private async Task RemoveAsync(RestaurantMenuItem item)
    {
        State.Remove(item.Id);

        await Task.Yield();
        await RestaurantDisplay.FocusAsync(cartDrawer.Element);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private RestaurantState State { get; set; } = default!;

    #endregion
}
