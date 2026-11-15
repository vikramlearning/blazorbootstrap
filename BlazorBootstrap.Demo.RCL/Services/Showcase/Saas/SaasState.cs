using System.Text.Json;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

/// <summary>
/// State of Lumora for the current browser tab, shared by the public site (visitors), the customer workspace, and the
/// operator console: the plans, the customer workspaces with their teams, invoices, and activity, notifications, sales leads,
/// and the demo sign-in. It is kept in the tab's <c>sessionStorage</c> through the showcase foundation, so it survives a
/// reload in the tab; nothing is sent or charged.
/// </summary>
public sealed class SaasState : IShowcaseSiteState
{
    #region Fields and Constants

    /// <summary>The customer workspace's start page.</summary>
    public const string AppPath = "/showcase/saas/app";

    /// <summary>The customer workspace's billing page.</summary>
    public const string BillingPath = AppPath + "/billing";

    /// <summary>The longest workspace name.</summary>
    public const int MaxWorkspaceNameLength = 60;

    private const int MaxActivities = 5000;

    private const int MaxInvoices = 10_000;

    private const int MaxLeads = 500;

    private const int MaxMembers = 5000;

    private const int MaxNotices = 2000;

    private const int MaxWorkspaces = 500;

    private static readonly string[] cardBrands = ShowcaseCards.TestCards.Select(card => card.Brand).Distinct().ToArray();

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<SaasActivity> activities;

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

    private int nextLeadId;

    private int nextMemberId;

    private int nextNoticeId;

    private int nextWorkspaceId;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out.</summary>
    public SaasState()
    {
        plans = SaasSeedData.Plans();
        workspaces = SaasSeedData.Workspaces();
        members = SaasSeedData.Members(workspaces);
        invoices = SaasSeedData.Invoices(workspaces);
        activities = SaasSeedData.Activities(workspaces);
        notices = SaasSeedData.Notices();
        leads = SaasSeedData.Leads();
        nextWorkspaceId = workspaces.Max(workspace => workspace.Id) + 1;
        nextMemberId = members.Max(member => member.Id) + 1;
        nextNoticeId = notices.Max(notice => notice.Id) + 1;
        nextLeadId = leads.Max(lead => lead.Id) + 1;
        seedMrr = Mrr;

        Identity = new ShowcaseIdentity(SaasSeedData.Roles(), SaasSeedData.DemoAccounts());
        Identity.Changed += () => Changed?.Invoke();
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns a workspace's activity, newest first.</summary>
    public IReadOnlyList<SaasActivity> ActivityOf(int workspaceId) =>
        activities.Where(activity => activity.WorkspaceId == workspaceId).OrderByDescending(activity => activity.At).ToList();

    /// <summary>Returns the snapshot of Lumora: plans, workspaces, teams, invoices, notices, leads, activity, and the ID counters.</summary>
    public object CaptureData() => new SaasSnapshot
    {
        Activities = activities.ToList(),
        AnnualBilling = annualBilling,
        Invoices = invoices.ToList(),
        Leads = leads.ToList(),
        Members = members.ToList(),
        NextLeadId = nextLeadId,
        NextMemberId = nextMemberId,
        NextNoticeId = nextNoticeId,
        NextWorkspaceId = nextWorkspaceId,
        Notices = notices.ToList(),
        Plans = plans.ToList(),
        Workspaces = workspaces.ToList(),
    };

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
            Status = isTrial || (workspace.Status == SaasWorkspaceStatus.PastDue && quote.ChargeToday > 0) ? SaasWorkspaceStatus.Active : workspace.Status,
            TrialEndsOn = null,
            RenewsOn = quote.NextRenewal,
            CreditBalance = quote.CreditAfter,
        };
        Replace(updated);

        var summary = $"{plan.Name}, {seats} {(seats == 1 ? "seat" : "seats")}{(paid ? $", {SaasSeedData.PeriodLabel(updated.Period)}" : string.Empty)}";

        if (quote.ChargeToday > 0)
        {
            var start = isTrial || PeriodRestarts(workspace, plan, period) ? today : PeriodStart(workspace);
            AddInvoice(workspace.Id, start, quote.NextRenewal, isTrial ? $"{summary} (trial ended)" : $"Change to {summary}, prorated", quote.ChargeToday, workspace.CardBrand, workspace.CardLast4);
        }

        Log(workspace.Id, isTrial ? "Trial converted" : "Plan changed", summary, IconName.ArrowUpCircle);

        foreach (var billingMember in OthersToTell(workspace.Id, member => member.Role is SaasMemberRole.Owner or SaasMemberRole.Billing))
            AddNotice(billingMember, "The plan changed", $"{Identity.Current!.Name} moved {workspace.Name} to {summary}.", BillingPath, IconName.ArrowUpCircle);

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

    /// <summary>Invites someone to the signed-in member's workspace; the invitation holds a seat.</summary>
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

        members.Add(new SaasMember(nextMemberId++, workspace.Id, null, name, email, role, SaasMemberStatus.Invited, Clock.Now, null));
        Log(workspace.Id, "Teammate invited", $"{name} as {RoleLabel(role)}", IconName.PersonPlus);
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
    public void NotifyRestored() => Changed?.Invoke();

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

        members.Remove(target);
        Log(me.WorkspaceId, target.Status == SaasMemberStatus.Invited ? "Invitation cancelled" : "Teammate removed", target.Name, IconName.PersonDash);
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
    /// Growth starts a 14-day trial without a card; Starter is free from the start. Enterprise goes through sales.
    /// </summary>
    public ShowcaseAccountResult StartTrial(string name, string email, string password, string company, string country, string planId, SaasBillingPeriod period)
    {
        // The same checks the snapshot restore applies, so a sign-up never produces data a reload would discard.
        if (FindPlan(planId) is not { IsSelfServe: true } plan || !Enum.IsDefined(period) || !IsText(company?.Trim(), MaxWorkspaceNameLength)
            || !IsText(name.Trim(), ShowcaseIdentity.MaxNameLength) || !IsEmail(email.Trim())
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
            0, 0, 0m, null, null, 0, 0, SaasSeedData.DefaultSettings(country));
        workspaces.Add(workspace);
        members.Add(new SaasMember(nextMemberId++, workspace.Id, account.Id, account.Name, account.Email, SaasMemberRole.Owner, SaasMemberStatus.Active, Clock.Now, Clock.Now));
        Log(workspace.Id, "Workspace created", trial ? $"{plan.Name} trial, {SaasSeedData.TrialDays} days" : $"{plan.Name} plan", IconName.Flag);
        AddNotice(account.Id, $"Welcome to {SaasSeedData.ProductName}",
            trial ? $"Your {plan.Name} trial runs until {trialEndsOn:MMM d}. Invite your team and send your first events." : "Invite your team and send your first events.",
            AppPath, IconName.Stars);
        Changed?.Invoke();

        return ShowcaseAccountResult.Success;
    }

    /// <summary>Records a message from the public "Contact sales" form; returns <see langword="null" /> when a field is out of range.</summary>
    public SaasLead? SubmitLead(string name, string email, string company, string teamSize, string message)
    {
        if (!IsText(name?.Trim(), 60) || !IsEmail(email?.Trim()) || !IsText(company?.Trim(), 60) || !SaasSeedData.TeamSizes.Contains(teamSize)
            || !IsText(message?.Trim(), 1000) || leads.Count >= MaxLeads)
            return null;

        var lead = new SaasLead(nextLeadId++, name!.Trim(), email!.Trim(), company!.Trim(), teamSize, message!.Trim(), Clock.Now, SaasLeadStatus.New);
        leads.Add(lead);
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
        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>Saves the signed-in member's workspace name and settings (owners and admins).</summary>
    public SaasResult UpdateWorkspace(string name, SaasWorkspaceSettings settings)
    {
        if (!CanManageSettings || CurrentWorkspace is not { } workspace)
            return SaasResult.NotAllowed;

        if (!IsText(name?.Trim(), MaxWorkspaceNameLength) || !ValidSettings(settings))
            return SaasResult.Invalid;

        var updated = workspace with { Name = name!.Trim(), Settings = settings };

        if (updated == workspace)
            return SaasResult.NoChange;

        Replace(updated);
        Log(workspace.Id, "Settings saved", updated.Name, IconName.Gear);
        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>
    /// Changes a workspace's plan, seats, or status from the operator console. Seats can't drop below the members and
    /// invitations; a suspension notifies the workspace's owners who can sign in.
    /// </summary>
    public SaasResult UpdateWorkspaceAsOperator(int workspaceId, string planId, int seats, SaasWorkspaceStatus status)
    {
        if (!IsOperator)
            return SaasResult.NotAllowed;

        if (FindWorkspace(workspaceId) is not { } workspace || FindPlan(planId) is not { } plan || !Enum.IsDefined(status))
            return SaasResult.NotFound;

        if (!SeatsFit(workspace, plan, seats))
            return SaasResult.SeatsOutOfRange;

        var updated = workspace with
        {
            PlanId = plan.Id,
            Period = plan.MonthlyPricePerSeat == 0 ? SaasBillingPeriod.Monthly : workspace.Period,
            Seats = seats,
            Status = status,
            TrialEndsOn = status == SaasWorkspaceStatus.Trial ? workspace.TrialEndsOn ?? Clock.Today.AddDays(SaasSeedData.TrialDays) : null,
        };

        if (updated == workspace)
            return SaasResult.NoChange;

        Replace(updated);
        Log(workspace.Id, "Changed by Lumora", $"{plan.Name}, {seats} seats, {status}", IconName.ShieldCheck);

        if (NotifiesOwner(workspace.Status, status))
        {
            foreach (var owner in OthersToTell(workspace.Id, member => member.Role == SaasMemberRole.Owner))
            {
                AddNotice(owner, status == SaasWorkspaceStatus.Suspended ? "Your workspace is suspended" : "Your workspace is active again",
                    status == SaasWorkspaceStatus.Suspended ? $"{workspace.Name} was suspended by Lumora. Contact support to restore it." : $"{workspace.Name} can be used again.",
                    AppPath, status == SaasWorkspaceStatus.Suspended ? IconName.SlashCircle : IconName.CheckCircle);
            }
        }

        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>Validates a stored snapshot and applies it only when every part is valid.</summary>
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

        if (snapshot is not
            {
                Plans: { Count: >= 1 and <= 10 } storedPlans,
                Workspaces: { Count: >= 1 and <= MaxWorkspaces } storedWorkspaces,
                Members: { Count: <= MaxMembers } storedMembers,
                Invoices: { Count: <= MaxInvoices } storedInvoices,
                Notices: { Count: <= MaxNotices } storedNotices,
                Leads: { Count: <= MaxLeads } storedLeads,
                Activities: { Count: <= MaxActivities } storedActivities,
            }
            || snapshot.NextWorkspaceId is < 2 or > 100_000
            || snapshot.NextMemberId is < 2 or > 1_000_000
            || snapshot.NextNoticeId is < 1 or > 1_000_000
            || snapshot.NextLeadId is < 1 or > 100_000)
            return false;

        if (storedPlans.Any(plan => plan is null || !IsText(plan.Id, 20) || !IsText(plan.Name, 40) || !IsText(plan.Description, 200) || !IsMoney(plan.MonthlyPricePerSeat)
                || plan.Highlights is not { Count: <= 8 } || plan.Highlights.Any(line => !IsText(line, 80)) || !IsText(plan.CallToAction, 40)
                || plan.SeatLimit is < 0 or > SaasSeedData.MaxSeats || plan.EventLimit is < 0 or > 10_000_000_000 || plan.DashboardLimit is < 0 or > 10_000)
            || storedPlans.Select(plan => plan.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() != storedPlans.Count)
            return false;

        var planById = storedPlans.ToDictionary(plan => plan.Id, StringComparer.OrdinalIgnoreCase);

        if (storedWorkspaces.Any(workspace => !ValidWorkspace(workspace, planById, snapshot.NextWorkspaceId))
            || storedWorkspaces.Select(workspace => workspace.Id).Distinct().Count() != storedWorkspaces.Count)
            return false;

        var workspaceIds = storedWorkspaces.Select(workspace => workspace.Id).ToHashSet();

        if (storedMembers.Any(member => member is null || member.Id < 1 || member.Id >= snapshot.NextMemberId || !workspaceIds.Contains(member.WorkspaceId)
                || (member.AccountId is not null && !IsAccountId(member.AccountId)) || !IsText(member.Name, ShowcaseIdentity.MaxNameLength) || !IsEmail(member.Email)
                || !Enum.IsDefined(member.Role) || !Enum.IsDefined(member.Status) || !IsPlausibleDate(member.AddedOn)
                || (member.LastActiveOn is { } lastActive && !IsPlausibleDate(lastActive)))
            || storedMembers.Select(member => member.Id).Distinct().Count() != storedMembers.Count
            || storedWorkspaces.Any(workspace => storedMembers.Count(member => member.WorkspaceId == workspace.Id) > workspace.Seats
                || !storedMembers.Any(member => member.WorkspaceId == workspace.Id && member.Role == SaasMemberRole.Owner)))
            return false;

        if (storedInvoices.Any(invoice => invoice is null || !IsText(invoice.Number, 20) || !workspaceIds.Contains(invoice.WorkspaceId) || !IsPlausibleDate(invoice.IssuedOn)
                || !IsPlausibleDate(invoice.PeriodStart) || !IsPlausibleDate(invoice.PeriodEnd) || invoice.PeriodEnd < invoice.PeriodStart || !IsText(invoice.Description, 120)
                || !IsMoney(invoice.Amount) || !IsMoney(invoice.RefundedAmount) || invoice.RefundedAmount > invoice.Amount || !Enum.IsDefined(invoice.Status)
                || (invoice.CardBrand is not null && !cardBrands.Contains(invoice.CardBrand)) || (invoice.CardLast4 is not null && !IsLast4(invoice.CardLast4)))
            || storedInvoices.Select(invoice => invoice.Number).Distinct().Count() != storedInvoices.Count)
            return false;

        if (storedNotices.Any(notice => notice is null || notice.Id < 1 || notice.Id >= snapshot.NextNoticeId || !IsAccountId(notice.AccountId) || !IsText(notice.Title, 80)
                || !IsText(notice.Text, 240) || (notice.Link is not null && !IsSitePath(notice.Link)) || !Enum.IsDefined(notice.IconName) || !IsPlausibleDate(notice.CreatedOn))
            || storedNotices.Select(notice => notice.Id).Distinct().Count() != storedNotices.Count)
            return false;

        if (storedLeads.Any(lead => lead is null || lead.Id < 1 || lead.Id >= snapshot.NextLeadId || !IsText(lead.Name, 60) || !IsEmail(lead.Email) || !IsText(lead.Company, 60)
                || !SaasSeedData.TeamSizes.Contains(lead.TeamSize) || !IsText(lead.Message, 1000) || !IsPlausibleDate(lead.CreatedOn) || !Enum.IsDefined(lead.Status))
            || storedLeads.Select(lead => lead.Id).Distinct().Count() != storedLeads.Count)
            return false;

        if (storedActivities.Any(activity => activity is null || !workspaceIds.Contains(activity.WorkspaceId) || !IsPlausibleDate(activity.At) || !IsText(activity.Title, 80)
                || !IsText(activity.Detail, 200) || !Enum.IsDefined(activity.IconName)))
            return false;

        Reload(plans, storedPlans);
        Reload(workspaces, storedWorkspaces);
        Reload(members, storedMembers);
        Reload(invoices, storedInvoices);
        Reload(notices, storedNotices);
        Reload(leads, storedLeads);
        Reload(activities, storedActivities);
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

    private static bool IsSitePath(string value) => value.Length <= 200 && (value == AppPath || value.StartsWith(AppPath + "/", StringComparison.Ordinal));

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

    private static string NameFromEmail(string email)
    {
        var local = email.Split('@')[0].Replace('.', ' ').Replace('_', ' ').Replace('-', ' ').Trim();

        return local.Length == 0 ? email : string.Join(' ', local.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
    }

    private static DateTime PeriodStart(SaasWorkspace workspace) =>
        workspace.Period == SaasBillingPeriod.Annual ? workspace.RenewsOn.AddYears(-1) : workspace.RenewsOn.AddMonths(-1);

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

    private static bool ValidSettings(SaasWorkspaceSettings? settings) =>
        settings is not null && SaasSeedData.TimeZones.Contains(settings.TimeZone) && settings.WeekStartsOn is "Monday" or "Sunday"
        && SaasSeedData.DataRegions.Contains(settings.DataRegion);

    private void AddInvoice(int workspaceId, DateTime start, DateTime end, string description, decimal amount, string? brand, string? last4)
    {
        var sequence = invoices.Count(invoice => invoice.WorkspaceId == workspaceId) + 1;

        while (invoices.Any(invoice => invoice.Number == SaasSeedData.InvoiceNumber(workspaceId, sequence)))
            sequence++;

        if (invoices.Count < MaxInvoices)
            invoices.Add(new SaasInvoice(SaasSeedData.InvoiceNumber(workspaceId, sequence), workspaceId, Clock.Now, start, end, description, amount, SaasInvoiceStatus.Paid, brand, last4, 0m));
    }

    private void AddNotice(string accountId, string title, string text, string? link, IconName iconName)
    {
        notices.Add(new SaasNotice(nextNoticeId++, accountId, title, text, link, iconName, Clock.Now, IsRead: false));

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
        var oldAmount = isTrial ? 0m : SaasPricing.PeriodAmount(PlanOf(workspace), workspace.Period, workspace.Seats);
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
        workspace is not null && workspace.Id >= 1 && workspace.Id < nextId && IsText(workspace.Name, MaxWorkspaceNameLength)
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

    /// <summary>Gets the snapshot format version.</summary>
    public int SnapshotVersion => 1;

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

    /// <summary>Lumora's part of the tab snapshot.</summary>
    private sealed class SaasSnapshot
    {
        #region Properties, Indexers

        public List<SaasActivity>? Activities { get; set; }

        public bool AnnualBilling { get; set; }

        public List<SaasInvoice>? Invoices { get; set; }

        public List<SaasLead>? Leads { get; set; }

        public List<SaasMember>? Members { get; set; }

        public int NextLeadId { get; set; }

        public int NextMemberId { get; set; }

        public int NextNoticeId { get; set; }

        public int NextWorkspaceId { get; set; }

        public List<SaasNotice>? Notices { get; set; }

        public List<SaasPlan>? Plans { get; set; }

        public List<SaasWorkspace>? Workspaces { get; set; }

        #endregion
    }

    #endregion
}
