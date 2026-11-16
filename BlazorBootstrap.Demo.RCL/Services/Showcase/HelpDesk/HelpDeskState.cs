using System.Globalization;
using System.Text.Json;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

/// <summary>
/// State of the "Beacon Desk" help desk for the current browser tab, shared by the help center (customers) and the agent
/// desk: tickets with their threads and activity, saved replies, article votes, and the demo sign-in. It is kept in the
/// tab's <c>sessionStorage</c> through the showcase foundation, and its clock moves on from the seed time (D-029), so SLA
/// countdowns run; nothing is sent.
/// </summary>
public sealed class HelpDeskState : IShowcaseSiteState
{
    #region Fields and Constants

    /// <summary>The longest customer message.</summary>
    public const int MaxCustomerMessageLength = 4000;

    /// <summary>The longest ticket subject.</summary>
    public const int MaxSubjectLength = 120;

    private const int MaxActivityPerTicket = 300;

    private const int MaxEditorHtmlLength = 20_000;

    private const int MaxMessagesPerTicket = 200;

    private const int MaxSavedReplies = 100;

    private const int MaxTickets = 1000;

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<HelpDeskSavedReply> savedReplies = HelpDeskSeedData.SavedReplies();

    private readonly Dictionary<int, (int Helpful, int NotHelpful)> votes = new();

    private int nextSavedReplyId;

    private int nextTicketId;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out.</summary>
    public HelpDeskState()
    {
        nextTicketId = Tickets.Max(ticket => ticket.Id) + 1;
        nextSavedReplyId = savedReplies.Max(reply => reply.Id) + 1;
        Identity = new ShowcaseIdentity(HelpDeskSeedData.Roles(), HelpDeskSeedData.DemoAccounts());
        Identity.Changed += () => Changed?.Invoke();
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns when a ticket's SLA is due: its creation plus the target hours of its priority.</summary>
    public static DateTime DueAt(HelpDeskTicket ticket) => ticket.CreatedAt.AddHours(TargetHours(ticket.Priority));

    /// <summary>Returns the hours to the first resolution for a priority.</summary>
    public static int TargetHours(HelpDeskPriority priority) => priority switch
    {
        HelpDeskPriority.Urgent => 4,
        HelpDeskPriority.High => 8,
        HelpDeskPriority.Normal => 24,
        _ => 72,
    };

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
                ticket.Status = HelpDeskStatus.Pending;
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

    /// <summary>Returns the snapshot of the help desk: tickets, saved replies, article votes, and the ID counters.</summary>
    public object CaptureData() => new HelpDeskSnapshot
    {
        NextSavedReplyId = nextSavedReplyId,
        NextTicketId = nextTicketId,
        SavedReplies = savedReplies.ToList(),
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

    /// <summary>Deletes a saved reply (agents).</summary>
    public bool DeleteSavedReply(int replyId)
    {
        if (!IsAgent || savedReplies.RemoveAll(reply => reply.Id == replyId) == 0)
            return false;

        Changed?.Invoke();

        return true;
    }

    /// <summary>Returns an article by its ID, or <see langword="null" />.</summary>
    public HelpDeskArticle? FindArticle(int articleId) => Articles.FirstOrDefault(article => article.Id == articleId);

    /// <summary>Returns a category by its ID, or the first category.</summary>
    public HelpDeskCategory FindCategory(string? categoryId) => Categories.FirstOrDefault(category => category.Id == categoryId) ?? Categories[0];

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

        ticket.Status = HelpDeskStatus.Resolved;
        ticket.ResolvedAt = Clock.Now;
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
            ticket.Status = HelpDeskStatus.Open;
            ticket.ResolvedAt = null;
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
        if (!IsAgent || Categories.All(category => category.Id != categoryId) || ticket.CategoryId == categoryId)
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
            return HelpDeskSlaState.Paused;

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
            }
            || snapshot.NextTicketId is < 2 or > 1_000_000
            || snapshot.NextSavedReplyId is < 1 or > 100_000)
            return false;

        if (storedTickets.Any(ticket => !ValidTicket(ticket, snapshot.NextTicketId)) || storedTickets.Select(ticket => ticket.Id).Distinct().Count() != storedTickets.Count)
            return false;

        if (storedReplies.Any(reply => reply is null || reply.Id < 1 || reply.Id >= snapshot.NextSavedReplyId || !IsText(reply.Title, 60) || !IsText(reply.Html, 5000))
            || storedReplies.Select(reply => reply.Id).Distinct().Count() != storedReplies.Count)
            return false;

        if (storedVotes.Any(vote => vote is null || FindArticle(vote.ArticleId) is null || vote.Helpful is < 0 or > 1_000_000 || vote.NotHelpful is < 0 or > 1_000_000)
            || storedVotes.Select(vote => vote.ArticleId).Distinct().Count() != storedVotes.Count)
            return false;

        Tickets.Clear();
        Tickets.AddRange(storedTickets.OrderByDescending(ticket => ticket.CreatedAt));
        savedReplies.Clear();
        savedReplies.AddRange(storedReplies);
        votes.Clear();

        foreach (var vote in storedVotes)
            votes[vote.ArticleId] = (vote.Helpful, vote.NotHelpful);

        nextTicketId = snapshot.NextTicketId;
        nextSavedReplyId = snapshot.NextSavedReplyId;

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
        ticket.Status = status;
        ticket.ResolvedAt = status == HelpDeskStatus.Resolved ? Clock.Now : null;

        if (status != HelpDeskStatus.Resolved)
        {
            ticket.Satisfaction = null;
            ticket.SatisfactionComment = null;
        }
    }

    private bool ValidTicket(HelpDeskTicket? ticket, int nextId) =>
        ticket is not null && ticket.Id >= 1 && ticket.Id < nextId && IsText(ticket.Subject, MaxSubjectLength)
        && IsText(ticket.CustomerName, ShowcaseIdentity.MaxNameLength) && IsEmail(ticket.CustomerEmail) && (ticket.Company is null || IsText(ticket.Company, 80))
        && (ticket.CustomerAccountId is null || IsAccountId(ticket.CustomerAccountId)) && Categories.Any(category => category.Id == ticket.CategoryId)
        && Enum.IsDefined(ticket.Priority) && Enum.IsDefined(ticket.Status) && (ticket.Assignee is null || Agents.Contains(ticket.Assignee))
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

    /// <summary>Gets the agents tickets can be assigned to; the demo agent first.</summary>
    public IReadOnlyList<string> Agents { get; } = HelpDeskSeedData.Agents();

    /// <summary>
    /// Gets the signed-in agent's name on the team roster, which replies and assignments use. It doesn't follow a profile
    /// rename: assignees must stay on the roster (<see cref="Agents" />), or a reload would reject the tab's data.
    /// </summary>
    public string AgentName => HelpDeskSeedData.AgentName;

    /// <summary>Gets the knowledge base articles.</summary>
    public IReadOnlyList<HelpDeskArticle> Articles { get; } = HelpDeskSeedData.Articles();

    /// <summary>Gets the ticket and article categories.</summary>
    public IReadOnlyList<HelpDeskCategory> Categories { get; } = HelpDeskSeedData.Categories();

    /// <summary>Gets the site's clock, which moves on from the seed time (D-029).</summary>
    public ShowcaseClock Clock { get; } = new(HelpDeskSeedData.Now);

    /// <summary>Gets the agent queue's filter (kept in memory for the tab).</summary>
    public HelpDeskFilter Filter { get; } = new();

    /// <summary>Gets the site's demo sign-in.</summary>
    public ShowcaseIdentity Identity { get; }

    /// <summary>Gets whether the signed-in account is a support agent.</summary>
    public bool IsAgent => Identity.Current?.RoleId == HelpDeskSeedData.AgentRole;

    /// <summary>Gets whether the signed-in account is a customer.</summary>
    public bool IsCustomer => Identity.Current?.RoleId == HelpDeskSeedData.CustomerRole;

    /// <summary>Gets the site's current time.</summary>
    public DateTime Now => Clock.Now;

    /// <summary>Gets the agents' saved replies, by title.</summary>
    public IReadOnlyList<HelpDeskSavedReply> SavedReplies => savedReplies.OrderBy(reply => reply.Title, StringComparer.OrdinalIgnoreCase).ToList();

    /// <summary>Gets the site's slug, used in the storage key.</summary>
    public string SiteSlug => "help-desk";

    /// <summary>Gets the snapshot format version.</summary>
    public int SnapshotVersion => 1;

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

        public int NextSavedReplyId { get; set; }

        public int NextTicketId { get; set; }

        public List<HelpDeskSavedReply>? SavedReplies { get; set; }

        public List<HelpDeskTicket>? Tickets { get; set; }

        public List<HelpDeskArticleVotes>? Votes { get; set; }

        #endregion
    }

    #endregion
}
