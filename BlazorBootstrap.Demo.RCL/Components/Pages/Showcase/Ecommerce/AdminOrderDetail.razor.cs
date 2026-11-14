using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// One order in the retailer's back office: its items, totals, customer, delivery, payment, tracking number, history, and
/// return requests, with the actions its status allows. The next fulfilment step (pack, ship with a tracking number, mark
/// delivered), cancelling before it ships (the items go back into stock), and refunding after it ships (stock is not
/// returned) each go through <see cref="EcommerceState" />; cancel and refund ask for confirmation in a <see cref="Modal" />.
/// </summary>
public partial class AdminOrderDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private Button? cancelButton;

    private EditContext cancelContext = default!;

    private bool cancelled;

    private CancelForm cancelForm = new();

    private Modal cancelModal = default!;

    private string? errorMessage;

    private string? lastAnnouncement;

    private Button? refundButton;

    private EditContext refundContext = default!;

    private bool refunded;

    private RefundForm refundForm = new();

    private Modal refundModal = default!;

    private int repeatCount;

    private Button? shipButton;

    private EditContext shipContext = default!;

    private bool shipped;

    private ShipForm shipForm = new();

    private Modal shipModal = default!;

    private int? shownId;

    private string? statusMessage;

    private ElementReference statusRegion;

    private TextInput? trackingInput;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        shipContext = new EditContext(shipForm);
        cancelContext = new EditContext(cancelForm);
        refundContext = new EditContext(refundForm);
        State.Changed += OnStateChanged;
    }

    /// <summary>Clears the messages when the page moves to another order (the component is reused).</summary>
    protected override void OnParametersSet()
    {
        if (shownId == Id)
            return;

        shownId = Id;
        statusMessage = null;
        errorMessage = null;
        lastAnnouncement = null;
        repeatCount = 0;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>A shipped or delivered order can be refunded while some of its total hasn't been refunded through returns.</summary>
    private static bool CanRefund(EcommerceOrder order) =>
        order.Status is EcommerceOrderStatus.Shipped or EcommerceOrderStatus.Delivered && RefundDue(order) > 0;

    private static string DiscountLabel(EcommerceOrder order) =>
        string.IsNullOrEmpty(order.PromoCode) ? "Discount" : $"Discount ({order.PromoCode})";

    private static bool HasActions(EcommerceOrder order) =>
        EcommerceState.NextStatus(order.Status) is not null || order.Status is EcommerceOrderStatus.Placed or EcommerceOrderStatus.Packed || CanRefund(order);

    private static string ItemsText(int count) => count == 1 ? "1 item" : $"{count} items";

    private static string NoActionsText(EcommerceOrder order) => order.Status switch
    {
        EcommerceOrderStatus.Cancelled => "This order was cancelled and its items went back into stock. There's nothing left to do.",
        EcommerceOrderStatus.Refunded => "This order was refunded in full. There's nothing left to do.",
        _ => "This order was delivered and its total was refunded through returns. There's nothing left to do.",
    };

    /// <summary>Renders a history note as encoded text, or nothing when the step has no note.</summary>
    private static RenderFragment? NoteContent(string? note) =>
        string.IsNullOrWhiteSpace(note) ? null : builder => builder.AddContent(0, note);

    private static decimal RefundDue(EcommerceOrder order) => Math.Max(0m, order.Totals.Total - order.RefundedAmount);

    private static BadgeColor ReturnColor(EcommerceReturnStatus status) => status switch
    {
        EcommerceReturnStatus.Requested => BadgeColor.Warning,
        EcommerceReturnStatus.Approved => BadgeColor.Success,
        _ => BadgeColor.Secondary,
    };

    private static TimelineColor TimelineColorFor(EcommerceOrderStatus status) => status switch
    {
        EcommerceOrderStatus.Placed => TimelineColor.Warning,
        EcommerceOrderStatus.Packed => TimelineColor.Info,
        EcommerceOrderStatus.Shipped => TimelineColor.Primary,
        EcommerceOrderStatus.Delivered => TimelineColor.Success,
        EcommerceOrderStatus.Refunded => TimelineColor.Secondary,
        _ => TimelineColor.Danger,
    };

    /// <summary>Marks the order packed or delivered. The clicked button is replaced, so focus moves to the status message.</summary>
    private async Task AdvanceAsync(EcommerceOrderStatus expected)
    {
        errorMessage = null;

        if (State.FindOrder(Id)?.Status != expected || !State.AdvanceOrder(Id))
        {
            errorMessage = $"Order #{Id} couldn't be updated: its status changed. The page now shows where it is.";
            await EcommerceDisplay.FocusAsync(statusRegion);

            return;
        }

        Announce(expected == EcommerceOrderStatus.Placed ? $"Order #{Id} marked packed." : $"Order #{Id} marked delivered.");
        await EcommerceDisplay.FocusAsync(statusRegion);
    }

    /// <summary>Sets the status message; a repeat of the same message gets a counter, so screen readers announce it again.</summary>
    private void Announce(string message)
    {
        repeatCount = message == lastAnnouncement ? repeatCount + 1 : 1;
        lastAnnouncement = message;
        statusMessage = repeatCount > 1 ? $"{message} ({repeatCount})" : message;
    }

    private async Task ConfirmCancelAsync()
    {
        errorMessage = null;

        if (State.CancelOrder(Id, cancelForm.Reason))
        {
            cancelled = true;
            Announce($"Order #{Id} cancelled. Its items are back in stock.");
        }
        else
        {
            errorMessage = $"Order #{Id} couldn't be cancelled: it has already shipped or closed.";
        }

        await cancelModal.HideAsync();
    }

    private async Task ConfirmRefundAsync()
    {
        errorMessage = null;

        var amount = State.FindOrder(Id) is { } order ? RefundDue(order) : 0m;

        if (State.RefundOrder(Id, refundForm.Reason))
        {
            refunded = true;
            Announce($"Order #{Id} refunded: {EcommerceDisplay.Money(amount)}.");
        }
        else
        {
            errorMessage = $"Order #{Id} couldn't be refunded: it hasn't shipped, or it is already closed.";
        }

        await refundModal.HideAsync();
    }

    private async Task ConfirmShipAsync()
    {
        var tracking = shipForm.TrackingNumber;

        errorMessage = null;

        if (State.FindOrder(Id)?.Status == EcommerceOrderStatus.Packed && State.AdvanceOrder(Id, tracking))
        {
            shipped = true;
            Announce($"Order #{Id} shipped with tracking number {tracking}.");
        }
        else
        {
            errorMessage = $"Order #{Id} couldn't be shipped: it is no longer waiting for the carrier.";
        }

        await shipModal.HideAsync();
    }

    /// <summary>
    /// The dialogs open from code, so Bootstrap does not return focus. After the action the buttons are gone, so focus goes
    /// to the status message; otherwise back to the button that opened the dialog.
    /// </summary>
    private async Task OnCancelHiddenAsync()
    {
        var done = cancelled || errorMessage is not null;
        cancelled = false;

        await EcommerceDisplay.FocusAsync(done ? statusRegion : cancelButton?.Element);
    }

    private async Task OnRefundHiddenAsync()
    {
        var done = refunded || errorMessage is not null;
        refunded = false;

        await EcommerceDisplay.FocusAsync(done ? statusRegion : refundButton?.Element);
    }

    private async Task OnShipHiddenAsync()
    {
        var done = shipped || errorMessage is not null;
        shipped = false;

        await EcommerceDisplay.FocusAsync(done ? statusRegion : shipButton?.Element);
    }

    private Task OnShipShownAsync() => EcommerceDisplay.FocusAsync(trackingInput?.Element);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenCancelAsync()
    {
        errorMessage = null;
        cancelled = false;
        cancelForm = new CancelForm();
        cancelContext = new EditContext(cancelForm);

        await cancelModal.ShowAsync();
    }

    private async Task OpenRefundAsync()
    {
        errorMessage = null;
        refunded = false;
        refundForm = new RefundForm();
        refundContext = new EditContext(refundForm);

        await refundModal.ShowAsync();
    }

    /// <summary>Opens the ship dialog with the order's tracking number filled in, so the demo is quick.</summary>
    private async Task OpenShipAsync()
    {
        errorMessage = null;
        shipped = false;
        shipForm = new ShipForm { TrackingNumber = EcommerceSeedData.TrackingNumberFor(Id) };
        shipContext = new EditContext(shipForm);

        await shipModal.ShowAsync();
    }

    private List<EcommerceReturn> RelatedReturns(EcommerceOrder order) => State.Returns
        .Where(request => request.OrderId == order.Id)
        .OrderByDescending(request => request.RequestedOn)
        .ToList();

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the order number from the route.</summary>
    [Parameter] public int Id { get; set; }

    private EcommerceOrder? Order => State.FindOrder(Id);

    private static string OrdersUrl => $"{EcommerceAdminLayout.Base}/orders";

    [Inject] private EcommerceState State { get; set; } = default!;

    #endregion

    #region Nested Types

    private sealed class CancelForm
    {
        private string reason = string.Empty;

        [StringLength(200, ErrorMessage = "Use 200 characters or fewer.")]
        public string Reason
        {
            get => reason;
            set => reason = value?.Trim() ?? string.Empty;
        }
    }

    private sealed class RefundForm
    {
        private string reason = string.Empty;

        [Required(ErrorMessage = "Enter the reason for the refund.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Use 3 to 200 characters.")]
        public string Reason
        {
            get => reason;
            set => reason = value?.Trim() ?? string.Empty;
        }
    }

    private sealed class ShipForm
    {
        private string trackingNumber = string.Empty;

        [Required(ErrorMessage = "Enter the tracking number.")]
        [StringLength(40, MinimumLength = 4, ErrorMessage = "Use 4 to 40 characters.")]
        [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9 \-]*$", ErrorMessage = "Use letters, digits, spaces, and dashes only.")]
        public string TrackingNumber
        {
            get => trackingNumber;
            set => trackingNumber = value?.Trim() ?? string.Empty;
        }
    }

    #endregion
}
