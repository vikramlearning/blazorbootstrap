using System.Globalization;
using System.Text.Json;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

/// <summary>
/// The enterprise baseline of <see cref="FintechState" /> (rule WR-001): the audit log, the header's notifications (for the
/// customer, the same list as the Alerts page, so the bell and the page always agree; for operations, staff notifications
/// raised where someone must act), the header's global search, the review queue's bulk release, reject, and hold, and the
/// checks the forms ask while someone types (payee names and nicknames, routing numbers, budgets). Audit entries and
/// notifications link only to pages on this site, and never carry a full account number.
/// </summary>
public sealed partial class FintechState
{
    #region Fields and Constants

    /// <summary>The site's base route (the same as <c>FintechLayout.Base</c>).</summary>
    private const string SiteBase = FintechSeedData.SiteBase;

    /// <summary>The operations console's base route (the same as <c>FintechOpsLayout.Base</c>).</summary>
    private const string OpsBase = FintechSeedData.OpsBase;

    private const string BudgetsPath = SiteBase + "/budgets";

    private const string PayeesPath = SiteBase + "/payees";

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Releases several held transfers at once from the review queue (operations), each like <see cref="ApproveTransfer" />:
    /// sent now, or scheduled for its date. A transfer that isn't held any more, or whose account no longer holds the money,
    /// is left as it is.
    /// </summary>
    public FintechBulkOutcome BulkApproveTransfers(IReadOnlyCollection<string> references, string? note) =>
        Bulk(references, reference => ApproveTransfer(reference, note), note is { Length: > MaxApproveNote });

    /// <summary>
    /// Holds several scheduled transfers at once for review (operations), each like <see cref="HoldTransfer" />, with one
    /// reason (<see cref="MinReason" /> to <see cref="MaxHoldReason" /> characters). Transfers that aren't scheduled are left
    /// as they are.
    /// </summary>
    public FintechBulkOutcome BulkHoldTransfers(IReadOnlyCollection<string> references, string reason) =>
        Bulk(references, reference => HoldTransfer(reference, reason), reason?.Trim().Length is not (>= MinReason and <= MaxHoldReason));

    /// <summary>
    /// Rejects several held transfers at once (operations), each like <see cref="RejectTransfer" />, with one reason the
    /// customers read (<see cref="MinReason" /> to <see cref="MaxRejectReason" /> characters). Transfers that aren't held any
    /// more are left as they are.
    /// </summary>
    public FintechBulkOutcome BulkRejectTransfers(IReadOnlyCollection<string> references, string reason) =>
        Bulk(references, reference => RejectTransfer(reference, reason), reason?.Trim().Length is not (>= MinReason and <= MaxRejectReason));

    /// <summary>
    /// Returns the signed-in account's notifications for the header's menu, newest first: for the customer, the alerts feed
    /// (the same list, IDs, and read state as the Alerts page); for operations, their staff notifications. Empty until the
    /// one-time code is entered.
    /// </summary>
    public IReadOnlyList<ShowcaseNotification> GetNotificationsForCurrentAccount()
    {
        if (IsOps)
            return staffNotices.Where(notice => notice.AccountId == Identity.Current!.Id).OrderByDescending(notice => notice.At).ThenByDescending(notice => notice.Id).ToList();

        if (!IsVerified)
            return Array.Empty<ShowcaseNotification>();

        return notices.OrderByDescending(notice => notice.At).ThenByDescending(notice => notice.Id).Select(notice => new ShowcaseNotification
        {
            Id = notice.Id,
            AccountId = verifiedAccountId!,
            At = notice.At,
            IsRead = notice.IsRead,
            Title = notice.Title,
            Text = notice.Body,
            Url = IsSiteLink(notice.Link) ? notice.Link : null,
        }).ToList();
    }

    /// <summary>Returns whether a spending category already has this month's budget (every budget is monthly, so one per category).</summary>
    public bool HasBudget(string? categoryId) => budgets.Any(budget => budget.CategoryId == categoryId);

    /// <summary>Returns whether another payee has this nickname, ignoring case and surrounding spaces; the payee being edited doesn't count.</summary>
    public bool IsNicknameTaken(string? nickname, string? exceptPayeeId = null) =>
        !string.IsNullOrWhiteSpace(nickname)
        && payees.Any(payee => payee.Id != exceptPayeeId && string.Equals(payee.Nickname?.Trim(), nickname.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns whether another payee has this name, ignoring case and surrounding spaces; the payee being edited doesn't count.</summary>
    public bool IsPayeeNameTaken(string? name, string? exceptPayeeId = null) =>
        !string.IsNullOrWhiteSpace(name) && payees.Any(payee => payee.Id != exceptPayeeId && string.Equals(payee.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Returns whether a US routing number (ABA number) is valid: 9 digits, spaces and hyphens allowed between them, whose
    /// checksum 3 × (d1 + d4 + d7) + 7 × (d2 + d5 + d8) + (d3 + d6 + d9) is a multiple of 10.
    /// </summary>
    public static bool IsRoutingNumber(string? value)
    {
        if (value is null || value.Any(character => !char.IsAsciiDigit(character) && character is not (' ' or '-')))
            return false;

        var digits = Digits(value);

        if (digits.Length != 9)
            return false;

        var sum = 0;

        for (var index = 0; index < 9; index++)
            sum += (digits[index] - '0') * (index % 3) switch { 0 => 3, 1 => 7, _ => 1 };

        return sum % 10 == 0;
    }

    /// <summary>Marks every notification of the signed-in account as read (the header's "Mark all as read").</summary>
    public void MarkAllNotificationsRead()
    {
        if (IsVerified)
        {
            MarkAllNoticesRead();

            return;
        }

        if (!IsOps || staffNotices.All(notice => notice.AccountId != Identity.Current!.Id || notice.IsRead))
            return;

        foreach (var notice in staffNotices.Where(notice => notice.AccountId == Identity.Current!.Id))
            notice.IsRead = true;

        RaiseChanged();
    }

    /// <summary>Marks one notification of the signed-in account as read; ignores an unknown ID or another account's notification.</summary>
    public void MarkNotificationRead(int id)
    {
        if (IsVerified)
        {
            MarkNoticeRead(id);

            return;
        }

        if (IsOps && staffNotices.FirstOrDefault(notice => notice.Id == id && notice.AccountId == Identity.Current!.Id) is { IsRead: false } notice)
        {
            notice.IsRead = true;
            RaiseChanged();
        }
    }

    /// <summary>
    /// The header's global search, best matches first (exactly the term, then starting with it, then containing it), and
    /// among equal matches by kind and then newest first. Operations find customers (by name, ID, or email), transfers (by
    /// reference), disputes (by number), and AML cases and alerts (by number, like "Case 517" or "Alert 7012"), each opening
    /// its operations page; the customer finds their own transactions
    /// (by merchant or description), payees (by name or nickname), transfers (by reference or recipient), and disputes (by
    /// number or merchant). Returns nothing for a blank term, or until the one-time code is entered.
    /// </summary>
    public IReadOnlyList<ShowcaseSearchResult> Search(string term, int maxResults)
    {
        term = term?.Trim() ?? string.Empty;

        if (term.Length == 0 || maxResults <= 0 || (!IsOps && !IsVerified))
            return Array.Empty<ShowcaseSearchResult>();

        int? Rank(params string?[] values) => ShowcaseSearch.Rank(term, values);

        var hasDigit = term.Any(char.IsAsciiDigit);
        var matches = new List<(int Rank, int Kind, long Order, ShowcaseSearchResult Result)>();

        if (IsOps)
        {
            SearchAml(term, Rank, matches);

            // Kind: 0 a customer, 1 a transfer, 2 a dispute (3 an AML case, 4 an AML alert).
            foreach (var customer in FintechSeedData.Customers())
            {
                if (Rank(customer.Name, customer.Id, customer.Email) is { } found)
                {
                    matches.Add((found, 0, 0, new ShowcaseSearchResult("Customer", customer.Name,
                        $"{customer.Segment} · {customer.City} · {(customer.Status == FintechCustomerStatus.Watch ? "on watch" : "active")} · {customer.Email}", CustomerOpsPath(customer.Id))));
                }
            }

            if (hasDigit)
            {
                foreach (var transfer in transfers)
                {
                    if (Rank(transfer.Reference) is { } found)
                    {
                        matches.Add((found, 1, -transfer.CreatedAt.Ticks, new ShowcaseSearchResult("Transfer", transfer.Reference,
                            $"{Money(transfer.Amount)} from {transfer.CustomerName} to {transfer.ToName} · {StatusWord(transfer.Status)}", TransferOpsPath(transfer.Reference))));
                    }
                }

                foreach (var dispute in disputes)
                {
                    if (Rank(Number(dispute.Id), $"Dispute {Number(dispute.Id)}") is { } found)
                    {
                        matches.Add((found, 2, -dispute.CreatedAt.Ticks, new ShowcaseSearchResult("Dispute", $"Dispute {Number(dispute.Id)}",
                            $"{CustomerName(dispute.CustomerId)} · {Money(dispute.Amount)} at {dispute.Merchant} · {DisputeWord(dispute.Status)}", DisputeOpsPath(dispute.Id))));
                    }
                }
            }
        }
        else
        {
            // Kind: 0 a payee, 1 a transfer, 2 a dispute, 3 a transaction.
            foreach (var payee in payees)
            {
                if (Rank(payee.Name, payee.Nickname) is { } found)
                {
                    matches.Add((found, 0, 0, new ShowcaseSearchResult("Payee", payee.Name,
                        $"{(payee.Nickname is null ? string.Empty : $"\"{payee.Nickname}\" · ")}{payee.BankName} · {payee.MaskedAccount}", $"{PayeesPath}/{Uri.EscapeDataString(payee.Id)}")));
                }
            }

            foreach (var transfer in transfers.Where(transfer => transfer.CustomerId == verifiedAccountId))
            {
                if (Rank(transfer.Reference, transfer.ToName) is { } found)
                {
                    matches.Add((found, 1, -transfer.CreatedAt.Ticks, new ShowcaseSearchResult("Transfer", $"{transfer.Reference} to {transfer.ToName}",
                        $"{Money(transfer.Amount)} · {Day(transfer.Date)} · {StatusWord(transfer.Status)}", TransferPath(transfer))));
                }
            }

            foreach (var dispute in disputes.Where(dispute => dispute.CustomerId == verifiedAccountId))
            {
                if (Rank(Number(dispute.Id), $"Dispute {Number(dispute.Id)}", dispute.Merchant) is { } found)
                {
                    matches.Add((found, 2, -dispute.CreatedAt.Ticks, new ShowcaseSearchResult("Dispute", $"Dispute {Number(dispute.Id)}",
                        $"{Money(dispute.Amount)} at {dispute.Merchant} · {DisputeWord(dispute.Status)}", DisputePath(dispute.Id))));
                }
            }

            foreach (var item in transactions.Where(item => item.Date <= Clock.Now.AddDays(1)))
            {
                if (Rank(item.Description, item.Note) is { } found)
                {
                    matches.Add((found, 3, -item.Date.Ticks, new ShowcaseSearchResult("Transaction", item.Description,
                        $"{item.Date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)} · {(item.Amount < 0 ? "−" : "+")}{Money(Math.Abs(item.Amount))} · {FindCategory(item.CategoryId).Name}",
                        string.Create(CultureInfo.InvariantCulture, $"{SiteBase}/transactions/{item.Id}"))));
                }
            }
        }

        return matches
            .OrderBy(match => match.Rank)
            .ThenBy(match => match.Kind)
            .ThenBy(match => match.Order)
            .ThenBy(match => match.Result.Label, StringComparer.OrdinalIgnoreCase)
            .Take(maxResults)
            .Select(match => match.Result)
            .ToList();
    }

    #endregion

    #region Private Methods

    private static string CustomerOpsPath(string customerId) => $"{OpsBase}/customers/{Uri.EscapeDataString(customerId)}";

    /// <summary>
    /// Returns the records that differ from the seed (changed or added, in list order) and the keys of seeded records that
    /// were removed; <see langword="null" /> for an empty list, so it is left out of the snapshot.
    /// </summary>
    private static (List<T>? Changed, List<TKey>? Removed) Diff<T, TKey>(IReadOnlyList<T> current, IReadOnlyList<T> seeded, Func<T, TKey> key, Func<T, T, bool>? equals = null)
        where TKey : notnull
    {
        var seededByKey = seeded.ToDictionary(key);
        var currentKeys = new HashSet<TKey>();
        var changed = new List<T>();
        equals ??= EqualityComparer<T>.Default.Equals;

        foreach (var item in current)
        {
            var itemKey = key(item);
            currentKeys.Add(itemKey);

            if (!seededByKey.TryGetValue(itemKey, out var original) || !equals(original, item))
                changed.Add(item);
        }

        var removed = seeded.Where(item => !currentKeys.Contains(key(item))).Select(key).ToList();

        return (changed.Count == 0 ? null : changed, removed.Count == 0 ? null : removed);
    }

    private static string DisputeOpsPath(int disputeId) => $"{OpsBase}/disputes/{Number(disputeId)}";

    private static string DisputePath(int disputeId) => $"{SiteBase}/disputes/{Number(disputeId)}";

    /// <summary>A dispute as the audit log names it: its number and the customer, like "Dispute 3101 · Jordan Ashdown".</summary>
    private static string DisputeTarget(FintechDispute dispute) => $"Dispute {Number(dispute.Id)} · {CustomerName(dispute.CustomerId)}";

    private static string DisputeWord(FintechDisputeStatus status) => status switch
    {
        FintechDisputeStatus.Submitted => "submitted",
        FintechDisputeStatus.UnderReview => "under review",
        FintechDisputeStatus.Refunded => "refunded",
        _ => "declined",
    };

    /// <summary>Checks a stored link: none, or a path on this site without spaces, quotes, or a second slash.</summary>
    private static bool IsSiteLink(string? url) => ShowcaseLinks.IsSiteLink(url, SiteBase, allowSiteBase: true);

    /// <summary>
    /// Rebuilds a list from the seed and a snapshot's changes: seeded records in their order (replaced by a stored record
    /// with the same key, left out when removed), then the added records in their stored order. Returns
    /// <see langword="null" /> for an inconsistent change list (a missing record, a repeated key, or a removed key that isn't
    /// seeded), which discards the snapshot.
    /// </summary>
    private static List<T>? Overlay<T, TKey>(IReadOnlyList<T> seeded, List<T>? changed, List<TKey>? removed, Func<T, TKey> key)
        where TKey : notnull
    {
        changed ??= new List<T>();
        removed ??= new List<TKey>();

        if (changed.Count > 10_000 || removed.Count > 10_000 || changed.Any(item => item is null) || removed.Any(item => item is null))
            return null;

        var replacements = new Dictionary<TKey, T>();

        foreach (var item in changed)
        {
            if (key(item) is not { } itemKey || !replacements.TryAdd(itemKey, item))
                return null;
        }

        var seededKeys = seeded.Select(key).ToHashSet();
        var removedKeys = removed.ToHashSet();

        if (removedKeys.Count != removed.Count || removedKeys.Any(item => !seededKeys.Contains(item) || replacements.ContainsKey(item)))
            return null;

        var result = new List<T>(seeded.Count + changed.Count);

        foreach (var item in seeded)
        {
            var itemKey = key(item);

            if (replacements.TryGetValue(itemKey, out var replacement))
                result.Add(replacement);
            else if (!removedKeys.Contains(itemKey))
                result.Add(Copy(item));
        }

        result.AddRange(changed.Where(item => !seededKeys.Contains(key(item))));

        return result;
    }

    /// <summary>
    /// Returns a seeded record to keep in the tab: records are immutable and shared, and the mutable classes (transfers,
    /// disputes, notices, staff notifications) are copied, so changing them never changes the shared seed.
    /// </summary>
    private static T Copy<T>(T item) => item switch
    {
        FintechTransfer transfer => (T)(object)new FintechTransfer
        {
            Amount = transfer.Amount, ClosedAt = transfer.ClosedAt, CreatedAt = transfer.CreatedAt, CustomerId = transfer.CustomerId, CustomerName = transfer.CustomerName,
            Date = transfer.Date, FromAccountId = transfer.FromAccountId, HoldReason = transfer.HoldReason, Note = transfer.Note, Reference = transfer.Reference,
            RejectReason = transfer.RejectReason, Status = transfer.Status, ToId = transfer.ToId, ToName = transfer.ToName, ToOwnAccount = transfer.ToOwnAccount,
        },
        FintechDispute dispute => (T)(object)new FintechDispute
        {
            Amount = dispute.Amount, ClosedAt = dispute.ClosedAt, CreatedAt = dispute.CreatedAt, CustomerId = dispute.CustomerId, Details = dispute.Details,
            EscalatedAt = dispute.EscalatedAt, Id = dispute.Id, Merchant = dispute.Merchant, Reason = dispute.Reason, Resolution = dispute.Resolution, Status = dispute.Status,
            TransactionId = dispute.TransactionId,
        },
        FintechNotice notice => (T)(object)new FintechNotice
        {
            At = notice.At, Body = notice.Body, Id = notice.Id, IsRead = notice.IsRead, Kind = notice.Kind, Link = notice.Link, Title = notice.Title,
        },
        ShowcaseNotification notice => (T)(object)new ShowcaseNotification
        {
            AccountId = notice.AccountId, At = notice.At, Id = notice.Id, IsRead = notice.IsRead, Text = notice.Text, Title = notice.Title, Url = notice.Url,
        },
        FintechAmlAlert or FintechAmlCase => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(item, snapshotOptions), snapshotOptions)!,
        _ => item,
    };

    private static bool SameAudit(ShowcaseAuditEntry left, ShowcaseAuditEntry right) =>
        left.Id == right.Id && left.At == right.At && left.Actor == right.Actor && left.Action == right.Action && left.Target == right.Target
        && left.TargetUrl == right.TargetUrl && left.Details == right.Details;

    private static bool SameDispute(FintechDispute left, FintechDispute right) =>
        left.Id == right.Id && left.Amount == right.Amount && left.ClosedAt == right.ClosedAt && left.CreatedAt == right.CreatedAt && left.CustomerId == right.CustomerId
        && left.Details == right.Details && left.EscalatedAt == right.EscalatedAt && left.Merchant == right.Merchant && left.Reason == right.Reason
        && left.Resolution == right.Resolution && left.Status == right.Status && left.TransactionId == right.TransactionId;

    private static bool SameNotice(FintechNotice left, FintechNotice right) =>
        left.Id == right.Id && left.At == right.At && left.Body == right.Body && left.IsRead == right.IsRead && left.Kind == right.Kind && left.Link == right.Link
        && left.Title == right.Title;

    private static bool SameStaffNotice(ShowcaseNotification left, ShowcaseNotification right) =>
        left.Id == right.Id && left.AccountId == right.AccountId && left.At == right.At && left.IsRead == right.IsRead && left.Text == right.Text && left.Title == right.Title
        && left.Url == right.Url;

    private static bool SameTransfer(FintechTransfer left, FintechTransfer right) =>
        left.Reference == right.Reference && left.Amount == right.Amount && left.ClosedAt == right.ClosedAt && left.CreatedAt == right.CreatedAt
        && left.CustomerId == right.CustomerId && left.CustomerName == right.CustomerName && left.Date == right.Date && left.FromAccountId == right.FromAccountId
        && left.HoldReason == right.HoldReason && left.Note == right.Note && left.RejectReason == right.RejectReason && left.Status == right.Status
        && left.ToId == right.ToId && left.ToName == right.ToName && left.ToOwnAccount == right.ToOwnAccount;

    private static string StatusWord(FintechTransferStatus status) => status switch
    {
        FintechTransferStatus.Completed => "sent",
        FintechTransferStatus.Scheduled => "scheduled",
        FintechTransferStatus.Held => "held for review",
        FintechTransferStatus.Rejected => "rejected",
        _ => "cancelled",
    };

    private static string TransferOpsPath(string reference) => $"{OpsBase}/transfers/{Uri.EscapeDataString(reference)}";

    /// <summary>A transfer as the audit log names it: its reference and the customer, like "TM-260412 · Jordan Ashdown".</summary>
    private static string TransferTarget(FintechTransfer transfer) => $"{transfer.Reference} · {transfer.CustomerName}";

    /// <summary>Runs one operations action on several transfers, raising <see cref="Changed" /> once; Invalid for an empty list or a bad reason.</summary>
    private FintechBulkOutcome Bulk(IReadOnlyCollection<string> references, Func<string, FintechResult> action, bool invalid)
    {
        if (!IsOps)
            return new FintechBulkOutcome(FintechResult.NotAllowed, 0, 0);

        var list = (references ?? Array.Empty<string>()).Where(reference => !string.IsNullOrWhiteSpace(reference)).Distinct(StringComparer.Ordinal).ToList();

        if (list.Count == 0 || invalid)
            return new FintechBulkOutcome(FintechResult.Invalid, 0, 0);

        int changed = 0, unchanged = 0;
        batchDepth++;

        try
        {
            foreach (var reference in list)
            {
                if (action(reference) == FintechResult.Ok)
                    changed++;
                else
                    unchanged++;
            }
        }
        finally
        {
            batchDepth--;
        }

        if (batchDepth == 0 && changedInBatch)
        {
            changedInBatch = false;
            Changed?.Invoke();
        }

        return new FintechBulkOutcome(changed == 0 ? FintechResult.NoChange : FintechResult.Ok, changed, unchanged);
    }

    /// <summary>
    /// Raises a staff notification for an operations account that can sign in (rule WR-001): nothing for anyone without a
    /// sign-in, and nothing for the person who caused it, unless the system did (<paramref name="fromSystem" />). Each
    /// account keeps its newest ones within the limit a restore accepts.
    /// </summary>
    private void NotifyStaff(string accountId, string title, string text, string? url, bool fromSystem = false)
    {
        if (Identity.FindAccount(accountId) is not { } account || !IsStaffRole(account.RoleId) || (!fromSystem && accountId == Identity.Current?.Id))
            return;

        staffNotices.Insert(0, new ShowcaseNotification
        {
            Id = nextStaffNoticeId++,
            AccountId = accountId,
            At = Clock.Now,
            Title = title.Length <= 80 ? title : string.Concat(title.AsSpan(0, 79), "…"),
            Text = text.Length <= 200 ? text : string.Concat(text.AsSpan(0, 199), "…"),
            Url = IsSiteLink(url) ? url : null,
        });

        while (staffNotices.Count > MaxStaffNotices)
            staffNotices.RemoveAt(staffNotices.Count - 1);
    }

    /// <summary>Tells every operations user who can sign in: a transfer held for review, a new dispute, a dispute past its target.</summary>
    private void NotifyOps(string title, string text, string? url, bool fromSystem = false)
    {
        foreach (var account in Identity.Accounts.Where(account => IsStaffRole(account.RoleId)).ToList())
            NotifyStaff(account.Id, title, text, url, fromSystem);
    }

    /// <summary>
    /// Adds an audit entry (rule WR-001): the signed-in person did <paramref name="action" /> (past tense) to
    /// <paramref name="target" />, now, on the site's clock; the system when nobody is signed in.
    /// </summary>
    private void Record(string action, string target, string? targetUrl, string? details) =>
        RecordAs(Identity.Current?.Name ?? FintechSeedData.SystemActor, action, target, targetUrl, details);

    /// <summary>Adds an audit entry for <paramref name="actor" />, now: the system's postings, holds, and escalations. The log keeps the newest entries.</summary>
    private void RecordAs(string actor, string action, string target, string? targetUrl, string? details)
    {
        if (nextAuditId >= 1_000_000 - MaxAudit)
            return;

        audit.Add(new ShowcaseAuditEntry
        {
            Id = nextAuditId++,
            At = Clock.Now,
            Actor = string.IsNullOrWhiteSpace(actor) ? FintechSeedData.SystemActor : actor.Length <= ShowcaseIdentity.MaxNameLength ? actor : actor[..ShowcaseIdentity.MaxNameLength],
            Action = action.Length <= 80 ? action : string.Concat(action.AsSpan(0, 79), "…"),
            Target = Clip(target),
            TargetUrl = IsSiteLink(targetUrl) ? targetUrl : null,
            Details = string.IsNullOrWhiteSpace(details) ? null : details.Length <= 420 ? details : string.Concat(details.AsSpan(0, 419), "…"),
        });

        if (audit.Count > MaxAudit)
            audit.RemoveAt(0);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the audit log, newest first, for operations; empty for customers (they see their own alerts instead).</summary>
    public IReadOnlyList<ShowcaseAuditEntry> AuditEntries =>
        IsOps ? audit.OrderByDescending(entry => entry.At).ThenByDescending(entry => entry.Id).ToList() : Array.Empty<ShowcaseAuditEntry>();

    #endregion
}
