using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// One of the customer's accounts: the balance, what is available now and how much is set aside for scheduled and held
/// transfers (or, for the credit card, what is owed, the limit, and the credit available), the linked card with its status
/// in text and a freeze (confirmed in a <see cref="Modal" /> that says what stops and what still goes) or unfreeze
/// button, the scheduled and held transfers from the account, and its last 60 days of transactions, each linking to its
/// page. "Move money" opens the transfer form from this account. An unknown account shows a not-found screen.
/// </summary>
public partial class AccountDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How many days of transactions the page lists.</summary>
    private const int TransactionDays = 60;

    private Button? cardButton;

    private string? dialogError;

    private bool focusStatus;

    /// <summary>Whether the freeze dialog closed after freezing, so focus goes to the status message.</summary>
    private bool freezeDone;

    private Modal freezeModal = default!;

    private string? loadedId;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference statusRegion;

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

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (loadedId == Id)
            return;

        loadedId = Id;
        statusMessage = null;
        statusIsError = false;
        dialogError = null;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Returns the name of the card linked to an account: the checking account's debit card or the credit card.</summary>
    private static string CardName(FintechAccount account) => account.Kind == FintechAccountKind.CreditCard ? "Credit card" : "Debit card";

    /// <summary>Formats a date with its weekday, like "Tue, Dec 1".</summary>
    private static string LongDay(DateOnly date) => date.ToString("ddd, MMM d", CultureInfo.InvariantCulture);

    /// <summary>Returns the message for a refused freeze or unfreeze.</summary>
    private static string ResultText(FintechResult result, bool frozen) => result switch
    {
        FintechResult.NoChange => frozen ? "The card is already frozen." : "The card is already active.",
        FintechResult.NotFound => "This account has no card to freeze or unfreeze, so nothing changed.",
        FintechResult.NotAllowed => "Sign in again with your password and a one-time code to change the card. Nothing changed.",
        _ => "That didn't work, and nothing changed. Try again.",
    };

    private static string TransactionPath(int transactionId) => string.Create(CultureInfo.InvariantCulture, $"{FintechLayout.Base}/transactions/{transactionId}");

    private static string TransferPath(string reference) => $"{FintechLayout.Base}/transfers/{Uri.EscapeDataString(reference)}";

    private async Task AskFreezeAsync()
    {
        dialogError = null;
        freezeDone = false;

        await freezeModal.ShowAsync();
    }

    private async Task ConfirmFreezeAsync()
    {
        if (CurrentAccount is not { CardLast4: { } last4 } account)
            return;

        var result = State.SetCardFrozen(account.Id, true);

        if (result is not (FintechResult.Ok or FintechResult.NoChange))
        {
            dialogError = ResultText(result, true);
            return;
        }

        freezeDone = true;
        statusIsError = false;
        statusMessage = result == FintechResult.Ok
            ? $"Your {CardName(account).ToLowerInvariant()} ending {last4} is frozen. New card payments are declined until you unfreeze it."
            : ResultText(result, true);

        await freezeModal.HideAsync();
    }

    /// <summary>
    /// The dialog opens from code, so Bootstrap does not return focus: after freezing, focus goes to the status message;
    /// otherwise back to the freeze button.
    /// </summary>
    private async Task OnFreezeHiddenAsync()
    {
        dialogError = null;

        if (freezeDone)
        {
            freezeDone = false;
            StateHasChanged();
            await FintechDisplay.FocusAsync(statusRegion);
        }
        else
        {
            await FintechDisplay.FocusAsync(cardButton?.Element);
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Unfreezing only lets payments through again, so it needs no confirmation; the outcome is announced.</summary>
    private void Unfreeze()
    {
        if (CurrentAccount is not { CardLast4: { } last4 } account)
            return;

        var result = State.SetCardFrozen(account.Id, false);

        statusIsError = result is not (FintechResult.Ok or FintechResult.NoChange);
        statusMessage = result == FintechResult.Ok
            ? $"Your {CardName(account).ToLowerInvariant()} ending {last4} is unfrozen and works again."
            : ResultText(result, false);
        focusStatus = true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Overview", Href = FintechLayout.Base },
        new() { Text = CurrentAccount?.Name ?? "Account", IsCurrentPage = true },
    };

    /// <summary>Gets the scheduled and held transfers from this account, held ones first, then by date.</summary>
    private IReadOnlyList<FintechTransfer> ComingUp => State.MyTransfers()
        .Where(transfer => transfer.FromAccountId == Id && transfer.Status is FintechTransferStatus.Scheduled or FintechTransferStatus.Held)
        .OrderBy(transfer => transfer.Status == FintechTransferStatus.Held ? 0 : 1)
        .ThenBy(transfer => transfer.Date)
        .ToList();

    private FintechAccount? CurrentAccount => State.FindAccount(Id);

    /// <summary>Gets or sets the account ID from the route.</summary>
    [Parameter] public string Id { get; set; } = string.Empty;

    /// <summary>Gets the account's transactions of the last 60 days on the site's clock, newest first.</summary>
    private IReadOnlyList<FintechTransaction> RecentTransactions
    {
        get
        {
            var since = State.Today.ToDateTime(TimeOnly.MinValue).AddDays(-TransactionDays + 1);

            return State.Transactions
                .Where(item => item.AccountId == Id && item.Date >= since)
                .OrderByDescending(item => item.Date)
                .ThenByDescending(item => item.Id)
                .ToList();
        }
    }

    [Inject] private FintechState State { get; set; } = default!;

    #endregion
}
