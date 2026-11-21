using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

/// <summary>
/// Fixed, fictitious content for "Parley", the AI assistant workspace of the made-up Halcyon Studio: the simulated models,
/// the demo accounts (one per workspace role), the workspace policy, and the routes the state links to. The volume around
/// them (members, conversations, prompts, the audit log, and notifications) comes from deterministic generators
/// (<see cref="Build" />), so every fresh tab starts the same way.
/// </summary>
public static partial class AiSeedData
{
    #region Fields and Constants

    /// <summary>The demo admin's name.</summary>
    public const string AdminName = "Mei Chen";

    /// <summary>The identity role of the demo admin (workspace roles are kept on <see cref="AiMember" />).</summary>
    public const string AdminRole = "admin";

    /// <summary>The base route of the workspace admin back office.</summary>
    public const string AdminBase = SiteBase + "/admin";

    /// <summary>The demo prompt editor's name.</summary>
    public const string EditorName = "Priya Shah";

    /// <summary>The identity role of the demo prompt editor.</summary>
    public const string EditorRole = "editor";

    /// <summary>The identity role of members (and of new accounts).</summary>
    public const string MemberRole = "member";

    /// <summary>The demo member's name.</summary>
    public const string MemberName = "Sam Rivera";

    /// <summary>The demo owner's name.</summary>
    public const string OwnerName = "Alex Morgan";

    /// <summary>The identity role of the demo owner.</summary>
    public const string OwnerRole = "owner";

    /// <summary>The fictitious product name.</summary>
    public const string ProductName = "Parley";

    /// <summary>The ID of the demo admin account.</summary>
    public const string SeededAdminId = "mei";

    /// <summary>The ID of the demo prompt editor account.</summary>
    public const string SeededEditorId = "priya";

    /// <summary>The ID of the demo member account.</summary>
    public const string SeededMemberId = "sam";

    /// <summary>The ID of the demo owner account.</summary>
    public const string SeededOwnerId = "alex";

    /// <summary>Every identity role: every signed-in account (what they may do comes from their workspace membership).</summary>
    public const string SignedInRoles = OwnerRole + "," + AdminRole + "," + EditorRole + "," + MemberRole;

    /// <summary>The base route of the site.</summary>
    public const string SiteBase = DemoRouteConstants.Showcase_URL_AiAssistant;

    /// <summary>The audit log's name for what the workspace does by itself (an invitation taken up, retention).</summary>
    public const string SystemActor = "Parley (automatic)";

    /// <summary>The fictitious company whose workspace this is (the Owner and admins can rename it).</summary>
    public const string WorkspaceName = "Halcyon Studio";

    /// <summary>When a fresh tab's clock starts; the clock moves on from there (D-029).</summary>
    public static readonly DateTime Now = new(2026, 11, 24, 9, 30, 0);

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the route of a conversation.</summary>
    public static string ConversationPath(int conversationId) => Invariant($"{SiteBase}/chat/{conversationId}");

    /// <summary>The demo accounts, one per workspace role, in the order the sign-in page lists them: Owner, Admin, Prompt editor, Member.</summary>
    public static IReadOnlyList<ShowcaseDemoAccount> DemoAccounts() => new List<ShowcaseDemoAccount>
    {
        new(SeededOwnerId, OwnerRole, OwnerName, "alex.morgan@example.com", "chat-admin-26", "AM"),
        new(SeededAdminId, AdminRole, AdminName, "mei.chen@example.com", "chat-ops-26", "MC"),
        new(SeededEditorId, EditorRole, EditorName, "priya.shah@example.com", "chat-editor-26", "PS"),
        new(SeededMemberId, MemberRole, MemberName, "sam.rivera@example.com", "chat-demo-26", "SR"),
    };

    /// <summary>Finds a model of the catalog by ID, or returns <see langword="null" />.</summary>
    public static AiModel? FindModel(string? modelId) => Models().FirstOrDefault(model => model.Id == modelId);

    /// <summary>Returns a stable pseudo-random number from 0 to 1 for the given inputs (no shared random state).</summary>
    public static double Hash(int a, int b, int c)
    {
        unchecked
        {
            var h = (uint)((a * 73_856_093) ^ (b * 19_349_663) ^ (c * 83_492_791));
            h ^= h >> 13;
            h *= 0x5bd1e995;
            h ^= h >> 15;

            return (h % 10_000) / 10_000.0;
        }
    }

    /// <summary>Returns the admin route of the members list, filtered to one person (by email) when given.</summary>
    public static string MembersPath(string? email = null) =>
        email is null ? $"{AdminBase}/members" : $"{AdminBase}/members?search={Uri.EscapeDataString(email)}";

    /// <summary>The simulated models: they differ in streaming speed and in the closing line of a reply; nothing is called.</summary>
    public static IReadOnlyList<AiModel> Models() => new List<AiModel>
    {
        new("swift", "Parley Swift", "Fast answers for everyday writing and quick questions.", 60, 16_000, false),
        new("balanced", "Parley Balanced", "A good default: careful answers at a steady pace.", 35, 64_000, false),
        new("deep", "Parley Deep", "Slower, more thorough answers for analysis and long documents.", 18, 200_000, false),
        new("code", "Parley Code", "Tuned for code, queries, and technical explanations.", 40, 128_000, true),
    };

    /// <summary>The workspace policy in a fresh tab: every model on, Balanced by default, two blocked terms.</summary>
    public static AiPolicy Policy() => new()
    {
        WorkspaceName = WorkspaceName,
        EnabledModelIds = Models().Select(model => model.Id).ToList(),
        DefaultModelId = "balanced",
        MonthlyTokenQuota = 200_000,
        AllowAttachments = true,
        MaxAttachmentMb = 10,
        RetentionDays = 0,
        BlockedTerms = new() { "password dump", "card numbers" },
    };

    /// <summary>Returns the members' route of a prompt: the editor of one's own, or the read-only page of a shared one.</summary>
    public static string PromptPath(int promptId) => Invariant($"{SiteBase}/prompts/{promptId}/edit");

    /// <summary>The identity roles of the site, one per demo account (a member's workspace role is on <see cref="AiMember" /> and can change).</summary>
    public static IReadOnlyList<ShowcaseRole> Roles() => new List<ShowcaseRole>
    {
        new(OwnerRole, "Owner", AdminBase, SiteBase + "/profile", "Everything an admin does, plus retention and handing ownership on. The workspace has one Owner."),
        new(AdminRole, "Admin", AdminBase, SiteBase + "/profile", "Manages members and roles (up to Admin), the model policy and settings, shared prompts, usage analytics, and the audit log."),
        new(EditorRole, "Prompt editor", AdminBase + "/prompts", SiteBase + "/profile", "Writes, publishes, and deletes the workspace's shared prompts, and chats. No access to members or settings."),
        new(MemberRole, "Member", SiteBase + "/chat", SiteBase + "/profile", "Chats with the assistant, keeps their own prompts, and follows their usage."),
    };

    /// <summary>Returns the back office route of a shared prompt's editor.</summary>
    public static string SharedPromptPath(int promptId) => Invariant($"{AdminBase}/prompts/{promptId}/edit");

    /// <summary>Returns the tokens a text counts for (about four characters each).</summary>
    public static int TokensOf(string text) => Math.Max(1, (int)Math.Ceiling(text.Length / 4.0));

    #endregion

    #region Private Methods

    private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);

    #endregion

    #endregion
}
