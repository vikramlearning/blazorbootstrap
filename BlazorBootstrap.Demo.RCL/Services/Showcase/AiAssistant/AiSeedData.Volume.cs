using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

/// <summary>
/// The fresh tab's data at realistic volume (rule WR-001), from deterministic generators: about ninety members and
/// invitations, a couple of hundred conversations of the four demo accounts (short exchanges with the prewritten replies,
/// a few of them months old so retention has something to delete), a prompt library of about a hundred and fifty personal
/// and shared prompts, the usage ledger those conversations make, the spend caps (the demo member near 80 % of their own
/// cap, a colleague already blocked, the workspace at about 70 %), past sensitive data events (one waiting for review), the
/// last <see cref="AuditDays" /> days of the audit log, and each demo account's notifications.
/// </summary>
public static partial class AiSeedData
{
    #region Fields and Constants

    /// <summary>How many days of the audit log a fresh tab holds.</summary>
    private const int AuditDays = 60;

    /// <summary>How many members and invitations a fresh tab holds.</summary>
    private const int SeededMembers = 92;

    private static readonly string[] firstNames =
    {
        "Ava", "Ben", "Chloe", "Daniel", "Elena", "Felix", "Grace", "Hugo", "Isla", "Jamal", "Kira", "Liam", "Maya", "Noah", "Olivia", "Pablo", "Quinn", "Rosa",
        "Samir", "Tara", "Uma", "Victor", "Wen", "Xavier", "Yara", "Zane", "Amara", "Bruno", "Carmen", "Dev", "Esme", "Farah", "Gabe", "Hana", "Ivan", "Jade",
        "Kofi", "Lena", "Marco", "Nadia",
    };

    private static readonly string[] lastNames =
    {
        "Abbott", "Banerjee", "Costa", "Dubois", "Eriksen", "Fischer", "Garcia", "Hughes", "Ibrahim", "Jensen", "Kowalski", "Lopez", "Mensah", "Novak", "Okafor",
        "Petrov", "Quinlan", "Rossi", "Silva", "Tanaka", "Ueda", "Varga", "Walsh", "Xu", "Yilmaz", "Zimmer", "Andersen", "Bianchi", "Chowdhury", "Delgado", "Evans",
        "Fontaine", "Grant", "Haddock", "Iqbal", "Janssen", "Keller", "Lindqvist", "Moreau", "Nakamura",
    };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Builds a fresh tab's data: members, conversations, prompts, the usage ledger, the audit log, notifications, and the
    /// policy. New objects every call, so a tab can change them without touching another copy.
    /// </summary>
    public static AiSeedSet Build()
    {
        var members = Members();
        var conversations = Conversations();
        var prompts = Prompts(members);
        var ledger = Ledger(conversations);
        var policy = Policy();
        var dlpEvents = DlpEvents(members, conversations);
        var caps = SetCaps(members, ledger, policy);

        return new AiSeedSet(members, conversations, prompts, ledger, Audit(members, prompts, dlpEvents, caps, policy), Notifications(prompts, dlpEvents, members), policy, dlpEvents);
    }

    /// <summary>
    /// The usage history before the seed time of the seeded members (not stored; generated the same way every time), by
    /// member number: the last 30 working days, by model. Deactivated members stopped 12 days ago; invitations have none.
    /// </summary>
    public static IReadOnlyDictionary<int, IReadOnlyList<AiUsagePoint>> UsageHistory()
    {
        var models = Models();
        var history = new Dictionary<int, IReadOnlyList<AiUsagePoint>>();

        foreach (var member in Members().Where(member => member.Status != AiMemberStatus.Invited))
        {
            var points = new List<AiUsagePoint>();

            for (var day = 1; day <= 30; day++)
            {
                var date = Now.Date.AddDays(-day);

                if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday || (member.Status == AiMemberStatus.Deactivated && day < 12) || Hash(day, member.Id, 4) < 0.15)
                    continue;

                var messages = (int)(Hash(day, member.Id, 1) * 18) + 2;
                var model = models[(int)(Hash(day, member.Id, 2) * models.Count) % models.Count];
                var tokens = messages * (300 + (int)(Hash(day, member.Id, 3) * 600));
                points.Add(new AiUsagePoint(date, member.Email, model.Id, messages, tokens));
            }

            history[member.Id] = points;
        }

        return history;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// The seeded audit log: the last <see cref="AuditDays" /> days of joins, invitations, role changes, access removed,
    /// shared prompts published and edited, and changes to the model policy and settings. Never chat content.
    /// </summary>
    private static List<ShowcaseAuditEntry> Audit(List<AiMember> members, List<AiPrompt> prompts, List<AiDlpEvent> dlpEvents, List<(AiMember Member, int Cap, double DaysAgo)> caps,
        AiPolicy policy)
    {
        var since = Now.AddDays(-AuditDays);
        var entries = new List<(DateTime At, string Actor, string Action, string Target, string? Url, string? Details)>();
        string Inviter(int seed) => Hash(seed, 2, 61) < 0.6 ? AdminName : OwnerName;

        foreach (var member in members)
        {
            var target = $"Member {member.Name}";
            var url = MembersPath(member.Email);

            if (member.Status == AiMemberStatus.Invited)
            {
                entries.Add((member.JoinedAt, Inviter(member.Id), "Invited", $"Invitation {member.Email}", url, $"As {RoleWord(member.Role)}"));

                continue;
            }

            if (member.JoinedAt >= since)
            {
                entries.Add((member.JoinedAt.AddDays(-2), Inviter(member.Id), "Invited", $"Invitation {member.Email}", url, $"As {RoleWord(AiMemberRole.Member)}"));
                entries.Add((member.JoinedAt, SystemActor, "Joined the workspace", target, url, "Took up the invitation"));
            }

            if (member.Role is AiMemberRole.PromptEditor or AiMemberRole.Admin && member.AccountId is null && Hash(member.Id, 5, 67) < 0.7)
                entries.Add((Now.AddDays(-(3 + (Hash(member.Id, 6, 71) * 50))), OwnerName, "Changed role", target, url, $"{RoleWord(AiMemberRole.Member)} → {RoleWord(member.Role)}"));

            if (member.Status == AiMemberStatus.Deactivated)
                entries.Add((Now.AddDays(-(2 + (Hash(member.Id, 7, 73) * 40))), AdminName, "Removed access", target, url, "Active → Deactivated; conversations kept"));
        }

        entries.Add((Now.AddDays(-41), OwnerName, "Changed role", $"Member {EditorName}", MembersPath("priya.shah@example.com"), "Member → Prompt editor"));

        foreach (var prompt in prompts.Where(prompt => prompt.IsShared))
        {
            var target = $"Shared prompt \"{prompt.Title}\"";

            if (prompt.UpdatedAt < since)
                continue;

            entries.Add((prompt.UpdatedAt, prompt.Author, prompt.IsPublished ? (prompt.Id % 3 == 0 ? "Edited shared prompt" : "Published shared prompt") : "Created shared prompt (draft)",
                target, SharedPromptPath(prompt.Id), $"{prompt.Category} · {prompt.Variables.Count} variables · {prompt.Text.Length} characters"));
        }

        entries.Add((Now.AddDays(-52), OwnerName, "Changed the model policy", "Models and policy", AdminBase + "/settings", "Default model: Parley Swift → Parley Balanced"));
        entries.Add((Now.AddDays(-33), AdminName, "Changed the model policy", "Models and policy", AdminBase + "/settings", "Monthly quota: 150,000 → 200,000 tokens per member"));
        entries.Add((Now.AddDays(-19), AdminName, "Changed the model policy", "Models and policy", AdminBase + "/settings", "Blocked terms: 1 → 2"));
        entries.Add((Now.AddDays(-12), OwnerName, "Changed the model policy", "Models and policy", AdminBase + "/settings", "Models on: Parley Code turned on"));
        entries.Add((Now.AddDays(-58), OwnerName, "Renamed the workspace", "Workspace settings", AdminBase + "/settings", "Halcyon → Halcyon Studio"));
        entries.Add((Now.AddDays(-23), OwnerName, "Changed spend caps", "Spend caps", AdminBase + "/spend",
            Invariant($"Workspace cap: {policy.WorkspaceMonthlyCap - 2_000_000:#,0} → {policy.WorkspaceMonthlyCap:#,0} tokens")));

        foreach (var (member, cap, daysAgo) in caps)
            entries.Add((Now.AddDays(-daysAgo), AdminName, "Changed member cap", $"Member {member.Name} ({member.Email})", AdminBase + "/spend",
                Invariant($"default ({policy.MonthlyTokenQuota:#,0}) → {cap:#,0} tokens a month")));

        var names = members.ToDictionary(member => member.Id, member => member.Name);

        foreach (var dlpEvent in dlpEvents)
        {
            var target = Invariant($"Sensitive data event {dlpEvent.Id} · {AiDlp.KindLabel(dlpEvent.Kind)}");
            entries.Add((dlpEvent.At, names[dlpEvent.MemberId], dlpEvent.ActionTaken == AiDlpAction.Block ? "Blocked sensitive data" : "Redacted sensitive data", target, AdminBase + "/sensitive-data",
                $"{names[dlpEvent.MemberId]}'s message; {dlpEvent.Masked}"));

            if (dlpEvent is { ReviewedAt: { } reviewedAt, ReviewedBy: { } reviewer })
            {
                if (dlpEvent.Status == AiDlpStatus.PolicyChanged)
                {
                    entries.Add((reviewedAt, reviewer, "Changed the sensitive data policy (from review)", "Sensitive data policy", AdminBase + "/sensitive-data", "Bank account (IBAN): block → redact"));
                    entries.Add((reviewedAt, reviewer, "Adjusted the policy from review", target, AdminBase + "/sensitive-data", Invariant($"{dlpEvent.Masked}; note {dlpEvent.Note.Length} characters")));
                }
                else
                {
                    entries.Add((reviewedAt, reviewer, "Dismissed sensitive data event", target, AdminBase + "/sensitive-data", Invariant($"{dlpEvent.Masked}; note {dlpEvent.Note.Length} characters")));
                }
            }
        }

        return entries.Where(entry => entry.At >= since && entry.At <= Now)
            .OrderBy(entry => entry.At)
            .Select((entry, index) => new ShowcaseAuditEntry
            {
                Id = index + 1, At = entry.At, Actor = entry.Actor, Action = entry.Action, Target = entry.Target, TargetUrl = entry.Url, Details = entry.Details,
            })
            .ToList();
    }

    /// <summary>
    /// The conversations in a fresh tab: the hand-written five (four of the member's, one pinned, and one of the owner's),
    /// then short exchanges of every demo account over the last seven weeks, with the prewritten replies.
    /// </summary>
    private static List<AiConversation> Conversations()
    {
        var messageId = 1;
        var list = new List<AiConversation>();

        void Conversation(int id, string accountId, string title, string modelId, double daysAgo, bool pinned, params (string Text, string Key, int Variant, AiFeedback Feedback)[] turns)
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
        Conversation(5, SeededOwnerId, "Explain race conditions", "deep", 2, false,
            ("Explain what a race condition is, for our new hires.", "explain", 0, AiFeedback.Helpful));

        var topics = new (string Title, string Text, string Key, string Model)[]
        {
            ("Weekly active users query", "Write a SQL query for weekly active users from the events table.", "sql", "code"),
            ("Join orders and refunds", "Help me join the orders and refunds tables in SQL so I can see net revenue.", "sql", "code"),
            ("Speeding up the monthly report", "This SQL query for the monthly report is slow. How can I speed up the join?", "sql", "code"),
            ("Async deadlock in C#", "Why does my C# async method deadlock when I call .Result on it?", "code-csharp", "code"),
            ("Grouping orders with LINQ", "How do I group orders by customer with LINQ in C#?", "code-csharp", "code"),
            ("Custom rule in a Blazor form", "How should I validate a Blazor form with a custom rule?", "code-csharp", "code"),
            ("Debouncing the search box", "How do I debounce a search box in JavaScript?", "code-js", "code"),
            ("React list not updating", "My React list doesn't update after the fetch finishes. What's wrong?", "code-js", "code"),
            ("TypeScript types for the orders API", "Write TypeScript types for our orders API response.", "code-js", "code"),
            ("Email about a delivery delay", "Draft an email to a client: the delivery moves by a week.", "email", "swift"),
            ("Thank-you note to the team", "Write a short email to the team thanking them for the release.", "email", "swift"),
            ("Follow-up after the demo", "Draft a follow-up email after yesterday's product demo.", "email", "balanced"),
            ("First two weeks for a new designer", "Plan the first two weeks for a new designer joining the team.", "plan", "balanced"),
            ("Design system workshop", "Plan a half-day workshop on our design system, with breaks.", "plan", "balanced"),
            ("Docs migration timeline", "Make a timeline for moving our docs to the new site in six weeks.", "plan", "deep"),
            ("Customer interview themes", "Summarize these customer interview notes into the top five themes.", "summary", "deep"),
            ("Board update in five bullets", "Summarize the board update in five bullet points.", "summary", "balanced"),
            ("Sprint retro recap", "Summarize our sprint retro: what went well and what to change.", "summary", "swift"),
            ("Eventual consistency, simply", "Explain what eventual consistency is, with an everyday example.", "explain", "deep"),
            ("OKRs and KPIs", "What's the difference between OKRs and KPIs?", "explain", "balanced"),
            ("How a CDN cache works", "How does a CDN cache work, in plain words?", "explain", "balanced"),
            ("Names for the reporting feature", "Give me ten name ideas for our new reporting feature.", "fallback", "swift"),
            ("A clearer pricing page", "Suggest ways to make our pricing page clearer.", "fallback", "balanced"),
            ("Questions for a design interview", "Suggest interview questions for a senior product designer.", "fallback", "balanced"),
        };
        var followUps = new (string Text, string Key)[]
        {
            ("Make it shorter, please.", "fallback"),
            ("Summarize that in three bullets.", "summary"),
            ("Can you explain the second point?", "explain"),
            ("Turn that into an email for my manager.", "email"),
        };
        var accounts = new (string Id, int Count)[] { (SeededMemberId, 66), (SeededOwnerId, 50), (SeededAdminId, 55), (SeededEditorId, 58) };
        var nextId = 6;

        for (var a = 0; a < accounts.Length; a++)
        {
            var (accountId, count) = accounts[a];

            for (var k = 0; k < count; k++)
            {
                var topic = topics[((k * 5) + (a * 3)) % topics.Length];
                var daysAgo = 0.6 + (k * (48.0 / count)) + (Hash(k, a, 21) * 0.8);
                var turnCount = 1 + Math.Min(2, (int)(Hash(k, a, 22) * 3));
                var turns = new List<(string Text, string Key, int Variant, AiFeedback Feedback)>();

                for (var t = 0; t < turnCount; t++)
                {
                    var (text, key) = t == 0 ? (topic.Text, topic.Key) : followUps[(k + t + a) % followUps.Length];
                    var variant = Hash(k, a * 10 + t, 23) < 0.5 ? 0 : 1;
                    var pick = Hash(k, a * 10 + t, 24);
                    var feedback = pick < 0.2 ? AiFeedback.Helpful : pick < 0.25 ? AiFeedback.NotHelpful : AiFeedback.None;
                    turns.Add((text, key, variant, feedback));
                }

                Conversation(nextId++, accountId, topic.Title, topic.Model, daysAgo, k > 0 && k % 23 == 0, turns.ToArray());
            }

            // A few older conversations each (two to eleven months), which a 30- or 90-day retention window deletes.
            for (var k = 0; k < 7; k++)
            {
                var topic = topics[((k * 7) + (a * 5) + 2) % topics.Length];
                Conversation(nextId++, accountId, topic.Title, topic.Model, 62 + (k * 46) + (Hash(k, a, 25) * 5), false, (topic.Text, topic.Key, Hash(k, a, 26) < 0.5 ? 0 : 1, AiFeedback.None));
            }
        }

        return list;
    }

    /// <summary>
    /// The past sensitive data events: the demo accounts' and colleagues' messages that held a card number, an ID number, an
    /// IBAN, or an API key over the last weeks, most reviewed (dismissed, or the IBAN rule switched to redact from review),
    /// and one card number blocked two hours ago that waits for review. Only the masked matches are kept.
    /// </summary>
    private static List<AiDlpEvent> DlpEvents(List<AiMember> members, List<AiConversation> conversations)
    {
        var colleagues = members.Where(member => member.AccountId is null && member.Status == AiMemberStatus.Active && member.Id > 8).ToList();
        var samChat = conversations.First(conversation => conversation.AccountId == SeededMemberId && conversation.Id > 5);
        var list = new List<AiDlpEvent>();

        void Add(double daysAgo, int memberId, AiDlpKind kind, string sample, AiDlpAction taken, AiConversation? conversation, string title, AiDlpStatus status, string? reviewer, string note)
        {
            var at = Now.AddDays(-daysAgo);
            list.Add(new AiDlpEvent
            {
                Id = list.Count + 1, At = at, MemberId = memberId, Kind = kind, ActionTaken = taken, Masked = AiDlp.Mask(kind, sample), ConversationId = conversation?.Id,
                ConversationTitle = conversation?.Title ?? title, Status = status, ReviewedBy = reviewer, ReviewedAt = reviewer is null ? null : at.AddHours(3), Note = note,
            });
        }

        Add(18, 4, AiDlpKind.NationalId, "219-09-9999", AiDlpAction.Redact, samChat, string.Empty, AiDlpStatus.Dismissed, AdminName, "A sample number from the onboarding guide.");
        Add(15, 8, AiDlpKind.PaymentCard, "5555 5555 5555 4444", AiDlpAction.Block, null, string.Empty, AiDlpStatus.Dismissed, AdminName, string.Empty);
        Add(12, colleagues[0].Id, AiDlpKind.Iban, "DE89 3704 0044 0532 0130 00", AiDlpAction.Block, null, "Supplier payment run", AiDlpStatus.PolicyChanged, OwnerName,
            "Bank details are fine once redacted; the IBAN rule redacts now.");
        Add(9, 3, AiDlpKind.ApiKey, "sk-test-4eC39HqLyjWDarjtT1zdp7dc", AiDlpAction.Block, null, string.Empty, AiDlpStatus.Dismissed, OwnerName, "The key was a test key; Priya rotated it anyway.");
        Add(6, 5, AiDlpKind.NationalId, "123-45-6789", AiDlpAction.Redact, null, "HR onboarding form", AiDlpStatus.Dismissed, AdminName, string.Empty);
        Add(3, colleagues[1].Id, AiDlpKind.PaymentCard, "4000 0566 5566 5556", AiDlpAction.Block, null, string.Empty, AiDlpStatus.Dismissed, AdminName, "Asked them to use the billing portal instead.");
        Add(1, 4, AiDlpKind.PaymentCard, "4111 1111 1111 1111", AiDlpAction.Block, null, string.Empty, AiDlpStatus.Dismissed, AdminName, "A test card from the payment docs.");
        Add(1.0 / 12, colleagues[2].Id, AiDlpKind.PaymentCard, "4242 4242 4242 4242", AiDlpAction.Block, null, string.Empty, AiDlpStatus.New, null, string.Empty);

        return list;
    }

    /// <summary>The usage ledger of the seeded conversations: one line per account, model, and day.</summary>
    private static List<AiUsageEntry> Ledger(List<AiConversation> conversations)
    {
        var ledger = new Dictionary<(string AccountId, string ModelId, DateTime Date), AiUsageEntry>();

        foreach (var conversation in conversations)
        {
            foreach (var message in conversation.Messages)
            {
                var key = (conversation.AccountId, message.ModelId, message.At.Date);

                if (!ledger.TryGetValue(key, out var line))
                    ledger[key] = line = new AiUsageEntry { AccountId = conversation.AccountId, ModelId = message.ModelId, Date = message.At.Date };

                line.Messages += message.Role == AiRole.User ? 1 : 0;
                line.Tokens += message.Tokens;
            }
        }

        return ledger.Values.OrderBy(line => line.Date).ThenBy(line => line.AccountId, StringComparer.Ordinal).ThenBy(line => line.ModelId, StringComparer.Ordinal).ToList();
    }

    /// <summary>
    /// The workspace in a fresh tab: the four demo accounts (Owner, Admin, Prompt editor, Member), a few hand-picked
    /// colleagues (an admin, a deactivated member, an invitation), and generated colleagues: mostly members, some prompt
    /// editors and admins, a few deactivated, and a few invitations waiting.
    /// </summary>
    private static List<AiMember> Members()
    {
        var list = new List<AiMember>
        {
            new() { Id = 1, AccountId = SeededOwnerId, Name = OwnerName, Email = "alex.morgan@example.com", Role = AiMemberRole.Owner, Status = AiMemberStatus.Active, JoinedAt = Now.AddDays(-410) },
            new() { Id = 2, AccountId = SeededAdminId, Name = AdminName, Email = "mei.chen@example.com", Role = AiMemberRole.Admin, Status = AiMemberStatus.Active, JoinedAt = Now.AddDays(-380) },
            new() { Id = 3, AccountId = SeededEditorId, Name = EditorName, Email = "priya.shah@example.com", Role = AiMemberRole.PromptEditor, Status = AiMemberStatus.Active, JoinedAt = Now.AddDays(-300) },
            new() { Id = 4, AccountId = SeededMemberId, Name = MemberName, Email = "sam.rivera@example.com", Role = AiMemberRole.Member, Status = AiMemberStatus.Active, JoinedAt = Now.AddDays(-120) },
            new() { Id = 5, Name = "Leo Martins", Email = "leo.martins@example.com", Role = AiMemberRole.Member, Status = AiMemberStatus.Active, JoinedAt = Now.AddDays(-90) },
            new() { Id = 6, Name = "Omar Haddad", Email = "omar.haddad@example.com", Role = AiMemberRole.Member, Status = AiMemberStatus.Deactivated, JoinedAt = Now.AddDays(-160) },
            new() { Id = 7, Name = "Nora Kim", Email = "nora.kim@example.com", Role = AiMemberRole.Member, Status = AiMemberStatus.Invited, JoinedAt = Now.AddDays(-2) },
            new() { Id = 8, Name = "Jonas Weber", Email = "jonas.weber@example.com", Role = AiMemberRole.Admin, Status = AiMemberStatus.Active, JoinedAt = Now.AddDays(-350) },
        };
        var emails = list.Select(member => member.Email).ToHashSet(StringComparer.Ordinal);

        for (var i = 0; list.Count < SeededMembers && i < 2000; i++)
        {
            var first = firstNames[(i * 7) % firstNames.Length];
            var last = lastNames[((i * 13) + (i / firstNames.Length)) % lastNames.Length];
            var email = $"{first}.{last}@example.com".ToLowerInvariant();

            if (!emails.Add(email))
                continue;

            var rolePick = Hash(i, 3, 11);
            var statusPick = Hash(i, 5, 13);
            var status = statusPick < 0.07 ? AiMemberStatus.Invited : statusPick < 0.13 ? AiMemberStatus.Deactivated : AiMemberStatus.Active;
            var role = rolePick < 0.09 ? AiMemberRole.PromptEditor : rolePick < 0.12 && status != AiMemberStatus.Invited ? AiMemberRole.Admin : AiMemberRole.Member;
            var days = status == AiMemberStatus.Invited ? 1 + (Hash(i, 7, 17) * 20) : 6 + (Hash(i, 7, 17) * 390);

            list.Add(new AiMember { Id = list.Count + 1, Name = $"{first} {last}", Email = email, Role = role, Status = status, JoinedAt = Now.AddDays(-days) });
        }

        return list;
    }

    /// <summary>Each demo account's notifications: five or six, the newest two unread; links stay on this site.</summary>
    private static List<ShowcaseNotification> Notifications(List<AiPrompt> prompts, List<AiDlpEvent> dlpEvents, List<AiMember> members)
    {
        var waiting = dlpEvents.First(item => item.Status == AiDlpStatus.New);
        var waitingText = $"{members.First(member => member.Id == waiting.MemberId).Name}'s message held {AiDlp.KindArticle(waiting.Kind)}; it was blocked. Review it in the queue.";
        var shared = prompts.Where(prompt => prompt.IsShared && prompt.IsPublished).OrderByDescending(prompt => prompt.UpdatedAt).ToList();
        var priyaPrompt = prompts.First(prompt => prompt.IsShared && prompt.AuthorId == SeededEditorId);
        var draft = prompts.First(prompt => prompt.IsShared && !prompt.IsPublished);
        var list = new List<(string AccountId, double HoursAgo, string Title, string? Text, string? Url)>
        {
            (SeededOwnerId, 1.9, "Sensitive data in a message", waitingText, AdminBase + "/sensitive-data"),
            (SeededOwnerId, 3, "Nora Kim's invitation is waiting", "Invited 2 days ago as a member. Resend it, or remove it, from Members and roles.", MembersPath("nora.kim@example.com")),
            (SeededOwnerId, 20, $"{AdminName} changed the model policy", "Blocked terms: 1 → 2. See the audit log for every change.", AdminBase + "/audit"),
            (SeededOwnerId, 52, $"New shared prompt: \"{shared[0].Title}\"", $"Published by {shared[0].Author}.", PromptPath(shared[0].Id)),
            (SeededOwnerId, 120, "3 people joined this week", "They took up their invitations and chat as members.", MembersPath()),
            (SeededOwnerId, 300, "Usage report for October is ready", "Tokens by model and member, in Usage analytics.", AdminBase),

            (SeededAdminId, 1.9, "Sensitive data in a message", waitingText, AdminBase + "/sensitive-data"),
            (SeededAdminId, 2, "A draft shared prompt needs review", $"\"{draft.Title}\" by {draft.Author} isn't published yet.", SharedPromptPath(draft.Id)),
            (SeededAdminId, 26, "Nora Kim's invitation is waiting", "Invited 2 days ago as a member.", MembersPath("nora.kim@example.com")),
            (SeededAdminId, 70, $"New shared prompt: \"{shared[1].Title}\"", $"Published by {shared[1].Author}.", PromptPath(shared[1].Id)),
            (SeededAdminId, 140, "Omar Haddad's access was removed", "Their conversations are kept; you can restore access from Members and roles.", MembersPath("omar.haddad@example.com")),
            (SeededAdminId, 340, $"{OwnerName} changed your role", $"{RoleWord(AiMemberRole.Member)} → {RoleWord(AiMemberRole.Admin)}.", SiteBase + "/profile"),

            (SeededEditorId, 4, $"{OwnerName} edited your shared prompt", $"\"{priyaPrompt.Title}\" changed. Check that it still says what you meant.", SharedPromptPath(priyaPrompt.Id)),
            (SeededEditorId, 30, $"New shared prompt: \"{shared[2].Title}\"", $"Published by {shared[2].Author}.", PromptPath(shared[2].Id)),
            (SeededEditorId, 75, "A draft shared prompt needs review", $"\"{draft.Title}\" by {draft.Author} isn't published yet.", SharedPromptPath(draft.Id)),
            (SeededEditorId, 200, $"New shared prompt: \"{shared[3].Title}\"", $"Published by {shared[3].Author}.", PromptPath(shared[3].Id)),
            (SeededEditorId, 41 * 24, $"{OwnerName} changed your role", $"{RoleWord(AiMemberRole.Member)} → {RoleWord(AiMemberRole.PromptEditor)}: you can now write and publish shared prompts.", AdminBase + "/prompts"),

            (SeededMemberId, 1.5, $"New shared prompt: \"{shared[0].Title}\"", $"Published by {shared[0].Author}. Use it from the prompt library.", PromptPath(shared[0].Id)),
            (SeededMemberId, 28, $"New shared prompt: \"{shared[4].Title}\"", $"Published by {shared[4].Author}.", PromptPath(shared[4].Id)),
            (SeededMemberId, 96, "Your monthly cap changed", "Mei Chen set your monthly cap; see how much is left under Usage.", SiteBase + "/usage"),
            (SeededMemberId, 170, $"New shared prompt: \"{shared[5].Title}\"", $"Published by {shared[5].Author}.", PromptPath(shared[5].Id)),
            (SeededMemberId, 400, $"Welcome to {ProductName}", "Start a chat, or save a prompt you use often.", SiteBase + "/chat"),
        };

        return list.OrderBy(item => Now.AddHours(-item.HoursAgo))
            .Select((item, index) => (item, index))
            .Select(pair => new ShowcaseNotification
            {
                Id = pair.index + 1,
                AccountId = pair.item.AccountId,
                At = Now.AddHours(-pair.item.HoursAgo),
                Title = pair.item.Title.Length <= 80 ? pair.item.Title : pair.item.Title[..79] + "…",
                Text = pair.item.Text,
                Url = pair.item.Url,
                // The newest two of each account are unread.
                IsRead = list.Count(other => other.AccountId == pair.item.AccountId && other.HoursAgo < pair.item.HoursAgo) >= 2,
            })
            .ToList();
    }

    /// <summary>
    /// The prompt library in a fresh tab: the demo accounts' own prompts, the four original shared prompts, and shared prompts
    /// of five teams written by the workspace's prompt editors, admins, and Owner; most are published, some are drafts.
    /// </summary>
    private static List<AiPrompt> Prompts(List<AiMember> members)
    {
        var list = new List<AiPrompt>
        {
            new() { Id = 1, OwnerId = SeededMemberId, AuthorId = SeededMemberId, Author = MemberName, Title = "Tighten my writing", Category = AiPromptCategory.Writing, Text = "Rewrite the text below so it's shorter and clearer. Keep my tone, and list what you changed.\n\n[paste text here]", Tags = { "writing" }, UpdatedAt = Now.AddDays(-20), UseCount = 7 },
            new() { Id = 2, OwnerId = SeededMemberId, AuthorId = SeededMemberId, Author = MemberName, Title = "Explain this error", Category = AiPromptCategory.Code, Text = "Explain what this error means and the three most likely causes, in plain words:\n\n[paste error here]", Tags = { "code", "debugging" }, UpdatedAt = Now.AddDays(-9), UseCount = 3 },
            new() { Id = 3, AuthorId = SeededOwnerId, Author = OwnerName, Title = "Weekly status update", Category = AiPromptCategory.Team, Text = "Turn these notes into a weekly status update with three sections: done, next, and blockers. Keep it under 150 words.\n\n{{notes}}", Variables = { "notes" }, Tags = { "writing", "team" }, UpdatedAt = Now.AddDays(-40), UseCount = 31, IsPublished = true },
            new() { Id = 4, AuthorId = SeededOwnerId, Author = OwnerName, Title = "Explain it like I'm new", Category = AiPromptCategory.Learning, Text = "Explain {{topic}} to someone in their first week on the team. Use one everyday example and no jargon.", Variables = { "topic" }, Tags = { "learning" }, UpdatedAt = Now.AddDays(-35), UseCount = 18, IsPublished = true },
            new() { Id = 5, AuthorId = SeededEditorId, Author = EditorName, Title = "Code review checklist", Category = AiPromptCategory.Code, Text = "Review the code below for correctness, readability, and tests. List problems first, then suggestions.\n\n{{code}}", Variables = { "code" }, Tags = { "code" }, UpdatedAt = Now.AddDays(-28), UseCount = 22, IsPublished = true },
            new() { Id = 6, AuthorId = SeededEditorId, Author = EditorName, Title = "Meeting summary", Category = AiPromptCategory.Team, Text = "Summarize this meeting transcript: decisions, owners, and dates. Leave out small talk.\n\n{{transcript}}", Variables = { "transcript" }, Tags = { "team", "summary" }, UpdatedAt = Now.AddDays(-1.5), UseCount = 26, IsPublished = true },
        };

        var personal = new (string Title, AiPromptCategory Category, string Text, string Tag)[]
        {
            ("Rewrite for a customer", AiPromptCategory.Writing, "Rewrite this for a customer who isn't technical. Keep it friendly and under 120 words:\n\n{{text}}", "writing"),
            ("Standup notes", AiPromptCategory.Team, "Turn my notes into a standup update: yesterday, today, and blockers.\n\n{{notes}}", "team"),
            ("Bug report from notes", AiPromptCategory.Code, "Write a bug report from these notes: steps to reproduce, expected, actual, and severity.\n\n{{notes}}", "code"),
            ("Say no politely", AiPromptCategory.Writing, "Help me decline {{request}} politely, and offer one alternative.", "writing"),
            ("Explain a chart", AiPromptCategory.Analysis, "Explain what this chart shows, the main trend, and one question it raises:\n\n{{chart}}", "analysis"),
            ("Prepare a one-on-one", AiPromptCategory.Team, "Suggest five questions for my one-on-one with {{person}} about {{topic}}.", "team"),
            ("SQL from a question", AiPromptCategory.Code, "Write a SQL query that answers: {{question}}. Explain each part in one line.", "sql"),
            ("Summarize a long thread", AiPromptCategory.Writing, "Summarize this thread: the decision, who does what, and open questions.\n\n{{thread}}", "summary"),
            ("Learning plan", AiPromptCategory.Learning, "Make a four-week learning plan for {{skill}}, an hour a day, with one small project.", "learning"),
            ("Pros and cons", AiPromptCategory.Analysis, "List the pros and cons of {{option}}, then say which matters most and why.", "analysis"),
            ("Reply to a support ticket", AiPromptCategory.Support, "Draft a reply to this support ticket. Be clear, kind, and give the next step.\n\n{{ticket}}", "support"),
            ("Name ideas", AiPromptCategory.Writing, "Give me ten name ideas for {{thing}}, each with a one-line reason.", "naming"),
            ("Test cases for a feature", AiPromptCategory.Code, "List test cases for {{feature}}: the happy path, edge cases, and failures.", "testing"),
            ("Interview questions", AiPromptCategory.Team, "Suggest interview questions for a {{role}}, with what a strong answer covers.", "hiring"),
        };
        var owners = new (string Id, string Name, int Count, int Offset)[] { (SeededMemberId, MemberName, 10, 0), (SeededOwnerId, OwnerName, 6, 3), (SeededAdminId, AdminName, 8, 5), (SeededEditorId, EditorName, 10, 2) };
        var nextId = 7;

        foreach (var (ownerId, ownerName, count, offset) in owners)
        {
            for (var k = 0; k < count; k++)
            {
                var template = personal[(k + offset) % personal.Length];
                list.Add(new AiPrompt
                {
                    Id = nextId++, OwnerId = ownerId, AuthorId = ownerId, Author = ownerName, Title = template.Title, Category = template.Category, Text = template.Text,
                    Variables = AiPromptText.VariablesIn(template.Text), Tags = { template.Tag }, UpdatedAt = Now.AddDays(-(1 + (Hash(nextId, k, 31) * 120))),
                    UseCount = (int)(Hash(nextId, k, 32) * 40),
                });
            }
        }

        var shared = new (string Title, AiPromptCategory Category, string Text)[]
        {
            ("Release notes draft", AiPromptCategory.Writing, "Write release notes for {{product}} {{version}} for the #TEAM# team. Group the changes into New, Improved, and Fixed, in plain words.\n\n{{changes}}"),
            ("Customer email reply", AiPromptCategory.Support, "Reply to this customer email for the #TEAM# team. Thank them, answer the question, and offer one next step.\n\n{{email}}"),
            ("Meeting agenda", AiPromptCategory.Team, "Draft a 30-minute agenda for a #TEAM# meeting about {{topic}}, with a goal and the time for each item."),
            ("Weekly metrics summary", AiPromptCategory.Analysis, "Summarize these weekly #TEAM# metrics: what changed, why it might have, and what to watch next week.\n\n{{metrics}}"),
            ("Pull request review", AiPromptCategory.Code, "Review this pull request for the #TEAM# codebase: correctness, tests, and readability. List blocking issues first.\n\n{{diff}}"),
            ("Incident summary", AiPromptCategory.Team, "Write an incident summary for #TEAM#: what happened, impact, timeline, and follow-ups, from these notes.\n\n{{notes}}"),
            ("Job description", AiPromptCategory.Writing, "Write a job description for a {{role}} on the #TEAM# team: the work, what we look for, and how we work."),
            ("FAQ from a document", AiPromptCategory.Support, "Turn this document into eight questions and answers #TEAM# customers would ask.\n\n{{document}}"),
            ("Competitor comparison", AiPromptCategory.Analysis, "Compare {{competitor}} with us for the #TEAM# team: strengths, gaps, and what to say when it comes up."),
            ("Onboarding checklist", AiPromptCategory.Learning, "Make a first-month onboarding checklist for a new {{role}} on the #TEAM# team, week by week."),
            ("Explain a concept", AiPromptCategory.Learning, "Explain {{concept}} for the #TEAM# team in three short paragraphs, with one example from our work."),
            ("Test plan", AiPromptCategory.Code, "Write a test plan for {{feature}} for #TEAM#: scope, cases, data, and who signs it off."),
            ("Social media post", AiPromptCategory.Writing, "Write three short posts announcing {{news}} for the #TEAM# team's channels, each under 40 words."),
            ("Survey questions", AiPromptCategory.Analysis, "Draft ten survey questions for #TEAM# about {{topic}}, mixing scales and open questions."),
            ("Project kickoff brief", AiPromptCategory.Team, "Write a one-page kickoff brief for {{project}} for #TEAM#: goal, scope, people, dates, and risks."),
            ("Apology email", AiPromptCategory.Support, "Write an apology email from the #TEAM# team about {{issue}}: own it, say what we fixed, and what happens next."),
            ("SQL review", AiPromptCategory.Code, "Review this SQL query for #TEAM#: is it correct, is it fast, and is it easy to read?\n\n{{query}}"),
            ("Decision memo", AiPromptCategory.Analysis, "Write a decision memo for #TEAM# on {{decision}}: the options, a recommendation, and what would change our mind."),
            ("Glossary entry", AiPromptCategory.Learning, "Write a glossary entry for {{term}} for the #TEAM# handbook: one sentence, then an example."),
            ("Proposal outline", AiPromptCategory.Writing, "Outline a proposal for {{client}} from the #TEAM# team: the problem, our approach, the timeline, and the price."),
            ("Escalation summary", AiPromptCategory.Support, "Summarize this escalation for #TEAM# leads: the customer, the issue, what we tried, and the ask.\n\n{{case}}"),
            ("Retro questions", AiPromptCategory.Team, "Suggest questions for a #TEAM# retrospective after {{milestone}}, to keep it honest and kind."),
            ("API documentation", AiPromptCategory.Code, "Document the {{endpoint}} endpoint for #TEAM#: what it does, parameters, a request, a response, and errors."),
            ("Quarterly goals draft", AiPromptCategory.Team, "Draft three quarterly goals for #TEAM# from these priorities, each with a measure.\n\n{{priorities}}"),
            ("Help center article", AiPromptCategory.Support, "Write a help center article for #TEAM# customers on how to {{task}}, in numbered steps."),
            ("Training quiz", AiPromptCategory.Learning, "Write a five-question quiz for #TEAM# on {{topic}}, with the answers and why."),
        };
        var teams = new[] { "Engineering", "Sales", "Support", "Marketing", "Finance" };

        // Authors: the demo Owner, Admin, and Prompt editor (who get notifications) and the colleagues who edit prompts (who don't).
        var authors = new List<(string Name, string? AccountId)> { (OwnerName, SeededOwnerId), (AdminName, SeededAdminId), (EditorName, SeededEditorId), (EditorName, SeededEditorId) };
        authors.AddRange(members.Where(member => member.AccountId is null && member.Status == AiMemberStatus.Active && member.Role is AiMemberRole.PromptEditor or AiMemberRole.Admin)
            .Select(member => (member.Name, (string?)null)));

        for (var b = 0; b < shared.Length; b++)
        {
            for (var t = 0; t < teams.Length; t++)
            {
                if (Hash(b, t, 41) < 0.11)
                    continue;

                var (title, category, text) = shared[b];
                var body = text.Replace("#TEAM#", teams[t], StringComparison.Ordinal);
                var author = authors[(int)(Hash(b, t, 42) * authors.Count) % authors.Count];
                var published = Hash(b, t, 43) >= 0.14;
                list.Add(new AiPrompt
                {
                    Id = nextId++, AuthorId = author.AccountId, Author = author.Name, Title = $"{title} ({teams[t]})", Category = category, Text = body,
                    Variables = AiPromptText.VariablesIn(body), Tags = new[] { category.ToString().ToLowerInvariant(), teams[t].ToLowerInvariant() }.Distinct().ToList(),
                    UpdatedAt = Now.AddDays(-(2 + (Hash(b, t, 44) * 220))), UseCount = published ? (int)(Hash(b, t, 45) * 260) : 0, IsPublished = published,
                });
            }
        }

        return list;
    }

    /// <summary>
    /// Sets the seeded spend caps from this month's usage: the demo member's own cap so they're at about 75 % (one push of the
    /// demo control passes 80 %), Leo Martins's below what he used (blocked), Jonas Weber's raised to 400,000, and the
    /// workspace cap so every member together is at about 70 %. Returns the members' caps, for the audit log.
    /// </summary>
    private static List<(AiMember Member, int Cap, double DaysAgo)> SetCaps(List<AiMember> members, List<AiUsageEntry> ledger, AiPolicy policy)
    {
        var history = UsageHistory();
        bool InMonth(DateTime date) => date.Year == Now.Year && date.Month == Now.Month;
        long Month(AiMember member) =>
            (history.TryGetValue(member.Id, out var points) ? points.Where(point => InMonth(point.Date)).Sum(point => (long)point.Tokens) : 0)
            + (member.AccountId is { } accountId ? ledger.Where(line => line.AccountId == accountId && InMonth(line.Date)).Sum(line => (long)line.Tokens) : 0);
        int Round(double value, int step, bool up) => (int)((up ? Math.Ceiling(value / step) : Math.Floor(value / step)) * step);

        var sam = members.First(member => member.AccountId == SeededMemberId);
        var leo = members.First(member => member.Id == 5);
        var jonas = members.First(member => member.Id == 8);
        sam.MonthlyCap = Math.Clamp(Round(Month(sam) / 0.76, 5_000, true), 1_000, 10_000_000);
        leo.MonthlyCap = Math.Clamp(Round(Month(leo), 1_000, false), 1_000, 10_000_000);
        jonas.MonthlyCap = 400_000;
        policy.WorkspaceMonthlyCap = Round(members.Sum(Month) / 0.70, 500_000, true);

        return new List<(AiMember, int, double)> { (jonas, 400_000, 21), (sam, sam.MonthlyCap.Value, 4), (leo, leo.MonthlyCap.Value, 2.5) };
    }

    /// <summary>A role in words for the middle of a sentence, like "prompt editor".</summary>
    private static string RoleWord(AiMemberRole role) => role switch
    {
        AiMemberRole.PromptEditor => "Prompt editor",
        _ => role.ToString(),
    };

    #endregion

    #endregion
}

/// <summary>A fresh tab's data, built by <see cref="AiSeedData.Build" />.</summary>
public sealed record AiSeedSet(
    List<AiMember> Members,
    List<AiConversation> Conversations,
    List<AiPrompt> Prompts,
    List<AiUsageEntry> Usage,
    List<ShowcaseAuditEntry> Audit,
    List<ShowcaseNotification> Notifications,
    AiPolicy Policy,
    List<AiDlpEvent> DlpEvents);
