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

/// <summary>A ticket and knowledge base category.</summary>
public sealed record HelpDeskCategory(string Id, string Name, IconName IconName);

/// <summary>
/// A message in a ticket's thread. Customer messages and ticket descriptions are plain text; agent replies are rich
/// text from the editor and are always rendered through <c>ShowcaseHtml.FromEditorHtml</c>.
/// </summary>
public sealed record HelpDeskMessage(string Author, bool IsAgent, bool IsInternalNote, bool IsRichText, string Body, DateTime PostedAt);

/// <summary>A support ticket.</summary>
public sealed class HelpDeskTicket
{
    #region Properties, Indexers

    /// <summary>Gets or sets the agent the ticket is assigned to, or <see langword="null" /> when unassigned.</summary>
    public string? Assignee { get; set; }

    /// <summary>Gets the category ID.</summary>
    public string CategoryId { get; init; } = string.Empty;

    /// <summary>Gets the customer's company, if any.</summary>
    public string? Company { get; init; }

    /// <summary>Gets when the ticket was created.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets the customer's email address.</summary>
    public string CustomerEmail { get; init; } = string.Empty;

    /// <summary>Gets the customer's name.</summary>
    public string CustomerName { get; init; } = string.Empty;

    /// <summary>Gets the ticket number.</summary>
    public int Id { get; init; }

    /// <summary>Gets the thread, oldest first.</summary>
    public List<HelpDeskMessage> Messages { get; init; } = new();

    /// <summary>Gets or sets the priority.</summary>
    public HelpDeskPriority Priority { get; set; }

    /// <summary>Gets or sets when the ticket was resolved.</summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>Gets or sets the status.</summary>
    public HelpDeskStatus Status { get; set; }

    /// <summary>Gets the subject line.</summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>Gets when the last message was posted, or the creation time.</summary>
    public DateTime UpdatedAt => Messages.Count == 0 ? CreatedAt : Messages[^1].PostedAt;

    #endregion
}

/// <summary>A knowledge base article. <see cref="Body" /> is seeded Markdown, never user input.</summary>
public sealed record HelpDeskArticle(int Id, string Title, string CategoryId, string Summary, string Body, DateOnly UpdatedOn, int Views)
{
    #region Properties, Indexers

    /// <summary>Gets the reading time in minutes, at about 200 words a minute.</summary>
    public int ReadingMinutes => Math.Max(1, Body.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length / 200);

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

    /// <summary>Gets whether any filter narrows the queue beyond the default (every ticket that is not resolved).</summary>
    public bool IsActive => AssignedToMe || Priority.Length > 0 || Status != StatusAllOpen || !string.IsNullOrWhiteSpace(Search);

    /// <summary>Gets or sets the priority name; empty means every priority.</summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>Gets or sets the search text, matched against the number, subject, customer, and company.</summary>
    public string Search { get; set; } = string.Empty;

    /// <summary>Gets or sets the status filter: a status name, <see cref="StatusAllOpen" />, or <see cref="StatusAll" />.</summary>
    public string Status { get; set; } = StatusAllOpen;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Resets every filter.</summary>
    public void Clear()
    {
        AssignedToMe = false;
        Priority = string.Empty;
        Search = string.Empty;
        Status = StatusAllOpen;
    }

    #endregion

    #endregion
}
