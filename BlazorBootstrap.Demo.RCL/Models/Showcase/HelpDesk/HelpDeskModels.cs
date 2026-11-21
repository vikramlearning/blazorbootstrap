namespace BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;

/// <summary>Urgency of a ticket; it sets the SLA target.</summary>
public enum HelpDeskPriority
{
    /// <summary>Resolve within 72 hours.</summary>
    Low,

    /// <summary>Resolve within 24 hours.</summary>
    Normal,

    /// <summary>Resolve within 8 hours.</summary>
    High,

    /// <summary>Resolve within 4 hours.</summary>
    Urgent,
}

/// <summary>Where a ticket is in its life.</summary>
public enum HelpDeskStatus
{
    /// <summary>Not answered yet.</summary>
    New,

    /// <summary>Being worked on.</summary>
    Open,

    /// <summary>Waiting for the customer; the SLA clock is paused.</summary>
    Pending,

    /// <summary>Done.</summary>
    Resolved,
}

/// <summary>State of a ticket against its SLA target.</summary>
public enum HelpDeskSlaState
{
    /// <summary>More than two hours, and more than a quarter of the target, left.</summary>
    OnTrack,

    /// <summary>Two hours or less, or under a quarter of the target, left.</summary>
    AtRisk,

    /// <summary>Past the due time.</summary>
    Breached,

    /// <summary>Waiting for the customer.</summary>
    Paused,

    /// <summary>Resolved before the due time.</summary>
    Met,

    /// <summary>Resolved after the due time.</summary>
    Missed,
}

/// <summary>A ticket and knowledge base category. New requests in it go to <see cref="TeamId" />'s queue until someone is assigned.</summary>
public sealed record HelpDeskCategory(string Id, string Name, IconName IconName, string TeamId);

/// <summary>
/// A team of agents (a group, in help desk terms). <see cref="LeadAgentId" /> is the team lead, an active agent on the team,
/// who is notified when one of the team's tickets is escalated; <see langword="null" /> when the team has no lead.
/// </summary>
public sealed record HelpDeskTeam(string Id, string Name, string Description, string? LeadAgentId = null);

/// <summary>What an escalation rule watches on an unresolved ticket.</summary>
public enum HelpDeskEscalationTrigger
{
    /// <summary>No public agent reply yet, the set number of hours after the ticket was created.</summary>
    NoFirstReply,

    /// <summary>Still not resolved after the set number of hours on the SLA clock (time waiting on the customer doesn't count).</summary>
    NotResolved,
}

/// <summary>
/// An SLA escalation rule (AC-7a): an unresolved ticket of <see cref="Priority" /> (any priority when <see langword="null" />)
/// that meets <see cref="Trigger" /> after <see cref="Hours" /> moves up one support tier, is flagged in the queue, and its
/// team lead is notified. Each rule escalates a ticket once.
/// </summary>
public sealed record HelpDeskEscalationRule(int Id, string Name, HelpDeskPriority? Priority, HelpDeskEscalationTrigger Trigger, int Hours, bool IsActive);

/// <summary>
/// An agent on the support roster. Tickets are assigned by <see cref="Name" />. <see cref="AccountId" /> links the roster
/// entry to a demo account that can sign in; agents an admin adds have none.
/// </summary>
public sealed record HelpDeskAgent(string Id, string Name, string Email, string TeamId, bool IsAdmin, bool IsActive, string? AccountId);

/// <summary>The SLA targets of one priority: hours to the first public reply, and hours to resolution (paused while pending).</summary>
public sealed record HelpDeskSlaPolicy(HelpDeskPriority Priority, int FirstReplyHours, int ResolveHours);

/// <summary>The outcome of an admin or knowledge base change.</summary>
public enum HelpDeskAdminResult
{
    /// <summary>The change was made.</summary>
    Ok,

    /// <summary>The signed-in account may not make it.</summary>
    NotAllowed,

    /// <summary>The item no longer exists.</summary>
    NotFound,

    /// <summary>A value is missing, too long, or out of range.</summary>
    Invalid,

    /// <summary>Another item already has that name or email.</summary>
    Duplicate,

    /// <summary>Something still uses the item (agents in a team, tickets in a category).</summary>
    InUse,

    /// <summary>The limit of items is reached.</summary>
    LimitReached,

    /// <summary>Nothing changed.</summary>
    NoChange,
}

/// <summary>
/// A day of archived tickets for one agent and priority (closed tickets no longer in the queue), so reports have a history
/// before the live tickets. Generated from the seed, never stored.
/// </summary>
public sealed record HelpDeskArchiveDay(DateOnly Date, string AgentId, HelpDeskPriority Priority, int Created, int Resolved, int ResolvedInSla, int FirstReplyInSla, int Good, int Bad);

/// <summary>A satisfaction comment shown in the satisfaction report.</summary>
public sealed record HelpDeskRatingComment(DateTime At, int? TicketId, string Customer, string Agent, HelpDeskSatisfaction Satisfaction, string Comment);

/// <summary>
/// A message in a ticket's thread. Customer messages and ticket descriptions are plain text; agent replies are rich
/// text from the editor and are always rendered through <c>ShowcaseHtml.FromEditorHtml</c>. Flags that are off are left out of
/// the tab's snapshot, to keep it small.
/// </summary>
public sealed record HelpDeskMessage(string Author, [property: System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)] bool IsAgent,
    [property: System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)] bool IsInternalNote, [property: System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)] bool IsRichText, string Body, DateTime PostedAt);

/// <summary>One entry in a ticket's activity log.</summary>
public sealed record HelpDeskActivity(DateTime At, string Actor, string Text);

/// <summary>A customer's rating of how a resolved ticket was handled.</summary>
public enum HelpDeskSatisfaction
{
    /// <summary>Good, I'm satisfied.</summary>
    Good,

    /// <summary>Bad, I'm unsatisfied.</summary>
    Bad,
}

/// <summary>A saved reply (macro) an agent inserts into a reply. <see cref="Html" /> is editor HTML, shown through <c>ShowcaseHtml</c>.</summary>
public sealed record HelpDeskSavedReply(int Id, string Title, string Html);

/// <summary>Helpful and not-helpful votes on one article.</summary>
public sealed record HelpDeskArticleVotes(int ArticleId, int Helpful, int NotHelpful);

/// <summary>A support ticket. Empty and default values are left out of the tab's snapshot, to keep it small.</summary>
public sealed class HelpDeskTicket
{
    #region Properties, Indexers

    /// <summary>Gets or sets the agent the ticket is assigned to, or <see langword="null" /> when unassigned.</summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public string? Assignee { get; set; }

    /// <summary>Gets or sets the category ID.</summary>
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>Gets the customer's company, if any.</summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public string? Company { get; init; }

    /// <summary>Gets when the ticket was created.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets the customer's demo account, or <see langword="null" /> for a customer who wrote in by email.</summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public string? CustomerAccountId { get; init; }

    /// <summary>Gets or sets whether the customer has an agent reply they haven't opened yet.</summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public bool CustomerUnread { get; set; }

    /// <summary>Gets the customer's email address.</summary>
    public string CustomerEmail { get; init; } = string.Empty;

    /// <summary>Gets the customer's name.</summary>
    public string CustomerName { get; init; } = string.Empty;

    /// <summary>Gets the ticket number.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the escalation rules that have escalated the ticket, or <see langword="null" /> when none has.</summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public List<int>? EscalatedByRuleIds { get; set; }

    /// <summary>Gets or sets how many tiers the ticket has been escalated: 0 (Tier 1, not escalated) to 2 (Tier 3).</summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public int EscalationLevel { get; set; }

    /// <summary>Gets whether the ticket has been escalated above Tier 1.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsEscalated => EscalationLevel > 0;

    /// <summary>Gets the support tier the ticket is at: 1 to 3.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public int Tier => EscalationLevel + 1;

    /// <summary>Gets the activity log, oldest first: status, priority, category, assignee, and escalation changes and who made them.</summary>
    public List<HelpDeskActivity> Activity { get; init; } = new();

    /// <summary>Gets the thread, oldest first.</summary>
    public List<HelpDeskMessage> Messages { get; init; } = new();

    /// <summary>Gets or sets the minutes the ticket has spent waiting on the customer (Pending) so far; the SLA pauses for them.</summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public double PausedMinutes { get; set; }

    /// <summary>Gets or sets when the ticket went Pending, while it is Pending.</summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime? PendingSince { get; set; }

    /// <summary>Gets or sets the priority.</summary>
    public HelpDeskPriority Priority { get; set; }

    /// <summary>Gets or sets when the ticket was resolved.</summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime? ResolvedAt { get; set; }

    /// <summary>Gets or sets the customer's rating of a resolved ticket.</summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public HelpDeskSatisfaction? Satisfaction { get; set; }

    /// <summary>Gets or sets the customer's comment with the rating.</summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public string? SatisfactionComment { get; set; }

    /// <summary>Gets or sets the status.</summary>
    public HelpDeskStatus Status { get; set; }

    /// <summary>Gets the subject line.</summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>Gets when the last message the customer can see was posted (internal notes don't count), or the creation time.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public DateTime CustomerUpdatedAt => Messages.LastOrDefault(message => !message.IsInternalNote)?.PostedAt ?? CreatedAt;

    /// <summary>Gets when an agent first replied publicly, or <see langword="null" /> before that.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public DateTime? FirstReplyAt => Messages.FirstOrDefault(message => message.IsAgent && !message.IsInternalNote)?.PostedAt;

    /// <summary>Gets when the last message was posted, or the creation time.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public DateTime UpdatedAt => Messages.Count == 0 ? CreatedAt : Messages[^1].PostedAt;

    #endregion
}

/// <summary>
/// A knowledge base article. <see cref="Html" /> is editor HTML (seeded or written by an agent) and is always rendered
/// through <c>ShowcaseHtml.FromEditorHtml</c> (A05). Drafts show only to agents.
/// </summary>
public sealed record HelpDeskArticle(int Id, string Title, string CategoryId, string Summary, string Html, DateTime UpdatedAt, string UpdatedBy, int Views, bool IsPublished)
{
    #region Properties, Indexers

    /// <summary>Gets the article text without markup, for search and reading time.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string PlainText => System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(Html, "<[^>]*>", " "));

    /// <summary>Gets the reading time in minutes, at about 200 words a minute.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public int ReadingMinutes => Math.Max(1, PlainText.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length / 200);

    #endregion
}

/// <summary>Queue filters, kept in the site state so they survive opening a ticket and coming back.</summary>
public sealed class HelpDeskFilter
{
    #region Fields and Constants

    /// <summary>The status filter value for every ticket.</summary>
    public const string StatusAll = "all";

    /// <summary>The status filter value for every ticket that is not resolved.</summary>
    public const string StatusAllOpen = "open-all";

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets whether only the signed-in agent's tickets are shown.</summary>
    public bool AssignedToMe { get; set; }

    /// <summary>Gets or sets whether only escalated tickets (Tier 2 or 3) are shown.</summary>
    public bool EscalatedOnly { get; set; }

    /// <summary>Gets whether any filter narrows the queue beyond the default (every ticket that is not resolved).</summary>
    public bool IsActive => AssignedToMe || EscalatedOnly || Priority.Length > 0 || TeamId.Length > 0 || Status != StatusAllOpen || !string.IsNullOrWhiteSpace(Search);

    /// <summary>Gets or sets the priority name; empty means every priority.</summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>Gets or sets the search text, matched against the number, subject, customer, and company.</summary>
    public string Search { get; set; } = string.Empty;

    /// <summary>Gets or sets the status filter: a status name, <see cref="StatusAllOpen" />, or <see cref="StatusAll" />.</summary>
    public string Status { get; set; } = StatusAllOpen;

    /// <summary>Gets or sets the team ID; empty means every team. A ticket's team is its assignee's, or its category's while unassigned.</summary>
    public string TeamId { get; set; } = string.Empty;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Resets every filter.</summary>
    public void Clear()
    {
        AssignedToMe = false;
        EscalatedOnly = false;
        Priority = string.Empty;
        Search = string.Empty;
        Status = StatusAllOpen;
        TeamId = string.Empty;
    }

    #endregion

    #endregion
}
