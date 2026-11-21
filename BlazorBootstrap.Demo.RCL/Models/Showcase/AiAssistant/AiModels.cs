namespace BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;

/// <summary>Who wrote a message in a conversation.</summary>
public enum AiRole
{
    /// <summary>The signed-in person.</summary>
    User,

    /// <summary>The simulated assistant.</summary>
    Assistant,
}

/// <summary>How the person rated an assistant reply.</summary>
public enum AiFeedback
{
    /// <summary>Not rated.</summary>
    None,

    /// <summary>Thumbs up.</summary>
    Helpful,

    /// <summary>Thumbs down.</summary>
    NotHelpful,
}

/// <summary>
/// A member's role in the workspace (AC-4's group roles), from the fewest rights to the most; each role has the rights of
/// the ones before it (<see cref="AiPermission" />; <c>AiState.RoleAllows</c> holds the matrix and the state checks it).
/// </summary>
public enum AiMemberRole
{
    /// <summary>Chats, keeps their own prompts, and follows their own usage.</summary>
    Member,

    /// <summary>Also writes, publishes, and deletes the workspace's shared prompts; no access to members or settings.</summary>
    PromptEditor,

    /// <summary>Also manages members (up to Admin), the workspace settings and model policy, and reads usage analytics and the audit log.</summary>
    Admin,

    /// <summary>Everything, and the only one who changes retention or transfers ownership. A workspace has exactly one.</summary>
    Owner,
}

/// <summary>What a workspace role may do; <c>AiState.RoleAllows</c> maps each role to its permissions, and the state checks them.</summary>
public enum AiPermission
{
    /// <summary>Chat, keep personal prompts, and see their own usage (every active member).</summary>
    UseAssistant,

    /// <summary>Create, edit, publish, unpublish, and delete shared workspace prompts (Prompt editor, Admin, Owner).</summary>
    ManageSharedPrompts,

    /// <summary>See the workspace's usage analytics (Admin, Owner).</summary>
    ViewAnalytics,

    /// <summary>Invite people, change roles up to Admin, remove and restore access, and resend invitations (Admin, Owner).</summary>
    ManageMembers,

    /// <summary>Change the workspace name, the models, the quota, attachments, and blocked terms (Admin, Owner).</summary>
    ManageSettings,

    /// <summary>Read the audit log (Admin, Owner).</summary>
    ViewAudit,

    /// <summary>Set the workspace's monthly spend cap, the default member cap, and members' own caps (Admin, Owner).</summary>
    ManageSpend,

    /// <summary>Set the sensitive data policy and work its review queue (Admin, Owner).</summary>
    ReviewSensitiveData,

    /// <summary>Change how long conversations are kept (Owner).</summary>
    ManageRetention,

    /// <summary>Hand the Owner role to another member (Owner).</summary>
    TransferOwnership,
}

/// <summary>A prompt's category, for the libraries' filters.</summary>
public enum AiPromptCategory
{
    /// <summary>Drafting, editing, and rewriting text.</summary>
    Writing,

    /// <summary>Code, queries, reviews, and debugging.</summary>
    Code,

    /// <summary>Data, research, and decisions.</summary>
    Analysis,

    /// <summary>Meetings, status, and planning with the team.</summary>
    Team,

    /// <summary>Customers and support replies.</summary>
    Support,

    /// <summary>Learning and explaining.</summary>
    Learning,
}

/// <summary>Where a member is.</summary>
public enum AiMemberStatus
{
    /// <summary>Invited by email; joins when they create an account with that address.</summary>
    Invited,

    /// <summary>Can use the workspace.</summary>
    Active,

    /// <summary>Access removed by an admin; their conversations are kept.</summary>
    Deactivated,
}

/// <summary>Outcome of an action on the AI Assistant site.</summary>
public enum AiResult
{
    /// <summary>Done.</summary>
    Ok,

    /// <summary>Sign in first, the account isn't an active member, it isn't yours, or it needs a workspace admin.</summary>
    NotAllowed,

    /// <summary>The conversation, message, prompt, member, or model doesn't exist.</summary>
    NotFound,

    /// <summary>A value is missing, too long, or out of range (or the tab is full).</summary>
    Invalid,

    /// <summary>The item's state doesn't allow it (for example, the model is turned off, or demoting the last admin).</summary>
    WrongState,

    /// <summary>Nothing changed.</summary>
    NoChange,

    /// <summary>It already exists (for example, an invitation for the same email).</summary>
    Duplicate,

    /// <summary>The member used this month's token quota.</summary>
    OverQuota,

    /// <summary>Nobody changes their own role or access (another admin does, or the Owner transfers ownership).</summary>
    OwnRole,

    /// <summary>The Owner's role and access can't be changed or removed; only the Owner hands the role on, by a transfer.</summary>
    OwnerProtected,

    /// <summary>The role is above what the signed-in person may grant (nobody grants Owner; it comes only through a transfer).</summary>
    NotGrantable,

    /// <summary>The workspace used its monthly spend cap: nobody can chat until an admin raises it.</summary>
    WorkspaceCapReached,

    /// <summary>The message holds data the sensitive data policy blocks (a card number, for example); it wasn't sent.</summary>
    SensitiveBlocked,

    /// <summary>A new cap isn't above what is already used this month.</summary>
    BelowUse,

    /// <summary>A member's cap can't be above the workspace's cap.</summary>
    AboveWorkspaceCap,
}

/// <summary>How much of a monthly spend cap is used.</summary>
public enum AiSpendLevel
{
    /// <summary>Under 80 %.</summary>
    Ok,

    /// <summary>80 % or more: a warning in chat, and one notification a month.</summary>
    Warning,

    /// <summary>100 % or more: chat is blocked for the scope until an admin raises the cap.</summary>
    Blocked,
}

/// <summary>A kind of data the sensitive data policy looks for in a message.</summary>
public enum AiDlpKind
{
    /// <summary>A payment card number: 13 to 19 digits (spaces or dashes allowed) that pass the Luhn check.</summary>
    PaymentCard,

    /// <summary>A US Social Security number like 123-45-6789 (impossible ranges left out).</summary>
    NationalId,

    /// <summary>An international bank account number (IBAN) whose check digits are right.</summary>
    Iban,

    /// <summary>A secret API key like sk-… (20 or more letters, digits, dashes, or underscores after it).</summary>
    ApiKey,
}

/// <summary>What the policy does with a match.</summary>
public enum AiDlpAction
{
    /// <summary>The message is sent with the match replaced, like "[card number removed]".</summary>
    Redact,

    /// <summary>The message isn't sent.</summary>
    Block,
}

/// <summary>Where a sensitive data event is in the review queue.</summary>
public enum AiDlpStatus
{
    /// <summary>Waiting for an admin.</summary>
    New,

    /// <summary>Reviewed: nothing to change.</summary>
    Dismissed,

    /// <summary>Reviewed, and the policy was adjusted from it.</summary>
    PolicyChanged,
}

/// <summary>One pattern of the sensitive data policy: on or off, and what happens to a match.</summary>
public sealed class AiDlpRule
{
    #region Properties, Indexers

    /// <summary>Gets or sets what happens to a match.</summary>
    public AiDlpAction Action { get; set; }

    /// <summary>Gets or sets whether the pattern is checked.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Gets or sets the kind of data.</summary>
    public AiDlpKind Kind { get; set; }

    #endregion
}

/// <summary>
/// A message that matched the sensitive data policy: who, when, which kind of data, what was done, and the match masked
/// (like "•••• •••• •••• 4242"); the raw value is never kept.
/// </summary>
public sealed class AiDlpEvent
{
    #region Properties, Indexers

    /// <summary>Gets what was done: redacted (sent without the data) or blocked (not sent).</summary>
    public AiDlpAction ActionTaken { get; init; }

    /// <summary>Gets when the message was sent (or refused).</summary>
    public DateTime At { get; init; }

    /// <summary>Gets the conversation the redacted message went to, or <see langword="null" /> when it was blocked before a new chat began.</summary>
    public int? ConversationId { get; init; }

    /// <summary>Gets the conversation's title when the event happened (it never holds the data: titles come from the redacted text).</summary>
    public string ConversationTitle { get; init; } = string.Empty;

    /// <summary>Gets the event number.</summary>
    public int Id { get; init; }

    /// <summary>Gets the kind of data matched.</summary>
    public AiDlpKind Kind { get; init; }

    /// <summary>Gets the match masked, like "•••• •••• •••• 4242".</summary>
    public string Masked { get; init; } = string.Empty;

    /// <summary>Gets the member who sent it.</summary>
    public int MemberId { get; init; }

    /// <summary>Gets or sets the reviewer's note (up to 200 characters).</summary>
    public string Note { get; set; } = string.Empty;

    /// <summary>Gets or sets when it was reviewed.</summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>Gets or sets who reviewed it.</summary>
    public string? ReviewedBy { get; set; }

    /// <summary>Gets or sets where it is in the review queue.</summary>
    public AiDlpStatus Status { get; set; }

    #endregion
}

/// <summary>What the sensitive data policy did with a message being sent: the kinds it blocked and the kinds it redacted.</summary>
public sealed record AiDlpCheck(IReadOnlyList<AiDlpKind> Blocked, IReadOnlyList<AiDlpKind> Redacted);

/// <summary>Tokens the Spend page's demo control added to a member's month (usage the demo pushes up quickly).</summary>
public sealed class AiDemoUsage
{
    #region Properties, Indexers

    /// <summary>Gets the day (a date without a time).</summary>
    public DateTime Date { get; init; }

    /// <summary>Gets the member.</summary>
    public int MemberId { get; init; }

    /// <summary>Gets the model it counts for (the workspace default when it was added).</summary>
    public string ModelId { get; init; } = string.Empty;

    /// <summary>Gets or sets the tokens.</summary>
    public int Tokens { get; set; }

    #endregion
}

/// <summary>How much of a monthly spend cap is used: tokens used this month, the cap, and the level.</summary>
public sealed record AiSpendStatus(long Used, long Cap, AiSpendLevel Level)
{
    /// <summary>Gets the share of the cap used, in percent.</summary>
    public double Percent => Cap <= 0 ? 0 : Used * 100d / Cap;

    /// <summary>Gets the tokens left this month (0 when blocked).</summary>
    public long Left => Math.Max(0, Cap - Used);
}

/// <summary>
/// The outcome of a bulk action: the overall result, how many records changed, how many were already that way (or changed
/// meanwhile), and how many the role rules kept as they were (yourself, or the Owner).
/// </summary>
public sealed record AiBulkOutcome(AiResult Result, int Changed, int Unchanged, int Protected);

/// <summary>A simulated model the workspace can offer. Nothing is called; it sets the streaming speed and the reply style.</summary>
public sealed record AiModel(string Id, string Name, string Description, int TokensPerSecond, int ContextTokens, bool IsCodeTuned);

/// <summary>A file attached to a message. Only its name, size, and type are kept; its content is never read.</summary>
public sealed record AiAttachment(string Name, long SizeBytes, string ContentType);

/// <summary>
/// A message in a conversation. A user message has plain <see cref="Text" />; an assistant message has no text of its own:
/// its Markdown comes from the fixed reply templates (<see cref="ReplyKey" />, <see cref="Variant" />), so nothing stored is
/// rendered as markup.
/// </summary>
public sealed class AiMessage
{
    #region Properties, Indexers

    /// <summary>Gets the files attached to a user message.</summary>
    public List<AiAttachment> Attachments { get; init; } = new();

    /// <summary>Gets when it was sent (an assistant reply streams from this time).</summary>
    public DateTime At { get; init; }

    /// <summary>Gets or sets the person's rating of an assistant reply.</summary>
    public AiFeedback Feedback { get; set; }

    /// <summary>Gets the message number.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets whether the person stopped the reply before it finished.</summary>
    public bool IsStopped { get; set; }

    /// <summary>Gets the model that wrote an assistant reply.</summary>
    public string ModelId { get; init; } = string.Empty;

    /// <summary>Gets the reply template of an assistant message, or <see langword="null" /> for a user message.</summary>
    public string? ReplyKey { get; init; }

    /// <summary>Gets who wrote it.</summary>
    public AiRole Role { get; init; }

    /// <summary>Gets or sets how many characters of a stopped reply were shown.</summary>
    public int StoppedLength { get; set; }

    /// <summary>Gets the text of a user message (plain); empty for an assistant message.</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>Gets the tokens the message counts for usage.</summary>
    public int Tokens { get; init; }

    /// <summary>Gets the variant of the reply template (Regenerate picks another).</summary>
    public int Variant { get; init; }

    #endregion
}

/// <summary>A conversation of one member with the assistant.</summary>
public sealed class AiConversation
{
    #region Properties, Indexers

    /// <summary>Gets the member's demo account.</summary>
    public string AccountId { get; init; } = string.Empty;

    /// <summary>Gets when it was started.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets the conversation number.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets whether it is pinned to the top of the list.</summary>
    public bool IsPinned { get; set; }

    /// <summary>Gets the messages, oldest first.</summary>
    public List<AiMessage> Messages { get; init; } = new();

    /// <summary>Gets or sets the model new messages use.</summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>Gets or sets the title (plain text).</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets when it last changed.</summary>
    public DateTime UpdatedAt { get; set; }

    #endregion
}

/// <summary>
/// A saved prompt: a member's own, or a shared one that prompt editors, admins, and the Owner manage. A shared prompt is a
/// draft until it is published; members see only published ones. Its text may hold variables like <c>{{audience}}</c>, each
/// declared in <see cref="Variables" />.
/// </summary>
public sealed class AiPrompt
{
    #region Properties, Indexers

    /// <summary>Gets or sets who wrote it (their name).</summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>Gets or sets the author's demo account, or <see langword="null" /> for a colleague without a sign-in (they get no notifications).</summary>
    public string? AuthorId { get; set; }

    /// <summary>Gets or sets the category.</summary>
    public AiPromptCategory Category { get; set; }

    /// <summary>Gets the prompt number.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets whether a shared prompt is published to every member (always <see langword="false" /> for a personal prompt).</summary>
    public bool IsPublished { get; set; }

    /// <summary>Gets whether it is a shared workspace prompt (no owner).</summary>
    public bool IsShared => OwnerId is null;

    /// <summary>Gets the owner's demo account, or <see langword="null" /> for a shared workspace prompt.</summary>
    public string? OwnerId { get; init; }

    /// <summary>Gets or sets the tags (lower-case words).</summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>Gets or sets the prompt text (plain).</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Gets or sets the title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets when it was last changed.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Gets or sets how often it was used in a conversation.</summary>
    public int UseCount { get; set; }

    /// <summary>Gets or sets the variables its text uses, like <c>audience</c> for <c>{{audience}}</c> (lower case, each once).</summary>
    public List<string> Variables { get; set; } = new();

    #endregion
}

/// <summary>A workspace member (or an invitation).</summary>
public sealed class AiMember
{
    #region Properties, Indexers

    /// <summary>Gets or sets the demo account, or <see langword="null" /> while only invited.</summary>
    public string? AccountId { get; set; }

    /// <summary>Gets or sets the email address (lower-case; it follows the member's profile).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the member's own monthly cap in tokens, or <see langword="null" /> for the workspace's default member cap.</summary>
    public int? MonthlyCap { get; set; }

    /// <summary>Gets the member number, unique within the workspace (the email can change, the number doesn't).</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets when they joined, or when the invitation was last sent.</summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the workspace role.</summary>
    public AiMemberRole Role { get; set; }

    /// <summary>Gets or sets the status.</summary>
    public AiMemberStatus Status { get; set; }

    #endregion
}

/// <summary>The workspace's model and policy settings (simulated: they shape this demo only).</summary>
public sealed class AiPolicy
{
    #region Properties, Indexers

    /// <summary>Gets or sets whether members can attach files.</summary>
    public bool AllowAttachments { get; set; } = true;

    /// <summary>Gets or sets words or phrases the assistant refuses to discuss.</summary>
    public List<string> BlockedTerms { get; set; } = new();

    /// <summary>Gets or sets the model new conversations start with.</summary>
    public string DefaultModelId { get; set; } = string.Empty;

    /// <summary>Gets or sets the sensitive data policy: one rule per kind of data.</summary>
    public List<AiDlpRule> DlpRules { get; set; } = new();

    /// <summary>Gets or sets the models members can choose.</summary>
    public List<string> EnabledModelIds { get; set; } = new();

    /// <summary>Gets or sets the largest attachment, in megabytes.</summary>
    public int MaxAttachmentMb { get; set; } = 10;

    /// <summary>Gets or sets the default monthly spend cap of a member, in tokens (a member's own cap replaces it).</summary>
    public int MonthlyTokenQuota { get; set; } = 200_000;

    /// <summary>Gets or sets after how many days without changes a conversation is deleted; 0 keeps them (the Owner changes it).</summary>
    public int RetentionDays { get; set; }

    /// <summary>Gets or sets the workspace's name, shown in the admin bar and on the members' pages.</summary>
    public string WorkspaceName { get; set; } = string.Empty;

    /// <summary>Gets or sets the workspace's monthly spend cap in tokens, every member together.</summary>
    public int WorkspaceMonthlyCap { get; set; }

    #endregion
}

/// <summary>The workspace settings a save asks for (<c>AiState.SavePolicy</c>).</summary>
public sealed record AiPolicyInput(
    string WorkspaceName,
    IReadOnlyList<string> EnabledModelIds,
    string DefaultModelId,
    int MonthlyTokenQuota,
    bool AllowAttachments,
    int MaxAttachmentMb,
    int RetentionDays,
    IReadOnlyList<string> BlockedTerms);

/// <summary>A prompt as a save asks for it (<c>AiState.SavePrompt</c>); <see cref="Publish" /> is for shared prompts only.</summary>
public sealed record AiPromptInput(string Title, string Text, AiPromptCategory Category, IReadOnlyList<string> Variables, IReadOnlyList<string> Tags, bool Publish);

/// <summary>A member's own preferences.</summary>
public sealed class AiPreferences
{
    #region Properties, Indexers

    /// <summary>Gets or sets whether Enter sends (Shift+Enter adds a line); otherwise Ctrl+Enter sends.</summary>
    public bool EnterToSend { get; set; } = true;

    /// <summary>Gets or sets the model the member prefers for new conversations, or <see langword="null" /> for the workspace default.</summary>
    public string? ModelId { get; set; }

    #endregion
}

/// <summary>
/// One line of the usage ledger: what an account used with one model on one day. It is kept when conversations are
/// deleted or replies regenerated, so the monthly quota counts everything used.
/// </summary>
public sealed class AiUsageEntry
{
    /// <summary>Gets the account that used the tokens.</summary>
    public string AccountId { get; init; } = string.Empty;

    /// <summary>Gets the day (a date without a time).</summary>
    public DateTime Date { get; init; }

    /// <summary>Gets or sets how many messages the member sent that day with the model.</summary>
    public int Messages { get; set; }

    /// <summary>Gets the model.</summary>
    public string ModelId { get; init; } = string.Empty;

    /// <summary>Gets or sets the tokens of the messages and replies.</summary>
    public int Tokens { get; set; }
}

/// <summary>Usage of one member with one model on one day.</summary>
public sealed record AiUsagePoint(DateTime Date, string MemberKey, string ModelId, int Messages, int Tokens);
