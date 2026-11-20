using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

/// <summary>
/// The enterprise baseline of <see cref="SaasState" /> (rule WR-001): the audit log, the header's notifications (the same
/// list as the customer's Notifications page, so the bell and the page always agree), the header's global search, the
/// uniqueness checks the forms ask while someone types, the tenant and lead lists' bulk actions, operator-added leads, and
/// the demo's way to accept an invitation on the invitee's behalf. Audit entries and notifications link only to pages on
/// this site.
/// </summary>
public sealed partial class SaasState
{
    #region Fields and Constants

    /// <summary>The site's base route (the same as <c>SaasLayout.Base</c>).</summary>
    private const string SiteBase = "/showcase/saas";

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Accepts an open invitation on the invitee's behalf (a demo shortcut: invitees have no sign-in in this demo). The member
    /// becomes active; the audit log records the invitee as the one who accepted, and the workspace's owners and admins who
    /// can sign in are told, the signed-in person included, since someone else accepted.
    /// </summary>
    public SaasResult AcceptInvitationForDemo(int memberId)
    {
        if (!CanManageTeam || CurrentMember is not { } me || CurrentWorkspace is not { } workspace)
            return SaasResult.NotAllowed;

        if (members.FirstOrDefault(member => member.Id == memberId && member.WorkspaceId == me.WorkspaceId) is not { } target)
            return SaasResult.NotFound;

        if (target.Status != SaasMemberStatus.Invited)
            return SaasResult.NoChange;

        var joined = target with { Status = SaasMemberStatus.Active, LastActiveOn = Clock.Now };
        Replace(joined);
        Log(workspace.Id, "Invitation accepted", $"{target.Name} joined as {RoleLabel(target.Role)}", IconName.PersonCheck);
        Record(Clock.Now, target.Name, "Invitation accepted", MemberTarget(target), TenantPath(workspace.Id), $"Joined as {RoleLabel(target.Role)}");

        foreach (var accountId in members.Where(member => member.WorkspaceId == workspace.Id && member.AccountId is not null && member.Role is SaasMemberRole.Owner or SaasMemberRole.Admin)
                     .Select(member => member.AccountId!).Distinct().ToList())
        {
            Notify(accountId, $"{target.Name} accepted your invitation", $"{target.Name} joined {workspace.Name} as {RoleLabel(target.Role)}.", AppPath + "/team", IconName.PersonCheck,
                fromSystem: true);
        }

        Changed?.Invoke();

        return SaasResult.Ok;
    }

    /// <summary>
    /// Changes several leads at once from the leads inbox (operator): sets their status, and adds the reason to the front of
    /// each note. Closing needs a reason, so every closed lead says why. Each change is audited like the single update.
    /// </summary>
    public SaasBulkOutcome BulkUpdateLeads(IReadOnlyCollection<int> leadIds, SaasLeadStatus status, string? reason)
    {
        if (!IsOperator)
            return new SaasBulkOutcome(SaasResult.NotAllowed, 0, 0);

        if (!Enum.IsDefined(status))
            return new SaasBulkOutcome(SaasResult.NotFound, 0, 0);

        reason = reason?.Trim() ?? string.Empty;
        var ids = (leadIds ?? Array.Empty<int>()).Distinct().ToList();

        if (ids.Count == 0 || reason.Length > MaxLeadReasonLength || (status == SaasLeadStatus.Closed && reason.Length == 0))
            return new SaasBulkOutcome(SaasResult.Invalid, 0, 0);

        int changed = 0, unchanged = 0;

        foreach (var id in ids)
        {
            if (leads.FindIndex(lead => lead.Id == id) is not (var index and >= 0))
            {
                unchanged++;
                continue;
            }

            var lead = leads[index];
            var note = reason.Length == 0 ? lead.Note : Trim($"{reason} {lead.Note}".Trim(), MaxLeadMessageLength);

            if (lead.Status == status && lead.Note == note)
            {
                unchanged++;
                continue;
            }

            leads[index] = lead with { Status = status, Note = note };
            Record("Lead updated", LeadTarget(lead), LeadPath(lead.Id),
                (lead.Status == status ? "Note added" : $"{lead.Status} → {status}") + (reason.Length == 0 ? string.Empty : $": {reason}"));
            changed++;
        }

        if (changed == 0)
            return new SaasBulkOutcome(SaasResult.NoChange, 0, unchanged);

        Changed?.Invoke();

        return new SaasBulkOutcome(SaasResult.Ok, changed, unchanged);
    }

    /// <summary>
    /// Changes several tenants at once from the tenant list (operator): moves them to a plan, suspends or reactivates them, or
    /// assigns an account manager (<c>""</c> removes it; <see langword="null" /> leaves it). Archived tenants are left as they
    /// are, and so is a tenant whose members and invitations don't fit the new plan's seat limit. Each change is audited and
    /// notified like the single edit.
    /// </summary>
    public SaasBulkOutcome BulkUpdateTenants(IReadOnlyCollection<int> workspaceIds, string? planId, SaasWorkspaceStatus? status, string? managerId)
    {
        if (!IsOperator)
            return new SaasBulkOutcome(SaasResult.NotAllowed, 0, 0);

        var plan = planId is null ? null : FindPlan(planId);

        if ((planId is not null && plan is null) || (managerId is { Length: > 0 } && SaasSeedData.ManagerName(managerId) is null))
            return new SaasBulkOutcome(SaasResult.NotFound, 0, 0);

        var ids = (workspaceIds ?? Array.Empty<int>()).Distinct().ToList();

        if (ids.Count == 0 || (plan is null && status is null && managerId is null) || status is not (null or SaasWorkspaceStatus.Suspended or SaasWorkspaceStatus.Active))
            return new SaasBulkOutcome(SaasResult.Invalid, 0, 0);

        int changed = 0, unchanged = 0;

        foreach (var workspace in ids.Select(FindWorkspace).OfType<SaasWorkspace>().ToList())
        {
            if (workspace.Status == SaasWorkspaceStatus.Archived)
            {
                unchanged++;
                continue;
            }

            var targetPlan = plan is not null && SeatsFit(workspace, plan, workspace.Seats) ? plan : PlanOf(workspace);
            var targetStatus = status switch
            {
                SaasWorkspaceStatus.Suspended => SaasWorkspaceStatus.Suspended,
                SaasWorkspaceStatus.Active when workspace.Status == SaasWorkspaceStatus.Suspended => SaasWorkspaceStatus.Active,
                _ => workspace.Status,
            };
            var targetManager = managerId is null ? workspace.ManagerId : managerId.Length == 0 ? null : managerId;
            var updated = OperatorChange(workspace, targetPlan, workspace.Seats, targetStatus, targetManager);

            if (updated == workspace)
            {
                unchanged++;
                continue;
            }

            Replace(updated);
            var target = workspace.Name;
            var link = TenantPath(workspace.Id);

            if (updated.PlanId != workspace.PlanId)
                Record("Plan changed", target, link, $"{PlanOf(workspace).Name} → {targetPlan.Name}, from the tenant list");

            if (updated.Status != workspace.Status)
                Record(StatusAction(updated.Status), target, link, $"{SaasDisplayStatus(workspace.Status)} → {SaasDisplayStatus(updated.Status)}, from the tenant list");

            if (updated.ManagerId != workspace.ManagerId)
            {
                Record(updated.ManagerId is null ? "Account manager removed" : "Account manager assigned", target, link,
                    updated.ManagerId is null ? $"{SaasSeedData.ManagerName(workspace.ManagerId)} no longer looks after it" : $"To {SaasSeedData.ManagerName(updated.ManagerId)}");
            }

            Log(workspace.Id, "Changed by Lumora", string.Create(CultureInfo.InvariantCulture, $"{targetPlan.Name}, {updated.Seats} seats, {updated.Status}"), IconName.ShieldCheck);
            TellAboutOperatorChange(workspace, updated);
            changed++;
        }

        if (changed == 0)
            return new SaasBulkOutcome(SaasResult.NoChange, 0, unchanged);

        Changed?.Invoke();

        return new SaasBulkOutcome(SaasResult.Ok, changed, unchanged);
    }

    /// <summary>
    /// Adds a lead from the operator console (a call or an event, not the contact form): the same fields and limits as the
    /// form, a status, and a note. A closed lead needs a note, and an email can have one open lead at a time.
    /// </summary>
    /// <returns>The result (<see cref="SaasResult.Duplicate" /> when the email already has an open lead) and the new lead on success.</returns>
    public (SaasResult Result, SaasLead? Lead) CreateLead(string name, string email, string company, string teamSize, string message, SaasLeadStatus status, string? note)
    {
        if (!IsOperator)
            return (SaasResult.NotAllowed, null);

        note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();

        if (!IsText(name?.Trim(), MaxLeadTextLength) || !IsEmail(email?.Trim()) || !IsText(company?.Trim(), MaxLeadTextLength) || !SaasSeedData.TeamSizes.Contains(teamSize)
            || !IsText(message?.Trim(), MaxLeadMessageLength) || !Enum.IsDefined(status) || note is { Length: > MaxLeadMessageLength }
            || (status == SaasLeadStatus.Closed && note is null) || leads.Count >= MaxLeads)
            return (SaasResult.Invalid, null);

        if (HasOpenLead(email))
            return (SaasResult.Duplicate, null);

        var lead = new SaasLead(nextLeadId++, name!.Trim(), email!.Trim(), company!.Trim(), teamSize, message!.Trim(), Clock.Now, status, note);
        leads.Add(lead);
        Record("Lead added", LeadTarget(lead), LeadPath(lead.Id), $"{lead.Status}: {lead.Name}, {lead.TeamSize} people");
        Changed?.Invoke();

        return (SaasResult.Ok, lead);
    }

    /// <summary>Returns a free subdomain made from a name: the name's own when it is free, otherwise with -2, -3, and so on.</summary>
    public string FreeSubdomain(string? name)
    {
        var stem = SaasSeedData.SubdomainFrom(name ?? string.Empty);

        if (!IsSubdomain(stem) && !IsSubdomain(stem = $"{stem}-team".Trim('-')))
            stem = "workspace";

        if (!IsSubdomainTaken(stem))
            return stem;

        for (var suffix = 2; ; suffix++)
        {
            var tail = string.Create(CultureInfo.InvariantCulture, $"-{suffix}");
            var candidate = stem[..Math.Min(stem.Length, MaxSubdomainLength - tail.Length)].TrimEnd('-') + tail;

            if (!IsSubdomainTaken(candidate))
                return candidate;
        }
    }

    /// <summary>Returns the signed-in account's notifications for the header's menu, newest first: the same list as <see cref="NoticesFor" />.</summary>
    public IReadOnlyList<ShowcaseNotification> GetNotificationsForCurrentAccount() =>
        Identity.Current is not { } account
            ? Array.Empty<ShowcaseNotification>()
            : NoticesFor(account.Id).Select(notice => new ShowcaseNotification
            {
                Id = notice.Id,
                AccountId = notice.AccountId,
                At = notice.CreatedOn,
                IsRead = notice.IsRead,
                Title = notice.Title,
                Text = notice.Text,
                Url = IsSiteLink(notice.Link) ? notice.Link : null,
            }).ToList();

    /// <summary>Returns whether another open (not closed) lead has this email, ignoring case; the lead being edited doesn't count.</summary>
    public bool HasOpenLead(string? email, int exceptLeadId = 0) =>
        leads.Any(lead => lead.Id != exceptLeadId && lead.Status != SaasLeadStatus.Closed && string.Equals(lead.Email, email?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns whether an account of the site (seeded or signed up) already uses this email, ignoring case.</summary>
    public bool IsAccountEmailTaken(string? email) =>
        Identity.Accounts.Any(account => string.Equals(account.Email, email?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns whether someone in the signed-in member's workspace (a member or an open invitation) has this email, ignoring case.</summary>
    public bool IsInTeam(string? email) =>
        CurrentWorkspace is { } workspace && members.Any(member => member.WorkspaceId == workspace.Id && string.Equals(member.Email, email?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns whether another plan has this name, ignoring case.</summary>
    public bool IsPlanNameTaken(string? name, string? exceptPlanId) =>
        plans.Any(plan => plan.Id != exceptPlanId && string.Equals(plan.Name.Trim(), name?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Returns whether a subdomain has the right form: <see cref="MinSubdomainLength" /> to <see cref="MaxSubdomainLength" />
    /// lower-case letters, digits, and single hyphens, starting with a letter and not ending with a hyphen, and not one Lumora
    /// keeps for itself.
    /// </summary>
    public static bool IsSubdomain(string? value) =>
        value is { Length: >= MinSubdomainLength and <= MaxSubdomainLength } && value[0] is >= 'a' and <= 'z' && value[^1] != '-'
        && value.All(character => character is >= 'a' and <= 'z' or >= '0' and <= '9' or '-') && !value.Contains("--", StringComparison.Ordinal)
        && !SaasSeedData.ReservedSubdomains.Contains(value);

    /// <summary>Returns whether another workspace has this subdomain, ignoring case.</summary>
    public bool IsSubdomainTaken(string? subdomain, int exceptWorkspaceId = 0) =>
        workspaces.Any(workspace => workspace.Id != exceptWorkspaceId && string.Equals(workspace.Subdomain, subdomain?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns whether another workspace has this name, ignoring case and surrounding spaces; the workspace being edited doesn't count.</summary>
    public bool IsWorkspaceNameTaken(string? name, int exceptWorkspaceId = 0) =>
        workspaces.Any(workspace => workspace.Id != exceptWorkspaceId && string.Equals(workspace.Name.Trim(), name?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Marks every notification of the signed-in account as read (the header's "Mark all as read").</summary>
    public void MarkAllNotificationsRead() => MarkNoticesRead();

    /// <summary>Marks one notification of the signed-in account as read; ignores an unknown ID or another account's notification.</summary>
    public void MarkNotificationRead(int id)
    {
        if (Identity.Current is not { } account || notices.FindIndex(notice => notice.Id == id && notice.AccountId == account.Id) is not (var index and >= 0) || notices[index].IsRead)
            return;

        notices[index] = notices[index] with { IsRead = true };
        Changed?.Invoke();
    }

    /// <summary>Returns each workspace's owner shown in lists (an active owner first), by workspace ID, in one pass.</summary>
    public IReadOnlyDictionary<int, SaasMember> OwnersByWorkspace() =>
        members.Where(member => member.Role == SaasMemberRole.Owner)
            .GroupBy(member => member.WorkspaceId)
            .ToDictionary(group => group.Key, group => group.OrderBy(member => member.Status).ThenBy(member => member.Name, StringComparer.OrdinalIgnoreCase).First());

    /// <summary>
    /// The header's global search, best matches first (exactly the term, then starting with it, then containing it), and
    /// among equal matches by kind. Operators find tenants (by name, subdomain, or owner name and email), leads (by name,
    /// company, or email), invoices (by number, when the term has a digit), and plans; customers find their own team (by
    /// name or email) and their own invoices. Returns nothing for a blank term or when nobody is signed in.
    /// </summary>
    public IReadOnlyList<ShowcaseSearchResult> Search(string term, int maxResults)
    {
        term = term?.Trim() ?? string.Empty;

        if (term.Length == 0 || maxResults <= 0 || Identity.Current is null)
            return Array.Empty<ShowcaseSearchResult>();

        // 0: exactly the term, 1: starts with it, 2: contains it; null: no match.
        int? Rank(params string?[] values) => values
            .Select(value => value is null ? (int?)null
                : string.Equals(value, term, StringComparison.OrdinalIgnoreCase) ? 0
                : value.StartsWith(term, StringComparison.OrdinalIgnoreCase) ? 1
                : value.Contains(term, StringComparison.OrdinalIgnoreCase) ? 2
                : null)
            .Where(rank => rank is not null)
            .DefaultIfEmpty(null)
            .Min();

        var hasDigit = term.Any(char.IsAsciiDigit);
        var matches = new List<(int Rank, int Kind, long Order, ShowcaseSearchResult Result)>();

        if (IsOperator)
        {
            var owners = OwnersByWorkspace();

            // Kind: 0 a tenant, 1 a lead, 2 an invoice, 3 a plan.
            foreach (var workspace in workspaces)
            {
                var owner = owners.GetValueOrDefault(workspace.Id);

                if (Rank(workspace.Name, workspace.Subdomain, owner?.Name, owner?.Email) is { } found)
                {
                    matches.Add((found, 0, -workspace.CreatedOn.Ticks, new ShowcaseSearchResult("Tenant", workspace.Name,
                        $"{PlanOf(workspace).Name} · {SaasDisplayStatus(workspace.Status)} · {workspace.Subdomain} · owner {owner?.Name ?? "none"}", TenantPath(workspace.Id))));
                }
            }

            foreach (var lead in leads)
            {
                if (Rank(lead.Name, lead.Company, lead.Email) is { } found)
                    matches.Add((found, 1, -lead.CreatedOn.Ticks, new ShowcaseSearchResult("Lead", $"{lead.Company} ({lead.Name})", $"{lead.Status} · {lead.TeamSize} people · {lead.Email}", LeadPath(lead.Id))));
            }

            if (hasDigit)
            {
                var names = workspaces.ToDictionary(workspace => workspace.Id, workspace => workspace.Name);

                foreach (var invoice in invoices)
                {
                    if (Rank(invoice.Number) is { } found)
                    {
                        matches.Add((found, 2, -invoice.IssuedOn.Ticks, new ShowcaseSearchResult("Invoice", invoice.Number,
                            $"{names.GetValueOrDefault(invoice.WorkspaceId) ?? "Unknown tenant"} · {Money(invoice.Amount)} · {InvoiceStatusWord(invoice)} · {invoice.IssuedOn.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)}",
                            InvoicePath(invoice.Number))));
                    }
                }
            }

            foreach (var plan in plans)
            {
                if (Rank(plan.Name) is { } found)
                    matches.Add((found, 3, 0, new ShowcaseSearchResult("Plan", plan.Name, plan.MonthlyPricePerSeat == 0 ? "Free" : $"{Money(plan.MonthlyPricePerSeat)} per seat a month", PlansPath)));
            }
        }
        else if (CurrentWorkspace is { } workspace)
        {
            // Kind: 0 someone in the team, 1 an invoice.
            foreach (var member in members.Where(member => member.WorkspaceId == workspace.Id))
            {
                if (Rank(member.Name, member.Email) is { } found)
                {
                    matches.Add((found, 0, 0, new ShowcaseSearchResult("Team", member.Name,
                        $"{member.Role} · {(member.Status == SaasMemberStatus.Invited ? "invited" : "active")} · {member.Email}", AppPath + "/team")));
                }
            }

            if (hasDigit)
            {
                foreach (var invoice in invoices.Where(invoice => invoice.WorkspaceId == workspace.Id))
                {
                    if (Rank(invoice.Number) is { } found)
                    {
                        matches.Add((found, 1, -invoice.IssuedOn.Ticks, new ShowcaseSearchResult("Invoice", invoice.Number,
                            $"{invoice.IssuedOn.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)} · {Money(invoice.Amount)} · {InvoiceStatusWord(invoice)}", AppInvoicePath(invoice.Number))));
                    }
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

    /// <summary>Returns the number of members and open invitations of each workspace, by workspace ID, in one pass.</summary>
    public IReadOnlyDictionary<int, int> SeatsUsedByWorkspace() =>
        members.GroupBy(member => member.WorkspaceId).ToDictionary(group => group.Key, group => group.Count());

    #endregion

    #region Private Methods

    /// <summary>The customer's page of one of their invoices.</summary>
    private static string AppInvoicePath(string number) => $"{BillingPath}/invoices/{number}";

    /// <summary>Shortens a text to the audit log's limit.</summary>
    private static string ClipAudit(string text) => text.Length <= MaxAuditTextLength ? text : string.Concat(text.AsSpan(0, MaxAuditTextLength - 1), "…");

    /// <summary>A message's first line, at most 120 characters, for a notification.</summary>
    private static string FirstLine(string message)
    {
        var line = message.Split('\n', 2)[0].Trim();

        return line.Length <= 120 ? line : string.Concat(line.AsSpan(0, 119), "…");
    }

    private static string InvoicePath(string number) => $"{AdminPath}/invoices/{number}";

    private static string InvoiceStatusWord(SaasInvoice invoice) => invoice.Status switch
    {
        SaasInvoiceStatus.Open => "open",
        SaasInvoiceStatus.Refunded => "refunded",
        _ when invoice.RefundedAmount > 0 => "partially refunded",
        _ => "paid",
    };

    private static string InvoiceTarget(string number) => $"Invoice {number}";

    /// <summary>Checks a stored audit or notification link: none, or a path on this site without spaces, quotes, or a second slash.</summary>
    private static bool IsSiteLink(string? url) =>
        url is null || (url.Length <= 200 && url.StartsWith(SiteBase + "/", StringComparison.Ordinal) && !url.Contains("//", StringComparison.Ordinal)
            && url.All(character => !char.IsWhiteSpace(character) && !char.IsControl(character) && character is not ('"' or '\'' or '<' or '>' or '\\')));

    private static string LeadPath(int leadId) => string.Create(CultureInfo.InvariantCulture, $"{LeadsPath}?lead={leadId}");

    private static string LeadTarget(SaasLead lead) => $"Lead: {lead.Company}";

    private static string PlanTarget(string name) => $"Plan: {name}";

    /// <summary>A status in words, as the console shows it ("Past due", "Archived").</summary>
    private static string SaasDisplayStatus(SaasWorkspaceStatus status) => status == SaasWorkspaceStatus.PastDue ? "Past due" : status.ToString();

    private static string TenantPath(int workspaceId) => string.Create(CultureInfo.InvariantCulture, $"{AdminPath}/tenants/{workspaceId}");

    /// <summary>A member as the audit log names them: the person and their workspace.</summary>
    private string MemberTarget(SaasMember member) => $"{member.Name} ({FindWorkspace(member.WorkspaceId)?.Name ?? "a removed workspace"})";

    /// <summary>
    /// Raises a notification for an account that can sign in (rule WR-001); nothing for a roster person or teammate without a
    /// sign-in, and nothing for the person who caused it, unless the system did (<paramref name="fromSystem" />). Each account
    /// keeps its newest 50.
    /// </summary>
    private void Notify(string? accountId, string title, string text, string? link, IconName iconName, bool fromSystem = false)
    {
        if (accountId is null || Identity.FindAccount(accountId) is null || (!fromSystem && accountId == Identity.Current?.Id))
            return;

        AddNotice(accountId, title, text, link, iconName);
    }

    /// <summary>Tells an account manager who can sign in that they now look after a tenant, unless they assigned it to themselves.</summary>
    private void NotifyManager(SaasWorkspace workspace, string managerId) =>
        Notify(managerId, $"You're the account manager of {workspace.Name}",
            string.Create(CultureInfo.InvariantCulture, $"{PlanOf(workspace).Name}, {workspace.Seats} seats, {SaasDisplayStatus(workspace.Status).ToLowerInvariant()}."), TenantPath(workspace.Id),
            IconName.PersonCheck);

    /// <summary>Tells every operator who can sign in (rule WR-001): a new sign-up or lead, an overdue invoice, a tenant out of seats.</summary>
    private void NotifyOperators(string title, string text, string? link, IconName iconName, bool fromSystem = false)
    {
        foreach (var account in Identity.Accounts.Where(account => account.RoleId == SaasSeedData.OperatorRole).ToList())
            Notify(account.Id, title, text, link, iconName, fromSystem);
    }

    /// <summary>
    /// A workspace with an operator's plan, seats, status, and account manager. A free plan bills monthly, a trial keeps its
    /// end date (or starts one), and a trial renews when it ends.
    /// </summary>
    private SaasWorkspace OperatorChange(SaasWorkspace workspace, SaasPlan plan, int seats, SaasWorkspaceStatus status, string? managerId)
    {
        var updated = workspace with
        {
            PlanId = plan.Id,
            Period = plan.MonthlyPricePerSeat == 0 ? SaasBillingPeriod.Monthly : workspace.Period,
            Seats = seats,
            Status = status,
            TrialEndsOn = status == SaasWorkspaceStatus.Trial ? workspace.TrialEndsOn ?? Clock.Today.AddDays(SaasSeedData.TrialDays) : null,
            ManagerId = managerId,
        };

        // A trial bills when it ends, so the renewal date follows the trial's end.
        return updated.TrialEndsOn is { } trialEndsOn ? updated with { RenewsOn = trialEndsOn } : updated;
    }

    /// <summary>
    /// Adds an audit entry (rule WR-001): the signed-in person did <paramref name="action" /> (past tense) to
    /// <paramref name="target" />, now, on the site's clock; automatic billing when nobody is signed in.
    /// </summary>
    private void Record(string action, string target, string? targetUrl, string? details = null) =>
        Record(Clock.Now, Identity.Current?.Name ?? SaasSeedData.SystemActorName, action, target, targetUrl, details);

    /// <summary>Adds an audit entry for <paramref name="actor" /> at <paramref name="at" />: automatic billing or a visitor act on their own. The log keeps the newest entries.</summary>
    private void Record(DateTime at, string actor, string action, string target, string? targetUrl, string? details)
    {
        if (nextAuditId >= 1_000_000)
            return;

        audit.Add(new ShowcaseAuditEntry
        {
            Id = nextAuditId++,
            At = at,
            Actor = Trim(string.IsNullOrWhiteSpace(actor) ? SaasSeedData.SystemActorName : actor, ShowcaseIdentity.MaxNameLength),
            Action = ClipAudit(action),
            Target = ClipAudit(target),
            TargetUrl = IsSiteLink(targetUrl) ? targetUrl : null,
            Details = string.IsNullOrWhiteSpace(details) ? null : ClipAudit(details),
        });

        if (audit.Count > MaxAudit)
            audit.RemoveAt(0);
    }

    /// <summary>
    /// Tells the people concerned about an operator's change to a tenant: its owners about a suspension or a return to active,
    /// its owners and billing members about a plan change, and a newly assigned account manager.
    /// </summary>
    private void TellAboutOperatorChange(SaasWorkspace before, SaasWorkspace after)
    {
        if (NotifiesOwner(before.Status, after.Status))
        {
            var suspended = after.Status == SaasWorkspaceStatus.Suspended;

            foreach (var owner in OthersToTell(after.Id, member => member.Role == SaasMemberRole.Owner))
            {
                Notify(owner, suspended ? "Your workspace is suspended" : "Your workspace is active again",
                    suspended ? $"{after.Name} was suspended by Lumora. Contact support to restore it." : $"{after.Name} can be used again.",
                    AppPath, suspended ? IconName.SlashCircle : IconName.CheckCircle);
            }
        }

        if (before.PlanId != after.PlanId)
        {
            foreach (var accountId in OthersToTell(after.Id, member => member.Role is SaasMemberRole.Owner or SaasMemberRole.Billing))
            {
                Notify(accountId, "Lumora changed your plan", $"{after.Name} is now on {PlanOf(after).Name} ({PlanOf(before).Name} before). The new price applies from your next invoice.",
                    BillingPath, IconName.ArrowUpCircle);
            }
        }

        if (after.ManagerId is { } managerId && managerId != before.ManagerId)
            NotifyManager(after, managerId);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the audit log, newest first, for operators; empty for anyone else (customers see their workspace's activity instead).</summary>
    public IReadOnlyList<ShowcaseAuditEntry> AuditEntries =>
        IsOperator ? audit.OrderByDescending(entry => entry.At).ThenByDescending(entry => entry.Id).ToList() : Array.Empty<ShowcaseAuditEntry>();

    #endregion
}
