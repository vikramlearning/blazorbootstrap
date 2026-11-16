using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// The customer's payees, by name: the name, nickname, bank, masked account, the name-check result in words, when it was
/// added, and a "New" badge for the first <see cref="FintechState.NewPayeeDays" /> days (larger transfers to it are held
/// for a check). Each payee can be paid (the transfer form with it preselected), edited (a new name check and a code),
/// given a nickname inline (no code: it doesn't change where money goes), or deleted after a confirmation; a payee with
/// a scheduled or held transfer can't be deleted, and the dialog says why. The status region reports every action and a
/// save from the editor (<see cref="Saved" />).
/// </summary>
public partial class Payees : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<string, ElementReference> deleteButtons = new(StringComparer.Ordinal);

    private readonly Dictionary<string, ElementReference> renameButtons = new(StringComparer.Ordinal);

    /// <summary>Set when the dialog's action ran, so focus moves to the status message once it is hidden.</summary>
    private bool actionDone;

    /// <summary>Whether a scheduled or held transfer goes to the payee in the dialog.</summary>
    private bool deleteBlocked;

    private bool deleteBusy;

    private Modal deleteModal = default!;

    /// <summary>The payee the dialog is about, captured when it opens, so the text stays put while the dialog closes.</summary>
    private FintechPayee? deleting;

    private string? dialogError;

    /// <summary>Where focus goes after the next render, if anywhere.</summary>
    private ElementReference? focusNext;

    /// <summary>Set when the nickname field opens, so it is focused once rendered.</summary>
    private bool focusRename;

    /// <summary>Set when "Go to transfers" was chosen, so the page navigates once the dialog is hidden.</summary>
    private bool goToTransfers;

    private string nicknameText = string.Empty;

    private string? renameError;

    private ElementReference renameInput;

    /// <summary>The payee whose nickname is being changed, if any.</summary>
    private string? renamingId;

    /// <summary>The save message from the editor, shown after the first render so the status region announces it.</summary>
    private string? savedMessage;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // The router focuses the heading after navigation; a message added now is announced by the status region.
        if (firstRender && savedMessage is not null)
        {
            SetStatus(savedMessage);
            savedMessage = null;
            StateHasChanged();
        }

        // The nickname field only exists after this render, so it is focused here.
        if (focusRename)
        {
            focusRename = false;
            await FintechDisplay.FocusAsync(renameInput);
        }

        if (focusNext is { } target)
        {
            focusNext = null;
            await FintechDisplay.FocusAsync(target);
        }
    }

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;

        if (Saved is { } id && State.FindPayee(id) is { } payee)
        {
            savedMessage = Added == true
                ? $"{payee.Name} is added to your payees. For the next {FintechState.NewPayeeDays} days, transfers of {FintechDisplay.Money(FintechState.HoldNewPayeeAmount)} or more to them are held for a check."
                : $"{payee.Name} is saved. The changed payee counts as new for {FintechState.NewPayeeDays} days.";
        }
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the icon of a name-check result.</summary>
    internal static IconName MatchIcon(FintechPayeeMatch match) => match switch
    {
        FintechPayeeMatch.Match => IconName.PatchCheck,
        FintechPayeeMatch.CloseMatch => IconName.PatchExclamation,
        FintechPayeeMatch.NoMatch => IconName.ExclamationTriangle,
        _ => IconName.QuestionCircle,
    };

    /// <summary>Returns the words of a name-check result.</summary>
    internal static string MatchText(FintechPayeeMatch match) => match switch
    {
        FintechPayeeMatch.Match => "Name confirmed",
        FintechPayeeMatch.CloseMatch => "Close match",
        FintechPayeeMatch.NoMatch => "Name didn't match",
        _ => "Not checked",
    };

    #endregion

    #region Private Methods

    private static string CountText(int count) => string.Create(CultureInfo.InvariantCulture, $"{count} of {FintechState.MaxPayees} payees.");

    private async Task AskDeleteAsync(FintechPayee payee)
    {
        CancelRename();
        deleting = payee;
        deleteBlocked = HasWaitingTransfer(payee.Id);
        dialogError = null;
        actionDone = false;
        deleteBusy = false;
        goToTransfers = false;

        await deleteModal.ShowAsync();
    }

    private void CancelRename()
    {
        if (renamingId is { } id && renameButtons.TryGetValue(id, out var button))
            focusNext = button;

        renamingId = null;
        renameError = null;
    }

    private async Task ConfirmDeleteAsync()
    {
        // Guards a double click: the first click deletes and closes the dialog.
        if (deleteBusy || deleting is not { } payee)
            return;

        deleteBusy = true;
        var result = State.DeletePayee(payee.Id);

        switch (result)
        {
            case FintechResult.Ok:
                actionDone = true;
                SetStatus($"{payee.Name} is deleted from your payees.");
                await deleteModal.HideAsync();
                break;

            case FintechResult.WrongState:
                deleteBlocked = true;
                dialogError = null;
                break;

            case FintechResult.NotFound:
                dialogError = "This payee is already gone from your payees.";
                break;

            case FintechResult.NotAllowed:
                dialogError = "Sign in again to change your payees. Nothing changed.";
                break;

            default:
                dialogError = "That didn't work, and nothing changed. Try again.";
                break;
        }

        deleteBusy = false;
    }

    private async Task GoToTransfersAsync()
    {
        goToTransfers = true;
        await deleteModal.HideAsync();
    }

    private bool HasWaitingTransfer(string payeeId) =>
        State.MyTransfers().Any(transfer => transfer.ToId == payeeId && transfer.Status is FintechTransferStatus.Scheduled or FintechTransferStatus.Held);

    /// <summary>
    /// The dialog opens from code, so Bootstrap does not return focus. After a deletion, focus goes to the status message
    /// (the payee's buttons are gone); otherwise back to the Delete button that opened the dialog.
    /// </summary>
    private async Task OnDeleteHiddenAsync()
    {
        var payeeId = deleting?.Id;
        dialogError = null;
        deleting = null;

        if (goToTransfers)
        {
            goToTransfers = false;
            Navigation.NavigateTo($"{FintechLayout.Base}/transfers");

            return;
        }

        if (actionDone)
        {
            actionDone = false;
            StateHasChanged();
            await Task.Yield();
            await FintechDisplay.FocusAsync(statusRegion);
        }
        else if (payeeId is not null && deleteButtons.TryGetValue(payeeId, out var button))
        {
            await FintechDisplay.FocusAsync(button);
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void SaveNickname()
    {
        if (renamingId is not { } id || State.FindPayee(id) is not { } payee)
        {
            renamingId = null;
            SetStatus("That payee is no longer in your payees.");

            return;
        }

        var result = State.SetPayeeNickname(id, nicknameText);

        switch (result)
        {
            case FintechResult.Ok:
            case FintechResult.NoChange:
                var saved = State.FindPayee(id)?.Nickname;
                SetStatus(result == FintechResult.NoChange
                    ? $"Nothing changed: {payee.Name}'s nickname is the same."
                    : saved is null ? $"{payee.Name}'s nickname is removed." : $"{payee.Name}'s nickname is now {saved}.");
                CancelRename();
                break;

            case FintechResult.Invalid:
                renameError = "Keep the nickname to 30 characters.";
                focusRename = true;
                break;

            case FintechResult.NotFound:
                renamingId = null;
                SetStatus("That payee is no longer in your payees.");
                break;

            default:
                renameError = "Sign in again to change your payees. Nothing changed.";
                focusRename = true;
                break;
        }
    }

    /// <summary>Shows a message in the status region; a repeat gets a count, so it is announced again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    private void StartRename(FintechPayee payee)
    {
        renamingId = payee.Id;
        nicknameText = payee.Nickname ?? string.Empty;
        renameError = null;
        focusNext = null;
        focusRename = true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets whether the payee in <see cref="Saved" /> was just added (from the query string).</summary>
    [SupplyParameterFromQuery(Name = "added")] public bool? Added { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>Gets or sets the payee the editor just saved (from the query string).</summary>
    [SupplyParameterFromQuery(Name = "saved")] public string? Saved { get; set; }

    [Inject] private FintechState State { get; set; } = default!;

    #endregion
}
