using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// One return at <c>admin/returns/{id}</c> (the retailer): the request (customer, reason, comment, the order's total and
/// what was refunded on it so far), its items, and, while it waits, the decision form. Approving takes a refund amount
/// above $0 and at most the items' total and what's left of the order (a cross-field rule), with a note required when the
/// refund is less than the items' total; rejecting takes a reason the customer reads (rule WR-002: a linked summary on a
/// failed submit). The decision goes through <see cref="EcommerceState.ApproveReturn" /> or
/// <see cref="EcommerceState.RejectReturn" />, which audit it and tell the customer. An unknown number shows a not-found
/// panel.
/// </summary>
public partial class AdminReturnDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const string ApproveValue = "approve";

    private const string RejectValue = "reject";

    private EditContext editContext = default!;

    private int errorCount;

    private string? errorMessage;

    private string? errorText;

    private bool focusStatus;

    private DecisionForm form = new();

    private int? loadedId;

    private string? statusMessage;

    private ElementReference statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusStatus)
        {
            focusStatus = false;
            await EcommerceDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (loadedId == Id)
            return;

        loadedId = Id;
        statusMessage = null;
        errorMessage = null;
        ResetForm();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Starts the form for this return: a full refund up to what's left of the order, or a rejection when nothing can be refunded.</summary>
    private void ResetForm()
    {
        var item = Return;
        var order = item is null ? null : State.FindOrder(item.OrderId);
        var maxRefund = item is not null && order is { Status: EcommerceOrderStatus.Delivered } ? State.MaxRefundFor(item, order) : 0m;

        form = new DecisionForm
        {
            Decision = maxRefund > 0 ? string.Empty : RejectValue,
            Refund = maxRefund > 0 ? maxRefund : null,
            MaxRefund = maxRefund,
            ItemsTotal = item?.Amount ?? 0m,
        };
        editContext = new EditContext(form);
    }

    /// <summary>
    /// Records the decision once the form is valid; a failed submit shows the summary (rule WR-002). The state checks again;
    /// a refusal (the return was decided meanwhile, or the order refunded) shows in the alert.
    /// </summary>
    private async Task Save()
    {
        if (Return is not { } item || !await editContext.ValidateAsync())
            return;

        var number = EcommerceDisplay.ReturnNumber(item.Id);

        if (form.Decision == ApproveValue && form.Refund is { } refund)
        {
            var amount = Math.Round(refund, 2, MidpointRounding.AwayFromZero);

            // The most there is to refund needs no reason, even below the items' total (the order was partly refunded already).
            if (!State.ApproveReturn(item.Id, form.Note, amount == form.MaxRefund ? null : amount))
            {
                ShowError($"Return {number} couldn't be approved: it was decided meanwhile, or its order was refunded. Nothing was refunded.");

                return;
            }

            statusMessage = $"Return {number} is approved: {EcommerceDisplay.Plural(item.Lines.Sum(line => line.Quantity), "item")} back in stock and {EcommerceDisplay.Money(amount)} refunded on order #{item.OrderId}. The customer was told.";
        }
        else
        {
            if (!State.RejectReturn(item.Id, form.Note))
            {
                ShowError($"Return {number} couldn't be rejected: it was decided meanwhile.");

                return;
            }

            statusMessage = $"Return {number} is rejected. Nothing was refunded, and the customer was told why.";
        }

        errorMessage = null;
        focusStatus = true;
    }

    private void ShowError(string message)
    {
        errorCount = message == errorText ? errorCount + 1 : 1;
        errorText = message;
        errorMessage = errorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({errorCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the return's number from the route.</summary>
    [Parameter] public int Id { get; set; }

    private EcommerceReturn? Return => State.FindReturn(Id);

    private static string ReturnsUrl => $"{EcommerceAdminLayout.Base}/returns";

    [Inject] private EcommerceState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>
    /// The decision's fields: approve or reject; to approve, a refund above $0 and at most <see cref="MaxRefund" /> (a note
    /// is required when it is below <see cref="ItemsTotal" />); to reject, a reason of 5 to 200 characters.
    /// </summary>
    private sealed class DecisionForm : IValidatableObject
    {
        [Required(ErrorMessage = "Choose whether to approve or reject the return.")]
        public string Decision { get; set; } = string.Empty;

        /// <summary>The returned items' total (set by the page).</summary>
        public decimal ItemsTotal { get; init; }

        /// <summary>The most the approval can refund: the items' total, or what's left of the order when less (set by the page).</summary>
        public decimal MaxRefund { get; init; }

        [StringLength(EcommerceState.MaxDecisionNoteLength, ErrorMessage = "Use 200 characters or fewer for the note.")]
        public string? Note { get; set; }

        public decimal? Refund { get; set; }

        /// <summary>Runs once the fields are filled in: the refund's range, and the note an approval below the total or a rejection needs.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var note = Note?.Trim() ?? string.Empty;

            if (Decision == RejectValue)
            {
                if (note.Length < EcommerceState.MinReasonLength)
                    yield return new ValidationResult("Tell the customer why the return is rejected, in 5 to 200 characters.", new[] { nameof(Note) });

                yield break;
            }

            if (Decision != ApproveValue)
                yield break;

            if (Refund is not { } refund)
            {
                yield return new ValidationResult($"Enter the refund, up to {EcommerceDisplay.Money(MaxRefund)}.", new[] { nameof(Refund) });
            }
            else if (refund <= 0 || Math.Round(refund, 2) > MaxRefund)
            {
                yield return new ValidationResult($"Enter a refund above $0.00 and up to {EcommerceDisplay.Money(MaxRefund)}.", new[] { nameof(Refund) });
            }
            else if (Math.Round(refund, 2) < ItemsTotal && Math.Round(refund, 2) < MaxRefund && note.Length < EcommerceState.MinReasonLength)
            {
                yield return new ValidationResult("Say why the refund is less than the items' total, like “15% restocking fee: the item was used”, in 5 to 200 characters.", new[] { nameof(Note) });
            }
        }
    }

    #endregion
}
