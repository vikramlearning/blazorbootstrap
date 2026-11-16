using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

/// <summary>
/// Fixed, fictitious content for "Parley", the AI assistant workspace of the made-up Halcyon Studio: the simulated models,
/// members, conversations, saved prompts, and the workspace policy. Usage history comes from a deterministic generator, so
/// every fresh tab starts the same way.
/// </summary>
public static class AiSeedData
{
    #region Fields and Constants

    /// <summary>The demo admin's name.</summary>
    public const string AdminName = "Alex Morgan";

    /// <summary>The identity role of the demo admin (workspace roles are kept on <see cref="AiMember" />).</summary>
    public const string AdminRole = "admin";

    /// <summary>The identity role of members (and of new accounts).</summary>
    public const string MemberRole = "member";

    /// <summary>The demo member's name.</summary>
    public const string MemberName = "Sam Rivera";

    /// <summary>The fictitious product name.</summary>
    public const string ProductName = "Parley";

    /// <summary>The ID of the demo admin account.</summary>
    public const string SeededAdminId = "alex";

    /// <summary>The ID of the demo member account.</summary>
    public const string SeededMemberId = "sam";

    /// <summary>Both identity roles: every signed-in account.</summary>
    public const string SignedInRoles = MemberRole + "," + AdminRole;

    /// <summary>The fictitious company whose workspace this is.</summary>
    public const string WorkspaceName = "Halcyon Studio";

    /// <summary>When a fresh tab's clock starts; the clock moves on from there (D-029).</summary>
    public static readonly DateTime Now = new(2026, 11, 24, 9, 30, 0);

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>The conversations in a fresh tab: four of the demo member's (one pinned) and one of the admin's.</summary>
    public static List<AiConversation> Conversations()
    {
        var messageId = 1;
        var list = new List<AiConversation>();

        AiConversation Conversation(int id, string accountId, string title, string modelId, double daysAgo, bool pinned, params (string Text, string Key, int Variant, AiFeedback Feedback)[] turns)
        {
            var start = Now.AddDays(-daysAgo);
            var conversation = new AiConversation { Id = id, AccountId = accountId, Title = title, ModelId = modelId, CreatedAt = start, IsPinned = pinned };
            var at = start;

            foreach (var turn in turns)
            {
                conversation.Messages.Add(new AiMessage { Id = messageId++, Role = AiRole.User, Text = turn.Text, At = at, ModelId = modelId, Tokens = TokensOf(turn.Text) });
                at = at.AddSeconds(4);
                var reply = AiReplies.Render(turn.Key, turn.Variant, FindModel(modelId));
                conversation.Messages.Add(new AiMessage { Id = messageId++, Role = AiRole.Assistant, ReplyKey = turn.Key, Variant = turn.Variant, At = at, ModelId = modelId, Tokens = TokensOf(reply), Feedback = turn.Feedback });
                at = at.AddMinutes(3);
            }

            conversation.UpdatedAt = at.AddMinutes(-3);
            list.Add(conversation);

            return conversation;
        }

        Conversation(1, SeededMemberId, "Q4 launch plan", "balanced", 6, true,
            ("Can you help me plan the launch of our new onboarding flow? We have four weeks.", "plan", 0, AiFeedback.Helpful),
            ("Summarize that plan in a short paragraph for my manager.", "summary", 1, AiFeedback.None));
        Conversation(2, SeededMemberId, "SQL for monthly churn", "code", 3, false,
            ("Write a SQL query for monthly churn from our orders table.", "sql", 0, AiFeedback.Helpful));
        Conversation(3, SeededMemberId, "Fixing a null reference in C#", "code", 1.2, false,
            ("I keep getting a NullReferenceException in C# when an order is missing. How should I handle it?", "code-csharp", 0, AiFeedback.None),
            ("Is there a shorter way?", "code-csharp", 1, AiFeedback.None));
        Conversation(4, SeededMemberId, "Email to the design team", "swift", 0.3, false,
            ("Draft an email to the design team: the launch moved to December 2.", "email", 0, AiFeedback.None));
        Conversation(5, SeededAdminId, "Explain race conditions", "deep", 2, false,
            ("Explain what a race condition is, for our new hires.", "explain", 0, AiFeedback.Helpful));

        return list;
    }

    /// <summary>The demo accounts: a member, and the workspace admin.</summary>
    public static IReadOnlyList<ShowcaseDemoAccount> DemoAccounts() => new List<ShowcaseDemoAccount>
    {
        new(SeededMemberId, MemberRole, MemberName, "sam.rivera@example.com", "chat-demo-26", "SR"),
        new(SeededAdminId, AdminRole, AdminName, "alex.morgan@example.com", "chat-admin-26", "AM"),
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

    /// <summary>The workspace members in a fresh tab: the two demo accounts, three colleagues, one deactivated, and one invitation.</summary>
    public static List<AiMember> Members() => new()
    {
        new() { AccountId = SeededMemberId, Name = MemberName, Email = "sam.rivera@example.com", Role = AiMemberRole.Member, Status = AiMemberStatus.Active, JoinedAt = Now.AddDays(-120) },
        new() { AccountId = SeededAdminId, Name = AdminName, Email = "alex.morgan@example.com", Role = AiMemberRole.Admin, Status = AiMemberStatus.Active, JoinedAt = Now.AddDays(-200) },
        new() { Name = "Priya Shah", Email = "priya.shah@example.com", Role = AiMemberRole.Member, Status = AiMemberStatus.Active, JoinedAt = Now.AddDays(-150) },
        new() { Name = "Leo Martins", Email = "leo.martins@example.com", Role = AiMemberRole.Member, Status = AiMemberStatus.Active, JoinedAt = Now.AddDays(-90) },
        new() { Name = "Mei Chen", Email = "mei.chen@example.com", Role = AiMemberRole.Admin, Status = AiMemberStatus.Active, JoinedAt = Now.AddDays(-180) },
        new() { Name = "Omar Haddad", Email = "omar.haddad@example.com", Role = AiMemberRole.Member, Status = AiMemberStatus.Deactivated, JoinedAt = Now.AddDays(-160) },
        new() { Name = "Nora Kim", Email = "nora.kim@example.com", Role = AiMemberRole.Member, Status = AiMemberStatus.Invited, JoinedAt = Now.AddDays(-2) },
    };

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
        EnabledModelIds = Models().Select(model => model.Id).ToList(),
        DefaultModelId = "balanced",
        MonthlyTokenQuota = 200_000,
        AllowAttachments = true,
        MaxAttachmentMb = 10,
        RetentionDays = 0,
        BlockedTerms = new() { "password dump", "card numbers" },
    };

    /// <summary>Saved prompts in a fresh tab: two of the demo member's, and four shared ones the admin wrote.</summary>
    public static List<AiPrompt> Prompts() => new()
    {
        new() { Id = 1, OwnerId = SeededMemberId, Author = MemberName, Title = "Tighten my writing", Text = "Rewrite the text below so it's shorter and clearer. Keep my tone, and list what you changed.\n\n[paste text here]", Tags = { "writing" }, UpdatedAt = Now.AddDays(-20), UseCount = 7 },
        new() { Id = 2, OwnerId = SeededMemberId, Author = MemberName, Title = "Explain this error", Text = "Explain what this error means and the three most likely causes, in plain words:\n\n[paste error here]", Tags = { "code", "debugging" }, UpdatedAt = Now.AddDays(-9), UseCount = 3 },
        new() { Id = 3, Author = AdminName, Title = "Weekly status update", Text = "Turn these notes into a weekly status update with three sections: done, next, and blockers. Keep it under 150 words.\n\n[paste notes here]", Tags = { "writing", "team" }, UpdatedAt = Now.AddDays(-40), UseCount = 31 },
        new() { Id = 4, Author = AdminName, Title = "Explain it like I'm new", Text = "Explain the topic below to someone in their first week on the team. Use one everyday example and no jargon.\n\nTopic: [topic]", Tags = { "learning" }, UpdatedAt = Now.AddDays(-35), UseCount = 18 },
        new() { Id = 5, Author = AdminName, Title = "Code review checklist", Text = "Review the code below for correctness, readability, and tests. List problems first, then suggestions.\n\n[paste code here]", Tags = { "code" }, UpdatedAt = Now.AddDays(-28), UseCount = 22 },
        new() { Id = 6, Author = AdminName, Title = "Meeting summary", Text = "Summarize this meeting transcript: decisions, owners, and dates. Leave out small talk.\n\n[paste transcript here]", Tags = { "team", "summary" }, UpdatedAt = Now.AddDays(-15), UseCount = 26 },
    };

    /// <summary>The roles of the site (identity roles; a member's workspace role is on <see cref="AiMember" />).</summary>
    public static IReadOnlyList<ShowcaseRole> Roles() => new List<ShowcaseRole>
    {
        new(MemberRole, "Member", "/showcase/ai-assistant/chat", "/showcase/ai-assistant/profile", "Chats with the assistant, saves prompts, and follows their usage."),
        new(AdminRole, "Workspace admin", "/showcase/ai-assistant/admin", "/showcase/ai-assistant/profile", "Also manages members and roles, shared prompts, usage, and the model and policy settings."),
    };

    /// <summary>Returns the tokens a text counts for (about four characters each).</summary>
    public static int TokensOf(string text) => Math.Max(1, (int)Math.Ceiling(text.Length / 4.0));

    /// <summary>
    /// The usage history before the seed time for the seeded members (not stored; generated the same way every time): the
    /// last 30 days, by model. The deactivated member stopped 12 days ago.
    /// </summary>
    public static IEnumerable<AiUsagePoint> UsageHistory(IReadOnlyList<AiMember> members)
    {
        // A seeded member is found by account (a demo account can change its email) or by email (colleagues).
        var seeded = Members();
        var models = Models();

        for (var m = 0; m < members.Count; m++)
        {
            var member = members[m];
            var memberIndex = seeded.FindIndex(item => item.AccountId is not null ? item.AccountId == member.AccountId : item.Email == member.Email);

            if (memberIndex < 0 || seeded[memberIndex].Status == AiMemberStatus.Invited)
                continue;

            for (var day = 1; day <= 30; day++)
            {
                var date = Now.Date.AddDays(-day);

                if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday || (seeded[memberIndex].Status == AiMemberStatus.Deactivated && day < 12))
                    continue;

                var messages = (int)(Hash(day, memberIndex, 1) * 18) + 2;
                var model = models[(int)(Hash(day, memberIndex, 2) * models.Count) % models.Count];
                var tokens = messages * (300 + (int)(Hash(day, memberIndex, 3) * 600));

                yield return new AiUsagePoint(date, member.Email, model.Id, messages, tokens);
            }
        }
    }

    #endregion

    #endregion
}
