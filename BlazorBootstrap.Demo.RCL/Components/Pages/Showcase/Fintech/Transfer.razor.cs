using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// The transfer form, an <see cref="EditForm" /> (rule WR-002): the account to send from (checking or savings, with what is
/// available after scheduled and held transfers), who to send to (the customer's own accounts and payees, in two groups,
/// with a link to add a payee; not the account it's sent from), the amount (more than $0, at most $50,000, in cents, and at
/// most what's available, what's owed on a card, and what's left of the daily limit for payees on that date, cross-field
/// rules), when (today, or a later date from today up to 60 days ahead), and a note of up to 60 characters, required for a
/// payment of <see cref="FintechState.HoldLargeAmount" /> or more to a payee. Messages show after a field is touched or on
/// submit, with a linked summary on a failed submit. A live hint says, before the customer confirms, whether a payee
/// transfer will be held for a check (mirroring the bank's rules). "Continue" stores the transfer with
/// <see cref="FintechState.StartTransfer" /> (which checks again) and opens the review screen, or shows the bank's message.
/// <c>?from=</c> and <c>?to=</c> preselect the source and the recipient; returning from the review screen ("Edit") restores
/// the transfer waiting there.
/// </summary>
public partial class Transfer : ComponentBase, IDisposable
{
    #region Fields and Constants

    private bool busy;

    private EditContext editContext = default!;

    private string? error;

    private int errorCount;

    /// <summary>The last error without its counter, so a repeat is recognized.</summary>
    private string? errorText;

    private ElementReference errorRegion;

    private bool focusError;

    private TransferForm form = new();

    private int noteLength;

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
            form.FromId = pending.FromAccountId;
            form.ToId = pending.ToId;
            form.Amount = pending.Amount;
            form.IsLater = pending.Date > State.Today;
            form.LaterDate = form.IsLater ? pending.Date : null;
            form.Note = pending.Note ?? string.Empty;
        }
        else
        {
            form.FromId = SourceAccounts.FirstOrDefault()?.Id ?? string.Empty;
        }

        // A link such as "Move money" on an account, or "Pay" on a payee, preselects the source or the recipient.
        if (From is { } from && SourceAccounts.Any(account => account.Id == from))
            form.FromId = from;

        if (To is { } to && (State.FindPayee(to) is not null || State.FindAccount(to) is not null))
        {
            form.ToId = to;

            // Sending to the account preselected as the source: send from the other one instead.
            if (form.ToId == form.FromId)
                form.FromId = SourceAccounts.FirstOrDefault(account => account.Id != form.ToId)?.Id ?? form.FromId;
        }

        if (form.ToId == form.FromId)
            form.ToId = string.Empty;

        noteLength = form.Note?.Length ?? 0;
        editContext = new EditContext(form);
        editContext.OnFieldChanged += OnFieldChanged;
        UpdateLimits();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        editContext.OnFieldChanged -= OnFieldChanged;
    }

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

    /// <summary>
    /// Stores the transfer for the review screen once the form is valid (a failed submit shows the summary); the bank checks
    /// again, and its message shows in the alert.
    /// </summary>
    private Task ContinueAsync()
    {
        // Guards a double submit: the first one stores the transfer and opens the review screen.
        if (busy)
            return Task.CompletedTask;

        UpdateLimits();

        if (!editContext.Validate())
            return Task.CompletedTask;

        busy = true;
        var date = form.IsLater ? form.LaterDate!.Value : State.Today;
        var draft = new FintechTransferDraft(form.FromId, form.ToId, form.Amount ?? 0, date, string.IsNullOrWhiteSpace(form.Note) ? null : form.Note.Trim());

        if (State.StartTransfer(draft) is { } message)
        {
            busy = false;
            ShowError(message);

            return Task.CompletedTask;
        }

        error = null;
        Navigation.NavigateTo($"{FintechLayout.Base}/transfer/confirm");

        return Task.CompletedTask;
    }

    /// <summary>
    /// The limits that depend on the choices are refreshed as they change: an account can't send to itself, a later date
    /// starts tomorrow, and the last error is cleared.
    /// </summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        if (args.FieldIdentifier.FieldName == nameof(TransferForm.FromId) && form.ToId == form.FromId)
        {
            form.ToId = string.Empty;
            editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.ToId));
        }

        if (args.FieldIdentifier.FieldName == nameof(TransferForm.IsLater) && form.IsLater && form.LaterDate is null)
            form.LaterDate = State.Today.AddDays(1);

        error = errorText = null;
        errorCount = 0;
        UpdateLimits();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void ShowError(string message)
    {
        errorCount = message == errorText ? errorCount + 1 : 1;
        errorText = message;
        error = errorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({errorCount})");
        focusError = true;
    }

    /// <summary>Gives the form the figures its cross-field rules check against: today, what's available, what's owed, and the daily limit left.</summary>
    private void UpdateLimits()
    {
        var source = SelectedSource;
        var date = form.IsLater && form.LaterDate is { } later ? later : State.Today;
        var card = State.FindAccount(form.ToId) is { Kind: FintechAccountKind.CreditCard } target ? target : null;

        form.Today = State.Today;
        form.LastDate = LastDate;
        form.SourceName = source?.Name ?? string.Empty;
        form.Available = source is null ? 0 : State.Available(source);
        form.IsPayee = SelectedPayee is not null;
        form.PayeeLimitLeft = State.PayeeLimitLeft(date);
        form.CardName = card?.Name;
        form.CardOwed = card is null ? null : Math.Max(0, -card.Balance - State.MyTransfers()
            .Where(transfer => transfer.ToId == card.Id && transfer.Status is FintechTransferStatus.Scheduled or FintechTransferStatus.Held).Sum(transfer => transfer.Amount));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the account to send from, from the query string.</summary>
    [SupplyParameterFromQuery(Name = "from")] public string? From { get; set; }

    /// <summary>Gets the hint that the transfer will be held for a check, or <see langword="null" />.</summary>
    private string? HoldHint => form.Amount is > 0 && ExpectedHoldReason(State, form.ToId, form.Amount.Value) is { } reason
        ? $"This payment will be held for a routine check before it's sent ({FintechSeedData.LowerFirst(reason)}). We usually check within one business day; the money is set aside meanwhile."
        : null;

    private DateOnly LastDate => State.Today.AddDays(FintechState.MaxScheduleDays);

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>Gets whether the note is required: a payment of <see cref="FintechState.HoldLargeAmount" /> or more to a payee.</summary>
    private bool NoteRequired => SelectedPayee is not null && form.Amount >= FintechState.HoldLargeAmount;

    private FintechPayee? SelectedPayee => State.FindAccount(form.ToId) is null ? State.FindPayee(form.ToId) : null;

    private FintechAccount? SelectedSource => SourceAccounts.FirstOrDefault(account => account.Id == form.FromId);

    private IEnumerable<FintechAccount> SourceAccounts => State.Accounts.Where(account => account.Kind != FintechAccountKind.CreditCard);

    [Inject] private FintechState State { get; set; } = default!;

    /// <summary>Gets or sets the recipient (an account or payee ID), from the query string.</summary>
    [SupplyParameterFromQuery(Name = "to")] public string? To { get; set; }

    #endregion

    #region Nested Types

    /// <summary>
    /// The transfer's fields, with the bank's limits (<see cref="FintechState.ValidateTransfer" />). The figures the
    /// cross-field rules check against (available, owed, daily limit left, today) are set by the page, not by a field.
    /// </summary>
    private sealed class TransferForm : IValidatableObject
    {
        [Required(ErrorMessage = "Enter the amount to send, like $250.00.")]
        [Range(typeof(decimal), "0.01", "50000", ParseLimitsInInvariantCulture = true, ErrorMessage = "Enter an amount from $0.01 to $50,000.00.")]
        public decimal? Amount { get; set; }

        public decimal Available { get; set; }

        public string? CardName { get; set; }

        public decimal? CardOwed { get; set; }

        [Required(ErrorMessage = "Choose a checking or savings account to send from.")]
        public string FromId { get; set; } = string.Empty;

        public bool IsLater { get; set; }

        public bool IsPayee { get; set; }

        public DateOnly LastDate { get; set; }

        public DateOnly? LaterDate { get; set; }

        [StringLength(FintechState.MaxTransferNote, ErrorMessage = "Keep the note to 60 characters.")]
        public string? Note { get; set; }

        public decimal PayeeLimitLeft { get; set; }

        public string SourceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose who to send money to.")]
        public string ToId { get; set; } = string.Empty;

        public DateOnly Today { get; set; }

        /// <summary>Runs once the fields are filled in: the recipient, the amount against the limits, the date, and the note.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ToId == FromId)
                yield return new ValidationResult("Choose a different account to send to: it can't be the one it's sent from.", new[] { nameof(ToId) });

            if (Amount is { } amount)
            {
                if (decimal.Round(amount, 2) != amount)
                    yield return new ValidationResult("Use at most two decimal places.", new[] { nameof(Amount) });
                else if (amount > Available)
                    yield return new ValidationResult($"{SourceName} has {FintechDisplay.Money(Available)} available; send up to that.", new[] { nameof(Amount) });
                else if (CardOwed is { } owed && amount > owed)
                    yield return new ValidationResult(owed == 0 ? $"Nothing more is owed on the {CardName}." : $"You owe {FintechDisplay.Money(owed)} on the {CardName}; pay up to that.", new[] { nameof(Amount) });
                else if (IsPayee && amount > PayeeLimitLeft)
                    yield return new ValidationResult($"Payments to payees are limited to {FintechDisplay.Money(FintechState.DailyPayeeLimit)} a day; {FintechDisplay.Money(PayeeLimitLeft)} is left for that day.", new[] { nameof(Amount) });

                if (IsPayee && amount >= FintechState.HoldLargeAmount && (Note?.Trim().Length ?? 0) < 3)
                    yield return new ValidationResult($"Add a note of at least 3 characters saying what this payment is for: it's {FintechDisplay.Money(FintechState.HoldLargeAmount)} or more to a payee.", new[] { nameof(Note) });
            }

            if (IsLater)
            {
                if (LaterDate is not { } date)
                    yield return new ValidationResult("Choose the date to send it on, or choose Today.", new[] { nameof(LaterDate) });
                else if (date < Today || date > LastDate)
                    yield return new ValidationResult($"Choose a date from today up to {Transfers.DateText(LastDate)}.", new[] { nameof(LaterDate) });
            }
        }
    }

    #endregion
}
