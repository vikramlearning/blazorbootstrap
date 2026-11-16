using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

/// <summary>One day of a report: tickets created and resolved, resolutions within the SLA, and ratings.</summary>
public sealed record HelpDeskReportDay(DateOnly Date, int Created, int Resolved, int ResolvedInSla, int Good, int Bad);

/// <summary>A priority's row of the SLA report, with its current policy.</summary>
public sealed record HelpDeskReportPriority(HelpDeskPriority Priority, int FirstReplyHours, int ResolveHours, int Resolved, int ResolvedInSla, int FirstReplyDue, int FirstReplyInSla,
    int OpenBreached, int OpenAtRisk);

/// <summary>An agent's row of a report.</summary>
public sealed record HelpDeskReportAgent(string AgentId, string Name, string TeamName, bool IsActive, int Resolved, int ResolvedInSla, int Good, int Bad, int OpenAssigned);

/// <summary>A team's row of a report.</summary>
public sealed record HelpDeskReportTeam(string TeamId, string Name, int Resolved, int ResolvedInSla, int Good, int Bad);

/// <summary>
/// The SLA and satisfaction figures for a date range, optionally for one team. They add the live tickets (which follow every
/// change in the tab, and the current SLA policies) to the seeded archive of closed tickets (fixed figures).
/// </summary>
public sealed class HelpDeskReport
{
    #region Properties, Indexers

    /// <summary>Gets each agent's row, the most resolved first.</summary>
    public IReadOnlyList<HelpDeskReportAgent> ByAgent { get; init; } = Array.Empty<HelpDeskReportAgent>();

    /// <summary>Gets each priority's row, Urgent first.</summary>
    public IReadOnlyList<HelpDeskReportPriority> ByPriority { get; init; } = Array.Empty<HelpDeskReportPriority>();

    /// <summary>Gets each team's row.</summary>
    public IReadOnlyList<HelpDeskReportTeam> ByTeam { get; init; } = Array.Empty<HelpDeskReportTeam>();

    /// <summary>Gets the rating comments in the range, newest first; <see cref="HelpDeskRatingComment.Agent" /> is the agent's name.</summary>
    public IReadOnlyList<HelpDeskRatingComment> Comments { get; init; } = Array.Empty<HelpDeskRatingComment>();

    /// <summary>Gets the tickets created in the range.</summary>
    public int Created => Days.Sum(day => day.Created);

    /// <summary>Gets the days of the range, oldest first.</summary>
    public IReadOnlyList<HelpDeskReportDay> Days { get; init; } = Array.Empty<HelpDeskReportDay>();

    /// <summary>Gets the tickets whose first reply was due (or given) in the range.</summary>
    public int FirstReplyDue => ByPriority.Sum(row => row.FirstReplyDue);

    /// <summary>Gets the tickets that got their first reply within the target.</summary>
    public int FirstReplyInSla => ByPriority.Sum(row => row.FirstReplyInSla);

    /// <summary>Gets the first day of the range.</summary>
    public DateOnly From { get; init; }

    /// <summary>Gets the bad ratings.</summary>
    public int Bad => Days.Sum(day => day.Bad);

    /// <summary>Gets the good ratings.</summary>
    public int Good => Days.Sum(day => day.Good);

    /// <summary>Gets the open tickets past their due time now (not limited to the range).</summary>
    public int OpenBreached => ByPriority.Sum(row => row.OpenBreached);

    /// <summary>Gets the open tickets at risk now.</summary>
    public int OpenAtRisk => ByPriority.Sum(row => row.OpenAtRisk);

    /// <summary>Gets the tickets resolved in the range.</summary>
    public int Resolved => Days.Sum(day => day.Resolved);

    /// <summary>Gets the tickets resolved within their SLA.</summary>
    public int ResolvedInSla => Days.Sum(day => day.ResolvedInSla);

    /// <summary>Gets the last day of the range.</summary>
    public DateOnly To { get; init; }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns a share as a whole percentage, or <see langword="null" /> when there is nothing to divide.</summary>
    public static int? Percent(int part, int whole) => whole <= 0 ? null : (int)Math.Round(100.0 * part / whole, MidpointRounding.AwayFromZero);

    #endregion

    #endregion
}

/// <summary>Builds the agent desk's SLA and satisfaction reports from the help desk state.</summary>
public static class HelpDeskReports
{
    #region Fields and Constants

    private static readonly IReadOnlyList<HelpDeskArchiveDay> archive = HelpDeskSeedData.Archive();

    private static readonly IReadOnlyList<HelpDeskRatingComment> archiveComments = HelpDeskSeedData.ArchiveComments();

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Builds the report of the last <paramref name="days" /> days up to the site's today, for every team or for
    /// <paramref name="teamId" />. Live tickets count by their own team (assignee's, or category's while unassigned); archived
    /// tickets by their agent's current team.
    /// </summary>
    public static HelpDeskReport Build(HelpDeskState state, int days, string? teamId = null)
    {
        days = Math.Clamp(days, 1, 90);
        var to = DateOnly.FromDateTime(state.Now);
        var from = to.AddDays(-(days - 1));
        var inTeam = (string? id) => string.IsNullOrEmpty(teamId) || id == teamId;
        var roster = state.Roster;
        var agentTeam = roster.ToDictionary(agent => agent.Id, agent => agent.TeamId, StringComparer.Ordinal);

        var archived = archive.Where(row => row.Date >= from && row.Date <= to && agentTeam.TryGetValue(row.AgentId, out var team) && inTeam(team)).ToList();
        var live = state.Tickets.Where(ticket => inTeam(state.TeamOf(ticket)?.Id)).ToList();

        bool InRange(DateTime? time) => time is { } value && DateOnly.FromDateTime(value) is var date && date >= from && date <= to;
        bool Met(HelpDeskTicket ticket) => ticket.ResolvedAt <= state.DueAt(ticket);
        bool Rated(HelpDeskTicket ticket, HelpDeskSatisfaction value) => ticket.Status == HelpDeskStatus.Resolved && ticket.Satisfaction == value && InRange(ticket.ResolvedAt);

        var resolvedLive = live.Where(ticket => ticket.Status == HelpDeskStatus.Resolved && InRange(ticket.ResolvedAt)).ToList();

        var dayRows = Enumerable.Range(0, days).Select(offset =>
        {
            var date = from.AddDays(offset);
            var onDay = archived.Where(row => row.Date == date).ToList();
            bool IsDay(DateTime? time) => time is { } value && DateOnly.FromDateTime(value) == date;
            var resolvedToday = resolvedLive.Where(ticket => IsDay(ticket.ResolvedAt)).ToList();

            return new HelpDeskReportDay(date,
                onDay.Sum(row => row.Created) + live.Count(ticket => IsDay(ticket.CreatedAt)),
                onDay.Sum(row => row.Resolved) + resolvedToday.Count,
                onDay.Sum(row => row.ResolvedInSla) + resolvedToday.Count(Met),
                onDay.Sum(row => row.Good) + resolvedToday.Count(ticket => ticket.Satisfaction == HelpDeskSatisfaction.Good),
                onDay.Sum(row => row.Bad) + resolvedToday.Count(ticket => ticket.Satisfaction == HelpDeskSatisfaction.Bad));
        }).ToList();

        var priorityRows = Enum.GetValues<HelpDeskPriority>().Reverse().Select(priority =>
        {
            var policy = state.Policy(priority);
            var rows = archived.Where(row => row.Priority == priority).ToList();
            var tickets = live.Where(ticket => ticket.Priority == priority).ToList();
            var created = tickets.Where(ticket => InRange(ticket.CreatedAt)).ToList();

            // A first reply counts once it is given, or once its due time passed without one.
            var replyDue = created.Where(ticket => ticket.FirstReplyAt is not null || state.Now > state.FirstReplyDueAt(ticket)).ToList();
            var open = tickets.Where(ticket => ticket.Status != HelpDeskStatus.Resolved).Select(state.Sla).ToList();
            var resolved = resolvedLive.Where(ticket => ticket.Priority == priority).ToList();

            return new HelpDeskReportPriority(priority, policy.FirstReplyHours, policy.ResolveHours,
                rows.Sum(row => row.Resolved) + resolved.Count,
                rows.Sum(row => row.ResolvedInSla) + resolved.Count(Met),
                rows.Sum(row => row.Created) + replyDue.Count,
                rows.Sum(row => row.FirstReplyInSla) + replyDue.Count(ticket => ticket.FirstReplyAt is { } at && at <= state.FirstReplyDueAt(ticket)),
                open.Count(sla => sla == HelpDeskSlaState.Breached),
                open.Count(sla => sla == HelpDeskSlaState.AtRisk));
        }).ToList();

        var agentRows = roster
            .Where(agent => inTeam(agent.TeamId))
            .Select(agent =>
            {
                var rows = archived.Where(row => row.AgentId == agent.Id).ToList();
                var resolved = resolvedLive.Where(ticket => ticket.Assignee == agent.Name).ToList();

                return new HelpDeskReportAgent(agent.Id, agent.Name, state.FindTeam(agent.TeamId)?.Name ?? string.Empty, agent.IsActive,
                    rows.Sum(row => row.Resolved) + resolved.Count,
                    rows.Sum(row => row.ResolvedInSla) + resolved.Count(Met),
                    rows.Sum(row => row.Good) + resolved.Count(ticket => Rated(ticket, HelpDeskSatisfaction.Good)),
                    rows.Sum(row => row.Bad) + resolved.Count(ticket => Rated(ticket, HelpDeskSatisfaction.Bad)),
                    live.Count(ticket => ticket.Assignee == agent.Name && ticket.Status != HelpDeskStatus.Resolved));
            })
            .Where(row => row.IsActive || row.Resolved > 0 || row.OpenAssigned > 0)
            .OrderByDescending(row => row.Resolved)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var teamRows = state.Teams
            .Where(team => inTeam(team.Id))
            .Select(team =>
            {
                var rows = archived.Where(row => agentTeam.GetValueOrDefault(row.AgentId) == team.Id).ToList();
                var resolved = resolvedLive.Where(ticket => state.TeamOf(ticket)?.Id == team.Id).ToList();

                return new HelpDeskReportTeam(team.Id, team.Name,
                    rows.Sum(row => row.Resolved) + resolved.Count,
                    rows.Sum(row => row.ResolvedInSla) + resolved.Count(Met),
                    rows.Sum(row => row.Good) + resolved.Count(ticket => ticket.Satisfaction == HelpDeskSatisfaction.Good),
                    rows.Sum(row => row.Bad) + resolved.Count(ticket => ticket.Satisfaction == HelpDeskSatisfaction.Bad));
            })
            .ToList();

        var comments = resolvedLive
            .Where(ticket => ticket.Satisfaction is not null && ticket.SatisfactionComment is not null)
            .Select(ticket => new HelpDeskRatingComment(ticket.ResolvedAt!.Value, ticket.Id, ticket.CustomerName, ticket.Assignee ?? "Unassigned", ticket.Satisfaction!.Value, ticket.SatisfactionComment!))
            .Concat(archiveComments
                .Where(comment => InRange(comment.At) && comment.At <= state.Now && agentTeam.TryGetValue(comment.Agent, out var team) && inTeam(team))
                .Select(comment => comment with { Agent = state.FindAgent(comment.Agent)?.Name ?? comment.Agent }))
            .OrderByDescending(comment => comment.At)
            .ToList();

        return new HelpDeskReport
        {
            From = from,
            To = to,
            Days = dayRows,
            ByPriority = priorityRows,
            ByAgent = agentRows,
            ByTeam = teamRows,
            Comments = comments,
        };
    }

    #endregion

    #endregion
}
