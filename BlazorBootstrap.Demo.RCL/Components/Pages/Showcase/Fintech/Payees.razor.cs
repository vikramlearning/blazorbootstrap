using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// The customer's payees in a <see cref="Grid{TItem}" />, by name: the name (linking to the editor; the whole row opens it)
/// with a "New" badge for the first <see cref="FintechState.NewPayeeDays" /> days (larger transfers to it are held for a
/// check), the nickname, bank, masked account, the name-check result in words, and when it was added, with sorting, paging,
/// the column chooser, and CSV export; the grid's view is saved for the visit and can be reset. Each payee can be paid (the
/// transfer form with it preselected), given a nickname in a dialog (an <see cref="EditForm" />, rule WR-002: up to 30
/// characters and unique among the payees, checked as you type; no code, since it doesn't change where money goes), or
/// deleted after a confirmation; a payee with a scheduled or held transfer can't be deleted, and the dialog says why. The
/// status region reports every action and a save from the editor (<see cref="Saved" />).
/// </summary>
public partial class Payees : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 10;

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

    /// <summary>Set when "Go to transfers" was chosen, so the page navigates once the dialog is hidden.</summary>
    private bool goToTransfers;

    private Grid<PayeeRow>? grid;

    private int gridVersion;

    private ShowcaseUniqueCheck? nicknameCheck;

    private int pageSize = DefaultPageSize;

    private EditContext renameContext = default!;

    /// <summary>Set when the nickname was saved, so focus moves to the status message once the dialog is hidden.</summary>
    private bool renameDone;

    private string? renameError;

    private NicknameForm renameForm = new();

    private InputText? renameInput;

    private Modal renameModal = default!;

    /// <summary>The payee whose nickname the dialog changes, captured when it opens.</summary>
    private FintechPayee? renaming;

    /// <summary>The save message from the editor, shown after the first render so the status region announces it.</summary>
    private string? savedMessage;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        await view.LoadAsync();
        pageSize = (await view.GetSettingsAsync()).PageSize;
        viewLoaded = true;

        // The router focuses the heading after navigation; a message added now is announced by the status region.
        if (savedMessage is not null)
        {
            SetStatus(savedMessage);
            savedMessage = null;
        }

        StateHasChanged();
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "payees", DefaultPageSize, "name", SortDirection.Ascending);
        renameContext = new EditContext(renameForm);
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

    private async Task AskDeleteAsync(string payeeId)
    {
        if (State.FindPayee(payeeId) is not { } payee)
            return;

        deleting = payee;
        deleteBlocked = HasWaitingTransfer(payee.Id);
        dialogError = null;
        actionDone = false;
        deleteBusy = false;
        goToTransfers = false;

        await deleteModal.ShowAsync();
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

    private Task<bool> IsNicknameTakenAsync(string nickname) => Task.FromResult(State.IsNicknameTaken(nickname, renaming?.Id));

    private string NicknameTakenMessage(string nickname) =>
        $"\"{nickname.Trim()}\" is already the nickname of {State.Payees.FirstOrDefault(payee => payee.Id != renaming?.Id && string.Equals(payee.Nickname, nickname.Trim(), StringComparison.OrdinalIgnoreCase))?.Name ?? "another payee"}. Choose a different one.";

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

    /// <summary>After a save, focus goes to the status message; after Cancel, back to the payee's Nickname button.</summary>
    private async Task OnRenameHiddenAsync()
    {
        var payeeId = renaming?.Id;
        renaming = null;
        renameError = null;

        if (renameDone)
        {
            renameDone = false;
            StateHasChanged();
            await Task.Yield();
            await FintechDisplay.FocusAsync(statusRegion);
        }
        else if (payeeId is not null && renameButtons.TryGetValue(payeeId, out var button))
        {
            await FintechDisplay.FocusAsync(button);
        }
    }

    private Task OnRenameShownAsync() => FintechDisplay.FocusAsync(renameInput?.Element);

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private Task<GridDataProviderResult<PayeeRow>> ProvideRowsAsync(GridDataProviderRequest<PayeeRow> request) =>
        view.ApplyAsync(request, State.Payees.Select(payee => new PayeeRow(payee.Id, payee.Name, Transfer.IsNewPayee(State, payee), payee.Nickname ?? string.Empty, payee.BankName,
            payee.MaskedAccount, MatchText(payee.Match), payee.Match, payee.AddedAt)).ToList());

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        SetStatus("The view is reset: by name, 10 payees a page, and every column shown.");
    }

    private async Task SaveNicknameAsync()
    {
        if (renaming is not { } payee)
            return;

        renameError = null;
        var free = nicknameCheck is null || await nicknameCheck.CheckNowAsync();

        if (!await renameContext.ValidateAsync() || !free)
            return;

        var result = State.SetPayeeNickname(payee.Id, renameForm.Nickname);

        switch (result)
        {
            case FintechResult.Ok:
            case FintechResult.NoChange:
                var saved = State.FindPayee(payee.Id)?.Nickname;
                SetStatus(result == FintechResult.NoChange
                    ? $"Nothing changed: {payee.Name}'s nickname is the same."
                    : saved is null ? $"{payee.Name}'s nickname is removed." : $"{payee.Name}'s nickname is now {saved}.");
                renameDone = true;
                await renameModal.HideAsync();
                break;

            case FintechResult.Duplicate:
                renameError = NicknameTakenMessage(renameForm.Nickname ?? string.Empty);
                break;

            case FintechResult.Invalid:
                renameError = $"Keep the nickname to {FintechState.MaxNicknameLength} characters.";
                break;

            case FintechResult.NotFound:
                SetStatus("That payee is no longer in your payees.");
                renameDone = true;
                await renameModal.HideAsync();
                break;

            default:
                renameError = "Sign in again to change your payees. Nothing changed.";
                break;
        }
    }

    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a message in the status region; a repeat gets a count, so it is announced again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    private async Task StartRenameAsync(string payeeId)
    {
        if (State.FindPayee(payeeId) is not { } payee)
            return;

        renaming = payee;
        renameForm = new NicknameForm { Nickname = payee.Nickname ?? string.Empty };
        renameContext = new EditContext(renameForm);
        renameError = null;
        renameDone = false;

        await renameModal.ShowAsync();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets whether the payee in <see cref="Saved" /> was just added (from the query string).</summary>
    [SupplyParameterFromQuery(Name = "added")] public bool? Added { get; set; }

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>Gets or sets the payee the editor just saved (from the query string).</summary>
    [SupplyParameterFromQuery(Name = "saved")] public string? Saved { get; set; }

    [Inject] private FintechState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The nickname dialog's field: optional, up to 30 characters (uniqueness is checked as you type).</summary>
    private sealed class NicknameForm
    {
        [StringLength(FintechState.MaxNicknameLength, ErrorMessage = "Keep the nickname to 30 characters.")]
        public string? Nickname { get; set; }
    }

    /// <summary>One payee as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record PayeeRow(string Id, string Name, bool IsNew, string Nickname, string Bank, string Account, string NameCheck, FintechPayeeMatch Match, DateTime Added);

    #endregion
}
