using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

/// <summary>
/// The tab snapshot of <see cref="AiState" /> (devspec finding F-4): only what differs from the deterministic seed is
/// stored — records changed or added, the keys of seeded records removed, the policy, the preferences, and the ID counters —
/// and the restore rebuilds the rest from the seed and validates every part before applying it, so a fresh tab's snapshot
/// stays a few hundred characters although it holds hundreds of conversations, prompts, and members.
/// </summary>
public sealed partial class AiState
{
    #region Methods

    #region Public Methods

    /// <summary>Returns the snapshot: the ID counters, the policy, the preferences, and, of every list, only what differs from the fresh tab's seed.</summary>
    public object CaptureData()
    {
        var data = seed.Value;
        var (changedAudit, removedAudit) = Diff(audit, data.Audit, entry => entry.Id, SameAudit);
        var (changedConversations, removedConversations) = Diff(conversations, data.Conversations, item => item.Id, SameConversation);
        var (changedDlp, removedDlp) = Diff(dlpEvents, data.DlpEvents, item => item.Id, SameDlpEvent);
        var (changedMembers, removedMembers) = Diff(members, data.Members, item => item.Id, SameMember);
        var (changedNotifications, removedNotifications) = Diff(notifications, data.Notifications, notice => notice.Id, SameNotification);
        var (changedPrompts, removedPrompts) = Diff(prompts, data.Prompts, item => item.Id, SamePrompt);
        var (changedUsage, removedUsage) = Diff(ledger, data.Usage, UsageKey, SameUsage);

        return new AiSnapshot
        {
            Audit = changedAudit,
            RemovedAuditIds = removedAudit,
            Conversations = changedConversations,
            RemovedConversationIds = removedConversations,
            DemoUsage = demoUsage.Count == 0 ? null : demoUsage.ToList(),
            DlpEvents = changedDlp,
            RemovedDlpEventIds = removedDlp,
            SpendAlerts = spendAlerts.Count == 0 ? null : spendAlerts.ToList(),
            Members = changedMembers,
            RemovedMemberIds = removedMembers,
            Notifications = changedNotifications,
            RemovedNotificationIds = removedNotifications,
            Prompts = changedPrompts,
            RemovedPromptIds = removedPrompts,
            Usage = changedUsage,
            RemovedUsageKeys = removedUsage,
            Policy = SamePolicy(policy, data.Policy) ? null : policy,
            Preferences = preferences.Count == 0 ? null : preferences.ToDictionary(pair => pair.Key, pair => pair.Value),
            NextAuditId = nextAuditId,
            NextConversationId = nextConversationId,
            NextDlpEventId = nextDlpEventId,
            NextMemberId = nextMemberId,
            NextMessageId = nextMessageId,
            NextNotificationId = nextNotificationId,
            NextPromptId = nextPromptId,
        };
    }

    /// <summary>
    /// Rebuilds the workspace from the seed and a stored snapshot's changes, validates every part against itself, and applies
    /// it only when everything is valid: known IDs and references, defined enum values, numbers and dates in range, bounded
    /// text, unique emails and prompt titles in their scope, prompt variables that agree with their text, exactly one Owner
    /// who is active and can sign in, and links that stay on this site.
    /// </summary>
    public bool TryRestoreData(JsonElement data)
    {
        AiSnapshot? snapshot;

        try
        {
            snapshot = data.Deserialize<AiSnapshot>(snapshotOptions);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException or ArgumentException)
        {
            return false;
        }

        if (snapshot is null
            || new[] { snapshot.NextAuditId, snapshot.NextConversationId, snapshot.NextDlpEventId, snapshot.NextMemberId, snapshot.NextMessageId, snapshot.NextNotificationId, snapshot.NextPromptId }
                .Any(id => id is < 1 or > NextIdLimit))
            return false;

        // A fresh copy of the seed: the stored changes replace, remove, and add records on it; an inconsistent change list discards the snapshot.
        var fresh = AiSeedData.Build();

        if (Overlay(fresh.Audit, snapshot.Audit, snapshot.RemovedAuditIds, entry => entry.Id) is not { Count: <= MaxAudit } storedAudit
            || Overlay(fresh.Conversations, snapshot.Conversations, snapshot.RemovedConversationIds, item => item.Id) is not { } storedConversations
            || Overlay(fresh.DlpEvents, snapshot.DlpEvents, snapshot.RemovedDlpEventIds, item => item.Id) is not { Count: <= MaxDlpEvents } storedDlpEvents
            || snapshot.DemoUsage is { Count: > MaxDemoUsage } || snapshot.SpendAlerts is { Count: > MaxSpendAlerts }
            || Overlay(fresh.Members, snapshot.Members, snapshot.RemovedMemberIds, item => item.Id) is not { Count: > 0 and <= MaxMembers } storedMembers
            || Overlay(fresh.Notifications, snapshot.Notifications, snapshot.RemovedNotificationIds, notice => notice.Id) is not { Count: <= MaxNotifications } storedNotifications
            || Overlay(fresh.Prompts, snapshot.Prompts, snapshot.RemovedPromptIds, item => item.Id) is not { Count: <= MaxPrompts } storedPrompts
            || Overlay(fresh.Usage, snapshot.Usage, snapshot.RemovedUsageKeys, UsageKey) is not { Count: <= MaxUsageEntries } storedUsage
            || snapshot.Preferences is { Count: > MaxMembers })
            return false;

        var storedPolicy = snapshot.Policy ?? fresh.Policy;

        // Members' own caps stay within the workspace cap.
        if (!ValidPolicy(storedPolicy) || !ValidMembers(storedMembers, snapshot) || storedMembers.Any(member => member.MonthlyCap > storedPolicy.WorkspaceMonthlyCap))
            return false;

        var memberIds = storedMembers.Select(member => member.Id).ToHashSet();
        var storedDemo = snapshot.DemoUsage ?? new List<AiDemoUsage>();
        var storedAlerts = snapshot.SpendAlerts ?? new List<string>();

        if (storedDemo.Any(line => line is null || !memberIds.Contains(line.MemberId) || FindModel(line.ModelId) is null || line.Date != line.Date.Date
                || line.Date < AiSeedData.Now.Date.AddDays(-400) || line.Date > AiSeedData.Now + ShowcaseClock.MaxElapsed || line.Tokens is < 1 or > MaxWorkspaceCap)
            || !Unique(storedDemo.Select(line => (line.MemberId, line.Date, line.ModelId)))
            || storedAlerts.Any(alert => alert is null || !SpendAlertPattern().IsMatch(alert)) || !Unique(storedAlerts))
            return false;

        if (storedDlpEvents.Any(item => !ValidDlpEvent(item, snapshot, memberIds)) || !Unique(storedDlpEvents.Select(item => item.Id)))
            return false;

        var accountIds = storedMembers.Where(member => member.AccountId is not null).Select(member => member.AccountId!).ToHashSet(StringComparer.Ordinal);

        if (storedConversations.Any(conversation => !ValidConversation(conversation, snapshot, accountIds))
            || !Unique(storedConversations.Select(conversation => conversation.Id))
            || !Unique(storedConversations.SelectMany(conversation => conversation.Messages).Select(message => message.Id))
            || storedConversations.GroupBy(conversation => conversation.AccountId).Any(group => group.Count() > MaxConversations))
            return false;

        if (storedPrompts.Any(prompt => !ValidPrompt(prompt, snapshot, accountIds)) || !Unique(storedPrompts.Select(prompt => prompt.Id))
            || !Unique(storedPrompts.Select(prompt => (prompt.OwnerId ?? string.Empty, prompt.Title.ToLowerInvariant()))))
            return false;

        if (snapshot.Preferences is { } storedPreferences
            && storedPreferences.Any(pair => !accountIds.Contains(pair.Key) || pair.Value is null || (pair.Value.ModelId is not null && FindModel(pair.Value.ModelId) is null)))
            return false;

        // The ledger: one line per account, model, and day; a month never holds more than the highest quota (a send is refused past the quota).
        if (storedUsage.Any(entry => entry is null || !accountIds.Contains(entry.AccountId) || FindModel(entry.ModelId) is null || entry.Date != entry.Date.Date
                || entry.Date < AiSeedData.Now.Date.AddDays(-400) || entry.Date > AiSeedData.Now + ShowcaseClock.MaxElapsed || entry.Messages is < 0 or > 100_000 || entry.Tokens is < 0 or > MaxQuota)
            || !Unique(storedUsage.Select(UsageKey))
            || storedUsage.GroupBy(entry => (entry.AccountId, entry.Date.Year, entry.Date.Month)).Any(group => group.Sum(entry => (long)entry.Tokens) > MaxQuota))
            return false;

        if (storedAudit.Any(entry => entry is null || entry.Id < 1 || entry.Id >= snapshot.NextAuditId || !IsPlausible(entry.At) || !IsText(entry.Actor, ShowcaseIdentity.MaxNameLength)
                || !IsText(entry.Action, MaxAuditTextLength) || !IsText(entry.Target, MaxAuditTextLength) || (entry.Details is not null && !IsText(entry.Details, MaxAuditTextLength))
                || !IsSiteLink(entry.TargetUrl))
            || !Unique(storedAudit.Select(entry => entry.Id)))
            return false;

        if (storedNotifications.Any(notice => notice is null || notice.Id < 1 || notice.Id >= snapshot.NextNotificationId || !IsAccountId(notice.AccountId) || !IsPlausible(notice.At)
                || !IsText(notice.Title, MaxNoticeTitleLength) || (notice.Text is not null && !IsText(notice.Text, MaxNoticeTextLength)) || !IsSiteLink(notice.Url))
            || !Unique(storedNotifications.Select(notice => notice.Id)))
            return false;

        Replace(audit, storedAudit);
        Replace(conversations, storedConversations);
        Replace(demoUsage, storedDemo);
        Replace(dlpEvents, storedDlpEvents);
        Replace(spendAlerts, storedAlerts);
        Replace(ledger, storedUsage);
        Replace(members, storedMembers);
        Replace(notifications, storedNotifications);
        Replace(prompts, storedPrompts);
        preferences.Clear();

        foreach (var pair in snapshot.Preferences ?? new Dictionary<string, AiPreferences>())
            preferences[pair.Key] = pair.Value;

        policy = storedPolicy;
        nextAuditId = snapshot.NextAuditId;
        nextConversationId = snapshot.NextConversationId;
        nextDlpEventId = snapshot.NextDlpEventId;
        nextMemberId = snapshot.NextMemberId;
        nextMessageId = snapshot.NextMessageId;
        nextNotificationId = snapshot.NextNotificationId;
        nextPromptId = snapshot.NextPromptId;

        return true;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Returns the records that differ from the seed (changed or added, in list order) and the keys of seeded records that
    /// were removed; <see langword="null" /> for an empty list, so it is left out of the snapshot.
    /// </summary>
    private static (List<T>? Changed, List<TKey>? Removed) Diff<T, TKey>(IReadOnlyList<T> current, IReadOnlyList<T> seeded, Func<T, TKey> key, Func<T, T, bool> equals)
        where TKey : notnull
    {
        var seededByKey = seeded.ToDictionary(key);
        var currentKeys = new HashSet<TKey>();
        var changed = new List<T>();

        foreach (var item in current)
        {
            var itemKey = key(item);
            currentKeys.Add(itemKey);

            if (!seededByKey.TryGetValue(itemKey, out var original) || !equals(original, item))
                changed.Add(item);
        }

        var removed = seeded.Where(item => !currentKeys.Contains(key(item))).Select(key).ToList();

        return (changed.Count == 0 ? null : changed, removed.Count == 0 ? null : removed);
    }

    /// <summary>
    /// Rebuilds a list from the seed and a snapshot's changes: seeded records in their order (replaced by a stored record
    /// with the same key, left out when removed), then the added records in their stored order. Returns
    /// <see langword="null" /> for an inconsistent change list (a missing record, a repeated key, or a removed key that isn't
    /// seeded), which discards the snapshot.
    /// </summary>
    private static List<T>? Overlay<T, TKey>(IReadOnlyList<T> seeded, List<T>? changed, List<TKey>? removed, Func<T, TKey> key)
        where TKey : notnull
    {
        changed ??= new List<T>();
        removed ??= new List<TKey>();

        if (changed.Count > 20_000 || removed.Count > 20_000 || changed.Any(item => item is null) || removed.Any(item => item is null))
            return null;

        var replacements = new Dictionary<TKey, T>();

        foreach (var item in changed)
        {
            if (key(item) is not { } itemKey || !replacements.TryAdd(itemKey, item))
                return null;
        }

        var seededKeys = seeded.Select(key).ToHashSet();
        var removedKeys = removed.ToHashSet();

        if (removedKeys.Count != removed.Count || removedKeys.Any(item => !seededKeys.Contains(item) || replacements.ContainsKey(item)))
            return null;

        var result = new List<T>(seeded.Count + changed.Count);

        foreach (var item in seeded)
        {
            var itemKey = key(item);

            if (replacements.TryGetValue(itemKey, out var replacement))
                result.Add(replacement);
            else if (!removedKeys.Contains(itemKey))
                result.Add(item);
        }

        result.AddRange(changed.Where(item => !seededKeys.Contains(key(item))));

        return result;
    }

    private static void Replace<T>(List<T> target, List<T> source)
    {
        target.Clear();
        target.AddRange(source);
    }

    private static bool SameAudit(ShowcaseAuditEntry left, ShowcaseAuditEntry right) =>
        left.Id == right.Id && left.At == right.At && left.Actor == right.Actor && left.Action == right.Action && left.Target == right.Target
        && left.TargetUrl == right.TargetUrl && left.Details == right.Details;

    private static bool SameConversation(AiConversation left, AiConversation right) =>
        left.Id == right.Id && left.AccountId == right.AccountId && left.CreatedAt == right.CreatedAt && left.IsPinned == right.IsPinned && left.ModelId == right.ModelId
        && left.Title == right.Title && left.UpdatedAt == right.UpdatedAt && left.Messages.Count == right.Messages.Count
        && left.Messages.Zip(right.Messages).All(pair => SameMessage(pair.First, pair.Second));

    private static bool SameDlpEvent(AiDlpEvent left, AiDlpEvent right) =>
        left.Id == right.Id && left.ActionTaken == right.ActionTaken && left.At == right.At && left.ConversationId == right.ConversationId && left.ConversationTitle == right.ConversationTitle
        && left.Kind == right.Kind && left.Masked == right.Masked && left.MemberId == right.MemberId && left.Note == right.Note && left.ReviewedAt == right.ReviewedAt
        && left.ReviewedBy == right.ReviewedBy && left.Status == right.Status;

    private static bool SameMember(AiMember left, AiMember right) =>
        left.Id == right.Id && left.AccountId == right.AccountId && left.Email == right.Email && left.JoinedAt == right.JoinedAt && left.Name == right.Name
        && left.Role == right.Role && left.Status == right.Status && left.MonthlyCap == right.MonthlyCap;

    private static bool SameMessage(AiMessage left, AiMessage right) =>
        left.Id == right.Id && left.At == right.At && left.Feedback == right.Feedback && left.IsStopped == right.IsStopped && left.ModelId == right.ModelId
        && left.ReplyKey == right.ReplyKey && left.Role == right.Role && left.StoppedLength == right.StoppedLength && left.Text == right.Text && left.Tokens == right.Tokens
        && left.Variant == right.Variant && left.Attachments.SequenceEqual(right.Attachments);

    private static bool SameNotification(ShowcaseNotification left, ShowcaseNotification right) =>
        left.Id == right.Id && left.AccountId == right.AccountId && left.At == right.At && left.IsRead == right.IsRead && left.Title == right.Title && left.Text == right.Text
        && left.Url == right.Url;

    private static bool SamePrompt(AiPrompt left, AiPrompt right) =>
        left.Id == right.Id && left.Author == right.Author && left.AuthorId == right.AuthorId && left.Category == right.Category && left.IsPublished == right.IsPublished
        && left.OwnerId == right.OwnerId && left.Text == right.Text && left.Title == right.Title && left.UpdatedAt == right.UpdatedAt && left.UseCount == right.UseCount
        && left.Tags.SequenceEqual(right.Tags) && left.Variables.SequenceEqual(right.Variables);

    private static bool SameUsage(AiUsageEntry left, AiUsageEntry right) =>
        left.AccountId == right.AccountId && left.Date == right.Date && left.ModelId == right.ModelId && left.Messages == right.Messages && left.Tokens == right.Tokens;

    /// <summary>A remembered spend alert: the scope (the workspace, or a member's number), the month, the level, and the cap.</summary>
    [GeneratedRegex(@"^(w|m[0-9]{1,7})\|20[0-9]{2}-(0[1-9]|1[0-2])\|(80|100)\|[0-9]{1,9}$")]
    private static partial Regex SpendAlertPattern();

    /// <summary>A ledger line's key: its account, model, and day.</summary>
    private static string UsageKey(AiUsageEntry entry) => $"{entry.AccountId}|{entry.ModelId}|{entry.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

    /// <summary>Checks one stored conversation: owner, IDs, title, model, times, and every message (assistant replies must name a template).</summary>
    private bool ValidConversation(AiConversation? conversation, AiSnapshot snapshot, HashSet<string> accountIds)
    {
        if (conversation is null || conversation.Id < 1 || conversation.Id >= snapshot.NextConversationId || conversation.AccountId is null || !accountIds.Contains(conversation.AccountId)
            || !IsText(conversation.Title, MaxTitleLength) || FindModel(conversation.ModelId) is null || !IsPlausible(conversation.CreatedAt) || !IsPlausible(conversation.UpdatedAt)
            || conversation.Messages is not { Count: > 0 and <= MaxMessages })
            return false;

        return conversation.Messages.All(message => message is not null && message.Id >= 1 && message.Id < snapshot.NextMessageId && Enum.IsDefined(message.Role)
            && message.At >= conversation.CreatedAt && message.At <= AiSeedData.Now + ShowcaseClock.MaxElapsed && FindModel(message.ModelId) is not null && Enum.IsDefined(message.Feedback)
            && message.Text is not null && message.Attachments is { Count: <= MaxAttachments }
            && message.Attachments.All(file => file is not null && IsText(file.Name, MaxAttachmentNameLength) && file.SizeBytes is >= 0 and <= MaxAttachmentMb * 1024L * 1024 && (file.ContentType?.Length ?? 0) <= 100)
            && (message.Role == AiRole.User
                ? IsText(message.Text, MaxMessageLength) && message.ReplyKey is null && !message.IsStopped && message.Feedback == AiFeedback.None
                    && message.Tokens == AiSeedData.TokensOf(message.Text) + (message.Attachments.Count * AttachmentTokens)
                : message.Text.Length == 0 && message.Attachments.Count == 0 && AiReplies.IsKnown(message.ReplyKey, message.Variant)
                    && message.StoppedLength >= 0 && message.StoppedLength <= FullReply(message).Length && message.Tokens == AiSeedData.TokensOf(FullReply(message))));
    }

    /// <summary>
    /// Checks a stored sensitive data event: its number, a known member, a defined kind, action, and status, a masked match of
    /// the form the policy writes (so no raw value is ever restored), bounded text, and review details only once reviewed.
    /// </summary>
    private static bool ValidDlpEvent(AiDlpEvent? item, AiSnapshot snapshot, HashSet<int> memberIds) =>
        item is not null && item.Id >= 1 && item.Id < snapshot.NextDlpEventId && memberIds.Contains(item.MemberId) && Enum.IsDefined(item.Kind) && Enum.IsDefined(item.ActionTaken)
        && Enum.IsDefined(item.Status) && IsPlausible(item.At) && AiDlp.IsMasked(item.Kind, item.Masked) && item.ConversationId is null or >= 1
        && item.ConversationTitle is { Length: <= MaxTitleLength } && item.Note is { Length: <= MaxReviewNoteLength }
        && (item.Status == AiDlpStatus.New
            ? item.ReviewedAt is null && item.ReviewedBy is null
            : item.ReviewedAt is { } reviewed && IsPlausible(reviewed) && IsText(item.ReviewedBy, ShowcaseIdentity.MaxNameLength));

    /// <summary>
    /// Checks the members: unique numbers, emails, and accounts; defined roles and statuses; an invitation has no account and
    /// isn't for Owner; and exactly one Owner, who is active and can sign in (so the workspace always has someone in charge).
    /// </summary>
    private static bool ValidMembers(List<AiMember> stored, AiSnapshot snapshot) =>
        !stored.Any(member => member is null || member.Id < 1 || member.Id >= snapshot.NextMemberId || !IsEmail(member.Email) || member.Email != member.Email.ToLowerInvariant()
                || !IsText(member.Name, ShowcaseIdentity.MaxNameLength) || !Enum.IsDefined(member.Role) || !Enum.IsDefined(member.Status) || !IsPlausible(member.JoinedAt)
                || (member.Status == AiMemberStatus.Invited && (member.AccountId is not null || member.Role == AiMemberRole.Owner))
                || (member.AccountId is not null && !IsAccountId(member.AccountId)) || member.MonthlyCap is < MinQuota or > MaxQuota)
        && Unique(stored.Select(member => member.Id)) && Unique(stored.Select(member => member.Email))
        && Unique(stored.Where(member => member.AccountId is not null).Select(member => member.AccountId))
        && stored.Count(member => member.Role == AiMemberRole.Owner) == 1
        && stored.Single(member => member.Role == AiMemberRole.Owner) is { Status: AiMemberStatus.Active, AccountId: not null };

    /// <summary>
    /// Checks the policy: a name, known and unique enabled models with the default among them, every limit in range, the
    /// default member cap within the workspace cap, and one sensitive data rule per kind.
    /// </summary>
    private bool ValidPolicy(AiPolicy? stored) =>
        stored is not null && stored.WorkspaceMonthlyCap is >= MinWorkspaceCap and <= MaxWorkspaceCap && stored.MonthlyTokenQuota <= stored.WorkspaceMonthlyCap
        && ValidDlpRules(stored.DlpRules) && stored.WorkspaceName is { Length: >= MinWorkspaceNameLength and <= MaxWorkspaceNameLength } name && name == name.Trim()
        && stored.EnabledModelIds is { Count: > 0 } && stored.EnabledModelIds.All(id => FindModel(id) is not null) && Unique(stored.EnabledModelIds)
        && stored.EnabledModelIds.Contains(stored.DefaultModelId) && stored.MonthlyTokenQuota is >= MinQuota and <= MaxQuota
        && stored.MaxAttachmentMb is >= 1 and <= MaxAttachmentMb && stored.RetentionDays is >= 0 and <= MaxRetentionDays && stored.BlockedTerms is { Count: <= MaxBlockedTerms }
        && stored.BlockedTerms.All(term => IsText(term, MaxBlockedTermLength) && term == term.Trim().ToLowerInvariant()) && Unique(stored.BlockedTerms);

    /// <summary>
    /// Checks one stored prompt: its number, owner (a member who can sign in) or none (shared), author, text and title in
    /// range, a defined category, variables that agree with the text, tags of the allowed form, and only shared prompts published.
    /// </summary>
    private static bool ValidPrompt(AiPrompt? prompt, AiSnapshot snapshot, HashSet<string> accountIds) =>
        prompt is not null && prompt.Id >= 1 && prompt.Id < snapshot.NextPromptId && (prompt.OwnerId is null || accountIds.Contains(prompt.OwnerId))
        && (prompt.AuthorId is null || IsAccountId(prompt.AuthorId)) && IsText(prompt.Author, ShowcaseIdentity.MaxNameLength) && IsText(prompt.Title, MaxPromptTitleLength)
        && prompt.Title == prompt.Title.Trim() && IsText(prompt.Text, MaxPromptTextLength) && Enum.IsDefined(prompt.Category) && (prompt.IsShared || !prompt.IsPublished)
        && prompt.Tags is { Count: <= MaxTags } && prompt.Tags.All(tag => tag is not null && tag.Length <= MaxTagLength && TagPattern().IsMatch(tag)) && Unique(prompt.Tags)
        && prompt.Variables is { } variables && variables.All(name => name is not null) && Unique(variables) && AiPromptText.IsConsistent(prompt.Text, variables)
        && IsPlausible(prompt.UpdatedAt) && prompt.UseCount is >= 0 and <= 1_000_000;

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the snapshot format version: 2 stores only what differs from the seed, and adds the group roles (Owner, Admin,
    /// Prompt editor, Member), member numbers, published and draft shared prompts with categories and variables, the
    /// workspace name, the audit log, and notifications; 3 adds the spend caps (the workspace's, members' own, the demo
    /// control's usage, and the alerts raised), older conversations, and the sensitive data policy and its review queue.
    /// </summary>
    public int SnapshotVersion => 3;

    #endregion

    #region Nested Types

    /// <summary>
    /// The stored form of the state: the ID counters, the policy when it differs from the seed, the preferences, and, of each
    /// list, the records that differ from the seed (changed or added) and the keys of seeded records that were removed; it is
    /// rebuilt over the seed on restore. Empty parts are left out of the JSON.
    /// </summary>
    private sealed class AiSnapshot
    {
        #region Properties, Indexers

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ShowcaseAuditEntry>? Audit { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<AiConversation>? Conversations { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<AiDemoUsage>? DemoUsage { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<AiDlpEvent>? DlpEvents { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<AiMember>? Members { get; set; }

        public int NextAuditId { get; set; }

        public int NextConversationId { get; set; }

        public int NextDlpEventId { get; set; }

        public int NextMemberId { get; set; }

        public int NextMessageId { get; set; }

        public int NextNotificationId { get; set; }

        public int NextPromptId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ShowcaseNotification>? Notifications { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AiPolicy? Policy { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, AiPreferences>? Preferences { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<AiPrompt>? Prompts { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedAuditIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedConversationIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedDlpEventIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedMemberIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedNotificationIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedPromptIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? RemovedUsageKeys { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? SpendAlerts { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<AiUsageEntry>? Usage { get; set; }

        #endregion
    }

    #endregion
}
