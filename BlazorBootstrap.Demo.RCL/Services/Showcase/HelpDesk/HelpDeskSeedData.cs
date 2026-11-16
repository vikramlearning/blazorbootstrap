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

    /// <summary>The support teams.</summary>
    public static List<HelpDeskTeam> Teams() => new()
    {
        new("care", "Customer care", "Billing, accounts, and feature requests."),
        new("tech", "Technical support", "Bugs, performance, and integrations."),
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

        var tickets = new List<HelpDeskTicket>(rows.Length);
        var rating = new Dictionary<int, (HelpDeskSatisfaction Satisfaction, string? Comment)>
        {
            [1046] = (HelpDeskSatisfaction.Good, "Quick fix, thanks for updating the address."),
            [1053] = (HelpDeskSatisfaction.Bad, "It took four days for a missing field."),
            [1056] = (HelpDeskSatisfaction.Good, null),
        };

        for (var index = 0; index < rows.Length; index++)
        {
            var row = rows[index];
            var created = Now.AddHours(-row.CreatedHoursAgo);
            var ticket = new HelpDeskTicket
            {
                Id = 1041 + index,
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

    #endregion

    #endregion
}
