namespace BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;

/// <summary>Kind of bank account.</summary>
public enum FintechAccountKind
{
    /// <summary>An everyday checking account.</summary>
    Checking,

    /// <summary>A savings account.</summary>
    Savings,

    /// <summary>A credit card; its balance is what is owed, stored as a negative amount.</summary>
    CreditCard,
}

/// <summary>Period the transactions screen covers.</summary>
public enum FintechPeriod
{
    /// <summary>The last 30 days.</summary>
    Last30Days = 30,

    /// <summary>The last 60 days.</summary>
    Last60Days = 60,

    /// <summary>The last 90 days.</summary>
    Last90Days = 90,
}

/// <summary>Where a transfer is.</summary>
public enum FintechTransferStatus
{
    /// <summary>The money moved.</summary>
    Completed,

    /// <summary>Waiting for its date; the money is set aside.</summary>
    Scheduled,

    /// <summary>Held for a check by bank operations; the money is set aside.</summary>
    Held,

    /// <summary>Not sent: bank operations rejected it, or there wasn't enough money on its date.</summary>
    Rejected,

    /// <summary>Cancelled by the customer before it was sent.</summary>
    Cancelled,
}

/// <summary>Why a customer disputes a card or account transaction.</summary>
public enum FintechDisputeReason
{
    /// <summary>The customer doesn't recognize it.</summary>
    NotRecognized,

    /// <summary>Goods or services were never received.</summary>
    NotReceived,

    /// <summary>Charged twice.</summary>
    Duplicate,

    /// <summary>The amount is wrong.</summary>
    WrongAmount,

    /// <summary>A cancelled subscription or booking was still charged.</summary>
    Cancelled,
}

/// <summary>Where a dispute is.</summary>
public enum FintechDisputeStatus
{
    /// <summary>Sent; waiting for bank operations.</summary>
    Submitted,

    /// <summary>Bank operations are looking into it with the merchant.</summary>
    UnderReview,

    /// <summary>Resolved in the customer's favor; the amount was credited back.</summary>
    Refunded,

    /// <summary>Resolved against the customer; the charge stands.</summary>
    Declined,
}

/// <summary>The result of checking a new payee's name against the account (confirmation of payee).</summary>
public enum FintechPayeeMatch
{
    /// <summary>The name matches the account holder.</summary>
    Match,

    /// <summary>The name is close to the account holder's; the bank shows the actual name.</summary>
    CloseMatch,

    /// <summary>The name doesn't match the account holder.</summary>
    NoMatch,

    /// <summary>The receiving bank couldn't check the name.</summary>
    Unavailable,
}

/// <summary>What a one-time code confirms.</summary>
public enum FintechCodePurpose
{
    /// <summary>The second step of signing in.</summary>
    SignIn,

    /// <summary>Adding or changing a payee.</summary>
    Payee,
}

/// <summary>Kinds of alert a customer can switch on, and of notice they get.</summary>
public enum FintechAlertKind
{
    /// <summary>An account went below the low-balance amount.</summary>
    LowBalance,

    /// <summary>A transaction over the large-transaction amount.</summary>
    LargeTransaction,

    /// <summary>A budget reached 80% or went over.</summary>
    Budget,

    /// <summary>A transfer was sent, scheduled, held, rejected, or cancelled.</summary>
    Transfer,

    /// <summary>A dispute was opened or changed.</summary>
    Dispute,

    /// <summary>A new sign-in, a new payee, or a card frozen or unfrozen.</summary>
    Security,
}

/// <summary>Outcome of a customer action.</summary>
public enum FintechResult
{
    /// <summary>Done.</summary>
    Ok,

    /// <summary>Sign in and confirm the one-time code first.</summary>
    NotAllowed,

    /// <summary>The item doesn't exist or isn't the customer's.</summary>
    NotFound,

    /// <summary>A value is missing, too long, or out of range.</summary>
    Invalid,

    /// <summary>The item's state doesn't allow it.</summary>
    WrongState,

    /// <summary>The one-time code is wrong or expired.</summary>
    WrongCode,

    /// <summary>Another item already has that name or account.</summary>
    Duplicate,

    /// <summary>The limit of items is reached.</summary>
    LimitReached,

    /// <summary>Not enough money available.</summary>
    InsufficientFunds,

    /// <summary>Nothing changed.</summary>
    NoChange,

    /// <summary>A scheduled or held transfer still goes to it, so its details can't change.</summary>
    InUse,
}

/// <summary>A bank account of the customer. <see cref="CardLast4" /> is set when a card is linked (debit or credit).</summary>
public sealed record FintechAccount(string Id, string Name, FintechAccountKind Kind, string MaskedNumber, decimal Balance, decimal? CreditLimit)
{
    #region Properties, Indexers

    /// <summary>Gets the balance that could be moved out, before scheduled and held transfers; <c>FintechState.Available</c> subtracts those.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public decimal Available => Kind == FintechAccountKind.CreditCard ? 0 : Math.Max(0, Balance);

    /// <summary>Gets whether the linked card is frozen: new card payments are declined until it is unfrozen.</summary>
    public bool CardFrozen { get; init; }

    /// <summary>Gets the last four digits of the linked card, or <see langword="null" /> without a card.</summary>
    public string? CardLast4 { get; init; }

    #endregion
}

/// <summary>A spending or income category.</summary>
public sealed record FintechCategory(string Id, string Name, IconName IconName, bool IsSpending);

/// <summary>A posted or pending transaction. A negative amount is money out of the account.</summary>
public sealed record FintechTransaction(int Id, DateTime Date, string AccountId, string Description, string CategoryId, decimal Amount, bool IsPending)
{
    #region Properties, Indexers

    /// <summary>Gets the dispute opened on it, if any.</summary>
    public int? DisputeId { get; init; }

    /// <summary>Gets the customer's private note.</summary>
    public string? Note { get; init; }

    /// <summary>Gets the transfer it belongs to, if any.</summary>
    public string? TransferReference { get; init; }

    #endregion
}

/// <summary>A monthly spending limit for one category.</summary>
public sealed record FintechBudget(string CategoryId, decimal Limit);

/// <summary>A saved person or company money can be sent to. Only the last four digits of the account are kept.</summary>
public sealed record FintechPayee(string Id, string Name, string MaskedAccount)
{
    #region Properties, Indexers

    /// <summary>Gets when the payee was added; a new payee's large transfers are held for a check.</summary>
    public DateTime AddedAt { get; init; }

    /// <summary>Gets the receiving bank's name.</summary>
    public string BankName { get; init; } = string.Empty;

    /// <summary>Gets the result of the name check when the payee was added or changed.</summary>
    public FintechPayeeMatch Match { get; init; } = FintechPayeeMatch.Match;

    /// <summary>Gets the customer's nickname for the payee, if any.</summary>
    public string? Nickname { get; init; }

    #endregion
}

/// <summary>A payee being added or changed.</summary>
public sealed class FintechPayeeDraft
{
    #region Properties, Indexers

    /// <summary>Gets or sets the account number (digits; only the last four are kept).</summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the account holder's name, as the customer typed it.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the nickname.</summary>
    public string? Nickname { get; set; }

    /// <summary>Gets or sets the routing number (9 digits).</summary>
    public string RoutingNumber { get; set; } = string.Empty;

    #endregion
}

/// <summary>The bank's answer to a payee name check.</summary>
public sealed record FintechPayeeCheck(FintechPayeeMatch Match, string? ActualName, string BankName);

/// <summary>A transfer the customer has filled in and not yet confirmed.</summary>
public sealed record FintechTransferDraft(string FromAccountId, string ToId, decimal Amount, DateOnly Date, string? Note);

/// <summary>A transfer: to another of the customer's accounts or to a payee, now or on a later date.</summary>
public sealed class FintechTransfer
{
    #region Properties, Indexers

    /// <summary>Gets or sets the amount.</summary>
    public decimal Amount { get; init; }

    /// <summary>Gets or sets when it was cancelled, rejected, or completed.</summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>Gets when the customer confirmed it.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets the customer's demo account.</summary>
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>Gets the customer's name.</summary>
    public string CustomerName { get; init; } = string.Empty;

    /// <summary>Gets the date it is sent on.</summary>
    public DateOnly Date { get; init; }

    /// <summary>Gets the account it is sent from.</summary>
    public string FromAccountId { get; init; } = string.Empty;

    /// <summary>Gets or sets why it was held for a check, while held.</summary>
    public string? HoldReason { get; set; }

    /// <summary>Gets the customer's note.</summary>
    public string? Note { get; init; }

    /// <summary>Gets the reference, like "TM-260417".</summary>
    public string Reference { get; init; } = string.Empty;

    /// <summary>Gets or sets why it was rejected.</summary>
    public string? RejectReason { get; set; }

    /// <summary>Gets or sets where it is.</summary>
    public FintechTransferStatus Status { get; set; }

    /// <summary>Gets the receiving account or payee ID.</summary>
    public string ToId { get; init; } = string.Empty;

    /// <summary>Gets the receiving account's or payee's name, kept for the record.</summary>
    public string ToName { get; init; } = string.Empty;

    /// <summary>Gets whether the money goes to one of the customer's own accounts.</summary>
    public bool ToOwnAccount { get; init; }

    #endregion
}

/// <summary>A dispute of a card or account transaction.</summary>
public sealed class FintechDispute
{
    #region Properties, Indexers

    /// <summary>Gets the disputed amount (positive).</summary>
    public decimal Amount { get; init; }

    /// <summary>Gets or sets when it was resolved.</summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>Gets when the customer opened it.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets the customer's demo account.</summary>
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>Gets the customer's explanation.</summary>
    public string Details { get; init; } = string.Empty;

    /// <summary>Gets the dispute number.</summary>
    public int Id { get; init; }

    /// <summary>Gets the merchant (the transaction's description).</summary>
    public string Merchant { get; init; } = string.Empty;

    /// <summary>Gets the reason.</summary>
    public FintechDisputeReason Reason { get; init; }

    /// <summary>Gets or sets the bank's explanation of the outcome.</summary>
    public string? Resolution { get; set; }

    /// <summary>Gets or sets where it is.</summary>
    public FintechDisputeStatus Status { get; set; }

    /// <summary>Gets the disputed transaction.</summary>
    public int TransactionId { get; init; }

    #endregion
}

/// <summary>Which alerts the customer gets, and their amounts.</summary>
public sealed class FintechAlertSettings
{
    #region Properties, Indexers

    /// <summary>Gets or sets whether budget alerts (80% and over) are on.</summary>
    public bool Budget { get; set; } = true;

    /// <summary>Gets or sets whether dispute updates are on.</summary>
    public bool Dispute { get; set; } = true;

    /// <summary>Gets or sets the large-transaction amount.</summary>
    public decimal LargeTransactionAmount { get; set; } = 500m;

    /// <summary>Gets or sets whether large-transaction alerts are on.</summary>
    public bool LargeTransaction { get; set; } = true;

    /// <summary>Gets or sets the low-balance amount.</summary>
    public decimal LowBalanceAmount { get; set; } = 1000m;

    /// <summary>Gets or sets whether low-balance alerts are on.</summary>
    public bool LowBalance { get; set; } = true;

    /// <summary>Gets or sets whether security alerts (sign-ins, payees, card freezes) are on.</summary>
    public bool Security { get; set; } = true;

    /// <summary>Gets or sets whether transfer updates are on.</summary>
    public bool Transfer { get; set; } = true;

    #endregion
}

/// <summary>A notice in the customer's alerts feed. <see cref="Link" /> is a path inside the site.</summary>
public sealed class FintechNotice
{
    #region Properties, Indexers

    /// <summary>Gets when it was raised.</summary>
    public DateTime At { get; init; }

    /// <summary>Gets the text.</summary>
    public string Body { get; init; } = string.Empty;

    /// <summary>Gets the notice number.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets whether the customer has read it.</summary>
    public bool IsRead { get; set; }

    /// <summary>Gets the kind.</summary>
    public FintechAlertKind Kind { get; init; }

    /// <summary>Gets the page it points to, if any.</summary>
    public string? Link { get; init; }

    /// <summary>Gets the title.</summary>
    public string Title { get; init; } = string.Empty;

    #endregion
}

/// <summary>Where a customer stands with the bank.</summary>
public enum FintechCustomerStatus
{
    /// <summary>In good standing.</summary>
    Active,

    /// <summary>Watched by operations (for example after unusual activity).</summary>
    Watch,
}

/// <summary>
/// A bank customer, as operations see them. Only the demo customer has a full ledger in the tab; the others are summaries
/// with their accounts' count and total, and the transfers and disputes seeded for them.
/// </summary>
public sealed record FintechCustomer(string Id, string Name, string Email, string City, DateOnly Since, string Segment, FintechCustomerStatus Status, int AccountCount, decimal TotalBalance);

/// <summary>An internal note operations wrote about a customer (customers never see it).</summary>
public sealed record FintechCustomerNote(int Id, string CustomerId, DateTime At, string Author, string Text);

/// <summary>One entry in the audit log: who did what, when, to what.</summary>
public sealed record FintechAuditEntry(int Id, DateTime At, string Actor, string Role, string Action, string Subject, string Details);
