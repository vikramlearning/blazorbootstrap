using System.Globalization;
using System.Text.Json;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

/// <summary>
/// State of the "Beacon Desk" help desk for the current browser tab, shared by the help center (customers), the agent
/// desk, and its admin modules: tickets with their threads and activity, saved replies, knowledge base articles and votes,
/// the roster and teams, SLA policies, categories, and the demo sign-in. It is kept in the tab's <c>sessionStorage</c>
/// through the showcase foundation, and its clock moves on from the seed time (D-029), so SLA countdowns run; nothing is sent.
/// It also keeps the enterprise baseline (rule WR-001): an audit log of every create, change, and delete, in-app
/// notifications for the staff accounts, and the header's global search.
/// </summary>
public sealed class HelpDeskState : IShowcaseEnterpriseState
{
    #region Fields and Constants

    /// <summary>The longest article body, in editor HTML.</summary>
    public const int MaxArticleHtmlLength = 20_000;

    /// <summary>The admin's agents and teams page, where audit entries about agents and teams link to.</summary>
    private const string AdminAgentsLink = AgentBase + "/admin/agents";

    /// <summary>The admin's categories page.</summary>
    private const string AdminCategoriesLink = AgentBase + "/admin/categories";

    /// <summary>The admin's SLA policies page.</summary>
    private const string AdminSlaLink = AgentBase + "/admin/sla";

    /// <summary>The agent desk's base route (the same as <c>HelpDeskAgentLayout.Base</c>).</summary>
    private const string AgentBase = SiteBase + "/agent";

    /// <summary>The agents' saved replies page.</summary>
    private const string SavedRepliesLink = AgentBase + "/saved-replies";

    /// <summary>The site's base route (the same as <c>HelpDeskLayout.Base</c>); audit and notification links stay under it.</summary>
    private const string SiteBase = DemoRouteConstants.Showcase_URL_HelpDesk;

    /// <summary>The longest article summary.</summary>
    public const int MaxArticleSummaryLength = 200;

    /// <summary>The longest article title.</summary>
    public const int MaxArticleTitleLength = 120;

    /// <summary>The longest category or team name.</summary>
    public const int MaxGroupNameLength = 40;

    /// <summary>The longest resolution target, in hours (30 days).</summary>
    public const int MaxResolveHours = 720;

    /// <summary>The longest first-reply target, in hours (one week).</summary>
    public const int MaxFirstReplyHours = 168;

    /// <summary>The longest team description.</summary>
    public const int MaxTeamDescriptionLength = 120;

    /// <summary>The longest customer message.</summary>
    public const int MaxCustomerMessageLength = 4000;

    /// <summary>The longest ticket subject.</summary>
    public const int MaxSubjectLength = 120;

    private const int MaxActivityPerTicket = 300;

    /// <summary>The name escalations are recorded under in ticket history and the audit log: the rules act, not a person.</summary>
    public const string EscalationActor = "Escalation rules";

    /// <summary>The most escalation rules a help desk keeps.</summary>
    public const int MaxEscalationRules = 20;

    /// <summary>The highest escalation level: Tier 3.</summary>
    public const int MaxEscalationLevel = 2;

    /// <summary>The longest escalation rule name.</summary>
    public const int MaxRuleNameLength = 60;

    /// <summary>The most audit entries kept; the oldest go first.</summary>
    private const int MaxAuditEntries = 500;

    /// <summary>The longest audit or notification text (action, record, details, title).</summary>
    private const int MaxAuditTextLength = 200;

    /// <summary>The most notifications kept, for every account together; the oldest go first.</summary>
    private const int MaxNotifications = 200;

    /// <summary>The most agents on the roster, inactive ones included.</summary>
    public const int MaxAgents = 50;

    /// <summary>The most knowledge base articles, drafts included.</summary>
    public const int MaxArticles = 200;

    /// <summary>The most categories.</summary>
    public const int MaxCategories = 30;

    /// <summary>The most teams.</summary>
    public const int MaxTeams = 20;

    private const int MaxEditorHtmlLength = 20_000;

    private const int MaxMessagesPerTicket = 200;

    private const double MaxPausedMinutes = 4 * 365 * 24 * 60;

    private const int MaxSavedReplies = 100;

    private const int MaxTickets = 1000;

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<HelpDeskAgent> agents = HelpDeskSeedData.Agents();

    private readonly List<HelpDeskArticle> articles = HelpDeskSeedData.Articles();

    private readonly List<ShowcaseAuditEntry> audit = new();

    private readonly List<HelpDeskCategory> categories = HelpDeskSeedData.Categories();

    private readonly List<HelpDeskEscalationRule> escalationRules = HelpDeskSeedData.EscalationRules();

    private readonly List<ShowcaseNotification> notifications = new();

    private readonly List<HelpDeskSavedReply> savedReplies = HelpDeskSeedData.SavedReplies();

    private readonly List<HelpDeskSlaPolicy> slaPolicies = HelpDeskSeedData.SlaPolicies();

    private readonly List<HelpDeskTeam> teams = HelpDeskSeedData.Teams();

    private readonly Dictionary<int, (int Helpful, int NotHelpful)> votes = new();

    private int nextAgentId = 1;

    private int nextArticleId;

    private int nextAuditId;

    private int nextCategoryId = 1;

    private int nextEscalationRuleId;

    private int nextNotificationId;

    private int nextSavedReplyId;

    private int nextTeamId = 1;

    private int nextTicketId;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out.</summary>
    public HelpDeskState()
    {
        nextTicketId = Tickets.Max(ticket => ticket.Id) + 1;
        nextSavedReplyId = savedReplies.Max(reply => reply.Id) + 1;
        nextArticleId = articles.Max(article => article.Id) + 1;
        audit.AddRange(HelpDeskSeedData.AuditEntries(Tickets, articles, agents));
        notifications.AddRange(HelpDeskSeedData.Notifications(Tickets, agents));
        nextAuditId = audit.Count == 0 ? 1 : audit.Max(entry => entry.Id) + 1;
        nextNotificationId = notifications.Count == 0 ? 1 : notifications.Max(notification => notification.Id) + 1;
        nextEscalationRuleId = escalationRules.Count == 0 ? 1 : escalationRules.Max(rule => rule.Id) + 1;
        Identity = new ShowcaseIdentity(HelpDeskSeedData.Roles(), HelpDeskSeedData.DemoAccounts());
        Identity.Changed += () => Changed?.Invoke();

        // The seeded rules have been running all along: the tickets they caught are escalated at the time each rule fired.
        Escalate(seeding: true);
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Returns when a ticket's resolution SLA is due: its creation plus the resolution target of its priority's policy, plus
    /// the time it has waited on the customer (Pending pauses the SLA, T28-F1).
    /// </summary>
    public DateTime DueAt(HelpDeskTicket ticket)
    {
        var paused = ticket.PausedMinutes;

        if (ticket.Status == HelpDeskStatus.Pending && ticket.PendingSince is { } since && Clock.Now > since)
            paused += (Clock.Now - since).TotalMinutes;

        return ticket.CreatedAt.AddHours(TargetHours(ticket.Priority)).AddMinutes(paused);
    }

    /// <summary>Returns when a ticket's first public reply is due under its priority's policy.</summary>
    public DateTime FirstReplyDueAt(HelpDeskTicket ticket) => ticket.CreatedAt.AddHours(Policy(ticket.Priority).FirstReplyHours);

    /// <summary>Returns the SLA policy of a priority.</summary>
    public HelpDeskSlaPolicy Policy(HelpDeskPriority priority) => slaPolicies.FirstOrDefault(policy => policy.Priority == priority) ?? slaPolicies[0];

    /// <summary>Returns the hours to resolution for a priority, from its SLA policy.</summary>
    public int TargetHours(HelpDeskPriority priority) => Policy(priority).ResolveHours;

    /// <summary>Posts a reply or an internal note as the signed-in agent. A public reply puts a new or open ticket on Pending and flags it for the customer.</summary>
    public bool AddReply(HelpDeskTicket ticket, string html, bool isInternalNote)
    {
        if (!IsAgent || string.IsNullOrWhiteSpace(html) || html.Length > MaxEditorHtmlLength || ticket.Messages.Count >= MaxMessagesPerTicket)
            return false;

        var before = TicketFacts(ticket);
        ticket.Messages.Add(new HelpDeskMessage(AgentName, IsAgent: true, isInternalNote, IsRichText: true, html, NextTime(ticket)));

        if (ticket.Assignee is null)
        {
            ticket.Assignee = AgentName;
            Log(ticket, $"Assigned to {AgentName}");
        }

        if (!isInternalNote)
        {
            ticket.CustomerUnread = true;

            if (ticket.Status is HelpDeskStatus.New or HelpDeskStatus.Open)
            {
                ApplyStatus(ticket, HelpDeskStatus.Pending);
                Log(ticket, "Replied; status set to Pending until the customer answers");
            }
        }

        Record(isInternalNote ? "Added a note" : "Replied", TicketTarget(ticket.Id), TicketLink(ticket.Id), TicketChanges(before, ticket));
        Changed?.Invoke();

        return true;
    }

    /// <summary>Assigns a ticket to an agent, or unassigns it; assigning a new ticket opens it.</summary>
    public bool Assign(HelpDeskTicket ticket, string? agent)
    {
        if (!IsAgent || (agent is not null && !Agents.Contains(agent)) || ticket.Assignee == agent)
            return false;

        var before = TicketFacts(ticket);
        ticket.Assignee = agent;
        Log(ticket, agent is null ? "Unassigned" : $"Assigned to {agent}");

        if (agent is not null && ticket.Status == HelpDeskStatus.New)
            SetStatusCore(ticket, HelpDeskStatus.Open);

        Record(agent is null ? "Unassigned" : "Assigned", TicketTarget(ticket.Id), TicketLink(ticket.Id), TicketChanges(before, ticket));

        if (agent is not null)
            Notify(FindAgentByName(agent)?.AccountId, FormattableString.Invariant($"Ticket #{ticket.Id} was assigned to you"), ticket.Subject, TicketLink(ticket.Id));

        Changed?.Invoke();

        return true;
    }

    /// <summary>
    /// Applies the same change to several tickets (the agent queue's bulk actions); a <see langword="null" /> value leaves that
    /// field as it is. Returns how many tickets changed.
    /// </summary>
    public int BulkUpdate(IEnumerable<int> ticketIds, string? assignee, bool unassign, HelpDeskStatus? status, HelpDeskPriority? priority)
    {
        if (!IsAgent || (assignee is not null && !Agents.Contains(assignee)) || (status is { } s && !Enum.IsDefined(s)) || (priority is { } p && !Enum.IsDefined(p)))
            return 0;

        var changed = 0;
        var newlyAssigned = new List<HelpDeskTicket>();

        foreach (var ticket in ticketIds.Distinct().Select(FindTicket).OfType<HelpDeskTicket>())
        {
            var before = TicketFacts(ticket);

            if (unassign && ticket.Assignee is not null)
            {
                ticket.Assignee = null;
                Log(ticket, "Unassigned (bulk)");
            }
            else if (assignee is not null && ticket.Assignee != assignee)
            {
                ticket.Assignee = assignee;
                Log(ticket, $"Assigned to {assignee} (bulk)");
                newlyAssigned.Add(ticket);
            }

            if (priority is { } newPriority && ticket.Priority != newPriority)
            {
                Log(ticket, $"Priority {ticket.Priority} → {newPriority} (bulk)");
                ticket.Priority = newPriority;
            }

            if (status is { } newStatus && ticket.Status != newStatus)
                SetStatusCore(ticket, newStatus, " (bulk)");
            else if (status is null && assignee is not null && ticket.Status == HelpDeskStatus.New)
                SetStatusCore(ticket, HelpDeskStatus.Open, " (bulk)");

            if (TicketChanges(before, ticket) is { } changes)
            {
                changed++;
                Record("Bulk updated", TicketTarget(ticket.Id), TicketLink(ticket.Id), changes);
            }
        }

        // One notification for the new assignee, however many tickets they got.
        if (newlyAssigned.Count > 0 && FindAgentByName(assignee)?.AccountId is { } accountId)
        {
            var first = newlyAssigned[0];

            if (newlyAssigned.Count == 1)
                Notify(accountId, FormattableString.Invariant($"Ticket #{first.Id} was assigned to you"), first.Subject, TicketLink(first.Id));
            else
                Notify(accountId, FormattableString.Invariant($"{newlyAssigned.Count} tickets were assigned to you"),
                    string.Join(", ", newlyAssigned.Take(5).Select(ticket => FormattableString.Invariant($"#{ticket.Id}"))) + (newlyAssigned.Count > 5 ? ", …" : string.Empty),
                    AgentBase);
        }

        if (changed > 0)
            Changed?.Invoke();

        return changed;
    }

    /// <summary>Returns the snapshot of the help desk: tickets, saved replies, articles and votes, the admin settings, the audit log and notifications, and the ID counters.</summary>
    public object CaptureData() => new HelpDeskSnapshot
    {
        Agents = agents.ToList(),
        Articles = articles.ToList(),
        Audit = audit.ToList(),
        Categories = categories.ToList(),
        EscalationRules = escalationRules.ToList(),
        NextAgentId = nextAgentId,
        NextEscalationRuleId = nextEscalationRuleId,
        NextArticleId = nextArticleId,
        NextAuditId = nextAuditId,
        NextCategoryId = nextCategoryId,
        NextNotificationId = nextNotificationId,
        NextSavedReplyId = nextSavedReplyId,
        NextTeamId = nextTeamId,
        NextTicketId = nextTicketId,
        Notifications = notifications.ToList(),
        SavedReplies = savedReplies.ToList(),
        SlaPolicies = slaPolicies.ToList(),
        Teams = teams.ToList(),
        Tickets = Tickets.ToList(),
        Votes = votes.Select(pair => new HelpDeskArticleVotes(pair.Key, pair.Value.Helpful, pair.Value.NotHelpful)).ToList(),
    };

    /// <summary>
    /// Opens a ticket on a customer's behalf (the agent's "New ticket", for a call or an email). A ticket from the email of a
    /// customer account belongs to that account, so it shows in their requests.
    /// </summary>
    public HelpDeskTicket? Create(string customerName, string email, string? company, string categoryId, HelpDeskPriority priority, string subject, string description)
    {
        if (!IsAgent || !ValidNewTicket(customerName, email, company, subject, description) || !Enum.IsDefined(priority))
            return null;

        var account = Identity.Accounts.FirstOrDefault(candidate => candidate.RoleId == HelpDeskSeedData.CustomerRole
            && string.Equals(candidate.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));
        var ticket = NewTicket(customerName, email, company, categoryId, priority, subject, description, account?.Id, $"Opened by {AgentName} for the customer");
        Record("Created", TicketTarget(ticket.Id), TicketLink(ticket.Id), $"For {ticket.CustomerName}, {FindCategory(ticket.CategoryId).Name}, {ticket.Priority} priority");
        Changed?.Invoke();

        return ticket;
    }

    /// <summary>Deletes an article (agents), with its votes.</summary>
    public HelpDeskAdminResult DeleteArticle(int articleId)
    {
        if (!IsAgent)
            return HelpDeskAdminResult.NotAllowed;

        if (articles.FirstOrDefault(article => article.Id == articleId) is not { } removed)
            return HelpDeskAdminResult.NotFound;

        articles.Remove(removed);
        votes.Remove(articleId);
        Record("Deleted", $"Article: {removed.Title}", null, removed.IsPublished ? "Was published" : "Was a draft");
        Changed?.Invoke();

        return HelpDeskAdminResult.Ok;
    }

    /// <summary>
    /// Deletes a category (admins), moving its tickets and articles to <paramref name="moveToId" /> first; the last category
    /// can't be deleted. <paramref name="moved" /> is how many tickets and articles moved.
    /// </summary>
    public HelpDeskAdminResult DeleteCategory(string categoryId, string moveToId, out int moved)
    {
        moved = 0;

        if (!IsAdmin)
            return HelpDeskAdminResult.NotAllowed;

        if (categories.FirstOrDefault(category => category.Id == categoryId) is not { } removed)
            return HelpDeskAdminResult.NotFound;

        if (categories.Count == 1)
            return HelpDeskAdminResult.InUse;

        if (moveToId == categoryId || categories.FirstOrDefault(category => category.Id == moveToId) is not { } target)
            return HelpDeskAdminResult.Invalid;

        foreach (var ticket in Tickets.Where(ticket => ticket.CategoryId == categoryId))
        {
            Log(ticket, $"Category {removed.Name} → {target.Name} (category deleted)");
            ticket.CategoryId = target.Id;
            moved++;
        }

        for (var index = 0; index < articles.Count; index++)
        {
            if (articles[index].CategoryId == categoryId)
            {
                articles[index] = articles[index] with { CategoryId = target.Id };
                moved++;
            }
        }

        categories.Remove(removed);
        Record("Deleted", $"Category: {removed.Name}", null,
            moved == 0 ? null : string.Create(CultureInfo.InvariantCulture, $"Moved {moved} {(moved == 1 ? "ticket or article" : "tickets and articles")} to {target.Name}"));
        Changed?.Invoke();

        return HelpDeskAdminResult.Ok;
    }

    /// <summary>Deletes a team (admins); a team that still has agents or routes a category is in use, and the last team stays.</summary>
    public HelpDeskAdminResult DeleteTeam(string teamId)
    {
        if (!IsAdmin)
            return HelpDeskAdminResult.NotAllowed;

        if (teams.FirstOrDefault(team => team.Id == teamId) is not { } removed)
            return HelpDeskAdminResult.NotFound;

        if (teams.Count == 1 || agents.Any(agent => agent.TeamId == teamId) || categories.Any(category => category.TeamId == teamId))
            return HelpDeskAdminResult.InUse;

        teams.Remove(removed);

        if (Filter.TeamId == teamId)
            Filter.TeamId = string.Empty;

        Record("Deleted", $"Team: {removed.Name}", null);
        Changed?.Invoke();

        return HelpDeskAdminResult.Ok;
    }

    /// <summary>Deletes a saved reply (agents).</summary>
    public bool DeleteSavedReply(int replyId)
    {
        if (!IsAgent || savedReplies.FirstOrDefault(reply => reply.Id == replyId) is not { } removed)
            return false;

        savedReplies.Remove(removed);
        Record("Deleted", $"Saved reply: {removed.Title}", null);
        Changed?.Invoke();

        return true;
    }

    /// <summary>Returns a roster agent by ID, or <see langword="null" />.</summary>
    public HelpDeskAgent? FindAgent(string? agentId) => agents.FirstOrDefault(agent => agent.Id == agentId);

    /// <summary>Returns the roster agent with a name (tickets are assigned by name), or <see langword="null" />.</summary>
    public HelpDeskAgent? FindAgentByName(string? name) => name is null ? null : agents.FirstOrDefault(agent => agent.Name == name);

    /// <summary>Returns a published article by its ID, or <see langword="null" /> (drafts are not public).</summary>
    public HelpDeskArticle? FindArticle(int articleId) => articles.FirstOrDefault(article => article.Id == articleId && article.IsPublished);

    /// <summary>Returns any article, draft or published, for an agent to edit; <see langword="null" /> for anyone else.</summary>
    public HelpDeskArticle? FindArticleForEdit(int articleId) => IsAgent ? articles.FirstOrDefault(article => article.Id == articleId) : null;

    /// <summary>Returns a category by its ID, or the first category.</summary>
    public HelpDeskCategory FindCategory(string? categoryId) => categories.FirstOrDefault(category => category.Id == categoryId) ?? categories[0];

    /// <summary>Returns a team by its ID, or <see langword="null" />.</summary>
    public HelpDeskTeam? FindTeam(string? teamId) => teams.FirstOrDefault(team => team.Id == teamId);

    /// <summary>
    /// Returns one of the signed-in customer's own requests and marks an agent's reply as read; <see langword="null" /> for
    /// someone else's ticket.
    /// </summary>
    public HelpDeskTicket? FindOwnRequest(int ticketId)
    {
        if (OwnTicket(ticketId) is not { } ticket)
            return null;

        if (ticket.CustomerUnread)
        {
            ticket.CustomerUnread = false;
            Changed?.Invoke();
        }

        return ticket;
    }

    /// <summary>Returns a ticket by its number for an agent, or <see langword="null" />.</summary>
    public HelpDeskTicket? FindTicket(int ticketId) => IsAgent ? Tickets.FirstOrDefault(ticket => ticket.Id == ticketId) : null;

    /// <summary>Returns the signed-in account's notifications, newest first; empty when nobody is signed in.</summary>
    public IReadOnlyList<ShowcaseNotification> GetNotificationsForCurrentAccount() =>
        Identity.Current is { } account
            ? notifications.Where(notification => notification.AccountId == account.Id).OrderByDescending(notification => notification.At).ThenByDescending(notification => notification.Id).ToList()
            : Array.Empty<ShowcaseNotification>();

    /// <summary>Marks every notification of the signed-in account as read.</summary>
    public void MarkAllNotificationsRead()
    {
        if (Identity.Current is not { } account)
            return;

        var unread = notifications.Where(notification => notification.AccountId == account.Id && !notification.IsRead).ToList();

        if (unread.Count == 0)
            return;

        foreach (var notification in unread)
            notification.IsRead = true;

        Changed?.Invoke();
    }

    /// <summary>Marks one notification of the signed-in account as read; ignores an unknown ID or another account's notification.</summary>
    public void MarkNotificationRead(int id)
    {
        if (Identity.Current is not { } account
            || notifications.FirstOrDefault(notification => notification.Id == id && notification.AccountId == account.Id) is not { IsRead: false } notification)
            return;

        notification.IsRead = true;
        Changed?.Invoke();
    }

    /// <summary>Marks one of the signed-in customer's requests as solved.</summary>
    public bool MarkSolved(int ticketId)
    {
        if (OwnTicket(ticketId) is not { } ticket || ticket.Status == HelpDeskStatus.Resolved)
            return false;

        var before = TicketFacts(ticket);
        ApplyStatus(ticket, HelpDeskStatus.Resolved);
        Log(ticket, "Marked as solved by the customer");
        Record("Marked solved", TicketTarget(ticket.Id), TicketLink(ticket.Id), TicketChanges(before, ticket));
        Changed?.Invoke();

        return true;
    }

    /// <summary>Returns the signed-in customer's requests, the most recently updated first.</summary>
    public IReadOnlyList<HelpDeskTicket> MyRequests() =>
        Identity.Current is { RoleId: HelpDeskSeedData.CustomerRole } account
            ? Tickets.Where(ticket => ticket.CustomerAccountId == account.Id).OrderByDescending(ticket => ticket.CustomerUpdatedAt).ToList()
            : Array.Empty<HelpDeskTicket>();

    /// <summary>Runs the escalation rules on the restored tickets (time moved on while the tab was away), then raises <see cref="Changed" />.</summary>
    public void NotifyRestored()
    {
        Escalate(seeding: false);
        Changed?.Invoke();
    }

    /// <summary>Returns the agent queue for the current filter: breached first, then at risk, then by due time.</summary>
    public IReadOnlyList<HelpDeskTicket> Queue()
    {
        var search = Filter.Search.Trim().TrimStart('#');

        return Tickets
            .Where(ticket => Filter.Status switch
            {
                HelpDeskFilter.StatusAll => true,
                HelpDeskFilter.StatusAllOpen => ticket.Status != HelpDeskStatus.Resolved,
                var status => ticket.Status.ToString() == status,
            })
            .Where(ticket => Filter.Priority.Length == 0 || ticket.Priority.ToString() == Filter.Priority)
            .Where(ticket => !Filter.AssignedToMe || ticket.Assignee == AgentName)
            .Where(ticket => !Filter.EscalatedOnly || ticket.IsEscalated)
            .Where(ticket => Filter.TeamId.Length == 0 || TeamOf(ticket)?.Id == Filter.TeamId)
            .Where(ticket => search.Length == 0
                || ticket.Id.ToString(CultureInfo.InvariantCulture).Contains(search, StringComparison.Ordinal)
                || ticket.Subject.Contains(search, StringComparison.OrdinalIgnoreCase)
                || ticket.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || (ticket.Company?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderBy(ticket => SlaRank(Sla(ticket)))
            .ThenBy(DueAt)
            .ToList();
    }

    /// <summary>Rates one of the signed-in customer's resolved requests, with an optional comment (up to 500 characters).</summary>
    public bool Rate(int ticketId, HelpDeskSatisfaction satisfaction, string? comment)
    {
        if (OwnTicket(ticketId) is not { Status: HelpDeskStatus.Resolved } ticket || !Enum.IsDefined(satisfaction))
            return false;

        comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();

        if (comment is { Length: > 500 })
            return false;

        ticket.Satisfaction = satisfaction;
        ticket.SatisfactionComment = comment;
        Log(ticket, $"Rated {(satisfaction == HelpDeskSatisfaction.Good ? "good" : "bad")} by the customer");
        Record("Rated", TicketTarget(ticket.Id), TicketLink(ticket.Id), satisfaction == HelpDeskSatisfaction.Good ? "Good" : "Bad");
        Changed?.Invoke();

        return true;
    }

    /// <summary>Returns the articles in a category, except one.</summary>
    public IReadOnlyList<HelpDeskArticle> RelatedArticles(string categoryId, int? exceptId = null) =>
        Articles.Where(article => article.CategoryId == categoryId && article.Id != exceptId).ToList();

    /// <summary>Reopens a resolved ticket (agents).</summary>
    public bool Reopen(HelpDeskTicket ticket)
    {
        if (!IsAgent || ticket.Status != HelpDeskStatus.Resolved)
            return false;

        var before = TicketFacts(ticket);
        SetStatusCore(ticket, HelpDeskStatus.Open);
        Record("Reopened", TicketTarget(ticket.Id), TicketLink(ticket.Id), TicketChanges(before, ticket));
        Changed?.Invoke();

        return true;
    }

    /// <summary>
    /// Adds the signed-in customer's reply to one of their requests (plain text). A reply to a pending or resolved request
    /// reopens it, so it is back in the agent queue.
    /// </summary>
    public bool ReplyAsCustomer(int ticketId, string text)
    {
        if (OwnTicket(ticketId) is not { } ticket || string.IsNullOrWhiteSpace(text) || text.Trim().Length > MaxCustomerMessageLength
            || ticket.Messages.Count >= MaxMessagesPerTicket)
            return false;

        var before = TicketFacts(ticket);
        ticket.Messages.Add(new HelpDeskMessage(ticket.CustomerName, IsAgent: false, IsInternalNote: false, IsRichText: false, text.Trim(), NextTime(ticket)));

        if (ticket.Status is HelpDeskStatus.Pending or HelpDeskStatus.Resolved)
        {
            var wasResolved = ticket.Status == HelpDeskStatus.Resolved;
            ApplyStatus(ticket, HelpDeskStatus.Open);
            ticket.Satisfaction = null;
            ticket.SatisfactionComment = null;
            Log(ticket, wasResolved ? "Reopened by the customer's reply" : "Customer replied; status set to Open");
        }
        else
        {
            Log(ticket, "Customer replied");
        }

        Record("Replied", TicketTarget(ticket.Id), TicketLink(ticket.Id), TicketChanges(before, ticket));
        Notify(FindAgentByName(ticket.Assignee)?.AccountId, FormattableString.Invariant($"{ticket.CustomerName} replied on ticket #{ticket.Id}"), ticket.Subject, TicketLink(ticket.Id));
        Changed?.Invoke();

        return true;
    }

    /// <summary>Resolves a ticket (agents).</summary>
    public bool Resolve(HelpDeskTicket ticket)
    {
        if (!IsAgent || ticket.Status == HelpDeskStatus.Resolved)
            return false;

        var before = TicketFacts(ticket);

        if (ticket.Assignee is null)
        {
            ticket.Assignee = AgentName;
            Log(ticket, $"Assigned to {AgentName}");
        }

        SetStatusCore(ticket, HelpDeskStatus.Resolved);
        Record("Resolved", TicketTarget(ticket.Id), TicketLink(ticket.Id), TicketChanges(before, ticket));
        Changed?.Invoke();

        return true;
    }

    /// <summary>
    /// Adds a roster agent (<paramref name="agentId" /> <see langword="null" />) or edits one (admins). Names and emails are
    /// unique. A rename follows to the tickets assigned to the agent. The admin flag of an agent who signs in to the demo comes
    /// from their account and can't change here.
    /// </summary>
    public HelpDeskAdminResult SaveAgent(string? agentId, string name, string email, string teamId, bool isAdmin, out HelpDeskAgent? saved)
    {
        saved = null;

        if (!IsAdmin)
            return HelpDeskAdminResult.NotAllowed;

        name = name?.Trim() ?? string.Empty;
        email = email?.Trim() ?? string.Empty;

        if (!IsText(name, ShowcaseIdentity.MaxNameLength) || !IsEmail(email) || FindTeam(teamId) is null)
            return HelpDeskAdminResult.Invalid;

        var existing = agentId is null ? null : FindAgent(agentId);

        if (agentId is not null && existing is null)
            return HelpDeskAdminResult.NotFound;

        if (agents.Any(agent => agent.Id != agentId && (string.Equals(agent.Name, name, StringComparison.OrdinalIgnoreCase) || string.Equals(agent.Email, email, StringComparison.OrdinalIgnoreCase))))
            return HelpDeskAdminResult.Duplicate;

        if (existing is null)
        {
            if (agents.Count >= MaxAgents)
                return HelpDeskAdminResult.LimitReached;

            saved = new HelpDeskAgent(FormattableString.Invariant($"a-{nextAgentId++}"), name, email, teamId, isAdmin, IsActive: true, AccountId: null);
            agents.Add(saved);
            Record("Added", $"Agent: {saved.Name}", AdminAgentsLink, $"{FindTeam(teamId)!.Name}, {(isAdmin ? "admin" : "agent")}");
            Changed?.Invoke();

            return HelpDeskAdminResult.Ok;
        }

        saved = existing with { Name = name, Email = email, TeamId = teamId, IsAdmin = existing.AccountId is null ? isAdmin : existing.IsAdmin };

        if (saved == existing)
            return HelpDeskAdminResult.NoChange;

        agents[agents.IndexOf(existing)] = saved;

        if (saved.Name != existing.Name)
        {
            foreach (var ticket in Tickets.Where(ticket => ticket.Assignee == existing.Name))
                ticket.Assignee = saved.Name;
        }

        var agentChanges = new List<string>();

        if (saved.Name != existing.Name)
            agentChanges.Add($"Name {existing.Name} → {saved.Name}");

        if (saved.Email != existing.Email)
            agentChanges.Add($"Email {existing.Email} → {saved.Email}");

        if (saved.TeamId != existing.TeamId)
        {
            agentChanges.Add($"Team {FindTeam(existing.TeamId)?.Name ?? existing.TeamId} → {FindTeam(saved.TeamId)!.Name}");

            // A lead leads their own team: moving them leaves the old team without one.
            if (ClearLead(saved.Id) is { } ledTeam)
                agentChanges.Add($"No longer leads {ledTeam}");
        }

        if (saved.IsAdmin != existing.IsAdmin)
            agentChanges.Add(saved.IsAdmin ? "Made an admin" : "No longer an admin");

        Record("Updated", $"Agent: {saved.Name}", AdminAgentsLink, string.Join("; ", agentChanges));
        Changed?.Invoke();

        return HelpDeskAdminResult.Ok;
    }

    /// <summary>
    /// Adds an article (<paramref name="articleId" /> 0) or edits one (agents). The body is editor HTML; it is stored as written
    /// and rendered through the allowlist everywhere (A05). <paramref name="publish" /> makes it public in the help center.
    /// </summary>
    public HelpDeskAdminResult SaveArticle(int articleId, string title, string categoryId, string summary, string html, bool publish, out HelpDeskArticle? saved)
    {
        saved = null;

        if (!IsAgent)
            return HelpDeskAdminResult.NotAllowed;

        title = title?.Trim() ?? string.Empty;
        summary = summary?.Trim() ?? string.Empty;

        if (!IsText(title, MaxArticleTitleLength) || !IsText(summary, MaxArticleSummaryLength) || categories.All(category => category.Id != categoryId)
            || string.IsNullOrWhiteSpace(html) || html.Length > MaxArticleHtmlLength
            || string.IsNullOrWhiteSpace(System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", string.Empty).Replace("&nbsp;", " ", StringComparison.Ordinal)))
            return HelpDeskAdminResult.Invalid;

        if (articles.Any(article => article.Id != articleId && string.Equals(article.Title, title, StringComparison.OrdinalIgnoreCase)))
            return HelpDeskAdminResult.Duplicate;

        if (articleId == 0)
        {
            if (articles.Count >= MaxArticles)
                return HelpDeskAdminResult.LimitReached;

            saved = new HelpDeskArticle(nextArticleId++, title, categoryId, summary, html, Clock.Now, AgentName, 0, publish);
            articles.Add(saved);
            Record(publish ? "Published" : "Created", $"Article: {saved.Title}", ArticleLink(saved.Id), publish ? "New article" : "Draft");
            Changed?.Invoke();

            return HelpDeskAdminResult.Ok;
        }

        var index = articles.FindIndex(article => article.Id == articleId);

        if (index < 0)
            return HelpDeskAdminResult.NotFound;

        var wasPublished = articles[index].IsPublished;
        saved = articles[index] with { Title = title, CategoryId = categoryId, Summary = summary, Html = html, IsPublished = publish, UpdatedAt = Clock.Now, UpdatedBy = AgentName };
        articles[index] = saved;
        Record(publish == wasPublished ? "Updated" : publish ? "Published" : "Unpublished", $"Article: {saved.Title}", ArticleLink(saved.Id));
        Changed?.Invoke();

        return HelpDeskAdminResult.Ok;
    }

    /// <summary>Adds a category (<paramref name="categoryId" /> <see langword="null" />) or edits one (admins): name, icon, and the team its new requests go to.</summary>
    public HelpDeskAdminResult SaveCategory(string? categoryId, string name, IconName iconName, string teamId, out HelpDeskCategory? saved)
    {
        saved = null;

        if (!IsAdmin)
            return HelpDeskAdminResult.NotAllowed;

        name = name?.Trim() ?? string.Empty;

        if (!IsText(name, MaxGroupNameLength) || !HelpDeskSeedData.CategoryIcons().Contains(iconName) || FindTeam(teamId) is null)
            return HelpDeskAdminResult.Invalid;

        var index = categoryId is null ? -1 : categories.FindIndex(category => category.Id == categoryId);

        if (categoryId is not null && index < 0)
            return HelpDeskAdminResult.NotFound;

        if (categories.Any(category => category.Id != categoryId && string.Equals(category.Name, name, StringComparison.OrdinalIgnoreCase)))
            return HelpDeskAdminResult.Duplicate;

        if (index < 0)
        {
            if (categories.Count >= MaxCategories)
                return HelpDeskAdminResult.LimitReached;

            saved = new HelpDeskCategory(FormattableString.Invariant($"c-{nextCategoryId++}"), name, iconName, teamId);
            categories.Add(saved);
            Record("Added", $"Category: {saved.Name}", AdminCategoriesLink, $"Routed to {FindTeam(teamId)!.Name}");
        }
        else
        {
            var previous = categories[index];
            saved = previous with { Name = name, IconName = iconName, TeamId = teamId };

            if (saved == previous)
                return HelpDeskAdminResult.NoChange;

            categories[index] = saved;

            var categoryChanges = new List<string>();

            if (saved.Name != previous.Name)
                categoryChanges.Add($"Name {previous.Name} → {saved.Name}");

            if (saved.TeamId != previous.TeamId)
                categoryChanges.Add($"Routed to {FindTeam(saved.TeamId)!.Name}");

            if (saved.IconName != previous.IconName)
                categoryChanges.Add("New icon");

            Record("Updated", $"Category: {saved.Name}", AdminCategoriesLink, string.Join("; ", categoryChanges));
        }

        Changed?.Invoke();

        return HelpDeskAdminResult.Ok;
    }

    /// <summary>
    /// Adds a team (<paramref name="teamId" /> <see langword="null" />) or edits one (admins). Names are unique. The lead, who
    /// is notified of the team's escalations, is an active agent on the team, or <see langword="null" />; a new team has no
    /// agents yet, so it starts without one.
    /// </summary>
    public HelpDeskAdminResult SaveTeam(string? teamId, string name, string? description, string? leadAgentId, out HelpDeskTeam? saved)
    {
        saved = null;

        if (!IsAdmin)
            return HelpDeskAdminResult.NotAllowed;

        name = name?.Trim() ?? string.Empty;
        description = description?.Trim() ?? string.Empty;
        leadAgentId = string.IsNullOrEmpty(leadAgentId) ? null : leadAgentId;

        if (!IsText(name, MaxGroupNameLength) || description.Length > MaxTeamDescriptionLength
            || (leadAgentId is not null && (teamId is null || FindAgent(leadAgentId) is not { IsActive: true } lead || lead.TeamId != teamId)))
            return HelpDeskAdminResult.Invalid;

        var index = teamId is null ? -1 : teams.FindIndex(team => team.Id == teamId);

        if (teamId is not null && index < 0)
            return HelpDeskAdminResult.NotFound;

        if (teams.Any(team => team.Id != teamId && string.Equals(team.Name, name, StringComparison.OrdinalIgnoreCase)))
            return HelpDeskAdminResult.Duplicate;

        if (index < 0)
        {
            if (teams.Count >= MaxTeams)
                return HelpDeskAdminResult.LimitReached;

            saved = new HelpDeskTeam(FormattableString.Invariant($"t-{nextTeamId++}"), name, description);
            teams.Add(saved);
            Record("Added", $"Team: {saved.Name}", AdminAgentsLink);
        }
        else
        {
            var previous = teams[index];
            saved = previous with { Name = name, Description = description, LeadAgentId = leadAgentId };

            if (saved == previous)
                return HelpDeskAdminResult.NoChange;

            teams[index] = saved;

            var teamChanges = new List<string>();

            if (saved.Name != previous.Name)
                teamChanges.Add($"Name {previous.Name} → {saved.Name}");

            if (saved.Description != previous.Description)
                teamChanges.Add("New description");

            if (saved.LeadAgentId != previous.LeadAgentId)
                teamChanges.Add($"Lead {FindAgent(previous.LeadAgentId)?.Name ?? "None"} → {FindAgent(saved.LeadAgentId)?.Name ?? "None"}");

            Record("Updated", $"Team: {saved.Name}", AdminAgentsLink, string.Join("; ", teamChanges));
        }

        Changed?.Invoke();

        return HelpDeskAdminResult.Ok;
    }

    /// <summary>
    /// The header's global search, best matches first (an exact number or name, then names that start with the term, then
    /// names that contain it). Agents and admins find tickets (by number, subject, customer name, or email) and knowledge base
    /// articles, drafts included; admins also find agents by name or email. Customers find their own requests and the
    /// published articles. Returns nothing for a blank term or when nobody is signed in.
    /// </summary>
    public IReadOnlyList<ShowcaseSearchResult> Search(string term, int maxResults)
    {
        term = term?.Trim() ?? string.Empty;

        if (term.Length == 0 || maxResults <= 0 || Identity.Current is not { } account)
            return Array.Empty<ShowcaseSearchResult>();

        var number = term.TrimStart('#');

        int? Rank(params string?[] values) => ShowcaseSearch.Rank(term, values);

        int? NumberRank(int id)
        {
            var text = id.ToString(CultureInfo.InvariantCulture);

            return number.Length == 0 ? null : text == number ? 0 : text.StartsWith(number, StringComparison.Ordinal) ? 1 : text.Contains(number, StringComparison.Ordinal) ? 2 : null;
        }

        var matches = new List<(int Rank, int Kind, ShowcaseSearchResult Result)>();
        var tickets = IsAgent ? Tickets : Tickets.Where(ticket => ticket.CustomerAccountId == account.Id && account.RoleId == HelpDeskSeedData.CustomerRole);

        foreach (var ticket in tickets)
        {
            var rank = new[] { NumberRank(ticket.Id), IsAgent ? Rank(ticket.Subject, ticket.CustomerName, ticket.CustomerEmail) : Rank(ticket.Subject) }.Min();

            if (rank is not { } found)
                continue;

            var label = FormattableString.Invariant($"#{ticket.Id} {ticket.Subject}");
            matches.Add(IsAgent
                ? (found, 0, new ShowcaseSearchResult("Ticket", label, $"{ticket.CustomerName} · {ticket.Status}", TicketLink(ticket.Id)))
                : (found, 0, new ShowcaseSearchResult("Request", label, FindCategory(ticket.CategoryId).Name, FormattableString.Invariant($"{SiteBase}/requests/{ticket.Id}"))));
        }

        foreach (var article in IsAgent ? AllArticles : Articles)
        {
            if (Rank(article.Title) is not { } found)
                continue;

            matches.Add(IsAgent
                ? (found, 1, new ShowcaseSearchResult("Article", article.Title, $"{FindCategory(article.CategoryId).Name} · {(article.IsPublished ? "Published" : "Draft")}", ArticleLink(article.Id)))
                : (found, 1, new ShowcaseSearchResult("Article", article.Title, FindCategory(article.CategoryId).Name, FormattableString.Invariant($"{SiteBase}/kb/{article.Id}"))));
        }

        // The roster page is an admin page, so only admins find agents.
        if (IsAdmin)
        {
            foreach (var agent in Roster)
            {
                if (Rank(agent.Name, agent.Email) is { } found)
                    matches.Add((found, 2, new ShowcaseSearchResult("Agent", agent.Name, agent.IsActive ? agent.Email : $"{agent.Email} · Inactive", AdminAgentsLink)));
            }
        }

        return matches
            .OrderBy(match => match.Rank)
            .ThenBy(match => match.Kind)
            .ThenBy(match => match.Result.Label, StringComparer.OrdinalIgnoreCase)
            .Take(maxResults)
            .Select(match => match.Result)
            .ToList();
    }

    /// <summary>
    /// Deactivates or reactivates a roster agent (admins). Deactivating unassigns the agent's open tickets, so they go back to
    /// their team's queue; <paramref name="unassigned" /> is how many. Agents who sign in to the demo stay active.
    /// </summary>
    public HelpDeskAdminResult SetAgentActive(string agentId, bool active, out int unassigned)
    {
        unassigned = 0;

        if (!IsAdmin)
            return HelpDeskAdminResult.NotAllowed;

        if (FindAgent(agentId) is not { } agent)
            return HelpDeskAdminResult.NotFound;

        if (agent.AccountId is not null)
            return HelpDeskAdminResult.NotAllowed;

        if (agent.IsActive == active)
            return HelpDeskAdminResult.NoChange;

        agents[agents.IndexOf(agent)] = agent with { IsActive = active };
        string? ledTeam = null;

        if (!active)
        {
            foreach (var ticket in Tickets.Where(ticket => ticket.Assignee == agent.Name && ticket.Status != HelpDeskStatus.Resolved))
            {
                ticket.Assignee = null;
                Log(ticket, $"Unassigned: {agent.Name} was deactivated");
                unassigned++;
            }

            ledTeam = ClearLead(agent.Id);
        }

        Record(active ? "Reactivated" : "Deactivated", $"Agent: {agent.Name}", AdminAgentsLink,
            active ? null : string.Create(CultureInfo.InvariantCulture, $"{unassigned} open {(unassigned == 1 ? "ticket" : "tickets")} unassigned")
                + (ledTeam is null ? null : $"; no longer leads {ledTeam}"));
        Changed?.Invoke();

        return HelpDeskAdminResult.Ok;
    }

    /// <summary>Publishes or unpublishes an article (agents).</summary>
    public HelpDeskAdminResult SetArticlePublished(int articleId, bool publish)
    {
        if (!IsAgent)
            return HelpDeskAdminResult.NotAllowed;

        var index = articles.FindIndex(article => article.Id == articleId);

        if (index < 0)
            return HelpDeskAdminResult.NotFound;

        if (articles[index].IsPublished == publish)
            return HelpDeskAdminResult.NoChange;

        articles[index] = articles[index] with { IsPublished = publish, UpdatedAt = Clock.Now, UpdatedBy = AgentName };
        Record(publish ? "Published" : "Unpublished", $"Article: {articles[index].Title}", ArticleLink(articleId));
        Changed?.Invoke();

        return HelpDeskAdminResult.Ok;
    }

    /// <summary>Returns the team a ticket belongs to: its assignee's team, or while unassigned its category's team.</summary>
    public HelpDeskTeam? TeamOf(HelpDeskTicket ticket) => FindTeam(FindAgentByName(ticket.Assignee)?.TeamId ?? FindCategory(ticket.CategoryId).TeamId);

    /// <summary>
    /// Changes a priority's SLA targets (admins): first reply 1–168 hours, resolution 1–720 hours and not shorter than the
    /// first reply. Every ticket of that priority follows the new targets right away.
    /// </summary>
    public HelpDeskAdminResult UpdateSlaPolicy(HelpDeskPriority priority, int firstReplyHours, int resolveHours)
    {
        if (!IsAdmin)
            return HelpDeskAdminResult.NotAllowed;

        if (!Enum.IsDefined(priority) || firstReplyHours is < 1 or > MaxFirstReplyHours || resolveHours is < 1 or > MaxResolveHours || firstReplyHours > resolveHours)
            return HelpDeskAdminResult.Invalid;

        var index = slaPolicies.FindIndex(policy => policy.Priority == priority);
        var updated = new HelpDeskSlaPolicy(priority, firstReplyHours, resolveHours);

        var previousPolicy = slaPolicies[index];

        if (previousPolicy == updated)
            return HelpDeskAdminResult.NoChange;

        slaPolicies[index] = updated;
        Record("Updated", $"SLA policy: {priority}", AdminSlaLink, string.Create(CultureInfo.InvariantCulture,
            $"First reply {previousPolicy.FirstReplyHours} → {firstReplyHours} h, resolution {previousPolicy.ResolveHours} → {resolveHours} h"));
        Changed?.Invoke();

        return HelpDeskAdminResult.Ok;
    }

    /// <summary>
    /// How many unresolved tickets meet a rule's conditions now and haven't been escalated by it (<paramref name="ruleId" />,
    /// when it is saved already): the tickets saving the rule escalates.
    /// </summary>
    public int CountRuleMatches(HelpDeskPriority? priority, HelpDeskEscalationTrigger trigger, int hours, int? ruleId = null) =>
        ValidRuleHours(trigger, hours) ? Tickets.Count(ticket => Meets(ticket, priority, trigger, hours) && (ruleId is not { } id || ticket.EscalatedByRuleIds?.Contains(id) != true)) : 0;

    /// <summary>Deletes an escalation rule (admins). Tickets it escalated stay at their tier.</summary>
    public HelpDeskAdminResult DeleteEscalationRule(int ruleId)
    {
        if (!IsAdmin)
            return HelpDeskAdminResult.NotAllowed;

        if (escalationRules.FirstOrDefault(rule => rule.Id == ruleId) is not { } removed)
            return HelpDeskAdminResult.NotFound;

        escalationRules.Remove(removed);
        Record("Deleted", $"Escalation rule: {removed.Name}", AdminSlaLink);
        Changed?.Invoke();

        return HelpDeskAdminResult.Ok;
    }

    /// <summary>How many tickets a rule has escalated.</summary>
    public int EscalatedCount(int ruleId) => Tickets.Count(ticket => ticket.EscalatedByRuleIds?.Contains(ruleId) == true);

    /// <summary>Returns an escalation rule by ID, or <see langword="null" />.</summary>
    public HelpDeskEscalationRule? FindEscalationRule(int ruleId) => escalationRules.FirstOrDefault(rule => rule.Id == ruleId);

    /// <summary>
    /// Runs the active escalation rules on the unresolved tickets now (AC-7a) and returns how many tickets moved up a tier or
    /// were flagged. The agent desk runs it on every page it opens, a restore runs it, and so does saving a rule.
    /// </summary>
    public int RunEscalations()
    {
        var escalated = Escalate(seeding: false);

        if (escalated > 0)
            Changed?.Invoke();

        return escalated;
    }

    /// <summary>
    /// Adds an escalation rule (<paramref name="ruleId" /> <see langword="null" />) or edits one (admins), then runs the rules,
    /// so the tickets that meet it now are escalated straight away; <paramref name="escalated" /> is how many. Names are
    /// unique; hours are 1 to 168 for a first reply and 1 to 720 for a resolution.
    /// </summary>
    public HelpDeskAdminResult SaveEscalationRule(int? ruleId, string name, HelpDeskPriority? priority, HelpDeskEscalationTrigger trigger, int hours, bool isActive,
        out HelpDeskEscalationRule? saved, out int escalated)
    {
        saved = null;
        escalated = 0;

        if (!IsAdmin)
            return HelpDeskAdminResult.NotAllowed;

        name = name?.Trim() ?? string.Empty;

        if (!IsText(name, MaxRuleNameLength) || (priority is { } p && !Enum.IsDefined(p)) || !Enum.IsDefined(trigger) || !ValidRuleHours(trigger, hours))
            return HelpDeskAdminResult.Invalid;

        var index = ruleId is null ? -1 : escalationRules.FindIndex(rule => rule.Id == ruleId);

        if (ruleId is not null && index < 0)
            return HelpDeskAdminResult.NotFound;

        if (escalationRules.Any(rule => rule.Id != ruleId && string.Equals(rule.Name, name, StringComparison.OrdinalIgnoreCase)))
            return HelpDeskAdminResult.Duplicate;

        if (index < 0)
        {
            if (escalationRules.Count >= MaxEscalationRules)
                return HelpDeskAdminResult.LimitReached;

            saved = new HelpDeskEscalationRule(nextEscalationRuleId++, name, priority, trigger, hours, isActive);
            escalationRules.Add(saved);
            Record("Added", $"Escalation rule: {saved.Name}", AdminSlaLink, RuleSummary(saved));
        }
        else
        {
            var previous = escalationRules[index];
            saved = previous with { Name = name, Priority = priority, Trigger = trigger, Hours = hours, IsActive = isActive };

            if (saved == previous)
                return HelpDeskAdminResult.NoChange;

            escalationRules[index] = saved;
            Record("Updated", $"Escalation rule: {saved.Name}", AdminSlaLink, RuleSummary(saved));
        }

        escalated = Escalate(seeding: false);
        Changed?.Invoke();

        return HelpDeskAdminResult.Ok;
    }

    /// <summary>A rule in words, like "Urgent tickets not resolved within 1 hour"; "Off" is added for a rule that is switched off.</summary>
    public static string RuleSummary(HelpDeskEscalationRule rule) => RuleSummary(rule.Priority, rule.Trigger, rule.Hours) + (rule.IsActive ? string.Empty : " (off)");

    /// <summary>A rule's conditions in words, like "Urgent tickets not resolved within 1 hour".</summary>
    public static string RuleSummary(HelpDeskPriority? priority, HelpDeskEscalationTrigger trigger, int hours)
    {
        var who = priority is { } p ? $"{p} tickets" : "Tickets of any priority";
        var within = hours == 1 ? "1 hour" : string.Create(CultureInfo.InvariantCulture, $"{hours} hours");

        return trigger == HelpDeskEscalationTrigger.NoFirstReply ? $"{who} with no first reply within {within}" : $"{who} not resolved within {within}";
    }

    /// <summary>Whether the hours suit the trigger: a first reply rule 1 to 168 hours, a resolution rule 1 to 720 hours.</summary>
    public static bool ValidRuleHours(HelpDeskEscalationTrigger trigger, int hours) =>
        hours >= 1 && hours <= (trigger == HelpDeskEscalationTrigger.NoFirstReply ? MaxFirstReplyHours : MaxResolveHours);

    /// <summary>Adds a saved reply (<paramref name="replyId" /> 0) or edits one (agents). The title is 1–60 characters.</summary>
    public HelpDeskSavedReply? SaveReply(int replyId, string title, string html)
    {
        if (!IsAgent || !IsText(title?.Trim(), 60) || string.IsNullOrWhiteSpace(html) || html.Length > 5000
            || System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", string.Empty).Length > MaxCustomerMessageLength)
            return null;

        if (replyId == 0)
        {
            if (savedReplies.Count >= MaxSavedReplies)
                return null;

            var added = new HelpDeskSavedReply(nextSavedReplyId++, title!.Trim(), html);
            savedReplies.Add(added);
            Record("Added", $"Saved reply: {added.Title}", SavedRepliesLink);
            Changed?.Invoke();

            return added;
        }

        var index = savedReplies.FindIndex(reply => reply.Id == replyId);

        if (index < 0)
            return null;

        savedReplies[index] = new HelpDeskSavedReply(replyId, title!.Trim(), html);
        Record("Updated", $"Saved reply: {savedReplies[index].Title}", SavedRepliesLink);
        Changed?.Invoke();

        return savedReplies[index];
    }

    /// <summary>Changes a ticket's category (agents).</summary>
    public bool SetCategory(HelpDeskTicket ticket, string categoryId)
    {
        if (!IsAgent || categories.All(category => category.Id != categoryId) || ticket.CategoryId == categoryId)
            return false;

        var before = TicketFacts(ticket);
        Log(ticket, $"Category {FindCategory(ticket.CategoryId).Name} → {FindCategory(categoryId).Name}");
        ticket.CategoryId = categoryId;
        Record("Changed category", TicketTarget(ticket.Id), TicketLink(ticket.Id), TicketChanges(before, ticket));
        Changed?.Invoke();

        return true;
    }

    /// <summary>Changes a ticket's priority (agents); the SLA due time follows the new priority.</summary>
    public bool SetPriority(HelpDeskTicket ticket, HelpDeskPriority priority)
    {
        if (!IsAgent || !Enum.IsDefined(priority) || ticket.Priority == priority)
            return false;

        var before = TicketFacts(ticket);
        Log(ticket, $"Priority {ticket.Priority} → {priority}");
        ticket.Priority = priority;
        Record("Changed priority", TicketTarget(ticket.Id), TicketLink(ticket.Id), TicketChanges(before, ticket));
        Changed?.Invoke();

        return true;
    }

    /// <summary>Changes a ticket's status (agents).</summary>
    public bool SetStatus(HelpDeskTicket ticket, HelpDeskStatus status)
    {
        if (!IsAgent || !Enum.IsDefined(status) || ticket.Status == status)
            return false;

        var before = TicketFacts(ticket);
        SetStatusCore(ticket, status);
        Record(status == HelpDeskStatus.Resolved ? "Resolved" : "Changed status", TicketTarget(ticket.Id), TicketLink(ticket.Id), TicketChanges(before, ticket));
        Changed?.Invoke();

        return true;
    }

    /// <summary>Returns a ticket's SLA state on the site's clock.</summary>
    public HelpDeskSlaState Sla(HelpDeskTicket ticket)
    {
        var due = DueAt(ticket);

        if (ticket.Status == HelpDeskStatus.Resolved)
            return ticket.ResolvedAt <= due ? HelpDeskSlaState.Met : HelpDeskSlaState.Missed;

        if (ticket.Status == HelpDeskStatus.Pending)
        {
            // The pause doesn't excuse a ticket that was already overdue when it went Pending.
            var dueBeforePause = ticket.CreatedAt.AddHours(TargetHours(ticket.Priority)).AddMinutes(ticket.PausedMinutes);

            return ticket.PendingSince is { } since && since >= dueBeforePause ? HelpDeskSlaState.Breached : HelpDeskSlaState.Paused;
        }

        var left = due - Clock.Now;

        if (left <= TimeSpan.Zero)
            return HelpDeskSlaState.Breached;

        return left <= TimeSpan.FromHours(2) || left.TotalHours <= TargetHours(ticket.Priority) / 4.0
            ? HelpDeskSlaState.AtRisk
            : HelpDeskSlaState.OnTrack;
    }

    /// <summary>
    /// Submits a request as the signed-in customer: the ticket uses their account's name and email, starts as New, and appears
    /// in the agent queue and in their requests.
    /// </summary>
    public HelpDeskTicket? SubmitRequest(string? company, string categoryId, HelpDeskPriority priority, string subject, string description)
    {
        if (Identity.Current is not { RoleId: HelpDeskSeedData.CustomerRole } account || !Enum.IsDefined(priority)
            || !ValidNewTicket(account.Name, account.Email, company, subject, description))
            return null;

        var ticket = NewTicket(account.Name, account.Email, company, categoryId, priority, subject, description, account.Id);
        Record("Submitted", TicketTarget(ticket.Id), TicketLink(ticket.Id), $"{FindCategory(ticket.CategoryId).Name}, {ticket.Priority} priority");
        Changed?.Invoke();

        return ticket;
    }

    /// <summary>Validates a stored snapshot and applies it only when every part is valid.</summary>
    public bool TryRestoreData(JsonElement data)
    {
        HelpDeskSnapshot? snapshot;

        try
        {
            snapshot = data.Deserialize<HelpDeskSnapshot>(snapshotOptions);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException)
        {
            return false;
        }

        if (snapshot is not
            {
                Tickets: { Count: >= 1 and <= MaxTickets } storedTickets,
                SavedReplies: { Count: <= MaxSavedReplies } storedReplies,
                Votes: { } storedVotes,
                Teams: { Count: >= 1 and <= MaxTeams } storedTeams,
                Agents: { Count: >= 1 and <= MaxAgents } storedAgents,
                Categories: { Count: >= 1 and <= MaxCategories } storedCategories,
                SlaPolicies: { } storedPolicies,
                Articles: { Count: <= MaxArticles } storedArticles,
                Audit: { Count: <= MaxAuditEntries } storedAudit,
                Notifications: { Count: <= MaxNotifications } storedNotifications,
                EscalationRules: { Count: <= MaxEscalationRules } storedRules,
            }
            || snapshot.NextTicketId is < 2 or > 1_000_000
            || snapshot.NextEscalationRuleId is < 1 or > 100_000
            || snapshot.NextAuditId is < 1 or > 10_000_000
            || snapshot.NextNotificationId is < 1 or > 10_000_000
            || snapshot.NextSavedReplyId is < 1 or > 100_000
            || snapshot.NextArticleId is < 1 or > 100_000
            || snapshot.NextAgentId is < 1 or > 100_000
            || snapshot.NextCategoryId is < 1 or > 100_000
            || snapshot.NextTeamId is < 1 or > 100_000)
            return false;

        if (!CounterAhead(storedTeams.Select(team => team?.Id), "t-", snapshot.NextTeamId)
            || !CounterAhead(storedAgents.Select(agent => agent?.Id), "a-", snapshot.NextAgentId)
            || !CounterAhead(storedCategories.Select(category => category?.Id), "c-", snapshot.NextCategoryId))
            return false;

        if (storedTeams.Any(team => team is null || !IsText(team.Id, 20) || !IsText(team.Name, MaxGroupNameLength) || team.Description is null || team.Description.Length > MaxTeamDescriptionLength)
            || !Unique(storedTeams.Select(team => team.Id)) || !Unique(storedTeams.Select(team => team.Name.ToUpperInvariant())))
            return false;

        var teamIds = storedTeams.Select(team => team.Id).ToHashSet(StringComparer.Ordinal);

        if (!ValidRoster(storedAgents, teamIds))
            return false;

        // A team lead is an active agent on the team.
        if (storedTeams.Any(team => team.LeadAgentId is { } leadId && !storedAgents.Any(agent => agent.Id == leadId && agent.IsActive && agent.TeamId == team.Id)))
            return false;

        if (storedRules.Any(rule => rule is null || rule.Id < 1 || rule.Id >= snapshot.NextEscalationRuleId || !IsText(rule.Name, MaxRuleNameLength)
                || (rule.Priority is { } rulePriority && !Enum.IsDefined(rulePriority)) || !Enum.IsDefined(rule.Trigger) || !ValidRuleHours(rule.Trigger, rule.Hours))
            || !Unique(storedRules.Select(rule => rule.Id)) || !Unique(storedRules.Select(rule => rule.Name.ToUpperInvariant())))
            return false;

        var icons = HelpDeskSeedData.CategoryIcons();

        if (storedCategories.Any(category => category is null || !IsText(category.Id, 20) || !IsText(category.Name, MaxGroupNameLength) || !icons.Contains(category.IconName) || !teamIds.Contains(category.TeamId))
            || !Unique(storedCategories.Select(category => category.Id)) || !Unique(storedCategories.Select(category => category.Name.ToUpperInvariant())))
            return false;

        if (storedPolicies.Count != 4 || storedPolicies.Any(policy => policy is null || !Enum.IsDefined(policy.Priority) || policy.FirstReplyHours is < 1 or > MaxFirstReplyHours
                || policy.ResolveHours is < 1 or > MaxResolveHours || policy.FirstReplyHours > policy.ResolveHours)
            || !Unique(storedPolicies.Select(policy => policy.Priority)))
            return false;

        var categoryIds = storedCategories.Select(category => category.Id).ToHashSet(StringComparer.Ordinal);

        if (storedArticles.Any(article => article is null || article.Id < 1 || article.Id >= snapshot.NextArticleId || !IsText(article.Title, MaxArticleTitleLength)
                || !IsText(article.Summary, MaxArticleSummaryLength) || !IsText(article.Html, MaxArticleHtmlLength) || !categoryIds.Contains(article.CategoryId)
                || !IsPlausibleDate(article.UpdatedAt) || !IsText(article.UpdatedBy, ShowcaseIdentity.MaxNameLength) || article.Views is < 0 or > 10_000_000)
            || !Unique(storedArticles.Select(article => article.Id)))
            return false;

        var agentNames = storedAgents.Select(agent => agent.Name).ToHashSet(StringComparer.Ordinal);

        if (storedTickets.Any(ticket => !ValidTicket(ticket, snapshot.NextTicketId, categoryIds, agentNames)) || !Unique(storedTickets.Select(ticket => ticket.Id)))
            return false;

        // A rule that was deleted keeps its ID on the tickets it escalated, so the IDs only need to be ones the counter gave out.
        if (storedTickets.Any(ticket => ticket.EscalationLevel is < 0 or > MaxEscalationLevel
                || (ticket.EscalatedByRuleIds is { } ruleIds && (ruleIds.Count is < 1 or > MaxEscalationRules * 5 || ruleIds.Any(id => id < 1 || id >= snapshot.NextEscalationRuleId) || !Unique(ruleIds)))
                || (ticket.EscalationLevel > 0 && ticket.EscalatedByRuleIds is null)))
            return false;

        if (storedReplies.Any(reply => reply is null || reply.Id < 1 || reply.Id >= snapshot.NextSavedReplyId || !IsText(reply.Title, 60) || !IsText(reply.Html, 5000))
            || storedReplies.Select(reply => reply.Id).Distinct().Count() != storedReplies.Count)
            return false;

        if (storedVotes.Any(vote => vote is null || storedArticles.All(article => article.Id != vote.ArticleId) || vote.Helpful is < 0 or > 1_000_000 || vote.NotHelpful is < 0 or > 1_000_000)
            || storedVotes.Select(vote => vote.ArticleId).Distinct().Count() != storedVotes.Count)
            return false;

        if (storedAudit.Any(entry => entry is null || entry.Id < 1 || entry.Id >= snapshot.NextAuditId || !IsPlausibleDate(entry.At)
                || !IsText(entry.Actor, ShowcaseIdentity.MaxNameLength) || !IsText(entry.Action, MaxAuditTextLength) || !IsText(entry.Target, MaxAuditTextLength)
                || (entry.Details is not null && !IsText(entry.Details, MaxAuditTextLength)) || !IsSiteLink(entry.TargetUrl))
            || !Unique(storedAudit.Select(entry => entry.Id)))
            return false;

        // Notifications are only ever raised for the demo staff accounts.
        var staffAccounts = HelpDeskSeedData.DemoAccounts().Where(account => account.RoleId is HelpDeskSeedData.AgentRole or HelpDeskSeedData.AdminRole).Select(account => account.Id).ToHashSet(StringComparer.Ordinal);

        if (storedNotifications.Any(notification => notification is null || notification.Id < 1 || notification.Id >= snapshot.NextNotificationId
                || !staffAccounts.Contains(notification.AccountId) || !IsPlausibleDate(notification.At) || !IsText(notification.Title, MaxAuditTextLength)
                || (notification.Text is not null && !IsText(notification.Text, MaxAuditTextLength)) || !IsSiteLink(notification.Url))
            || !Unique(storedNotifications.Select(notification => notification.Id)))
            return false;

        Tickets.Clear();
        Tickets.AddRange(storedTickets.OrderByDescending(ticket => ticket.CreatedAt));
        savedReplies.Clear();
        savedReplies.AddRange(storedReplies);
        votes.Clear();

        foreach (var vote in storedVotes)
            votes[vote.ArticleId] = (vote.Helpful, vote.NotHelpful);

        teams.Clear();
        teams.AddRange(storedTeams);
        agents.Clear();
        agents.AddRange(storedAgents);
        categories.Clear();
        categories.AddRange(storedCategories);
        slaPolicies.Clear();
        slaPolicies.AddRange(storedPolicies.OrderBy(policy => policy.Priority));
        articles.Clear();
        articles.AddRange(storedArticles);
        audit.Clear();
        audit.AddRange(storedAudit.OrderBy(entry => entry.Id));
        notifications.Clear();
        notifications.AddRange(storedNotifications.OrderBy(notification => notification.Id));
        escalationRules.Clear();
        escalationRules.AddRange(storedRules.OrderBy(rule => rule.Id));

        nextTicketId = snapshot.NextTicketId;
        nextEscalationRuleId = snapshot.NextEscalationRuleId;
        nextSavedReplyId = snapshot.NextSavedReplyId;
        nextArticleId = snapshot.NextArticleId;
        nextAgentId = snapshot.NextAgentId;
        nextCategoryId = snapshot.NextCategoryId;
        nextTeamId = snapshot.NextTeamId;
        nextAuditId = snapshot.NextAuditId;
        nextNotificationId = snapshot.NextNotificationId;

        if (Filter.TeamId.Length > 0 && FindTeam(Filter.TeamId) is null)
            Filter.TeamId = string.Empty;

        return true;
    }

    /// <summary>Records a helpful or not-helpful vote on an article.</summary>
    public void Vote(int articleId, bool helpful)
    {
        if (FindArticle(articleId) is null)
            return;

        var current = Votes(articleId);
        votes[articleId] = helpful ? (Math.Min(current.Helpful + 1, 1_000_000), current.NotHelpful) : (current.Helpful, Math.Min(current.NotHelpful + 1, 1_000_000));
        Changed?.Invoke();
    }

    /// <summary>Returns an article's votes.</summary>
    public (int Helpful, int NotHelpful) Votes(int articleId) => votes.TryGetValue(articleId, out var count) ? count : (0, 0);

    #endregion

    #region Private Methods

    private static bool IsAccountId(string? accountId) =>
        accountId is not null && (HelpDeskSeedData.DemoAccounts().Any(demo => demo.Id == accountId) || (accountId.StartsWith("u-", StringComparison.Ordinal) && accountId.Length <= 12));

    private static bool IsEmail(string? value) =>
        IsText(value, ShowcaseIdentity.MaxEmailLength) && value!.IndexOf('@') is > 0 and var at && at < value.Length - 1 && !value.Any(char.IsWhiteSpace);

    private static bool IsPlausibleDate(DateTime value) => value >= HelpDeskSeedData.Now.AddYears(-2) && value <= HelpDeskSeedData.Now.AddYears(2);

    /// <summary>Checks a stored audit or notification link: none, or a path on this site without spaces, quotes, or a second slash.</summary>
    private static bool IsSiteLink(string? url) => ShowcaseLinks.IsSiteLink(url, SiteBase, allowSiteBase: false);

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

    /// <summary>The agent desk's edit page of an article (the same link as <c>HelpDeskDisplay.ArticleEditUrl</c>).</summary>
    private static string ArticleLink(int articleId) => FormattableString.Invariant($"{AgentBase}/kb/{articleId}");

    /// <summary>Shortens a text to the audit log's limit.</summary>
    private static string Clip(string text) => text.Length <= MaxAuditTextLength ? text : string.Concat(text.AsSpan(0, MaxAuditTextLength - 1), "…");

    /// <summary>The fields of a ticket that audit entries describe changes to.</summary>
    private static (string? Assignee, HelpDeskStatus Status, HelpDeskPriority Priority, string CategoryId) TicketFacts(HelpDeskTicket ticket) =>
        (ticket.Assignee, ticket.Status, ticket.Priority, ticket.CategoryId);

    /// <summary>The agent desk's page of a ticket (the same link as <c>HelpDeskDisplay.TicketUrl</c>).</summary>
    private static string TicketLink(int ticketId) => FormattableString.Invariant($"{AgentBase}/tickets/{ticketId}");

    /// <summary>The audit log's name for a ticket, like "Ticket #1047".</summary>
    private static string TicketTarget(int ticketId) => FormattableString.Invariant($"Ticket #{ticketId}");

    /// <summary>Checks that an ID counter is past every generated ID with its prefix, so the next generated ID is new.</summary>
    private static bool CounterAhead(IEnumerable<string?> ids, string prefix, int next) =>
        ids.All(id => id is null || !id.StartsWith(prefix, StringComparison.Ordinal)
            || !int.TryParse(id.AsSpan(prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var number) || number < next);

    private static bool Unique<T>(IEnumerable<T> values)
    {
        var list = values.ToList();

        return list.Distinct().Count() == list.Count;
    }

    /// <summary>
    /// Checks a stored roster: unique IDs, names, and emails, known teams, and the demo staff accounts each linked exactly once,
    /// active, with the admin flag of their role (so signing in always finds the roster entry).
    /// </summary>
    private static bool ValidRoster(List<HelpDeskAgent> roster, HashSet<string> teamIds)
    {
        if (roster.Any(agent => agent is null || !IsText(agent.Id, 20) || !IsText(agent.Name, ShowcaseIdentity.MaxNameLength) || !IsEmail(agent.Email) || !teamIds.Contains(agent.TeamId))
            || !Unique(roster.Select(agent => agent.Id)) || !Unique(roster.Select(agent => agent.Name.ToUpperInvariant())) || !Unique(roster.Select(agent => agent.Email.ToUpperInvariant())))
            return false;

        var staff = HelpDeskSeedData.DemoAccounts().Where(account => account.RoleId is HelpDeskSeedData.AgentRole or HelpDeskSeedData.AdminRole).ToList();
        var linked = roster.Where(agent => agent.AccountId is not null).ToList();

        return linked.Count == staff.Count
            && staff.All(account => linked.Count(agent => agent.AccountId == account.Id) == 1
                && linked.First(agent => agent.AccountId == account.Id) is { IsActive: true } entry
                && entry.IsAdmin == (account.RoleId == HelpDeskSeedData.AdminRole));
    }

    /// <summary>Changes a ticket's status and keeps the SLA pause and resolution time in step: Pending pauses the SLA (T28-F1).</summary>
    private void ApplyStatus(HelpDeskTicket ticket, HelpDeskStatus status)
    {
        var now = Clock.Now;

        // A deactivated agent keeps only the resolved tickets they worked; a reopened one goes back to the team's queue.
        if (ticket.Status == HelpDeskStatus.Resolved && status != HelpDeskStatus.Resolved && FindAgentByName(ticket.Assignee) is { IsActive: false } inactive)
        {
            ticket.Assignee = null;
            Log(ticket, $"Unassigned: {inactive.Name} is inactive");
        }

        if (ticket.Status == HelpDeskStatus.Pending && status != HelpDeskStatus.Pending && ticket.PendingSince is { } since)
        {
            ticket.PausedMinutes = Math.Min(ticket.PausedMinutes + Math.Max(0, (now - since).TotalMinutes), MaxPausedMinutes);
            ticket.PendingSince = null;
        }
        else if (status == HelpDeskStatus.Pending && ticket.Status != HelpDeskStatus.Pending)
        {
            ticket.PendingSince = now;
        }

        ticket.Status = status;
        ticket.ResolvedAt = status == HelpDeskStatus.Resolved ? now : null;
    }

    private static int SlaRank(HelpDeskSlaState state) => state switch
    {
        HelpDeskSlaState.Breached => 0,
        HelpDeskSlaState.AtRisk => 1,
        HelpDeskSlaState.OnTrack => 2,
        HelpDeskSlaState.Paused => 3,
        _ => 4,
    };

    private static bool ValidNewTicket(string name, string email, string? company, string subject, string description) =>
        IsText(name?.Trim(), ShowcaseIdentity.MaxNameLength) && IsEmail(email?.Trim()) && (string.IsNullOrWhiteSpace(company) || company.Trim().Length <= 80)
        && IsText(subject?.Trim(), MaxSubjectLength) && IsText(description?.Trim(), MaxCustomerMessageLength);

    /// <summary>
    /// Raises a notification for a demo account (rule WR-001); nothing for a roster agent without a sign-in, or for the
    /// person who caused it. The site keeps the newest <see cref="MaxNotifications" />.
    /// </summary>
    private void Notify(string? accountId, string title, string? text, string? url)
    {
        if (accountId is null || accountId == Identity.Current?.Id)
            return;

        notifications.Add(new ShowcaseNotification
        {
            Id = nextNotificationId++,
            AccountId = accountId,
            At = Clock.Now,
            Title = Clip(title),
            Text = string.IsNullOrWhiteSpace(text) ? null : Clip(text),
            Url = url,
        });

        if (notifications.Count > MaxNotifications)
            notifications.RemoveAt(0);
    }

    /// <summary>
    /// Adds an audit entry (rule WR-001): the signed-in person did <paramref name="action" /> (past tense) to
    /// <paramref name="target" />, now, on the site's clock. The log keeps the newest <see cref="MaxAuditEntries" />.
    /// </summary>
    private void Record(string action, string target, string? targetUrl, string? details = null) =>
        Record(Clock.Now, Identity.Current?.Name ?? HelpDeskSeedData.ProductName, action, target, targetUrl, details);

    /// <summary>Adds an audit entry for <paramref name="actor" /> at <paramref name="at" />: the escalation rules act on their own, not as the signed-in person.</summary>
    private void Record(DateTime at, string actor, string action, string target, string? targetUrl, string? details)
    {
        audit.Add(new ShowcaseAuditEntry
        {
            Id = nextAuditId++,
            At = at,
            Actor = actor,
            Action = action,
            Target = Clip(target),
            TargetUrl = targetUrl,
            Details = string.IsNullOrWhiteSpace(details) ? null : Clip(details),
        });

        if (audit.Count > MaxAuditEntries)
            audit.RemoveAt(0);
    }

    /// <summary>What changed on a ticket since <paramref name="before" />, for the audit log, or <see langword="null" /> when nothing did.</summary>
    private string? TicketChanges((string? Assignee, HelpDeskStatus Status, HelpDeskPriority Priority, string CategoryId) before, HelpDeskTicket ticket)
    {
        var changes = new List<string>();

        if (before.Assignee != ticket.Assignee)
            changes.Add($"Assignee {before.Assignee ?? "Unassigned"} → {ticket.Assignee ?? "Unassigned"}");

        if (before.Status != ticket.Status)
            changes.Add($"Status {before.Status} → {ticket.Status}");

        if (before.Priority != ticket.Priority)
            changes.Add($"Priority {before.Priority} → {ticket.Priority}");

        if (before.CategoryId != ticket.CategoryId)
            changes.Add($"Category {FindCategory(before.CategoryId).Name} → {FindCategory(ticket.CategoryId).Name}");

        return changes.Count == 0 ? null : string.Join("; ", changes);
    }

    private void Log(HelpDeskTicket ticket, string text)
    {
        ticket.Activity.Add(new HelpDeskActivity(Clock.Now, Identity.Current?.Name ?? ticket.CustomerName, text));

        if (ticket.Activity.Count > MaxActivityPerTicket)
            ticket.Activity.RemoveAt(0);
    }

    /// <summary>Clears the lead of the team an agent leads, and returns that team's name, or <see langword="null" /> when they lead none.</summary>
    private string? ClearLead(string agentId)
    {
        var index = teams.FindIndex(team => team.LeadAgentId == agentId);

        if (index < 0)
            return null;

        teams[index] = teams[index] with { LeadAgentId = null };

        return teams[index].Name;
    }

    /// <summary>
    /// Escalates every unresolved ticket that meets an active rule it hasn't been escalated by yet, and returns how many
    /// tickets changed. Each escalation moves the ticket up one tier (up to Tier 3), adds a line to its history, an audit
    /// entry, and a notification for its team's lead and its assignee. While <paramref name="seeding" />, each one is dated
    /// when the rule fired and its notifications are already read, so a fresh tab opens with a believable history.
    /// </summary>
    private int Escalate(bool seeding)
    {
        var changed = new HashSet<int>();

        foreach (var rule in escalationRules.Where(rule => rule.IsActive).OrderBy(rule => rule.Id).ToList())
        {
            foreach (var ticket in Tickets.Where(ticket => Meets(ticket, rule.Priority, rule.Trigger, rule.Hours) && ticket.EscalatedByRuleIds?.Contains(rule.Id) != true).ToList())
            {
                EscalateTicket(ticket, rule, seeding ? FiredAt(ticket, rule) : Clock.Now, seeding);
                changed.Add(ticket.Id);
            }
        }

        return changed.Count;
    }

    /// <summary>Moves one ticket up a tier for a rule, and records it.</summary>
    private void EscalateTicket(HelpDeskTicket ticket, HelpDeskEscalationRule rule, DateTime at, bool seeding)
    {
        var fromTier = ticket.Tier;

        (ticket.EscalatedByRuleIds ??= new List<int>()).Add(rule.Id);

        if (ticket.EscalationLevel < MaxEscalationLevel)
            ticket.EscalationLevel++;

        var moved = ticket.Tier != fromTier;
        var history = moved
            ? FormattableString.Invariant($"Escalated to Tier {ticket.Tier} by the rule \"{rule.Name}\"")
            : FormattableString.Invariant($"Flagged by the rule \"{rule.Name}\"; already at Tier {ticket.Tier}");

        // The history stays oldest first, also for an escalation dated in the past.
        var position = ticket.Activity.FindLastIndex(entry => entry.At <= at) + 1;
        ticket.Activity.Insert(position, new HelpDeskActivity(at, EscalationActor, history));

        if (ticket.Activity.Count > MaxActivityPerTicket)
            ticket.Activity.RemoveAt(0);

        Record(at, EscalationActor, "Escalated", TicketTarget(ticket.Id), TicketLink(ticket.Id),
            (moved ? FormattableString.Invariant($"Tier {fromTier} → {ticket.Tier}; ") : FormattableString.Invariant($"Stays at Tier {ticket.Tier}; ")) + $"rule \"{rule.Name}\"");

        var title = moved ? FormattableString.Invariant($"Ticket #{ticket.Id} escalated to Tier {ticket.Tier}") : FormattableString.Invariant($"Ticket #{ticket.Id} flagged again (Tier {ticket.Tier})");
        var text = $"{rule.Name}: {ticket.Subject}";
        var lead = FindAgent(TeamOf(ticket)?.LeadAgentId)?.AccountId;
        var assignee = FindAgentByName(ticket.Assignee)?.AccountId;

        NotifyAt(lead, at, title, text, TicketLink(ticket.Id), seeding);

        if (assignee != lead)
            NotifyAt(assignee, at, title, text, TicketLink(ticket.Id), seeding);
    }

    /// <summary>When a rule fired for a ticket, for the seeded history: when its hours ran out, but never later than now.</summary>
    private DateTime FiredAt(HelpDeskTicket ticket, HelpDeskEscalationRule rule)
    {
        var at = ticket.CreatedAt.AddHours(rule.Hours);

        if (rule.Trigger == HelpDeskEscalationTrigger.NotResolved)
            at = at.Add(PausedSoFar(ticket));

        return at > Clock.Now ? Clock.Now : at;
    }

    /// <summary>Whether an unresolved ticket meets a rule's conditions now.</summary>
    private bool Meets(HelpDeskTicket ticket, HelpDeskPriority? priority, HelpDeskEscalationTrigger trigger, int hours) =>
        ticket.Status != HelpDeskStatus.Resolved
        && (priority is null || ticket.Priority == priority)
        && (trigger == HelpDeskEscalationTrigger.NoFirstReply
            ? ticket.FirstReplyAt is null && Clock.Now >= ticket.CreatedAt.AddHours(hours)
            : Clock.Now - ticket.CreatedAt - PausedSoFar(ticket) >= TimeSpan.FromHours(hours));

    /// <summary>
    /// Raises a notification the escalation rules send: unlike <see cref="Notify" />, it reaches the signed-in person too,
    /// since nobody here caused it. Nothing for a roster agent without a sign-in.
    /// </summary>
    private void NotifyAt(string? accountId, DateTime at, string title, string? text, string? url, bool isRead)
    {
        if (accountId is null)
            return;

        notifications.Add(new ShowcaseNotification
        {
            Id = nextNotificationId++,
            AccountId = accountId,
            At = at,
            Title = Clip(title),
            Text = string.IsNullOrWhiteSpace(text) ? null : Clip(text),
            Url = url,
            IsRead = isRead,
        });

        if (notifications.Count > MaxNotifications)
            notifications.Remove(notifications.MinBy(notification => notification.At)!);
    }

    /// <summary>The time a ticket has spent waiting on the customer so far, including a wait still going on.</summary>
    private TimeSpan PausedSoFar(HelpDeskTicket ticket)
    {
        var paused = ticket.PausedMinutes;

        if (ticket.Status == HelpDeskStatus.Pending && ticket.PendingSince is { } since && Clock.Now > since)
            paused += (Clock.Now - since).TotalMinutes;

        return TimeSpan.FromMinutes(paused);
    }

    private HelpDeskTicket NewTicket(string customerName, string email, string? company, string categoryId, HelpDeskPriority priority, string subject, string description, string? accountId,
        string? openedBy = null)
    {
        var now = Clock.Now;
        var ticket = new HelpDeskTicket
        {
            Id = nextTicketId++,
            Subject = subject.Trim(),
            CustomerName = customerName.Trim(),
            CustomerEmail = email.Trim(),
            CustomerAccountId = accountId,
            Company = string.IsNullOrWhiteSpace(company) ? null : company.Trim(),
            CategoryId = FindCategory(categoryId).Id,
            Priority = priority,
            Status = HelpDeskStatus.New,
            CreatedAt = now,
        };
        ticket.Messages.Add(new HelpDeskMessage(ticket.CustomerName, IsAgent: false, IsInternalNote: false, IsRichText: false, description.Trim(), now));
        ticket.Activity.Add(openedBy is null
            ? new HelpDeskActivity(now, ticket.CustomerName, "Submitted the request")
            : new HelpDeskActivity(now, AgentName, openedBy));
        Tickets.Insert(0, ticket);

        if (Tickets.Count > MaxTickets)
            Tickets.Remove(Tickets.Where(other => other.Status == HelpDeskStatus.Resolved).OrderBy(other => other.CreatedAt).FirstOrDefault() ?? Tickets[^1]);

        return ticket;
    }

    /// <summary>The time for a new message: now, or a minute after the last message if that is later (keeps the thread in order).</summary>
    private DateTime NextTime(HelpDeskTicket ticket) => ticket.UpdatedAt >= Clock.Now ? ticket.UpdatedAt.AddMinutes(1) : Clock.Now;

    private HelpDeskTicket? OwnTicket(int ticketId) =>
        Identity.Current is { RoleId: HelpDeskSeedData.CustomerRole } account
            ? Tickets.FirstOrDefault(ticket => ticket.Id == ticketId && ticket.CustomerAccountId == account.Id)
            : null;

    private void SetStatusCore(HelpDeskTicket ticket, HelpDeskStatus status, string suffix = "")
    {
        Log(ticket, $"Status {ticket.Status} → {status}{suffix}");
        ApplyStatus(ticket, status);

        if (status != HelpDeskStatus.Resolved)
        {
            ticket.Satisfaction = null;
            ticket.SatisfactionComment = null;
        }
    }

    private static bool ValidTicket(HelpDeskTicket? ticket, int nextId, HashSet<string> categoryIds, HashSet<string> agentNames) =>
        ticket is not null && ticket.Id >= 1 && ticket.Id < nextId && IsText(ticket.Subject, MaxSubjectLength)
        && IsText(ticket.CustomerName, ShowcaseIdentity.MaxNameLength) && IsEmail(ticket.CustomerEmail) && (ticket.Company is null || IsText(ticket.Company, 80))
        && (ticket.CustomerAccountId is null || IsAccountId(ticket.CustomerAccountId)) && categoryIds.Contains(ticket.CategoryId)
        && Enum.IsDefined(ticket.Priority) && Enum.IsDefined(ticket.Status) && (ticket.Assignee is null || agentNames.Contains(ticket.Assignee))
        && ticket.PausedMinutes is >= 0 and <= MaxPausedMinutes && !double.IsNaN(ticket.PausedMinutes)
        && (ticket.Status == HelpDeskStatus.Pending ? ticket.PendingSince is { } pendingSince && IsPlausibleDate(pendingSince) && pendingSince >= ticket.CreatedAt : ticket.PendingSince is null)
        && IsPlausibleDate(ticket.CreatedAt)
        && (ticket.Status == HelpDeskStatus.Resolved ? ticket.ResolvedAt is { } resolvedAt && IsPlausibleDate(resolvedAt) : ticket.ResolvedAt is null)
        && (ticket.Satisfaction is null || (ticket.Status == HelpDeskStatus.Resolved && Enum.IsDefined(ticket.Satisfaction.Value)))
        && (ticket.SatisfactionComment is null || (ticket.Satisfaction is not null && IsText(ticket.SatisfactionComment, 500)))
        && ticket.Messages is { Count: >= 1 and <= MaxMessagesPerTicket }
        && ticket.Messages.All(message => message is not null && IsText(message.Author, ShowcaseIdentity.MaxNameLength) && IsPlausibleDate(message.PostedAt)
            && IsText(message.Body, message.IsRichText ? MaxEditorHtmlLength : MaxCustomerMessageLength) && (!message.IsInternalNote || message.IsAgent))
        && ticket.Activity is { Count: <= MaxActivityPerTicket }
        && ticket.Activity.All(entry => entry is not null && IsPlausibleDate(entry.At) && IsText(entry.Actor, ShowcaseIdentity.MaxNameLength) && IsText(entry.Text, 200));

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the names of the active agents, whom tickets can be assigned to; the signed-in agent first.</summary>
    public IReadOnlyList<string> Agents => agents.Where(agent => agent.IsActive).OrderBy(agent => agent.Name != AgentName).Select(agent => agent.Name).ToList();

    /// <summary>
    /// Gets the signed-in agent's name on the roster, which replies and assignments use. It follows an admin's rename on the
    /// roster, not a profile rename: assignees must stay on the roster, or a reload would reject the tab's data.
    /// </summary>
    public string AgentName => agents.FirstOrDefault(agent => agent.AccountId is not null && agent.AccountId == Identity.Current?.Id)?.Name ?? HelpDeskSeedData.AgentName;

    /// <summary>Gets every article, drafts included, the most recently updated first; empty for anyone but an agent.</summary>
    public IReadOnlyList<HelpDeskArticle> AllArticles => IsAgent ? articles.OrderByDescending(article => article.UpdatedAt).ToList() : Array.Empty<HelpDeskArticle>();

    /// <summary>Gets the audit log, newest first, for agents and admins; empty for anyone else.</summary>
    public IReadOnlyList<ShowcaseAuditEntry> AuditEntries =>
        IsAgent ? audit.OrderByDescending(entry => entry.At).ThenByDescending(entry => entry.Id).ToList() : Array.Empty<ShowcaseAuditEntry>();

    /// <summary>Gets the published knowledge base articles (the help center's).</summary>
    public IReadOnlyList<HelpDeskArticle> Articles => articles.Where(article => article.IsPublished).ToList();

    /// <summary>Gets the ticket and article categories, in order.</summary>
    public IReadOnlyList<HelpDeskCategory> Categories => categories;

    /// <summary>Gets the site's clock, which moves on from the seed time (D-029).</summary>
    public ShowcaseClock Clock { get; } = new(HelpDeskSeedData.Now);

    /// <summary>Gets the SLA escalation rules, in the order they were added.</summary>
    public IReadOnlyList<HelpDeskEscalationRule> EscalationRules => escalationRules;

    /// <summary>Gets the agent queue's filter (kept in memory for the tab).</summary>
    public HelpDeskFilter Filter { get; } = new();

    /// <summary>Gets the site's demo sign-in.</summary>
    public ShowcaseIdentity Identity { get; }

    /// <summary>Gets whether the signed-in account is a help desk admin.</summary>
    public bool IsAdmin => Identity.Current?.RoleId == HelpDeskSeedData.AdminRole;

    /// <summary>Gets whether the signed-in account works tickets: a support agent or an admin.</summary>
    public bool IsAgent => Identity.Current?.RoleId is HelpDeskSeedData.AgentRole or HelpDeskSeedData.AdminRole;

    /// <summary>Gets whether the signed-in account is a customer.</summary>
    public bool IsCustomer => Identity.Current?.RoleId == HelpDeskSeedData.CustomerRole;

    /// <summary>Gets the site's current time.</summary>
    public DateTime Now => Clock.Now;

    /// <summary>Gets the whole roster, active and inactive, by name.</summary>
    public IReadOnlyList<HelpDeskAgent> Roster => agents.OrderBy(agent => agent.Name, StringComparer.OrdinalIgnoreCase).ToList();

    /// <summary>Gets the agents' saved replies, by title.</summary>
    public IReadOnlyList<HelpDeskSavedReply> SavedReplies => savedReplies.OrderBy(reply => reply.Title, StringComparer.OrdinalIgnoreCase).ToList();

    /// <summary>Gets the site's slug, used in the storage key.</summary>
    public string SiteSlug => "help-desk";

    /// <summary>Gets the SLA policies, Low to Urgent.</summary>
    public IReadOnlyList<HelpDeskSlaPolicy> SlaPolicies => slaPolicies;

    /// <summary>Gets the snapshot format version (4: escalation rules, ticket tiers, and team leads).</summary>
    public int SnapshotVersion => 4;

    /// <summary>Gets the teams, in order.</summary>
    public IReadOnlyList<HelpDeskTeam> Teams => teams;

    /// <summary>Gets every ticket, newest first.</summary>
    public List<HelpDeskTicket> Tickets { get; } = HelpDeskSeedData.Tickets().OrderByDescending(ticket => ticket.CreatedAt).ToList();

    /// <summary>Gets how many of the signed-in customer's requests have an agent reply they haven't opened.</summary>
    public int UnreadRequestCount => IsCustomer ? Tickets.Count(ticket => ticket.CustomerAccountId == Identity.Current!.Id && ticket.CustomerUnread) : 0;

    #endregion

    #region Events

    /// <summary>Raised when anything in the state changes, including sign-in.</summary>
    public event Action? Changed;

    #endregion

    #region Nested Types

    /// <summary>The help desk's part of the tab snapshot.</summary>
    private sealed class HelpDeskSnapshot
    {
        #region Properties, Indexers

        public List<HelpDeskAgent>? Agents { get; set; }

        public List<HelpDeskArticle>? Articles { get; set; }

        public List<ShowcaseAuditEntry>? Audit { get; set; }

        public List<HelpDeskCategory>? Categories { get; set; }

        public List<HelpDeskEscalationRule>? EscalationRules { get; set; }

        public int NextAgentId { get; set; }

        public int NextEscalationRuleId { get; set; }

        public int NextArticleId { get; set; }

        public int NextAuditId { get; set; }

        public int NextCategoryId { get; set; }

        public int NextNotificationId { get; set; }

        public int NextSavedReplyId { get; set; }

        public int NextTeamId { get; set; }

        public int NextTicketId { get; set; }

        public List<ShowcaseNotification>? Notifications { get; set; }

        public List<HelpDeskSavedReply>? SavedReplies { get; set; }

        public List<HelpDeskSlaPolicy>? SlaPolicies { get; set; }

        public List<HelpDeskTeam>? Teams { get; set; }

        public List<HelpDeskTicket>? Tickets { get; set; }

        public List<HelpDeskArticleVotes>? Votes { get; set; }

        #endregion
    }

    #endregion
}
