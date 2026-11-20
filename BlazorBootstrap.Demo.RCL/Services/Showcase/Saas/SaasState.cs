using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

/// <summary>
/// State of Lumora for the current browser tab, shared by the public site (visitors), the customer workspace, and the
/// operator console: the plans, the customer workspaces with their teams, invoices, and activity, notifications, sales leads,
/// the audit log, and the demo sign-in. It is kept in the tab's <c>sessionStorage</c> through the showcase foundation, so
/// it survives a reload in the tab; nothing is sent or charged. The enterprise baseline (audit log, notifications, global
/// search, and the lists' bulk actions) is in <c>SaasState.Enterprise.cs</c>.
/// </summary>
public sealed partial class SaasState : IShowcaseEnterpriseState
{
    #region Fields and Constants

    /// <summary>The customer workspace's start page.</summary>
    public const string AppPath = "/showcase/saas/app";

    /// <summary>The customer workspace's billing page.</summary>
    public const string BillingPath = AppPath + "/billing";

    /// <summary>The operator console's start page.</summary>
    public const string AdminPath = "/showcase/saas/admin";

    /// <summary>The operator's leads inbox.</summary>
    public const string LeadsPath = AdminPath + "/leads";

    /// <summary>The operator's plan editor.</summary>
    public const string PlansPath = AdminPath + "/plans";

    /// <summary>The longest lead name or company.</summary>
    public const int MaxLeadTextLength = 60;

    /// <summary>The longest lead message or sales note.</summary>
    public const int MaxLeadMessageLength = 1000;

    /// <summary>The longest reason for a bulk lead change, added to each note.</summary>
    public const int MaxLeadReasonLength = 200;

    /// <summary>The longest subdomain.</summary>
    public const int MaxSubdomainLength = 30;

    /// <summary>The shortest subdomain.</summary>
    public const int MinSubdomainLength = 3;

    /// <summary>The longest workspace name.</summary>
    public const int MaxWorkspaceNameLength = 60;

    /// <summary>The lowest price of a paid plan per seat a month; lower prices would round to $0 on annual billing.</summary>
    public const decimal MinPaidSeatPrice = 1m;

    /// <summary>The most workspaces a tab holds.</summary>
    public const int MaxWorkspaces = 500;

    private const int MaxActivities = 3000;

    private const int MaxAudit = 1000;

    /// <summary>The longest audit text (action, record, details).</summary>
    private const int MaxAuditTextLength = 300;

    private const int MaxInvoices = 6000;

    private const int MaxLeads = 500;

    private const int MaxMembers = 5000;

    private const int MaxNoticeTextLength = 240;

    private const int MaxNoticeTitleLength = 80;

    private const int MaxNotices = 2000;

    private const int MaxRenewalsPerRun = 24;

    private static readonly string[] cardBrands = ShowcaseCards.TestCards.Select(card => card.Brand).Distinct().ToArray();

    /// <summary>The fresh tab's data, built once: a snapshot keeps only what differs from it.</summary>
    private static readonly Lazy<SeedSet> seed = new(SeedSet.Build);

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<SaasActivity> activities;

    private readonly List<ShowcaseAuditEntry> audit;

    private readonly List<SaasInvoice> invoices;

    private readonly List<SaasLead> leads;

    private readonly List<SaasMember> members;

    private readonly List<SaasNotice> notices;

    private readonly List<SaasPlan> plans;

    private readonly decimal seedMrr;

    private readonly List<SaasWorkspace> workspaces;

    private bool annualBilling = true;

    private SaasAnalyticsRange analyticsRange = SaasAnalyticsRange.Last30Days;

    private bool isDarkTheme;

    private int nextAuditId;

    private int nextLeadId;

    private int nextMemberId;

    private int nextNoticeId;

    private int nextWorkspaceId;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out.</summary>
    public SaasState()
    {
        var data = seed.Value;
        plans = data.Plans.ToList();
        workspaces = data.Workspaces.ToList();
        members = data.Members.ToList();
        invoices = data.Invoices.ToList();
        activities = data.Activities.ToList();
        notices = data.Notices.ToList();
        leads = data.Leads.ToList();
        audit = data.Audit.ToList();
        nextAuditId = audit.Count + 1;
        nextWorkspaceId = workspaces.Max(workspace => workspace.Id) + 1;
        nextMemberId = members.Max(member => member.Id) + 1;
        nextNoticeId = notices.Max(notice => notice.Id) + 1;
        nextLeadId = leads.Max(lead => lead.Id) + 1;
        seedMrr = Mrr;

        Identity = new ShowcaseIdentity(SaasSeedData.Roles(), SaasSeedData.DemoAccounts());
        Identity.Changed += () =>
        {
            SyncMemberProfile();
            Changed?.Invoke();
        };
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Bills whatever has come due since the tab's clock last looked: trials that ended and renewals that passed. A card on
    /// file pays the invoice; without one the invoice stays open and the workspace is past due. Pages call this when they
    /// open; it does nothing on a fresh tab, where every date is in the future.
    /// </summary>
    public void CatchUpBilling()
    {
        var today = Clock.Today;
        var changed = false;

        foreach (var original in workspaces.ToList())
        {
            var workspace = original;

            for (var run = 0; run < MaxRenewalsPerRun && workspace.Status is SaasWorkspaceStatus.Trial or SaasWorkspaceStatus.Active or SaasWorkspaceStatus.PastDue
                 && workspace.RenewsOn.Date <= today; run++)
            {
                workspace = Renew(workspace);
                changed = true;
            }
        }

        if (changed)
            Changed?.Invoke();
    }

    /// <summary>
    /// Charges the card on file for an open invoice (operator "Retry payment"); a past-due workspace with nothing else open
    /// becomes active. The workspace's owners and billing members who can sign in are told the payment was received.
    /// </summary>
    public SaasResult CollectInvoice(string number)
    {
        if (!IsOperator)
            return SaasResult.NotAllowed;

        if (invoices.FirstOrDefault(invoice => invoice.Number == number) is not { Status: SaasInvoiceStatus.Open } invoice || FindWorkspace(invoice.WorkspaceId) is not { } workspace)
            return SaasResult.NotFound;

        if (workspace.CardLast4 is null)
            return SaasResult.CardRequired;

        Pay(invoice, workspace);
        Record("Payment collected", InvoiceTarget(invoice.Number), InvoicePath(invoice.Number), $"{workspace.Name}: {Money(invoice.Amount)} from the {workspace.CardBrand} ending in {workspace.CardLast4}");

        foreach (var accountId in OthersToTell(workspace.Id, member => member.Role is SaasMemberRole.Owner or SaasMemberRole.Billing))
        {
            Notify(accountId, $"Payment received for {invoice.Number}", $"{Money(invoice.Amount)} was charged to the {workspace.CardBrand} ending in {workspace.CardLast4}. Thank you.",
                AppInvoicePath(invoice.Number), IconName.Receipt);
        }

        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>Returns the money collected and refunded per calendar month, oldest first, for the last <paramref name="months" /> months.</summary>
    public IReadOnlyList<SaasMonthRevenue> CollectedByMonth(int months)
    {
        var first = new DateTime(Clock.Today.Year, Clock.Today.Month, 1).AddMonths(-(Math.Clamp(months, 1, 36) - 1));

        return Enumerable.Range(0, Math.Clamp(months, 1, 36))
            .Select(offset => first.AddMonths(offset))
            .Select(month => new SaasMonthRevenue(month,
                invoices.Where(invoice => invoice.Status != SaasInvoiceStatus.Open && invoice.IssuedOn.Year == month.Year && invoice.IssuedOn.Month == month.Month).Sum(invoice => invoice.Amount),
                invoices.Where(invoice => invoice.RefundedAmount > 0 && invoice.IssuedOn.Year == month.Year && invoice.IssuedOn.Month == month.Month).Sum(invoice => invoice.RefundedAmount)))
            .ToList();
    }

    /// <summary>
    /// Creates a tenant from the operator console: the workspace (with a unique name and subdomain) and its owner (who has no
    /// demo sign-in). A trial runs 14 days on a self-serve plan; an active paid tenant gets its first invoice, open until the
    /// owner pays it. A lead it came from is closed with a note. The account manager, when another operator, is told.
    /// </summary>
    /// <returns>The result (<see cref="SaasResult.Duplicate" /> for a name or subdomain in use) and the new workspace's ID on success.</returns>
    public (SaasResult Result, int? WorkspaceId) CreateWorkspaceAsOperator(string name, string subdomain, string ownerName, string ownerEmail, string planId, SaasBillingPeriod period,
        int seats, string country, bool startTrial, int? leadId, string? managerId)
    {
        if (!IsOperator)
            return (SaasResult.NotAllowed, null);

        if (FindPlan(planId) is not { } plan || !Enum.IsDefined(period) || (startTrial && (plan.MonthlyPricePerSeat == 0 || !plan.IsSelfServe)))
            return (SaasResult.NotFound, null);

        subdomain = subdomain?.Trim().ToLowerInvariant() ?? string.Empty;

        if (!IsText(name?.Trim(), MaxWorkspaceNameLength) || !IsText(ownerName?.Trim(), ShowcaseIdentity.MaxNameLength) || !IsEmail(ownerEmail?.Trim())
            || !SaasSeedData.Countries.Contains(country) || !IsSubdomain(subdomain) || (managerId is not null && SaasSeedData.ManagerName(managerId) is null)
            || workspaces.Count >= MaxWorkspaces || members.Count >= MaxMembers)
            return (SaasResult.Invalid, null);

        if (IsWorkspaceNameTaken(name) || IsSubdomainTaken(subdomain))
            return (SaasResult.Duplicate, null);

        if (seats < 1 || seats > (plan.SeatLimit == 0 ? SaasSeedData.MaxSeats : plan.SeatLimit))
            return (SaasResult.SeatsOutOfRange, null);

        var today = Clock.Today;
        var paid = plan.MonthlyPricePerSeat > 0;
        var billing = paid ? period : SaasBillingPeriod.Monthly;
        DateTime? trialEndsOn = startTrial ? today.AddDays(SaasSeedData.TrialDays) : null;
        var renewsOn = trialEndsOn ?? (paid ? SaasPricing.PeriodEnd(today, billing) : today.AddMonths(1));
        var workspace = new SaasWorkspace(nextWorkspaceId++, name!.Trim(), plan.Id, billing, seats, startTrial ? SaasWorkspaceStatus.Trial : SaasWorkspaceStatus.Active,
            today, trialEndsOn, renewsOn, country, 0, 0, 0m, null, null, 0, 0, SaasSeedData.DefaultSettings(country))
        {
            Subdomain = subdomain,
            ManagerId = managerId,
        };
        workspaces.Add(workspace);
        members.Add(new SaasMember(nextMemberId++, workspace.Id, null, ownerName!.Trim(), ownerEmail!.Trim(), SaasMemberRole.Owner, SaasMemberStatus.Invited, Clock.Now, null));
        Log(workspace.Id, "Workspace created by Lumora", startTrial ? $"{plan.Name} trial, {SaasSeedData.TrialDays} days" : $"{plan.Name} plan, {seats} seats", IconName.Flag);

        if (!startTrial && paid)
        {
            AddInvoice(workspace.Id, today, renewsOn, $"{plan.Name} plan, {seats} seats, {SaasSeedData.PeriodLabel(billing)}", SaasPricing.PeriodAmount(plan, billing, seats), null, null,
                SaasInvoiceStatus.Open);
        }

        if (leadId is { } id && leads.FindIndex(lead => lead.Id == id) is var index and >= 0)
        {
            var lead = leads[index];
            leads[index] = lead with { Status = SaasLeadStatus.Closed, Note = Trim($"Won: tenant {workspace.Name} created on {today.ToString("MMM d", CultureInfo.InvariantCulture)}. {lead.Note}", MaxLeadMessageLength) };
            Record("Lead closed", LeadTarget(lead), LeadPath(lead.Id), $"Won: tenant {workspace.Name} created");
        }

        Record("Tenant created", workspace.Name, TenantPath(workspace.Id), startTrial
            ? $"{plan.Name} trial for {ownerName.Trim()}, {workspace.Subdomain}"
            : string.Create(CultureInfo.InvariantCulture, $"{plan.Name}, {seats} seats, {SaasSeedData.PeriodLabel(billing)}, owner {ownerName.Trim()}, {workspace.Subdomain}"));

        if (managerId is not null)
            NotifyManager(workspace, managerId);

        Changed?.Invoke();

        return (SaasResult.Ok, workspace.Id);
    }

    /// <summary>
    /// Deletes an archived tenant for good: its members, invoices, and activity. A tenant whose members can sign in to this
    /// demo is kept, so nobody loses the workspace they're using.
    /// </summary>
    public SaasResult DeleteWorkspaceAsOperator(int workspaceId)
    {
        if (!IsOperator)
            return SaasResult.NotAllowed;

        if (FindWorkspace(workspaceId) is not { } workspace)
            return SaasResult.NotFound;

        if (workspace.Status != SaasWorkspaceStatus.Archived || members.Any(member => member.WorkspaceId == workspaceId && member.AccountId is not null))
            return SaasResult.NotAllowed;

        workspaces.Remove(workspace);
        members.RemoveAll(member => member.WorkspaceId == workspaceId);
        invoices.RemoveAll(invoice => invoice.WorkspaceId == workspaceId);
        activities.RemoveAll(activity => activity.WorkspaceId == workspaceId);
        Record("Tenant deleted", workspace.Name, null, "Archived tenant removed with its members, invoices, and activity.");
        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>Returns a lead by its ID, or <see langword="null" />.</summary>
    public SaasLead? FindLead(int leadId) => leads.FirstOrDefault(lead => lead.Id == leadId);

    /// <summary>Pays every open invoice of the signed-in member's workspace with the card on file; a past-due workspace becomes active.</summary>
    public SaasResult PayOpenInvoices()
    {
        if (!CanManageBilling || CurrentWorkspace is not { } workspace)
            return SaasResult.NotAllowed;

        var open = invoices.Where(invoice => invoice.WorkspaceId == workspace.Id && invoice.Status == SaasInvoiceStatus.Open).ToList();

        if (open.Count == 0)
            return SaasResult.NoChange;

        if (workspace.CardLast4 is null)
            return SaasResult.CardRequired;

        foreach (var invoice in open)
            Pay(invoice, FindWorkspace(workspace.Id)!);

        var summary = $"{open.Count} open {(open.Count == 1 ? "invoice" : "invoices")}, {Money(open.Sum(invoice => invoice.Amount))}";
        Log(workspace.Id, "Invoices paid", summary, IconName.Receipt);
        Record("Invoices paid", workspace.Name, TenantPath(workspace.Id), $"{summary}: {string.Join(", ", open.Select(invoice => invoice.Number))}");
        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>
    /// Refunds part or all of a paid invoice to the card it was paid with (operator). A full refund marks it refunded; the
    /// workspace's owners who can sign in get a notice.
    /// </summary>
    public SaasResult RefundInvoice(string number, decimal amount, string reason)
    {
        if (!IsOperator)
            return SaasResult.NotAllowed;

        if (invoices.FirstOrDefault(invoice => invoice.Number == number) is not { } invoice || invoice.Status == SaasInvoiceStatus.Open || FindWorkspace(invoice.WorkspaceId) is not { } workspace)
            return SaasResult.NotFound;

        amount = SaasPricing.Cents(amount);

        if (amount <= 0 || amount > invoice.Amount - invoice.RefundedAmount || !IsText(reason?.Trim(), 200))
            return SaasResult.Invalid;

        var refunded = invoice.RefundedAmount + amount;
        var updated = invoice with { RefundedAmount = refunded, Status = refunded >= invoice.Amount ? SaasInvoiceStatus.Refunded : SaasInvoiceStatus.Paid };
        invoices[invoices.IndexOf(invoice)] = updated;
        Log(workspace.Id, "Refund issued", $"{Money(amount)} on {invoice.Number}", IconName.ArrowCounterclockwise);

        foreach (var owner in OthersToTell(workspace.Id, member => member.Role is SaasMemberRole.Owner or SaasMemberRole.Billing))
        {
            Notify(owner, $"{Money(amount)} refunded", $"Lumora refunded {Money(amount)} on invoice {invoice.Number} to the {invoice.CardBrand ?? "card"} ending in {invoice.CardLast4 ?? "----"}.",
                AppInvoicePath(invoice.Number), IconName.ArrowCounterclockwise);
        }

        Record("Refund issued", InvoiceTarget(invoice.Number), InvoicePath(invoice.Number), $"{workspace.Name}: {Money(amount)}. {reason!.Trim()}");
        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>Changes a lead's status and the sales team's note (operator). A closed lead needs a note saying why.</summary>
    public SaasResult UpdateLead(int leadId, SaasLeadStatus status, string? note)
    {
        if (!IsOperator)
            return SaasResult.NotAllowed;

        if (leads.FindIndex(lead => lead.Id == leadId) is not (var index and >= 0) || !Enum.IsDefined(status))
            return SaasResult.NotFound;

        note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();

        if (note is { Length: > MaxLeadMessageLength } || (status == SaasLeadStatus.Closed && note is null))
            return SaasResult.Invalid;

        var lead = leads[index];

        if (lead.Status == status && lead.Note == note)
            return SaasResult.NoChange;

        leads[index] = lead with { Status = status, Note = note };
        Record("Lead updated", LeadTarget(lead), LeadPath(lead.Id), lead.Status == status ? "Note changed" : $"{lead.Status} → {status}");
        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>
    /// Saves a plan from the operator's plan editor. The public pricing page, sign-up, and billing follow it right away, and
    /// workspaces on the plan pay the new price from their next invoice. A free plan stays free and a paid plan stays paid;
    /// a seat limit can't drop below the seats a workspace on the plan already has; names are unique; one plan at most is
    /// marked popular, and only a self-serve plan can be.
    /// </summary>
    public SaasResult UpdatePlan(SaasPlan edited)
    {
        if (!IsOperator)
            return SaasResult.NotAllowed;

        if (edited is null || plans.FindIndex(plan => plan.Id == edited.Id) is not (var index and >= 0))
            return SaasResult.NotFound;

        var current = plans[index];
        edited = edited with
        {
            Name = edited.Name?.Trim() ?? string.Empty,
            Description = edited.Description?.Trim() ?? string.Empty,
            CallToAction = edited.CallToAction?.Trim() ?? string.Empty,
            MonthlyPricePerSeat = SaasPricing.Cents(edited.MonthlyPricePerSeat),
            Highlights = (edited.Highlights ?? Array.Empty<string>()).Select(line => line?.Trim() ?? string.Empty).Where(line => line.Length > 0).ToList(),
        };

        if (!ValidPlan(edited) || (edited.MonthlyPricePerSeat == 0) != (current.MonthlyPricePerSeat == 0)
            || (edited.MonthlyPricePerSeat is > 0 and < MinPaidSeatPrice)
            || (edited.IsPopular && !edited.IsSelfServe)
            || (!edited.IsSelfServe && !plans.Any(plan => plan.Id != edited.Id && plan.IsSelfServe)))
            return SaasResult.Invalid;

        if (IsPlanNameTaken(edited.Name, edited.Id))
            return SaasResult.Duplicate;

        if (edited.SeatLimit > 0 && workspaces.Any(workspace => workspace.PlanId == edited.Id && workspace.Seats > edited.SeatLimit))
            return SaasResult.SeatsOutOfRange;

        if (edited == current || (edited.Highlights.SequenceEqual(current.Highlights) && edited with { Highlights = current.Highlights } == current))
            return SaasResult.NoChange;

        if (edited.IsPopular)
        {
            for (var other = 0; other < plans.Count; other++)
            {
                if (other != index && plans[other].IsPopular)
                    plans[other] = plans[other] with { IsPopular = false };
            }
        }

        plans[index] = edited;
        Record("Plan updated", PlanTarget(edited.Name), PlansPath, PlanChanges(current, edited));
        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>Returns a workspace's activity, newest first.</summary>
    public IReadOnlyList<SaasActivity> ActivityOf(int workspaceId) =>
        activities.Where(activity => activity.WorkspaceId == workspaceId).OrderByDescending(activity => activity.At).ToList();

    /// <summary>
    /// Returns the snapshot of Lumora: the plans and counters, and of the other lists only what differs from the fresh tab's
    /// seed (records changed or added, and the keys of seeded records removed). The seed is the same in every tab, so the
    /// restore rebuilds the rest from it; a fresh tab's snapshot is a few kilobytes although it has hundreds of tenants.
    /// </summary>
    public object CaptureData()
    {
        var data = seed.Value;
        var workspaceIds = workspaces.Select(workspace => workspace.Id).ToHashSet();
        var (changedWorkspaces, removedWorkspaces) = Diff(workspaces, data.Workspaces, workspace => workspace.Id);
        var (changedMembers, removedMembers) = Diff(members, data.Members, member => member.Id, member => workspaceIds.Contains(member.WorkspaceId));
        var (changedInvoices, removedInvoices) = Diff(invoices, data.Invoices, invoice => invoice.Number, invoice => workspaceIds.Contains(invoice.WorkspaceId));
        var (changedNotices, removedNotices) = Diff(notices, data.Notices, notice => notice.Id);
        var (changedLeads, removedLeads) = Diff(leads, data.Leads, lead => lead.Id);
        var (changedAudit, removedAudit) = Diff(audit, data.Audit, entry => entry.Id, equals: SameAuditEntry);

        // Activity has no key: entries not in the seed are added, seeded entries gone (and not with their workspace) are removed.
        var seededActivity = data.Activities.ToHashSet();
        var currentActivity = activities.ToHashSet();
        var addedActivity = activities.Where(activity => !seededActivity.Contains(activity)).ToList();
        var removedActivity = data.Activities.Where(activity => !currentActivity.Contains(activity) && workspaceIds.Contains(activity.WorkspaceId)).ToList();

        return new SaasSnapshot
        {
            Activities = addedActivity.Count == 0 ? null : addedActivity,
            RemovedActivities = removedActivity.Count == 0 ? null : removedActivity,
            AnnualBilling = annualBilling,
            Audit = changedAudit,
            RemovedAuditIds = removedAudit,
            Invoices = changedInvoices,
            RemovedInvoiceNumbers = removedInvoices,
            Leads = changedLeads,
            RemovedLeadIds = removedLeads,
            Members = changedMembers,
            RemovedMemberIds = removedMembers,
            NextAuditId = nextAuditId,
            NextLeadId = nextLeadId,
            NextMemberId = nextMemberId,
            NextNoticeId = nextNoticeId,
            NextWorkspaceId = nextWorkspaceId,
            Notices = changedNotices,
            RemovedNoticeIds = removedNotices,
            Plans = plans.ToList(),
            Workspaces = changedWorkspaces,
            RemovedWorkspaceIds = removedWorkspaces,
        };
    }

    /// <summary>
    /// Changes the signed-in member's workspace to another plan, billing period, or number of seats, charging the card on file
    /// for any amount due today (see <see cref="QuotePlan" />). Ending a trial on a paid plan starts a new period today.
    /// </summary>
    public SaasResult ChangePlan(string planId, SaasBillingPeriod period, int seats)
    {
        if (!CanManageBilling || CurrentWorkspace is not { } workspace)
            return SaasResult.NotAllowed;

        if (FindPlan(planId) is not { IsSelfServe: true } plan || !Enum.IsDefined(period))
            return SaasResult.NotFound;

        if (!SeatsFit(workspace, plan, seats))
            return SaasResult.SeatsOutOfRange;

        var isTrial = workspace.Status == SaasWorkspaceStatus.Trial;

        if (!isTrial && plan.Id == workspace.PlanId && period == workspace.Period && seats == workspace.Seats)
            return SaasResult.NoChange;

        var paid = plan.MonthlyPricePerSeat > 0;

        if (paid && workspace.CardLast4 is null)
            return SaasResult.CardRequired;

        var quote = Quote(workspace, plan, period, seats);
        var today = Clock.Today;

        var updated = workspace with
        {
            PlanId = plan.Id,
            Period = paid ? period : SaasBillingPeriod.Monthly,
            Seats = seats,
            Status = isTrial ? SaasWorkspaceStatus.Active : workspace.Status,
            TrialEndsOn = null,
            RenewsOn = quote.NextRenewal,
            CreditBalance = quote.CreditAfter,
        };
        Replace(updated);

        var summary = $"{plan.Name}, {seats} {(seats == 1 ? "seat" : "seats")}{(paid ? $", {SaasSeedData.PeriodLabel(updated.Period)}" : string.Empty)}";

        if (quote.ChargeToday > 0)
        {
            AddInvoice(workspace.Id, today, quote.NextRenewal, isTrial ? $"{summary} (trial ended)" : $"Change to {summary}, prorated", quote.ChargeToday, workspace.CardBrand, workspace.CardLast4);
        }

        Log(workspace.Id, isTrial ? "Trial converted" : "Plan changed", summary, IconName.ArrowUpCircle);
        Record(isTrial ? "Trial converted" : "Plan changed", workspace.Name, TenantPath(workspace.Id),
            Changes(("Plan", PlanOf(workspace).Name, plan.Name), ("Seats", Number(workspace.Seats), Number(seats)), ("Billing", workspace.Period.ToString(), updated.Period.ToString()))
            + (quote.ChargeToday > 0 ? $"; {Money(quote.ChargeToday)} charged" : string.Empty));

        foreach (var billingMember in OthersToTell(workspace.Id, member => member.Role is SaasMemberRole.Owner or SaasMemberRole.Billing))
            Notify(billingMember, "The plan changed", $"{Identity.Current!.Name} moved {workspace.Name} to {summary}.", BillingPath, IconName.ArrowUpCircle);

        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>Changes a member's role. Only an owner can grant or take away the owner role, and nobody changes their own role.</summary>
    public SaasResult ChangeMemberRole(int memberId, SaasMemberRole role)
    {
        if (!CanManageTeam || CurrentMember is not { } me || !Enum.IsDefined(role))
            return SaasResult.NotAllowed;

        if (members.FirstOrDefault(member => member.Id == memberId && member.WorkspaceId == me.WorkspaceId) is not { } target)
            return SaasResult.NotFound;

        if (target.Id == me.Id || ((target.Role == SaasMemberRole.Owner || role == SaasMemberRole.Owner) && me.Role != SaasMemberRole.Owner))
            return SaasResult.NotAllowed;

        if (target.Role == role)
            return SaasResult.NoChange;

        Replace(target with { Role = role });
        Log(me.WorkspaceId, "Role changed", $"{target.Name} is now {RoleLabel(role)}", IconName.PersonGear);
        Record("Role changed", MemberTarget(target), TenantPath(me.WorkspaceId), $"{target.Role} → {role}");
        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>Returns a plan by its ID, or <see langword="null" />.</summary>
    public SaasPlan? FindPlan(string? planId) => plans.FirstOrDefault(plan => string.Equals(plan.Id, planId, StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns an invoice of the signed-in member's workspace (or any invoice for the operator), or <see langword="null" />.</summary>
    public SaasInvoice? FindInvoice(string? number) =>
        invoices.FirstOrDefault(invoice => invoice.Number == number && (IsOperator || invoice.WorkspaceId == CurrentMember?.WorkspaceId));

    /// <summary>Returns a workspace by its ID, or <see langword="null" />.</summary>
    public SaasWorkspace? FindWorkspace(int workspaceId) => workspaces.FirstOrDefault(workspace => workspace.Id == workspaceId);

    /// <summary>
    /// Invites someone to the signed-in member's workspace; the invitation holds a seat. When it takes the last free seat,
    /// Lumora's operators are told (a chance to sell more seats).
    /// </summary>
    public SaasResult InviteMember(string? name, string email, SaasMemberRole role)
    {
        if (!CanManageTeam || CurrentMember is not { } me || CurrentWorkspace is not { } workspace || !Enum.IsDefined(role))
            return SaasResult.NotAllowed;

        if (role == SaasMemberRole.Owner && me.Role != SaasMemberRole.Owner)
            return SaasResult.NotAllowed;

        email = email?.Trim() ?? string.Empty;
        name = string.IsNullOrWhiteSpace(name) ? NameFromEmail(email) : name.Trim();

        if (!IsEmail(email) || !IsText(name, ShowcaseIdentity.MaxNameLength))
            return SaasResult.Invalid;

        var team = MembersOf(workspace.Id);

        if (team.Any(member => string.Equals(member.Email, email, StringComparison.OrdinalIgnoreCase)))
            return SaasResult.AlreadyMember;

        if (team.Count >= workspace.Seats)
            return SaasResult.NoFreeSeat;

        if (members.Count >= MaxMembers)
            return SaasResult.Invalid;

        var invited = new SaasMember(nextMemberId++, workspace.Id, null, name, email, role, SaasMemberStatus.Invited, Clock.Now, null);
        members.Add(invited);
        Log(workspace.Id, "Teammate invited", $"{name} as {RoleLabel(role)}", IconName.PersonPlus);
        Record("Teammate invited", MemberTarget(invited), TenantPath(workspace.Id), $"As {RoleLabel(role)}");

        if (team.Count + 1 == workspace.Seats)
        {
            NotifyOperators($"{workspace.Name} uses all its seats", string.Create(CultureInfo.InvariantCulture,
                $"{workspace.Seats} of {workspace.Seats} seats on {PlanOf(workspace).Name} are taken by members and invitations."), TenantPath(workspace.Id), IconName.People);
        }

        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>Marks every notification of the signed-in account as read.</summary>
    public void MarkNoticesRead()
    {
        if (Identity.Current is not { } account)
            return;

        var changed = false;

        for (var index = 0; index < notices.Count; index++)
        {
            if (notices[index].AccountId == account.Id && !notices[index].IsRead)
            {
                notices[index] = notices[index] with { IsRead = true };
                changed = true;
            }
        }

        if (changed)
            Changed?.Invoke();
    }

    /// <summary>Returns the members of a workspace: owners first, then active members by name, then invitations.</summary>
    public IReadOnlyList<SaasMember> MembersOf(int workspaceId) =>
        members.Where(member => member.WorkspaceId == workspaceId)
            .OrderBy(member => member.Status)
            .ThenBy(member => member.Role)
            .ThenBy(member => member.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>Returns what a workspace pays per month on average: 0 for a trial, a free plan, or a suspended or archived workspace.</summary>
    public decimal MonthlyRevenueFor(SaasWorkspace workspace) =>
        workspace.Status is SaasWorkspaceStatus.Active or SaasWorkspaceStatus.PastDue && FindPlan(workspace.PlanId) is { } plan
            ? SaasPricing.MonthlyAmount(plan, workspace.Period, workspace.Seats)
            : 0m;

    /// <summary>Returns an account's notifications, newest first.</summary>
    public IReadOnlyList<SaasNotice> NoticesFor(string? accountId) =>
        notices.Where(notice => notice.AccountId == accountId).OrderByDescending(notice => notice.CreatedOn).ThenByDescending(notice => notice.Id).ToList();

    /// <summary>Raises <see cref="Changed" /> after a snapshot was restored.</summary>
    public void NotifyRestored()
    {
        CatchUpBilling();
        Changed?.Invoke();
    }

    /// <summary>Returns the owner shown for a workspace (its first owner), or <see langword="null" />.</summary>
    public SaasMember? OwnerOf(int workspaceId) => MembersOf(workspaceId).FirstOrDefault(member => member.Role == SaasMemberRole.Owner);

    /// <summary>Returns the plan of a workspace.</summary>
    public SaasPlan PlanOf(SaasWorkspace workspace) => FindPlan(workspace.PlanId) ?? plans[0];

    /// <summary>
    /// Returns what changing the signed-in member's workspace would cost today and at renewal, without changing anything;
    /// <see langword="null" /> when the change isn't allowed. A change within the same period is prorated to the day; a new
    /// billing period (or leaving a trial or a free plan) starts today, with the unused part of the old period credited.
    /// </summary>
    public SaasPlanQuote? QuotePlan(string planId, SaasBillingPeriod period, int seats) =>
        CurrentWorkspace is { } workspace && FindPlan(planId) is { IsSelfServe: true } plan && Enum.IsDefined(period) && SeatsFit(workspace, plan, seats)
            ? Quote(workspace, plan, period, seats)
            : null;

    /// <summary>Removes a member or cancels an invitation. Nobody removes themselves, and only an owner removes an owner.</summary>
    public SaasResult RemoveMember(int memberId)
    {
        if (!CanManageTeam || CurrentMember is not { } me)
            return SaasResult.NotAllowed;

        if (members.FirstOrDefault(member => member.Id == memberId && member.WorkspaceId == me.WorkspaceId) is not { } target)
            return SaasResult.NotFound;

        if (target.Id == me.Id || (target.Role == SaasMemberRole.Owner && me.Role != SaasMemberRole.Owner))
            return SaasResult.NotAllowed;

        var invitation = target.Status == SaasMemberStatus.Invited;
        members.Remove(target);
        Log(me.WorkspaceId, invitation ? "Invitation cancelled" : "Teammate removed", target.Name, IconName.PersonDash);
        Record(invitation ? "Invitation cancelled" : "Teammate removed", MemberTarget(target), TenantPath(me.WorkspaceId), target.Email);
        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>Sends an invitation again (in this demo nothing is sent; the date is updated).</summary>
    public SaasResult ResendInvite(int memberId)
    {
        if (!CanManageTeam || CurrentMember is not { } me)
            return SaasResult.NotAllowed;

        if (members.FirstOrDefault(member => member.Id == memberId && member.WorkspaceId == me.WorkspaceId && member.Status == SaasMemberStatus.Invited) is not { } target)
            return SaasResult.NotFound;

        Replace(target with { AddedOn = Clock.Now });
        Log(me.WorkspaceId, "Invitation sent again", target.Email, IconName.Envelope);
        Record("Invitation sent again", MemberTarget(target), TenantPath(me.WorkspaceId), target.Email);
        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>Returns the number of seats a workspace uses: members and open invitations.</summary>
    public int SeatsUsed(int workspaceId) => members.Count(member => member.WorkspaceId == workspaceId);

    /// <summary>Applies the theme chosen in the site bar, for the charts.</summary>
    public void SetTheme(string? themeName)
    {
        var isDark = string.Equals(themeName, "dark", StringComparison.OrdinalIgnoreCase);

        if (isDarkTheme == isDark)
            return;

        isDarkTheme = isDark;
        ThemeChanged?.Invoke();
    }

    /// <summary>Returns Lumora's own site traffic for the operator console.</summary>
    public SaasAnalyticsSnapshot SiteAnalytics(SaasAnalyticsRange range) => SaasAnalyticsSeedData.Analytics(range, 1, SaasAnalyticsSeedData.LumoraPages);

    /// <summary>
    /// Starts a workspace from the public sign-up: creates the customer account (signed in), the workspace, and its owner.
    /// Growth starts a 14-day trial without a card; Starter is free from the start. Enterprise goes through sales. The
    /// company becomes the workspace's name, which must be free on Lumora (<see cref="ShowcaseAccountResult.NotAllowed" />
    /// otherwise); its subdomain is made from the name. Lumora's operators are told about the new sign-up.
    /// </summary>
    public ShowcaseAccountResult StartTrial(string name, string email, string password, string company, string country, string planId, SaasBillingPeriod period)
    {
        // The same checks the snapshot restore applies, so a sign-up never produces data a reload would discard.
        if (FindPlan(planId) is not { IsSelfServe: true } plan || !Enum.IsDefined(period) || !IsText(company?.Trim(), MaxWorkspaceNameLength)
            || !IsText(name.Trim(), ShowcaseIdentity.MaxNameLength) || !IsEmail(email.Trim()) || IsWorkspaceNameTaken(company)
            || !SaasSeedData.Countries.Contains(country) || workspaces.Count >= MaxWorkspaces || members.Count >= MaxMembers)
            return ShowcaseAccountResult.NotAllowed;

        var result = Identity.Register(SaasSeedData.CustomerRole, name, email, password, out var account);

        if (result != ShowcaseAccountResult.Success || account is null)
            return result;

        var today = Clock.Today;
        var trial = plan.MonthlyPricePerSeat > 0;
        var trialEndsOn = today.AddDays(SaasSeedData.TrialDays);
        var workspace = new SaasWorkspace(nextWorkspaceId++, company!.Trim(), plan.Id, trial ? period : SaasBillingPeriod.Monthly, trial ? 5 : Math.Max(1, plan.SeatLimit),
            trial ? SaasWorkspaceStatus.Trial : SaasWorkspaceStatus.Active, today, trial ? trialEndsOn : null, trial ? trialEndsOn : today.AddMonths(1), country,
            0, 0, 0m, null, null, 0, 0, SaasSeedData.DefaultSettings(country))
        {
            Subdomain = FreeSubdomain(company),
        };
        workspaces.Add(workspace);
        members.Add(new SaasMember(nextMemberId++, workspace.Id, account.Id, account.Name, account.Email, SaasMemberRole.Owner, SaasMemberStatus.Active, Clock.Now, Clock.Now));
        Log(workspace.Id, "Workspace created", trial ? $"{plan.Name} trial, {SaasSeedData.TrialDays} days" : $"{plan.Name} plan", IconName.Flag);
        Record(trial ? "Trial started" : "Workspace created", workspace.Name, TenantPath(workspace.Id),
            $"{plan.Name}{(trial ? $" trial until {trialEndsOn.ToString("MMM d", CultureInfo.InvariantCulture)}" : " plan")}, {country}, {workspace.Subdomain}; signed up by {account.Name}");
        AddNotice(account.Id, $"Welcome to {SaasSeedData.ProductName}",
            trial ? $"Your {plan.Name} trial runs until {trialEndsOn.ToString("MMM d", CultureInfo.InvariantCulture)}. Invite your team and send your first events." : "Invite your team and send your first events.",
            AppPath, IconName.Stars);
        NotifyOperators($"New sign-up: {workspace.Name}", $"{account.Name} started {(trial ? $"a {plan.Name} trial" : $"on {plan.Name}")} from {country}.", TenantPath(workspace.Id), IconName.PersonPlus);
        Changed?.Invoke();

        return ShowcaseAccountResult.Success;
    }

    /// <summary>
    /// Records a message from the public "Contact sales" form, and tells Lumora's operators; returns <see langword="null" />
    /// when a field is out of range.
    /// </summary>
    public SaasLead? SubmitLead(string name, string email, string company, string teamSize, string message)
    {
        if (!IsText(name?.Trim(), MaxLeadTextLength) || !IsEmail(email?.Trim()) || !IsText(company?.Trim(), MaxLeadTextLength) || !SaasSeedData.TeamSizes.Contains(teamSize)
            || !IsText(message?.Trim(), MaxLeadMessageLength) || leads.Count >= MaxLeads)
            return null;

        var lead = new SaasLead(nextLeadId++, name!.Trim(), email!.Trim(), company!.Trim(), teamSize, message!.Trim(), Clock.Now, SaasLeadStatus.New);
        leads.Add(lead);
        Record(Clock.Now, Identity.Current?.Name ?? lead.Name, "Lead received", LeadTarget(lead), LeadPath(lead.Id), $"From the contact form: {lead.Name}, {lead.TeamSize} people");
        NotifyOperators($"New lead from {lead.Company}", $"{lead.Name}, {lead.TeamSize} people: {FirstLine(lead.Message)}", LeadPath(lead.Id), IconName.Inbox);
        Changed?.Invoke();

        return lead;
    }

    /// <summary>Returns the whole days left in a trial (0 on the last day), or <see langword="null" /> for a workspace that isn't on trial.</summary>
    public int? TrialDaysLeft(SaasWorkspace workspace) =>
        workspace is { Status: SaasWorkspaceStatus.Trial, TrialEndsOn: { } endsOn } ? Math.Max(0, (endsOn.Date - Clock.Today).Days) : null;

    /// <summary>Returns the number of unread notifications of an account.</summary>
    public int UnreadNoticeCount(string? accountId) => notices.Count(notice => notice.AccountId == accountId && !notice.IsRead);

    /// <summary>Saves a card the page charged as a zero-amount check (only approved test cards reach here); it pays the next invoices.</summary>
    public SaasResult UpdatePaymentMethod(ShowcaseCardCharge check, int expiryMonth, int expiryYear)
    {
        if (!CanManageBilling || CurrentWorkspace is not { } workspace)
            return SaasResult.NotAllowed;

        if (!check.IsApproved)
            return SaasResult.CardDeclined;

        if (!cardBrands.Contains(check.Brand) || check.Last4 is not { Length: 4 } || !check.Last4.All(char.IsAsciiDigit) || expiryMonth is < 1 or > 12 || expiryYear is < 2000 or > 2100)
            return SaasResult.Invalid;

        Replace(workspace with { CardBrand = check.Brand, CardLast4 = check.Last4, CardExpiryMonth = expiryMonth, CardExpiryYear = expiryYear });
        Log(workspace.Id, "Payment method updated", $"{check.Brand} ending in {check.Last4}", IconName.CreditCard);
        Record("Payment method updated", workspace.Name, TenantPath(workspace.Id), $"{check.Brand} ending in {check.Last4}");
        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>Saves the signed-in member's workspace name (unique on Lumora) and settings (owners and admins).</summary>
    public SaasResult UpdateWorkspace(string name, SaasWorkspaceSettings settings)
    {
        if (!CanManageSettings || CurrentWorkspace is not { } workspace)
            return SaasResult.NotAllowed;

        if (!IsText(name?.Trim(), MaxWorkspaceNameLength) || !ValidSettings(settings))
            return SaasResult.Invalid;

        if (IsWorkspaceNameTaken(name, workspace.Id))
            return SaasResult.Duplicate;

        var updated = workspace with { Name = name!.Trim(), Settings = settings };

        if (updated == workspace)
            return SaasResult.NoChange;

        Replace(updated);
        Log(workspace.Id, "Settings saved", updated.Name, IconName.Gear);
        Record("Settings saved", updated.Name, TenantPath(workspace.Id), SettingsChanges(workspace, updated));
        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>
    /// Changes a workspace's plan, seats, status, or account manager from the operator console. Seats can't drop below the
    /// members and invitations; a suspension (or a return to active) notifies the workspace's owners who can sign in, a plan
    /// change its owners and billing members, and a new account manager is told when they are another operator.
    /// </summary>
    public SaasResult UpdateWorkspaceAsOperator(int workspaceId, string planId, int seats, SaasWorkspaceStatus status, string? managerId)
    {
        if (!IsOperator)
            return SaasResult.NotAllowed;

        if (FindWorkspace(workspaceId) is not { } workspace || FindPlan(planId) is not { } plan || !Enum.IsDefined(status)
            || (managerId is not null && SaasSeedData.ManagerName(managerId) is null))
            return SaasResult.NotFound;

        if (!SeatsFit(workspace, plan, seats))
            return SaasResult.SeatsOutOfRange;

        var updated = OperatorChange(workspace, plan, seats, status, managerId);

        if (updated == workspace)
            return SaasResult.NoChange;

        Replace(updated);
        Log(workspace.Id, "Changed by Lumora", string.Create(CultureInfo.InvariantCulture, $"{plan.Name}, {seats} seats, {status}"), IconName.ShieldCheck);
        Record(status != workspace.Status ? StatusAction(status) : "Tenant updated", workspace.Name, TenantPath(workspace.Id),
            Changes(("Plan", PlanOf(workspace).Name, plan.Name), ("Seats", Number(workspace.Seats), Number(seats)), ("Status", workspace.Status.ToString(), status.ToString()),
                ("Account manager", SaasSeedData.ManagerName(workspace.ManagerId) ?? "none", SaasSeedData.ManagerName(managerId) ?? "none")));
        TellAboutOperatorChange(workspace, updated);
        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>Validates a stored snapshot (applied over the seed) and applies it only when every part is valid.</summary>
    public bool TryRestoreData(JsonElement data)
    {
        SaasSnapshot? snapshot;

        try
        {
            snapshot = data.Deserialize<SaasSnapshot>(snapshotOptions);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException)
        {
            return false;
        }

        if (snapshot is not { Plans: { Count: >= 1 and <= 10 } storedPlans }
            || snapshot.NextWorkspaceId is < 2 or > 100_000
            || snapshot.NextMemberId is < 2 or > 1_000_000
            || snapshot.NextNoticeId is < 1 or > 1_000_000
            || snapshot.NextLeadId is < 1 or > 100_000
            || snapshot.NextAuditId is < 1 or > 1_000_000)
            return false;

        var fresh = seed.Value;

        // Rebuild every list from the seed and the stored changes; an inconsistent change list discards the snapshot.
        if (Overlay(fresh.Workspaces, snapshot.Workspaces, snapshot.RemovedWorkspaceIds, workspace => workspace.Id) is not { Count: >= 1 and <= MaxWorkspaces } storedWorkspaces)
            return false;

        var remaining = storedWorkspaces.Select(workspace => workspace?.Id ?? 0).ToHashSet();

        if (Overlay(fresh.Members, snapshot.Members, snapshot.RemovedMemberIds, member => member.Id, member => remaining.Contains(member.WorkspaceId)) is not { Count: <= MaxMembers } storedMembers
            || Overlay(fresh.Invoices, snapshot.Invoices, snapshot.RemovedInvoiceNumbers, invoice => invoice.Number, invoice => remaining.Contains(invoice.WorkspaceId)) is not { Count: <= MaxInvoices } storedInvoices
            || Overlay(fresh.Notices, snapshot.Notices, snapshot.RemovedNoticeIds, notice => notice.Id) is not { Count: <= MaxNotices } storedNotices
            || Overlay(fresh.Leads, snapshot.Leads, snapshot.RemovedLeadIds, lead => lead.Id) is not { Count: <= MaxLeads } storedLeads
            || Overlay(fresh.Audit, snapshot.Audit, snapshot.RemovedAuditIds, entry => entry.Id) is not { Count: <= MaxAudit } storedAudit
            || OverlayActivities(fresh.Activities, snapshot.Activities, snapshot.RemovedActivities, remaining) is not { Count: <= MaxActivities } storedActivities)
            return false;

        var seededPlanIds = SaasSeedData.Plans().Select(plan => plan.Id).ToList();

        if (!storedPlans.Select(plan => plan?.Id).SequenceEqual(seededPlanIds) || storedPlans.Any(plan => !ValidPlan(plan))
            || storedPlans.Count(plan => plan.IsPopular) > 1 || !storedPlans.Any(plan => plan.IsSelfServe)
            || !Unique(storedPlans.Select(plan => plan.Name.ToUpperInvariant()))
            || storedPlans.Zip(SaasSeedData.Plans()).Any(pair => (pair.First.MonthlyPricePerSeat == 0) != (pair.Second.MonthlyPricePerSeat == 0)))
            return false;

        var planById = storedPlans.ToDictionary(plan => plan.Id, StringComparer.OrdinalIgnoreCase);

        if (storedWorkspaces.Any(workspace => !ValidWorkspace(workspace, planById, snapshot.NextWorkspaceId))
            || !Unique(storedWorkspaces.Select(workspace => workspace.Id))
            || !Unique(storedWorkspaces.Select(workspace => workspace.Name.Trim().ToUpperInvariant()))
            || !Unique(storedWorkspaces.Select(workspace => workspace.Subdomain)))
            return false;

        var workspaceIds = storedWorkspaces.Select(workspace => workspace.Id).ToHashSet();

        if (storedMembers.Any(member => member is null || member.Id < 1 || member.Id >= snapshot.NextMemberId || !workspaceIds.Contains(member.WorkspaceId)
                || (member.AccountId is not null && !IsAccountId(member.AccountId)) || !IsText(member.Name, ShowcaseIdentity.MaxNameLength) || !IsEmail(member.Email)
                || !Enum.IsDefined(member.Role) || !Enum.IsDefined(member.Status) || !IsPlausibleDate(member.AddedOn)
                || (member.LastActiveOn is { } lastActive && !IsPlausibleDate(lastActive)))
            || !Unique(storedMembers.Select(member => member.Id)))
            return false;

        var teams = storedMembers.GroupBy(member => member.WorkspaceId).ToDictionary(group => group.Key, group => group.ToList());

        if (storedWorkspaces.Any(workspace => !teams.TryGetValue(workspace.Id, out var team) || team.Count > workspace.Seats || !team.Any(member => member.Role == SaasMemberRole.Owner)))
            return false;

        if (storedInvoices.Any(invoice => invoice is null || !IsText(invoice.Number, 20) || !workspaceIds.Contains(invoice.WorkspaceId) || !IsPlausibleDate(invoice.IssuedOn)
                || !IsPlausibleDate(invoice.PeriodStart) || !IsPlausibleDate(invoice.PeriodEnd) || invoice.PeriodEnd < invoice.PeriodStart || !IsText(invoice.Description, 120)
                || !IsMoney(invoice.Amount) || !IsMoney(invoice.RefundedAmount) || invoice.RefundedAmount > invoice.Amount || !Enum.IsDefined(invoice.Status)
                || (invoice.CardBrand is not null && !cardBrands.Contains(invoice.CardBrand)) || (invoice.CardLast4 is not null && !IsLast4(invoice.CardLast4)))
            || !Unique(storedInvoices.Select(invoice => invoice.Number)))
            return false;

        if (storedNotices.Any(notice => notice is null || notice.Id < 1 || notice.Id >= snapshot.NextNoticeId || !IsAccountId(notice.AccountId) || !IsText(notice.Title, MaxNoticeTitleLength)
                || !IsText(notice.Text, MaxNoticeTextLength) || !IsSiteLink(notice.Link) || !Enum.IsDefined(notice.IconName) || !IsPlausibleDate(notice.CreatedOn))
            || !Unique(storedNotices.Select(notice => notice.Id)))
            return false;

        if (storedLeads.Any(lead => lead is null || lead.Id < 1 || lead.Id >= snapshot.NextLeadId || !IsText(lead.Name, MaxLeadTextLength) || !IsEmail(lead.Email) || !IsText(lead.Company, MaxLeadTextLength)
                || !SaasSeedData.TeamSizes.Contains(lead.TeamSize) || !IsText(lead.Message, MaxLeadMessageLength) || !IsPlausibleDate(lead.CreatedOn) || !Enum.IsDefined(lead.Status)
                || (lead.Note is not null && !IsText(lead.Note, MaxLeadMessageLength)))
            || !Unique(storedLeads.Select(lead => lead.Id)))
            return false;

        if (storedActivities.Any(activity => activity is null || !workspaceIds.Contains(activity.WorkspaceId) || !IsPlausibleDate(activity.At) || !IsText(activity.Title, 80)
                || !IsText(activity.Detail, 200) || !Enum.IsDefined(activity.IconName)))
            return false;

        if (storedAudit.Any(entry => entry is null || entry.Id < 1 || entry.Id >= snapshot.NextAuditId || !IsPlausibleDate(entry.At)
                || !IsText(entry.Actor, ShowcaseIdentity.MaxNameLength) || !IsText(entry.Action, MaxAuditTextLength) || !IsText(entry.Target, MaxAuditTextLength)
                || (entry.Details is not null && !IsText(entry.Details, MaxAuditTextLength)) || !IsSiteLink(entry.TargetUrl))
            || !Unique(storedAudit.Select(entry => entry.Id)))
            return false;

        Reload(plans, storedPlans);
        Reload(audit, storedAudit);
        Reload(workspaces, storedWorkspaces);
        Reload(members, storedMembers);
        Reload(invoices, storedInvoices);
        Reload(notices, storedNotices);
        Reload(leads, storedLeads);
        Reload(activities, storedActivities);
        nextAuditId = snapshot.NextAuditId;
        nextWorkspaceId = snapshot.NextWorkspaceId;
        nextMemberId = snapshot.NextMemberId;
        nextNoticeId = snapshot.NextNoticeId;
        nextLeadId = snapshot.NextLeadId;
        annualBilling = snapshot.AnnualBilling;

        return true;
    }

    /// <summary>
    /// Returns whether an operator's status change notifies the workspace's owners: a suspension, or a return to active from
    /// suspended or archived.
    /// </summary>
    public static bool NotifiesOwner(SaasWorkspaceStatus before, SaasWorkspaceStatus after) =>
        (after == SaasWorkspaceStatus.Suspended && before != SaasWorkspaceStatus.Suspended)
        || (after == SaasWorkspaceStatus.Active && before is SaasWorkspaceStatus.Suspended or SaasWorkspaceStatus.Archived);

    /// <summary>Returns a workspace's invoices, newest first.</summary>
    public IReadOnlyList<SaasInvoice> InvoicesOf(int workspaceId) =>
        invoices.Where(invoice => invoice.WorkspaceId == workspaceId).OrderByDescending(invoice => invoice.IssuedOn).ThenByDescending(invoice => invoice.Number).ToList();

    /// <summary>Returns a workspace's product traffic: the volume follows the events it tracks this month.</summary>
    public SaasAnalyticsSnapshot WorkspaceAnalytics(SaasWorkspace workspace, SaasAnalyticsRange range) =>
        SaasAnalyticsSeedData.Analytics(range, Math.Clamp(workspace.EventsThisMonth / 5_000_000d, .02, 4), SaasAnalyticsSeedData.ProductPages);

    #endregion

    #region Private Methods

    private static bool IsAccountId(string? accountId) =>
        accountId is not null && (SaasSeedData.DemoAccounts().Any(demo => demo.Id == accountId) || (accountId.StartsWith("u-", StringComparison.Ordinal) && accountId.Length <= 12));

    private static bool IsEmail(string? value) =>
        IsText(value, 120) && value!.IndexOf('@') is > 0 and var at && at < value.Length - 1 && !value.Any(char.IsWhiteSpace);

    private static bool IsLast4(string value) => value.Length == 4 && value.All(char.IsAsciiDigit);

    private static bool IsMoney(decimal value) => value is >= 0 and <= 100_000_000;

    private static bool IsPlausibleDate(DateTime value) => value >= SaasSeedData.Today.AddYears(-5) && value <= SaasSeedData.Today.AddYears(5);

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

    private static string NameFromEmail(string email)
    {
        var local = email.Split('@')[0].Replace('.', ' ').Replace('_', ' ').Replace('-', ' ').Trim();

        return local.Length == 0 ? email : string.Join(' ', local.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
    }

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Rebuilds a list from the seed and a snapshot's changes: seeded records in their order (replaced by a stored record
    /// with the same key, left out when removed or when <paramref name="keepSeeded" /> says their parent is gone), then the
    /// added records in their stored order. Returns <see langword="null" /> for an inconsistent change list (a repeated key,
    /// or a removed key that isn't seeded), which discards the snapshot.
    /// </summary>
    private static List<T>? Overlay<T, TKey>(IReadOnlyList<T> seeded, List<T>? changed, List<TKey>? removed, Func<T, TKey> key, Func<T, bool>? keepSeeded = null)
        where TKey : notnull
    {
        changed ??= new List<T>();
        removed ??= new List<TKey>();

        if (changed.Count > 10_000 || removed.Count > 10_000 || changed.Any(item => item is null) || removed.Any(item => item is null))
            return null;

        var replacements = new Dictionary<TKey, T>();

        foreach (var item in changed)
        {
            if (!replacements.TryAdd(key(item), item))
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
            else if (!removedKeys.Contains(itemKey) && (keepSeeded?.Invoke(item) ?? true))
                result.Add(item);
        }

        result.AddRange(changed.Where(item => !seededKeys.Contains(key(item))));

        return result;
    }

    /// <summary>Rebuilds the activity from the seed: seeded entries of remaining workspaces less the removed ones, then the added ones; <see langword="null" /> when a removed entry isn't seeded.</summary>
    private static List<SaasActivity>? OverlayActivities(IReadOnlyList<SaasActivity> seeded, List<SaasActivity>? added, List<SaasActivity>? removed, HashSet<int> workspaceIds)
    {
        added ??= new List<SaasActivity>();
        removed ??= new List<SaasActivity>();

        if (added.Count > MaxActivities || removed.Count > MaxActivities || added.Any(item => item is null) || removed.Any(item => item is null))
            return null;

        var seededSet = seeded.ToHashSet();
        var removedSet = removed.ToHashSet();

        if (removedSet.Count != removed.Count || removedSet.Any(item => !seededSet.Contains(item)))
            return null;

        return seeded.Where(item => workspaceIds.Contains(item.WorkspaceId) && !removedSet.Contains(item)).Concat(added).ToList();
    }

    /// <summary>The records that differ from the seed (changed or added, in list order) and the keys of seeded records removed; <see langword="null" /> for an empty list.</summary>
    private static (List<T>? Changed, List<TKey>? Removed) Diff<T, TKey>(IReadOnlyList<T> current, IReadOnlyList<T> seeded, Func<T, TKey> key, Func<T, bool>? countRemoved = null,
        Func<T, T, bool>? equals = null)
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

        var removed = seeded.Where(item => !currentKeys.Contains(key(item)) && (countRemoved?.Invoke(item) ?? true)).Select(key).ToList();

        return (changed.Count == 0 ? null : changed, removed.Count == 0 ? null : removed);
    }

    private static void Reload<T>(List<T> target, List<T> source)
    {
        target.Clear();
        target.AddRange(source);
    }

    /// <summary>Returns "an owner", "an admin", "billing", or "a member".</summary>
    private static string RoleLabel(SaasMemberRole role) => role switch
    {
        SaasMemberRole.Owner => "an owner",
        SaasMemberRole.Admin => "an admin",
        SaasMemberRole.Billing => "billing",
        _ => "a member",
    };

    private static bool SameAuditEntry(ShowcaseAuditEntry first, ShowcaseAuditEntry second) =>
        ReferenceEquals(first, second) || (first.Id == second.Id && first.At == second.At && first.Actor == second.Actor && first.Action == second.Action
            && first.Target == second.Target && first.TargetUrl == second.TargetUrl && first.Details == second.Details);

    private static bool Unique<T>(IEnumerable<T> values)
    {
        var seen = new HashSet<T>();

        return values.All(seen.Add);
    }

    private static bool ValidSettings(SaasWorkspaceSettings? settings) =>
        settings is not null && SaasSeedData.TimeZones.Contains(settings.TimeZone) && settings.WeekStartsOn is "Monday" or "Sunday"
        && SaasSeedData.DataRegions.Contains(settings.DataRegion);

    private static string Money(decimal amount) => amount.ToString("$#,0.00", CultureInfo.InvariantCulture);

    private static string Changes(params (string Name, string Before, string After)[] fields)
    {
        var changed = fields.Where(field => field.Before != field.After).Select(field => $"{field.Name} {field.Before} → {field.After}").ToList();

        return changed.Count == 0 ? "No change" : string.Join("; ", changed);
    }

    private static string PlanChanges(SaasPlan before, SaasPlan after)
    {
        string Limit(long value) => value == 0 ? "unlimited" : value.ToString("#,0", CultureInfo.InvariantCulture);

        var text = Changes(
            ("Name", before.Name, after.Name),
            ("Price", Money(before.MonthlyPricePerSeat), Money(after.MonthlyPricePerSeat)),
            ("Seat limit", Limit(before.SeatLimit), Limit(after.SeatLimit)),
            ("Event limit", Limit(before.EventLimit), Limit(after.EventLimit)),
            ("Dashboard limit", Limit(before.DashboardLimit), Limit(after.DashboardLimit)),
            ("Self-serve", before.IsSelfServe ? "yes" : "no", after.IsSelfServe ? "yes" : "no"),
            ("Popular", before.IsPopular ? "yes" : "no", after.IsPopular ? "yes" : "no"));

        if (before.Description != after.Description || before.CallToAction != after.CallToAction || !before.Highlights.SequenceEqual(after.Highlights))
            text = text == "No change" ? "Copy changed" : $"{text}; copy changed";

        return Trim(text, MaxAuditTextLength);
    }

    private static string SettingsChanges(SaasWorkspace before, SaasWorkspace after)
    {
        var (old, now) = (before.Settings, after.Settings);
        static string OnOff(bool value) => value ? "on" : "off";

        return Changes(
            ("Name", before.Name, after.Name),
            ("Time zone", old.TimeZone, now.TimeZone),
            ("Week starts", old.WeekStartsOn, now.WeekStartsOn),
            ("Data region", old.DataRegion, now.DataRegion),
            ("Weekly digest", OnOff(old.WeeklyDigest), OnOff(now.WeeklyDigest)),
            ("Security alerts", OnOff(old.SecurityAlerts), OnOff(now.SecurityAlerts)),
            ("Billing emails", OnOff(old.BillingEmails), OnOff(now.BillingEmails)),
            ("Product updates", OnOff(old.ProductUpdates), OnOff(now.ProductUpdates)));
    }

    private static string StatusAction(SaasWorkspaceStatus status) => status switch
    {
        SaasWorkspaceStatus.Suspended => "Tenant suspended",
        SaasWorkspaceStatus.Archived => "Tenant archived",
        SaasWorkspaceStatus.Active => "Tenant activated",
        SaasWorkspaceStatus.PastDue => "Tenant marked past due",
        _ => "Tenant put on trial",
    };

    private static string Trim(string value, int maxLength) => value.Length <= maxLength ? value.Trim() : value[..maxLength].Trim();

    private static bool ValidPlan(SaasPlan? plan) =>
        plan is not null && IsText(plan.Id, 20) && IsText(plan.Name, 40) && plan.Name.Trim().Length >= 2 && IsText(plan.Description, 200) && IsMoney(plan.MonthlyPricePerSeat)
        && plan.MonthlyPricePerSeat <= 10_000 && plan.MonthlyPricePerSeat == SaasPricing.Cents(plan.MonthlyPricePerSeat)
        && plan.Highlights is { Count: >= 1 and <= 8 } && plan.Highlights.All(line => IsText(line, 80)) && IsText(plan.CallToAction, 40)
        && plan.SeatLimit is >= 0 and <= SaasSeedData.MaxSeats && plan.EventLimit is 0 or (>= 1000 and <= 10_000_000_000) && plan.DashboardLimit is >= 0 and <= 10_000;

    private void Pay(SaasInvoice invoice, SaasWorkspace workspace)
    {
        invoices[invoices.IndexOf(invoice)] = invoice with { Status = SaasInvoiceStatus.Paid, CardBrand = workspace.CardBrand, CardLast4 = workspace.CardLast4 };

        if (workspace.Status == SaasWorkspaceStatus.PastDue && !invoices.Any(other => other.WorkspaceId == workspace.Id && other.Status == SaasInvoiceStatus.Open))
            Replace(workspace with { Status = SaasWorkspaceStatus.Active });
    }

    /// <summary>
    /// Bills one due date of a workspace: the end of a trial or a renewal. Returns the updated workspace. Without a card the
    /// invoice stays open: the workspace is past due, the audit log says the payment failed, and Lumora's operators are told
    /// the invoice is overdue (the follow-up steps for a failed payment are a separate workflow).
    /// </summary>
    private SaasWorkspace Renew(SaasWorkspace workspace)
    {
        var plan = PlanOf(workspace);
        var dueOn = workspace.RenewsOn.Date;
        var wasTrial = workspace.Status == SaasWorkspaceStatus.Trial;

        if (plan.MonthlyPricePerSeat == 0)
        {
            var free = workspace with { Status = SaasWorkspaceStatus.Active, TrialEndsOn = null, Period = SaasBillingPeriod.Monthly, RenewsOn = dueOn.AddMonths(1) };
            Replace(free);

            return free;
        }

        var nextRenewal = SaasPricing.PeriodEnd(dueOn, workspace.Period);
        var amount = SaasPricing.PeriodAmount(plan, workspace.Period, workspace.Seats);
        var fromCredit = Math.Min(workspace.CreditBalance, amount);
        var due = amount - fromCredit;
        var hasCard = workspace.CardLast4 is not null;
        var description = $"{plan.Name} plan, {workspace.Seats} seats, {SaasSeedData.PeriodLabel(workspace.Period)}{(wasTrial ? " (trial ended)" : string.Empty)}"
            + (fromCredit > 0 ? $", {Money(fromCredit)} paid from credit" : string.Empty);
        var number = due > 0 ? AddInvoice(workspace.Id, dueOn, nextRenewal, description, due, hasCard ? workspace.CardBrand : null, hasCard ? workspace.CardLast4 : null,
            hasCard ? SaasInvoiceStatus.Paid : SaasInvoiceStatus.Open) : null;

        var stillOpen = invoices.Any(invoice => invoice.WorkspaceId == workspace.Id && invoice.Status == SaasInvoiceStatus.Open);
        var renewed = workspace with
        {
            Status = stillOpen ? SaasWorkspaceStatus.PastDue : SaasWorkspaceStatus.Active,
            TrialEndsOn = null,
            RenewsOn = nextRenewal,
            CreditBalance = workspace.CreditBalance - fromCredit,
        };
        Replace(renewed);

        var paidOrCovered = hasCard || due == 0;
        var title = wasTrial ? (paidOrCovered ? "Your trial ended and the plan started" : "Your trial ended: add a card to keep going")
            : paidOrCovered ? "Your plan renewed" : "Your renewal payment is due";
        var next = nextRenewal.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
        var text = due == 0
            ? $"Your credit balance paid the {Money(amount)} renewal. Next renewal: {next}."
            : hasCard
                ? $"{Money(due)} was charged to the {workspace.CardBrand} ending in {workspace.CardLast4}. Next renewal: {next}."
                : $"{Money(due)} is due. Add a card in Billing and pay the open invoice.";
        Log(workspace.Id, wasTrial ? "Trial ended" : "Renewed", description, wasTrial ? IconName.HourglassBottom : IconName.ArrowRepeat);

        // Automatic billing does this, so it reaches the signed-in person too.
        foreach (var owner in members.Where(member => member.WorkspaceId == workspace.Id && member.AccountId is not null && member.Role is SaasMemberRole.Owner or SaasMemberRole.Billing)
                     .Select(member => member.AccountId!).Distinct().ToList())
            Notify(owner, title, text, number is null ? BillingPath : AppInvoicePath(number), paidOrCovered ? IconName.Receipt : IconName.ExclamationTriangle, fromSystem: true);

        if (number is not null)
        {
            Record(Clock.Now, SaasSeedData.SystemActorName, hasCard ? "Invoice paid" : "Payment failed", InvoiceTarget(number), InvoicePath(number), hasCard
                ? $"{workspace.Name}: {Money(due)} charged to the {workspace.CardBrand} ending in {workspace.CardLast4}"
                : $"{workspace.Name}: {Money(due)} due on {dueOn.ToString("MMM d", CultureInfo.InvariantCulture)}; no card on file. The tenant is past due.");
        }

        if (!hasCard && number is not null)
        {
            NotifyOperators($"Invoice {number} is overdue", $"{workspace.Name}: {Money(due)} with no card on file. The tenant is past due.", InvoicePath(number),
                IconName.ExclamationTriangle, fromSystem: true);
        }

        return renewed;
    }

    /// <summary>
    /// What the workspace paid for its current period: its paid invoices ending at the renewal, less refunds. Proration values
    /// unused time at this amount, so an operator's later price change never credits more than was paid. A period with no
    /// invoice (one paid entirely from credit) is valued at the plan's price.
    /// </summary>
    private decimal PaidForCurrentPeriod(SaasWorkspace workspace)
    {
        var paid = invoices.Where(invoice => invoice.WorkspaceId == workspace.Id && invoice.Status != SaasInvoiceStatus.Open && invoice.PeriodEnd.Date == workspace.RenewsOn.Date)
            .Sum(invoice => invoice.Amount - invoice.RefundedAmount);

        return paid > 0 ? paid : SaasPricing.PeriodAmount(PlanOf(workspace), workspace.Period, workspace.Seats);
    }

    /// <summary>A profile change on the account page also renames the member it signs in as (T26-F2).</summary>
    private void SyncMemberProfile()
    {
        if (Identity.Current is not { } account)
            return;

        for (var index = 0; index < members.Count; index++)
        {
            if (members[index].AccountId != account.Id)
                continue;

            // Only values the snapshot restore accepts are copied, so a profile edit can never make the tab's data unloadable.
            var name = IsText(account.Name, ShowcaseIdentity.MaxNameLength) ? account.Name.Trim() : members[index].Name;
            var email = IsEmail(account.Email) ? account.Email.Trim() : members[index].Email;

            if (members[index].Name != name || members[index].Email != email)
                members[index] = members[index] with { Name = name, Email = email };
        }
    }

    /// <summary>Adds an invoice with the workspace's next free number and returns the number, or <see langword="null" /> when the tab holds the most invoices it can.</summary>
    private string? AddInvoice(int workspaceId, DateTime start, DateTime end, string description, decimal amount, string? brand, string? last4,
        SaasInvoiceStatus status = SaasInvoiceStatus.Paid)
    {
        var sequence = invoices.Count(invoice => invoice.WorkspaceId == workspaceId) + 1;

        while (invoices.Any(invoice => invoice.Number == SaasSeedData.InvoiceNumber(workspaceId, sequence)))
            sequence++;

        if (invoices.Count >= MaxInvoices)
            return null;

        var number = SaasSeedData.InvoiceNumber(workspaceId, sequence);
        invoices.Add(new SaasInvoice(number, workspaceId, Clock.Now, start, end, Trim(description, 120), amount, status,
            status == SaasInvoiceStatus.Paid ? brand : null, status == SaasInvoiceStatus.Paid ? last4 : null, 0m));

        return number;
    }

    private void AddNotice(string accountId, string title, string text, string? link, IconName iconName)
    {
        notices.Add(new SaasNotice(nextNoticeId++, accountId, Trim(title, MaxNoticeTitleLength), Trim(text, MaxNoticeTextLength), IsSiteLink(link) ? link : null, iconName, Clock.Now, IsRead: false));

        // Keep each account's newest 50 notices.
        var old = notices.Where(notice => notice.AccountId == accountId).OrderByDescending(notice => notice.Id).Skip(50).ToList();
        notices.RemoveAll(old.Contains);

        // And never more than the restore accepts in total.
        if (notices.Count > MaxNotices)
            notices.RemoveAll(notices.OrderBy(notice => notice.Id).Take(notices.Count - MaxNotices).ToList().Contains);
    }

    private void Log(int workspaceId, string title, string detail, IconName iconName)
    {
        activities.Add(new SaasActivity(workspaceId, Clock.Now, title, detail.Length > 200 ? detail[..200] : detail, iconName));

        if (activities.Count > MaxActivities)
            activities.Remove(activities.OrderBy(activity => activity.At).First());
    }

    /// <summary>Returns the accounts of a workspace's members who can sign in and match a filter, except the signed-in account.</summary>
    private IEnumerable<string> OthersToTell(int workspaceId, Func<SaasMember, bool> filter) =>
        members.Where(member => member.WorkspaceId == workspaceId && member.AccountId is not null && member.AccountId != Identity.Current?.Id && filter(member))
            .Select(member => member.AccountId!)
            .Distinct()
            .ToList();

    /// <summary>Whether a change starts a new billing period today: a new period length, or leaving a free plan.</summary>
    private bool PeriodRestarts(SaasWorkspace workspace, SaasPlan plan, SaasBillingPeriod period) =>
        plan.MonthlyPricePerSeat > 0 && (period != workspace.Period || PlanOf(workspace).MonthlyPricePerSeat == 0);

    private SaasPlanQuote Quote(SaasWorkspace workspace, SaasPlan plan, SaasBillingPeriod period, int seats)
    {
        var today = Clock.Today;
        var credit = workspace.CreditBalance;
        var isTrial = workspace.Status == SaasWorkspaceStatus.Trial;
        var oldAmount = isTrial ? 0m : PaidForCurrentPeriod(workspace);
        var share = isTrial ? 0m : SaasPricing.RemainingShare(today, workspace.RenewsOn, workspace.Period);
        var unused = SaasPricing.Cents(oldAmount * share);

        // Moving to the free plan credits the unused part of the paid period.
        if (plan.MonthlyPricePerSeat == 0)
            return new SaasPlanQuote(0m, credit + unused, 0m, today.AddMonths(1));

        var nextAmount = SaasPricing.PeriodAmount(plan, period, seats);

        // A new period starts today: the unused part of the old one and the credit balance pay towards it.
        if (isTrial || PeriodRestarts(workspace, plan, period))
        {
            var available = credit + unused;

            return new SaasPlanQuote(Math.Max(0m, nextAmount - available), Math.Max(0m, available - nextAmount), nextAmount, SaasPricing.PeriodEnd(today, period));
        }

        // Same period: the difference is prorated to the day; a downgrade's difference is credited.
        var difference = SaasPricing.Cents((nextAmount - oldAmount) * share);

        return difference >= 0
            ? new SaasPlanQuote(Math.Max(0m, difference - credit), Math.Max(0m, credit - difference), nextAmount, workspace.RenewsOn)
            : new SaasPlanQuote(0m, credit - difference, nextAmount, workspace.RenewsOn);
    }

    private void Replace(SaasWorkspace updated)
    {
        var index = workspaces.FindIndex(workspace => workspace.Id == updated.Id);

        if (index >= 0)
            workspaces[index] = updated;
    }

    private void Replace(SaasMember updated)
    {
        var index = members.FindIndex(member => member.Id == updated.Id);

        if (index >= 0)
            members[index] = updated;
    }

    private bool SeatsFit(SaasWorkspace workspace, SaasPlan plan, int seats) =>
        seats >= Math.Max(1, SeatsUsed(workspace.Id)) && seats <= (plan.SeatLimit == 0 ? SaasSeedData.MaxSeats : plan.SeatLimit);

    private bool ValidWorkspace(SaasWorkspace? workspace, Dictionary<string, SaasPlan> planById, int nextId) =>
        workspace is not null && workspace.Id >= 1 && workspace.Id < nextId && IsText(workspace.Name, MaxWorkspaceNameLength) && IsSubdomain(workspace.Subdomain)
        && (workspace.ManagerId is null || SaasSeedData.ManagerName(workspace.ManagerId) is not null)
        && planById.TryGetValue(workspace.PlanId ?? string.Empty, out var plan) && Enum.IsDefined(workspace.Period) && Enum.IsDefined(workspace.Status)
        && workspace.Seats >= 1 && workspace.Seats <= (plan.SeatLimit == 0 ? SaasSeedData.MaxSeats : Math.Max(plan.SeatLimit, workspace.Seats))
        && workspace.Seats <= SaasSeedData.MaxSeats
        && IsPlausibleDate(workspace.CreatedOn) && IsPlausibleDate(workspace.RenewsOn) && (workspace.TrialEndsOn is null || IsPlausibleDate(workspace.TrialEndsOn.Value))
        && (workspace.Status != SaasWorkspaceStatus.Trial || workspace.TrialEndsOn is not null)
        && IsText(workspace.Country, 60) && workspace.EventsThisMonth is >= 0 and <= 10_000_000_000 && workspace.Dashboards is >= 0 and <= 10_000 && IsMoney(workspace.CreditBalance)
        && (workspace.CardLast4 is null
            ? workspace.CardBrand is null && workspace.CardExpiryMonth == 0 && workspace.CardExpiryYear == 0
            : workspace.CardBrand is not null && cardBrands.Contains(workspace.CardBrand) && IsLast4(workspace.CardLast4) && workspace.CardExpiryMonth is >= 1 and <= 12
              && workspace.CardExpiryYear is >= 2000 and <= 2100)
        && ValidSettings(workspace.Settings);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the number of workspaces in good standing on a free or paid plan.</summary>
    public int ActiveCount => workspaces.Count(workspace => workspace.Status == SaasWorkspaceStatus.Active);

    /// <summary>Gets or sets the period shown on the operator's analytics screen.</summary>
    public SaasAnalyticsRange AnalyticsRange
    {
        get => analyticsRange;
        set
        {
            if (analyticsRange == value || !Enum.IsDefined(value))
                return;

            analyticsRange = value;
            Changed?.Invoke();
        }
    }

    /// <summary>Gets or sets whether the public pricing shows annual prices (the visitor's choice, kept in the tab).</summary>
    public bool AnnualBilling
    {
        get => annualBilling;
        set
        {
            if (annualBilling == value)
                return;

            annualBilling = value;
            Changed?.Invoke();
        }
    }

    /// <summary>Gets whether the signed-in member may change the plan and payment method (owners and billing).</summary>
    public bool CanManageBilling => CurrentMember is { Role: SaasMemberRole.Owner or SaasMemberRole.Billing } && IsWorkspaceUsable;

    /// <summary>Gets whether the signed-in member may change workspace settings (owners and admins).</summary>
    public bool CanManageSettings => CurrentMember is { Role: SaasMemberRole.Owner or SaasMemberRole.Admin } && IsWorkspaceUsable;

    /// <summary>Gets whether the signed-in member may invite, change, and remove members (owners and admins).</summary>
    public bool CanManageTeam => CurrentMember is { Role: SaasMemberRole.Owner or SaasMemberRole.Admin } && IsWorkspaceUsable;

    /// <summary>Gets the site's clock.</summary>
    public ShowcaseClock Clock { get; } = new(SaasSeedData.SeedTime);

    /// <summary>Gets the signed-in customer's membership, or <see langword="null" /> when signed out or not a customer.</summary>
    public SaasMember? CurrentMember =>
        Identity.Current is { RoleId: SaasSeedData.CustomerRole } account ? members.FirstOrDefault(member => member.AccountId == account.Id) : null;

    /// <summary>Gets the signed-in customer's workspace, or <see langword="null" />.</summary>
    public SaasWorkspace? CurrentWorkspace => CurrentMember is { } member ? FindWorkspace(member.WorkspaceId) : null;

    /// <summary>Gets the site's demo sign-in.</summary>
    public ShowcaseIdentity Identity { get; }

    /// <summary>Gets whether charts should use dark-theme colors.</summary>
    public bool IsDarkTheme => isDarkTheme;

    /// <summary>Gets whether the signed-in account is a Lumora operator.</summary>
    public bool IsOperator => Identity.Current?.RoleId == SaasSeedData.OperatorRole;

    /// <summary>Gets every invoice, newest first, for the operator console.</summary>
    public IReadOnlyList<SaasInvoice> AllInvoices => invoices.OrderByDescending(invoice => invoice.IssuedOn).ThenByDescending(invoice => invoice.Number).ToList();

    /// <summary>Gets the sales leads, newest first.</summary>
    public IReadOnlyList<SaasLead> Leads => leads.OrderByDescending(lead => lead.CreatedOn).ToList();

    /// <summary>Gets the monthly recurring revenue: what active and past-due workspaces pay per month.</summary>
    public decimal Mrr => workspaces.Sum(MonthlyRevenueFor);

    /// <summary>Gets the change of MRR against last month, in percent.</summary>
    public double MrrGrowthPercent
    {
        get
        {
            var history = RevenueHistory;
            var previous = history[^2];

            return previous == 0 ? 0 : Math.Round((history[^1] - previous) / previous * 100, 1);
        }
    }

    /// <summary>Gets the number of paid seats across workspaces that pay.</summary>
    public int PaidSeats => workspaces.Where(workspace => MonthlyRevenueFor(workspace) > 0).Sum(workspace => workspace.Seats);

    /// <summary>Gets the plans, in the order the pricing page shows them.</summary>
    public IReadOnlyList<SaasPlan> Plans => plans;

    /// <summary>Gets the newest activity across all workspaces, for the operator.</summary>
    public IReadOnlyList<SaasActivity> RecentActivity => activities.OrderByDescending(activity => activity.At).Take(6).ToList();

    /// <summary>Gets MRR for the last 12 months; the current month follows the live figure.</summary>
    public IReadOnlyList<double> RevenueHistory => SaasAnalyticsSeedData.RevenueHistory(seedMrr, Mrr);

    /// <summary>Gets the revenue targets for the last 12 months.</summary>
    public IReadOnlyList<double> RevenueTargets => SaasAnalyticsSeedData.RevenueTargets(seedMrr);

    /// <summary>Gets the site's slug, used in the storage key.</summary>
    public string SiteSlug => "saas";

    /// <summary>Gets the snapshot format version (3: hundreds of tenants stored as changes to the seed, subdomains, account managers, and the shared audit log).</summary>
    public int SnapshotVersion => 3;

    /// <summary>Gets the number of suspended workspaces.</summary>
    public int SuspendedCount => workspaces.Count(workspace => workspace.Status == SaasWorkspaceStatus.Suspended);

    /// <summary>Gets the number of workspaces on trial.</summary>
    public int TrialCount => workspaces.Count(workspace => workspace.Status == SaasWorkspaceStatus.Trial);

    /// <summary>Gets every workspace, for the operator console.</summary>
    public IReadOnlyList<SaasWorkspace> Workspaces => workspaces;

    private bool IsWorkspaceUsable => CurrentWorkspace is { Status: not (SaasWorkspaceStatus.Suspended or SaasWorkspaceStatus.Archived) };

    #endregion

    #region Events

    /// <summary>Raised when anything in the state changes, including sign-in.</summary>
    public event Action? Changed;

    /// <summary>Raised when the site's theme changes, so charts can redraw.</summary>
    public event Action? ThemeChanged;

    #endregion

    #region Nested Types

    /// <summary>
    /// Lumora's part of the tab snapshot. The plans and counters are stored whole; every other list holds only the records
    /// that differ from the seed (changed or added) and the keys of seeded records that were removed, and is rebuilt over the
    /// seed on restore.
    /// </summary>
    private sealed class SaasSnapshot
    {
        #region Properties, Indexers

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<SaasActivity>? Activities { get; set; }

        public bool AnnualBilling { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ShowcaseAuditEntry>? Audit { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<SaasInvoice>? Invoices { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<SaasLead>? Leads { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<SaasMember>? Members { get; set; }

        public int NextAuditId { get; set; }

        public int NextLeadId { get; set; }

        public int NextMemberId { get; set; }

        public int NextNoticeId { get; set; }

        public int NextWorkspaceId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<SaasNotice>? Notices { get; set; }

        public List<SaasPlan>? Plans { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<SaasActivity>? RemovedActivities { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedAuditIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? RemovedInvoiceNumbers { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedLeadIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedMemberIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedNoticeIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedWorkspaceIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<SaasWorkspace>? Workspaces { get; set; }

        #endregion
    }

    /// <summary>The fresh tab's data, built from <see cref="SaasSeedData" />; the records are immutable, so every tab shares them.</summary>
    private sealed record SeedSet(
        IReadOnlyList<SaasPlan> Plans,
        IReadOnlyList<SaasWorkspace> Workspaces,
        IReadOnlyList<SaasMember> Members,
        IReadOnlyList<SaasInvoice> Invoices,
        IReadOnlyList<SaasActivity> Activities,
        IReadOnlyList<SaasNotice> Notices,
        IReadOnlyList<SaasLead> Leads,
        IReadOnlyList<ShowcaseAuditEntry> Audit)
    {
        public static SeedSet Build()
        {
            var workspaces = SaasSeedData.Workspaces();
            var members = SaasSeedData.Members(workspaces);
            var invoices = SaasSeedData.Invoices(workspaces);
            var leads = SaasSeedData.Leads();

            return new SeedSet(SaasSeedData.Plans(), workspaces, members, invoices, SaasSeedData.Activities(workspaces), SaasSeedData.Notices(workspaces, members, invoices), leads,
                SaasSeedData.Audit(workspaces, members, invoices, leads));
        }
    }

    #endregion
}
