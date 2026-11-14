using BlazorBootstrap.Demo.RCL.Models.Showcase.Restaurant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Restaurant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Restaurant;

/// <summary>
/// Menu: search, a vegetarian filter, and category choices (kept in the site state), and item cards that add one to
/// the order. Adding announces itself in a status message and a toast.
/// </summary>
public partial class Menu : ComponentBase, IDisposable
{
    #region Fields and Constants

    private string? addedMessage;

    private ElementReference countRegion;

    /// <summary>The value bound to the search box; it trails the filter while the user types.</summary>
    private string searchBoxValue = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        searchBoxValue = State.Filter.Search;
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void Add(RestaurantMenuItem item)
    {
        var added = State.AddToCart(item.Id, 1);
        var inCart = State.Cart.FirstOrDefault(line => line.ItemId == item.Id)?.Quantity ?? 0;

        addedMessage = added > 0
            ? $"Added {item.Name}. You have {inCart} in your order."
            : $"Your order already has the most we can make of {item.Name} ({RestaurantSeedData.MaxQuantity}).";

        Toasts.Notify(added > 0
            ? new ToastMessage(ToastType.Success, IconName.Basket2, "Added to your order", addedMessage)
            : new ToastMessage(ToastType.Warning, IconName.ExclamationTriangle, "Limit reached", addedMessage));
    }

    /// <summary>The button disables itself, so focus moves to the item count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        State.Filter.Clear();
        searchBoxValue = string.Empty;
        StateHasChanged();

        await RestaurantDisplay.FocusAsync(countRegion);
    }

    private void OnCategoryChanged(string categoryId) => State.Filter.CategoryId = categoryId;

    private void OnSearchChanged(string value)
    {
        searchBoxValue = value;
        State.Filter.Search = value;
    }

    private void OnSearchInput(ChangeEventArgs args) => State.Filter.Search = args.Value?.ToString() ?? string.Empty;

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void OnVegetarianChanged(bool value) => State.Filter.VegetarianOnly = value;

    #endregion

    #endregion

    #region Properties, Indexers

    private string CountText => Items.Count switch
    {
        0 => "No dishes found",
        1 => "1 dish",
        var count => $"{count} dishes",
    };

    private IReadOnlyList<RestaurantMenuItem> Items => State.FilteredMenu();

    [Inject] private RestaurantState State { get; set; } = default!;

    [Inject] private ToastService Toasts { get; set; } = default!;

    #endregion
}
