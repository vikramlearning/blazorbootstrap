using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// One transfer as operations see it (<see cref="FintechState.FindAnyTransfer" />; an unknown reference shows a not-found
/// screen): the customer, the status in words, the amount, the account it's sent from (for the demo customer with its
/// balance, what's available, and what's free for this transfer; for others the masked label), the payee or own account,
/// the dates, the note, the hold or reject reason, and the audit log entries for its reference. A held transfer can be
/// approved (an optional internal note; it is sent now or scheduled for its date) or rejected (a required reason the
/// customer sees; it can't be undone); a scheduled one can be held for review. Each action asks first in a dialog whose
/// field is an <see cref="EditForm" /> (rule WR-002: required reasons and lengths, the message under the field on submit),
/// and its result is announced in a status message that also links to the next transfer in the queue.
/// </summary>
public partial class OpsTransfer : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The longest internal note on an approval.</summary>
    private const int MaxApproveNote = FintechState.MaxApproveNote;

    /// <summary>The longest reason for holding a scheduled transfer.</summary>
    private const int MaxHoldReason = FintechState.MaxHoldReason;

    /// <summary>The longest reason for rejecting a held transfer.</summary>
    private const int MaxRejectReason = FintechState.MaxRejectReason;

    /// <summary>The shortest reason for holding or rejecting a transfer.</summary>
    private const int MinReason = FintechState.MinReason;

    /// <summary>Set when a dialog's action ran, so focus moves to the status message once it is hidden.</summary>
    private bool actionDone;

    private ElementReference? approveButton;

    private EditContext approveContext = default!;

    private NoteForm approveForm = new();

    private int approveLength;

    private Modal approveModal = default!;

    private InputTextArea? approveNoteInput;

    /// <summary>Guards a double click: the first click acts and closes the dialog.</summary>
    private bool busy;

    private int dialogErrorCount;

    private string? dialogError;

    /// <summary>The last refusal without its counter, so a repeat is recognized.</summary>
    private string? dialogErrorText;

    private ElementReference? holdButton;

    private EditContext holdContext = default!;

    private HoldForm holdForm = new();

    private int holdLength;

    private Modal holdModal = default!;

    private InputTextArea? holdReasonInput;

    /// <summary>The button that opened the dialog, so focus can go back to it.</summary>
    private ElementReference? opener;

    private ElementReference? rejectButton;

    private EditContext rejectContext = default!;

    private RejectForm rejectForm = new();

    private int rejectLength;

    private Modal rejectModal = default!;

    private InputTextArea? rejectReasonInput;

    /// <summary>The reference the status message is about; a different route reference clears it.</summary>
    private string? shownReference;

    private AlertColor statusColor = AlertColor.Success;

    private string? statusMessage;

    private ElementReference statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        approveContext = new EditContext(approveForm);
        holdContext = new EditContext(holdForm);
        rejectContext = new EditContext(rejectForm);
        State.Changed += OnStateChanged;
    }

    protected override void OnParametersSet()
    {
        // "Next in the queue" reuses this component for another reference: start it without the last one's message.
        if (shownReference != Reference)
        {
            shownReference = Reference;
            statusMessage = null;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the operations words of a transfer's status.</summary>
    internal static string StatusText(FintechTransferStatus status) => status switch
    {
        FintechTransferStatus.Completed => "Sent",
        FintechTransferStatus.Scheduled => "Scheduled",
        FintechTransferStatus.Held => "Held for review",
        FintechTransferStatus.Rejected => "Rejected",
        _ => "Cancelled by the customer",
    };

    #endregion

    #region Private Methods

    private static string BalanceText(decimal amount) => amount < 0 ? FintechDisplay.SignedMoney(amount) : FintechDisplay.Money(amount);

    private static string ClosedLabel(FintechTransferStatus status) => status switch
    {
        FintechTransferStatus.Completed => "Sent",
        FintechTransferStatus.Rejected => "Rejected",
        _ => "Cancelled",
    };

    private static bool IsLedger(FintechTransfer transfer) => transfer.CustomerId == FintechSeedData.SeededCustomerId;

    /// <summary>Returns the length of a field's text as it is typed, for its character counter.</summary>
    private static int Length(ChangeEventArgs args) => args.Value?.ToString()?.Length ?? 0;

    private static string MatchText(FintechPayeeMatch match) => match switch
    {
        FintechPayeeMatch.Match => "name confirmed",
        FintechPayeeMatch.CloseMatch => "name was a close match",
        FintechPayeeMatch.NoMatch => "name didn't match",
        _ => "name couldn't be checked",
    };

    /// <summary>Ends a reason with exactly one full stop.</summary>
    private static string Sentence(string text) => text.TrimEnd().TrimEnd('.') + ".";

    private Task AskApproveAsync() => OpenAsync(approveModal, approveButton, () =>
    {
        approveForm = new NoteForm();
        approveContext = new EditContext(approveForm);
        approveLength = 0;
    });

    private Task AskHoldAsync() => OpenAsync(holdModal, holdButton, () =>
    {
        holdForm = new HoldForm();
        holdContext = new EditContext(holdForm);
        holdLength = 0;
    });

    private Task AskRejectAsync() => OpenAsync(rejectModal, rejectButton, () =>
    {
        rejectForm = new RejectForm();
        rejectContext = new EditContext(rejectForm);
        rejectLength = 0;
    });

    /// <summary>Returns the audit log entries about a transfer, newest first.</summary>
    private List<ShowcaseAuditEntry> AuditFor(FintechTransfer transfer) =>
        State.AuditEntries.Where(entry => entry.Target.StartsWith(transfer.Reference, StringComparison.Ordinal)).ToList();

    private async Task ConfirmApproveAsync()
    {
        if (busy || Item is not { } transfer || !await approveContext.ValidateAsync())
            return;

        busy = true;
        var result = State.ApproveTransfer(transfer.Reference, approveForm.Text?.Trim());

        if (result == FintechResult.Ok)
        {
            await FinishAsync(approveModal, AlertColor.Success, transfer.Status == FintechTransferStatus.Scheduled
                ? $"Approved. {FintechDisplay.Money(transfer.Amount)} to {transfer.ToName} is scheduled for {Transfers.DateText(transfer.Date)}, and {transfer.CustomerName} is told it was checked."
                : $"Approved and sent. {FintechDisplay.Money(transfer.Amount)} went from {FromText(transfer)} to {transfer.ToName}, and {transfer.CustomerName} is told it was checked.");

            return;
        }

        if (result is FintechResult.WrongState or FintechResult.NotFound)
        {
            await FinishAsync(approveModal, AlertColor.Warning, ResultText(result, transfer));

            return;
        }

        ShowDialogError(result == FintechResult.InsufficientFunds ? InsufficientText(transfer) : ResultText(result, transfer));
        busy = false;
    }

    private async Task ConfirmHoldAsync()
    {
        if (busy || Item is not { } transfer || !await holdContext.ValidateAsync())
            return;

        busy = true;
        var result = State.HoldTransfer(transfer.Reference, holdForm.Text.Trim());

        if (result == FintechResult.Ok)
        {
            await FinishAsync(holdModal, AlertColor.Success,
                $"Held for review. {transfer.Reference} now waits in the review queue and isn't sent on {Transfers.DateText(transfer.Date)} unless it's approved. {transfer.CustomerName} is told it's held for a check.");

            return;
        }

        if (result is FintechResult.WrongState or FintechResult.NotFound)
        {
            await FinishAsync(holdModal, AlertColor.Warning, result == FintechResult.WrongState
                ? $"{transfer.Reference} isn't scheduled any more: it was sent, held, rejected, or cancelled meanwhile. Nothing changed."
                : ResultText(result, transfer));

            return;
        }

        ShowDialogError(result == FintechResult.Invalid ? $"Write a reason of {MinReason} to {MaxHoldReason} characters." : ResultText(result, transfer));
        busy = false;
    }

    private async Task ConfirmRejectAsync()
    {
        if (busy || Item is not { } transfer || !await rejectContext.ValidateAsync())
            return;

        busy = true;
        var result = State.RejectTransfer(transfer.Reference, rejectForm.Text.Trim());

        if (result == FintechResult.Ok)
        {
            await FinishAsync(rejectModal, AlertColor.Secondary,
                $"Rejected. {FintechDisplay.Money(transfer.Amount)} to {transfer.ToName} wasn't sent and the money set aside is released. {transfer.CustomerName} sees your reason.");

            return;
        }

        if (result is FintechResult.WrongState or FintechResult.NotFound)
        {
            await FinishAsync(rejectModal, AlertColor.Warning, ResultText(result, transfer));

            return;
        }

        ShowDialogError(result == FintechResult.Invalid ? $"Write a reason of {MinReason} to {MaxRejectReason} characters." : ResultText(result, transfer));
        busy = false;
    }

    /// <summary>Records the result of an action and closes its dialog; focus then moves to the status message.</summary>
    private async Task FinishAsync(Modal modal, AlertColor color, string message)
    {
        statusColor = color;
        statusMessage = message;
        actionDone = true;

        await modal.HideAsync();
    }

    /// <summary>Returns what a held or scheduled transfer has to draw on: the account's balance less the other transfers set aside from it.</summary>
    private decimal Free(FintechAccount source, FintechTransfer transfer) => source.Balance - State.Reserved(source.Id, transfer.Reference);

    /// <summary>Names the account a transfer is sent from: the demo customer's account name, or another customer's masked label.</summary>
    private string FromText(FintechTransfer transfer) =>
        IsLedger(transfer) ? State.FindAccount(transfer.FromAccountId)?.Name ?? "the customer's account" : transfer.FromAccountId;

    /// <summary>Explains an approval refused because the account no longer holds the money.</summary>
    private string InsufficientText(FintechTransfer transfer)
    {
        if (State.FindAccount(transfer.FromAccountId) is not { } source)
            return $"The account this transfer is sent from is no longer open, so it can't be approved and nothing was sent. Reject it with a reason so {transfer.CustomerName} knows.";

        return $"{source.Name} no longer holds {FintechDisplay.Money(transfer.Amount)}: it has {BalanceText(Free(source, transfer))} free after its other scheduled and held transfers. "
               + $"Nothing was sent. Reject the transfer with a reason so {transfer.CustomerName} knows.";
    }

    /// <summary>Returns the oldest other held transfer, for the link after an action.</summary>
    private FintechTransfer? NextHeld(FintechTransfer current) =>
        State.AllTransfers.Where(transfer => transfer.Status == FintechTransferStatus.Held && transfer.Reference != current.Reference)
            .OrderBy(transfer => transfer.CreatedAt).FirstOrDefault();

    private Task OnApproveShownAsync() => FintechDisplay.FocusAsync(approveNoteInput?.Element);

    /// <summary>
    /// The dialogs open from code, so Bootstrap does not return focus. After an action, focus goes to the status message
    /// (the button that opened the dialog is gone); otherwise back to that button.
    /// </summary>
    private async Task OnDialogHiddenAsync()
    {
        dialogError = dialogErrorText = null;
        dialogErrorCount = 0;
        busy = false;

        if (actionDone)
        {
            actionDone = false;
            StateHasChanged();
            await Task.Yield();
            await FintechDisplay.FocusAsync(statusRegion);
        }
        else
        {
            await FintechDisplay.FocusAsync(opener);
        }

        opener = null;
    }

    private Task OnHoldShownAsync() => FintechDisplay.FocusAsync(holdReasonInput?.Element);

    private Task OnRejectShownAsync() => FintechDisplay.FocusAsync(rejectReasonInput?.Element);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenAsync(Modal modal, ElementReference? button, Action reset)
    {
        reset();
        dialogError = dialogErrorText = null;
        dialogErrorCount = 0;
        actionDone = false;
        busy = false;
        opener = button;

        await modal.ShowAsync();
    }

    /// <summary>Returns the message for a refused action; every result gets its own explanation.</summary>
    private static string ResultText(FintechResult result, FintechTransfer transfer) => result switch
    {
        FintechResult.WrongState => $"{transfer.Reference} isn't held any more: it was approved, rejected, or cancelled meanwhile. Nothing changed.",
        FintechResult.NotFound => $"We couldn't find {transfer.Reference} any more, so nothing changed.",
        FintechResult.NotAllowed => "Your operations sign-in has ended. Sign in again with your one-time code; nothing changed.",
        FintechResult.InsufficientFunds => $"The account no longer holds {FintechDisplay.Money(transfer.Amount)}, so nothing was sent. Reject the transfer with a reason instead.",
        FintechResult.Invalid => "A value is too short or too long. Check the text and try again; nothing changed.",
        _ => "That didn't work, and nothing changed. Try again.",
    };

    /// <summary>Shows a refusal in the dialog's alert; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void ShowDialogError(string message)
    {
        dialogErrorCount = message == dialogErrorText ? dialogErrorCount + 1 : 1;
        dialogErrorText = message;
        dialogError = dialogErrorCount == 1 ? message : $"{message} ({dialogErrorCount.ToString(System.Globalization.CultureInfo.InvariantCulture)})";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private FintechTransfer? Item => State.FindAnyTransfer(Reference);

    private static string QueueUrl => $"{FintechOpsLayout.Base}/queue";

    /// <summary>Gets or sets the transfer reference from the route.</summary>
    [Parameter] public string Reference { get; set; } = string.Empty;

    [Inject] private FintechState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The hold dialog's field: why the scheduled transfer is held (the customer sees it as the hold reason).</summary>
    private sealed class HoldForm
    {
        [Required(ErrorMessage = "Write why you're holding the transfer.")]
        [StringLength(MaxHoldReason, MinimumLength = MinReason, ErrorMessage = "Write a reason of 10 to 100 characters, like \"Checking the payee's details with the customer\".")]
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>The approval dialog's optional internal note.</summary>
    private sealed class NoteForm
    {
        [StringLength(MaxApproveNote, ErrorMessage = "Keep the note to 300 characters.")]
        public string? Text { get; set; }
    }

    /// <summary>The reject dialog's field: the reason the customer reads.</summary>
    private sealed class RejectForm
    {
        [Required(ErrorMessage = "Write the reason the customer will read.")]
        [StringLength(MaxRejectReason, MinimumLength = MinReason, ErrorMessage = "Write a reason of 10 to 300 characters: the customer reads it.")]
        public string Text { get; set; } = string.Empty;
    }

    #endregion
}
