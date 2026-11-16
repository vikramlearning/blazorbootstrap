using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

/// <summary>A one-time code waiting to be entered, shown in the demo as the text message the customer would get.</summary>
public sealed record FintechCodeChallenge(FintechCodePurpose Purpose, string Code, DateTime ExpiresAt, int AttemptsLeft);

/// <summary>
/// State of the "Tallymint" bank for the current browser tab: the customer's accounts and cards, transactions, transfers,
/// payees, budgets, disputes, alerts, and the demo sign-in with its one-time-code step. It is kept in the tab's
/// <c>sessionStorage</c> through the showcase foundation, and its clock moves on from the seed time (D-029), so scheduled
/// transfers post on their date. No money moves and nothing is sent.
/// </summary>
public sealed partial class FintechState : IShowcaseSiteState
{
    #region Fields and Constants

    /// <summary>How many wrong codes are allowed before a new code is needed.</summary>
    public const int CodeAttempts = 5;

    /// <summary>How long a one-time code is valid, in minutes.</summary>
    public const int CodeMinutes = 5;

    /// <summary>How far back a transaction can be disputed, in days.</summary>
    public const int DisputeDays = 120;

    /// <summary>The amount from which a transfer to any payee is held for a check.</summary>
    public const decimal HoldLargeAmount = 2000m;

    /// <summary>The amount from which a transfer to a new or unconfirmed payee is held for a check.</summary>
    public const decimal HoldNewPayeeAmount = 500m;

    /// <summary>The longest dispute explanation.</summary>
    public const int MaxDisputeDetails = 1000;

    /// <summary>The longest transaction note.</summary>
    public const int MaxNoteLength = 200;

    /// <summary>The most saved payees.</summary>
    public const int MaxPayees = 30;

    /// <summary>A payee added within this many days is new: its larger transfers are held for a check.</summary>
    public const int NewPayeeDays = 7;

    private const decimal MaxMoney = 10_000_000m;

    private const int MaxNotices = 100;

    private const int MaxTransactions = 2000;

    private const int MaxTransfers = 500;

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<FintechAccount> accounts = FintechSeedData.Accounts();

    private readonly List<FintechBudget> budgets = FintechSeedData.Budgets();

    private readonly List<FintechDispute> disputes;

    private readonly List<FintechNotice> notices = FintechSeedData.Notices();

    private readonly List<FintechPayee> payees = FintechSeedData.Payees();

    private readonly decimal seedNetWorth;

    private readonly List<FintechTransaction> transactions = FintechSeedData.Transactions();

    private readonly List<FintechTransfer> transfers = FintechSeedData.Transfers();

    private FintechCodeChallenge? challenge;

    private int challengeAttempts;

    private bool isDarkTheme;

    private int nextDisputeId = 3102;

    private int nextNoticeId = 4;

    private int nextPayeeId = 1;

    private int nextReference = 260417;

    private int nextTransactionId = 9000;

    private string? verifiedAccountId;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out.</summary>
    public FintechState()
    {
        seedNetWorth = NetWorth;
        disputes = FintechSeedData.Disputes(transactions);

        foreach (var dispute in disputes)
            SetTransaction(dispute.TransactionId, item => item with { DisputeId = dispute.Id });

        Identity = new ShowcaseIdentity(FintechSeedData.Roles(), FintechSeedData.DemoAccounts());
        Identity.Changed += OnIdentityChanged;
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Adds a monthly budget for a spending category that has none (limit $1 to $100,000).</summary>
    public FintechResult AddBudget(string categoryId, decimal limit)
    {
        if (!IsVerified)
            return FintechResult.NotAllowed;

        if (Categories.FirstOrDefault(category => category.Id == categoryId) is not { IsSpending: true } || !ValidLimit(limit))
            return FintechResult.Invalid;

        if (budgets.Any(budget => budget.CategoryId == categoryId))
            return FintechResult.Duplicate;

        budgets.Add(new FintechBudget(categoryId, limit));
        Changed?.Invoke();

        return FintechResult.Ok;
    }

    /// <summary>Returns the money an account can send now: its balance less scheduled and held transfers out of it. A credit card can't send money.</summary>
    public decimal Available(FintechAccount account) =>
        account.Kind == FintechAccountKind.CreditCard ? 0 : Math.Max(0, account.Balance - Reserved(account.Id));

    /// <summary>Cancels one of the customer's scheduled or held transfers; the money it set aside is free again.</summary>
    public FintechResult CancelTransfer(string reference)
    {
        if (!IsVerified)
            return FintechResult.NotAllowed;

        CatchUp();

        if (FindTransfer(reference) is not { } transfer)
            return FintechResult.NotFound;

        if (transfer.Status is not (FintechTransferStatus.Scheduled or FintechTransferStatus.Held))
            return FintechResult.WrongState;

        transfer.Status = FintechTransferStatus.Cancelled;
        transfer.ClosedAt = Clock.Now;
        transfer.HoldReason = null;
        Notify(FintechAlertKind.Transfer, "Transfer cancelled", $"Your {Money(transfer.Amount)} transfer to {transfer.ToName} is cancelled. The money stays in {AccountName(transfer.FromAccountId)}.", TransferPath(transfer));
        Changed?.Invoke();

        return FintechResult.Ok;
    }

    /// <summary>Returns why a transaction can't be disputed, or <see langword="null" /> when it can.</summary>
    public string? CannotDispute(FintechTransaction transaction)
    {
        if (transaction.DisputeId is not null)
            return "This transaction is already disputed.";

        if (transaction.Amount >= 0)
            return "Only money paid out can be disputed.";

        if (transaction.CategoryId is "transfer" or "income")
            return "Transfers you made can't be disputed; cancel a scheduled transfer instead.";

        if (transaction.IsPending)
            return "Pending transactions can't be disputed yet. Wait until it posts, usually within 2 days.";

        if (transaction.Date < Clock.Now.AddDays(-DisputeDays))
            return $"Transactions older than {DisputeDays} days can't be disputed.";

        return null;
    }

    /// <summary>
    /// Posts scheduled transfers whose date has come: the money moves, or, when the account no longer has enough, the
    /// transfer is rejected. Called after a restore and as pages load, so the moving clock (D-029) is applied.
    /// </summary>
    public void CatchUp()
    {
        var changed = false;

        foreach (var transfer in transfers.Where(transfer => transfer.Status == FintechTransferStatus.Scheduled && transfer.Date.ToDateTime(new TimeOnly(6, 0)) <= Clock.Now)
                     .OrderBy(transfer => transfer.Date).ToList())
        {
            var source = FindAccount(transfer.FromAccountId);

            if (source is null || source.Balance - Reserved(source.Id, transfer.Reference) < transfer.Amount)
            {
                transfer.Status = FintechTransferStatus.Rejected;
                transfer.RejectReason = "There wasn't enough money in the account on the transfer date.";
                transfer.ClosedAt = transfer.Date.ToDateTime(new TimeOnly(6, 0));
                Notify(FintechAlertKind.Transfer, "Scheduled transfer not sent", $"Your {Money(transfer.Amount)} transfer to {transfer.ToName} wasn't sent: there wasn't enough money on {Day(transfer.Date)}.", TransferPath(transfer));
            }
            else
            {
                Post(transfer, transfer.Date.ToDateTime(new TimeOnly(6, 0)));
                Notify(FintechAlertKind.Transfer, "Scheduled transfer sent", $"{Money(transfer.Amount)} went to {transfer.ToName} on {Day(transfer.Date)}.", TransferPath(transfer));
            }

            changed = true;
        }

        if (changed)
            Changed?.Invoke();
    }

    /// <summary>
    /// Checks a payee's name against the account (confirmation of payee) without saving anything. The demo directory
    /// knows a few fictitious accounts; any other account can't be checked.
    /// </summary>
    public FintechPayeeCheck CheckPayee(FintechPayeeDraft draft)
    {
        var routing = Digits(draft.RoutingNumber);
        var account = Digits(draft.AccountNumber);
        var bank = routing.Length == 9 ? $"Bank with routing number ending {routing[^4..]}" : "Unknown bank";

        if (!FintechSeedData.PayeeDirectory().TryGetValue(account, out var holder))
            return new FintechPayeeCheck(FintechPayeeMatch.Unavailable, null, bank);

        var typed = NormalizeName(draft.Name);
        var actual = NormalizeName(holder);

        if (typed == actual)
            return new FintechPayeeCheck(FintechPayeeMatch.Match, holder, bank);

        var typedParts = typed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var actualParts = actual.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Close: the same surname and first initial, or the same first name and a surname one or two letters off (a typo).
        var close = typedParts.Length > 0 && actualParts.Length > 0
            && ((typedParts[^1] == actualParts[^1] && typedParts[0][0] == actualParts[0][0])
                || (typedParts[0] == actualParts[0] && typedParts.Length > 1 && actualParts.Length > 1 && EditDistance(typedParts[^1], actualParts[^1]) <= 2));

        return close
            ? new FintechPayeeCheck(FintechPayeeMatch.CloseMatch, holder, bank)
            : new FintechPayeeCheck(FintechPayeeMatch.NoMatch, null, bank);
    }

    /// <summary>Returns the code waiting for a purpose (the demo shows it as the text message), or <see langword="null" />.</summary>
    public FintechCodeChallenge? Challenge(FintechCodePurpose purpose) =>
        challenge is { } current && current.Purpose == purpose && current.ExpiresAt > Clock.Now ? current with { AttemptsLeft = CodeAttempts - challengeAttempts } : null;

    /// <summary>
    /// Confirms the transfer waiting on the review screen. A later date schedules it; a large transfer to a payee, or a
    /// smaller one to a new or unconfirmed payee, is held for a check by bank operations; anything else moves at once.
    /// </summary>
    public FintechResult ConfirmPendingTransfer(out FintechTransfer? transfer)
    {
        transfer = null;

        if (!IsVerified || Identity.Current is not { } customer)
            return FintechResult.NotAllowed;

        if (PendingTransfer is not { } draft)
            return FintechResult.NotFound;

        CatchUp();

        if (ValidateTransfer(draft) is not null)
            return FintechResult.Invalid;

        if (transfers.Count >= MaxTransfers)
            return FintechResult.LimitReached;

        var toAccount = FindAccount(draft.ToId);
        var payee = FindPayee(draft.ToId);
        var holdReason = toAccount is null && payee is not null ? HoldReason(payee, draft.Amount) : null;

        transfer = new FintechTransfer
        {
            Reference = string.Create(CultureInfo.InvariantCulture, $"TM-{nextReference++}"),
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            FromAccountId = draft.FromAccountId,
            ToId = draft.ToId,
            ToName = DestinationName(draft.ToId),
            ToOwnAccount = toAccount is not null,
            Amount = draft.Amount,
            Date = draft.Date,
            Note = string.IsNullOrWhiteSpace(draft.Note) ? null : draft.Note.Trim(),
            CreatedAt = Clock.Now,
            Status = holdReason is not null ? FintechTransferStatus.Held : draft.Date > Today ? FintechTransferStatus.Scheduled : FintechTransferStatus.Completed,
            HoldReason = holdReason,
        };

        transfers.Insert(0, transfer);
        PendingTransfer = null;

        switch (transfer.Status)
        {
            case FintechTransferStatus.Completed:
                Post(transfer, Clock.Now);
                break;

            case FintechTransferStatus.Held:
                Notify(FintechAlertKind.Transfer, "Transfer held for a check", $"Your {Money(transfer.Amount)} transfer to {transfer.ToName} is held for a routine check ({holdReason!.ToLowerInvariant()}). We'll update you within one business day.", TransferPath(transfer));
                break;

            default:
                Notify(FintechAlertKind.Transfer, "Transfer scheduled", $"{Money(transfer.Amount)} to {transfer.ToName} goes on {Day(transfer.Date)}. The money is set aside until then.", TransferPath(transfer));
                break;
        }

        LastTransferReference = transfer.Reference;
        Changed?.Invoke();

        return FintechResult.Ok;
    }

    /// <summary>Returns the snapshot of the bank: accounts, transactions, transfers, payees, budgets, disputes, alerts, the code check, and the ID counters.</summary>
    public object CaptureData() => new FintechSnapshot
    {
        Accounts = accounts.ToList(),
        AlertSettings = AlertSettings,
        Budgets = budgets.ToList(),
        Disputes = disputes.ToList(),
        NextDisputeId = nextDisputeId,
        NextNoticeId = nextNoticeId,
        NextPayeeId = nextPayeeId,
        NextReference = nextReference,
        NextTransactionId = nextTransactionId,
        Notices = notices.ToList(),
        Payees = payees.ToList(),
        Transactions = transactions.ToList(),
        Transfers = transfers.ToList(),
        VerifiedAccountId = verifiedAccountId,
    };

    /// <summary>Returns the credit still available on a credit card: its limit less what is owed.</summary>
    public static decimal CreditAvailable(FintechAccount card) => card.CreditLimit is { } limit ? Math.Max(0, limit + card.Balance) : 0;

    /// <summary>Removes a budget.</summary>
    public FintechResult DeleteBudget(string categoryId)
    {
        if (!IsVerified)
            return FintechResult.NotAllowed;

        if (budgets.RemoveAll(budget => budget.CategoryId == categoryId) == 0)
            return FintechResult.NotFound;

        Changed?.Invoke();

        return FintechResult.Ok;
    }

    /// <summary>Removes a payee that has no scheduled or held transfer waiting.</summary>
    public FintechResult DeletePayee(string payeeId)
    {
        if (!IsVerified)
            return FintechResult.NotAllowed;

        if (FindPayee(payeeId) is not { } payee)
            return FintechResult.NotFound;

        if (transfers.Any(transfer => transfer.ToId == payeeId && transfer.Status is FintechTransferStatus.Scheduled or FintechTransferStatus.Held))
            return FintechResult.WrongState;

        payees.Remove(payee);
        Changed?.Invoke();

        return FintechResult.Ok;
    }

    /// <summary>Returns the recipient name of a transfer target: one of the customer's accounts or a saved payee.</summary>
    public string DestinationName(string toId) =>
        FindAccount(toId)?.Name ?? FindPayee(toId)?.Name ?? "Unknown recipient";

    /// <summary>Finds an account by its ID, or returns <see langword="null" />.</summary>
    public FintechAccount? FindAccount(string? accountId) => accounts.FirstOrDefault(account => account.Id == accountId);

    /// <summary>Finds a category by its ID; unknown IDs fall back to Transfers.</summary>
    public FintechCategory FindCategory(string categoryId) =>
        Categories.FirstOrDefault(category => category.Id == categoryId) ?? Categories.First(category => category.Id == "transfer");

    /// <summary>Returns one of the customer's disputes, or <see langword="null" />.</summary>
    public FintechDispute? FindDispute(int disputeId) => IsVerified ? disputes.FirstOrDefault(dispute => dispute.Id == disputeId && dispute.CustomerId == verifiedAccountId) : null;

    /// <summary>Finds a saved payee by its ID, or returns <see langword="null" />.</summary>
    public FintechPayee? FindPayee(string? payeeId) => payees.FirstOrDefault(payee => payee.Id == payeeId);

    /// <summary>Finds a transaction by its ID, or returns <see langword="null" />.</summary>
    public FintechTransaction? FindTransaction(int transactionId) => transactions.FirstOrDefault(item => item.Id == transactionId);

    /// <summary>Returns one of the customer's transfers by reference, or <see langword="null" />.</summary>
    public FintechTransfer? FindTransfer(string? reference) =>
        IsVerified ? transfers.FirstOrDefault(transfer => transfer.Reference == reference && transfer.CustomerId == verifiedAccountId) : null;

    /// <summary>Marks every notice as read.</summary>
    public void MarkAllNoticesRead()
    {
        if (!IsVerified || notices.All(notice => notice.IsRead))
            return;

        notices.ForEach(notice => notice.IsRead = true);
        Changed?.Invoke();
    }

    /// <summary>Marks one notice as read.</summary>
    public void MarkNoticeRead(int noticeId)
    {
        if (IsVerified && notices.FirstOrDefault(notice => notice.Id == noticeId) is { IsRead: false } notice)
        {
            notice.IsRead = true;
            Changed?.Invoke();
        }
    }

    /// <summary>Returns the customer's disputes, newest first.</summary>
    public IReadOnlyList<FintechDispute> MyDisputes() =>
        IsVerified ? disputes.Where(dispute => dispute.CustomerId == verifiedAccountId).OrderByDescending(dispute => dispute.CreatedAt).ToList() : Array.Empty<FintechDispute>();

    /// <summary>Returns the customer's transfers, newest first.</summary>
    public IReadOnlyList<FintechTransfer> MyTransfers() =>
        IsVerified ? transfers.Where(transfer => transfer.CustomerId == verifiedAccountId).OrderByDescending(transfer => transfer.CreatedAt).ToList() : Array.Empty<FintechTransfer>();

    /// <summary>Applies the moving clock after a snapshot was restored, then raises <see cref="Changed" />.</summary>
    public void NotifyRestored()
    {
        // The identity is applied after the data; a verification for another (or no) account doesn't carry over.
        if (Identity.Current?.Id != verifiedAccountId)
            verifiedAccountId = null;

        CatchUp();
        Changed?.Invoke();
    }

    /// <summary>
    /// Opens a dispute of a card or account payment (see <see cref="CannotDispute" />). The explanation is 10 to
    /// <see cref="MaxDisputeDetails" /> characters.
    /// </summary>
    public FintechResult OpenDispute(int transactionId, FintechDisputeReason reason, string details, out FintechDispute? dispute)
    {
        dispute = null;

        if (!IsVerified)
            return FintechResult.NotAllowed;

        if (FindTransaction(transactionId) is not { } transaction)
            return FintechResult.NotFound;

        if (CannotDispute(transaction) is not null)
            return FintechResult.WrongState;

        details = details?.Trim() ?? string.Empty;

        if (!Enum.IsDefined(reason) || details.Length is < 10 or > MaxDisputeDetails)
            return FintechResult.Invalid;

        dispute = new FintechDispute
        {
            Id = nextDisputeId++,
            CustomerId = verifiedAccountId!,
            TransactionId = transaction.Id,
            Merchant = transaction.Description,
            Amount = -transaction.Amount,
            Reason = reason,
            Details = details,
            CreatedAt = Clock.Now,
            Status = FintechDisputeStatus.Submitted,
        };
        disputes.Add(dispute);
        var disputeId = dispute.Id;
        SetTransaction(transaction.Id, item => item with { DisputeId = disputeId });
        Notify(FintechAlertKind.Dispute, $"Dispute {dispute.Id} received", $"We've received your dispute of {Money(dispute.Amount)} at {dispute.Merchant}. We'll update you within 10 business days.",
            $"/showcase/fintech/disputes/{dispute.Id.ToString(CultureInfo.InvariantCulture)}");
        Changed?.Invoke();

        return FintechResult.Ok;
    }

    /// <summary>Changes a transaction's category; budgets follow.</summary>
    public FintechResult Recategorize(int transactionId, string categoryId)
    {
        if (!IsVerified)
            return FintechResult.NotAllowed;

        if (FindTransaction(transactionId) is not { } transaction)
            return FintechResult.NotFound;

        if (Categories.All(category => category.Id != categoryId) || transaction.CategoryId is "transfer" || categoryId is "transfer")
            return FintechResult.Invalid;

        if (transaction.CategoryId == categoryId)
            return FintechResult.NoChange;

        var before = BudgetShare(categoryId);
        SetTransaction(transactionId, item => item with { CategoryId = categoryId });
        CheckBudget(categoryId, before);
        Changed?.Invoke();

        return FintechResult.Ok;
    }

    /// <summary>
    /// Sends a new one-time code for a purpose (the demo shows it as a text message): after the password at sign-in, or
    /// before a payee is saved. A new code replaces the previous one.
    /// </summary>
    public FintechResult RequestCode(FintechCodePurpose purpose)
    {
        if (Identity.Current is not { RoleId: FintechSeedData.CustomerRole } || !Enum.IsDefined(purpose) || (purpose != FintechCodePurpose.SignIn && !IsVerified))
            return FintechResult.NotAllowed;

        challenge = new FintechCodeChallenge(purpose, RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", CultureInfo.InvariantCulture), Clock.Now.AddMinutes(CodeMinutes), CodeAttempts);
        challengeAttempts = 0;
        Changed?.Invoke();

        return FintechResult.Ok;
    }

    /// <summary>Returns the account a transaction or transfer reserves money from, and how much: scheduled and held transfers out of an account.</summary>
    public decimal Reserved(string accountId, string? exceptReference = null) =>
        transfers.Where(transfer => transfer.FromAccountId == accountId && transfer.Reference != exceptReference && transfer.Status is FintechTransferStatus.Scheduled or FintechTransferStatus.Held)
            .Sum(transfer => transfer.Amount);

    /// <summary>
    /// Adds a payee (<paramref name="payeeId" /> <see langword="null" />) or changes one, after the one-time code for payees
    /// is entered. The name is checked against the account first; a payee whose name doesn't match is only saved when
    /// <paramref name="acceptMismatch" /> confirms the customer still wants to (WrongState otherwise).
    /// </summary>
    public FintechResult SavePayee(string? payeeId, FintechPayeeDraft draft, string code, bool acceptMismatch, out FintechPayee? payee)
    {
        payee = null;

        if (!IsVerified)
            return FintechResult.NotAllowed;

        var existing = payeeId is null ? null : FindPayee(payeeId);

        if (payeeId is not null && existing is null)
            return FintechResult.NotFound;

        var name = draft.Name?.Trim() ?? string.Empty;
        var nickname = string.IsNullOrWhiteSpace(draft.Nickname) ? null : draft.Nickname.Trim();
        var routing = Digits(draft.RoutingNumber);
        var account = Digits(draft.AccountNumber);

        if (!IsText(name, 60) || (nickname is { Length: > 30 }) || routing.Length != 9 || account.Length is < 4 or > 17
            || draft.RoutingNumber.Any(c => !char.IsAsciiDigit(c) && c != ' ' && c != '-') || draft.AccountNumber.Any(c => !char.IsAsciiDigit(c) && c != ' ' && c != '-'))
            return FintechResult.Invalid;

        if (payees.Any(other => other.Id != payeeId && string.Equals(other.Name, name, StringComparison.OrdinalIgnoreCase)))
            return FintechResult.Duplicate;

        if (existing is null && payees.Count >= MaxPayees)
            return FintechResult.LimitReached;

        if (existing is not null && transfers.Any(transfer => transfer.ToId == existing.Id && transfer.Status is FintechTransferStatus.Scheduled or FintechTransferStatus.Held))
            return FintechResult.InUse;

        var check = CheckPayee(draft);

        if (check.Match == FintechPayeeMatch.NoMatch && !acceptMismatch)
            return FintechResult.WrongState;

        if (!TryCode(FintechCodePurpose.Payee, code))
            return FintechResult.WrongCode;

        payee = new FintechPayee(existing?.Id ?? FormattableString.Invariant($"payee-{nextPayeeId++}"), name, $"•••• {account[^4..]}")
        {
            AddedAt = Clock.Now,
            BankName = check.BankName,
            Match = check.Match,
            Nickname = nickname,
        };

        if (existing is null)
            payees.Add(payee);
        else
            payees[payees.IndexOf(existing)] = payee;

        Notify(FintechAlertKind.Security, existing is null ? "New payee added" : "Payee changed",
            $"{payee.Name} ({payee.MaskedAccount}) was {(existing is null ? "added to" : "changed in")} your payees. If this wasn't you, contact us straight away.", "/showcase/fintech/payees");
        Changed?.Invoke();

        return FintechResult.Ok;
    }

    /// <summary>Freezes or unfreezes the card linked to an account: while frozen, new card payments are declined.</summary>
    public FintechResult SetCardFrozen(string accountId, bool frozen)
    {
        if (!IsVerified)
            return FintechResult.NotAllowed;

        if (FindAccount(accountId) is not { CardLast4: { } last4 } account)
            return FintechResult.NotFound;

        if (account.CardFrozen == frozen)
            return FintechResult.NoChange;

        Replace(account with { CardFrozen = frozen });
        Notify(FintechAlertKind.Security, frozen ? "Card frozen" : "Card unfrozen",
            frozen ? $"Your card ending {last4} is frozen. New payments are declined until you unfreeze it." : $"Your card ending {last4} works again.", $"/showcase/fintech/accounts/{accountId}");
        Changed?.Invoke();

        return FintechResult.Ok;
    }

    /// <summary>Changes a transaction's private note (up to <see cref="MaxNoteLength" /> characters; empty removes it).</summary>
    public FintechResult SetNote(int transactionId, string? note)
    {
        if (!IsVerified)
            return FintechResult.NotAllowed;

        if (FindTransaction(transactionId) is not { } transaction)
            return FintechResult.NotFound;

        note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();

        if (note is { Length: > MaxNoteLength })
            return FintechResult.Invalid;

        if (transaction.Note == note)
            return FintechResult.NoChange;

        SetTransaction(transactionId, item => item with { Note = note });
        Changed?.Invoke();

        return FintechResult.Ok;
    }

    /// <summary>Changes a payee's nickname (no code needed: it doesn't change where money goes).</summary>
    public FintechResult SetPayeeNickname(string payeeId, string? nickname)
    {
        if (!IsVerified)
            return FintechResult.NotAllowed;

        if (FindPayee(payeeId) is not { } payee)
            return FintechResult.NotFound;

        nickname = string.IsNullOrWhiteSpace(nickname) ? null : nickname.Trim();

        if (nickname is { Length: > 30 })
            return FintechResult.Invalid;

        if (payee.Nickname == nickname)
            return FintechResult.NoChange;

        payees[payees.IndexOf(payee)] = payee with { Nickname = nickname };
        Changed?.Invoke();

        return FintechResult.Ok;
    }

    /// <summary>Records the color mode the theme switcher applied, so charts can redraw in matching colors.</summary>
    public void SetTheme(string? themeName)
    {
        var isDark = string.Equals(themeName, "dark", StringComparison.OrdinalIgnoreCase);

        if (isDarkTheme == isDark)
            return;

        isDarkTheme = isDark;
        ThemeChanged?.Invoke();
    }

    /// <summary>Returns this month's spending in a category, as a positive amount (posted and pending).</summary>
    public decimal SpentThisMonth(string categoryId)
    {
        var monthStart = new DateTime(Today.Year, Today.Month, 1);

        return -transactions
            .Where(item => item.CategoryId == categoryId && item.Date >= monthStart && item.Date <= Clock.Now && item.Amount < 0)
            .Sum(item => item.Amount);
    }

    /// <summary>Stores a validated transfer for the review screen. Returns the validation message, or <see langword="null" /> when stored.</summary>
    public string? StartTransfer(FintechTransferDraft draft)
    {
        if (!IsVerified)
            return "Sign in again to move money.";

        CatchUp();

        if (ValidateTransfer(draft) is { } error)
            return error;

        PendingTransfer = draft;
        Changed?.Invoke();

        return null;
    }

    /// <summary>Returns the reference of the transfer confirmed last and forgets it, so the confirmation shows only once.</summary>
    public string? TakeLastTransferReference()
    {
        var reference = LastTransferReference;
        LastTransferReference = null;

        return reference;
    }

    /// <summary>Validates a stored snapshot and applies it only when every part is valid.</summary>
    public bool TryRestoreData(JsonElement data)
    {
        FintechSnapshot? snapshot;

        try
        {
            snapshot = data.Deserialize<FintechSnapshot>(snapshotOptions);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException)
        {
            return false;
        }

        if (snapshot is not
            {
                Accounts: { Count: 3 } storedAccounts,
                Budgets: { } storedBudgets,
                Disputes: { Count: <= 500 } storedDisputes,
                Notices: { Count: <= MaxNotices } storedNotices,
                Payees: { Count: <= MaxPayees } storedPayees,
                Transactions: { Count: >= 1 and <= MaxTransactions } storedTransactions,
                Transfers: { Count: <= MaxTransfers } storedTransfers,
                AlertSettings: { } storedAlerts,
            }
            || snapshot.NextDisputeId is < 3102 or > 1_000_000 - 500
            || snapshot.NextNoticeId is < 1 or > 1_000_000 - MaxNotices
            || snapshot.NextPayeeId is < 1 or > 100_000
            || snapshot.NextReference is < 260417 or > 9_000_000
            || snapshot.NextTransactionId is < 9000 or > 1_000_000 - MaxTransactions)
            return false;

        // The accounts are the customer's three seeded accounts, with their kinds and cards; only balances and freezes change.
        var seeded = FintechSeedData.Accounts();

        if (storedAccounts.Any(account => account is null || seeded.FirstOrDefault(seed => seed.Id == account.Id) is not { } seed
                || account with { Balance = seed.Balance, CardFrozen = seed.CardFrozen } != seed || !IsMoney(account.Balance))
            || !Unique(storedAccounts.Select(account => account.Id)))
            return false;

        var accountIds = storedAccounts.Select(account => account.Id).ToHashSet(StringComparer.Ordinal);

        if (storedBudgets.Count > Categories.Count || storedBudgets.Any(budget => budget is null || Categories.FirstOrDefault(category => category.Id == budget.CategoryId) is not { IsSpending: true }
                || !ValidLimit(budget.Limit))
            || !Unique(storedBudgets.Select(budget => budget.CategoryId)))
            return false;

        if (storedTransactions.Any(item => item is null || item.Id < 1 || item.Id >= snapshot.NextTransactionId || !IsPlausible(item.Date) || !accountIds.Contains(item.AccountId)
                || !IsText(item.Description, 120) || Categories.All(category => category.Id != item.CategoryId) || !IsMoney(item.Amount) || item.Amount == 0
                || (item.Note is not null && !IsText(item.Note, MaxNoteLength)) || (item.TransferReference is not null && !IsText(item.TransferReference, 20)))
            || !Unique(storedTransactions.Select(item => item.Id)))
            return false;

        if (storedPayees.Any(payee => payee is null || !IsText(payee.Id, 30) || !IsText(payee.Name, 60) || !Regex.IsMatch(payee.MaskedAccount ?? string.Empty, @"^•••• \d{4}$")
                || !IsText(payee.BankName, 80) || !Enum.IsDefined(payee.Match) || !IsPlausible(payee.AddedAt) || (payee.Nickname is not null && !IsText(payee.Nickname, 30)))
            || !Unique(storedPayees.Select(payee => payee.Id)) || !Unique(storedPayees.Select(payee => payee.Name.ToUpperInvariant())))
            return false;

        var payeeIds = storedPayees.Select(payee => payee.Id).ToHashSet(StringComparer.Ordinal);

        if (storedTransfers.Any(transfer => !ValidTransfer(transfer, accountIds, payeeIds, snapshot.NextReference)) || !Unique(storedTransfers.Select(transfer => transfer.Reference)))
            return false;

        var transactionsById = storedTransactions.ToDictionary(item => item.Id);

        if (storedDisputes.Any(dispute => dispute is null || dispute.Id < 3101 || dispute.Id >= snapshot.NextDisputeId || !IsAccountId(dispute.CustomerId)
                || !transactionsById.TryGetValue(dispute.TransactionId, out var disputed) || disputed.DisputeId != dispute.Id || dispute.Amount != -disputed.Amount
                || !IsText(dispute.Merchant, 120) || !Enum.IsDefined(dispute.Reason) || !Enum.IsDefined(dispute.Status) || dispute.Details is not { Length: >= 10 and <= MaxDisputeDetails }
                || !IsPlausible(dispute.CreatedAt) || (dispute.Status is FintechDisputeStatus.Refunded or FintechDisputeStatus.Declined) != dispute.ClosedAt is not null
                || (dispute.Resolution is not null && !IsText(dispute.Resolution, MaxDisputeDetails)))
            || !Unique(storedDisputes.Select(dispute => dispute.Id))
            || storedTransactions.Any(item => item.DisputeId is { } id && storedDisputes.All(dispute => dispute.Id != id)))
            return false;

        if (storedNotices.Any(notice => notice is null || notice.Id < 1 || notice.Id >= snapshot.NextNoticeId || !IsPlausible(notice.At) || !Enum.IsDefined(notice.Kind)
                || !IsText(notice.Title, 80) || !IsText(notice.Body, 400) || (notice.Link is not null && !notice.Link.StartsWith("/showcase/fintech", StringComparison.Ordinal)))
            || !Unique(storedNotices.Select(notice => notice.Id)))
            return false;

        if (storedAlerts.LowBalanceAmount is < 0 or > 1_000_000 || storedAlerts.LargeTransactionAmount is < 1 or > 1_000_000 || !IsMoney(storedAlerts.LowBalanceAmount)
            || !IsMoney(storedAlerts.LargeTransactionAmount))
            return false;

        if (snapshot.VerifiedAccountId is not null && !IsAccountId(snapshot.VerifiedAccountId))
            return false;

        accounts.Clear();
        accounts.AddRange(storedAccounts.OrderBy(account => seeded.FindIndex(seed => seed.Id == account.Id)));
        budgets.Clear();
        budgets.AddRange(storedBudgets);
        transactions.Clear();
        transactions.AddRange(storedTransactions.OrderByDescending(item => item.Date).ThenByDescending(item => item.Id));
        payees.Clear();
        payees.AddRange(storedPayees);
        transfers.Clear();
        transfers.AddRange(storedTransfers);
        disputes.Clear();
        disputes.AddRange(storedDisputes);
        notices.Clear();
        notices.AddRange(storedNotices.OrderByDescending(notice => notice.At));
        AlertSettings = storedAlerts;

        nextDisputeId = snapshot.NextDisputeId;
        nextNoticeId = snapshot.NextNoticeId;
        nextPayeeId = snapshot.NextPayeeId;
        nextReference = snapshot.NextReference;
        nextTransactionId = snapshot.NextTransactionId;

        // The identity is applied after the data; the verified account only counts for the account signed in then.
        verifiedAccountId = snapshot.VerifiedAccountId;
        PendingTransfer = null;
        challenge = null;

        return true;
    }

    /// <summary>Changes the alert settings (amounts from $0, or $1 for large transactions, to $1,000,000).</summary>
    public FintechResult UpdateAlerts(FintechAlertSettings settings)
    {
        if (!IsVerified)
            return FintechResult.NotAllowed;

        if (settings.LowBalanceAmount is < 0 or > 1_000_000 || settings.LargeTransactionAmount is < 1 or > 1_000_000
            || !IsMoney(settings.LowBalanceAmount) || !IsMoney(settings.LargeTransactionAmount))
            return FintechResult.Invalid;

        AlertSettings = new FintechAlertSettings
        {
            Budget = settings.Budget,
            Dispute = settings.Dispute,
            LargeTransaction = settings.LargeTransaction,
            LargeTransactionAmount = settings.LargeTransactionAmount,
            LowBalance = settings.LowBalance,
            LowBalanceAmount = settings.LowBalanceAmount,
            Security = true,
            Transfer = settings.Transfer,
        };
        Changed?.Invoke();

        return FintechResult.Ok;
    }

    /// <summary>Changes a budget's monthly limit ($1 to $100,000).</summary>
    public FintechResult UpdateBudget(string categoryId, decimal limit)
    {
        if (!IsVerified)
            return FintechResult.NotAllowed;

        var index = budgets.FindIndex(budget => budget.CategoryId == categoryId);

        if (index < 0)
            return FintechResult.NotFound;

        if (!ValidLimit(limit))
            return FintechResult.Invalid;

        if (budgets[index].Limit == limit)
            return FintechResult.NoChange;

        budgets[index] = budgets[index] with { Limit = limit };
        Changed?.Invoke();

        return FintechResult.Ok;
    }

    /// <summary>Checks a transfer against the rules. Returns a message for the customer, or <see langword="null" /> when it is valid.</summary>
    public string? ValidateTransfer(FintechTransferDraft draft)
    {
        if (FindAccount(draft.FromAccountId) is not { } source || source.Kind == FintechAccountKind.CreditCard)
            return "Choose a checking or savings account to send from.";

        if (draft.ToId == draft.FromAccountId)
            return "Choose a different account to send to.";

        if (FindAccount(draft.ToId) is null && FindPayee(draft.ToId) is null)
            return "Choose who to send money to.";

        if (draft.Amount <= 0)
            return "Enter an amount greater than $0.00.";

        if (draft.Amount != Math.Round(draft.Amount, 2))
            return "Use at most two decimal places.";

        if (draft.Amount > 50_000m)
            return "Transfers are limited to $50,000.00 each.";

        if (FindAccount(draft.ToId) is { Kind: FintechAccountKind.CreditCard } card)
        {
            var owed = Math.Max(0, -card.Balance - transfers.Where(transfer => transfer.ToId == card.Id && transfer.Status is FintechTransferStatus.Scheduled or FintechTransferStatus.Held).Sum(transfer => transfer.Amount));

            if (owed == 0)
                return $"Nothing more is owed on the {card.Name}.";

            if (draft.Amount > owed)
                return $"You owe {Money(owed)} on the {card.Name}{(owed != -card.Balance ? " after payments already scheduled" : string.Empty)}; pay up to that.";
        }

        if (draft.Amount > Available(source))
            return $"{source.Name} has {Money(Available(source))} available{(Reserved(source.Id) > 0 ? " after scheduled and held transfers" : string.Empty)}.";

        if (draft.Date < Today || draft.Date > Today.AddDays(60))
            return "Choose a date from today up to 60 days ahead.";

        if (draft.Note is { Length: > 60 })
            return "Keep the note to 60 characters.";

        return null;
    }

    /// <summary>Confirms the second sign-in step with the code from the (demo) text message.</summary>
    public FintechResult VerifySignIn(string code)
    {
        if (Identity.Current is not { RoleId: FintechSeedData.CustomerRole } account)
            return FintechResult.NotAllowed;

        if (!TryCode(FintechCodePurpose.SignIn, code))
            return FintechResult.WrongCode;

        verifiedAccountId = account.Id;
        Notify(FintechAlertKind.Security, "New sign-in", $"You signed in on {Clock.Now.ToString("MMM d 'at' HH:mm", CultureInfo.InvariantCulture)} with a one-time code. If this wasn't you, contact us straight away.", null);
        Changed?.Invoke();

        return FintechResult.Ok;
    }

    #endregion

    #region Private Methods

    private static string Day(DateOnly date) => date.ToString("MMM d", CultureInfo.InvariantCulture);

    /// <summary>Returns the Levenshtein distance between two short words.</summary>
    private static int EditDistance(string a, string b)
    {
        var previous = Enumerable.Range(0, b.Length + 1).ToArray();

        for (var i = 1; i <= a.Length; i++)
        {
            var current = new int[b.Length + 1];
            current[0] = i;

            for (var j = 1; j <= b.Length; j++)
                current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1), previous[j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));

            previous = current;
        }

        return previous[b.Length];
    }

    private static string Digits(string? value) => new((value ?? string.Empty).Where(char.IsAsciiDigit).ToArray());

    private static bool IsAccountId(string? accountId) => accountId is not null && FintechSeedData.DemoAccounts().Any(demo => demo.Id == accountId);

    private static bool IsMoney(decimal value) => value is >= -MaxMoney and <= MaxMoney && decimal.Round(value, 2) == value;

    private static bool IsPlausible(DateTime value) => value >= FintechSeedData.Now.AddYears(-3) && value <= FintechSeedData.Now.AddYears(2);

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

    private static string Money(decimal value) => value.ToString("$#,0.00", CultureInfo.InvariantCulture);

    private static string NormalizeName(string? name) =>
        string.Join(' ', Regex.Replace((name ?? string.Empty).ToLowerInvariant(), @"[^a-z0-9 ]", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(part => part is not ("llc" or "inc" or "ltd" or "co" or "the")));

    private static string TransferPath(FintechTransfer transfer) => $"/showcase/fintech/transfers/{transfer.Reference}";

    private static bool Unique<T>(IEnumerable<T> values)
    {
        var list = values.ToList();

        return list.Distinct().Count() == list.Count;
    }

    private static bool ValidLimit(decimal limit) => limit is >= 1 and <= 100_000 && decimal.Round(limit, 2) == limit;

    private string AccountName(string accountId) => FindAccount(accountId)?.Name ?? "your account";

    /// <summary>Returns a category's share of its budget this month, or <see langword="null" /> without a budget.</summary>
    private decimal? BudgetShare(string categoryId) =>
        budgets.FirstOrDefault(budget => budget.CategoryId == categoryId) is { } budget ? SpentThisMonth(categoryId) / budget.Limit : null;

    /// <summary>Raises a budget notice when a change takes a category past 80% or past its limit.</summary>
    private void CheckBudget(string categoryId, decimal? before)
    {
        if (BudgetShare(categoryId) is not { } after || before is not { } was)
            return;

        var name = FindCategory(categoryId).Name;

        if (was <= 1 && after > 1)
            Notify(FintechAlertKind.Budget, $"{name} is over budget", $"You've spent more than this month's {name.ToLowerInvariant()} budget.", "/showcase/fintech/budgets");
        else if (was < 0.8m && after >= 0.8m)
            Notify(FintechAlertKind.Budget, $"{name} is at 80% of its budget", $"You've used most of this month's {name.ToLowerInvariant()} budget.", "/showcase/fintech/budgets");
    }

    /// <summary>Returns why a transfer to a payee would be held for a check, or <see langword="null" /> when it wouldn't.</summary>
    public string? HoldReason(FintechPayee payee, decimal amount)
    {
        if (amount >= HoldLargeAmount)
            return payee.AddedAt > Clock.Now.AddDays(-NewPayeeDays) ? "A large payment to a payee added in the last 7 days" : "A large payment to a payee";

        if (amount >= HoldNewPayeeAmount && payee.AddedAt > Clock.Now.AddDays(-NewPayeeDays))
            return "A payment to a payee added in the last 7 days";

        if (amount >= HoldNewPayeeAmount && payee.Match is not FintechPayeeMatch.Match)
            return "A payment to a payee whose name couldn't be confirmed";

        return null;
    }

    private void Notify(FintechAlertKind kind, string title, string body, string? link)
    {
        var enabled = kind switch
        {
            FintechAlertKind.LowBalance => AlertSettings.LowBalance,
            FintechAlertKind.LargeTransaction => AlertSettings.LargeTransaction,
            FintechAlertKind.Budget => AlertSettings.Budget,
            FintechAlertKind.Transfer => AlertSettings.Transfer,
            FintechAlertKind.Dispute => AlertSettings.Dispute,

            // Security notices (sign-ins, payees, card freezes) are always sent, as banks do.
            _ => true,
        };

        if (!enabled)
            return;

        notices.Insert(0, new FintechNotice { Id = nextNoticeId++, At = Clock.Now, Kind = kind, Title = title, Body = body, Link = link });

        if (notices.Count > MaxNotices)
            notices.RemoveAt(notices.Count - 1);
    }

    private void OnIdentityChanged()
    {
        // The code step belongs to one sign-in: another account, or signing out, needs it again.
        if (Identity.Current?.Id != verifiedAccountId)
            verifiedAccountId = null;

        challenge = null;
        PendingTransfer = null;
        Changed?.Invoke();
    }

    /// <summary>Moves a transfer's money: the debit, the credit to an own account, the balances, and the alerts.</summary>
    private void Post(FintechTransfer transfer, DateTime at)
    {
        var source = FindAccount(transfer.FromAccountId)!;
        var label = transfer.Note is null ? string.Empty : $" · {transfer.Note}";

        InsertInOrder(new FintechTransaction(nextTransactionId++, at, source.Id, Clip($"Transfer to {transfer.ToName}{label}"), "transfer", -transfer.Amount, false)
        {
            TransferReference = transfer.Reference,
        });
        Replace(source with { Balance = source.Balance - transfer.Amount });

        if (transfer.ToOwnAccount && FindAccount(transfer.ToId) is { } target)
        {
            InsertInOrder(new FintechTransaction(nextTransactionId++, at, target.Id, Clip($"Transfer from {source.Name}{label}"), "transfer", transfer.Amount, false)
            {
                TransferReference = transfer.Reference,
            });
            Replace(target with { Balance = target.Balance + transfer.Amount });
        }

        // Trim the oldest rows over the cap, but never a disputed one (its dispute points to it).
        while (transactions.Count > MaxTransactions && transactions.FindLastIndex(item => item.DisputeId is null) is var oldest and >= 0)
            transactions.RemoveAt(oldest);

        transfer.Status = FintechTransferStatus.Completed;
        transfer.ClosedAt = at;
        transfer.HoldReason = null;

        if (transfer.Amount >= AlertSettings.LargeTransactionAmount)
            Notify(FintechAlertKind.LargeTransaction, "Large transfer sent", $"{Money(transfer.Amount)} went from {source.Name} to {transfer.ToName}.", TransferPath(transfer));

        if (FindAccount(source.Id) is { } after && after.Balance < AlertSettings.LowBalanceAmount && after.Balance + transfer.Amount >= AlertSettings.LowBalanceAmount)
            Notify(FintechAlertKind.LowBalance, $"{after.Name} is below {Money(AlertSettings.LowBalanceAmount)}", $"The balance is now {Money(after.Balance)}.", $"/showcase/fintech/accounts/{after.Id}");
    }

    private void Replace(FintechAccount account) => accounts[accounts.FindIndex(item => item.Id == account.Id)] = account;

    /// <summary>Shortens a transaction description to the 120 characters a restore accepts.</summary>
    private static string Clip(string description) => description.Length <= 120 ? description : string.Concat(description.AsSpan(0, 119), "…");

    /// <summary>Inserts a transaction so the list stays newest first.</summary>
    private void InsertInOrder(FintechTransaction transaction)
    {
        var index = transactions.FindIndex(item => item.Date < transaction.Date);
        transactions.Insert(index < 0 ? transactions.Count : index, transaction);
    }

    private void SetTransaction(int transactionId, Func<FintechTransaction, FintechTransaction> change)
    {
        var index = transactions.FindIndex(item => item.Id == transactionId);

        if (index >= 0)
            transactions[index] = change(transactions[index]);
    }

    /// <summary>Checks a code for a purpose: right, unexpired, and within the attempts. A wrong code uses an attempt; the last one discards the code.</summary>
    private bool TryCode(FintechCodePurpose purpose, string? code)
    {
        if (challenge is not { } current || current.Purpose != purpose || current.ExpiresAt <= Clock.Now)
            return false;

        if (CryptographicOperations.FixedTimeEquals(System.Text.Encoding.ASCII.GetBytes(current.Code), System.Text.Encoding.ASCII.GetBytes(Digits(code))))
        {
            challenge = null;

            return true;
        }

        if (++challengeAttempts >= CodeAttempts)
            challenge = null;

        Changed?.Invoke();

        return false;
    }

    private bool ValidTransfer(FintechTransfer? transfer, HashSet<string> accountIds, HashSet<string> payeeIds, int nextReference) =>
        transfer is not null && Regex.Match(transfer.Reference ?? string.Empty, @"^TM-(\d{6,7})$") is { Success: true } match
        && int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) < nextReference
        && IsAccountId(transfer.CustomerId) && IsText(transfer.CustomerName, ShowcaseIdentity.MaxNameLength)
        && accountIds.Contains(transfer.FromAccountId) && transfer.FromAccountId != transfer.ToId
        && (transfer.ToOwnAccount ? accountIds.Contains(transfer.ToId) : payeeIds.Contains(transfer.ToId) || transfer.Status is FintechTransferStatus.Completed or FintechTransferStatus.Rejected or FintechTransferStatus.Cancelled)
        && IsText(transfer.ToName, 60) && transfer.Amount is > 0 and <= 50_000m && IsMoney(transfer.Amount)
        && transfer.Date >= DateOnly.FromDateTime(FintechSeedData.Now.AddYears(-3)) && transfer.Date <= DateOnly.FromDateTime(FintechSeedData.Now.AddYears(2))
        && IsPlausible(transfer.CreatedAt) && Enum.IsDefined(transfer.Status)
        && (transfer.Status is FintechTransferStatus.Scheduled or FintechTransferStatus.Held) == transfer.ClosedAt is null
        && (transfer.Status == FintechTransferStatus.Held) == transfer.HoldReason is not null && (transfer.HoldReason is null || IsText(transfer.HoldReason, 120))
        && (transfer.RejectReason is null || IsText(transfer.RejectReason, 300)) && (transfer.Note is null || IsText(transfer.Note, 60));

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the customer's accounts.</summary>
    public IReadOnlyList<FintechAccount> Accounts => accounts;

    /// <summary>Gets the alert settings.</summary>
    public FintechAlertSettings AlertSettings { get; private set; } = new();

    /// <summary>Gets the monthly budgets.</summary>
    public IReadOnlyList<FintechBudget> Budgets => budgets;

    /// <summary>Gets the spending and income categories.</summary>
    public IReadOnlyList<FintechCategory> Categories { get; } = FintechSeedData.Categories();

    /// <summary>Gets the site's clock, which moves on from the seed time (D-029).</summary>
    public ShowcaseClock Clock { get; } = new(FintechSeedData.Now);

    /// <summary>Gets the site's demo sign-in.</summary>
    public ShowcaseIdentity Identity { get; }

    /// <summary>Gets whether the dark color mode is applied.</summary>
    public bool IsDarkTheme => isDarkTheme;

    /// <summary>Gets whether a customer is signed in and has entered the one-time code: every banking action needs it.</summary>
    public bool IsVerified => Identity.Current is { RoleId: FintechSeedData.CustomerRole } account && verifiedAccountId == account.Id;

    /// <summary>Gets the reference of the transfer confirmed last in this tab, so its page can confirm it.</summary>
    public string? LastTransferReference { get; private set; }

    /// <summary>Gets the total of all balances (the credit card counts as debt).</summary>
    public decimal NetWorth => accounts.Sum(account => account.Balance);

    /// <summary>Gets net worth for the last 12 months; the last value is the live <see cref="NetWorth" />.</summary>
    public IReadOnlyList<double> NetWorthHistory => FintechSeedData.NetWorthHistory(seedNetWorth, NetWorth);

    /// <summary>Gets the customer's alerts feed, newest first.</summary>
    public IReadOnlyList<FintechNotice> Notices => IsVerified ? notices : Array.Empty<FintechNotice>();

    /// <summary>Gets the saved payees, by name.</summary>
    public IReadOnlyList<FintechPayee> Payees => payees.OrderBy(payee => payee.Name, StringComparer.OrdinalIgnoreCase).ToList();

    /// <summary>Gets the transfer waiting on the review screen, if any.</summary>
    public FintechTransferDraft? PendingTransfer { get; private set; }

    /// <summary>Gets the site's slug, used in the storage key.</summary>
    public string SiteSlug => "fintech";

    /// <summary>Gets the snapshot format version.</summary>
    public int SnapshotVersion => 1;

    /// <summary>Gets the site's today, on its clock.</summary>
    public DateOnly Today => DateOnly.FromDateTime(Clock.Now);

    /// <summary>Gets every transaction, newest first.</summary>
    public IReadOnlyList<FintechTransaction> Transactions => transactions;

    /// <summary>Gets how many notices the customer hasn't read.</summary>
    public int UnreadNoticeCount => IsVerified ? notices.Count(notice => !notice.IsRead) : 0;

    #endregion

    #region Events

    /// <summary>Raised when anything in the state changes, including sign-in.</summary>
    public event Action? Changed;

    /// <summary>Raised when the applied color mode changes.</summary>
    public event Action? ThemeChanged;

    #endregion

    #region Nested Types

    /// <summary>The bank's part of the tab snapshot.</summary>
    private sealed class FintechSnapshot
    {
        #region Properties, Indexers

        public List<FintechAccount>? Accounts { get; set; }

        public FintechAlertSettings? AlertSettings { get; set; }

        public List<FintechBudget>? Budgets { get; set; }

        public List<FintechDispute>? Disputes { get; set; }

        public int NextDisputeId { get; set; }

        public int NextNoticeId { get; set; }

        public int NextPayeeId { get; set; }

        public int NextReference { get; set; }

        public int NextTransactionId { get; set; }

        public List<FintechNotice>? Notices { get; set; }

        public List<FintechPayee>? Payees { get; set; }

        public List<FintechTransaction>? Transactions { get; set; }

        public List<FintechTransfer>? Transfers { get; set; }

        public string? VerifiedAccountId { get; set; }

        #endregion
    }

    #endregion
}
