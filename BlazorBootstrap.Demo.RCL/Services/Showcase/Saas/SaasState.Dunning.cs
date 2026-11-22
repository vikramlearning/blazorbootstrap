using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

/// <summary>
/// Dunning, Lumora's signature workflow (AC-7d): a failed charge (a renewal, a trial's end, or a payment someone tries)
/// opens a case that retries on the policy's schedule, then gives a grace period, then suspends the workspace. A charge goes
/// through when the workspace has a card on file that doesn't fail (<see cref="SaasWorkspace.CardFails" />), so the demo is
/// predictable: the customer replaces the card, and the next retry or "Pay now" recovers the tenant. The engine runs on the
/// site's clock whenever billing catches up (a page opens, a snapshot is restored, or the demo clock moves a day). Every
/// step is audited, and the workspace's billing contacts and Lumora's operators are notified where someone must act.
/// </summary>
public sealed partial class SaasState
{
    #region Fields and Constants

    /// <summary>The most days an operator extends a grace period at once.</summary>
    public const int MaxExtensionDays = 14;

    /// <summary>The most days of grace a policy gives.</summary>
    public const int MaxGraceDays = 30;

    /// <summary>The longest reason for an extension or a suspension.</summary>
    public const int MaxDunningReasonLength = 200;

    /// <summary>The furthest the demo clock moves forward, in days (real time included), so a tab's changes stay well inside its storage.</summary>
    public const int MaxDemoDays = 30;

    /// <summary>The latest day of a retry, counted from the failure.</summary>
    public const int MaxRetryDay = 30;

    /// <summary>The most retries a policy schedules.</summary>
    public const int MaxRetries = 5;

    private const int MaxDunningCases = 2000;

    private const int MaxDunningEvents = 60;

    private const string DunningPath = AdminPath + "/dunning";

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Moves the site's clock forward one day (a demo control: dunning is measured in days), then bills and runs dunning
    /// for everything that came due: renewals, trial ends, retries, grace periods. Operators can always do it; a customer can
    /// while their workspace is in dunning.
    /// </summary>
    public SaasResult AdvanceDemoDay()
    {
        if (!IsOperator && (CurrentWorkspace is not { } workspace || OpenDunningCase(workspace.Id) is null))
            return SaasResult.NotAllowed;

        var next = Clock.Elapsed + TimeSpan.FromDays(1);

        if (next > TimeSpan.FromDays(MaxDemoDays) || !Clock.TryRestore(next.TotalSeconds))
            return SaasResult.Invalid;

        CatchUpBilling();
        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>
    /// Retries several open cases now (operator, from the dunning list); each goes through or fails like "Retry now". Returns
    /// how many recovered (changed) and how many still fail or weren't open (unchanged).
    /// </summary>
    public SaasBulkOutcome BulkRetryDunning(IReadOnlyCollection<int> caseIds)
    {
        if (!IsOperator)
            return new SaasBulkOutcome(SaasResult.NotAllowed, 0, 0);

        var ids = (caseIds ?? Array.Empty<int>()).Distinct().ToList();

        if (ids.Count == 0)
            return new SaasBulkOutcome(SaasResult.Invalid, 0, 0);

        int recovered = 0, failing = 0;

        foreach (var id in ids)
        {
            if (FindDunningCase(id) is not { IsOpen: true } item || FindWorkspace(item.WorkspaceId) is null)
            {
                failing++;
                continue;
            }

            if (Attempt(item.Id, Clock.Now, Identity.Current!.Name, AttemptKind.Operator))
                recovered++;
            else
                failing++;
        }

        Changed?.Invoke();

        return new SaasBulkOutcome(recovered > 0 ? SaasResult.Ok : SaasResult.CardDeclined, recovered, failing);
    }

    /// <summary>Extends the grace period of several cases (operator, from the dunning list); cases still retrying or recovered are left as they are.</summary>
    public SaasBulkOutcome BulkExtendGrace(IReadOnlyCollection<int> caseIds, int days, string? reason)
    {
        if (!IsOperator)
            return new SaasBulkOutcome(SaasResult.NotAllowed, 0, 0);

        reason = reason?.Trim() ?? string.Empty;
        var ids = (caseIds ?? Array.Empty<int>()).Distinct().ToList();

        if (ids.Count == 0 || days is < 1 or > MaxExtensionDays || !IsText(reason, MaxDunningReasonLength))
            return new SaasBulkOutcome(SaasResult.Invalid, 0, 0);

        int extended = 0, skipped = 0;

        foreach (var id in ids)
        {
            if (ExtendCase(id, days, reason))
                extended++;
            else
                skipped++;
        }

        if (extended == 0)
            return new SaasBulkOutcome(SaasResult.NoChange, 0, skipped);

        Changed?.Invoke();

        return new SaasBulkOutcome(SaasResult.Ok, extended, skipped);
    }

    /// <summary>Returns a short description of a policy, such as "Retry 1, 3, and 7 days after a failed charge, then 7 days of grace, then suspend the workspace."</summary>
    public static string DescribePolicy(IReadOnlyList<int> retryDays, int graceDays) =>
        $"Retry {SaasSeedData.DaysText(retryDays).Replace(" after the failure", " after a failed charge", StringComparison.Ordinal)}, then "
        + (graceDays == 0 ? "suspend the workspace at once." : $"{graceDays} {(graceDays == 1 ? "day" : "days")} of grace, then suspend the workspace.");

    /// <summary>
    /// Extends a case's grace period by 1 to 14 days with a reason (operator). A suspended workspace becomes usable again
    /// (past due) until the new end. The workspace's billing contacts are told the new date.
    /// </summary>
    public SaasResult ExtendGrace(int caseId, int days, string? reason)
    {
        if (!IsOperator)
            return SaasResult.NotAllowed;

        reason = reason?.Trim() ?? string.Empty;

        if (FindDunningCase(caseId) is not { } item)
            return SaasResult.NotFound;

        if (days is < 1 or > MaxExtensionDays || !IsText(reason, MaxDunningReasonLength))
            return SaasResult.Invalid;

        if (item.Stage is not (SaasDunningStage.Grace or SaasDunningStage.Suspended))
            return SaasResult.WrongState;

        if (!ExtendCase(caseId, days, reason))
            return SaasResult.Invalid;

        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>Returns a dunning case by its ID, or <see langword="null" />.</summary>
    public SaasDunningCase? FindDunningCase(int caseId) => dunning.FirstOrDefault(item => item.Id == caseId);

    /// <summary>Returns the grace end an extension would give, or <see langword="null" /> when the case can't be extended.</summary>
    public DateTime? GraceEndAfterExtension(int caseId, int days) => FindDunningCase(caseId) switch
    {
        { Stage: SaasDunningStage.Grace, GraceEndsAt: { } ends } => ends.AddDays(days),
        { Stage: SaasDunningStage.Suspended } => Clock.Now.AddDays(days),
        _ => null,
    };

    /// <summary>Returns the workspace's open dunning case (retrying, in grace, or suspended), or <see langword="null" />.</summary>
    public SaasDunningCase? OpenDunningCase(int workspaceId) => dunning.FirstOrDefault(item => item.WorkspaceId == workspaceId && item.IsOpen);

    /// <summary>
    /// Returns what is wrong with a retry schedule and grace period, or <see langword="null" />: 1 to 5 retries, each on day
    /// 1 to 30 after the failure and later than the one before, and 0 to 30 days of grace.
    /// </summary>
    public static string? PolicyProblem(IReadOnlyList<int> retryDays, int graceDays)
    {
        if (retryDays.Count is < 1 or > MaxRetries)
            return $"Schedule 1 to {MaxRetries} retries.";

        if (retryDays.Any(day => day is < 1 or > MaxRetryDay))
            return $"Use retry days from 1 to {MaxRetryDay} after the failure.";

        for (var index = 1; index < retryDays.Count; index++)
        {
            if (retryDays[index] <= retryDays[index - 1])
                return "List the retry days in order, each later than the one before (like 1, 3, 7).";
        }

        return graceDays is < 0 or > MaxGraceDays ? $"Use 0 to {MaxGraceDays} days of grace." : null;
    }

    /// <summary>Retries an open case now (operator "Retry now"): Ok when the charge went through, CardDeclined or CardRequired when it failed again.</summary>
    public SaasResult RetryDunningNow(int caseId)
    {
        if (!IsOperator)
            return SaasResult.NotAllowed;

        if (FindDunningCase(caseId) is not { } item || FindWorkspace(item.WorkspaceId) is not { } workspace)
            return SaasResult.NotFound;

        if (!item.IsOpen)
            return SaasResult.WrongState;

        var paid = Attempt(item.Id, Clock.Now, Identity.Current!.Name, AttemptKind.Operator);
        Changed?.Invoke();

        return paid ? SaasResult.Ok : workspace.CardLast4 is null ? SaasResult.CardRequired : SaasResult.CardDeclined;
    }

    /// <summary>
    /// Charges a tenant's next renewal now with a card that declines (operator, a demo shortcut to start dunning on a tenant
    /// in good standing, such as the demo customer's workspace). The renewal moves to the period after.
    /// </summary>
    public SaasResult SimulateFailedCharge(int workspaceId)
    {
        if (!IsOperator)
            return SaasResult.NotAllowed;

        if (FindWorkspace(workspaceId) is not { } workspace)
            return SaasResult.NotFound;

        if (workspace.Status != SaasWorkspaceStatus.Active || PlanOf(workspace).MonthlyPricePerSeat == 0 || OpenDunningCase(workspace.Id) is not null)
            return SaasResult.WrongState;

        if (dunning.Count >= MaxDunningCases)
            return SaasResult.Invalid;

        var failing = workspace.CardLast4 is null ? workspace : workspace with { CardFails = SaasFailureReason.CardDeclined };
        Replace(failing);
        Record("Failed charge simulated", workspace.Name, TenantPath(workspace.Id),
            $"Demo: the next renewal was charged early, and the {(workspace.CardLast4 is null ? "missing card" : $"{workspace.CardBrand} ending in {workspace.CardLast4}")} failed.");
        Renew(failing, early: true);
        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>Suspends a workspace in dunning before its grace ends (operator), with a reason; paying restores it.</summary>
    public SaasResult SuspendForNonPayment(int caseId, string? reason)
    {
        if (!IsOperator)
            return SaasResult.NotAllowed;

        reason = reason?.Trim() ?? string.Empty;

        if (FindDunningCase(caseId) is not { } item)
            return SaasResult.NotFound;

        if (!IsText(reason, MaxDunningReasonLength))
            return SaasResult.Invalid;

        if (item.Stage is not (SaasDunningStage.Retrying or SaasDunningStage.Grace))
            return SaasResult.WrongState;

        Suspend(item.Id, Clock.Now, Identity.Current!.Name, $"Suspended early by {Identity.Current!.Name}: {reason}");
        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>
    /// Saves the dunning policy (operator): 1 to 5 increasing retry days from 1 to 30, 0 to 30 days of grace, and whether the
    /// customer is emailed (a label). It applies to charges that fail from now on.
    /// </summary>
    public SaasResult UpdateDunningPolicy(IReadOnlyList<int> retryDays, int graceDays, bool emailCustomer)
    {
        if (!IsOperator)
            return SaasResult.NotAllowed;

        if (retryDays is null || PolicyProblem(retryDays, graceDays) is not null)
            return SaasResult.Invalid;

        if (retryDays.SequenceEqual(dunningPolicy.RetryDays) && graceDays == dunningPolicy.GraceDays && emailCustomer == dunningPolicy.EmailCustomer)
            return SaasResult.NoChange;

        var before = dunningPolicy;
        dunningPolicy = new SaasDunningPolicy(retryDays.ToArray(), graceDays, emailCustomer);
        Record("Dunning policy updated", "Dunning policy", DunningPath,
            Changes(("Retries", SaasSeedData.DaysText(before.RetryDays), SaasSeedData.DaysText(retryDays)), ("Grace", Days(before.GraceDays), Days(graceDays)),
                ("Customer emails", before.EmailCustomer ? "on" : "off", emailCustomer ? "on" : "off")));
        Changed?.Invoke();

        return SaasResult.Ok;
    }

    #endregion

    #region Private Methods

    private static string Days(int days) => days == 1 ? "1 day" : string.Create(CultureInfo.InvariantCulture, $"{days} days");

    private static string ShortDate(DateTime value) => value.ToString("MMM d", CultureInfo.InvariantCulture);

    /// <summary>
    /// Tries a case's charge at <paramref name="at" />. It goes through when the workspace has a card that doesn't fail; then
    /// the case recovers. A scheduled retry that fails moves the schedule on (and starts grace after the last one); a retry
    /// by an operator or the customer that fails is recorded without changing the schedule. Returns whether it was paid.
    /// </summary>
    private bool Attempt(int caseId, DateTime at, string by, AttemptKind kind)
    {
        if (FindDunningCase(caseId) is not { IsOpen: true } item || FindWorkspace(item.WorkspaceId) is not { } workspace)
            return false;

        if (workspace.CardLast4 is not null && workspace.CardFails is null)
        {
            Recover(item, workspace, at, by, kind == AttemptKind.Scheduled
                ? string.Create(CultureInfo.InvariantCulture, $"Charged to the {workspace.CardBrand} ending in {workspace.CardLast4} on retry {item.RetriesDone + 1}.")
                : $"Charged to the {workspace.CardBrand} ending in {workspace.CardLast4} by {by}.");

            return true;
        }

        var reason = workspace.CardLast4 is null ? SaasFailureReason.NoCard : workspace.CardFails ?? SaasFailureReason.CardDeclined;
        var retries = kind == AttemptKind.Scheduled ? item.RetriesDone + 1 : item.RetriesDone;
        var detail = kind == AttemptKind.Scheduled
            ? string.Create(CultureInfo.InvariantCulture, $"Retry {retries} of {item.RetryDays.Count}: {SaasSeedData.FailureText(reason)}")
            : $"Retry by {by}: {SaasSeedData.FailureText(reason)}";
        var step = new SaasDunningEvent(at, SaasDunningEventKind.RetryFailed, by, detail);
        var updated = item with { Reason = reason, RetriesDone = retries, Events = AddEvent(item, step) };

        if (kind == AttemptKind.Scheduled)
            updated = updated with { NextRetryAt = retries < item.RetryDays.Count ? item.FailedAt.AddDays(item.RetryDays[retries]) : null };

        ReplaceCase(updated);
        RecordStep(updated, step, workspace.Name);

        var next = updated.NextRetryAt is { } nextRetry ? $" The next retry is on {ShortDate(nextRetry)}." : string.Empty;
        TellBillingContacts(workspace.Id, $"A payment retry failed: {Money(item.Amount)}", $"{Capitalize(SaasSeedData.FailureText(reason))}.{next} Update your card in Billing to keep {workspace.Name} running.",
            IconName.ExclamationTriangle, fromSystem: kind == AttemptKind.Scheduled);

        if (kind == AttemptKind.Scheduled && retries >= item.RetryDays.Count)
            StartGrace(updated.Id, at);

        return false;
    }

    private static IReadOnlyList<SaasDunningEvent> AddEvent(SaasDunningCase item, SaasDunningEvent step) =>
        item.Events.Concat(new[] { step }).TakeLast(MaxDunningEvents).ToList();

    private static string Capitalize(string text) => text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text[1..];

    /// <summary>Adds days of grace to a case in grace, or reopens grace for a suspended one (the workspace is usable again); returns whether it changed.</summary>
    private bool ExtendCase(int caseId, int days, string reason)
    {
        if (FindDunningCase(caseId) is not { } item || FindWorkspace(item.WorkspaceId) is not { } workspace || GraceEndAfterExtension(caseId, days) is not { } ends)
            return false;

        var wasSuspended = item.Stage == SaasDunningStage.Suspended;
        var by = Identity.Current?.Name ?? SaasSeedData.SystemActorName;
        var step = new SaasDunningEvent(Clock.Now, SaasDunningEventKind.GraceExtended, by,
            $"{Days(days)} more, to {ShortDate(ends)}{(wasSuspended ? "; the workspace is usable again" : string.Empty)}: {reason}");
        var updated = item with { Stage = SaasDunningStage.Grace, GraceEndsAt = ends, Events = AddEvent(item, step) };
        ReplaceCase(updated);

        if (wasSuspended && workspace.Status == SaasWorkspaceStatus.Suspended)
        {
            Replace(workspace with { Status = SaasWorkspaceStatus.PastDue });
            Log(workspace.Id, "Usable again", $"Grace extended to {ShortDate(ends)}", IconName.HourglassSplit);
        }

        RecordStep(updated, step, workspace.Name);
        TellBillingContacts(workspace.Id, $"You have until {ShortDate(ends)} to pay", $"Lumora gave {workspace.Name} {Days(days)} more to pay {Money(item.Amount)}. Update your card and pay in Billing.",
            IconName.HourglassSplit, fromSystem: false);

        return true;
    }

    /// <summary>
    /// Runs the dunning steps that came due on the site's clock, in time order: scheduled retries, the start of grace after
    /// the last one, and suspensions when grace ends. Cases of archived workspaces wait. Returns whether anything changed.
    /// </summary>
    private bool RunDunning()
    {
        var changed = false;
        var now = Clock.Now;

        foreach (var id in dunning.Where(item => item.IsOpen).Select(item => item.Id).ToList())
        {
            for (var guard = 0; guard < MaxRetries + 3; guard++)
            {
                if (FindDunningCase(id) is not { } item || FindWorkspace(item.WorkspaceId) is not { Status: not SaasWorkspaceStatus.Archived })
                    break;

                if (item is { Stage: SaasDunningStage.Retrying, NextRetryAt: { } due } && due <= now)
                    Attempt(id, due, SaasSeedData.SystemActorName, AttemptKind.Scheduled);
                else if (item is { Stage: SaasDunningStage.Grace, GraceEndsAt: { } ends } && ends <= now)
                    Suspend(id, ends, SaasSeedData.SystemActorName, $"Grace period ended with {Money(item.Amount)} unpaid.");
                else
                    break;

                changed = true;
            }
        }

        return changed;
    }

    /// <summary>Records a dunning step in the audit log with the same wording the seed uses.</summary>
    private void RecordStep(SaasDunningCase item, SaasDunningEvent step, string workspaceName)
    {
        var entry = SaasSeedData.DunningAudit(item, step, workspaceName);
        Record(entry.At, entry.Actor, entry.Action, entry.Target, entry.Url, entry.Details);
    }

    /// <summary>
    /// A charge went through: the invoice is paid, the case recovers, and a workspace that was past due (or suspended by
    /// dunning) is active again. The billing contacts and Lumora's operators are told.
    /// </summary>
    private void Recover(SaasDunningCase item, SaasWorkspace workspace, DateTime at, string by, string detail)
    {
        if (invoices.FirstOrDefault(invoice => invoice.Number == item.InvoiceNumber) is { Status: SaasInvoiceStatus.Open } invoice)
            Pay(invoice, workspace);

        var current = FindWorkspace(workspace.Id) ?? workspace;

        if (item.Stage == SaasDunningStage.Suspended && current.Status == SaasWorkspaceStatus.Suspended
            && !invoices.Any(other => other.WorkspaceId == workspace.Id && other.Status == SaasInvoiceStatus.Open))
            Replace(current with { Status = SaasWorkspaceStatus.Active });

        var step = new SaasDunningEvent(at, SaasDunningEventKind.Paid, by, detail);
        var updated = item with { Stage = SaasDunningStage.Recovered, NextRetryAt = null, ClosedAt = at, Events = AddEvent(item, step) };
        ReplaceCase(updated);
        Log(workspace.Id, "Payment recovered", $"{Money(item.Amount)} on {item.InvoiceNumber}", IconName.CheckCircle);
        RecordStep(updated, step, workspace.Name);

        var bySystem = by == SaasSeedData.SystemActorName;
        TellBillingContacts(workspace.Id, $"Payment received: {Money(item.Amount)}", $"{item.InvoiceNumber} is paid, and {workspace.Name} is in good standing again.", IconName.CheckCircle, bySystem);
        NotifyOperators($"Payment recovered: {workspace.Name}", $"{item.InvoiceNumber} ({Money(item.Amount)}) is paid after {Plural(item.Attempts, "attempt", "attempts")}.", TenantPath(workspace.Id),
            IconName.CheckCircle, bySystem);
    }

    /// <summary>Formats a count with its noun, like "1 invoice" or "1,204 invoices"; <paramref name="plural" /> defaults to the noun plus "s".</summary>
    internal static string Plural(int count, string noun, string? plural = null) =>
        count == 1 ? $"1 {noun}" : $"{count.ToString("#,0", CultureInfo.InvariantCulture)} {plural ?? noun + "s"}";

    private void ReplaceCase(SaasDunningCase updated)
    {
        var index = dunning.FindIndex(item => item.Id == updated.Id);

        if (index >= 0)
            dunning[index] = updated;
    }

    /// <summary>
    /// Opens a case for a charge that failed (a renewal or a trial's end without a working card): the first retry is
    /// scheduled from the policy in force now, the failure is audited, and the billing contacts and operators are told.
    /// </summary>
    private void StartCase(SaasWorkspace workspace, string invoiceNumber, decimal amount, SaasFailureReason reason, DateTime at)
    {
        if (dunning.Count >= MaxDunningCases || OpenDunningCase(workspace.Id) is not null)
            return;

        var policy = dunningPolicy;
        var step = new SaasDunningEvent(at, SaasDunningEventKind.Failed, SaasSeedData.SystemActorName, SaasSeedData.FailureText(reason));
        var item = new SaasDunningCase(nextDunningId++, workspace.Id, invoiceNumber, amount, reason, at, policy.RetryDays.ToArray(), policy.GraceDays, SaasDunningStage.Retrying, 0,
            at.AddDays(policy.RetryDays[0]), null, null, new[] { step });
        dunning.Add(item);
        Log(workspace.Id, "Payment failed", $"{Money(amount)} on {invoiceNumber}: {SaasSeedData.FailureText(reason)}", IconName.ExclamationTriangle);
        RecordStep(item, step, workspace.Name);

        TellBillingContacts(workspace.Id, $"Payment failed: {Money(amount)}", $"{Capitalize(SaasSeedData.FailureText(reason))}. We'll try again on {ShortDate(item.NextRetryAt!.Value)}. "
            + $"Update your card in Billing to keep {workspace.Name} running.", IconName.ExclamationTriangle, fromSystem: true);
        NotifyOperators($"Payment failed: {workspace.Name}", $"{invoiceNumber}, {Money(amount)}: {SaasSeedData.FailureText(reason)}. Retries on {SaasSeedData.DaysText(item.RetryDays)}.",
            InvoicePath(invoiceNumber), IconName.ExclamationTriangle, fromSystem: true);
    }

    /// <summary>The last scheduled retry failed: grace starts (or, with no grace days, the workspace is suspended at once).</summary>
    private void StartGrace(int caseId, DateTime at)
    {
        if (FindDunningCase(caseId) is not { } item || FindWorkspace(item.WorkspaceId) is not { } workspace)
            return;

        if (item.GraceDays == 0)
        {
            Suspend(caseId, at, SaasSeedData.SystemActorName, "Every retry failed, and the policy gives no grace period.");

            return;
        }

        var ends = at.AddDays(item.GraceDays);
        var step = new SaasDunningEvent(at, SaasDunningEventKind.GraceStarted, SaasSeedData.SystemActorName, $"Suspends on {ShortDate(ends)} unless the invoice is paid.");
        var updated = item with { Stage = SaasDunningStage.Grace, GraceEndsAt = ends, NextRetryAt = null, Events = AddEvent(item, step) };
        ReplaceCase(updated);
        Log(workspace.Id, "Grace period started", $"Suspends on {ShortDate(ends)} unless paid", IconName.HourglassSplit);
        RecordStep(updated, step, workspace.Name);

        TellBillingContacts(workspace.Id, $"Last chance to pay {Money(item.Amount)}", $"Every retry failed. {workspace.Name} is suspended on {ShortDate(ends)} unless the invoice is paid: update your card in Billing.",
            IconName.HourglassSplit, fromSystem: true);
        NotifyOperators($"{workspace.Name} is in its grace period", $"Every retry of {item.InvoiceNumber} ({Money(item.Amount)}) failed. It is suspended on {ShortDate(ends)} unless paid.",
            DunningPath, IconName.HourglassSplit, fromSystem: true);
    }

    /// <summary>Suspends the workspace for the unpaid invoice: by the engine when grace ends, or early by an operator.</summary>
    private void Suspend(int caseId, DateTime at, string by, string detail)
    {
        if (FindDunningCase(caseId) is not { } item || FindWorkspace(item.WorkspaceId) is not { } workspace)
            return;

        var step = new SaasDunningEvent(at, SaasDunningEventKind.Suspended, by, detail);
        var updated = item with { Stage = SaasDunningStage.Suspended, NextRetryAt = null, Events = AddEvent(item, step) };
        ReplaceCase(updated);

        if (workspace.Status is SaasWorkspaceStatus.PastDue or SaasWorkspaceStatus.Active)
            Replace(workspace with { Status = SaasWorkspaceStatus.Suspended });

        Log(workspace.Id, "Suspended for non-payment", $"{Money(item.Amount)} on {item.InvoiceNumber} is unpaid", IconName.SlashCircle);
        RecordStep(updated, step, workspace.Name);

        var bySystem = by == SaasSeedData.SystemActorName;
        TellBillingContacts(workspace.Id, "Your workspace is suspended", $"{workspace.Name} is read-only until {Money(item.Amount)} is paid. Update your card and select Pay now in Billing to restore it.",
            IconName.SlashCircle, bySystem);
        NotifyOperators($"{workspace.Name} was suspended", $"{item.InvoiceNumber} ({Money(item.Amount)}) is unpaid. {detail}", TenantPath(workspace.Id), IconName.SlashCircle, bySystem);
    }

    /// <summary>Tells the workspace's owners, admins, and billing members who can sign in (rule WR-001).</summary>
    private void TellBillingContacts(int workspaceId, string title, string text, IconName iconName, bool fromSystem)
    {
        foreach (var accountId in members.Where(member => member.WorkspaceId == workspaceId && member.AccountId is not null && member.Role is not SaasMemberRole.Member)
                     .Select(member => member.AccountId!).Distinct().ToList())
            Notify(accountId, title, text, BillingPath, iconName, fromSystem);
    }

    /// <summary>
    /// An operator made a workspace suspended by dunning usable again (the tenant editor or the bulk bar): the case goes back
    /// to grace for the policy's grace days (at least one), so it is suspended again if nothing is paid.
    /// </summary>
    private void ReopenGraceAfterReactivation(SaasWorkspace before, SaasWorkspace after)
    {
        if (before.Status != SaasWorkspaceStatus.Suspended || after.Status is SaasWorkspaceStatus.Suspended or SaasWorkspaceStatus.Archived
            || OpenDunningCase(after.Id) is not { Stage: SaasDunningStage.Suspended } item)
            return;

        var ends = Clock.Now.AddDays(Math.Max(1, dunningPolicy.GraceDays));
        var step = new SaasDunningEvent(Clock.Now, SaasDunningEventKind.Reactivated, Identity.Current?.Name ?? SaasSeedData.SystemActorName,
            $"Usable again with the invoice unpaid; grace until {ShortDate(ends)}.");
        var updated = item with { Stage = SaasDunningStage.Grace, GraceEndsAt = ends, Events = AddEvent(item, step) };
        ReplaceCase(updated);
        RecordStep(updated, step, after.Name);
    }

    /// <summary>A customer saved a new card while a payment is failing: the case notes it, and Lumora's operators are told.</summary>
    private void NoteCardUpdate(SaasWorkspace workspace, string cardText)
    {
        if (OpenDunningCase(workspace.Id) is not { } item)
            return;

        var step = new SaasDunningEvent(Clock.Now, SaasDunningEventKind.CardUpdated, Identity.Current?.Name ?? "The customer", cardText);
        ReplaceCase(item with { Events = AddEvent(item, step) });
        NotifyOperators($"{workspace.Name} updated its card", $"{cardText} is on file for {item.InvoiceNumber} ({Money(item.Amount)}). The next retry or the customer's Pay now charges it.",
            TenantPath(workspace.Id), IconName.CreditCard);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets whether the signed-in member may fix a payment: owners and billing members, also while the workspace is suspended by dunning.</summary>
    public bool CanFixPayment => CurrentMember is { Role: SaasMemberRole.Owner or SaasMemberRole.Billing }
        && CurrentWorkspace is { } workspace && (IsWorkspaceUsable || (workspace.Status == SaasWorkspaceStatus.Suspended && OpenDunningCase(workspace.Id) is not null));

    /// <summary>Gets every dunning case, the newest failure first (for operators; empty for anyone else).</summary>
    public IReadOnlyList<SaasDunningCase> DunningCases => IsOperator ? dunning.OrderByDescending(item => item.FailedAt).ThenByDescending(item => item.Id).ToList() : Array.Empty<SaasDunningCase>();

    /// <summary>Gets the money in open dunning cases: what Lumora loses if nobody pays.</summary>
    public decimal DunningAtRisk => dunning.Where(item => item.IsOpen).Sum(item => item.Amount);

    /// <summary>Gets the dunning policy.</summary>
    public SaasDunningPolicy DunningPolicy => dunningPolicy;

    #endregion

    #region Nested Types

    /// <summary>Who tries a charge: the engine on the schedule, an operator, or the customer.</summary>
    private enum AttemptKind
    {
        Scheduled,
        Operator,
        Customer,
    }

    #endregion
}
