using System.Text.Json;
using System.Text.Json.Serialization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

/// <summary>
/// The tab snapshot of <see cref="HelpDeskState" /> (devspec finding F-4, work item 26112201): only what differs from a fresh
/// tab is stored, as for the other kept sites. The restore rebuilds the full help desk from a fresh tab plus the stored
/// changes, then validates and applies it exactly as before.
/// </summary>
public sealed partial class HelpDeskState
{
    #region Fields and Constants

    /// <summary>A fresh tab's records as JSON, by key: what <see cref="CaptureData" /> compares the help desk with.</summary>
    private static readonly Lazy<HelpDeskBaseline> baseline = new(() => new HelpDeskBaseline(new HelpDeskState().CaptureFull()));

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the snapshot: the ID counters and, of every list, only what differs from a fresh tab.</summary>
    public object CaptureData()
    {
        var current = CaptureFull();
        var seed = baseline.Value;

        var (agents, removedAgents) = ShowcaseSnapshotDelta.Diff(current.Agents!, seed.Agents, item => item.Id, snapshotOptions);
        var (articles, removedArticles) = ShowcaseSnapshotDelta.Diff(current.Articles!, seed.Articles, item => item.Id, snapshotOptions);
        var (audit, removedAudit) = ShowcaseSnapshotDelta.Diff(current.Audit!, seed.Audit, item => item.Id, snapshotOptions);
        var (categories, removedCategories) = ShowcaseSnapshotDelta.Diff(current.Categories!, seed.Categories, item => item.Id, snapshotOptions);
        var (rules, removedRules) = ShowcaseSnapshotDelta.Diff(current.EscalationRules!, seed.EscalationRules, item => item.Id, snapshotOptions);
        var (notifications, removedNotifications) = ShowcaseSnapshotDelta.Diff(current.Notifications!, seed.Notifications, item => item.Id, snapshotOptions);
        var (replies, removedReplies) = ShowcaseSnapshotDelta.Diff(current.SavedReplies!, seed.SavedReplies, item => item.Id, snapshotOptions);
        var (policies, removedPolicies) = ShowcaseSnapshotDelta.Diff(current.SlaPolicies!, seed.SlaPolicies, item => item.Priority, snapshotOptions);
        var (teams, removedTeams) = ShowcaseSnapshotDelta.Diff(current.Teams!, seed.Teams, item => item.Id, snapshotOptions);
        var (tickets, removedTickets) = ShowcaseSnapshotDelta.Diff(current.Tickets!, seed.Tickets, item => item.Id, snapshotOptions);
        var (votes, removedVotes) = ShowcaseSnapshotDelta.Diff(current.Votes!, seed.Votes, item => item.ArticleId, snapshotOptions);

        return new HelpDeskDelta
        {
            Agents = agents,
            RemovedAgentIds = removedAgents,
            Articles = articles,
            RemovedArticleIds = removedArticles,
            Audit = audit,
            RemovedAuditIds = removedAudit,
            Categories = categories,
            RemovedCategoryIds = removedCategories,
            EscalationRules = rules,
            RemovedEscalationRuleIds = removedRules,
            Notifications = notifications,
            RemovedNotificationIds = removedNotifications,
            SavedReplies = replies,
            RemovedSavedReplyIds = removedReplies,
            SlaPolicies = policies,
            RemovedSlaPolicyPriorities = removedPolicies,
            Teams = teams,
            RemovedTeamIds = removedTeams,
            Tickets = tickets,
            RemovedTicketIds = removedTickets,
            Votes = votes,
            RemovedVoteArticleIds = removedVotes,
            NextAgentId = current.NextAgentId,
            NextArticleId = current.NextArticleId,
            NextAuditId = current.NextAuditId,
            NextCategoryId = current.NextCategoryId,
            NextEscalationRuleId = current.NextEscalationRuleId,
            NextNotificationId = current.NextNotificationId,
            NextSavedReplyId = current.NextSavedReplyId,
            NextTeamId = current.NextTeamId,
            NextTicketId = current.NextTicketId,
        };
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Rebuilds the full help desk snapshot from a stored change list and a fresh tab; <see langword="null" /> when the stored
    /// data can't be read. An inconsistent change list leaves a list <see langword="null" />, which the validation rejects.
    /// </summary>
    private static HelpDeskSnapshot? RebuildFromDelta(JsonElement data)
    {
        HelpDeskDelta? delta;

        try
        {
            delta = data.Deserialize<HelpDeskDelta>(snapshotOptions);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException or ArgumentException)
        {
            return null;
        }

        if (delta is null)
            return null;

        // Fresh records, not the cached baseline's, so the restored help desk never shares objects with another tab.
        var fresh = new HelpDeskState().CaptureFull();

        return new HelpDeskSnapshot
        {
            Agents = ShowcaseSnapshotDelta.Overlay(fresh.Agents!, delta.Agents, delta.RemovedAgentIds, item => item.Id),
            Articles = ShowcaseSnapshotDelta.Overlay(fresh.Articles!, delta.Articles, delta.RemovedArticleIds, item => item.Id),
            Audit = ShowcaseSnapshotDelta.Overlay(fresh.Audit!, delta.Audit, delta.RemovedAuditIds, item => item.Id),
            Categories = ShowcaseSnapshotDelta.Overlay(fresh.Categories!, delta.Categories, delta.RemovedCategoryIds, item => item.Id),
            EscalationRules = ShowcaseSnapshotDelta.Overlay(fresh.EscalationRules!, delta.EscalationRules, delta.RemovedEscalationRuleIds, item => item.Id),
            Notifications = ShowcaseSnapshotDelta.Overlay(fresh.Notifications!, delta.Notifications, delta.RemovedNotificationIds, item => item.Id),
            SavedReplies = ShowcaseSnapshotDelta.Overlay(fresh.SavedReplies!, delta.SavedReplies, delta.RemovedSavedReplyIds, item => item.Id),
            SlaPolicies = ShowcaseSnapshotDelta.Overlay(fresh.SlaPolicies!, delta.SlaPolicies, delta.RemovedSlaPolicyPriorities, item => item.Priority),
            Teams = ShowcaseSnapshotDelta.Overlay(fresh.Teams!, delta.Teams, delta.RemovedTeamIds, item => item.Id),
            Tickets = ShowcaseSnapshotDelta.Overlay(fresh.Tickets!, delta.Tickets, delta.RemovedTicketIds, item => item.Id),
            Votes = ShowcaseSnapshotDelta.Overlay(fresh.Votes!, delta.Votes, delta.RemovedVoteArticleIds, item => item.ArticleId),
            NextAgentId = delta.NextAgentId,
            NextArticleId = delta.NextArticleId,
            NextAuditId = delta.NextAuditId,
            NextCategoryId = delta.NextCategoryId,
            NextEscalationRuleId = delta.NextEscalationRuleId,
            NextNotificationId = delta.NextNotificationId,
            NextSavedReplyId = delta.NextSavedReplyId,
            NextTeamId = delta.NextTeamId,
            NextTicketId = delta.NextTicketId,
        };
    }

    #endregion

    #endregion

    #region Nested Types

    /// <summary>A fresh tab's records as JSON, by key.</summary>
    private sealed class HelpDeskBaseline
    {
        #region Constructors and Finalizers

        public HelpDeskBaseline(HelpDeskSnapshot fresh)
        {
            Agents = ShowcaseSnapshotDelta.Index(fresh.Agents!, item => item.Id, snapshotOptions);
            Articles = ShowcaseSnapshotDelta.Index(fresh.Articles!, item => item.Id, snapshotOptions);
            Audit = ShowcaseSnapshotDelta.Index(fresh.Audit!, item => item.Id, snapshotOptions);
            Categories = ShowcaseSnapshotDelta.Index(fresh.Categories!, item => item.Id, snapshotOptions);
            EscalationRules = ShowcaseSnapshotDelta.Index(fresh.EscalationRules!, item => item.Id, snapshotOptions);
            Notifications = ShowcaseSnapshotDelta.Index(fresh.Notifications!, item => item.Id, snapshotOptions);
            SavedReplies = ShowcaseSnapshotDelta.Index(fresh.SavedReplies!, item => item.Id, snapshotOptions);
            SlaPolicies = ShowcaseSnapshotDelta.Index(fresh.SlaPolicies!, item => item.Priority, snapshotOptions);
            Teams = ShowcaseSnapshotDelta.Index(fresh.Teams!, item => item.Id, snapshotOptions);
            Tickets = ShowcaseSnapshotDelta.Index(fresh.Tickets!, item => item.Id, snapshotOptions);
            Votes = ShowcaseSnapshotDelta.Index(fresh.Votes!, item => item.ArticleId, snapshotOptions);
        }

        #endregion

        #region Properties, Indexers

        public Dictionary<string, string> Agents { get; }

        public Dictionary<int, string> Articles { get; }

        public Dictionary<int, string> Audit { get; }

        public Dictionary<string, string> Categories { get; }

        public Dictionary<int, string> EscalationRules { get; }

        public Dictionary<int, string> Notifications { get; }

        public Dictionary<int, string> SavedReplies { get; }

        public Dictionary<HelpDeskPriority, string> SlaPolicies { get; }

        public Dictionary<string, string> Teams { get; }

        public Dictionary<int, string> Tickets { get; }

        public Dictionary<int, string> Votes { get; }

        #endregion
    }

    /// <summary>The help desk's part of the tab snapshot: what differs from a fresh tab, and the ID counters.</summary>
    private sealed class HelpDeskDelta
    {
        #region Properties, Indexers

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HelpDeskAgent>? Agents { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HelpDeskArticle>? Articles { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ShowcaseAuditEntry>? Audit { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HelpDeskCategory>? Categories { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HelpDeskEscalationRule>? EscalationRules { get; set; }

        public int NextAgentId { get; set; }

        public int NextArticleId { get; set; }

        public int NextAuditId { get; set; }

        public int NextCategoryId { get; set; }

        public int NextEscalationRuleId { get; set; }

        public int NextNotificationId { get; set; }

        public int NextSavedReplyId { get; set; }

        public int NextTeamId { get; set; }

        public int NextTicketId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ShowcaseNotification>? Notifications { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? RemovedAgentIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedArticleIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedAuditIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? RemovedCategoryIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedEscalationRuleIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedNotificationIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedSavedReplyIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HelpDeskPriority>? RemovedSlaPolicyPriorities { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? RemovedTeamIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedTicketIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedVoteArticleIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HelpDeskSavedReply>? SavedReplies { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HelpDeskSlaPolicy>? SlaPolicies { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HelpDeskTeam>? Teams { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HelpDeskTicket>? Tickets { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HelpDeskArticleVotes>? Votes { get; set; }

        #endregion
    }

    #endregion
}
