using BlazorBootstrap.Demo.RCL.Models.Showcase.Restaurant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Restaurant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Restaurant;

/// <summary>
/// Order page: each line with a <see cref="NumberInput{TValue}" /> quantity and a remove button, and the price
/// summary. Removing a line moves focus to the heading and announces the change.
/// </summary>
public partial class Cart : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly List<BreadcrumbItem> breadcrumbItems = new()
    {
        new() { Text = "Menu", Href = RestaurantLayout.Base },
        new() { Text = "Your order", IsCurrentPage = true },
    };

    private ElementReference heading;

    private string? statusMessage;

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

    /// <summary>Quantities stay from 1 to the maximum; a cleared box (reported as 0) keeps the line at 1. Remove takes a line out.</summary>
    private void OnQuantityChanged(RestaurantMenuItem item, int value)
    {
        // NumberInput reports its value once after the first render; only a real change (or a clamp) is announced.
        var before = State.Cart.FirstOrDefault(line => line.ItemId == item.Id)?.Quantity;
        if (value == before)
            return;

        State.SetQuantity(item.Id, value);
        var quantity = State.Cart.FirstOrDefault(line => line.ItemId == item.Id)?.Quantity ?? 0;
        statusMessage = $"{item.Name}: quantity {quantity}.";
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>The line and its button disappear, so focus moves to the heading, and the status says what happened.</summary>
    private async Task RemoveAsync(RestaurantMenuItem item)
    {
        State.Remove(item.Id);
        statusMessage = $"Removed {item.Name}. {(State.CartCount == 0 ? "Your order is empty." : $"{State.CartCount} {(State.CartCount == 1 ? "item" : "items")} left.")}";
        StateHasChanged();

        await RestaurantDisplay.FocusAsync(heading);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private RestaurantState State { get; set; } = default!;

    #endregion
}
