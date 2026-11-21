using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

/// <summary>
/// The workspace's governance (AC-4) and sensitive data protection (AC-7i):
/// <list type="bullet">
/// <item><b>Spend caps</b>, in tokens (the unit the site already counts; there is no pricing): a monthly cap for the whole
/// workspace, a default cap for every member, and members' own caps (up to the workspace cap). At 80 % of a cap chat warns
/// the scope and the person hears once a month (the member for their own cap, the Owner and Admins for the workspace's);
/// at 100 % <see cref="Send" /> refuses until an Admin or the Owner raises the cap above what is used, which unblocks at once
/// and tells whoever it unblocked. A demo control pushes usage up so the thresholds can be watched.</item>
/// <item><b>Retention</b> (the Owner): deleting conversations older than the window, with each member whose conversations
/// went told once.</item>
/// <item><b>Sensitive data</b>: the policy's patterns (<see cref="AiDlp" />) redact or block a message as it is sent; each
/// match goes to the review queue with the member, the kind, what was done, and the match masked (never the raw value),
/// where Admins and the Owner dismiss it or adjust the policy from it.</item>
/// </list>
/// Every change is audited.
/// </summary>
public sealed partial class AiState
{
    #region Fields and Constants

    /// <summary>The longest note on a reviewed sensitive data event.</summary>
    public const int MaxReviewNoteLength = 200;

    /// <summary>The highest monthly workspace cap.</summary>
    public const int MaxWorkspaceCap = 100_000_000;

    /// <summary>The lowest monthly workspace cap.</summary>
    public const int MinWorkspaceCap = 10_000;

    /// <summary>The most demo usage lines (one per member, model, and day).</summary>
    private const int MaxDemoUsage = 2000;

    /// <summary>The most sensitive data events the tab keeps (the oldest reviewed ones go first).</summary>
    private const int MaxDlpEvents = 500;

    /// <summary>The most sensitive data events one message records.</summary>
    private const int MaxDlpEventsPerMessage = 5;

    /// <summary>The most spend alerts remembered (so each cap warns once a month).</summary>
    private const int MaxSpendAlerts = 400;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Adds demo usage to a member's month (Admins and the Owner; the Spend page's "Demo: use tokens"), so the 80 % warning and
    /// the 100 % block can be watched. Invitations have no usage. Audited.
    /// </summary>
    public AiResult AddDemoUsage(int memberId, int tokens)
    {
        if (!Can(AiPermission.ManageSpend))
            return AiResult.NotAllowed;

        if (FindMember(memberId) is not { } member)
            return AiResult.NotFound;

        if (member.Status == AiMemberStatus.Invited)
            return AiResult.WrongState;

        if (tokens is < 1 or > MaxQuota || !AddDemoTokens(member, tokens))
            return AiResult.Invalid;

        Record("Added demo usage", MemberTarget(member), AiSeedData.AdminBase + "/spend", FormattableString.Invariant($"{tokens:#,0} tokens (demo control)"));
        CheckSpendAlerts(member);
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>
    /// Adds demo usage worth 5 % of the workspace cap, spread over the active members who can't sign in (so the demo accounts'
    /// own caps aren't touched). Admins and the Owner. Audited.
    /// </summary>
    public AiResult AddWorkspaceDemoUsage(out int tokens)
    {
        tokens = 0;

        if (!Can(AiPermission.ManageSpend))
            return AiResult.NotAllowed;

        var colleagues = members.Where(member => member.Status == AiMemberStatus.Active && member.AccountId is null).ToList();

        if (colleagues.Count == 0)
            colleagues = members.Where(member => member.Status == AiMemberStatus.Active).ToList();

        tokens = Math.Max(colleagues.Count, policy.WorkspaceMonthlyCap / 20);
        var share = tokens / colleagues.Count;

        foreach (var colleague in colleagues)
        {
            if (!AddDemoTokens(colleague, share))
                return AiResult.Invalid;
        }

        tokens = share * colleagues.Count;
        Record("Added demo usage", "Workspace", AiSeedData.AdminBase + "/spend", FormattableString.Invariant($"{tokens:#,0} tokens over {colleagues.Count} members without a sign-in (demo control)"));
        CheckSpendAlerts(null);
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>
    /// Adjusts the policy for an event's kind of data from the review queue (Admins and the Owner): on or off, and redact or
    /// block. The event is marked "Policy changed" with the note (or what changed). Audited on the event and the policy.
    /// </summary>
    public AiResult AdjustDlpFromReview(int eventId, bool enabled, AiDlpAction action, string? note)
    {
        if (!Can(AiPermission.ReviewSensitiveData))
            return AiResult.NotAllowed;

        if (dlpEvents.FirstOrDefault(item => item.Id == eventId) is not { } dlpEvent)
            return AiResult.NotFound;

        note = note?.Trim() ?? string.Empty;

        if (note.Length > MaxReviewNoteLength || !Enum.IsDefined(action))
            return AiResult.Invalid;

        var rule = policy.DlpRules.First(item => item.Kind == dlpEvent.Kind);

        if (rule.Enabled == enabled && (!enabled || rule.Action == action))
            return AiResult.NoChange;

        var change = $"{AiDlp.KindLabel(rule.Kind)}: {RuleWords(rule)} → {RuleWords(new AiDlpRule { Kind = rule.Kind, Enabled = enabled, Action = action })}";
        rule.Enabled = enabled;
        rule.Action = enabled ? action : rule.Action;
        dlpEvent.Status = AiDlpStatus.PolicyChanged;
        dlpEvent.Note = note.Length > 0 ? note : $"Policy adjusted from review: {change}";
        dlpEvent.ReviewedAt = Clock.Now;
        dlpEvent.ReviewedBy = ActorName();
        Record("Changed the sensitive data policy (from review)", "Sensitive data policy", ReviewPath, change);
        Record("Adjusted the policy from review", DlpTarget(dlpEvent), ReviewPath, $"{dlpEvent.Masked}; {change}; note {note.Length} characters");
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>Sets several members' own caps at once (or back to the default with <see langword="null" />), each like <see cref="SetMemberCap" />.</summary>
    public AiBulkOutcome BulkSetMemberCap(IReadOnlyCollection<int> memberIds, int? cap)
    {
        if (!Can(AiPermission.ManageSpend))
            return new AiBulkOutcome(AiResult.NotAllowed, 0, 0, 0);

        if (cap is { } value && (value is < MinQuota or > MaxQuota))
            return new AiBulkOutcome(AiResult.Invalid, 0, 0, 0);

        if (cap > policy.WorkspaceMonthlyCap)
            return new AiBulkOutcome(AiResult.AboveWorkspaceCap, 0, 0, 0);

        var list = (memberIds ?? Array.Empty<int>()).Distinct().ToList();

        if (list.Count == 0)
            return new AiBulkOutcome(AiResult.Invalid, 0, 0, 0);

        int changed = 0, unchanged = 0, belowUse = 0;
        batchDepth++;

        try
        {
            foreach (var id in list)
            {
                switch (SetMemberCap(id, cap))
                {
                    case AiResult.Ok:
                        changed++;
                        break;

                    case AiResult.BelowUse:
                        belowUse++;
                        break;

                    default:
                        unchanged++;
                        break;
                }
            }
        }
        finally
        {
            batchDepth--;
        }

        FlushBatch();

        // "Protected" here counts the members who already used more than the new cap.
        return new AiBulkOutcome(changed == 0 ? AiResult.NoChange : AiResult.Ok, changed, unchanged, belowUse);
    }

    /// <summary>Returns the cap that applies to a member this month: their own, or the default member cap.</summary>
    public int CapOf(AiMember member) => member.MonthlyCap ?? policy.MonthlyTokenQuota;

    /// <summary>Dismisses several new sensitive data events at once (Admins and the Owner), with one optional note. Audited per event.</summary>
    public AiBulkOutcome DismissDlpEvents(IReadOnlyCollection<int> eventIds, string? note)
    {
        if (!Can(AiPermission.ReviewSensitiveData))
            return new AiBulkOutcome(AiResult.NotAllowed, 0, 0, 0);

        note = note?.Trim() ?? string.Empty;
        var list = (eventIds ?? Array.Empty<int>()).Distinct().ToList();

        if (list.Count == 0 || note.Length > MaxReviewNoteLength)
            return new AiBulkOutcome(AiResult.Invalid, 0, 0, 0);

        int changed = 0, unchanged = 0;

        foreach (var id in list)
        {
            if (dlpEvents.FirstOrDefault(item => item.Id == id) is not { Status: AiDlpStatus.New } dlpEvent)
            {
                unchanged++;

                continue;
            }

            dlpEvent.Status = AiDlpStatus.Dismissed;
            dlpEvent.Note = note;
            dlpEvent.ReviewedAt = Clock.Now;
            dlpEvent.ReviewedBy = ActorName();
            Record("Dismissed sensitive data event", DlpTarget(dlpEvent), ReviewPath, $"{dlpEvent.Masked}; note {note.Length} characters");
            changed++;
        }

        if (changed > 0)
            RaiseChanged();

        return new AiBulkOutcome(changed == 0 ? AiResult.NoChange : AiResult.Ok, changed, unchanged, 0);
    }

    /// <summary>Returns a sensitive data event (Admins and the Owner), or <see langword="null" />.</summary>
    public AiDlpEvent? FindDlpEvent(int eventId) => Can(AiPermission.ReviewSensitiveData) ? dlpEvents.FirstOrDefault(item => item.Id == eventId) : null;

    /// <summary>Returns how many members have conversations a retention window would delete now.</summary>
    public int MembersWithConversationsOlderThan(int days) =>
        days <= 0 ? 0 : conversations.Where(conversation => conversation.UpdatedAt < Clock.Now.AddDays(-days)).Select(conversation => conversation.AccountId).Distinct().Count();

    /// <summary>
    /// Saves the sensitive data policy (Admins and the Owner): one rule per kind of data, each on or off, redact or block.
    /// Audited with what changed.
    /// </summary>
    public AiResult SaveDlpPolicy(IReadOnlyList<AiDlpRule> rules)
    {
        if (!Can(AiPermission.ReviewSensitiveData))
            return AiResult.NotAllowed;

        if (rules is null || !ValidDlpRules(rules))
            return AiResult.Invalid;

        var changes = new List<string>();

        foreach (var rule in policy.DlpRules)
        {
            var next = rules.First(item => item.Kind == rule.Kind);

            if (rule.Enabled != next.Enabled || (next.Enabled && rule.Action != next.Action))
                changes.Add($"{AiDlp.KindLabel(rule.Kind)}: {RuleWords(rule)} → {RuleWords(next)}");
        }

        if (changes.Count == 0)
            return AiResult.NoChange;

        foreach (var rule in policy.DlpRules)
        {
            var next = rules.First(item => item.Kind == rule.Kind);
            rule.Enabled = next.Enabled;
            rule.Action = next.Action;
        }

        Record("Changed the sensitive data policy", "Sensitive data policy", ReviewPath, string.Join("; ", changes));
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>
    /// Saves the workspace's monthly cap and the default member cap (Admins and the Owner). The workspace cap must be above
    /// what the workspace used this month when it changes (BelowUse) and at least every member's own cap (WrongState); the
    /// default can't be above it (AboveWorkspaceCap). Raising a cap that blocked chat unblocks at once and tells whoever it
    /// unblocked. Audited.
    /// </summary>
    public AiResult SaveSpendCaps(int workspaceCap, int defaultMemberCap)
    {
        if (!Can(AiPermission.ManageSpend))
            return AiResult.NotAllowed;

        if (workspaceCap is < MinWorkspaceCap or > MaxWorkspaceCap || defaultMemberCap is < MinQuota or > MaxQuota)
            return AiResult.Invalid;

        if (defaultMemberCap > workspaceCap)
            return AiResult.AboveWorkspaceCap;

        if (members.Any(member => member.MonthlyCap > workspaceCap))
            return AiResult.WrongState;

        var before = WorkspaceSpend;

        if (workspaceCap != policy.WorkspaceMonthlyCap && workspaceCap <= before.Used)
            return AiResult.BelowUse;

        if (workspaceCap == policy.WorkspaceMonthlyCap && defaultMemberCap == policy.MonthlyTokenQuota)
            return AiResult.NoChange;

        var usage = MonthUsageByEmail();
        var blockedBefore = members.Where(member => member.Status == AiMemberStatus.Active && member.MonthlyCap is null && SpendOf(member, usage).Level == AiSpendLevel.Blocked).ToList();
        var details = new List<string>();

        if (workspaceCap != policy.WorkspaceMonthlyCap)
            details.Add(FormattableString.Invariant($"Workspace cap: {policy.WorkspaceMonthlyCap:#,0} → {workspaceCap:#,0} tokens"));

        if (defaultMemberCap != policy.MonthlyTokenQuota)
            details.Add(FormattableString.Invariant($"Default member cap: {policy.MonthlyTokenQuota:#,0} → {defaultMemberCap:#,0} tokens"));

        policy.WorkspaceMonthlyCap = workspaceCap;
        policy.MonthlyTokenQuota = defaultMemberCap;
        Record("Changed spend caps", "Spend caps", AiSeedData.AdminBase + "/spend", string.Join("; ", details));

        if (before.Level == AiSpendLevel.Blocked && WorkspaceSpend.Level != AiSpendLevel.Blocked)
            NotifyMembers("Chat is open again", FormattableString.Invariant($"{ActorName()} raised the workspace's monthly cap to {workspaceCap:#,0} tokens."), AiSeedData.SiteBase + "/chat");

        foreach (var member in blockedBefore.Where(member => SpendOf(member, usage).Level != AiSpendLevel.Blocked))
            Notify(member.AccountId, "Your cap was raised: you can chat again", FormattableString.Invariant($"The default monthly cap is now {defaultMemberCap:#,0} tokens."), AiSeedData.SiteBase + "/usage");

        CheckSpendAlerts(null);
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>
    /// Sets a member's own monthly cap (Admins and the Owner), or back to the default with <see langword="null" />. A cap must be
    /// in range (Invalid), at most the workspace cap (AboveWorkspaceCap), and above what they used this month (BelowUse).
    /// Raising a cap that blocked them unblocks at once; they hear about any change. Audited.
    /// </summary>
    public AiResult SetMemberCap(int memberId, int? cap)
    {
        if (!Can(AiPermission.ManageSpend))
            return AiResult.NotAllowed;

        if (FindMember(memberId) is not { } member)
            return AiResult.NotFound;

        if (member.Status == AiMemberStatus.Invited)
            return AiResult.WrongState;

        if (cap is { } value)
        {
            if (value is < MinQuota or > MaxQuota)
                return AiResult.Invalid;

            if (value > policy.WorkspaceMonthlyCap)
                return AiResult.AboveWorkspaceCap;
        }

        if (member.MonthlyCap == cap)
            return AiResult.NoChange;

        var before = SpendOf(member);

        if (cap is { } newCap && newCap <= before.Used)
            return AiResult.BelowUse;

        var from = member.MonthlyCap is { } own ? FormattableString.Invariant($"{own:#,0}") : FormattableString.Invariant($"default ({policy.MonthlyTokenQuota:#,0})");
        var to = cap is { } set ? FormattableString.Invariant($"{set:#,0}") : FormattableString.Invariant($"default ({policy.MonthlyTokenQuota:#,0})");
        member.MonthlyCap = cap;
        Record("Changed member cap", MemberTarget(member), AiSeedData.AdminBase + "/spend", $"{from} → {to} tokens a month");

        var after = SpendOf(member);
        Notify(member.AccountId, before.Level == AiSpendLevel.Blocked && after.Level != AiSpendLevel.Blocked ? "Your cap was raised: you can chat again" : "Your monthly cap changed",
            FormattableString.Invariant($"{ActorName()} set your monthly cap to {CapOf(member):#,0} tokens; you've used {after.Used:#,0} this month."), AiSeedData.SiteBase + "/usage");
        CheckSpendAlerts(member);
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>
    /// Returns every joined member's spend status this month, by member number, in one pass over the usage (Admins and the
    /// Owner; empty for anyone else).
    /// </summary>
    public IReadOnlyDictionary<int, AiSpendStatus> SpendByMember()
    {
        if (!Can(AiPermission.ManageSpend) && !Can(AiPermission.ViewAnalytics))
            return new Dictionary<int, AiSpendStatus>();

        var usage = MonthUsageByEmail();

        return members.Where(member => member.Status != AiMemberStatus.Invited).ToDictionary(member => member.Id, member => SpendOf(member, usage));
    }

    /// <summary>Returns a member's spend status this month: tokens used against their cap.</summary>
    public AiSpendStatus SpendOf(AiMember member) => SpendOf(member, MonthUsageByEmail());

    #endregion

    #region Private Methods

    private static string DlpTarget(AiDlpEvent dlpEvent) => FormattableString.Invariant($"Sensitive data event {dlpEvent.Id} · {AiDlp.KindLabel(dlpEvent.Kind)}");

    private static string RuleWords(AiDlpRule rule) => !rule.Enabled ? "off" : rule.Action == AiDlpAction.Block ? "block" : "redact";

    private static AiSpendStatus StatusOf(long used, long cap) =>
        new(used, cap, used >= cap ? AiSpendLevel.Blocked : used * 5 >= cap * 4 ? AiSpendLevel.Warning : AiSpendLevel.Ok);

    /// <summary>Checks a set of rules: one per kind of data, with defined actions.</summary>
    private static bool ValidDlpRules(IReadOnlyList<AiDlpRule>? rules) =>
        rules is { Count: 4 } && rules.All(rule => rule is not null && Enum.IsDefined(rule.Kind) && Enum.IsDefined(rule.Action)) && Unique(rules.Select(rule => rule.Kind));

    /// <summary>Adds demo tokens to a member's day (one line per member, model, and day); <see langword="false" /> when the list is full.</summary>
    private bool AddDemoTokens(AiMember member, int tokens)
    {
        var date = Clock.Now.Date;

        if (demoUsage.FirstOrDefault(line => line.MemberId == member.Id && line.Date == date && line.ModelId == policy.DefaultModelId) is { } line)
        {
            line.Tokens = (int)Math.Min(MaxWorkspaceCap, (long)line.Tokens + tokens);

            return true;
        }

        if (demoUsage.Count >= MaxDemoUsage)
            return false;

        demoUsage.Add(new AiDemoUsage { MemberId = member.Id, Date = date, ModelId = policy.DefaultModelId, Tokens = tokens });

        return true;
    }

    /// <summary>
    /// Raises the 80 % and 100 % alerts once per cap and month: to a member (or nobody for <see langword="null" />) about their
    /// own cap, and to the Owner and Admins about the workspace's. A raised cap is a new cap, so it can alert again.
    /// </summary>
    private void CheckSpendAlerts(AiMember? member)
    {
        var usage = MonthUsageByEmail();
        var month = Clock.Now.ToString("yyyy-MM", CultureInfo.InvariantCulture);

        if (member is { Status: AiMemberStatus.Active } && SpendOf(member, usage) is { Level: not AiSpendLevel.Ok } memberStatus && AddAlert($"m{member.Id.ToString(CultureInfo.InvariantCulture)}", month, memberStatus))
        {
            Notify(member.AccountId, memberStatus.Level == AiSpendLevel.Blocked ? "You've reached your monthly cap: chat is paused" : "You've used 80% of your monthly cap",
                memberStatus.Level == AiSpendLevel.Blocked
                    ? $"Ask an Admin or {OwnerName} (the Owner) to raise it, or wait until the 1st."
                    : FormattableString.Invariant($"{memberStatus.Used:#,0} of {memberStatus.Cap:#,0} tokens. At 100% chat pauses until an admin raises your cap."),
                AiSeedData.SiteBase + "/usage", fromSystem: true);
        }

        var workspace = StatusOf(usage.Values.Sum(item => item.Tokens), policy.WorkspaceMonthlyCap);

        if (workspace.Level != AiSpendLevel.Ok && AddAlert("w", month, workspace))
        {
            NotifyManagers(workspace.Level == AiSpendLevel.Blocked ? "The workspace reached its monthly cap" : "The workspace used 80% of its monthly cap",
                workspace.Level == AiSpendLevel.Blocked
                    ? "Nobody can chat until you raise the workspace cap on the Spend page."
                    : FormattableString.Invariant($"{workspace.Used:#,0} of {workspace.Cap:#,0} tokens. At 100% chat pauses for everyone."),
                AiSeedData.AdminBase + "/spend", fromSystem: true);
        }
    }

    /// <summary>Remembers an alert of a scope's cap this month; returns whether it is new (a block also counts as the warning).</summary>
    private bool AddAlert(string scope, string month, AiSpendStatus status)
    {
        var level = status.Level == AiSpendLevel.Blocked ? "100" : "80";
        var key = FormattableString.Invariant($"{scope}|{month}|{level}|{status.Cap}");

        if (spendAlerts.Contains(key))
            return false;

        spendAlerts.Add(key);

        if (level == "100")
        {
            var warning = FormattableString.Invariant($"{scope}|{month}|80|{status.Cap}");

            if (!spendAlerts.Contains(warning))
                spendAlerts.Add(warning);
        }

        while (spendAlerts.Count > MaxSpendAlerts)
            spendAlerts.RemoveAt(0);

        return true;
    }

    /// <summary>
    /// Deletes conversations that haven't changed within the retention period and tells each member whose conversations
    /// went, once (not whoever applied it); returns how many were deleted (the caller audits it).
    /// </summary>
    private int ApplyRetention()
    {
        if (policy.RetentionDays <= 0)
            return 0;

        var cutoff = Clock.Now.AddDays(-policy.RetentionDays);
        var removed = conversations.Where(conversation => conversation.UpdatedAt < cutoff).ToList();

        if (removed.Count == 0)
            return 0;

        conversations.RemoveAll(conversation => conversation.UpdatedAt < cutoff);

        foreach (var group in removed.GroupBy(conversation => conversation.AccountId))
        {
            Notify(group.Key, group.Count() == 1 ? "1 conversation was deleted by retention" : FormattableString.Invariant($"{group.Count()} conversations were deleted by retention"),
                FormattableString.Invariant($"{policy.WorkspaceName} keeps conversations for {policy.RetentionDays} days; older ones are deleted."), AiSeedData.SiteBase + "/chat");
        }

        return removed.Count;
    }

    /// <summary>
    /// Records what the sensitive data policy did with a message: one event per match (a few at most), audited with the kind
    /// and the match masked; the Owner and Admins hear about it (one notice a minute at most).
    /// </summary>
    private void RecordDlpEvents(AiMember member, AiConversation? conversation, IReadOnlyList<AiDlpMatch> matches, AiDlpAction taken)
    {
        foreach (var match in matches.Take(MaxDlpEventsPerMessage))
        {
            if (nextDlpEventId >= NextIdLimit)
                return;

            var dlpEvent = new AiDlpEvent
            {
                Id = nextDlpEventId++, At = Clock.Now, MemberId = member.Id, Kind = match.Kind, ActionTaken = taken, Masked = match.Masked, Status = AiDlpStatus.New,
                ConversationId = conversation?.Id, ConversationTitle = conversation?.Title ?? string.Empty,
            };
            dlpEvents.Add(dlpEvent);
            Record(taken == AiDlpAction.Block ? "Blocked sensitive data" : "Redacted sensitive data", DlpTarget(dlpEvent), ReviewPath,
                $"{member.Name}'s message; {match.Masked}{(conversation is null ? null : $"; conversation \"{conversation.Title}\"")}");
        }

        // The oldest reviewed events go first when the queue is full.
        while (dlpEvents.Count > MaxDlpEvents && dlpEvents.FindIndex(item => item.Status != AiDlpStatus.New) is >= 0 and var index)
            dlpEvents.RemoveAt(index);

        foreach (var manager in members.Where(item => item.Status == AiMemberStatus.Active && RoleAllows(item.Role, AiPermission.ReviewSensitiveData)).ToList())
        {
            if (!notifications.Any(notice => notice.AccountId == manager.AccountId && notice.Url == ReviewPath && notice.At > Clock.Now.AddMinutes(-1)))
            {
                Notify(manager.AccountId, "Sensitive data in a message",
                    $"{member.Name}'s message held {AiDlp.KindArticle(matches[0].Kind)}; it was {(taken == AiDlpAction.Block ? "blocked" : "redacted")}. Review it in the queue.", ReviewPath);
            }
        }
    }

    /// <summary>Returns the refusal when a cap blocks the member: the workspace's first, then their own; <see langword="null" /> when they can chat.</summary>
    private AiResult? SpendBlock(AiMember member)
    {
        var usage = MonthUsageByEmail();

        if (StatusOf(usage.Values.Sum(item => item.Tokens), policy.WorkspaceMonthlyCap).Level == AiSpendLevel.Blocked)
            return AiResult.WorkspaceCapReached;

        return SpendOf(member, usage).Level == AiSpendLevel.Blocked ? AiResult.OverQuota : null;
    }

    private AiSpendStatus SpendOf(AiMember member, IReadOnlyDictionary<string, (int Messages, long Tokens)> usage) =>
        StatusOf(usage.TryGetValue(member.Email, out var used) ? used.Tokens : 0, CapOf(member));

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets how many conversations the workspace holds (every member's), for the retention preview.</summary>
    public int ConversationCount => conversations.Count;

    /// <summary>Gets the signed-in member's spend status this month, or <see langword="null" /> when nobody active is signed in.</summary>
    public AiSpendStatus? CurrentSpend => CurrentMember is { Status: AiMemberStatus.Active } member ? SpendOf(member) : null;

    /// <summary>Gets the sensitive data events (Admins and the Owner; empty for anyone else), newest first.</summary>
    public IReadOnlyList<AiDlpEvent> DlpEvents => Can(AiPermission.ReviewSensitiveData)
        ? dlpEvents.OrderByDescending(item => item.At).ThenByDescending(item => item.Id).ToList()
        : Array.Empty<AiDlpEvent>();

    /// <summary>Gets how many sensitive data events wait for review (Admins and the Owner; 0 for anyone else).</summary>
    public int NewDlpEventCount => Can(AiPermission.ReviewSensitiveData) ? dlpEvents.Count(item => item.Status == AiDlpStatus.New) : 0;

    /// <summary>Gets the workspace's spend status this month: every member's tokens against the workspace cap.</summary>
    public AiSpendStatus WorkspaceSpend => StatusOf(MonthUsageByEmail().Values.Sum(item => item.Tokens), policy.WorkspaceMonthlyCap);

    private static string ReviewPath => AiSeedData.AdminBase + "/sensitive-data";

    #endregion
}
