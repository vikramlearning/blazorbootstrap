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

/// <summary>A bank account of the signed-in (fictitious) customer.</summary>
public sealed record FintechAccount(string Id, string Name, FintechAccountKind Kind, string MaskedNumber, decimal Balance, decimal? CreditLimit)
{
    #region Properties, Indexers

    /// <summary>Gets the amount that can be moved out: the balance, or nothing for a credit card.</summary>
    public decimal Available => Kind == FintechAccountKind.CreditCard ? 0 : Math.Max(0, Balance);

    #endregion
}

/// <summary>A spending or income category.</summary>
public sealed record FintechCategory(string Id, string Name, IconName IconName, bool IsSpending);

/// <summary>A posted or pending transaction. A negative amount is money out of the account.</summary>
public sealed record FintechTransaction(int Id, DateTime Date, string AccountId, string Description, string CategoryId, decimal Amount, bool IsPending);

/// <summary>A monthly spending limit for one category.</summary>
public sealed record FintechBudget(string CategoryId, decimal Limit);

/// <summary>A saved person or company money can be sent to.</summary>
public sealed record FintechPayee(string Id, string Name, string MaskedAccount);

/// <summary>A transfer the user has filled in and not yet confirmed.</summary>
public sealed record FintechTransferDraft(string FromAccountId, string ToId, decimal Amount, DateOnly Date, string? Note);

/// <summary>The outcome of a confirmed transfer.</summary>
public sealed record FintechTransferResult(string Reference, FintechTransferDraft Draft, bool IsScheduled);
