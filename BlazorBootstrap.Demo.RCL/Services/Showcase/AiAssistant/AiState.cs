using System.Text.Json;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

/// <summary>
/// State of "Parley", the AI assistant workspace, for the current browser tab: members, conversations, saved prompts, the
/// workspace policy, members' preferences, and the demo sign-in. It is kept in the tab's <c>sessionStorage</c> through the
/// showcase foundation; nothing is sent and no model is called.
/// </summary>
/// <remarks>
/// Assistant replies are fixed templates (<see cref="AiReplies" />): a reply stores only its template key and variant, and
/// its text streams by the clock (characters shown = seconds since it was sent × the model's speed) until it finishes or
/// is stopped. Members' rights come from their workspace membership (<see cref="AiMember" />): an active member chats;
/// a workspace admin also manages members, shared prompts, and the policy. A new account joins as a member, or takes up
/// its invitation.
/// </remarks>
public sealed partial class AiState : IShowcaseSiteState
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
    public const int MaxConversations = 100;

    /// <summary>The most members (and invitations).</summary>
    public const int MaxMembers = 50;

    /// <summary>The most messages in a conversation (each turn is two).</summary>
    public const int MaxMessages = 100;

    /// <summary>The longest message a member can send.</summary>
    public const int MaxMessageLength = 4000;

    /// <summary>The most saved prompts (personal and shared together).</summary>
    public const int MaxPrompts = 100;

    /// <summary>The longest prompt text.</summary>
    public const int MaxPromptTextLength = 4000;

    /// <summary>The longest prompt title.</summary>
    public const int MaxPromptTitleLength = 80;

    /// <summary>The highest monthly token quota an admin can set.</summary>
    public const int MaxQuota = 10_000_000;

    /// <summary>The longest retention an admin can set, in days.</summary>
    public const int MaxRetentionDays = 3650;

    /// <summary>The most tags on a prompt.</summary>
    public const int MaxTags = 5;

    /// <summary>The longest tag.</summary>
    public const int MaxTagLength = 24;

    /// <summary>The longest conversation title.</summary>
    public const int MaxTitleLength = 80;

    /// <summary>The lowest monthly token quota an admin can set.</summary>
    public const int MinQuota = 1_000;

    /// <summary>The largest the site's snapshot may grow: the store's limit less room for its envelope.</summary>
    private const int MaxSnapshotLength = ShowcaseSessionStore.MaxSnapshotLength - (48 * 1024);

    /// <summary>The tokens an attachment adds to a message (its name and size are read, not its content).</summary>
    private const int AttachmentTokens = 50;

    /// <summary>The most usage ledger lines (one per account, model, and day).</summary>
    private const int MaxUsageEntries = 2000;

    private const int NextIdLimit = 1_000_000;

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<AiConversation> conversations = AiSeedData.Conversations();

    private readonly List<AiUsageEntry> ledger = new();

    private readonly List<AiMember> members = AiSeedData.Members();

    private readonly Dictionary<string, AiPreferences> preferences = new();

    private readonly List<AiPrompt> prompts = AiSeedData.Prompts();

    private bool isDarkTheme;

    private int nextConversationId;

    private int nextMessageId;

    private int nextPromptId;

    private AiPolicy policy = AiSeedData.Policy();

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out.</summary>
    public AiState()
    {
        nextConversationId = conversations.Max(conversation => conversation.Id) + 1;
        nextMessageId = conversations.SelectMany(conversation => conversation.Messages).Max(message => message.Id) + 1;
        nextPromptId = prompts.Max(prompt => prompt.Id) + 1;

        foreach (var conversation in conversations)
            foreach (var message in conversation.Messages)
                Record(conversation.AccountId, message.ModelId, message.At, message.Role == AiRole.User ? 1 : 0, message.Tokens);

        Identity = new ShowcaseIdentity(AiSeedData.Roles(), AiSeedData.DemoAccounts());
        Identity.Changed += OnIdentityChanged;
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the snapshot: members, conversations, prompts, the policy, preferences, and the ID counters.</summary>
    public object CaptureData() => new AiSnapshot
    {
        Conversations = conversations.ToList(),
        Members = members.ToList(),
        NextConversationId = nextConversationId,
        NextMessageId = nextMessageId,
        NextPromptId = nextPromptId,
        Policy = policy,
        Preferences = preferences.ToDictionary(pair => pair.Key, pair => pair.Value),
        Prompts = prompts.ToList(),
        Usage = ledger.ToList(),
    };

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

        Changed?.Invoke();

        return AiResult.Ok;
    }

    /// <summary>Deletes one of the member's own prompts, or (workspace admins) a shared prompt.</summary>
    public AiResult DeletePrompt(int promptId)
    {
        if (ActiveAccount() is not { } account)
            return AiResult.NotAllowed;

        if (prompts.FirstOrDefault(prompt => prompt.Id == promptId) is not { } prompt)
            return AiResult.NotFound;

        if (prompt.IsShared ? !IsWorkspaceAdmin : prompt.OwnerId != account.Id)
            return prompt.IsShared ? AiResult.NotAllowed : AiResult.NotFound;

        prompts.Remove(prompt);
        Changed?.Invoke();

        return AiResult.Ok;
    }

    /// <summary>Returns one of the signed-in member's conversations, or <see langword="null" />.</summary>
    public AiConversation? FindConversation(int conversationId) =>
        Identity.Current is { } account ? conversations.FirstOrDefault(conversation => conversation.Id == conversationId && conversation.AccountId == account.Id) : null;

    /// <summary>Finds a member (or invitation) by email address, or returns <see langword="null" />.</summary>
    public AiMember? FindMember(string? email) => members.FirstOrDefault(member => string.Equals(member.Email, email?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Finds a model of the catalog by ID, or returns <see langword="null" />.</summary>
    public AiModel? FindModel(string? modelId) => Models.FirstOrDefault(model => model.Id == modelId);

    /// <summary>Returns a prompt the signed-in member can see (their own or a shared one), or <see langword="null" />.</summary>
    public AiPrompt? FindPrompt(int promptId) =>
        Identity.Current is { } account ? prompts.FirstOrDefault(prompt => prompt.Id == promptId && (prompt.IsShared || prompt.OwnerId == account.Id)) : null;

    /// <summary>
    /// Invites someone by email (workspace admins): they join with the role when they create an account with that address.
    /// </summary>
    public AiResult Invite(string email, string name, AiMemberRole role, out AiMember? invited)
    {
        invited = null;

        if (!IsWorkspaceAdmin)
            return AiResult.NotAllowed;

        email = (email ?? string.Empty).Trim().ToLowerInvariant();
        name = name?.Trim() ?? string.Empty;

        if (!IsEmail(email) || !IsText(name, ShowcaseIdentity.MaxNameLength) || !Enum.IsDefined(role))
            return AiResult.Invalid;

        if (FindMember(email) is not null)
            return AiResult.Duplicate;

        if (members.Count >= MaxMembers)
            return AiResult.Invalid;

        invited = new AiMember { Email = email, Name = name, Role = role, Status = AiMemberStatus.Invited, JoinedAt = Clock.Now };
        members.Add(invited);

        if (!FitsSnapshot())
        {
            members.Remove(invited);
            invited = null;

            return AiResult.Invalid;
        }

        Changed?.Invoke();

        return AiResult.Ok;
    }

    /// <summary>Returns whether an assistant reply is still streaming.</summary>
    public bool IsStreaming(AiMessage message) => message.Role == AiRole.Assistant && !message.IsStopped && VisibleLength(message) < FullReply(message).Length;

    /// <summary>Returns the tokens a member (by email) used this calendar month, including the generated history.</summary>
    public int MonthTokens(string email) =>
        (int)Math.Min(int.MaxValue, Usage(email).Where(point => point.Date.Year == Clock.Now.Year && point.Date.Month == Clock.Now.Month).Sum(point => (long)point.Tokens));

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
        var undoUsage = Record(conversation.AccountId, model.Id, Clock.Now, 0, tokens);

        if (undoUsage is null || !FitsSnapshot())
        {
            undoUsage?.Invoke();
            conversation.Messages[index] = last;

            return AiResult.Invalid;
        }

        conversation.ModelId = model.Id;
        conversation.UpdatedAt = Clock.Now;
        reply = created;
        Changed?.Invoke();

        return AiResult.Ok;
    }

    /// <summary>Removes an invitation that wasn't taken up (workspace admins).</summary>
    public AiResult RemoveInvitation(string email)
    {
        if (!IsWorkspaceAdmin)
            return AiResult.NotAllowed;

        if (FindMember(email) is not { } member)
            return AiResult.NotFound;

        if (member.Status != AiMemberStatus.Invited)
            return AiResult.WrongState;

        members.Remove(member);
        Changed?.Invoke();

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

        Changed?.Invoke();

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
    /// Saves the workspace policy (workspace admins): the enabled models (at least one) and the default among them, each
    /// member's monthly token quota, attachments and their size, retention (deleting conversations that haven't changed for
    /// that many days, now and on later changes), and blocked terms. Returns how many conversations retention deleted.
    /// </summary>
    public AiResult SavePolicy(IReadOnlyList<string> enabledModelIds, string defaultModelId, int monthlyTokenQuota, bool allowAttachments, int maxAttachmentMb, int retentionDays,
        IReadOnlyList<string> blockedTerms, out int deletedConversations)
    {
        deletedConversations = 0;

        if (!IsWorkspaceAdmin)
            return AiResult.NotAllowed;

        var enabled = (enabledModelIds ?? Array.Empty<string>()).Distinct().ToList();
        var terms = (blockedTerms ?? Array.Empty<string>()).Select(term => term?.Trim().ToLowerInvariant() ?? string.Empty).Where(term => term.Length > 0).Distinct().ToList();

        if (enabled.Count == 0 || enabled.Any(id => FindModel(id) is null) || !enabled.Contains(defaultModelId) || monthlyTokenQuota is < MinQuota or > MaxQuota
            || maxAttachmentMb is < 1 or > 100 || retentionDays is < 0 or > MaxRetentionDays || terms.Count > MaxBlockedTerms || terms.Any(term => term.Length > MaxBlockedTermLength))
            return AiResult.Invalid;

        var unchanged = policy.EnabledModelIds.Order().SequenceEqual(enabled.Order()) && policy.DefaultModelId == defaultModelId && policy.MonthlyTokenQuota == monthlyTokenQuota
            && policy.AllowAttachments == allowAttachments && policy.MaxAttachmentMb == maxAttachmentMb && policy.RetentionDays == retentionDays && policy.BlockedTerms.SequenceEqual(terms);

        if (unchanged)
            return AiResult.NoChange;

        policy = new AiPolicy
        {
            EnabledModelIds = enabled, DefaultModelId = defaultModelId, MonthlyTokenQuota = monthlyTokenQuota, AllowAttachments = allowAttachments,
            MaxAttachmentMb = maxAttachmentMb, RetentionDays = retentionDays, BlockedTerms = terms,
        };
        deletedConversations = ApplyRetention();
        Changed?.Invoke();

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
        Changed?.Invoke();

        return AiResult.Ok;
    }

    /// <summary>
    /// Creates (<paramref name="promptId" /> 0) or edits a saved prompt: a member's own, or with <paramref name="shared" /> a
    /// workspace prompt (workspace admins only). Tags are lower-case words; at most <see cref="MaxTags" />.
    /// </summary>
    public AiResult SavePrompt(int promptId, string title, string text, IReadOnlyList<string> tags, bool shared, out AiPrompt? saved)
    {
        saved = null;

        if (ActiveAccount() is not { } account)
            return AiResult.NotAllowed;

        if (shared && !IsWorkspaceAdmin)
            return AiResult.NotAllowed;

        var existing = promptId == 0 ? null : prompts.FirstOrDefault(prompt => prompt.Id == promptId);

        if (promptId != 0 && (existing is null || (existing.IsShared ? !shared : existing.OwnerId != account.Id || shared)))
            return existing is { IsShared: true } && !IsWorkspaceAdmin ? AiResult.NotAllowed : AiResult.NotFound;

        title = title?.Trim() ?? string.Empty;
        text = text?.Trim() ?? string.Empty;
        var cleanTags = (tags ?? Array.Empty<string>()).Select(tag => tag?.Trim().ToLowerInvariant() ?? string.Empty).Where(tag => tag.Length > 0).Distinct().ToList();

        if (!IsText(title, MaxPromptTitleLength) || !IsText(text, MaxPromptTextLength) || cleanTags.Count > MaxTags || cleanTags.Any(tag => tag.Length > MaxTagLength || !TagPattern().IsMatch(tag)))
            return AiResult.Invalid;

        if (existing is null)
        {
            if (prompts.Count >= MaxPrompts || nextPromptId >= NextIdLimit)
                return AiResult.Invalid;

            saved = new AiPrompt { Id = nextPromptId++, OwnerId = shared ? null : account.Id, Author = Clip(account.Name, ShowcaseIdentity.MaxNameLength), Title = title, Text = text, Tags = cleanTags, UpdatedAt = Clock.Now };
            prompts.Add(saved);

            if (!FitsSnapshot())
            {
                prompts.Remove(saved);
                saved = null;

                return AiResult.Invalid;
            }

            Changed?.Invoke();

            return AiResult.Ok;
        }

        if (existing.Title == title && existing.Text == text && existing.Tags.SequenceEqual(cleanTags))
        {
            saved = existing;

            return AiResult.NoChange;
        }

        var before = (existing.Title, existing.Text, existing.Tags, existing.UpdatedAt);
        (existing.Title, existing.Text, existing.Tags, existing.UpdatedAt) = (title, text, cleanTags, Clock.Now);

        if (!FitsSnapshot())
        {
            (existing.Title, existing.Text, existing.Tags, existing.UpdatedAt) = before;

            return AiResult.Invalid;
        }

        saved = existing;
        Changed?.Invoke();

        return AiResult.Ok;
    }

    /// <summary>Returns the signed-in member's conversations whose title or messages contain the text (all of them for an empty search), pinned first, then the most recent.</summary>
    public IReadOnlyList<AiConversation> Search(string? text)
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
    /// or attachments are off.
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

        var undoUsage = Record(account.Id, model.Id, Clock.Now, 1, userTokens + replyTokens);

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
        Changed?.Invoke();

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
        Changed?.Invoke();

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
        Changed?.Invoke();

        return AiResult.Ok;
    }

    /// <summary>
    /// Changes a member's workspace role or status (workspace admins): promote, demote, deactivate, or reactivate. The last
    /// active admin who can sign in can't be demoted or deactivated, and admins can't deactivate themselves.
    /// </summary>
    public AiResult SetMember(string email, AiMemberRole role, AiMemberStatus status)
    {
        if (!IsWorkspaceAdmin || Identity.Current is not { } account)
            return AiResult.NotAllowed;

        if (FindMember(email) is not { } member)
            return AiResult.NotFound;

        if (!Enum.IsDefined(role) || !Enum.IsDefined(status))
            return AiResult.Invalid;

        // An invitation only changes the role it joins with; its status changes when the person signs up.
        if (member.Status == AiMemberStatus.Invited)
        {
            if (status != AiMemberStatus.Invited)
                return AiResult.WrongState;

            if (member.Role == role)
                return AiResult.NoChange;

            member.Role = role;
            Changed?.Invoke();

            return AiResult.Ok;
        }

        if (status == AiMemberStatus.Invited)
            return AiResult.Invalid;

        if (member.Role == role && member.Status == status)
            return AiResult.NoChange;

        // Only admins with an account in this tab can sign in, so only they keep the workspace administrable.
        var activeAdmins = members.Count(item => item.Role == AiMemberRole.Admin && item.Status == AiMemberStatus.Active && item.AccountId is not null);
        var losesAdmin = member.Role == AiMemberRole.Admin && member.Status == AiMemberStatus.Active && (role != AiMemberRole.Admin || status != AiMemberStatus.Active);

        if ((losesAdmin && activeAdmins <= 1) || (member.AccountId == account.Id && status == AiMemberStatus.Deactivated))
            return AiResult.WrongState;

        member.Role = role;
        member.Status = status;
        Changed?.Invoke();

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
        Changed?.Invoke();

        return AiResult.Ok;
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
        Changed?.Invoke();

        return AiResult.Ok;
    }

    /// <summary>Validates a stored snapshot against its own data and applies it only when every part is valid.</summary>
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

        if (snapshot is not
            {
                Conversations: { } storedConversations,
                Members: { Count: > 0 and <= MaxMembers } storedMembers,
                Policy: { } storedPolicy,
                Preferences: { Count: <= MaxMembers } storedPreferences,
                Prompts: { Count: <= MaxPrompts } storedPrompts,
            }
            || snapshot.NextConversationId is < 1 or > NextIdLimit || snapshot.NextMessageId is < 1 or > NextIdLimit || snapshot.NextPromptId is < 1 or > NextIdLimit)
            return false;

        if (storedPolicy.EnabledModelIds is not { Count: > 0 } || storedPolicy.EnabledModelIds.Any(id => FindModel(id) is null) || !Unique(storedPolicy.EnabledModelIds)
            || !storedPolicy.EnabledModelIds.Contains(storedPolicy.DefaultModelId) || storedPolicy.MonthlyTokenQuota is < MinQuota or > MaxQuota
            || storedPolicy.MaxAttachmentMb is < 1 or > 100 || storedPolicy.RetentionDays is < 0 or > MaxRetentionDays || storedPolicy.BlockedTerms is not { Count: <= MaxBlockedTerms }
            || storedPolicy.BlockedTerms.Any(term => !IsText(term, MaxBlockedTermLength) || term != term.Trim().ToLowerInvariant()) || !Unique(storedPolicy.BlockedTerms))
            return false;

        if (storedMembers.Any(member => member is null || !IsEmail(member.Email) || member.Email != member.Email.ToLowerInvariant() || !IsText(member.Name, ShowcaseIdentity.MaxNameLength)
                || !Enum.IsDefined(member.Role) || !Enum.IsDefined(member.Status) || !IsPlausible(member.JoinedAt)
                // An invitation has no account yet; seeded colleagues have none either (they can't sign in).
                || (member.Status == AiMemberStatus.Invited && member.AccountId is not null) || (member.AccountId is not null && !IsAccountId(member.AccountId)))
            || !Unique(storedMembers.Select(member => member.Email)) || !Unique(storedMembers.Where(member => member.AccountId is not null).Select(member => member.AccountId))
            || !storedMembers.Any(member => member.Role == AiMemberRole.Admin && member.Status == AiMemberStatus.Active && member.AccountId is not null))
            return false;

        var accountIds = storedMembers.Where(member => member.AccountId is not null).Select(member => member.AccountId!).ToHashSet();

        if (storedConversations.Any(conversation => !ValidConversation(conversation, snapshot, accountIds))
            || !Unique(storedConversations.Select(conversation => conversation.Id))
            || !Unique(storedConversations.SelectMany(conversation => conversation.Messages).Select(message => message.Id))
            || storedConversations.GroupBy(conversation => conversation.AccountId).Any(group => group.Count() > MaxConversations))
            return false;

        if (storedPrompts.Any(prompt => prompt is null || prompt.Id < 1 || prompt.Id >= snapshot.NextPromptId || (prompt.OwnerId is not null && !accountIds.Contains(prompt.OwnerId))
                || !IsText(prompt.Author, ShowcaseIdentity.MaxNameLength) || !IsText(prompt.Title, MaxPromptTitleLength) || !IsText(prompt.Text, MaxPromptTextLength)
                || prompt.Tags is not { Count: <= MaxTags } || prompt.Tags.Any(tag => tag is null || tag.Length > MaxTagLength || !TagPattern().IsMatch(tag)) || !Unique(prompt.Tags)
                || !IsPlausible(prompt.UpdatedAt) || prompt.UseCount is < 0 or > 1_000_000)
            || !Unique(storedPrompts.Select(prompt => prompt.Id)))
            return false;

        if (storedPreferences.Any(pair => !accountIds.Contains(pair.Key) || pair.Value is null || (pair.Value.ModelId is not null && FindModel(pair.Value.ModelId) is null)))
            return false;

        // The ledger: one line per account, model, and day; a month never holds more than the highest quota (a send is refused past the quota).
        if (snapshot.Usage is not { Count: <= MaxUsageEntries } storedUsage
            || storedUsage.Any(entry => entry is null || !accountIds.Contains(entry.AccountId) || FindModel(entry.ModelId) is null || entry.Date != entry.Date.Date
                || entry.Date < AiSeedData.Now.Date.AddDays(-60) || entry.Date > AiSeedData.Now + ShowcaseClock.MaxElapsed || entry.Messages is < 0 or > 100_000 || entry.Tokens is < 0 or > MaxQuota)
            || !Unique(storedUsage.Select(entry => (entry.Date, entry.AccountId, entry.ModelId)))
            || storedUsage.GroupBy(entry => (entry.AccountId, entry.Date.Year, entry.Date.Month)).Any(group => group.Sum(entry => (long)entry.Tokens) > MaxQuota))
            return false;

        conversations.Clear();
        conversations.AddRange(storedConversations);
        members.Clear();
        members.AddRange(storedMembers);
        prompts.Clear();
        prompts.AddRange(storedPrompts);
        ledger.Clear();
        ledger.AddRange(storedUsage);
        preferences.Clear();

        foreach (var pair in storedPreferences)
            preferences[pair.Key] = pair.Value;

        policy = storedPolicy;
        nextConversationId = snapshot.NextConversationId;
        nextMessageId = snapshot.NextMessageId;
        nextPromptId = snapshot.NextPromptId;

        return true;
    }

    /// <summary>
    /// Returns a member's usage (by email; everyone's for <see langword="null" />), oldest first: the generated history
    /// before the seed time, plus the usage ledger of this tab (kept when conversations are deleted).
    /// </summary>
    public IReadOnlyList<AiUsagePoint> Usage(string? email = null)
    {
        var points = AiSeedData.UsageHistory(members).ToList();
        var emails = members.Where(member => member.AccountId is not null).ToDictionary(member => member.AccountId!, member => member.Email);

        points.AddRange(ledger.Where(entry => emails.ContainsKey(entry.AccountId))
            .Select(entry => new AiUsagePoint(entry.Date, emails[entry.AccountId], entry.ModelId, entry.Messages, entry.Tokens)));

        return points.Where(point => email is null || point.MemberKey == email).OrderBy(point => point.Date).ToList();
    }

    /// <summary>Marks a saved prompt used (the member put it in the composer).</summary>
    public AiResult UsePrompt(int promptId)
    {
        if (ActiveAccount() is null)
            return AiResult.NotAllowed;

        if (FindPrompt(promptId) is not { } prompt)
            return AiResult.NotFound;

        prompt.UseCount = Math.Min(prompt.UseCount + 1, 1_000_000);
        Changed?.Invoke();

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

    private static bool IsEmail(string? value) => value is not null && value.Length <= ShowcaseIdentity.MaxEmailLength && EmailPattern().IsMatch(value);

    private static bool IsPlausible(DateTime value) => value >= AiSeedData.Now.AddYears(-3) && value <= AiSeedData.Now.AddYears(2);

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

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

    /// <summary>Deletes conversations that haven't changed within the retention period; returns how many.</summary>
    private int ApplyRetention() => policy.RetentionDays <= 0 ? 0 : conversations.RemoveAll(conversation => conversation.UpdatedAt < Clock.Now.AddDays(-policy.RetentionDays));

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
    /// invitation, with the invited role, or a seeded colleague's seat, as it is), or becomes a member while there is room.
    /// A member's name and email follow their profile. Then retention runs, in case time passed.
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
                }
            }
            else if (FindMember(email) is null && members.Count < MaxMembers && IsEmail(email))
            {
                members.Add(new AiMember
                {
                    AccountId = account.Id, Email = email, Name = name, Role = AiMemberRole.Member, Status = AiMemberStatus.Active, JoinedAt = Clock.Now,
                });
            }
        }

        ApplyRetention();
        Changed?.Invoke();
    }

    /// <summary>
    /// Adds tokens (and messages) to the ledger line of an account, model, and day; returns how to take them back, or
    /// <see langword="null" /> when the ledger is full.
    /// </summary>
    private Action? Record(string accountId, string modelId, DateTime at, int messages, int tokens)
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

    /// <summary>Checks one stored conversation: owner, IDs, title, model, times, and every message (assistant replies must name a template).</summary>
    private bool ValidConversation(AiConversation? conversation, AiSnapshot snapshot, HashSet<string> accountIds)
    {
        if (conversation is null || conversation.Id < 1 || conversation.Id >= snapshot.NextConversationId || !accountIds.Contains(conversation.AccountId)
            || !IsText(conversation.Title, MaxTitleLength) || FindModel(conversation.ModelId) is null || !IsPlausible(conversation.CreatedAt) || !IsPlausible(conversation.UpdatedAt)
            || conversation.Messages is not { Count: > 0 and <= MaxMessages })
            return false;

        return conversation.Messages.All(message => message is not null && message.Id >= 1 && message.Id < snapshot.NextMessageId && Enum.IsDefined(message.Role)
            && message.At >= conversation.CreatedAt && message.At <= AiSeedData.Now + ShowcaseClock.MaxElapsed && FindModel(message.ModelId) is not null && Enum.IsDefined(message.Feedback)
            && message.Text is not null && message.Attachments is { Count: <= MaxAttachments }
            && message.Attachments.All(file => file is not null && IsText(file.Name, MaxAttachmentNameLength) && file.SizeBytes is >= 0 and <= 100L * 1024 * 1024 && (file.ContentType?.Length ?? 0) <= 100)
            && (message.Role == AiRole.User
                ? IsText(message.Text, MaxMessageLength) && message.ReplyKey is null && !message.IsStopped && message.Feedback == AiFeedback.None
                    && message.Tokens == AiSeedData.TokensOf(message.Text) + (message.Attachments.Count * AttachmentTokens)
                : message.Text.Length == 0 && message.Attachments.Count == 0 && AiReplies.IsKnown(message.ReplyKey, message.Variant)
                    && message.StoppedLength >= 0 && message.StoppedLength <= FullReply(message).Length && message.Tokens == AiSeedData.TokensOf(FullReply(message))));
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

    /// <summary>Gets whether charts should use dark-theme colors.</summary>
    public bool IsDarkTheme => isDarkTheme;

    /// <summary>Gets whether the signed-in account is an active member of the workspace.</summary>
    public bool IsActiveMember => CurrentMember is { Status: AiMemberStatus.Active };

    /// <summary>Gets whether the signed-in account is an active workspace admin.</summary>
    public bool IsWorkspaceAdmin => CurrentMember is { Status: AiMemberStatus.Active, Role: AiMemberRole.Admin };

    /// <summary>Gets the members and invitations: invitations, then active, then deactivated members; admins first within each, then by name.</summary>
    public IReadOnlyList<AiMember> Members => members.OrderBy(member => member.Status).ThenByDescending(member => member.Role).ThenBy(member => member.Name, StringComparer.OrdinalIgnoreCase).ToList();

    /// <summary>Gets the model catalog.</summary>
    public IReadOnlyList<AiModel> Models { get; } = AiSeedData.Models();

    /// <summary>Gets the signed-in member's own prompts, the most recently changed first.</summary>
    public IReadOnlyList<AiPrompt> MyPrompts =>
        Identity.Current is { } account ? prompts.Where(prompt => prompt.OwnerId == account.Id).OrderByDescending(prompt => prompt.UpdatedAt).ToList() : Array.Empty<AiPrompt>();

    /// <summary>Gets the workspace policy (read it; change it with <see cref="SavePolicy" />).</summary>
    public AiPolicy Policy => policy;

    /// <summary>Gets the model a new conversation starts with: the member's preference when the workspace enables it, else the workspace default.</summary>
    public string PreferredModelId => Preferences.ModelId is { } preferred && policy.EnabledModelIds.Contains(preferred) ? preferred : policy.DefaultModelId;

    /// <summary>Gets the signed-in member's preferences (the defaults until saved).</summary>
    public AiPreferences Preferences => Identity.Current is { } account && preferences.TryGetValue(account.Id, out var saved) ? saved : new AiPreferences();

    /// <summary>Gets the shared workspace prompts, the most used first.</summary>
    public IReadOnlyList<AiPrompt> SharedPrompts => prompts.Where(prompt => prompt.IsShared).OrderByDescending(prompt => prompt.UseCount).ThenBy(prompt => prompt.Title, StringComparer.OrdinalIgnoreCase).ToList();

    /// <summary>Gets the site's slug, used in the storage key.</summary>
    public string SiteSlug => "ai-assistant";

    /// <summary>Gets the snapshot format version.</summary>
    public int SnapshotVersion => 1;

    #endregion

    #region Events

    /// <summary>Raised when anything in the state changes, including sign-in.</summary>
    public event Action? Changed;

    /// <summary>Raised when the site's theme changes, so charts can redraw.</summary>
    public event Action? ThemeChanged;

    #endregion

    #region Nested Types

    /// <summary>The AI assistant site's part of the tab snapshot.</summary>
    private sealed class AiSnapshot
    {
        #region Properties, Indexers

        public List<AiConversation>? Conversations { get; set; }

        public List<AiMember>? Members { get; set; }

        public int NextConversationId { get; set; }

        public int NextMessageId { get; set; }

        public int NextPromptId { get; set; }

        public AiPolicy? Policy { get; set; }

        public Dictionary<string, AiPreferences>? Preferences { get; set; }

        public List<AiPrompt>? Prompts { get; set; }

        public List<AiUsageEntry>? Usage { get; set; }

        #endregion
    }

    #endregion
}
