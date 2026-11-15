using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The retailer's dashboard for the last 30 days: net revenue, orders to fulfil, average order value, stock alerts, the
/// most recent orders, and the best sellers. Every figure is derived from the store's state, so an order a customer
/// places, or a refund in the admin, shows up here.
/// </summary>
public partial class Admin : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Products with this many or fewer in stock are flagged.</summary>
    private const int LowStockLimit = 5;

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

    #endregion

    #endregion

    #region Properties, Indexers

    private IReadOnlyList<(string Name, int Units)> BestSellers => RecentPaid
        .SelectMany(order => order.Lines)
        .GroupBy(line => line.Name)
        .Select(group => (Name: group.Key, Units: group.Sum(line => line.Quantity)))
        .OrderByDescending(entry => entry.Units)
        .ThenBy(entry => entry.Name)
        .Take(5)
        .ToList();

    private string ItemsSoldText
    {
        get
        {
            var items = RecentPaid.Sum(order => order.Totals.ItemCount);

            return items == 1 ? "1 item sold" : $"{items} items sold";
        }
    }

    private string OrderCountText => RecentPaid.Count == 1 ? "1 order" : $"{RecentPaid.Count} orders";

    private int OutOfStockCount => StockAlerts.Count(product => product.Stock == 0);

    /// <summary>Gets the last 30 days' orders that weren't cancelled.</summary>
    private IReadOnlyList<EcommerceOrder> RecentPaid => State.Orders
        .Where(order => order.Status != EcommerceOrderStatus.Cancelled && order.PlacedOn >= State.Clock.Today.AddDays(-29))
        .ToList();

    private IReadOnlyList<EcommerceOrder> RecentOrders => State.Orders.OrderByDescending(order => order.PlacedOn).Take(6).ToList();

    private decimal Revenue => RecentPaid.Sum(order => order.Totals.Total - order.RefundedAmount);

    [Inject] private EcommerceState State { get; set; } = default!;

    private IReadOnlyList<EcommerceProduct> StockAlerts => State.Products
        .Where(product => product.Stock <= LowStockLimit)
        .OrderBy(product => product.Stock)
        .ThenBy(product => product.Name)
        .ToList();

    private int ToFulfil => State.Orders.Count(order => order.Status is EcommerceOrderStatus.Placed or EcommerceOrderStatus.Packed);

    #endregion
}
