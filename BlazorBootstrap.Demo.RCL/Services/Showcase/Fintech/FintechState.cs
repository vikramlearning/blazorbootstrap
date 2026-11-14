using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

/// <summary>
/// In-memory state of the personal finance site for the current session: accounts, transactions, budgets, and the
/// transfer being reviewed. Nothing is persisted, sent, or moved.
/// </summary>
public sealed class FintechState
{
    #region Fields and Constants

    private readonly List<FintechAccount> accounts = FintechSeedData.Accounts();

    private readonly List<FintechBudget> budgets = FintechSeedData.Budgets();

    private readonly decimal seedNetWorth;

    private readonly List<FintechTransaction> transactions = FintechSeedData.Transactions();

    private bool isDarkTheme;

    private int nextReference = 260417;

    private int nextTransactionId = 9000;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh session.</summary>
    public FintechState() => seedNetWorth = NetWorth;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the recipient name of a transfer target: one of the customer's accounts or a saved payee.</summary>
    public string DestinationName(string toId) =>
        FindAccount(toId)?.Name ?? Payees.FirstOrDefault(payee => payee.Id == toId)?.Name ?? "Unknown recipient";

    /// <summary>
    /// Carries out the pending transfer. A transfer dated today moves money at once; a later date is scheduled and only
    /// listed as pending. Returns <see langword="null" /> when nothing is pending or the draft is no longer valid.
    /// </summary>
    public FintechTransferResult? ExecutePendingTransfer()
    {
        if (PendingTransfer is not { } draft || ValidateTransfer(draft) is not null)
            return null;

        var today = DateOnly.FromDateTime(FintechSeedData.Today);
        var isScheduled = draft.Date > today;
        var when = draft.Date.ToDateTime(new TimeOnly(10, 0)).AddMinutes(transactions.Count % 50);
        var source = FindAccount(draft.FromAccountId)!;
        var toAccount = FindAccount(draft.ToId);
        var toName = DestinationName(draft.ToId);
        var label = string.IsNullOrWhiteSpace(draft.Note) ? string.Empty : $" · {draft.Note.Trim()}";

        transactions.Insert(0, new FintechTransaction(nextTransactionId++, when, source.Id, $"Transfer to {toName}{label}", "transfer", -draft.Amount, isScheduled));

        if (toAccount is not null)
            transactions.Insert(0, new FintechTransaction(nextTransactionId++, when, toAccount.Id, $"Transfer from {source.Name}{label}", "transfer", draft.Amount, isScheduled));

        if (!isScheduled)
        {
            Replace(source with { Balance = source.Balance - draft.Amount });

            if (toAccount is not null)
                Replace(toAccount with { Balance = toAccount.Balance + draft.Amount });
        }

        var result = new FintechTransferResult($"TM-{nextReference++.ToString(CultureInfo.InvariantCulture)}", draft, isScheduled);
        LastTransfer = result;
        PendingTransfer = null;
        Changed?.Invoke();

        return result;
    }

    /// <summary>Finds an account by its ID, or returns <see langword="null" />.</summary>
    public FintechAccount? FindAccount(string? accountId) => accounts.FirstOrDefault(account => account.Id == accountId);

    /// <summary>Finds a category by its ID; unknown IDs fall back to Transfers.</summary>
    public FintechCategory FindCategory(string categoryId) =>
        Categories.FirstOrDefault(category => category.Id == categoryId) ?? Categories.First(category => category.Id == "transfer");

    /// <summary>Records the color mode the theme switcher applied, so charts can redraw in matching colors.</summary>
    public void SetTheme(string? themeName)
    {
        var isDark = string.Equals(themeName, "dark", StringComparison.OrdinalIgnoreCase);

        if (isDarkTheme == isDark)
            return;

        isDarkTheme = isDark;
        ThemeChanged?.Invoke();
    }

    /// <summary>Stores a validated transfer for the confirm screen. Returns the validation message, or <see langword="null" /> when stored.</summary>
    public string? StartTransfer(FintechTransferDraft draft)
    {
        if (ValidateTransfer(draft) is { } error)
            return error;

        PendingTransfer = draft;
        Changed?.Invoke();

        return null;
    }

    /// <summary>Returns this month's spending in a category, as a positive amount (posted and pending).</summary>
    public decimal SpentThisMonth(string categoryId)
    {
        var monthStart = new DateTime(FintechSeedData.Today.Year, FintechSeedData.Today.Month, 1);

        return -transactions
            .Where(item => item.CategoryId == categoryId && item.Date >= monthStart && item.Date.Date <= FintechSeedData.Today && item.Amount < 0)
            .Sum(item => item.Amount);
    }

    /// <summary>Changes a budget's monthly limit.</summary>
    public void UpdateBudget(string categoryId, decimal limit)
    {
        var index = budgets.FindIndex(budget => budget.CategoryId == categoryId);

        if (index < 0 || limit <= 0 || budgets[index].Limit == limit)
            return;

        budgets[index] = budgets[index] with { Limit = Math.Round(limit, 2) };
        Changed?.Invoke();
    }

    /// <summary>Checks a transfer against the rules. Returns a message for the user, or <see langword="null" /> when it is valid.</summary>
    public string? ValidateTransfer(FintechTransferDraft draft)
    {
        var today = DateOnly.FromDateTime(FintechSeedData.Today);

        if (FindAccount(draft.FromAccountId) is not { } source || source.Kind == FintechAccountKind.CreditCard)
            return "Choose a checking or savings account to send from.";

        if (draft.ToId == draft.FromAccountId)
            return "Choose a different account to send to.";

        if (FindAccount(draft.ToId) is null && Payees.All(payee => payee.Id != draft.ToId))
            return "Choose who to send money to.";

        if (draft.Amount <= 0)
            return "Enter an amount greater than $0.00.";

        if (draft.Amount != Math.Round(draft.Amount, 2))
            return "Use at most two decimal places.";

        if (draft.Amount > source.Available)
            return $"{source.Name} has {source.Available.ToString("$#,0.00", CultureInfo.InvariantCulture)} available.";

        if (draft.Date < today || draft.Date > today.AddDays(60))
            return "Choose a date from today up to 60 days ahead.";

        return null;
    }

    #endregion

    #region Private Methods

    private void Replace(FintechAccount account) => accounts[accounts.FindIndex(item => item.Id == account.Id)] = account;

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the customer's accounts.</summary>
    public IReadOnlyList<FintechAccount> Accounts => accounts;

    /// <summary>Gets the monthly budgets.</summary>
    public IReadOnlyList<FintechBudget> Budgets => budgets;

    /// <summary>Gets the spending and income categories.</summary>
    public IReadOnlyList<FintechCategory> Categories { get; } = FintechSeedData.Categories();

    /// <summary>Gets whether the dark color mode is applied.</summary>
    public bool IsDarkTheme => isDarkTheme;

    /// <summary>Gets the result of the last confirmed transfer in this session, if any.</summary>
    public FintechTransferResult? LastTransfer { get; private set; }

    /// <summary>Gets the total of all balances (the credit card counts as debt).</summary>
    public decimal NetWorth => accounts.Sum(account => account.Balance);

    /// <summary>Gets net worth for the last 12 months; the last value is the live <see cref="NetWorth" />.</summary>
    public IReadOnlyList<double> NetWorthHistory => FintechSeedData.NetWorthHistory(seedNetWorth, NetWorth);

    /// <summary>Gets the saved payees.</summary>
    public IReadOnlyList<FintechPayee> Payees { get; } = FintechSeedData.Payees();

    /// <summary>Gets the transfer waiting on the confirm screen, if any.</summary>
    public FintechTransferDraft? PendingTransfer { get; private set; }

    /// <summary>Gets every transaction, newest first.</summary>
    public IReadOnlyList<FintechTransaction> Transactions => transactions;

    #endregion

    #region Events

    /// <summary>Raised when balances, transactions, budgets, or the pending transfer change.</summary>
    public event Action? Changed;

    /// <summary>Raised when the applied color mode changes.</summary>
    public event Action? ThemeChanged;

    #endregion
}
