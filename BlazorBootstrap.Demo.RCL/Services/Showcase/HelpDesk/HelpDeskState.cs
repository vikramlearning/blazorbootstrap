using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

/// <summary>
/// In-memory state of the help desk for the current session: tickets, their threads, queue filters, and article votes.
/// Nothing is persisted or sent.
/// </summary>
public sealed class HelpDeskState
{
    #region Fields and Constants

    private readonly Dictionary<int, (int Helpful, int NotHelpful)> votes = new();

    private int nextTicketId;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh session.</summary>
    public HelpDeskState() => nextTicketId = Tickets.Max(ticket => ticket.Id) + 1;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Adds an agent reply or internal note. A reply to a new or open ticket sets it to Pending; a note changes nothing else.</summary>
    public void AddReply(HelpDeskTicket ticket, string html, bool isInternalNote)
    {
        ticket.Messages.Add(new HelpDeskMessage(HelpDeskSeedData.AgentName, IsAgent: true, isInternalNote, IsRichText: true, html, NextTime(ticket)));
        ticket.Assignee ??= HelpDeskSeedData.AgentName;

        if (!isInternalNote && ticket.Status is HelpDeskStatus.New or HelpDeskStatus.Open)
            ticket.Status = HelpDeskStatus.Pending;

        Changed?.Invoke();
    }

    /// <summary>Assigns a ticket; a new ticket becomes Open.</summary>
    public void Assign(HelpDeskTicket ticket, string? agent)
    {
        ticket.Assignee = agent;

        if (agent is not null && ticket.Status == HelpDeskStatus.New)
            ticket.Status = HelpDeskStatus.Open;

        Changed?.Invoke();
    }

    /// <summary>Creates a ticket from the new-ticket form and returns it. The description is stored as plain text.</summary>
    public HelpDeskTicket Create(string customerName, string email, string? company, string categoryId, HelpDeskPriority priority, string subject, string description)
    {
        var ticket = new HelpDeskTicket
        {
            Id = nextTicketId++,
            Subject = subject.Trim(),
            CustomerName = customerName.Trim(),
            CustomerEmail = email.Trim(),
            Company = string.IsNullOrWhiteSpace(company) ? null : company.Trim(),
            CategoryId = FindCategory(categoryId).Id,
            Priority = priority,
            Status = HelpDeskStatus.New,
            CreatedAt = HelpDeskSeedData.Now,
        };

        ticket.Messages.Add(new HelpDeskMessage(ticket.CustomerName, IsAgent: false, IsInternalNote: false, IsRichText: false, description.Trim(), HelpDeskSeedData.Now));
        Tickets.Insert(0, ticket);
        Changed?.Invoke();

        return ticket;
    }

    /// <summary>Returns when a ticket's SLA target is due.</summary>
    public static DateTime DueAt(HelpDeskTicket ticket) => ticket.CreatedAt.AddHours(TargetHours(ticket.Priority));

    /// <summary>Finds an article by its ID, or returns <see langword="null" />.</summary>
    public HelpDeskArticle? FindArticle(int articleId) => Articles.FirstOrDefault(article => article.Id == articleId);

    /// <summary>Finds a category by its ID; unknown IDs fall back to the first category.</summary>
    public HelpDeskCategory FindCategory(string? categoryId) => Categories.FirstOrDefault(category => category.Id == categoryId) ?? Categories[0];

    /// <summary>Finds a ticket by its number, or returns <see langword="null" />.</summary>
    public HelpDeskTicket? FindTicket(int ticketId) => Tickets.FirstOrDefault(ticket => ticket.Id == ticketId);

    /// <summary>Returns the queue for the current filters, most urgent SLA first.</summary>
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
            .Where(ticket => !Filter.AssignedToMe || ticket.Assignee == HelpDeskSeedData.AgentName)
            .Where(ticket => search.Length == 0
                || ticket.Id.ToString(System.Globalization.CultureInfo.InvariantCulture).Contains(search, StringComparison.Ordinal)
                || ticket.Subject.Contains(search, StringComparison.OrdinalIgnoreCase)
                || ticket.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || (ticket.Company?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderBy(ticket => SlaRank(Sla(ticket)))
            .ThenBy(DueAt)
            .ToList();
    }

    /// <summary>Returns other articles in the same category.</summary>
    public IReadOnlyList<HelpDeskArticle> RelatedArticles(string categoryId, int? exceptId = null) =>
        Articles.Where(article => article.CategoryId == categoryId && article.Id != exceptId).ToList();

    /// <summary>Reopens a resolved ticket.</summary>
    public void Reopen(HelpDeskTicket ticket)
    {
        ticket.Status = HelpDeskStatus.Open;
        ticket.ResolvedAt = null;
        Changed?.Invoke();
    }

    /// <summary>Resolves a ticket now.</summary>
    public void Resolve(HelpDeskTicket ticket)
    {
        ticket.Status = HelpDeskStatus.Resolved;
        ticket.ResolvedAt = HelpDeskSeedData.Now;
        ticket.Assignee ??= HelpDeskSeedData.AgentName;
        Changed?.Invoke();
    }

    /// <summary>Returns a ticket's state against its SLA target at the fixed "now".</summary>
    public static HelpDeskSlaState Sla(HelpDeskTicket ticket)
    {
        var due = DueAt(ticket);

        if (ticket.Status == HelpDeskStatus.Resolved)
            return ticket.ResolvedAt <= due ? HelpDeskSlaState.Met : HelpDeskSlaState.Missed;

        if (ticket.Status == HelpDeskStatus.Pending)
            return HelpDeskSlaState.Paused;

        var left = due - HelpDeskSeedData.Now;

        if (left <= TimeSpan.Zero)
            return HelpDeskSlaState.Breached;

        return left <= TimeSpan.FromHours(2) || left.TotalHours <= TargetHours(ticket.Priority) / 4.0
            ? HelpDeskSlaState.AtRisk
            : HelpDeskSlaState.OnTrack;
    }

    /// <summary>Returns the resolution target of a priority, in hours.</summary>
    public static int TargetHours(HelpDeskPriority priority) => priority switch
    {
        HelpDeskPriority.Urgent => 4,
        HelpDeskPriority.High => 8,
        HelpDeskPriority.Normal => 24,
        _ => 72,
    };

    /// <summary>Records a "was this helpful?" vote for an article.</summary>
    public void Vote(int articleId, bool helpful)
    {
        var current = Votes(articleId);
        votes[articleId] = helpful ? (current.Helpful + 1, current.NotHelpful) : (current.Helpful, current.NotHelpful + 1);
        Changed?.Invoke();
    }

    /// <summary>Returns an article's votes in this session.</summary>
    public (int Helpful, int NotHelpful) Votes(int articleId) => votes.TryGetValue(articleId, out var count) ? count : (0, 0);

    #endregion

    #region Private Methods

    /// <summary>A new message is posted at the fixed "now", or a minute after the last message if that is later.</summary>
    private static DateTime NextTime(HelpDeskTicket ticket) =>
        ticket.UpdatedAt >= HelpDeskSeedData.Now ? ticket.UpdatedAt.AddMinutes(1) : HelpDeskSeedData.Now;

    private static int SlaRank(HelpDeskSlaState state) => state switch
    {
        HelpDeskSlaState.Breached => 0,
        HelpDeskSlaState.AtRisk => 1,
        HelpDeskSlaState.OnTrack => 2,
        HelpDeskSlaState.Paused => 3,
        _ => 4,
    };

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the support agents.</summary>
    public IReadOnlyList<string> Agents { get; } = HelpDeskSeedData.Agents();

    /// <summary>Gets the knowledge base articles.</summary>
    public IReadOnlyList<HelpDeskArticle> Articles { get; } = HelpDeskSeedData.Articles();

    /// <summary>Gets the categories.</summary>
    public IReadOnlyList<HelpDeskCategory> Categories { get; } = HelpDeskSeedData.Categories();

    /// <summary>Gets the queue filters.</summary>
    public HelpDeskFilter Filter { get; } = new();

    /// <summary>Gets every ticket, newest first.</summary>
    public List<HelpDeskTicket> Tickets { get; } = HelpDeskSeedData.Tickets().OrderByDescending(ticket => ticket.CreatedAt).ToList();

    #endregion

    #region Events

    /// <summary>Raised when tickets or votes change.</summary>
    public event Action? Changed;

    #endregion
}
