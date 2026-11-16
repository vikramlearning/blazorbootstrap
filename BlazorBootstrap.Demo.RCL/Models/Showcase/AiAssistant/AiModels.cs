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

/// <summary>A member's role in the workspace.</summary>
public enum AiMemberRole
{
    /// <summary>Uses the assistant.</summary>
    Member,

    /// <summary>Also manages members, shared prompts, models, and policies.</summary>
    Admin,
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
}

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

/// <summary>A saved prompt: a member's own, or a shared one the workspace admins manage.</summary>
public sealed class AiPrompt
{
    #region Properties, Indexers

    /// <summary>Gets or sets who wrote it (their name).</summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>Gets the prompt number.</summary>
    public int Id { get; init; }

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

    /// <summary>Gets or sets when they joined or were invited.</summary>
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

    /// <summary>Gets or sets the models members can choose.</summary>
    public List<string> EnabledModelIds { get; set; } = new();

    /// <summary>Gets or sets the largest attachment, in megabytes.</summary>
    public int MaxAttachmentMb { get; set; } = 10;

    /// <summary>Gets or sets each member's monthly token quota.</summary>
    public int MonthlyTokenQuota { get; set; } = 200_000;

    /// <summary>Gets or sets after how many days without changes a conversation is deleted; 0 keeps them.</summary>
    public int RetentionDays { get; set; }

    #endregion
}

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
