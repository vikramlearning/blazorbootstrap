using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// The transfers hub: "Coming up" lists scheduled transfers and transfers held for a check, by date, with the hold reason
/// and a Cancel action (a dialog says the money set aside is released); "Recent" lists sent, rejected (with the reason),
/// and cancelled transfers, newest first. Every row links to the transfer's receipt, statuses are words, and a prominent
/// "New transfer" button opens the transfer form. The status texts and the cancel messages are shared with
/// <see cref="TransferDetail" />.
/// </summary>
public partial class Transfers : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<string, ElementReference> cancelButtons = new(StringComparer.Ordinal);

    /// <summary>Set when the dialog's action ran, so focus moves to the status message once it is hidden.</summary>
    private bool actionDone;

    private bool cancelBusy;

    /// <summary>The transfer the dialog is about, captured when it opens, so the text stays put while the dialog closes.</summary>
    private FintechTransfer? cancelling;

    private Modal cancelModal = default!;

    private string? dialogError;

    /// <summary>The button that opened the dialog, so focus can go back to it.</summary>
    private ElementReference? opener;

    private string? statusMessage;

    private ElementReference statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the message for the result of cancelling a transfer.</summary>
    internal static string CancelResultText(FintechResult result, FintechTransfer transfer, string fromName) => result switch
    {
        FintechResult.Ok => $"The {FintechDisplay.Money(transfer.Amount)} transfer to {transfer.ToName} is cancelled. The money is available in {fromName} again.",
        FintechResult.WrongState => "This transfer can't be cancelled any more: it was already sent, rejected, or cancelled.",
        FintechResult.NotFound => "We couldn't find this transfer on your account, so nothing changed.",
        FintechResult.NotAllowed => "Sign in again to cancel transfers. Nothing changed.",
        _ => "That didn't work, and nothing changed. Try again.",
    };

    /// <summary>Says what cancelling a waiting transfer stops.</summary>
    internal static string CancelText(FintechTransfer transfer) => transfer.Status == FintechTransferStatus.Held
        ? $"{FintechDisplay.Money(transfer.Amount)} to {transfer.ToName}, held for a check, won't be sent."
        : $"{FintechDisplay.Money(transfer.Amount)} to {transfer.ToName}, scheduled for {DateText(transfer.Date)}, won't be sent.";

    /// <summary>Formats a transfer date, like "Tue, Dec 1, 2026".</summary>
    internal static string DateText(DateOnly date) => date.ToString("ddd, MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>Returns the receipt's URL of a transfer.</summary>
    internal static string ReceiptUrl(string reference) => $"{FintechLayout.Base}/transfers/{Uri.EscapeDataString(reference)}";

    /// <summary>Formats a date and time, like "Nov 24, 2026 at 09:00".</summary>
    internal static string StampText(DateTime at) => at.ToString("MMM d, yyyy 'at' HH:mm", CultureInfo.InvariantCulture);

    /// <summary>Returns the badge color of a status; the badge's text carries the meaning.</summary>
    internal static BadgeColor StatusColor(FintechTransferStatus status) => status switch
    {
        FintechTransferStatus.Completed => BadgeColor.Success,
        FintechTransferStatus.Scheduled => BadgeColor.Info,
        FintechTransferStatus.Held => BadgeColor.Warning,
        FintechTransferStatus.Rejected => BadgeColor.Danger,
        _ => BadgeColor.Secondary,
    };

    /// <summary>Returns the icon of a status.</summary>
    internal static IconName StatusIcon(FintechTransferStatus status) => status switch
    {
        FintechTransferStatus.Completed => IconName.CheckCircle,
        FintechTransferStatus.Scheduled => IconName.CalendarEvent,
        FintechTransferStatus.Held => IconName.ShieldExclamation,
        FintechTransferStatus.Rejected => IconName.XCircle,
        _ => IconName.SlashCircle,
    };

    /// <summary>Returns the words of a status.</summary>
    internal static string StatusText(FintechTransferStatus status) => status switch
    {
        FintechTransferStatus.Completed => "Sent",
        FintechTransferStatus.Scheduled => "Scheduled",
        FintechTransferStatus.Held => "Held for a check",
        FintechTransferStatus.Rejected => "Not sent",
        _ => "Cancelled",
    };

    #endregion

    #region Private Methods

    private static bool IsWaiting(FintechTransfer transfer) => transfer.Status is FintechTransferStatus.Scheduled or FintechTransferStatus.Held;

    private string AccountName(string accountId) => State.FindAccount(accountId)?.Name ?? "your account";

    private async Task AskCancelAsync(FintechTransfer transfer)
    {
        cancelling = transfer;
        dialogError = null;
        actionDone = false;
        cancelBusy = false;
        opener = cancelButtons.TryGetValue(transfer.Reference, out var button) ? button : null;

        await cancelModal.ShowAsync();
    }

    /// <summary>Says when a sent, rejected, or cancelled transfer closed.</summary>
    private static string ClosedText(FintechTransfer transfer)
    {
        var at = transfer.ClosedAt ?? transfer.CreatedAt;

        return transfer.Status switch
        {
            FintechTransferStatus.Completed => $"Sent {FintechDisplay.Day(at)}",
            FintechTransferStatus.Rejected => $"Not sent {FintechDisplay.Day(at)}",
            _ => $"Cancelled {FintechDisplay.Day(at)}",
        };
    }

    private async Task ConfirmCancelAsync()
    {
        // Guards a double click: the first click cancels and closes the dialog.
        if (cancelBusy || cancelling is not { } transfer)
            return;

        cancelBusy = true;
        var result = State.CancelTransfer(transfer.Reference);
        var message = CancelResultText(result, transfer, AccountName(transfer.FromAccountId));

        if (result == FintechResult.Ok)
        {
            actionDone = true;
            statusMessage = message;
            await cancelModal.HideAsync();
        }
        else
        {
            dialogError = message;
            cancelBusy = false;
        }
    }

    /// <summary>
    /// The dialog opens from code, so Bootstrap does not return focus. After a cancellation, focus goes to the status
    /// message (the row has moved to "Recent"); otherwise back to the Cancel button that opened the dialog.
    /// </summary>
    private async Task OnCancelHiddenAsync()
    {
        dialogError = null;
        cancelBusy = false;

        if (actionDone)
        {
            actionDone = false;
            cancelling = null;
            StateHasChanged();
            await Task.Yield();
            await FintechDisplay.FocusAsync(statusRegion);
        }
        else
        {
            cancelling = null;
            await FintechDisplay.FocusAsync(opener);
        }

        opener = null;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private FintechState State { get; set; } = default!;

    #endregion
}
