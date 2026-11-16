using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// The transfer form: the account to send from (checking or savings, with what is available after scheduled and held
/// transfers), who to send to (the customer's own accounts and payees, in two groups, with a link to add a payee), the
/// amount, when (today, or a later date up to 60 days ahead), and a note of up to 60 characters. Live hints show the
/// available amount and, before the customer confirms, whether a payee transfer will be held for a check (mirroring the
/// bank's rules). "Continue" stores the transfer with <see cref="FintechState.StartTransfer" /> and opens the review
/// screen, or shows the bank's message. <c>?from=</c> and <c>?to=</c> preselect the source and the recipient; returning
/// from the review screen ("Edit") restores the transfer waiting there.
/// </summary>
public partial class Transfer : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The longest note on a transfer.</summary>
    private const int NoteMax = 60;

    private decimal? amount;

    private bool busy;

    private string? error;

    private ElementReference errorRegion;

    private bool focusError;

    private string fromId = string.Empty;

    private bool isLater;

    private DateOnly? laterDate;

    private string note = string.Empty;

    private string toId = string.Empty;

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

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;

        if (State.PendingTransfer is { } pending)
        {
            fromId = pending.FromAccountId;
            toId = pending.ToId;
            amount = pending.Amount;
            isLater = pending.Date > State.Today;
            laterDate = isLater ? pending.Date : null;
            note = pending.Note ?? string.Empty;
        }
        else
        {
            fromId = SourceAccounts.FirstOrDefault()?.Id ?? string.Empty;
        }

        // A link such as "Move money" on an account, or "Pay" on a payee, preselects the source or the recipient.
        if (From is { } from && SourceAccounts.Any(account => account.Id == from))
            fromId = from;

        if (To is { } to && (State.FindPayee(to) is not null || State.FindAccount(to) is not null))
        {
            toId = to;

            // Sending to the account preselected as the source: send from the other one instead.
            if (toId == fromId)
                fromId = SourceAccounts.FirstOrDefault(account => account.Id != toId)?.Id ?? fromId;
        }

        if (toId == fromId)
            toId = string.Empty;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>
    /// Returns why a transfer to a payee will be held for a check, or <see langword="null" /> when it won't. Mirrors the
    /// bank's rules in <see cref="FintechState.ConfirmPendingTransfer" />, so the customer knows before confirming:
    /// <see cref="FintechState.HoldLargeAmount" /> or more to any payee, or <see cref="FintechState.HoldNewPayeeAmount" />
    /// or more to a payee added in the last <see cref="FintechState.NewPayeeDays" /> days or whose name couldn't be confirmed.
    /// Transfers between the customer's own accounts are never held.
    /// </summary>
    internal static string? ExpectedHoldReason(FintechState state, string toId, decimal amount)
    {
        if (state.FindAccount(toId) is not null || state.FindPayee(toId) is not { } payee)
            return null;

        return state.HoldReason(payee, amount);
    }

    /// <summary>Returns whether a payee was added in the last <see cref="FintechState.NewPayeeDays" /> days.</summary>
    internal static bool IsNewPayee(FintechState state, FintechPayee payee) => payee.AddedAt > state.Clock.Now.AddDays(-FintechState.NewPayeeDays);

    #endregion

    #region Private Methods

    private static string IsoDate(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>Says how a payee's name check went, so the customer knows why a payment may be held.</summary>
    private static string MatchHint(FintechPayee payee) => payee.Match switch
    {
        FintechPayeeMatch.Match => "Name confirmed.",
        FintechPayeeMatch.CloseMatch => "Name was a close match.",
        FintechPayeeMatch.NoMatch => "Name didn't match when added.",
        _ => "Name couldn't be checked.",
    };

    private static string PayeeOption(FintechPayee payee) =>
        payee.Nickname is { } nickname ? $"{payee.Name} \"{nickname}\" ({payee.MaskedAccount})" : $"{payee.Name} ({payee.MaskedAccount})";

    private void Continue()
    {
        // Guards a double submit: the first one stores the transfer and opens the review screen.
        if (busy)
            return;

        if (isLater && laterDate is null)
        {
            ShowError("Choose the date to send it on, or choose Today.");
            return;
        }

        busy = true;
        var date = isLater ? laterDate!.Value : State.Today;
        var draft = new FintechTransferDraft(fromId, toId, amount ?? 0, date, string.IsNullOrWhiteSpace(note) ? null : note.Trim());

        if (State.StartTransfer(draft) is { } message)
        {
            busy = false;
            ShowError(message);
            return;
        }

        error = null;
        Navigation.NavigateTo($"{FintechLayout.Base}/transfer/confirm");
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void SelectFrom(string accountId)
    {
        fromId = accountId;

        // An account can't send to itself.
        if (toId == fromId)
            toId = string.Empty;
    }

    private void SetLater(bool later)
    {
        isLater = later;

        if (later && laterDate is null)
            laterDate = State.Today.AddDays(1);
    }

    private void ShowError(string message)
    {
        error = message;
        focusError = true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the account to send from, from the query string.</summary>
    [SupplyParameterFromQuery(Name = "from")] public string? From { get; set; }

    /// <summary>Gets the hint that the transfer will be held for a check, or <see langword="null" />.</summary>
    private string? HoldHint => amount is > 0 && ExpectedHoldReason(State, toId, amount.Value) is { } reason
        ? $"This payment will be held for a routine check before it's sent ({reason.ToLowerInvariant()}). We usually check within one business day; the money is set aside meanwhile."
        : null;

    private DateOnly LastDate => State.Today.AddDays(60);

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private bool OverAvailable => amount is > 0 && SelectedSource is { } source && amount.Value > State.Available(source);

    private FintechPayee? SelectedPayee => State.FindAccount(toId) is null ? State.FindPayee(toId) : null;

    private FintechAccount? SelectedSource => SourceAccounts.FirstOrDefault(account => account.Id == fromId);

    private IEnumerable<FintechAccount> SourceAccounts => State.Accounts.Where(account => account.Kind != FintechAccountKind.CreditCard);

    [Inject] private FintechState State { get; set; } = default!;

    /// <summary>Gets or sets the recipient (an account or payee ID), from the query string.</summary>
    [SupplyParameterFromQuery(Name = "to")] public string? To { get; set; }

    #endregion
}
