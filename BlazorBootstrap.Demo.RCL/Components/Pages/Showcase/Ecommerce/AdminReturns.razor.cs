using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The retailer's returns: requests filtered by status (waiting ones first by default), each with its order, customer,
/// items, reason, and the decision once made. A waiting return is approved (its items go back into stock and the amount is
/// refunded) or rejected with a note, each after a confirmation dialog; focus then moves to the status message, or back to
/// the button that opened the dialog when it is dismissed.
/// </summary>
public partial class AdminReturns : ComponentBase, IDisposable
{
    #region Fields and Constants

    private static readonly ReturnFilter[] filters = { ReturnFilter.Requested, ReturnFilter.Approved, ReturnFilter.Rejected, ReturnFilter.All };

    private readonly Dictionary<int, Button> approveButtons = new();

    private readonly Dictionary<int, Button> rejectButtons = new();

    private EditContext approveContext = default!;

    private NoteForm approveForm = new();

    private Modal approveModal = default!;

    /// <summary>Whether the open dialog recorded a decision, so focus goes to the status rather than back to the (now gone) button.</summary>
    private bool decided;

    private EcommerceReturn? deciding;

    /// <summary>Which dialog is open, so a dismissed dialog returns focus to the button that opened it.</summary>
    private bool decidingApprove;

    private ReturnFilter filter = ReturnFilter.Requested;

    private EditContext rejectContext = default!;

    private RejectForm rejectForm = new();

    private Modal rejectModal = default!;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        ResetForms();
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string FilterLabel(ReturnFilter value) => value switch
    {
        ReturnFilter.Requested => "Requested",
        ReturnFilter.Approved => "Approved",
        ReturnFilter.Rejected => "Rejected",
        _ => "All",
    };

    private static string Items(int count) => count == 1 ? "1 item" : $"{count} items";

    private static string ReturnNumber(EcommerceReturn item) => string.Create(CultureInfo.InvariantCulture, $"R-{item.Id:0000}");

    private static BadgeColor StatusColor(EcommerceReturnStatus status) => status switch
    {
        EcommerceReturnStatus.Requested => BadgeColor.Warning,
        EcommerceReturnStatus.Approved => BadgeColor.Success,
        _ => BadgeColor.Danger,
    };

    private static string StatusLabel(EcommerceReturnStatus status) => status switch
    {
        EcommerceReturnStatus.Requested => "Requested",
        EcommerceReturnStatus.Approved => "Approved and refunded",
        _ => "Rejected",
    };

    /// <summary>What approving does, with the amount actually refunded: never more than what's left of the order's total.</summary>
    private string ApproveConsequence(EcommerceReturn item)
    {
        var units = item.Lines.Sum(line => line.Quantity);
        var itemsText = units == 1 ? "1 item goes" : $"{units} items go";

        if (State.FindOrder(item.OrderId) is not { } order)
            return $"{itemsText} back into stock.";

        var refund = RefundFor(item, order);

        return $"{itemsText} back into stock and {EcommerceDisplay.Money(refund)} is refunded to the original card ({order.CardBrand} ending {order.CardLast4}). "
            + "This is a demo: nothing is charged or refunded.";
    }

    private async Task AskApproveAsync(EcommerceReturn item)
    {
        deciding = item;
        decidingApprove = true;
        decided = false;
        ResetForms();

        await approveModal.ShowAsync();
    }

    private async Task AskRejectAsync(EcommerceReturn item)
    {
        deciding = item;
        decidingApprove = false;
        decided = false;
        ResetForms();

        await rejectModal.ShowAsync();
    }

    private async Task ConfirmApproveAsync()
    {
        if (deciding is { } item && State.FindOrder(item.OrderId) is { } order)
        {
            var refund = RefundFor(item, order);

            if (State.ApproveReturn(item.Id, approveForm.Note))
            {
                decided = true;
                SetStatus($"Return {ReturnNumber(item)} is approved: {Items(item.Lines.Sum(line => line.Quantity))} back in stock and {EcommerceDisplay.Money(refund)} refunded on order #{item.OrderId}.");
            }
        }

        await approveModal.HideAsync();
    }

    private async Task ConfirmRejectAsync()
    {
        if (deciding is { } item && State.RejectReturn(item.Id, rejectForm.Note))
        {
            decided = true;
            SetStatus($"Return {ReturnNumber(item)} is rejected. Nothing was refunded.");
        }

        await rejectModal.HideAsync();
    }

    private int CountFor(ReturnFilter value) => value == ReturnFilter.All
        ? State.Returns.Count
        : State.Returns.Count(item => item.Status == ToStatus(value));

    /// <summary>
    /// The dialogs open from code, so Bootstrap does not return focus. After a decision the return's buttons are gone, so
    /// focus goes to the status message; otherwise back to the button that opened the dialog.
    /// </summary>
    private async Task OnDecisionHiddenAsync()
    {
        var id = deciding?.Id;
        var wasApprove = decidingApprove;
        var wasDecided = decided;

        deciding = null;
        decided = false;
        ResetForms();

        if (wasDecided)
            await EcommerceDisplay.FocusAsync(statusRegion);
        else if (id is int returnId && (wasApprove ? approveButtons : rejectButtons).TryGetValue(returnId, out var button))
            await EcommerceDisplay.FocusAsync(button.Element);
    }

    private void OnFilterChanged(ReturnFilter value) => filter = value;

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private decimal RefundFor(EcommerceReturn item, EcommerceOrder order) =>
        Math.Max(0m, Math.Min(item.Amount, order.Totals.Total - order.RefundedAmount));

    private void ResetForms()
    {
        approveForm = new NoteForm();
        approveContext = new EditContext(approveForm);
        rejectForm = new RejectForm();
        rejectContext = new EditContext(rejectForm);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : $"{message} ({statusCount})";
    }

    private static EcommerceReturnStatus ToStatus(ReturnFilter value) => value switch
    {
        ReturnFilter.Approved => EcommerceReturnStatus.Approved,
        ReturnFilter.Rejected => EcommerceReturnStatus.Rejected,
        _ => EcommerceReturnStatus.Requested,
    };

    #endregion

    #endregion

    #region Properties, Indexers

    private string EmptyText => filter switch
    {
        ReturnFilter.Requested => State.Returns.Count == 0
            ? "No return requests yet. Customers can ask to return items from a delivered order."
            : "No returns are waiting for a decision. New requests from customers appear here.",
        ReturnFilter.Approved => "No approved returns yet.",
        ReturnFilter.Rejected => "No rejected returns.",
        _ => "No return requests yet. Customers can ask to return items from a delivered order.",
    };

    private string LeadText
    {
        get
        {
            var waiting = CountFor(ReturnFilter.Requested);
            var refunded = State.Returns.Where(item => item.Status == EcommerceReturnStatus.Approved).Sum(item => item.Amount);

            return $"{(waiting == 1 ? "1 return waits" : $"{waiting} returns wait")} for a decision · {EcommerceDisplay.Money(refunded)} refunded on approved returns";
        }
    }

    /// <summary>The returns in the chosen filter: waiting ones oldest first (the queue), decided ones most recent first.</summary>
    private IReadOnlyList<EcommerceReturn> Shown
    {
        get
        {
            var items = filter == ReturnFilter.All ? State.Returns : State.Returns.Where(item => item.Status == ToStatus(filter));

            return filter == ReturnFilter.Requested
                ? items.OrderBy(item => item.RequestedOn).ThenBy(item => item.Id).ToList()
                : items.OrderByDescending(item => item.DecidedOn ?? item.RequestedOn).ThenByDescending(item => item.Id).ToList();
        }
    }

    [Inject] private EcommerceState State { get; set; } = default!;

    #endregion

    #region Nested Types

    private enum ReturnFilter
    {
        Requested,
        Approved,
        Rejected,
        All,
    }

    private sealed class NoteForm
    {
        private string note = string.Empty;

        [StringLength(200, ErrorMessage = "Use 200 characters or fewer.")]
        public string Note { get => note; set => note = value?.Trim() ?? string.Empty; }
    }

    private sealed class RejectForm
    {
        private string note = string.Empty;

        [Required(ErrorMessage = "Tell the customer why the return is rejected.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Use 5 to 200 characters.")]
        public string Note { get => note; set => note = value?.Trim() ?? string.Empty; }
    }

    #endregion
}
