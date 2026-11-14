using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

/// <summary>
/// Fixed, fictitious content for the "Beacon Desk" help desk: customers, companies, tickets, and articles are made up,
/// and every time is relative to a fixed "now", so each load shows the same queue and SLA states.
/// </summary>
public static class HelpDeskSeedData
{
    #region Fields and Constants

    /// <summary>The fictitious signed-in agent.</summary>
    public const string AgentName = "Jordan Ashdown";

    /// <summary>The fictitious product name.</summary>
    public const string ProductName = "Beacon Desk";

    /// <summary>The fixed "now" every time on the site is relative to.</summary>
    public static readonly DateTime Now = new(2026, 11, 24, 10, 30, 0);

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>The support agents, the signed-in agent first.</summary>
    public static IReadOnlyList<string> Agents() => new[] { AgentName, "Priya Nair", "Marcus Osei" };

    /// <summary>The knowledge base. Bodies are seeded Markdown.</summary>
    public static IReadOnlyList<HelpDeskArticle> Articles() => new List<HelpDeskArticle>
    {
        new(501, "Update your billing details", "billing", "Change the card, billing address, or tax ID on your account.", Steps("billing details",
            "Open **Settings › Billing**.", "Choose **Edit** next to the card or address.", "Save. The next invoice uses the new details."), new DateOnly(2026, 10, 2), 1840),
        new(502, "Understand your invoice", "billing", "What each line on a monthly invoice means, and when you are charged.", Steps("invoice",
            "Open **Settings › Billing › Invoices**.", "Select an invoice to see seats, add-ons, and credits.", "Download a PDF copy for your records."), new DateOnly(2026, 9, 18), 2310),
        new(503, "Set up two-factor sign-in", "account", "Protect your account with an authenticator app.", Steps("two-factor sign-in",
            "Open **Profile › Security**.", "Scan the QR code with an authenticator app.", "Enter the six-digit code and save your recovery codes."), new DateOnly(2026, 11, 3), 3120),
        new(504, "Reset a forgotten password", "account", "Get back into your account when you can't sign in.", Steps("password reset",
            "Select **Forgot password** on the sign-in page.", "Check your inbox and spam folder for the reset link.", "The link works once and expires after 30 minutes."), new DateOnly(2026, 10, 27), 4580),
        new(505, "Speed up slow dashboards", "bugs", "Narrow date ranges and filters so charts load faster.", Steps("dashboard performance",
            "Choose a shorter date range, such as the last 30 days.", "Remove filters you don't need.", "Use **Refresh data** once instead of reloading the page."), new DateOnly(2026, 11, 10), 960),
        new(506, "Connect Slack", "integrations", "Post new tickets and replies to a Slack channel.", Steps("Slack connection",
            "Open **Settings › Integrations › Slack**.", "Select **Connect** and choose a channel.", "Send a test message to confirm it works."), new DateOnly(2026, 8, 30), 1505),
        new(507, "Verify webhook signatures", "integrations", "Check that webhook calls really come from Beacon Desk.", Steps("webhook signature",
            "Read the `X-Beacon-Signature` header.", "Compute an HMAC-SHA256 of the raw body with your signing secret.", "Compare the two values with a constant-time comparison."), new DateOnly(2026, 11, 12), 870),
        new(508, "How we handle feature requests", "features", "Where requests go, and how we decide what to build.", Steps("feature requests",
            "Describe the problem you want solved, not only the solution.", "We group similar requests and review them monthly.", "You'll get an update on the ticket when something ships."), new DateOnly(2026, 7, 21), 640),
    };

    /// <summary>The ticket and article categories.</summary>
    public static IReadOnlyList<HelpDeskCategory> Categories() => new List<HelpDeskCategory>
    {
        new("billing", "Billing", IconName.CreditCard),
        new("account", "Account & sign-in", IconName.PersonLock),
        new("bugs", "Troubleshooting", IconName.Bug),
        new("integrations", "Integrations", IconName.Plug),
        new("features", "Feature requests", IconName.Lightbulb),
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
        };

        var tickets = new List<HelpDeskTicket>(rows.Length);

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
                ResolvedAt = row.ResolvedHoursAgo is { } hours ? Now.AddHours(-hours) : null,
            };

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

            tickets.Add(ticket);
        }

        return tickets;
    }

    #endregion

    #region Private Methods

    private static string Steps(string topic, string first, string second, string third) => $"""
        ## Before you start

        You need an **admin** or **owner** role to change the {topic} settings. Members can view them but not edit them.

        ## Steps

        1. {first}
        2. {second}
        3. {third}

        > Tip: changes apply right away for everyone on your workspace.

        ## Still stuck?

        Open a ticket from the help center and mention this article, so we can pick up where you left off.
        """;

    #endregion

    #endregion
}
