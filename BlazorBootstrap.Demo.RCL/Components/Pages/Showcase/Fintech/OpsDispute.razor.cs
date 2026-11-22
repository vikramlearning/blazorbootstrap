using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// One dispute, as operations see it: the customer (linking to their profile), merchant, amount, reason, what the customer
/// told us, the status with a short timeline (the current step said in text), the disputed transaction for the demo
/// customer (date, account, amount, description), the resolution once closed, and the audit entries about the dispute.
/// A submitted dispute can be put under review; an open one can be refunded or declined, each confirmed in a
/// <see cref="Modal" /> that names the consequence and asks for the resolution the customer reads, in an <see cref="EditForm" />
/// (rule WR-002: required, 10–1000 characters, with a counter; the message shows on submit and is tied to the field). An open
/// dispute past <see cref="FintechState.DisputeTargetDays" /> is marked overdue. Every outcome is announced in a status
/// message that takes focus. An unknown number shows a not-found screen.
/// </summary>
public partial class OpsDispute : ComponentBase, IDisposable
{
    #region Fields and Constants

    private bool resolving;

    private const int MaxResolution = FintechState.MaxDisputeDetails;

    private const int MinResolution = FintechState.MinReason;

    private Button? declineButton;

    private bool focusStatus;

    private int? loadedId;

    private Button? refundButton;

    /// <summary>Whether the dialog closed after a decision was sent, so focus goes to the status message.</summary>
    private bool resolveDone;

    private Modal resolveModal = default!;

    /// <summary>Whether the dialog refunds (true) or declines (false) the dispute.</summary>
    private bool resolveRefund;

    private EditContext resolveContext = default!;

    private ResolutionForm resolveForm = new();

    private int resolutionErrorCount;

    private string? resolutionError;

    private InputTextArea? resolutionInput;

    private int resolutionLength;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusStatus)
        {
            focusStatus = false;
            await FintechDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        resolveContext = new EditContext(resolveForm);
        State.Changed += OnStateChanged;
    }

    protected override void OnParametersSet()
    {
        if (loadedId == Id)
            return;

        loadedId = Id;
        statusMessage = null;
        statusIsError = false;
        resolutionError = null;
        ResetResolveForm();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Says who a refund goes to: the demo customer's account by name, or the customer's account.</summary>
    private static string RefundTarget(string customerName, FintechAccount? account) =>
        account is null ? $"{customerName}'s account" : $"{customerName}'s {account.Name} ({account.MaskedNumber})";

    /// <summary>Returns the message for a refused action; Invalid is answered in the dialog instead.</summary>
    private static string RefusedText(FintechResult result, int disputeId, bool review) => result switch
    {
        FintechResult.NotAllowed => "Sign in again as operations, with a one-time code, to change disputes. Nothing changed.",
        FintechResult.NotFound => $"We couldn't find dispute {disputeId.ToString(CultureInfo.InvariantCulture)}, so nothing changed.",
        FintechResult.WrongState => review
            ? "This dispute isn't waiting for a review any more: it's already under review or closed. Nothing changed."
            : "This dispute is already closed, so nothing changed.",
        FintechResult.Invalid => $"Write a resolution of {MinResolution} to {MaxResolution.ToString("#,0", CultureInfo.InvariantCulture)} characters.",
        _ => "That didn't work, and nothing changed. Try again.",
    };

    private static string StatusSentence(FintechDispute dispute) => dispute.Status switch
    {
        FintechDisputeStatus.Submitted => "Waiting for operations: start the review when you contact the merchant.",
        FintechDisputeStatus.UnderReview => "Under review with the merchant: refund or decline it once you know.",
        FintechDisputeStatus.Refunded => "Closed in the customer's favor: the amount was credited back.",
        _ => "Closed: the charge stands.",
    };

    private async Task AskResolveAsync(bool refund)
    {
        resolveRefund = refund;
        resolveDone = false;
        resolutionError = null;
        ResetResolveForm();

        await resolveModal.ShowAsync();
    }

    /// <summary>Returns the disputed transaction: only the demo customer's ledger is in the tab.</summary>
    private FintechTransaction? Charge(FintechDispute dispute) =>
        dispute.CustomerId == FintechSeedData.SeededCustomerId && dispute.TransactionId != 0 ? State.FindTransaction(dispute.TransactionId) : null;

    private async Task ConfirmResolveAsync()
    {
        // A second click while the dialog closes would send the decision twice.
        if (Dispute is not { } dispute || resolving)
            return;

        resolutionError = null;

        // A failed check shows the message under the field, and ShowcaseFormErrors moves focus to it.
        if (!await resolveContext.ValidateAsync())
            return;

        var text = resolveForm.Resolution.Trim();

        var customerName = State.FindCustomer(dispute.CustomerId)?.Name ?? "The customer";
        var result = State.ResolveDispute(dispute.Id, resolveRefund, text);

        if (result == FintechResult.Invalid)
        {
            resolutionErrorCount++;
            resolutionError = resolutionErrorCount == 1 ? RefusedText(result, dispute.Id, false) : $"{RefusedText(result, dispute.Id, false)} ({resolutionErrorCount.ToString(CultureInfo.InvariantCulture)})";
            await FintechDisplay.FocusAsync(resolutionInput?.Element);
            return;
        }

        resolving = true;
        statusIsError = result != FintechResult.Ok;
        statusMessage = result == FintechResult.Ok
            ? resolveRefund
                ? $"Dispute {dispute.Id.ToString(CultureInfo.InvariantCulture)} is refunded: {FintechDisplay.Money(dispute.Amount)} was credited back, and {customerName} has been told."
                : $"Dispute {dispute.Id.ToString(CultureInfo.InvariantCulture)} is declined: the charge stands, and {customerName} has been told."
            : RefusedText(result, dispute.Id, false);
        resolveDone = true;

        await resolveModal.HideAsync();
    }

    /// <summary>Starting a review only tells the customer it's being looked at, so it needs no confirmation; the outcome is announced.</summary>
    private void StartReview()
    {
        if (Dispute is not { } dispute)
            return;

        var result = State.StartDisputeReview(dispute.Id);
        var customerName = State.FindCustomer(dispute.CustomerId)?.Name ?? "The customer";

        statusIsError = result != FintechResult.Ok;
        statusMessage = result == FintechResult.Ok
            ? $"Dispute {dispute.Id.ToString(CultureInfo.InvariantCulture)} is under review. {customerName} has been told."
            : RefusedText(result, dispute.Id, true);
        focusStatus = true;
    }

    /// <summary>
    /// The dialog opens from code, so Bootstrap does not return focus: after a decision, focus goes to the status message
    /// (the buttons are gone once the dispute closes); after Cancel, back to the button that opened it.
    /// </summary>
    private async Task OnResolveHiddenAsync()
    {
        resolving = false;
        resolutionError = null;

        if (resolveDone)
        {
            resolveDone = false;
            StateHasChanged();
            await FintechDisplay.FocusAsync(statusRegion);
        }
        else
        {
            await FintechDisplay.FocusAsync((resolveRefund ? refundButton : declineButton)?.Element);
        }
    }

    private void OnResolutionInput(ChangeEventArgs args) => resolutionLength = args.Value?.ToString()?.Length ?? 0;

    private Task OnResolveShownAsync() => FintechDisplay.FocusAsync(resolutionInput?.Element);

    /// <summary>Starts the dialog's form empty, with no messages from a previous decision.</summary>
    private void ResetResolveForm()
    {
        resolveForm = new ResolutionForm();
        resolveContext = new EditContext(resolveForm);
        resolutionLength = 0;
        resolutionErrorCount = 0;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private IReadOnlyList<Step> Steps(FintechDispute dispute, string customerName)
    {
        var closed = !OpsDisputes.IsOpen(dispute);
        var reviewing = dispute.Status == FintechDisputeStatus.UnderReview;
        var reviewEntry = AuditEntries.LastOrDefault(entry => entry.Action == "Dispute under review");
        var reviewedOn = reviewEntry is null ? null : $" on {Transfers.StampText(reviewEntry.At)}";
        var closedOn = dispute.ClosedAt is { } closedAt ? $" on {Transfers.StampText(closedAt)}" : string.Empty;

        return new List<Step>
        {
            new("Submitted", $"{customerName} opened it on {Transfers.StampText(dispute.CreatedAt)}.", Done: true, IsCurrent: dispute.Status == FintechDisputeStatus.Submitted),
            new("Under review",
                reviewing ? $"Review started{reviewedOn ?? string.Empty}." : closed ? (reviewedOn is null ? "Reviewed with the merchant." : $"Review started{reviewedOn}.") : "Not started: start the review when you contact the merchant.",
                Done: closed, IsCurrent: reviewing),
            closed
                ? new(Disputes.StatusText(dispute.Status), $"{Disputes.StatusText(dispute.Status)}{closedOn}.", Done: true, IsCurrent: true)
                : new("Refunded or declined", "Refund the amount, or explain why the charge stands.", Done: false, IsCurrent: false),
        };
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the audit entries about this dispute, newest first (their subject starts with "Dispute {id}").</summary>
    private IReadOnlyList<ShowcaseAuditEntry> AuditEntries
    {
        get
        {
            var subject = $"Dispute {Id.ToString(CultureInfo.InvariantCulture)}";

            return State.AuditEntries
                .Where(entry => entry.Target == subject || entry.Target.StartsWith(subject + " ", StringComparison.Ordinal))
                .ToList();
        }
    }

    private FintechDispute? Dispute => State.FindAnyDispute(Id);

    private static string DisputesPath => $"{FintechOpsLayout.Base}/disputes";

    /// <summary>Gets or sets the dispute number from the route.</summary>
    [Parameter] public int Id { get; set; }

    [Inject] private FintechState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The decision dialog's field: the resolution the customer reads.</summary>
    private sealed class ResolutionForm
    {
        [Required(ErrorMessage = "Write the resolution the customer will read.")]
        [StringLength(MaxResolution, MinimumLength = MinResolution, ErrorMessage = "Write a resolution of 10 to 1,000 characters: the customer reads this.")]
        public string Resolution { get; set; } = string.Empty;
    }

    /// <summary>One step of the dispute timeline: done steps show a check; the current one is named in text.</summary>
    private sealed record Step(string Title, string Note, bool Done, bool IsCurrent);

    #endregion
}
