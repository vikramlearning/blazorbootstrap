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
/// </summary>
public sealed class HelpDeskState : IShowcaseSiteState
{
    #region Fields and Constants

    /// <summary>The longest article body, in editor HTML.</summary>
    public const int MaxArticleHtmlLength = 20_000;

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

    private readonly List<HelpDeskCategory> categories = HelpDeskSeedData.Categories();

    private readonly List<HelpDeskSavedReply> savedReplies = HelpDeskSeedData.SavedReplies();

    private readonly List<HelpDeskSlaPolicy> slaPolicies = HelpDeskSeedData.SlaPolicies();

    private readonly List<HelpDeskTeam> teams = HelpDeskSeedData.Teams();

    private readonly Dictionary<int, (int Helpful, int NotHelpful)> votes = new();

    private int nextAgentId = 1;

    private int nextArticleId;

    private int nextCategoryId = 1;

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
        Identity = new ShowcaseIdentity(HelpDeskSeedData.Roles(), HelpDeskSeedData.DemoAccounts());
        Identity.Changed += () => Changed?.Invoke();
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

        Changed?.Invoke();

        return true;
    }

    /// <summary>Assigns a ticket to an agent, or unassigns it; assigning a new ticket opens it.</summary>
    public bool Assign(HelpDeskTicket ticket, string? agent)
    {
        if (!IsAgent || (agent is not null && !Agents.Contains(agent)) || ticket.Assignee == agent)
            return false;

        ticket.Assignee = agent;
        Log(ticket, agent is null ? "Unassigned" : $"Assigned to {agent}");

        if (agent is not null && ticket.Status == HelpDeskStatus.New)
            SetStatusCore(ticket, HelpDeskStatus.Open);

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

        foreach (var ticket in ticketIds.Distinct().Select(FindTicket).OfType<HelpDeskTicket>())
        {
            var before = (ticket.Assignee, ticket.Status, ticket.Priority);

            if (unassign && ticket.Assignee is not null)
            {
                ticket.Assignee = null;
                Log(ticket, "Unassigned (bulk)");
            }
            else if (assignee is not null && ticket.Assignee != assignee)
            {
                ticket.Assignee = assignee;
                Log(ticket, $"Assigned to {assignee} (bulk)");
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

            if (before != (ticket.Assignee, ticket.Status, ticket.Priority))
                changed++;
        }

        if (changed > 0)
            Changed?.Invoke();

        return changed;
    }

    /// <summary>Returns the snapshot of the help desk: tickets, saved replies, articles and votes, the admin settings, and the ID counters.</summary>
    public object CaptureData() => new HelpDeskSnapshot
    {
        Agents = agents.ToList(),
        Articles = articles.ToList(),
        Categories = categories.ToList(),
        NextAgentId = nextAgentId,
        NextArticleId = nextArticleId,
        NextCategoryId = nextCategoryId,
        NextSavedReplyId = nextSavedReplyId,
        NextTeamId = nextTeamId,
        NextTicketId = nextTicketId,
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
        Changed?.Invoke();

        return ticket;
    }

    /// <summary>Deletes an article (agents), with its votes.</summary>
    public HelpDeskAdminResult DeleteArticle(int articleId)
    {
        if (!IsAgent)
            return HelpDeskAdminResult.NotAllowed;

        if (articles.RemoveAll(article => article.Id == articleId) == 0)
            return HelpDeskAdminResult.NotFound;

        votes.Remove(articleId);
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

        Changed?.Invoke();

        return HelpDeskAdminResult.Ok;
    }

    /// <summary>Deletes a saved reply (agents).</summary>
    public bool DeleteSavedReply(int replyId)
    {
        if (!IsAgent || savedReplies.RemoveAll(reply => reply.Id == replyId) == 0)
            return false;

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

    /// <summary>Marks one of the signed-in customer's requests as solved.</summary>
    public bool MarkSolved(int ticketId)
    {
        if (OwnTicket(ticketId) is not { } ticket || ticket.Status == HelpDeskStatus.Resolved)
            return false;

        ApplyStatus(ticket, HelpDeskStatus.Resolved);
        Log(ticket, "Marked as solved by the customer");
        Changed?.Invoke();

        return true;
    }

    /// <summary>Returns the signed-in customer's requests, the most recently updated first.</summary>
    public IReadOnlyList<HelpDeskTicket> MyRequests() =>
        Identity.Current is { RoleId: HelpDeskSeedData.CustomerRole } account
            ? Tickets.Where(ticket => ticket.CustomerAccountId == account.Id).OrderByDescending(ticket => ticket.CustomerUpdatedAt).ToList()
            : Array.Empty<HelpDeskTicket>();

    /// <summary>Raises <see cref="Changed" /> after a snapshot was restored.</summary>
    public void NotifyRestored() => Changed?.Invoke();

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

        SetStatusCore(ticket, HelpDeskStatus.Open);
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

        Changed?.Invoke();

        return true;
    }

    /// <summary>Resolves a ticket (agents).</summary>
    public bool Resolve(HelpDeskTicket ticket)
    {
        if (!IsAgent || ticket.Status == HelpDeskStatus.Resolved)
            return false;

        if (ticket.Assignee is null)
        {
            ticket.Assignee = AgentName;
            Log(ticket, $"Assigned to {AgentName}");
        }

        SetStatusCore(ticket, HelpDeskStatus.Resolved);
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
            Changed?.Invoke();

            return HelpDeskAdminResult.Ok;
        }

        var index = articles.FindIndex(article => article.Id == articleId);

        if (index < 0)
            return HelpDeskAdminResult.NotFound;

        saved = articles[index] with { Title = title, CategoryId = categoryId, Summary = summary, Html = html, IsPublished = publish, UpdatedAt = Clock.Now, UpdatedBy = AgentName };
        articles[index] = saved;
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
        }
        else
        {
            saved = categories[index] with { Name = name, IconName = iconName, TeamId = teamId };

            if (saved == categories[index])
                return HelpDeskAdminResult.NoChange;

            categories[index] = saved;
        }

        Changed?.Invoke();

        return HelpDeskAdminResult.Ok;
    }

    /// <summary>Adds a team (<paramref name="teamId" /> <see langword="null" />) or edits one (admins). Names are unique.</summary>
    public HelpDeskAdminResult SaveTeam(string? teamId, string name, string? description, out HelpDeskTeam? saved)
    {
        saved = null;

        if (!IsAdmin)
            return HelpDeskAdminResult.NotAllowed;

        name = name?.Trim() ?? string.Empty;
        description = description?.Trim() ?? string.Empty;

        if (!IsText(name, MaxGroupNameLength) || description.Length > MaxTeamDescriptionLength)
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
        }
        else
        {
            saved = teams[index] with { Name = name, Description = description };

            if (saved == teams[index])
                return HelpDeskAdminResult.NoChange;

            teams[index] = saved;
        }

        Changed?.Invoke();

        return HelpDeskAdminResult.Ok;
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

        if (!active)
        {
            foreach (var ticket in Tickets.Where(ticket => ticket.Assignee == agent.Name && ticket.Status != HelpDeskStatus.Resolved))
            {
                ticket.Assignee = null;
                Log(ticket, $"Unassigned: {agent.Name} was deactivated");
                unassigned++;
            }
        }

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

        if (slaPolicies[index] == updated)
            return HelpDeskAdminResult.NoChange;

        slaPolicies[index] = updated;
        Changed?.Invoke();

        return HelpDeskAdminResult.Ok;
    }

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
            Changed?.Invoke();

            return added;
        }

        var index = savedReplies.FindIndex(reply => reply.Id == replyId);

        if (index < 0)
            return null;

        savedReplies[index] = new HelpDeskSavedReply(replyId, title!.Trim(), html);
        Changed?.Invoke();

        return savedReplies[index];
    }

    /// <summary>Changes a ticket's category (agents).</summary>
    public bool SetCategory(HelpDeskTicket ticket, string categoryId)
    {
        if (!IsAgent || categories.All(category => category.Id != categoryId) || ticket.CategoryId == categoryId)
            return false;

        Log(ticket, $"Category {FindCategory(ticket.CategoryId).Name} → {FindCategory(categoryId).Name}");
        ticket.CategoryId = categoryId;
        Changed?.Invoke();

        return true;
    }

    /// <summary>Changes a ticket's priority (agents); the SLA due time follows the new priority.</summary>
    public bool SetPriority(HelpDeskTicket ticket, HelpDeskPriority priority)
    {
        if (!IsAgent || !Enum.IsDefined(priority) || ticket.Priority == priority)
            return false;

        Log(ticket, $"Priority {ticket.Priority} → {priority}");
        ticket.Priority = priority;
        Changed?.Invoke();

        return true;
    }

    /// <summary>Changes a ticket's status (agents).</summary>
    public bool SetStatus(HelpDeskTicket ticket, HelpDeskStatus status)
    {
        if (!IsAgent || !Enum.IsDefined(status) || ticket.Status == status)
            return false;

        SetStatusCore(ticket, status);
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
            }
            || snapshot.NextTicketId is < 2 or > 1_000_000
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

        if (storedReplies.Any(reply => reply is null || reply.Id < 1 || reply.Id >= snapshot.NextSavedReplyId || !IsText(reply.Title, 60) || !IsText(reply.Html, 5000))
            || storedReplies.Select(reply => reply.Id).Distinct().Count() != storedReplies.Count)
            return false;

        if (storedVotes.Any(vote => vote is null || storedArticles.All(article => article.Id != vote.ArticleId) || vote.Helpful is < 0 or > 1_000_000 || vote.NotHelpful is < 0 or > 1_000_000)
            || storedVotes.Select(vote => vote.ArticleId).Distinct().Count() != storedVotes.Count)
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

        nextTicketId = snapshot.NextTicketId;
        nextSavedReplyId = snapshot.NextSavedReplyId;
        nextArticleId = snapshot.NextArticleId;
        nextAgentId = snapshot.NextAgentId;
        nextCategoryId = snapshot.NextCategoryId;
        nextTeamId = snapshot.NextTeamId;

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

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

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

    private void Log(HelpDeskTicket ticket, string text)
    {
        ticket.Activity.Add(new HelpDeskActivity(Clock.Now, Identity.Current?.Name ?? ticket.CustomerName, text));

        if (ticket.Activity.Count > MaxActivityPerTicket)
            ticket.Activity.RemoveAt(0);
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

    /// <summary>Gets the published knowledge base articles (the help center's).</summary>
    public IReadOnlyList<HelpDeskArticle> Articles => articles.Where(article => article.IsPublished).ToList();

    /// <summary>Gets the ticket and article categories, in order.</summary>
    public IReadOnlyList<HelpDeskCategory> Categories => categories;

    /// <summary>Gets the site's clock, which moves on from the seed time (D-029).</summary>
    public ShowcaseClock Clock { get; } = new(HelpDeskSeedData.Now);

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

    /// <summary>Gets the snapshot format version (2: admin settings, articles, and the SLA pause).</summary>
    public int SnapshotVersion => 2;

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

        public List<HelpDeskCategory>? Categories { get; set; }

        public int NextAgentId { get; set; }

        public int NextArticleId { get; set; }

        public int NextCategoryId { get; set; }

        public int NextSavedReplyId { get; set; }

        public int NextTeamId { get; set; }

        public int NextTicketId { get; set; }

        public List<HelpDeskSavedReply>? SavedReplies { get; set; }

        public List<HelpDeskSlaPolicy>? SlaPolicies { get; set; }

        public List<HelpDeskTeam>? Teams { get; set; }

        public List<HelpDeskTicket>? Tickets { get; set; }

        public List<HelpDeskArticleVotes>? Votes { get; set; }

        #endregion
    }

    #endregion
}
