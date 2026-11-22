using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// The receipt of one of the customer's transfers (<see cref="FintechState.FindTransfer" />; anyone else's or an unknown
/// reference shows a not-found screen): the reference, the status in words, from, to, the amount, the date, when it was
/// confirmed and closed, the note, the hold or reject reason, and links to the transactions it created. Straight after
/// confirming, a one-time banner (<see cref="FintechState.TakeLastTransferReference" />) says it was sent, scheduled, or
/// held for a check and what happens next. A scheduled or held transfer can be cancelled after a confirmation dialog.
/// Print opens the browser's print dialog; the site's chrome and the buttons don't print.
/// </summary>
public partial class TransferDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Set when the dialog's action ran, so focus moves to the status message once it is hidden.</summary>
    private bool actionDone;

    /// <summary>The one-time confirmation shown now.</summary>
    private Banner? banner;

    private bool cancelBusy;

    private ElementReference? cancelButton;

    /// <summary>The transfer the dialog is about, captured when it opens, so the text stays put while the dialog closes.</summary>
    private FintechTransfer? cancelling;

    private Modal cancelModal = default!;

    private string? dialogError;

    /// <summary>The confirmation taken from the state, shown after the first render so the status region announces it.</summary>
    private Banner? pendingBanner;

    private string? statusMessage;

    private ElementReference statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnAfterRender(bool firstRender)
    {
        // The router focuses the heading after navigation; a banner added now is announced by the status region.
        if (firstRender && pendingBanner is not null)
        {
            banner = pendingBanner;
            pendingBanner = null;
            StateHasChanged();
        }
    }

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;

        // Only the transfer just confirmed in this tab gets the banner, and only once.
        if (State.LastTransferReference == Reference && State.TakeLastTransferReference() is not null && Item is { } transfer)
            pendingBanner = BannerFor(transfer);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string ClosedLabel(FintechTransferStatus status) => status switch
    {
        FintechTransferStatus.Completed => "Completed",
        FintechTransferStatus.Rejected => "Rejected",
        _ => "Cancelled",
    };

    private string AccountName(string accountId) => State.FindAccount(accountId)?.Name ?? "your account";

    private async Task AskCancelAsync()
    {
        if (Item is not { } transfer)
            return;

        cancelling = transfer;
        dialogError = null;
        actionDone = false;
        cancelBusy = false;

        await cancelModal.ShowAsync();
    }

    /// <summary>Says what just happened to a transfer the customer confirmed, and what happens next.</summary>
    private Banner BannerFor(FintechTransfer transfer)
    {
        var from = AccountName(transfer.FromAccountId);

        return transfer.Status switch
        {
            FintechTransferStatus.Held => new Banner(AlertColor.Warning, IconName.ShieldExclamation, "Held for a check",
                $"{transfer.HoldReason ?? "This payment needs a routine check"}, so we're checking it before it's sent. Our team usually checks within one business day, and we'll send you an alert when it's sent or if it isn't. The {FintechDisplay.Money(transfer.Amount)} is set aside in {from} meanwhile; you can cancel the transfer while it's held."),
            FintechTransferStatus.Scheduled => new Banner(AlertColor.Info, IconName.CalendarCheck, "Scheduled",
                $"{FintechDisplay.Money(transfer.Amount)} to {transfer.ToName} goes on {Transfers.DateText(transfer.Date)}. The money is set aside in {from} until then, and you can cancel it before it's sent."),
            FintechTransferStatus.Completed => new Banner(AlertColor.Success, IconName.CheckCircleFill, "Sent",
                $"{FintechDisplay.Money(transfer.Amount)} went from {from} to {transfer.ToName}. It's in your transactions now."),
            _ => new Banner(AlertColor.Secondary, IconName.InfoCircle, Transfers.StatusText(transfer.Status), "This transfer wasn't sent."),
        };
    }

    private async Task ConfirmCancelAsync()
    {
        // Guards a double click: the first click cancels and closes the dialog.
        if (cancelBusy || cancelling is not { } transfer)
            return;

        cancelBusy = true;
        var result = State.CancelTransfer(transfer.Reference);
        var message = Transfers.CancelResultText(result, transfer, AccountName(transfer.FromAccountId));

        if (result == FintechResult.Ok)
        {
            actionDone = true;
            banner = null;
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
    /// message (the Cancel button is gone); otherwise back to the Cancel button.
    /// </summary>
    private async Task OnCancelHiddenAsync()
    {
        dialogError = null;
        cancelBusy = false;
        cancelling = null;

        if (actionDone)
        {
            actionDone = false;
            await Task.Yield();
            await FintechDisplay.FocusAsync(statusRegion);
        }
        else
        {
            await FintechDisplay.FocusAsync(cancelButton);
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task PrintAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("print");
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException)
        {
            // Printing is a convenience; the receipt stays on screen, and the browser's own print command still works.
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private FintechTransfer? Item => State.FindTransfer(Reference);

    [Inject] private IJSRuntime JS { get; set; } = default!;

    /// <summary>Gets or sets the transfer reference from the route.</summary>
    [Parameter] public string Reference { get; set; } = string.Empty;

    [Inject] private FintechState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The one-time confirmation after a transfer is confirmed.</summary>
    private sealed record Banner(AlertColor Color, IconName Icon, string Title, string Text);

    #endregion
}
