using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

/// <summary>
/// Fixed, fictitious content for the "Beacon Desk" help desk: customers, companies, tickets, and articles are made up,
/// and every time is relative to a fixed "now", so each load shows the same queue and SLA states.
/// </summary>
public static class HelpDeskSeedData
{
    #region Fields and Constants

    /// <summary>The role of a help desk admin: everything an agent does, plus agents and teams, SLA policies, and categories.</summary>
    public const string AdminRole = "admin";

    /// <summary>The demo admin.</summary>
    public const string AdminName = "Morgan Lee";

    /// <summary>The role of a support agent (the agent desk).</summary>
    public const string AgentRole = "agent";

    /// <summary>The demo agent.</summary>
    public const string AgentName = "Jordan Ashdown";

    /// <summary>The role of a customer (the help center's requests).</summary>
    public const string CustomerRole = "customer";

    /// <summary>The ID of the seeded customer account.</summary>
    public const string SeededCustomerId = "ana";

    /// <summary>The seeded customer's email; tickets from it belong to the account.</summary>
    public const string SeededCustomerEmail = "ana.ruiz@example.com";

    /// <summary>The fictitious product name.</summary>
    public const string ProductName = "Beacon Desk";

    /// <summary>The number of the newest generated ticket; the others count down from it, so the hand-written tickets stay the newest.</summary>
    private const int FirstGeneratedTicketId = 1040;

    /// <summary>How many older tickets are generated behind the hand-written queue (about 250 tickets in all).</summary>
    private const int GeneratedTicketCount = 231;

    /// <summary>The base route of the site, for the links in seeded audit entries and notifications.</summary>
    private const string SiteBase = DemoRouteConstants.Showcase_URL_HelpDesk;

    /// <summary>When a fresh tab's clock starts; the seeded times are relative to it, and the clock moves on from there (D-029).</summary>
    public static readonly DateTime Now = new(2026, 11, 24, 10, 30, 0);

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>The support roster: the two demo staff accounts, and two agents without a sign-in.</summary>
    public static List<HelpDeskAgent> Agents() => new()
    {
        new("jordan", AgentName, "jordan@beacondesk.example", "care", IsAdmin: false, IsActive: true, AccountId: "jordan"),
        new("priya", "Priya Nair", "priya@beacondesk.example", "tech", IsAdmin: false, IsActive: true, AccountId: null),
        new("marcus", "Marcus Osei", "marcus@beacondesk.example", "tech", IsAdmin: false, IsActive: true, AccountId: null),
        new("morgan", AdminName, "morgan@beacondesk.example", "care", IsAdmin: true, IsActive: true, AccountId: "morgan"),
    };

    /// <summary>
    /// The archive behind the reports: 30 days of closed tickets per agent and priority before the seed day. The numbers come
    /// from a fixed formula, so every tab shows the same history.
    /// </summary>
    public static IReadOnlyList<HelpDeskArchiveDay> Archive()
    {
        var agents = new[] { "jordan", "priya", "marcus", "morgan" };
        var load = new[] { 1.0, 0.9, 0.85, 0.35 };
        var rows = new List<HelpDeskArchiveDay>();
        var today = DateOnly.FromDateTime(Now);

        for (var day = 30; day >= 1; day--)
        {
            var date = today.AddDays(-day);
            var weekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

            for (var a = 0; a < agents.Length; a++)
            {
                foreach (var priority in Enum.GetValues<HelpDeskPriority>())
                {
                    var p = (int)priority;
                    var seed = Mix((day * 31) + (a * 7) + (p * 3));
                    var basis = priority switch { HelpDeskPriority.Low => 1.2, HelpDeskPriority.Normal => 2.2, HelpDeskPriority.High => 1.3, _ => 0.5 };
                    var resolved = (int)Math.Round(basis * load[a] * (weekend ? 0.35 : 1.0) + (seed % 3) - 1);
                    resolved = Math.Max(0, resolved);
                    var created = Math.Max(0, resolved + (int)(Mix(seed) % 3) - 1);

                    // Each archived ticket is drawn on its own: SLA compliance dips in a backlog week mid-month and on urgent
                    // tickets; about 45% of resolved tickets get rated, and one in ten ratings is bad (more in the backlog week).
                    var backlog = day is >= 12 and <= 17 ? 18 : 0;
                    var inSlaPercent = 90 - backlog - (priority == HelpDeskPriority.Urgent ? 10 : 0) - (a == 2 ? 5 : 0);
                    var inSla = Enumerable.Range(0, resolved).Count(i => (int)Mix(seed + 11 + (ulong)(i * 17)) % 100 < inSlaPercent);
                    var firstReply = Enumerable.Range(0, created).Count(i => (int)Mix(seed + 23 + (ulong)(i * 13)) % 100 < inSlaPercent + 4);
                    var ratings = Enumerable.Range(0, resolved).Where(i => (int)Mix(seed + 37 + (ulong)(i * 19)) % 100 < 45).ToList();
                    var bad = ratings.Count(i => (int)Mix(seed + 53 + (ulong)(i * 29)) % 100 < 9 + backlog - (a == 3 ? 5 : 0));
                    var rated = ratings.Count;
                    rows.Add(new HelpDeskArchiveDay(date, agents[a], priority, created, resolved, inSla, firstReply, rated - bad, bad));
                }
            }
        }

        return rows;
    }

    /// <summary>Comments from archived ratings, shown in the satisfaction report next to the live ones.</summary>
    public static IReadOnlyList<HelpDeskRatingComment> ArchiveComments() => new List<HelpDeskRatingComment>
    {
        new(Now.AddDays(-2).AddHours(-3), null, "Tomás Ferreira", "jordan", HelpDeskSatisfaction.Good, "Sorted in ten minutes. Thank you, Jordan!"),
        new(Now.AddDays(-3).AddHours(-5), null, "Hannah Brooks", "priya", HelpDeskSatisfaction.Good, "Priya explained the export settings really clearly."),
        new(Now.AddDays(-5).AddHours(-1), null, "Victor Lang", "marcus", HelpDeskSatisfaction.Bad, "It took three days to hear back about a broken integration."),
        new(Now.AddDays(-8).AddHours(-6), null, "Sofia Romano", "jordan", HelpDeskSatisfaction.Good, "Refund arrived the next day, great service."),
        new(Now.AddDays(-13).AddHours(-2), null, "Aaron Mills", "marcus", HelpDeskSatisfaction.Bad, "Had to explain the problem twice to two different people."),
        new(Now.AddDays(-15).AddHours(-4), null, "Leila Haddad", "priya", HelpDeskSatisfaction.Bad, "The fix worked, but the wait was long."),
        new(Now.AddDays(-21).AddHours(-7), null, "Ivan Petrov", "morgan", HelpDeskSatisfaction.Good, "Morgan set up our SSO call the same afternoon."),
    };

    /// <summary>The knowledge base. Bodies are seeded editor HTML, rendered through the same allowlist as agents' articles.</summary>
    public static List<HelpDeskArticle> Articles() => new()
    {
        new(501, "Update your billing details", "billing", "Change the card, billing address, or tax ID on your account.", Steps("billing details",
            "Open **Settings › Billing**.", "Choose **Edit** next to the card or address.", "Save. The next invoice uses the new details."), At(2026, 10, 2), "Beacon Desk team", 1840, IsPublished: true),
        new(502, "Understand your invoice", "billing", "What each line on a monthly invoice means, and when you are charged.", Steps("invoice",
            "Open **Settings › Billing › Invoices**.", "Select an invoice to see seats, add-ons, and credits.", "Download a PDF copy for your records."), At(2026, 9, 18), "Beacon Desk team", 2310, IsPublished: true),
        new(503, "Set up two-factor sign-in", "account", "Protect your account with an authenticator app.", Steps("two-factor sign-in",
            "Open **Profile › Security**.", "Scan the QR code with an authenticator app.", "Enter the six-digit code and save your recovery codes."), At(2026, 11, 3), "Beacon Desk team", 3120, IsPublished: true),
        new(504, "Reset a forgotten password", "account", "Get back into your account when you can't sign in.", Steps("password reset",
            "Select **Forgot password** on the sign-in page.", "Check your inbox and spam folder for the reset link.", "The link works once and expires after 30 minutes."), At(2026, 10, 27), "Beacon Desk team", 4580, IsPublished: true),
        new(505, "Speed up slow dashboards", "bugs", "Narrow date ranges and filters so charts load faster.", Steps("dashboard performance",
            "Choose a shorter date range, such as the last 30 days.", "Remove filters you don't need.", "Use **Refresh data** once instead of reloading the page."), At(2026, 11, 10), "Beacon Desk team", 960, IsPublished: true),
        new(506, "Connect Slack", "integrations", "Post new tickets and replies to a Slack channel.", Steps("Slack connection",
            "Open **Settings › Integrations › Slack**.", "Select **Connect** and choose a channel.", "Send a test message to confirm it works."), At(2026, 8, 30), "Beacon Desk team", 1505, IsPublished: true),
        new(507, "Verify webhook signatures", "integrations", "Check that webhook calls really come from Beacon Desk.", Steps("webhook signature",
            "Read the `X-Beacon-Signature` header.", "Compute an HMAC-SHA256 of the raw body with your signing secret.", "Compare the two values with a constant-time comparison."), At(2026, 11, 12), "Beacon Desk team", 870, IsPublished: true),
        new(508, "How we handle feature requests", "features", "Where requests go, and how we decide what to build.", Steps("feature requests",
            "Describe the problem you want solved, not only the solution.", "We group similar requests and review them monthly.", "You'll get an update on the ticket when something ships."), At(2026, 7, 21), "Beacon Desk team", 640, IsPublished: true),
    };

    /// <summary>
    /// The audit log a fresh tab starts with: the staff's most recent assignments and resolutions from the seeded tickets'
    /// activity, plus a few knowledge base and admin changes, numbered oldest first.
    /// </summary>
    public static List<ShowcaseAuditEntry> AuditEntries(IReadOnlyList<HelpDeskTicket> tickets, IReadOnlyList<HelpDeskArticle> articles, IReadOnlyList<HelpDeskAgent> agents)
    {
        var staff = agents.Select(agent => agent.Name).ToHashSet(StringComparer.Ordinal);
        var lines = new List<(DateTime At, string Actor, string Action, string Target, string? Url, string? Details)>();

        foreach (var ticket in tickets)
        {
            foreach (var activity in ticket.Activity.Where(activity => staff.Contains(activity.Actor) && activity.At <= Now))
            {
                if (activity.Text.StartsWith("Assigned to ", StringComparison.Ordinal))
                    lines.Add((activity.At, activity.Actor, "Assigned", TicketTarget(ticket.Id), TicketUrl(ticket.Id), activity.Text));
                else if (activity.Text == "Resolved the ticket")
                    lines.Add((activity.At, activity.Actor, "Resolved", TicketTarget(ticket.Id), TicketUrl(ticket.Id), null));
            }
        }

        var entries = lines.OrderByDescending(line => line.At).Take(34).ToList();

        foreach (var article in articles.OrderByDescending(article => article.UpdatedAt).Take(3))
            entries.Add((article.UpdatedAt, article.UpdatedBy, "Published", $"Article: {article.Title}", FormattableString.Invariant($"{SiteBase}/agent/kb/{article.Id}"), null));

        entries.Add((Now.AddDays(-6).AddHours(-2), AdminName, "Updated", "SLA policy: Normal", $"{SiteBase}/agent/admin/sla", "First reply 4 h, resolution 24 h"));
        entries.Add((Now.AddDays(-9).AddHours(-5), AdminName, "Updated", "Category: Integrations", $"{SiteBase}/agent/admin/categories", "Routed to Technical support"));
        entries.Add((Now.AddDays(-14).AddHours(-1), AdminName, "Added", "Saved reply: Fixed, please confirm", $"{SiteBase}/agent/saved-replies", null));

        return entries
            .OrderBy(entry => entry.At)
            .Select((entry, index) => new ShowcaseAuditEntry
            {
                Id = index + 1,
                At = entry.At,
                Actor = entry.Actor,
                Action = entry.Action,
                Target = entry.Target,
                TargetUrl = entry.Url,
                Details = entry.Details,
            })
            .ToList();
    }

    /// <summary>The demo accounts: a customer with tickets of her own, an agent, and an admin.</summary>
    public static IReadOnlyList<ShowcaseDemoAccount> DemoAccounts() => new List<ShowcaseDemoAccount>
    {
        new(SeededCustomerId, CustomerRole, "Ana Ruiz", SeededCustomerEmail, "help-demo-26", "AR"),
        new("jordan", AgentRole, AgentName, "jordan@beacondesk.example", "desk-demo-26", "JA"),
        new("morgan", AdminRole, AdminName, "morgan@beacondesk.example", "admin-demo-26", "ML"),
    };

    /// <summary>The icons an admin can give a category.</summary>
    public static IReadOnlyList<IconName> CategoryIcons() => new[]
    {
        IconName.CreditCard, IconName.PersonLock, IconName.Bug, IconName.Plug, IconName.Lightbulb, IconName.ShieldLock,
        IconName.Gear, IconName.Phone, IconName.Globe, IconName.Truck, IconName.BoxSeam, IconName.QuestionCircle,
    };

    /// <summary>
    /// The notifications a fresh tab starts with, for each staff account on the roster: its latest assignments and customer
    /// replies on its tickets, from the seeded activity. The two newest of each account are unread.
    /// </summary>
    public static List<ShowcaseNotification> Notifications(IReadOnlyList<HelpDeskTicket> tickets, IReadOnlyList<HelpDeskAgent> agents)
    {
        var items = new List<(string AccountId, DateTime At, string Title, string Text, string Url, bool IsRead)>();

        foreach (var agent in agents.Where(agent => agent.AccountId is not null))
        {
            var mine = tickets.Where(ticket => ticket.Assignee == agent.Name).ToList();
            var assigned = mine
                .SelectMany(ticket => ticket.Activity
                    .Where(activity => activity.Text == $"Assigned to {agent.Name}" && activity.At <= Now)
                    .Select(activity => (At: activity.At, Title: FormattableString.Invariant($"Ticket #{ticket.Id} was assigned to you"), Text: ticket.Subject, Url: TicketUrl(ticket.Id))));
            var replies = mine
                .Where(ticket => ticket.Status != HelpDeskStatus.Resolved && ticket.Messages.Count > 1 && ticket.Messages[^1] is { IsAgent: false } && ticket.Messages[^1].PostedAt <= Now)
                .Select(ticket => (At: ticket.Messages[^1].PostedAt, Title: FormattableString.Invariant($"{ticket.CustomerName} replied on ticket #{ticket.Id}"), Text: ticket.Subject, Url: TicketUrl(ticket.Id)));

            items.AddRange(assigned.Concat(replies)
                .OrderByDescending(item => item.At)
                .Take(5)
                .Select((item, index) => (agent.AccountId!, item.At, item.Title, item.Text, item.Url, IsRead: index >= 2)));
        }

        return items
            .OrderBy(item => item.At)
            .Select((item, index) => new ShowcaseNotification
            {
                Id = index + 1,
                AccountId = item.AccountId,
                At = item.At,
                Title = item.Title,
                Text = item.Text,
                Url = item.Url,
                IsRead = item.IsRead,
            })
            .ToList();
    }

    /// <summary>The roles of the site.</summary>
    public static IReadOnlyList<ShowcaseRole> Roles() => new List<ShowcaseRole>
    {
        new(CustomerRole, "Customer", "/showcase/help-desk/requests", "/showcase/help-desk/account", "Searches the help center, submits requests, and follows them up."),
        new(AgentRole, "Support agent", "/showcase/help-desk/agent", "/showcase/help-desk/agent/account", "Works the ticket queue, writes knowledge base articles, and reads the reports."),
        new(AdminRole, "Admin", "/showcase/help-desk/agent", "/showcase/help-desk/agent/account", "Everything an agent does, plus agents and teams, SLA policies, and categories."),
    };

    /// <summary>The SLA policy of each priority.</summary>
    public static List<HelpDeskSlaPolicy> SlaPolicies() => new()
    {
        new(HelpDeskPriority.Low, FirstReplyHours: 8, ResolveHours: 72),
        new(HelpDeskPriority.Normal, FirstReplyHours: 4, ResolveHours: 24),
        new(HelpDeskPriority.High, FirstReplyHours: 2, ResolveHours: 8),
        new(HelpDeskPriority.Urgent, FirstReplyHours: 1, ResolveHours: 4),
    };

    /// <summary>The support teams and their leads, who are notified when one of the team's tickets is escalated.</summary>
    public static List<HelpDeskTeam> Teams() => new()
    {
        new("care", "Customer care", "Billing, accounts, and feature requests.", LeadAgentId: "morgan"),
        new("tech", "Technical support", "Bugs, performance, and integrations.", LeadAgentId: "priya"),
    };

    /// <summary>The SLA escalation rules a fresh tab starts with (AC-7a).</summary>
    public static List<HelpDeskEscalationRule> EscalationRules() => new()
    {
        new(1, "Urgent not resolved in 1 hour", HelpDeskPriority.Urgent, HelpDeskEscalationTrigger.NotResolved, 1, IsActive: true),
        new(2, "High with no first reply in 2 hours", HelpDeskPriority.High, HelpDeskEscalationTrigger.NoFirstReply, 2, IsActive: true),
        new(3, "Open longer than 3 days", null, HelpDeskEscalationTrigger.NotResolved, 72, IsActive: true),
    };

    /// <summary>The agents' saved replies.</summary>
    public static List<HelpDeskSavedReply> SavedReplies() => new()
    {
        new(1, "Thanks, looking into it", "<p>Hi, thanks for getting in touch. I'm looking into this now and will update this ticket as soon as I know more.</p>"),
        new(2, "Need more details", "<p>Thanks for the report. Could you tell us:</p><ul><li>the steps that lead to the problem</li><li>what you expected to happen</li><li>a screenshot, if you can</li></ul>"),
        new(3, "Refund issued", "<p>I've issued the refund. It reaches your card within 5 to 10 business days, depending on your bank.</p>"),
        new(4, "Fixed, please confirm", "<p>We've released a fix for this. Could you try again and let us know if it works for you?</p>"),
    };

    /// <summary>The ticket and article categories.</summary>
    public static List<HelpDeskCategory> Categories() => new()
    {
        new("billing", "Billing", IconName.CreditCard, "care"),
        new("account", "Account & sign-in", IconName.PersonLock, "care"),
        new("bugs", "Troubleshooting", IconName.Bug, "tech"),
        new("integrations", "Integrations", IconName.Plug, "tech"),
        new("features", "Feature requests", IconName.Lightbulb, "care"),
    };

    /// <summary>The queue: a mix of priorities, statuses, owners, and SLA states.</summary>
    public static List<HelpDeskTicket> Tickets()
    {
        var rows = new (string Subject, string Customer, string Email, string? Company, string Category, HelpDeskPriority Priority, HelpDeskStatus Status, string? Assignee, double CreatedHoursAgo, double? ResolvedHoursAgo, string Question)[]
        {
            ("Charged twice for the November invoice", "Ana Ruiz", "ana.ruiz@example.com", "Fernwood Studio", "billing", HelpDeskPriority.High, HelpDeskStatus.Open, AgentName, 6.5, null,
                "Hi, our card was charged twice for the November invoice. Can you refund the duplicate?"),
            ("Can't sign in after enabling two-factor", "Ben Carter", "ben.carter@example.com", "Northgate Logistics", "account", HelpDeskPriority.Urgent, HelpDeskStatus.New, null, 1, null,
                "I turned on two-factor sign-in yesterday and now the code is always rejected. I'm locked out."),
            ("Export to CSV drops the last column", "Chloe Martin", "chloe.martin@example.com", "Pinecrest Analytics", "bugs", HelpDeskPriority.Normal, HelpDeskStatus.Open, "Priya Nair", 26, null,
                "When I export the report to CSV, the Notes column is missing. It shows on screen."),
            ("Slack integration stopped posting", "Dev Patel", "dev.patel@example.com", "Brightline Media", "integrations", HelpDeskPriority.High, HelpDeskStatus.Pending, "Marcus Osei", 20, null,
                "New tickets haven't appeared in our #support channel since Friday."),
            ("Request: dark mode for the mobile app", "Ella Novak", "ella.novak@example.com", null, "features", HelpDeskPriority.Low, HelpDeskStatus.Open, null, 30, null,
                "Any plans for a dark mode in the mobile app? The web app already has one."),
            ("Invoice address is wrong", "Femi Adeyemi", "femi.adeyemi@example.com", "Harbor & Co", "billing", HelpDeskPriority.Normal, HelpDeskStatus.Resolved, AgentName, 50, 30,
                "Our invoices still show our old office address. We moved last month."),
            ("Password reset email never arrives", "Grace Liu", "grace.liu@example.com", "Summit Health", "account", HelpDeskPriority.High, HelpDeskStatus.Open, AgentName, 9, null,
                "I've requested a password reset three times and nothing arrives, not even in spam."),
            ("API returns 500 on large uploads", "Hugo Silva", "hugo.silva@example.com", "Quarry Labs", "bugs", HelpDeskPriority.Urgent, HelpDeskStatus.Open, "Marcus Osei", 2.5, null,
                "Uploads over about 20 MB fail with a 500 error from the attachments endpoint."),
            ("How do I add a teammate?", "Iris Bauer", "iris.bauer@example.com", "Lumen Bakery", "account", HelpDeskPriority.Low, HelpDeskStatus.New, null, 3, null,
                "We hired someone new. How do I give them access?"),
            ("Webhook signature doesn't match", "Jonas Berg", "jonas.berg@example.com", "Fieldnote", "integrations", HelpDeskPriority.Normal, HelpDeskStatus.Open, "Priya Nair", 12, null,
                "Our server rejects your webhook calls because the signature never matches."),
            ("Downgrade plan at end of month", "Kemi Okafor", "kemi.okafor@example.com", "Clearwater Dental", "billing", HelpDeskPriority.Low, HelpDeskStatus.Pending, AgentName, 40, null,
                "Please move us to the Starter plan from next month."),
            ("Dashboard charts load slowly", "Liam Walsh", "liam.walsh@example.com", "Ridgeway Freight", "bugs", HelpDeskPriority.Normal, HelpDeskStatus.New, null, 20, null,
                "The main dashboard takes about 30 seconds to show the charts."),
            ("Zapier trigger missing new fields", "Maya Cohen", "maya.cohen@example.com", "Orchard Retail", "integrations", HelpDeskPriority.Low, HelpDeskStatus.Resolved, "Marcus Osei", 100, 20,
                "The Zapier trigger doesn't include the custom fields we added."),
            ("Add single sign-on with Okta", "Noah Kim", "noah.kim@example.com", "Vantage Legal", "features", HelpDeskPriority.Normal, HelpDeskStatus.Open, "Priya Nair", 5, null,
                "We'd like our staff to sign in with Okta. Is that on the roadmap?"),
            ("Refund for unused seats", "Olivia Grant", "olivia.grant@example.com", "Meadow Schools", "billing", HelpDeskPriority.High, HelpDeskStatus.New, null, 4, null,
                "We removed 12 seats mid-cycle. Can we get a prorated refund?"),
            ("Locked out after too many attempts", "Pablo Reyes", "pablo.reyes@example.com", "Tidewater Hotels", "account", HelpDeskPriority.Urgent, HelpDeskStatus.Resolved, AgentName, 30, 28,
                "My account is locked after too many sign-in attempts and our front desk can't work."),
            ("Typo in the welcome email", "Quinn Avery", "quinn.avery@example.com", null, "bugs", HelpDeskPriority.Low, HelpDeskStatus.Open, AgentName, 80, null,
                "Small thing: the welcome email says \"recieve\" in the second line."),
            ("Calendar sync duplicates events", "Rosa Diaz", "rosa.diaz@example.com", "Kite Travel", "integrations", HelpDeskPriority.High, HelpDeskStatus.Open, AgentName, 2, null,
                "Since this morning every event shows up twice in Google Calendar."),
            ("Add a PO number to our invoices", "Ana Ruiz", SeededCustomerEmail, "Fernwood Studio", "billing", HelpDeskPriority.Normal, HelpDeskStatus.Resolved, "Priya Nair", 75, 70,
                "Our finance team needs a purchase order number on every invoice. Can you add one?"),
        };

        // The hand-written tickets are the newest (1041 on); the generated history comes before them (FirstGeneratedTicketId down).
        var seeds = rows
            .Select((row, index) => new SeedTicket(1041 + index, row.Subject, row.Customer, row.Email, row.Company, row.Category, row.Priority, row.Status, row.Assignee,
                row.CreatedHoursAgo, row.ResolvedHoursAgo, row.Question, null, null))
            .Concat(GeneratedTickets())
            .ToList();
        var tickets = new List<HelpDeskTicket>(seeds.Count);
        var rating = new Dictionary<int, (HelpDeskSatisfaction Satisfaction, string? Comment)>
        {
            [1046] = (HelpDeskSatisfaction.Good, "Quick fix, thanks for updating the address."),
            [1053] = (HelpDeskSatisfaction.Bad, "It took four days for a missing field."),
            [1056] = (HelpDeskSatisfaction.Good, null),
        };

        foreach (var seed in seeds.Where(seed => seed.Satisfaction is not null))
            rating[seed.Id] = (seed.Satisfaction!.Value, seed.Comment);

        for (var index = 0; index < seeds.Count; index++)
        {
            var row = seeds[index];
            var created = Now.AddHours(-row.CreatedHoursAgo);
            var ticket = new HelpDeskTicket
            {
                Id = row.Id,
                Subject = row.Subject,
                CustomerName = row.Customer,
                CustomerEmail = row.Email,
                Company = row.Company,
                CategoryId = row.Category,
                Priority = row.Priority,
                Status = row.Status,
                Assignee = row.Assignee,
                CreatedAt = created,
                CustomerAccountId = row.Email == SeededCustomerEmail ? SeededCustomerId : null,
                ResolvedAt = row.ResolvedHoursAgo is { } hours ? Now.AddHours(-hours) : null,
            };

            ticket.Activity.Add(new HelpDeskActivity(created, row.Customer, "Submitted the request"));

            if (row.Assignee is { } assignee)
                ticket.Activity.Add(new HelpDeskActivity(created.AddMinutes(15), assignee, $"Assigned to {assignee}"));

            if (ticket.ResolvedAt is { } resolvedAt)
                ticket.Activity.Add(new HelpDeskActivity(resolvedAt, row.Assignee ?? AgentName, "Resolved the ticket"));

            ticket.Messages.Add(new HelpDeskMessage(row.Customer, IsAgent: false, IsInternalNote: false, IsRichText: false, row.Question, created));

            if (row.Status != HelpDeskStatus.New && row.Assignee is { } agent)
            {
                var firstName = row.Customer.Split(' ')[0];
                var replyAt = created.AddMinutes(25 + (index * 7 % 40));

                if (index % 4 == 1)
                    ticket.Messages.Add(new HelpDeskMessage(agent, IsAgent: true, IsInternalNote: true, IsRichText: true,
                        "<p>Checked the logs: this matches the issue tracked in <b>ENG-2214</b>. Waiting on the fix.</p>", replyAt.AddMinutes(-10)));

                ticket.Messages.Add(new HelpDeskMessage(agent, IsAgent: true, IsInternalNote: false, IsRichText: true,
                    $"<p>Hi {firstName}, thanks for getting in touch. I'm looking into this now.</p><ul><li>I've confirmed what you described.</li><li>I'll update this ticket as soon as I know more.</li></ul><p>Best,<br>{agent.Split(' ')[0]}</p>", replyAt));

                // The customer's follow-up comes 45 minutes after the reply, and only if that is still in the past.
                var followUpAt = replyAt.AddMinutes(45);

                if (row.Status is HelpDeskStatus.Open or HelpDeskStatus.Resolved && followUpAt < (ticket.ResolvedAt ?? Now))
                    ticket.Messages.Add(new HelpDeskMessage(row.Customer, IsAgent: false, IsInternalNote: false, IsRichText: false,
                        row.Status == HelpDeskStatus.Resolved ? "That fixed it, thank you!" : "Thanks. Any update?", followUpAt));
            }

            // A pending ticket has waited on the customer since the agent's last reply; its SLA is paused from then (T28-F1).
            if (ticket.Status == HelpDeskStatus.Pending)
                ticket.PendingSince = ticket.Messages.LastOrDefault(message => message.IsAgent && !message.IsInternalNote)?.PostedAt ?? created;

            if (rating.TryGetValue(ticket.Id, out var given))
            {
                ticket.Satisfaction = given.Satisfaction;
                ticket.SatisfactionComment = given.Comment;
                ticket.Activity.Add(new HelpDeskActivity(ticket.ResolvedAt!.Value.AddHours(1), row.Customer, $"Rated {(given.Satisfaction == HelpDeskSatisfaction.Good ? "good" : "bad")} by the customer"));
            }

            tickets.Add(ticket);
        }

        return tickets;
    }

    #endregion

    #region Private Methods

    private static DateTime At(int year, int month, int day) => new(year, month, day, 9, 0, 0);

    /// <summary>
    /// The older tickets behind the hand-written queue: one every nine hours or so, from about four and a half to ninety days
    /// ago, mostly resolved by the category's team within its SLA, a few still pending, open, or new, and about four in ten
    /// rated. Customers, companies, and subjects come from small word lists through <see cref="Mix(int)" />, so every tab
    /// gets the same history.
    /// </summary>
    private static IEnumerable<SeedTicket> GeneratedTickets()
    {
        var firstNames = new[]
        {
            "Aaron", "Bella", "Caleb", "Daria", "Elias", "Fatima", "Gavin", "Helena", "Isaac", "Jasmine", "Kai", "Lena", "Mateo", "Nadia", "Oscar",
            "Paige", "Rafael", "Sara", "Theo", "Uma", "Viktor", "Wren", "Yusuf", "Zara", "Adele", "Bruno", "Clara", "Diego", "Esme", "Felix",
        };
        var lastNames = new[]
        {
            "Abbott", "Bianchi", "Castro", "Dubois", "Eriksen", "Fischer", "Gallagher", "Hughes", "Ibrahim", "Jensen", "Kowalski", "Lindqvist", "Moreau", "Nakamura", "Obi",
            "Price", "Quintero", "Rossi", "Schmidt", "Tanaka", "Underwood", "Varga", "Weber", "Young", "Zimmer", "Alvarez", "Becker", "Chen", "Doyle", "Evans",
        };
        var companies = new[]
        {
            "Northwind Traders", "Bluebird Clinics", "Copperline Studio", "Granite Payroll", "Harborview Realty", "Ironwood Labs", "Juniper Foods", "Keystone Legal",
            "Larkspur Media", "Maple & Main", "Nimbus Software", "Oakridge Schools", "Pebble Retail", "Quill Publishing", "Redfern Logistics", "Saltmarsh Travel",
            "Thistle Dental", "Upland Energy", "Vista Hotels", "Willow Health", null, null, null, null,
        };
        var apps = new[] { "Salesforce", "HubSpot", "Slack", "Jira", "Zapier", "Microsoft Teams" };
        var reports = new[] { "SLA", "Satisfaction", "Agent activity", "Backlog" };

        // {0} is filled per ticket: the previous month for billing, a report for troubleshooting, an app for integrations.
        var topics = new (string Category, string Subject, string Question)[]
        {
            ("billing", "Card declined on renewal", "Our card was declined when the subscription renewed this morning. How can we retry the payment?"),
            ("billing", "Need a copy of the {0} invoice", "Could you send us a PDF of the {0} invoice? Our accountant needs it."),
            ("billing", "VAT number missing from invoices", "Our VAT number isn't printed on the invoices. Can you add it?"),
            ("billing", "Charged for seats we removed", "We removed three seats last week but were still charged for them."),
            ("billing", "Switch to annual billing", "We'd like to move from monthly to annual billing. What would it cost?"),
            ("billing", "Refund for an accidental upgrade", "Someone on our team upgraded the plan by mistake. Can we get a refund?"),
            ("account", "Can't change my email address", "When I change my email in the profile, the Save button does nothing."),
            ("account", "Two-factor codes rejected", "The codes from my authenticator app are always rejected."),
            ("account", "Remove a former employee's access", "A colleague left the company. How do I remove their access?"),
            ("account", "Transfer workspace ownership", "Our owner is leaving. How do we transfer ownership to me?"),
            ("account", "Sign-in link expired", "The sign-in link in the email says it has expired every time I open it."),
            ("account", "Locked out after a password change", "I changed my password and now I can't sign in on my phone."),
            ("bugs", "{0} report shows no data", "The {0} report is empty although we had tickets this month."),
            ("bugs", "Search results are out of date", "Search doesn't find the tickets I created this morning."),
            ("bugs", "Attachments fail to upload", "Uploading a PDF attachment fails with an error after a few seconds."),
            ("bugs", "Page freezes when filtering", "The page freezes for several seconds whenever I change a filter."),
            ("bugs", "Dates show in the wrong time zone", "Every date shows one hour off since the clocks changed."),
            ("bugs", "Notifications arrive twice", "We get every email notification twice since yesterday."),
            ("integrations", "{0} sync stopped", "Our {0} sync stopped yesterday and new records aren't coming through."),
            ("integrations", "{0} connection keeps expiring", "We have to reconnect {0} every few days. Is that expected?"),
            ("integrations", "Webhook retries flood our server", "Your webhook retries the same event dozens of times."),
            ("integrations", "API rate limit hit during import", "We hit the API rate limit during our nightly import."),
            ("integrations", "Map custom fields in {0}", "How do we map our custom ticket fields to {0}?"),
            ("integrations", "Duplicate contacts from {0}", "Every contact from {0} shows up twice after the last sync."),
            ("features", "Request: bulk edit for tags", "It would save us hours if we could edit the tags of many tickets at once."),
            ("features", "Request: scheduled reports by email", "Could reports be emailed to our managers every Monday?"),
            ("features", "Request: custom ticket statuses", "We'd like a status for tickets waiting on a third party."),
            ("features", "Request: Spanish help center", "Many of our customers speak Spanish. Can the help center be translated?"),
            ("features", "Request: keyboard shortcuts", "Keyboard shortcuts for assigning and resolving would help our agents."),
            ("features", "Request: export audit history", "Our auditors want the change history as a spreadsheet."),
        };
        var goodComments = new[] { "Fast and friendly, thank you!", "Clear steps, it works now.", "Solved on the first reply." };
        var badComments = new[] { "Took too long to get an answer.", "I had to explain the problem twice.", "The fix only worked for a day." };
        var policies = SlaPolicies();

        for (var i = 0; i < GeneratedTicketCount; i++)
        {
            var id = FirstGeneratedTicketId - i;
            var hoursAgo = 108 + (i * 8.9) + (Mix(id * 19) % 60 / 10.0);
            var created = Now.AddHours(-hoursAgo);

            // Billing and troubleshooting come in most often, feature requests least.
            var categoryPick = (int)(Mix(id * 3) % 100);
            var category = categoryPick switch { < 25 => "billing", < 45 => "account", < 70 => "bugs", < 88 => "integrations", _ => "features" };
            var categoryTopics = topics.Where(topic => topic.Category == category).ToArray();
            var topic = categoryTopics[(int)(Mix(id * 5) % (ulong)categoryTopics.Length)];
            var fill = category switch
            {
                "billing" => created.AddMonths(-1).ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture),
                "bugs" => reports[(int)(Mix(id * 23) % (ulong)reports.Length)],
                _ => apps[(int)(Mix(id * 23) % (ulong)apps.Length)],
            };

            // Ana Ruiz, the demo customer, has a few older requests of her own.
            var isAna = i % 40 == 7;
            var first = firstNames[(int)(Mix(id * 7) % (ulong)firstNames.Length)];
            var last = lastNames[(int)(Mix(id * 11) % (ulong)lastNames.Length)];
            var customer = isAna ? "Ana Ruiz" : $"{first} {last}";
            var email = isAna ? SeededCustomerEmail : $"{first}.{last}@example.com".ToLowerInvariant();
            var company = isAna ? "Fernwood Studio" : companies[(int)(Mix(id * 13) % (ulong)companies.Length)];

            var priorityPick = (int)(Mix(id * 17) % 100);
            var priority = category == "features"
                ? (priorityPick < 60 ? HelpDeskPriority.Low : HelpDeskPriority.Normal)
                : priorityPick switch { < 20 => HelpDeskPriority.Low, < 62 => HelpDeskPriority.Normal, < 90 => HelpDeskPriority.High, _ => HelpDeskPriority.Urgent };

            // A handful are still in the queue: pending on the customer (the last three weeks or so), open, or never picked up.
            var status = i switch
            {
                3 or 11 or 23 or 37 => HelpDeskStatus.Open,
                6 or 19 => HelpDeskStatus.New,
                < 60 when i % 6 == 2 => HelpDeskStatus.Pending,
                _ => HelpDeskStatus.Resolved,
            };

            // The category's team works the ticket: Jordan and Morgan in customer care, Priya and Marcus in technical support.
            var teamPick = (int)(Mix(id * 29) % 100);
            var careCategory = category is "billing" or "account" or "features";
            string? assignee = status == HelpDeskStatus.New || i == 37
                ? null
                : careCategory ? (teamPick < 72 ? AgentName : AdminName) : (teamPick < 52 ? "Priya Nair" : "Marcus Osei");

            // About 85% are resolved within the resolution target; the rest run over.
            double? resolvedAgo = null;
            HelpDeskSatisfaction? satisfaction = null;
            string? comment = null;

            if (status == HelpDeskStatus.Resolved)
            {
                var target = policies.First(policy => policy.Priority == priority).ResolveHours;
                var speed = (int)(Mix(id * 31) % 100);
                var share = speed < 85 ? 0.2 + (speed / 100.0 * 0.75) : 1.05 + ((speed - 85) / 20.0);
                var duration = Math.Max(1.5, target * share);
                resolvedAgo = Math.Max(2, hoursAgo - duration);

                if (Mix(id * 37) % 100 < 40)
                {
                    var inSla = hoursAgo - resolvedAgo.Value <= target;
                    var bad = (int)(Mix(id * 41) % 100) < (inSla ? 8 : 40);
                    var comments = bad ? badComments : goodComments;
                    satisfaction = bad ? HelpDeskSatisfaction.Bad : HelpDeskSatisfaction.Good;
                    comment = Mix(id * 43) % 4 == 0 ? comments[(int)(Mix(id * 47) % (ulong)comments.Length)] : null;
                }
            }

            yield return new SeedTicket(id, string.Format(System.Globalization.CultureInfo.InvariantCulture, topic.Subject, fill), customer, email, company, category,
                priority, status, assignee, hoursAgo, resolvedAgo, string.Format(System.Globalization.CultureInfo.InvariantCulture, topic.Question, fill), satisfaction, comment);
        }
    }

    private static ulong Mix(int value)
    {
        var x = (ulong)(value + 0x9E37) * 0x9E3779B97F4A7C15UL;
        x ^= x >> 29;

        return x % 1000;
    }

    private static ulong Mix(ulong value) => Mix((int)(value % 100_000));

    private static string Steps(string topic, string first, string second, string third) =>
        $"<h2>Before you start</h2><p>You need an <b>admin</b> or <b>owner</b> role to change the {topic} settings. Members can view them but not edit them.</p>"
        + $"<h2>Steps</h2><ol><li>{Inline(first)}</li><li>{Inline(second)}</li><li>{Inline(third)}</li></ol>"
        + "<blockquote>Tip: changes apply right away for everyone on your workspace.</blockquote>"
        + "<h2>Still stuck?</h2><p>Open a ticket from the help center and mention this article, so we can pick up where you left off.</p>";

    /// <summary>Turns the seed's <c>**bold**</c> and <c>`code`</c> into tags.</summary>
    private static string Inline(string text) =>
        System.Text.RegularExpressions.Regex.Replace(System.Text.RegularExpressions.Regex.Replace(text, @"\*\*(.+?)\*\*", "<b>$1</b>"), "`(.+?)`", "<code>$1</code>");

    /// <summary>The audit log's name for a ticket, like "Ticket #1047".</summary>
    private static string TicketTarget(int ticketId) => FormattableString.Invariant($"Ticket #{ticketId}");

    /// <summary>The agent desk's page of a ticket (the same link as <c>HelpDeskDisplay.TicketUrl</c>).</summary>
    private static string TicketUrl(int ticketId) => FormattableString.Invariant($"{SiteBase}/agent/tickets/{ticketId}");

    #endregion

    #endregion

    #region Nested Types

    /// <summary>One seeded ticket before it is built: the hand-written rows and the generated history share this shape.</summary>
    private sealed record SeedTicket(int Id, string Subject, string Customer, string Email, string? Company, string Category, HelpDeskPriority Priority, HelpDeskStatus Status,
        string? Assignee, double CreatedHoursAgo, double? ResolvedHoursAgo, string Question, HelpDeskSatisfaction? Satisfaction, string? Comment);

    #endregion
}
