using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The customer's orders, newest first, with a filter by status group (all, in progress, delivered, cancelled or
/// refunded). Each order shows its number, date, status, items, total, and refund; "Buy again" puts its items back in the
/// cart as far as they're still sold and in stock, and "Cancel" (only before the order is packed) asks for confirmation,
/// then puts the items back into stock and refunds the payment.
/// </summary>
public partial class AccountOrders : ComponentBase, IDisposable
{
    #region Fields and Constants

    private static readonly OrderGroup[] groups = { OrderGroup.All, OrderGroup.InProgress, OrderGroup.Delivered, OrderGroup.Closed };

    private readonly Dictionary<int, Button> cancelButtons = new();

    private bool cancelled;

    private Modal cancelModal = default!;

    private EcommerceOrder? cancelling;

    private readonly Dictionary<OrderGroup, Button> filterButtons = new();

    private OrderGroup group = OrderGroup.All;

    private bool showCartLink;

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

    private static int CountIn(IReadOnlyList<EcommerceOrder> orders, OrderGroup option) => orders.Count(order => InGroup(order, option));

    private static string EmptyGroupText(OrderGroup option) => option switch
    {
        OrderGroup.InProgress => "No orders are on their way right now.",
        OrderGroup.Delivered => "None of your orders has been delivered yet.",
        OrderGroup.Closed => "None of your orders was cancelled or refunded.",
        _ => "No orders yet.",
    };

    private static string GroupLabel(OrderGroup option) => option switch
    {
        OrderGroup.InProgress => "In progress",
        OrderGroup.Delivered => "Delivered",
        OrderGroup.Closed => "Cancelled or refunded",
        _ => "All",
    };

    private static bool InGroup(EcommerceOrder order, OrderGroup option) => option switch
    {
        OrderGroup.InProgress => order.Status is EcommerceOrderStatus.Placed or EcommerceOrderStatus.Packed or EcommerceOrderStatus.Shipped,
        OrderGroup.Delivered => order.Status == EcommerceOrderStatus.Delivered,
        OrderGroup.Closed => order.Status is EcommerceOrderStatus.Cancelled or EcommerceOrderStatus.Refunded,
        _ => true,
    };

    private static string ItemCountText(EcommerceOrder order) => Items(order.Lines.Sum(line => line.Quantity));

    private static string Items(int count) => count == 1 ? "1 item" : $"{count} items";

    private static string ItemsSummary(EcommerceOrder order) =>
        string.Join(", ", order.Lines.Select(line => $"{line.Quantity} × {line.Name}"));

    private async Task AskCancelAsync(EcommerceOrder order)
    {
        cancelling = order;
        cancelled = false;

        await cancelModal.ShowAsync();
    }

    /// <summary>Puts the order's items in the cart; the button stays in place, so focus stays on it.</summary>
    private void BuyAgain(EcommerceOrder order)
    {
        var (added, skipped) = State.Reorder(order.Id);
        var skippedText = skipped == 0 ? string.Empty : $"{skipped} couldn't be added: sold out, no longer sold, or already at the most your cart can hold.";

        showCartLink = added > 0 || State.CartCount > 0;
        SetStatus(added > 0
            ? $"Added {Items(added)} from order #{order.Id} to your cart. {skippedText}".TrimEnd()
            : $"Nothing from order #{order.Id} could be added to your cart. {skippedText}".TrimEnd());
    }

    private async Task ConfirmCancelAsync()
    {
        if (cancelling is { } order)
        {
            // The store may have packed the order while the dialog was open.
            if (State.FindOrder(order.Id) is { } current && State.CanCustomerCancel(current) && State.CancelOrder(order.Id, null))
            {
                cancelled = true;
                showCartLink = false;
                SetStatus($"Order #{order.Id} is cancelled. {EcommerceDisplay.Money(current.Totals.Total)} is refunded to your card.");
            }
            else
            {
                cancelled = true;
                showCartLink = false;
                SetStatus($"Order #{order.Id} can't be cancelled any more: the store has started on it. Once it's delivered, you can return the items.");
            }
        }

        await cancelModal.HideAsync();
    }

    /// <summary>The dialog opens from code, so Bootstrap does not return focus: after a cancel the button is gone, so focus goes to the status.</summary>
    private async Task OnCancelHiddenAsync()
    {
        var order = cancelling;
        cancelling = null;

        if (cancelled)
        {
            cancelled = false;
            await EcommerceDisplay.FocusAsync(statusRegion);
        }
        else if (order is not null && cancelButtons.TryGetValue(order.Id, out var button))
        {
            await EcommerceDisplay.FocusAsync(button.Element);
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void SelectGroup(OrderGroup option) => group = option;

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : $"{message} ({statusCount})";
    }

    /// <summary>The empty group's "Show all orders" button goes away, so focus moves to the "All" filter.</summary>
    private async Task ShowAllAsync()
    {
        group = OrderGroup.All;
        StateHasChanged();

        await Task.Yield();

        if (filterButtons.TryGetValue(OrderGroup.All, out var button))
            await EcommerceDisplay.FocusAsync(button.Element);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private EcommerceState State { get; set; } = default!;

    #endregion

    #region Nested Types

    private enum OrderGroup
    {
        All,
        InProgress,
        Delivered,
        Closed,
    }

    #endregion
}
