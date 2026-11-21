using System.Text.Json;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

/// <summary>
/// State of "Parley", the AI assistant workspace, for the current browser tab: members and their workspace roles,
/// conversations, saved prompts, the workspace policy, members' preferences, the audit log, notifications, and the demo
/// sign-in. It is kept in the tab's <c>sessionStorage</c> through the showcase foundation (only what differs from the
/// deterministic seed); nothing is sent and no model is called.
/// </summary>
/// <remarks>
/// Assistant replies are fixed templates (<see cref="AiReplies" />): a reply stores only its template key and variant, and
/// its text streams by the clock (characters shown = seconds since it was sent × the model's speed) until it finishes or
/// is stopped. Members' rights come from their workspace role (<see cref="AiMember.Role" />, AC-4) through the permission
/// matrix in <see cref="RoleAllows" />, and every action checks it here, not only on screen. A new account joins as a
/// member, or takes up its invitation.
/// </remarks>
public sealed partial class AiState : IShowcaseEnterpriseState
{
    #region Fields and Constants

    /// <summary>The most attachments on one message.</summary>
    public const int MaxAttachments = 5;

    /// <summary>The longest attachment name.</summary>
    public const int MaxAttachmentNameLength = 120;

    /// <summary>The most blocked terms.</summary>
    public const int MaxBlockedTerms = 20;

    /// <summary>The longest blocked term.</summary>
    public const int MaxBlockedTermLength = 40;

    /// <summary>The most conversations per member.</summary>
    public const int MaxConversations = 150;

    /// <summary>The largest attachment size an admin can allow, in megabytes.</summary>
    public const int MaxAttachmentMb = 100;

    /// <summary>The most members (and invitations).</summary>
    public const int MaxMembers = 150;

    /// <summary>The most messages in a conversation (each turn is two).</summary>
    public const int MaxMessages = 100;

    /// <summary>The longest message a member can send.</summary>
    public const int MaxMessageLength = 4000;

    /// <summary>The most saved prompts (personal and shared together).</summary>
    public const int MaxPrompts = 400;

    /// <summary>The longest prompt text.</summary>
    public const int MaxPromptTextLength = 4000;

    /// <summary>The longest prompt title.</summary>
    public const int MaxPromptTitleLength = 80;

    /// <summary>The highest monthly token quota an admin can set.</summary>
    public const int MaxQuota = 10_000_000;

    /// <summary>The longest retention the Owner can set, in days.</summary>
    public const int MaxRetentionDays = 3650;

    /// <summary>The most tags on a prompt.</summary>
    public const int MaxTags = 5;

    /// <summary>The longest tag.</summary>
    public const int MaxTagLength = 24;

    /// <summary>The longest conversation title.</summary>
    public const int MaxTitleLength = 80;

    /// <summary>The longest workspace name.</summary>
    public const int MaxWorkspaceNameLength = 60;

    /// <summary>The shortest prompt text.</summary>
    public const int MinPromptTextLength = 10;

    /// <summary>The lowest monthly token quota an admin can set.</summary>
    public const int MinQuota = 1_000;

    /// <summary>The shortest workspace name.</summary>
    public const int MinWorkspaceNameLength = 2;

    /// <summary>The tokens an attachment adds to a message (its name and size are read, not its content).</summary>
    private const int AttachmentTokens = 50;

    /// <summary>The most audit entries this tab adds (the seeded log stays; the oldest added entries go first).</summary>
    private const int MaxAddedAudit = 600;

    /// <summary>The most audit entries the log holds.</summary>
    private const int MaxAudit = 2000;

    /// <summary>The longest action, record, or detail of an audit entry.</summary>
    private const int MaxAuditTextLength = 300;

    /// <summary>The most notifications the tab keeps (every account together; the oldest go first).</summary>
    private const int MaxNotifications = 400;

    /// <summary>The largest the site's snapshot may grow: the store's limit less room for its envelope.</summary>
    private const int MaxSnapshotLength = ShowcaseSessionStore.MaxSnapshotLength - (48 * 1024);

    /// <summary>The most usage ledger lines (one per account, model, and day).</summary>
    private const int MaxUsageEntries = 4000;

    private const int NextIdLimit = 1_000_000;

    /// <summary>The usage history before the seed time, by member number, generated once (it never changes).</summary>
    private static readonly Lazy<IReadOnlyDictionary<int, IReadOnlyList<AiUsagePoint>>> history = new(AiSeedData.UsageHistory);

    /// <summary>The fresh tab's data, built once and never changed: a snapshot keeps only what differs from it.</summary>
    private static readonly Lazy<AiSeedSet> seed = new(AiSeedData.Build);

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<ShowcaseAuditEntry> audit;

    private readonly List<AiConversation> conversations;

    private readonly List<AiUsageEntry> ledger;

    private readonly List<AiMember> members;

    private readonly List<ShowcaseNotification> notifications;

    private readonly Dictionary<string, AiPreferences> preferences = new();

    private readonly List<AiPrompt> prompts;

    /// <summary>How many bulk changes are running; <see cref="Changed" /> is raised once when the outermost one ends.</summary>
    private int batchDepth;

    private bool changedInBatch;

    private bool isDarkTheme;

    private int nextAuditId;

    private int nextConversationId;

    private int nextMemberId;

    private int nextMessageId;

    private int nextNotificationId;

    private int nextPromptId;

    private AiPolicy policy;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out.</summary>
    public AiState()
    {
        var data = AiSeedData.Build();
        audit = data.Audit;
        conversations = data.Conversations;
        ledger = data.Usage;
        members = data.Members;
        notifications = data.Notifications;
        prompts = data.Prompts;
        policy = data.Policy;
        ResetCounters(data);

        Identity = new ShowcaseIdentity(AiSeedData.Roles(), AiSeedData.DemoAccounts());
        Identity.Changed += OnIdentityChanged;
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns how many conversations in the workspace (every member's) a retention period would delete now.</summary>
    public int ConversationsOlderThan(int days) => days <= 0 ? 0 : conversations.Count(conversation => conversation.UpdatedAt < Clock.Now.AddDays(-days));

    /// <summary>Returns how many conversations in the workspace (every member's) use a model.</summary>
    public int ConversationsUsingModel(string? modelId) => conversations.Count(conversation => conversation.ModelId == modelId);

    /// <summary>Deletes one of the signed-in member's conversations.</summary>
    public AiResult DeleteConversation(int conversationId)
    {
        if (ActiveAccount() is not { } account)
            return AiResult.NotAllowed;

        if (conversations.RemoveAll(conversation => conversation.Id == conversationId && conversation.AccountId == account.Id) == 0)
            return AiResult.NotFound;

        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>Returns one of the signed-in member's conversations, or <see langword="null" />.</summary>
    public AiConversation? FindConversation(int conversationId) =>
        Identity.Current is { } account ? conversations.FirstOrDefault(conversation => conversation.Id == conversationId && conversation.AccountId == account.Id) : null;

    /// <summary>Finds a model of the catalog by ID, or returns <see langword="null" />.</summary>
    public AiModel? FindModel(string? modelId) => Models.FirstOrDefault(model => model.Id == modelId);

    /// <summary>Returns whether an assistant reply is still streaming.</summary>
    public bool IsStreaming(AiMessage message) => message.Role == AiRole.Assistant && !message.IsStopped && VisibleLength(message) < FullReply(message).Length;

    /// <summary>Returns the tokens a member (by email) used this calendar month, including the generated history.</summary>
    public int MonthTokens(string email) =>
        (int)Math.Min(int.MaxValue, Usage(email).Where(point => point.Date.Year == Clock.Now.Year && point.Date.Month == Clock.Now.Month).Sum(point => (long)point.Tokens));

    /// <summary>Returns this calendar month's messages and tokens of every member, by email (one pass, for lists).</summary>
    public IReadOnlyDictionary<string, (int Messages, long Tokens)> MonthUsageByEmail() =>
        Usage().Where(point => point.Date.Year == Clock.Now.Year && point.Date.Month == Clock.Now.Month)
            .GroupBy(point => point.MemberKey)
            .ToDictionary(group => group.Key, group => (group.Sum(point => point.Messages), group.Sum(point => (long)point.Tokens)));

    /// <summary>Raises <see cref="Changed" /> after a snapshot was restored.</summary>
    public void NotifyRestored() => Changed?.Invoke();

    /// <summary>
    /// Regenerates the last reply of one of the member's conversations with the other template variant (and the
    /// conversation's current model, or the workspace default when an admin turned it off); the old reply is replaced.
    /// WrongState: the last message isn't a finished reply.
    /// </summary>
    public AiResult Regenerate(int conversationId, out AiMessage? reply)
    {
        reply = null;

        if (ActiveAccount() is null)
            return AiResult.NotAllowed;

        if (FindConversation(conversationId) is not { } conversation)
            return AiResult.NotFound;

        if (conversation.Messages.LastOrDefault() is not { Role: AiRole.Assistant, ReplyKey: { } key } last || IsStreaming(last))
            return AiResult.WrongState;

        // A model a workspace admin turned off gives way to the workspace default.
        var modelId = policy.EnabledModelIds.Contains(conversation.ModelId) ? conversation.ModelId : policy.DefaultModelId;

        if (FindModel(modelId) is not { } model)
            return AiResult.WrongState;

        var variant = (last.Variant + 1) % AiReplies.VariantCount(key);
        var tokens = AiSeedData.TokensOf(AiReplies.Render(key, variant, model));

        if (MonthTokens(MemberEmail()) + tokens > policy.MonthlyTokenQuota)
            return AiResult.OverQuota;

        if (nextMessageId >= NextIdLimit)
            return AiResult.Invalid;

        var created = new AiMessage { Id = nextMessageId++, Role = AiRole.Assistant, ReplyKey = key, Variant = variant, At = Clock.Now, ModelId = model.Id, Tokens = tokens };
        var index = conversation.Messages.IndexOf(last);
        conversation.Messages[index] = created;

        // The replaced reply's tokens stay counted: they were used.
        var undoUsage = RecordUsage(conversation.AccountId, model.Id, Clock.Now, 0, tokens);

        if (undoUsage is null || !FitsSnapshot())
        {
            undoUsage?.Invoke();
            conversation.Messages[index] = last;

            return AiResult.Invalid;
        }

        conversation.ModelId = model.Id;
        conversation.UpdatedAt = Clock.Now;
        reply = created;
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>Renames one of the signed-in member's conversations (plain text).</summary>
    public AiResult RenameConversation(int conversationId, string title)
    {
        if (ActiveAccount() is null)
            return AiResult.NotAllowed;

        if (FindConversation(conversationId) is not { } conversation)
            return AiResult.NotFound;

        title = title?.Trim() ?? string.Empty;

        if (!IsText(title, MaxTitleLength))
            return AiResult.Invalid;

        if (conversation.Title == title)
            return AiResult.NoChange;

        var previous = conversation.Title;
        conversation.Title = title;

        if (!FitsSnapshot())
        {
            conversation.Title = previous;

            return AiResult.Invalid;
        }

        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>Returns the Markdown of an assistant reply as far as it has streamed (the whole of it once finished).</summary>
    public string ReplyMarkdown(AiMessage message)
    {
        var full = FullReply(message);
        var visible = VisibleLength(message);

        return visible >= full.Length ? full : full[..visible];
    }

    /// <summary>
    /// Saves the workspace settings and model policy (Admins and the Owner): the workspace name, the enabled models (at least
    /// one) and the default among them, each member's monthly token quota, attachments and their size, blocked terms, and
    /// (the Owner only) retention, which deletes conversations that haven't changed for that many days, now and on later
    /// changes. Returns how many conversations retention deleted. Each change is audited.
    /// </summary>
    public AiResult SavePolicy(AiPolicyInput input, out int deletedConversations)
    {
        deletedConversations = 0;

        if (!Can(AiPermission.ManageSettings) || input is null)
            return AiResult.NotAllowed;

        var name = input.WorkspaceName?.Trim() ?? string.Empty;
        var enabled = (input.EnabledModelIds ?? Array.Empty<string>()).Distinct().ToList();
        var terms = (input.BlockedTerms ?? Array.Empty<string>()).Select(term => term?.Trim().ToLowerInvariant() ?? string.Empty).Where(term => term.Length > 0).Distinct().ToList();

        if (name.Length is < MinWorkspaceNameLength or > MaxWorkspaceNameLength || enabled.Count == 0 || enabled.Any(id => FindModel(id) is null) || !enabled.Contains(input.DefaultModelId)
            || input.MonthlyTokenQuota is < MinQuota or > MaxQuota || input.MaxAttachmentMb is < 1 or > MaxAttachmentMb || input.RetentionDays is < 0 or > MaxRetentionDays
            || terms.Count > MaxBlockedTerms || terms.Any(term => term.Length > MaxBlockedTermLength))
            return AiResult.Invalid;

        // Retention deletes every member's conversations, so only the Owner changes it.
        if (input.RetentionDays != policy.RetentionDays && !Can(AiPermission.ManageRetention))
            return AiResult.NotAllowed;

        var before = policy;
        var after = new AiPolicy
        {
            WorkspaceName = name, EnabledModelIds = enabled, DefaultModelId = input.DefaultModelId, MonthlyTokenQuota = input.MonthlyTokenQuota, AllowAttachments = input.AllowAttachments,
            MaxAttachmentMb = input.MaxAttachmentMb, RetentionDays = input.RetentionDays, BlockedTerms = terms,
        };

        if (SamePolicy(before, after))
            return AiResult.NoChange;

        policy = after;
        deletedConversations = ApplyRetention();

        if (before.WorkspaceName != after.WorkspaceName)
            Record("Renamed the workspace", "Workspace settings", SettingsPath, $"{before.WorkspaceName} → {after.WorkspaceName}");

        if (PolicyChanges(before, after) is { Count: > 0 } changes)
        {
            Record("Changed the model policy", "Models and policy", SettingsPath, string.Join("; ", changes));

            // The Owner answers for the workspace: they hear about an admin's policy change.
            if (OwnerMember() is { AccountId: { } ownerId })
                Notify(ownerId, $"{ActorName()} changed the model policy", string.Join("; ", changes), AiSeedData.AdminBase + "/audit");
        }

        if (before.RetentionDays != after.RetentionDays)
            Record("Changed retention", "Models and policy", SettingsPath,
                $"{RetentionWords(before.RetentionDays)} → {RetentionWords(after.RetentionDays)}; {deletedConversations} conversations deleted");

        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>Saves the signed-in member's preferences (the model must be one the workspace enables, or none).</summary>
    public AiResult SavePreferences(string? modelId, bool enterToSend)
    {
        if (ActiveAccount() is not { } account)
            return AiResult.NotAllowed;

        if (modelId is not null && !policy.EnabledModelIds.Contains(modelId))
            return AiResult.WrongState;

        var current = Preferences;

        if (current.ModelId == modelId && current.EnterToSend == enterToSend)
            return AiResult.NoChange;

        preferences[account.Id] = new AiPreferences { ModelId = modelId, EnterToSend = enterToSend };
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>Returns the signed-in member's conversations whose title or messages contain the text (all of them for an empty search), pinned first, then the most recent.</summary>
    public IReadOnlyList<AiConversation> SearchConversations(string? text)
    {
        var term = text?.Trim() ?? string.Empty;

        return Conversations.Where(conversation => term.Length == 0 || conversation.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
            || conversation.Messages.Any(message => message.Text.Contains(term, StringComparison.OrdinalIgnoreCase))).ToList();
    }

    /// <summary>
    /// Sends a message (plain text, up to <see cref="MaxMessageLength" /> characters, with up to <see cref="MaxAttachments" />
    /// attachments' names and sizes) to one of the member's conversations, or starts one (<paramref name="conversationId" />
    /// 0, titled from the text). The assistant's reply is added at once and streams by the clock. A prompt with a blocked
    /// term gets the policy refusal. OverQuota: this month's tokens would pass the quota; WrongState: the model is turned off
    /// or attachments are off. Chat content is never audited.
    /// </summary>
    public AiResult Send(int conversationId, string text, IReadOnlyList<AiAttachment> attachments, string modelId, out AiConversation? conversation, out AiMessage? reply)
    {
        conversation = null;
        reply = null;

        if (ActiveAccount() is not { } account)
            return AiResult.NotAllowed;

        var existing = conversationId == 0 ? null : FindConversation(conversationId);

        if (conversationId != 0 && existing is null)
            return AiResult.NotFound;

        text = text?.Trim() ?? string.Empty;
        var files = (attachments ?? Array.Empty<AiAttachment>()).ToList();

        if (!IsText(text, MaxMessageLength) || files.Count > MaxAttachments || files.Any(file => file is null || !IsText(file.Name, MaxAttachmentNameLength)
                || file.SizeBytes < 0 || file.SizeBytes > policy.MaxAttachmentMb * 1024L * 1024L || (file.ContentType?.Length ?? 0) > 100))
            return AiResult.Invalid;

        if ((files.Count > 0 && !policy.AllowAttachments) || !policy.EnabledModelIds.Contains(modelId) || FindModel(modelId) is not { } model)
            return AiResult.WrongState;

        if (existing is null && conversations.Count(item => item.AccountId == account.Id) >= MaxConversations)
            return AiResult.Invalid;

        if (existing is not null && existing.Messages.Count + 2 > MaxMessages)
            return AiResult.Invalid;

        var isBlocked = policy.BlockedTerms.Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
        var key = AiReplies.Pick(text, files.Count > 0, isBlocked);
        var variant = (int)(AiSeedData.Hash(nextMessageId, text.Length, 7) * AiReplies.VariantCount(key)) % AiReplies.VariantCount(key);
        var userTokens = AiSeedData.TokensOf(text) + (files.Count * AttachmentTokens);
        var replyTokens = AiSeedData.TokensOf(AiReplies.Render(key, variant, model));

        if (MonthTokens(MemberEmail()) + userTokens + replyTokens > policy.MonthlyTokenQuota)
            return AiResult.OverQuota;

        if (nextMessageId >= NextIdLimit - 2 || (existing is null && nextConversationId >= NextIdLimit))
            return AiResult.Invalid;

        var target = existing ?? new AiConversation
        {
            Id = nextConversationId++, AccountId = account.Id, Title = TitleFrom(text), ModelId = model.Id, CreatedAt = Clock.Now,
        };

        var userMessage = new AiMessage
        {
            Id = nextMessageId++, Role = AiRole.User, Text = text, At = Clock.Now, ModelId = model.Id, Tokens = userTokens,
            Attachments = files.Select(file => new AiAttachment(file.Name.Trim(), file.SizeBytes, Clip(file.ContentType ?? string.Empty, 100))).ToList(),
        };
        var replyMessage = new AiMessage { Id = nextMessageId++, Role = AiRole.Assistant, ReplyKey = key, Variant = variant, At = Clock.Now, ModelId = model.Id, Tokens = replyTokens };
        var previousModel = target.ModelId;
        var previousUpdated = target.UpdatedAt;

        target.Messages.Add(userMessage);
        target.Messages.Add(replyMessage);
        target.ModelId = model.Id;
        target.UpdatedAt = Clock.Now;

        if (existing is null)
            conversations.Add(target);

        var undoUsage = RecordUsage(account.Id, model.Id, Clock.Now, 1, userTokens + replyTokens);

        if (undoUsage is null || !FitsSnapshot())
        {
            undoUsage?.Invoke();
            target.Messages.Remove(userMessage);
            target.Messages.Remove(replyMessage);
            target.ModelId = previousModel;
            target.UpdatedAt = previousUpdated;

            if (existing is null)
                conversations.Remove(target);

            return AiResult.Invalid;
        }

        conversation = target;
        reply = replyMessage;
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>Changes the model of one of the member's conversations (one the workspace enables).</summary>
    public AiResult SetConversationModel(int conversationId, string modelId)
    {
        if (ActiveAccount() is null)
            return AiResult.NotAllowed;

        if (FindConversation(conversationId) is not { } conversation)
            return AiResult.NotFound;

        if (!policy.EnabledModelIds.Contains(modelId))
            return AiResult.WrongState;

        if (conversation.ModelId == modelId)
            return AiResult.NoChange;

        conversation.ModelId = modelId;
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>Rates an assistant reply in one of the member's conversations.</summary>
    public AiResult SetFeedback(int conversationId, int messageId, AiFeedback feedback)
    {
        if (ActiveAccount() is null)
            return AiResult.NotAllowed;

        if (FindConversation(conversationId)?.Messages.FirstOrDefault(message => message.Id == messageId && message.Role == AiRole.Assistant) is not { } message)
            return AiResult.NotFound;

        if (!Enum.IsDefined(feedback))
            return AiResult.Invalid;

        if (message.Feedback == feedback)
            return AiResult.NoChange;

        message.Feedback = feedback;
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>Pins or unpins one of the member's conversations.</summary>
    public AiResult SetPinned(int conversationId, bool isPinned)
    {
        if (ActiveAccount() is null)
            return AiResult.NotAllowed;

        if (FindConversation(conversationId) is not { } conversation)
            return AiResult.NotFound;

        if (conversation.IsPinned == isPinned)
            return AiResult.NoChange;

        conversation.IsPinned = isPinned;
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>Records the color mode the theme switcher applied, so charts can redraw in matching colors.</summary>
    public void SetTheme(string? themeName)
    {
        var isDark = string.Equals(themeName, "dark", StringComparison.OrdinalIgnoreCase);

        if (isDarkTheme == isDark)
            return;

        isDarkTheme = isDark;
        ThemeChanged?.Invoke();
    }

    /// <summary>Stops a reply that is still streaming; what was shown stays.</summary>
    public AiResult Stop(int conversationId, int messageId)
    {
        if (ActiveAccount() is null)
            return AiResult.NotAllowed;

        if (FindConversation(conversationId)?.Messages.FirstOrDefault(message => message.Id == messageId && message.Role == AiRole.Assistant) is not { } message)
            return AiResult.NotFound;

        if (!IsStreaming(message))
            return AiResult.NoChange;

        message.StoppedLength = VisibleLength(message);
        message.IsStopped = true;
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>
    /// Returns a member's usage (by email; everyone's for <see langword="null" />), oldest first: the generated history
    /// before the seed time, plus the usage ledger of this tab (kept when conversations are deleted).
    /// </summary>
    public IReadOnlyList<AiUsagePoint> Usage(string? email = null)
    {
        var points = new List<AiUsagePoint>();
        var generated = history.Value;

        foreach (var member in members)
        {
            if ((email is null || member.Email == email) && generated.TryGetValue(member.Id, out var memberPoints))
                points.AddRange(memberPoints.Select(point => point with { MemberKey = member.Email }));
        }

        var emails = members.Where(member => member.AccountId is not null).ToDictionary(member => member.AccountId!, member => member.Email);

        points.AddRange(ledger.Where(entry => emails.TryGetValue(entry.AccountId, out var owner) && (email is null || owner == email))
            .Select(entry => new AiUsagePoint(entry.Date, emails[entry.AccountId], entry.ModelId, entry.Messages, entry.Tokens)));

        return points.OrderBy(point => point.Date).ToList();
    }

    /// <summary>Marks a saved prompt used (the member put it in the composer).</summary>
    public AiResult UsePrompt(int promptId)
    {
        if (ActiveAccount() is null)
            return AiResult.NotAllowed;

        if (FindPrompt(promptId) is not { } prompt)
            return AiResult.NotFound;

        prompt.UseCount = Math.Min(prompt.UseCount + 1, 1_000_000);
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>Returns how many characters of an assistant reply are shown now: by the clock and the model's speed, or where it was stopped.</summary>
    public int VisibleLength(AiMessage message)
    {
        var full = FullReply(message).Length;

        if (message.IsStopped)
            return Math.Clamp(message.StoppedLength, 0, full);

        var speed = (FindModel(message.ModelId)?.TokensPerSecond ?? 40) * 4.0;
        var shown = (Clock.Now - message.At).TotalSeconds * speed;

        return shown >= full ? full : Math.Max(0, (int)shown);
    }

    #endregion

    #region Private Methods

    private static string Clip(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];

    [GeneratedRegex(@"^[^@\s]{1,64}@[^@\s]+\.[^@\s]{2,}$")]
    private static partial Regex EmailPattern();

    private static bool IsAccountId(string? accountId) =>
        accountId is not null && (AiSeedData.DemoAccounts().Any(demo => demo.Id == accountId) || (accountId.StartsWith("u-", StringComparison.Ordinal) && accountId.Length <= 12));

    private static bool IsPlausible(DateTime value) => value >= AiSeedData.Now.AddYears(-3) && value <= AiSeedData.Now.AddYears(2);

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

    /// <summary>What changed in the model policy, in words, for the audit log (the name and retention have their own entries).</summary>
    private static List<string> PolicyChanges(AiPolicy before, AiPolicy after)
    {
        var changes = new List<string>();
        string Names(IEnumerable<string> ids) => string.Join(", ", AiSeedData.Models().Where(model => ids.Contains(model.Id)).Select(model => model.Name));

        if (!before.EnabledModelIds.Order().SequenceEqual(after.EnabledModelIds.Order()))
            changes.Add($"Models on: {Names(after.EnabledModelIds)}");

        if (before.DefaultModelId != after.DefaultModelId)
            changes.Add($"Default model: {AiSeedData.FindModel(before.DefaultModelId)?.Name} → {AiSeedData.FindModel(after.DefaultModelId)?.Name}");

        if (before.MonthlyTokenQuota != after.MonthlyTokenQuota)
            changes.Add(FormattableString.Invariant($"Monthly quota: {before.MonthlyTokenQuota:#,0} → {after.MonthlyTokenQuota:#,0} tokens per member"));

        if (before.AllowAttachments != after.AllowAttachments || before.MaxAttachmentMb != after.MaxAttachmentMb)
            changes.Add(FormattableString.Invariant($"Attachments: {(before.AllowAttachments ? $"on, {before.MaxAttachmentMb} MB" : "off")} → {(after.AllowAttachments ? $"on, {after.MaxAttachmentMb} MB" : "off")}"));

        if (!before.BlockedTerms.SequenceEqual(after.BlockedTerms))
            changes.Add(FormattableString.Invariant($"Blocked terms: {before.BlockedTerms.Count} → {after.BlockedTerms.Count}"));

        return changes;
    }

    private static string RetentionWords(int days) => days == 0 ? "Keep forever" : FormattableString.Invariant($"{days} days");

    private static bool SamePolicy(AiPolicy left, AiPolicy right) =>
        left.WorkspaceName == right.WorkspaceName && left.EnabledModelIds.Order().SequenceEqual(right.EnabledModelIds.Order()) && left.DefaultModelId == right.DefaultModelId
        && left.MonthlyTokenQuota == right.MonthlyTokenQuota && left.AllowAttachments == right.AllowAttachments && left.MaxAttachmentMb == right.MaxAttachmentMb
        && left.RetentionDays == right.RetentionDays && left.BlockedTerms.SequenceEqual(right.BlockedTerms);

    [GeneratedRegex("^[a-z0-9][a-z0-9-]*$")]
    private static partial Regex TagPattern();

    /// <summary>A conversation title from its first message: the first line, cut at a word near 60 characters.</summary>
    private static string TitleFrom(string text)
    {
        var line = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? text;

        if (line.Length <= 60)
            return line;

        var cut = line.LastIndexOf(' ', 60);

        return (cut > 20 ? line[..cut] : line[..60]).TrimEnd() + "…";
    }

    private static bool Unique<T>(IEnumerable<T> values)
    {
        var list = values.ToList();

        return list.Distinct().Count() == list.Count;
    }

    /// <summary>The signed-in account when it is an active member, or <see langword="null" />.</summary>
    private ShowcaseAccount? ActiveAccount() =>
        Identity.Current is { } account && members.Any(member => member.AccountId == account.Id && member.Status == AiMemberStatus.Active) ? account : null;

    /// <summary>Deletes conversations that haven't changed within the retention period; returns how many (the caller audits it).</summary>
    private int ApplyRetention() =>
        policy.RetentionDays <= 0 ? 0 : conversations.RemoveAll(conversation => conversation.UpdatedAt < Clock.Now.AddDays(-policy.RetentionDays));

    /// <summary>Whether the snapshot, as it would be saved now, still fits under <see cref="MaxSnapshotLength" />.</summary>
    private bool FitsSnapshot()
    {
        try
        {
            return JsonSerializer.Serialize(CaptureData(), snapshotOptions).Length <= MaxSnapshotLength;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            return false;
        }
    }

    private string FullReply(AiMessage message) => message.ReplyKey is null ? string.Empty : AiReplies.Render(message.ReplyKey, message.Variant, FindModel(message.ModelId));

    private string MemberEmail() => members.FirstOrDefault(member => member.AccountId == Identity.Current?.Id)?.Email ?? string.Empty;

    /// <summary>
    /// A newly signed-in account joins the workspace: it takes up the membership for its email that has no account yet (an
    /// invitation, with the invited role, or a seeded colleague's seat, as it is), or becomes a member while there is room;
    /// a join is audited, and the Owner and admins hear about it. A member's name and email follow their profile. Then
    /// retention runs, in case time passed.
    /// </summary>
    private void OnIdentityChanged()
    {
        if (Identity.Current is { } account)
        {
            var email = account.Email.Trim().ToLowerInvariant();
            var name = Clip(account.Name, ShowcaseIdentity.MaxNameLength);

            if (members.FirstOrDefault(member => member.AccountId == account.Id) is { } current)
            {
                current.Name = name;

                if (current.Email != email && IsEmail(email) && FindMember(email) is null)
                    current.Email = email;
            }
            else if (FindMember(email) is { AccountId: null } seat)
            {
                seat.AccountId = account.Id;
                seat.Name = name;

                if (seat.Status == AiMemberStatus.Invited)
                {
                    seat.Status = AiMemberStatus.Active;
                    seat.JoinedAt = Clock.Now;
                    Record("Joined the workspace", MemberTarget(seat), AiSeedData.MembersPath(seat.Email), $"Took up the invitation as {RoleWords(seat.Role)}", system: true);
                    NotifyManagers($"{seat.Name} joined {policy.WorkspaceName}", $"They took up their invitation and joined as {RoleWords(seat.Role)}.", AiSeedData.MembersPath(seat.Email));
                }
            }
            else if (FindMember(email) is null && members.Count < MaxMembers && IsEmail(email) && nextMemberId < NextIdLimit)
            {
                var joined = new AiMember
                {
                    Id = nextMemberId++, AccountId = account.Id, Email = email, Name = name, Role = AiMemberRole.Member, Status = AiMemberStatus.Active, JoinedAt = Clock.Now,
                };
                members.Add(joined);
                Record("Joined the workspace", MemberTarget(joined), AiSeedData.MembersPath(joined.Email), "Created an account; joined as a member", system: true);
                NotifyManagers($"{joined.Name} joined {policy.WorkspaceName}", "They created an account and joined as a member.", AiSeedData.MembersPath(joined.Email));
            }
        }

        if (ApplyRetention() is > 0 and var deleted)
            Record("Applied retention", "Models and policy", SettingsPath, FormattableString.Invariant($"{deleted} conversations older than {policy.RetentionDays} days deleted"), system: true);

        Changed?.Invoke();
    }

    /// <summary>Raises <see cref="Changed" />, or once at the end of a bulk change.</summary>
    private void RaiseChanged()
    {
        if (batchDepth > 0)
        {
            changedInBatch = true;

            return;
        }

        Changed?.Invoke();
    }

    /// <summary>
    /// Adds tokens (and messages) to the ledger line of an account, model, and day; returns how to take them back, or
    /// <see langword="null" /> when the ledger is full.
    /// </summary>
    private Action? RecordUsage(string accountId, string modelId, DateTime at, int messages, int tokens)
    {
        var date = at.Date;

        if (ledger.FirstOrDefault(entry => entry.AccountId == accountId && entry.ModelId == modelId && entry.Date == date) is { } line)
        {
            line.Messages += messages;
            line.Tokens += tokens;

            return () =>
            {
                line.Messages -= messages;
                line.Tokens -= tokens;
            };
        }

        if (ledger.Count >= MaxUsageEntries)
            return null;

        var added = new AiUsageEntry { AccountId = accountId, ModelId = modelId, Date = date, Messages = messages, Tokens = tokens };
        ledger.Add(added);

        return () => ledger.Remove(added);
    }

    /// <summary>Sets the ID counters past the highest IDs of a fresh tab's data.</summary>
    private void ResetCounters(AiSeedSet data)
    {
        nextAuditId = data.Audit.Count == 0 ? 1 : data.Audit.Max(entry => entry.Id) + 1;
        nextConversationId = data.Conversations.Max(conversation => conversation.Id) + 1;
        nextMemberId = data.Members.Max(member => member.Id) + 1;
        nextMessageId = data.Conversations.SelectMany(conversation => conversation.Messages).Max(message => message.Id) + 1;
        nextNotificationId = data.Notifications.Count == 0 ? 1 : data.Notifications.Max(notice => notice.Id) + 1;
        nextPromptId = data.Prompts.Max(prompt => prompt.Id) + 1;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the site's clock, which moves on from the seed time (D-029).</summary>
    public ShowcaseClock Clock { get; } = new(AiSeedData.Now);

    /// <summary>Gets the signed-in member's conversations, pinned first, then the most recently changed.</summary>
    public IReadOnlyList<AiConversation> Conversations =>
        Identity.Current is { } account
            ? conversations.Where(conversation => conversation.AccountId == account.Id).OrderByDescending(conversation => conversation.IsPinned).ThenByDescending(conversation => conversation.UpdatedAt).ToList()
            : Array.Empty<AiConversation>();

    /// <summary>Gets the signed-in account's membership, or <see langword="null" />.</summary>
    public AiMember? CurrentMember => Identity.Current is { } account ? members.FirstOrDefault(member => member.AccountId == account.Id) : null;

    /// <summary>Gets the models members can choose now.</summary>
    public IReadOnlyList<AiModel> EnabledModels => Models.Where(model => policy.EnabledModelIds.Contains(model.Id)).ToList();

    /// <summary>Gets the site's demo sign-in.</summary>
    public ShowcaseIdentity Identity { get; }

    /// <summary>Gets whether the signed-in account is an active member of the workspace.</summary>
    public bool IsActiveMember => CurrentMember is { Status: AiMemberStatus.Active };

    /// <summary>Gets whether charts should use dark-theme colors.</summary>
    public bool IsDarkTheme => isDarkTheme;

    /// <summary>Gets the model catalog.</summary>
    public IReadOnlyList<AiModel> Models { get; } = AiSeedData.Models();

    /// <summary>Gets the workspace policy (read it; change it with <see cref="SavePolicy" />).</summary>
    public AiPolicy Policy => policy;

    /// <summary>Gets the model a new conversation starts with: the member's preference when the workspace enables it, else the workspace default.</summary>
    public string PreferredModelId => Preferences.ModelId is { } preferred && policy.EnabledModelIds.Contains(preferred) ? preferred : policy.DefaultModelId;

    /// <summary>Gets the signed-in member's preferences (the defaults until saved).</summary>
    public AiPreferences Preferences => Identity.Current is { } account && preferences.TryGetValue(account.Id, out var saved) ? saved : new AiPreferences();

    /// <summary>Gets the site's slug, used in the storage key.</summary>
    public string SiteSlug => "ai-assistant";

    /// <summary>Gets the workspace's name (the Owner and admins rename it in the settings).</summary>
    public string WorkspaceName => policy.WorkspaceName;

    private static string SettingsPath => AiSeedData.AdminBase + "/settings";

    #endregion

    #region Events

    /// <summary>Raised when anything in the state changes, including sign-in.</summary>
    public event Action? Changed;

    /// <summary>Raised when the site's theme changes, so charts can redraw.</summary>
    public event Action? ThemeChanged;

    #endregion
}
