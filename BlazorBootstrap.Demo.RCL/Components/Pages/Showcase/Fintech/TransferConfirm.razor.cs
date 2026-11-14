using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Confirm screen: shows the pending transfer, asks for a final confirmation in a <see cref="Modal" />, and then shows
/// the result in an <see cref="Alert" />. Without a pending transfer (for example after a reload) it says there is
/// nothing to confirm.
/// </summary>
public partial class TransferConfirm : ComponentBase
{
    #region Fields and Constants

    private Button? confirmButton;

    private Modal confirmModal = default!;

    /// <summary>The transfer shown in the dialog, captured when it opens, so the text stays put while the dialog closes.</summary>
    private FintechTransferDraft? confirming;

    private string? errorMessage;

    private FintechTransferResult? result;

    private ElementReference resultRegion;

    private bool sending;

    #endregion

    #region Methods

    #region Private Methods

    private async Task ConfirmAsync()
    {
        if (State.PendingTransfer is not { } draft)
            return;

        confirming = draft;
        errorMessage = null;
        StateHasChanged();

        await confirmModal.ShowAsync();
    }

    /// <summary>
    /// The dialog opens from code, so focus is placed explicitly once it has closed: on the result when the transfer went
    /// through, otherwise (Cancel, Escape, or a failed transfer) back on "Confirm transfer".
    /// </summary>
    private async Task OnConfirmHiddenAsync()
    {
        confirming = null;
        sending = false;
        StateHasChanged();
        await Task.Yield();
        await FintechDisplay.FocusAsync(result is null ? confirmButton?.Element : resultRegion);
    }

    private async Task SendAsync()
    {
        // Guards a double click: the first click carries out the transfer and closes the dialog.
        if (sending)
            return;

        sending = true;
        result = State.ExecutePendingTransfer();
        errorMessage = result is null ? "This transfer is no longer valid. Edit it and try again." : null;

        await confirmModal.HideAsync();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private FintechState State { get; set; } = default!;

    private static DateOnly Today => DateOnly.FromDateTime(FintechSeedData.Today);

    #endregion
}
