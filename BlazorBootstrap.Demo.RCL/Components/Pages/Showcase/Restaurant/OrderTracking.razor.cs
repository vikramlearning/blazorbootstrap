using BlazorBootstrap.Demo.RCL.Models.Showcase.Restaurant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Restaurant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Restaurant;

/// <summary>
/// Order tracking: the status, a <see cref="Progress" /> bar with the step in words, the step list with times, a demo
/// "simulate the next update" control, and the order details. An unknown number renders the site's not-found screen.
/// </summary>
public partial class OrderTracking : ComponentBase, IDisposable
{
    #region Fields and Constants

    private int? loadedId;

    private string? statusMessage;

    private ElementReference statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        // Another order starts without the previous order's status text.
        if (loadedId == Id)
            return;

        loadedId = Id;
        statusMessage = null;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static IconName StepIcon(RestaurantOrderStatus status) => status switch
    {
        RestaurantOrderStatus.Received => IconName.Receipt,
        RestaurantOrderStatus.Preparing => IconName.Fire,
        RestaurantOrderStatus.OnTheWay => IconName.Truck,
        _ => IconName.HouseCheck,
    };

    /// <summary>
    /// Moves the order on one step. On the last step the button is replaced by "Order again", so focus moves to the
    /// status message, which says what happened.
    /// </summary>
    private async Task AdvanceAsync()
    {
        if (Order is null)
            return;

        State.Advance(Order);
        statusMessage = $"Update: {RestaurantDisplay.StatusLabel(Order.Status, Order.Fulfillment)} at {RestaurantDisplay.Time(Order.StatusTimes[Order.Status])}.";

        if (Order.Status == RestaurantOrderStatus.Completed)
        {
            StateHasChanged();
            await RestaurantDisplay.FocusAsync(statusRegion);
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Menu", Href = RestaurantLayout.Base },
        new() { Text = Order is null ? "Order" : $"Order #{Order.Id}", IsCurrentPage = true },
    };

    /// <summary>Gets or sets the order number from the route.</summary>
    [Parameter] public int Id { get; set; }

    private RestaurantOrder? Order => State.FindOrder(Id);

    [Inject] private RestaurantState State { get; set; } = default!;

    #endregion
}
