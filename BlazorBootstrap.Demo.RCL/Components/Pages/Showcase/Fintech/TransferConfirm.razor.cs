using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// The review screen of the transfer waiting in <see cref="FintechState.PendingTransfer" />: the amount, from, to (with a
/// payee's bank and masked account), the date, the note, and what happens next (sent now, scheduled for a date, or held
/// for a check and why). "Confirm transfer" calls <see cref="FintechState.ConfirmPendingTransfer" /> once (a second click
/// is ignored) and opens the receipt; "Edit" goes back to the form, which restores the transfer. Without a waiting
/// transfer (for example after a reload) it explains that there is nothing to review.
/// </summary>
public partial class TransferConfirm : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Set once the transfer is confirmed, while the receipt opens.</summary>
    private bool confirmed;

    private string? errorMessage;

    private ElementReference errorRegion;

    private bool focusError;

    private bool submitting;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusError)
        {
            focusError = false;
            await FintechDisplay.FocusAsync(errorRegion);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Lowers the first letter of a hold reason, so it reads inside a sentence.</summary>
    private static string HoldBecause(string reason) => reason.Length == 0 ? reason : char.ToLowerInvariant(reason[0]) + reason[1..];

    private void Confirm()
    {
        // Guards a double submit: the first click confirms the transfer and opens the receipt.
        if (submitting || confirmed)
            return;

        submitting = true;
        var draft = State.PendingTransfer;
        var result = State.ConfirmPendingTransfer(out var transfer);

        if (result == FintechResult.Ok && transfer is not null)
        {
            confirmed = true;
            Navigation.NavigateTo(Transfers.ReceiptUrl(transfer.Reference));

            return;
        }

        submitting = false;
        errorMessage = result switch
        {
            FintechResult.NotAllowed => "Sign in again to move money. Nothing was sent.",
            FintechResult.NotFound => "This transfer is no longer waiting for review. Nothing was sent; start a new transfer.",
            FintechResult.Invalid => $"{(draft is null ? null : State.ValidateTransfer(draft)) ?? "This transfer is no longer valid."} Nothing was sent; edit the transfer and try again.",
            FintechResult.LimitReached => "This demo keeps up to 500 transfers, and that's reached. Use Reset demo data in the account menu to start again. Nothing was sent.",
            FintechResult.InsufficientFunds => "There isn't enough money available for this transfer. Nothing was sent; edit the amount and try again.",
            _ => "That didn't work, and nothing was sent. Try again.",
        };
        focusError = true;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private FintechState State { get; set; } = default!;

    #endregion
}
