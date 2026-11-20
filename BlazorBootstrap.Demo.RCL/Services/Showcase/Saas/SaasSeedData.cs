using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

/// <summary>
/// Fixed, fictitious content for Lumora, a product analytics SaaS: the public site's copy, the plans, the demo accounts,
/// and <see cref="TenantCount" /> customer workspaces created over more than two years, with their teams, invoices, history,
/// sales leads, the operator's audit log, and notifications. Every value comes from constants and index arithmetic (no
/// random numbers), so each fresh tab starts from the same data, and the tab's snapshot only needs to keep what changed
/// since (see <c>SaasState.CaptureData</c>).
/// </summary>
public static class SaasSeedData
{
    #region Fields and Constants

    /// <summary>The role of a person who uses Lumora in their company's workspace.</summary>
    public const string CustomerRole = "customer";

    /// <summary>The ID of the demo workspace the seeded customer owns.</summary>
    public const int DemoWorkspaceId = 1;

    /// <summary>The most seats a workspace can buy on a plan without a seat limit.</summary>
    public const int MaxSeats = 500;

    /// <summary>The role of a person who runs Lumora (the operator console).</summary>
    public const string OperatorRole = "operator";

    /// <summary>The ID of the seeded operator account (also an account manager on the roster).</summary>
    public const string OperatorId = "sam";

    /// <summary>The seeded operator's name.</summary>
    public const string OperatorName = "Sam Ortega";

    /// <summary>The fictitious product name.</summary>
    public const string ProductName = "Lumora";

    /// <summary>The ID of the seeded customer account.</summary>
    public const string SeededCustomerId = "amara";

    /// <summary>The name automatic billing acts under in the audit log.</summary>
    public const string SystemActorName = "Automatic billing";

    /// <summary>How many customer workspaces (tenants) a fresh tab has, the demo workspace included.</summary>
    public const int TenantCount = 260;

    /// <summary>How long a trial lasts.</summary>
    public const int TrialDays = 14;

    /// <summary>The plan a trial runs on.</summary>
    public const string TrialPlanId = "growth";

    /// <summary>The fixed "today" every date on the site is relative to.</summary>
    public static readonly DateTime Today = new(2026, 11, 9);

    /// <summary>When a fresh tab's clock starts.</summary>
    public static readonly DateTime SeedTime = Today.AddHours(9);

    /// <summary>The people at Lumora who look after tenants; Sam Ortega also signs in to the operator console.</summary>
    public static readonly IReadOnlyList<SaasAccountManager> AccountManagers = new[]
    {
        new SaasAccountManager(OperatorId, OperatorName),
        new SaasAccountManager("lena", "Lena Brooks"),
        new SaasAccountManager("kofi", "Kofi Mensah"),
        new SaasAccountManager("mia", "Mia Chen"),
    };

    /// <summary>Countries offered when a workspace is created.</summary>
    public static readonly IReadOnlyList<string> Countries = new[]
    {
        "United States", "Germany", "India", "United Kingdom", "Canada", "Australia", "Brazil", "Japan", "Netherlands", "Singapore",
    };

    /// <summary>Data regions offered in workspace settings.</summary>
    public static readonly IReadOnlyList<string> DataRegions = new[] { "United States", "European Union", "Asia Pacific" };

    /// <summary>The countries whose workspaces keep their data in the European Union.</summary>
    public static readonly IReadOnlyList<string> EuCountries = new[] { "Germany", "Netherlands" };

    /// <summary>Subdomains Lumora keeps for itself, so no workspace can take them.</summary>
    public static readonly IReadOnlyList<string> ReservedSubdomains = new[] { "admin", "api", "app", "billing", "help", "lumora", "status", "support", "www" };

    /// <summary>Team sizes offered on the contact sales form.</summary>
    public static readonly IReadOnlyList<string> TeamSizes = new[] { "1–25", "26–100", "101–500", "501–2,000", "More than 2,000" };

    /// <summary>Time zones offered in workspace settings.</summary>
    public static readonly IReadOnlyList<string> TimeZones = new[]
    {
        "UTC", "America/New_York", "America/Los_Angeles", "Europe/London", "Europe/Berlin", "Asia/Kolkata", "Asia/Singapore", "Australia/Sydney",
    };

    private static readonly string[] cardBrands = { "Visa", "Mastercard", "American Express" };

    private static readonly string[] cardLast4 = { "4242", "4444", "0005" };

    private static readonly string[] firstNames =
    {
        "Avery", "Jordan", "Riley", "Morgan", "Casey", "Quinn", "Rowan", "Skyler", "Emerson", "Finley", "Harper", "Kendall",
        "Logan", "Parker", "Reese", "Sawyer", "Tatum", "Blake", "Dakota", "Ellis", "Marlow", "Noor", "Sasha", "Wren",
    };

    private static readonly string[] lastNames =
    {
        "Ashdown", "Bellweather", "Calloway", "Dunmore", "Eastlake", "Fairbank", "Greenhollow", "Hartwell", "Ironside", "Juniper", "Kingsley", "Larkspur",
        "Merriweather", "Northcott", "Oakhurst", "Pemberton", "Quillfeather", "Ravenscroft", "Silverton", "Thornbury", "Underhill", "Vanterpool", "Whitlock", "Yarrow",
    };

    private static readonly string[] leadCompanyPrefixes =
    {
        "Atlas", "Beryl", "Cascade", "Delta", "Ember", "Fjord", "Glacier", "Harbor", "Ion", "Jade", "Keystone", "Lattice",
        "Marble", "Nimbus", "Onyx", "Prism", "Quartz", "Ridge", "Solace", "Tundra", "Union", "Vertex", "Wharf", "Yonder",
    };

    private static readonly string[] leadCompanySuffixes = { "Air", "Bank", "Retail", "Logistics", "Health", "Group", "Partners", "Systems", "Insurance", "Telecom" };

    private static readonly string[] leadMessages =
    {
        "We need SSO and an EU data region. Can we get a demo next week?",
        "Comparing analytics tools for our product team. What does Enterprise add over Growth?",
        "Our security team needs a questionnaire and a DPA before we can start a pilot.",
        "Can Lumora sync with our warehouse in real time? We have about 400 analysts.",
        "Looking for volume pricing for several business units.",
        "We'd like a call about migrating three years of event history.",
        "Do you offer a 99.99% uptime SLA and a dedicated manager?",
        "Interested in an annual contract starting in the new year.",
    };

    private static readonly string[] tenantPrefixes =
    {
        "Amberly", "Aspen", "Beacon", "Birchwood", "Bluefin", "Brassica", "Cobalt", "Cormorant", "Crescent", "Dovetail", "Elmstead", "Fernhill",
        "Firefly", "Foxglove", "Granite", "Halcyon", "Hazel", "Heron", "Indigo", "Kingfisher", "Larch", "Linden", "Maple", "Meadow",
        "Moonstone", "Northgate", "Oakridge", "Opal", "Orchard", "Pebble", "Quarry", "Redwood", "Rook", "Sable", "Seabright", "Sparrow",
        "Summit", "Thistle", "Umber", "Vale", "Willow", "Zephyr",
    };

    private static readonly string[] tenantSuffixes =
    {
        "Analytics", "Apparel", "Bank", "Bikes", "Clinics", "Coffee", "Design", "Energy", "Foods", "Freight", "Games", "Health",
        "Insurance", "Labs", "Legal", "Media", "Mobility", "Pharmacy", "Robotics", "Schools", "Software", "Studio", "Travel", "Ventures",
    };

    /// <summary>The first workspaces' names, as the public site's stories and quotes know them.</summary>
    private static readonly string[] workspaceNames =
    {
        "Northwind Outfitters", "Brightlane Health", "Orbitworks", "Fable & Finch", "Cinderpeak Labs", "Harborline Freight", "Mossgrove Studio", "Quillstone Media",
        "Tidewater Robotics", "Lanternfield Co", "Pinecrest Foods", "Violet Harbor", "Stonebridge Legal", "Skylark Travel", "Emberwick Games", "Ridgeview Energy",
        "Copperleaf Bank", "Driftwood Apparel", "Glasshouse Design", "Kestrel Mobility", "Marigold Schools", "Nettlebrook Farms", "Saltmarsh Studios", "Wildfern Outdoors",
    };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Workspace activity: when each workspace was created, a report export and a new dashboard for the ones in use, and
    /// the cancellation of a cancelled one.
    /// </summary>
    public static List<SaasActivity> Activities(IEnumerable<SaasWorkspace> workspaces)
    {
        var activities = new List<SaasActivity>();

        foreach (var workspace in workspaces)
        {
            var offset = workspace.Id % 5;

            activities.Add(new(workspace.Id, workspace.CreatedOn.AddHours(9), "Workspace created", $"Started on the {PlanName(workspace.PlanId)} plan", IconName.Flag));

            if (workspace.Status == SaasWorkspaceStatus.Archived)
            {
                activities.Add(new(workspace.Id, CancelledOn(workspace).AddHours(16), "Workspace cancelled", "Closed by the owner; Lumora keeps the record", IconName.XCircle));
                continue;
            }

            activities.Add(new(workspace.Id, Today.AddDays(-(1 + offset)).AddHours(10), "Exported a report", "Weekly usage summary (CSV)", IconName.FileEarmarkArrowDown));
            activities.Add(new(workspace.Id, Today.AddDays(-(6 + (offset * 2))).AddHours(15), "Created a dashboard", "Checkout funnel", IconName.Grid1X2));
        }

        return activities;
    }

    /// <summary>
    /// The audit log at seed time, oldest first and numbered from 1, so it agrees with the seeded data: tenants set up by
    /// sales, account managers assigned, failed renewal payments, suspensions, cancellations, refunds, the work on the seeded
    /// leads, and the demo workspace's own team changes. Each entry links to the operator's page of its record.
    /// </summary>
    public static List<ShowcaseAuditEntry> Audit(IReadOnlyList<SaasWorkspace> workspaces, IReadOnlyList<SaasMember> members, IReadOnlyList<SaasInvoice> invoices, IReadOnlyList<SaasLead> leads)
    {
        var entries = new List<(DateTime At, string Actor, string Action, string Target, string? Url, string? Details)>();
        var owners = members.Where(member => member.Role == SaasMemberRole.Owner).GroupBy(member => member.WorkspaceId).ToDictionary(group => group.Key, group => group.First());

        foreach (var workspace in workspaces)
        {
            var url = TenantUrl(workspace.Id);
            var plan = PlanName(workspace.PlanId);

            // Enterprise is sold through sales, so an operator set those tenants up.
            if (workspace.PlanId == "enterprise")
            {
                var setUpBy = ManagerName(workspace.ManagerId) ?? OperatorName;
                entries.Add((workspace.CreatedOn.AddHours(10), setUpBy, "Tenant created", workspace.Name, url,
                    string.Create(CultureInfo.InvariantCulture, $"{plan}, {workspace.Seats} seats, {PeriodLabel(workspace.Period)}, owner {owners.GetValueOrDefault(workspace.Id)?.Name ?? "none"}")));
            }

            if (workspace.ManagerId is { } managerId && workspace.CreatedOn >= Today.AddDays(-120))
                entries.Add((workspace.CreatedOn.AddDays(1).AddHours(11), OperatorName, "Account manager assigned", workspace.Name, url, $"To {ManagerName(managerId)}"));

            var open = invoices.FirstOrDefault(invoice => invoice.WorkspaceId == workspace.Id && invoice.Status == SaasInvoiceStatus.Open);

            if (open is not null && workspace.Status is SaasWorkspaceStatus.PastDue or SaasWorkspaceStatus.Suspended)
            {
                entries.Add((open.IssuedOn.AddHours(6), SystemActorName, "Payment failed", workspace.Name, url,
                    $"{Money(open.Amount)} due on {open.IssuedOn.ToString("MMM d", CultureInfo.InvariantCulture)}; no card on file. The tenant is past due."));
            }

            if (workspace.Status == SaasWorkspaceStatus.Suspended)
            {
                // A few days after the payment failed, and before the tab's clock starts.
                var suspendedOn = Min((open?.IssuedOn ?? Today.AddDays(-14)).AddDays(3 + (workspace.Id % 5)).AddHours(11), SeedTime.AddMinutes(-30));
                entries.Add((suspendedOn, OperatorName, "Tenant suspended", workspace.Name, url, "The renewal invoice is unpaid and the card on file failed."));
            }

            if (workspace.Status == SaasWorkspaceStatus.Archived)
                entries.Add((CancelledOn(workspace).AddHours(17), ManagerName(workspace.ManagerId) ?? OperatorName, "Tenant archived", workspace.Name, url, "The owner cancelled the subscription."));
        }

        foreach (var invoice in invoices.Where(invoice => invoice.RefundedAmount > 0))
        {
            entries.Add((invoice.IssuedOn.AddDays(3).AddHours(14), OperatorName, "Refund issued", InvoiceTarget(invoice.Number), InvoiceUrl(invoice.Number),
                $"{Money(invoice.RefundedAmount)}. {(invoice.Status == SaasInvoiceStatus.Refunded ? "Charged twice by mistake." : "Seats removed early in the period.")}"));
        }

        foreach (var lead in leads.Where(lead => lead.Status != SaasLeadStatus.New))
        {
            var url = string.Create(CultureInfo.InvariantCulture, $"/showcase/saas/admin/leads?lead={lead.Id}");
            entries.Add((lead.CreatedOn.AddDays(1).AddHours(2), OperatorName, "Lead updated", LeadTarget(lead), url, $"New → {lead.Status}"));
        }

        // The demo workspace's own team changes, by its people.
        var demoUrl = TenantUrl(DemoWorkspaceId);
        entries.Add((new DateTime(2026, 5, 4, 10, 0, 0), "Amara Okafor", "Seats added", "Northwind Outfitters", demoUrl, "10 → 12 seats on Growth, prorated to renewal"));
        entries.Add((new DateTime(2026, 5, 4, 10, 20, 0), "Amara Okafor", "Teammate invited", "Hana Sato (Northwind Outfitters)", demoUrl, "As a member"));
        entries.Add((new DateTime(2026, 5, 4, 10, 25, 0), "Amara Okafor", "Teammate invited", "Owen Clarke (Northwind Outfitters)", demoUrl, "As a member"));
        entries.Add((Today.AddDays(-2).AddHours(15), "Jonah Pike", "Teammate invited", "Ines Duarte (Northwind Outfitters)", demoUrl, "As a member"));

        return entries
            .Where(entry => entry.At <= SeedTime)
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

    /// <summary>Returns when a cancelled (archived) workspace was cancelled, from its ID.</summary>
    public static DateTime CancelledOn(SaasWorkspace workspace) => Max(workspace.CreatedOn.AddDays(35), Today.AddDays(-(7 + Pick(workspace.Id, 6, 180))));

    /// <summary>Customer stories.</summary>
    public static IReadOnlyList<SaasCaseStudy> CaseStudies() => new List<SaasCaseStudy>
    {
        new("Northwind Outfitters", "Retail", "Unified 14 storefront dashboards into one", "38% faster weekly reporting", IconName.Shop),
        new("Brightlane Health", "Healthcare", "Gave clinic managers live wait-time insights", "21% shorter patient waits", IconName.HeartPulse),
        new("Orbitworks", "Logistics", "Spotted route delays before customers did", "2.4× fewer late deliveries", IconName.Truck),
        new("Fable & Finch", "Publishing", "Replaced spreadsheets with self-serve funnels", "11 hours saved per editor / month", IconName.Book),
    };

    /// <summary>Plan comparison rows.</summary>
    public static IReadOnlyList<SaasComparisonRow> Comparison() => new List<SaasComparisonRow>
    {
        new("Tracked events / month", "100k", "5M", "Unlimited"),
        new("Dashboards", "5", "Unlimited", "Unlimited"),
        new("Team seats", "3", "25", "Unlimited"),
        new("Data retention", "90 days", "2 years", "Custom"),
        new("Funnels and cohorts", "Basic", "Advanced", "Advanced"),
        new("AI insights", "—", "Included", "Included"),
        new("Warehouse sync", "—", "Daily", "Real time"),
        new("Single sign-on (SAML)", "—", "—", "Included"),
        new("Audit log", "—", "30 days", "Unlimited"),
        new("Support", "Community", "Priority email", "Dedicated manager"),
        new("Uptime SLA", "—", "99.9%", "99.99%"),
    };

    /// <summary>The settings a new workspace starts with.</summary>
    public static SaasWorkspaceSettings DefaultSettings(string country) =>
        new(country == "United States" ? "America/New_York" : "UTC", "Monday", country is "Germany" or "Netherlands" or "United Kingdom" ? "European Union" : "United States",
            WeeklyDigest: true, SecurityAlerts: true, BillingEmails: true, ProductUpdates: false);

    /// <summary>The seeded demo accounts: one customer (owner of the demo workspace) and one operator.</summary>
    public static IReadOnlyList<ShowcaseDemoAccount> DemoAccounts() => new List<ShowcaseDemoAccount>
    {
        new(SeededCustomerId, CustomerRole, "Amara Okafor", "amara@northwind.example", "lumora-demo-26", "AO"),
        new(OperatorId, OperatorRole, OperatorName, "sam@lumora.example", "ops-demo-26", "SO"),
    };

    /// <summary>Frequently asked questions.</summary>
    public static IReadOnlyList<SaasFaq> Faqs() => new List<SaasFaq>
    {
        new("trial", "Billing", "Is there a free trial?", "Yes. Growth starts with a 14-day free trial, and no card is needed to start. Starter is free for as long as you like."),
        new("switch", "Billing", "Can I switch plans later?", "Upgrade or downgrade at any time from Billing. Changes are prorated to the day: you pay the difference now, or the unused part is credited to your next invoice."),
        new("annual", "Billing", "How does annual billing work?", "Annual plans are billed once a year and cost 20% less than paying monthly — roughly two months free."),
        new("cancel", "Billing", "What happens if I cancel?", "You keep access until the end of the billing period. Your data stays exportable for 30 days afterwards."),
        new("install", "Product", "How long does setup take?", "Most teams send their first events within 10 minutes using the JavaScript snippet or one of our server SDKs."),
        new("sources", "Product", "Which data sources do you support?", "Web and mobile SDKs, server-side events, and two-way sync with the most common data warehouses."),
        new("ai", "Product", "What are AI insights?", "Lumora watches your metrics and writes a short, plain-language note whenever something changes more than you would expect."),
        new("gdpr", "Security", "Is Lumora GDPR-ready?", "Yes. You choose the data region, can anonymize IP addresses, and can delete any person's data from the dashboard."),
        new("sso", "Security", "Do you support single sign-on?", "SAML single sign-on and SCIM provisioning are included in the Enterprise plan."),
        new("pii", "Security", "Do you store personal data?", "Only what you send. Field-level redaction rules let you drop sensitive properties before they are stored."),
    };

    /// <summary>Product capabilities.</summary>
    public static IReadOnlyList<SaasFeature> Features() => new List<SaasFeature>
    {
        new("dashboards", "Live dashboards", "Drag-and-drop dashboards that refresh in real time.", "Pin any chart, share a read-only link, and schedule a PDF digest for stakeholders every Monday morning.", IconName.Grid1X2, "Analyze"),
        new("funnels", "Funnels & cohorts", "See exactly where people drop off.", "Build multi-step funnels in seconds, break them down by any property, and save cohorts to reuse across reports.", IconName.Funnel, "Analyze"),
        new("ai", "AI insights", "Plain-language notes when metrics move.", "Anomaly detection runs on every metric. When something changes, you get a short explanation and the segments driving it.", IconName.Stars, "Analyze"),
        new("alerts", "Smart alerts", "Get pinged before a dip becomes a problem.", "Set thresholds or let Lumora learn normal ranges. Alerts go to email, chat, or your on-call tool.", IconName.BellFill, "Act"),
        new("experiments", "Experiments", "Ship A/B tests with confidence.", "Define goals, split traffic, and read results with built-in significance checks and guardrail metrics.", IconName.Shuffle, "Act"),
        new("sync", "Warehouse sync", "Your data, where your team already works.", "Two-way sync keeps your warehouse and Lumora in step, so analysts and product teams see the same numbers.", IconName.ArrowRepeat, "Connect"),
        new("sdks", "SDKs for every stack", "Web, mobile, and server SDKs.", "Lightweight SDKs with automatic retries, offline queues, and type-safe event definitions.", IconName.CodeSlash, "Connect"),
        new("governance", "Governance", "Roles, audit logs, and data controls.", "Granular roles, field-level redaction, and an audit log of every change keep your workspace tidy and compliant.", IconName.ShieldCheck, "Connect"),
    };

    /// <summary>Returns an invoice number, such as <c>LUM-001-0002</c>.</summary>
    public static string InvoiceNumber(int workspaceId, int sequence) => string.Create(CultureInfo.InvariantCulture, $"LUM-{workspaceId:000}-{sequence:0000}");

    /// <summary>
    /// Invoices of the paid workspaces, oldest first per workspace: every month of the last year for monthly billing, every
    /// renewal for annual billing, up to the renewal that started the current period (or the cancellation). The latest
    /// invoice of a past-due or suspended workspace is still open; a few older ones were refunded in full or in part.
    /// </summary>
    public static List<SaasInvoice> Invoices(IEnumerable<SaasWorkspace> workspaces)
    {
        var invoices = new List<SaasInvoice>();
        var plans = Plans();
        var horizon = Today.AddMonths(-12);

        foreach (var workspace in workspaces.Where(workspace => workspace.Status != SaasWorkspaceStatus.Trial))
        {
            var plan = plans.First(candidate => candidate.Id == workspace.PlanId);

            if (plan.MonthlyPricePerSeat == 0)
                continue;

            var amount = SaasPricing.PeriodAmount(plan, workspace.Period, workspace.Seats);
            var bound = workspace.Status == SaasWorkspaceStatus.Archived ? CancelledOn(workspace) : Today;
            var starts = new List<DateTime>();

            for (var start = workspace.CreatedOn.Date; start <= bound; start = SaasPricing.PeriodEnd(start, workspace.Period))
            {
                if (workspace.Period == SaasBillingPeriod.Annual || start >= horizon)
                    starts.Add(start);
            }

            var openLast = workspace.Status is SaasWorkspaceStatus.PastDue or SaasWorkspaceStatus.Suspended;
            var brand = workspace.CardBrand ?? cardBrands[workspace.Id % cardBrands.Length];
            var last4 = workspace.CardLast4 ?? cardLast4[workspace.Id % cardLast4.Length];

            for (var index = 0; index < starts.Count; index++)
            {
                var start = starts[index];
                var status = openLast && index == starts.Count - 1 ? SaasInvoiceStatus.Open : SaasInvoiceStatus.Paid;
                var refunded = 0m;

                // A few paid invoices before the latest were refunded: some in full (charged twice), some in part.
                if (status == SaasInvoiceStatus.Paid && index < starts.Count - 1 && Pick((workspace.Id * 50) + index, 15, 100) < 3)
                {
                    var full = Pick((workspace.Id * 50) + index, 16, 3) == 0;
                    refunded = full ? amount : SaasPricing.Cents(amount / 4);
                    status = full ? SaasInvoiceStatus.Refunded : SaasInvoiceStatus.Paid;
                }

                invoices.Add(new SaasInvoice(InvoiceNumber(workspace.Id, index + 1), workspace.Id, start, start, SaasPricing.PeriodEnd(start, workspace.Period),
                    $"{plan.Name} plan, {workspace.Seats} seats, {PeriodLabel(workspace.Period)}", amount, status,
                    status == SaasInvoiceStatus.Open ? null : brand, status == SaasInvoiceStatus.Open ? null : last4, refunded));
            }
        }

        // The demo workspace started on 8 seats, bought 10 at its last renewal, and added 2 more in May, prorated to the next renewal.
        var growth = plans.First(plan => plan.Id == "growth");
        var demo = invoices.Where(invoice => invoice.WorkspaceId == DemoWorkspaceId).OrderBy(invoice => invoice.IssuedOn).ToList();

        for (var index = 0; index < demo.Count; index++)
        {
            var seats = index == demo.Count - 1 ? 10 : 8;
            invoices[invoices.IndexOf(demo[index])] = demo[index] with
            {
                Description = $"Growth plan, {seats} seats, {PeriodLabel(SaasBillingPeriod.Annual)}",
                Amount = SaasPricing.PeriodAmount(growth, SaasBillingPeriod.Annual, seats),
                RefundedAmount = 0m,
                Status = SaasInvoiceStatus.Paid,
                CardBrand = "Visa",
                CardLast4 = "4242",
            };
        }

        var latest = invoices.Last(invoice => invoice.WorkspaceId == DemoWorkspaceId);
        var addedOn = new DateTime(2026, 5, 4);
        var share = SaasPricing.RemainingShare(addedOn, latest.PeriodEnd, SaasBillingPeriod.Annual);
        invoices.Add(new SaasInvoice(InvoiceNumber(DemoWorkspaceId, demo.Count + 1), DemoWorkspaceId, addedOn, addedOn, latest.PeriodEnd, "2 more seats on the Growth plan, prorated to renewal",
            SaasPricing.Cents(SaasPricing.PeriodAmount(growth, SaasBillingPeriod.Annual, 2) * share), SaasInvoiceStatus.Paid, "Visa", "4242", 0m));

        return invoices;
    }

    /// <summary>
    /// The sales inbox: three hand-written messages (Meridian Air, Coral Bank, Lumen Retail) and older ones from the last six
    /// months, the newest still new and the older ones contacted, qualified, or closed with the sales team's note.
    /// </summary>
    public static List<SaasLead> Leads()
    {
        var leads = new List<SaasLead>
        {
            new(1, "Hiro Tanaka", "hiro.tanaka@meridianair.example", "Meridian Air", "501–2,000", "We need SSO and an EU data region for 900 analysts. Can we talk this week?", Today.AddDays(-1).AddHours(14), SaasLeadStatus.New),
            new(2, "Grace Mbeki", "grace@coralbank.example", "Coral Bank", "101–500", "Looking at Enterprise for a security review in December.", Today.AddDays(-4).AddHours(11), SaasLeadStatus.Contacted, "Called Nov 6. Security questionnaire sent; follow up on Nov 16."),
            new(3, "Tomás Ruiz", "tomas@lumenretail.example", "Lumen Retail", "26–100", "Is real-time warehouse sync available on a smaller plan?", Today.AddDays(-9).AddHours(16), SaasLeadStatus.Closed, "Not a fit: they need real-time sync on 30 seats. Pointed them to Growth with daily sync."),
        };
        var companies = new HashSet<string>(leads.Select(lead => lead.Company), StringComparer.OrdinalIgnoreCase);

        for (var index = 0; leads.Count < 64; index++)
        {
            var company = $"{leadCompanyPrefixes[index % leadCompanyPrefixes.Length]} {leadCompanySuffixes[((index * 3) + (index / leadCompanyPrefixes.Length)) % leadCompanySuffixes.Length]}";

            if (!companies.Add(company))
                continue;

            var first = firstNames[Pick(index, 21, firstNames.Length)];
            var last = lastNames[Pick(index, 22, lastNames.Length)];
            var daysAgo = 2 + (index * 3) + Pick(index, 23, 3);
            var status = daysAgo < 12 ? SaasLeadStatus.New : (Pick(index, 24, 10)) switch
            {
                < 4 => SaasLeadStatus.Contacted,
                < 7 => SaasLeadStatus.Qualified,
                _ => SaasLeadStatus.Closed,
            };
            var note = status switch
            {
                SaasLeadStatus.Contacted => $"Intro call held. {first} is checking the budget with their finance team.",
                SaasLeadStatus.Qualified => $"Budget confirmed for Q1. Proposal for {TeamSizes[Pick(index, 25, TeamSizes.Count)]} people sent.",
                SaasLeadStatus.Closed => Pick(index, 26, 2) == 0 ? "Lost: they chose a tool they already use elsewhere." : "Not a fit right now; asked us to check in next year.",
                _ => null,
            };

            leads.Add(new SaasLead(leads.Count + 1, $"{first} {last}", $"{first}.{last}@{Slug(company)}.example".ToLowerInvariant(), company, TeamSizes[Pick(index, 25, TeamSizes.Count)],
                leadMessages[Pick(index, 27, leadMessages.Length)], Today.AddDays(-daysAgo).AddHours(9 + Pick(index, 28, 8)), status, note));
        }

        return leads;
    }

    /// <summary>Returns the name of an account manager on the roster, or <see langword="null" />.</summary>
    public static string? ManagerName(string? managerId) => AccountManagers.FirstOrDefault(manager => manager.Id == managerId)?.Name;

    /// <summary>Members of every workspace: the owner first, then teammates; the demo workspace's team is spelled out.</summary>
    public static List<SaasMember> Members(IEnumerable<SaasWorkspace> workspaces)
    {
        var members = new List<SaasMember>();

        void Add(int workspaceId, string? accountId, string name, string email, SaasMemberRole role, SaasMemberStatus status, DateTime addedOn, DateTime? lastActiveOn) =>
            members.Add(new SaasMember(members.Count + 1, workspaceId, accountId, name, email, role, status, addedOn, lastActiveOn));

        foreach (var workspace in workspaces)
        {
            if (workspace.Id == DemoWorkspaceId)
            {
                var created = workspace.CreatedOn;
                Add(workspace.Id, SeededCustomerId, "Amara Okafor", "amara@northwind.example", SaasMemberRole.Owner, SaasMemberStatus.Active, created, Today.AddHours(8).AddMinutes(40));
                Add(workspace.Id, null, "Jonah Pike", "jonah.pike@northwind.example", SaasMemberRole.Admin, SaasMemberStatus.Active, created.AddDays(2), Today.AddDays(-1).AddHours(17));
                Add(workspace.Id, null, "Lucia Moreno", "lucia.moreno@northwind.example", SaasMemberRole.Billing, SaasMemberStatus.Active, created.AddDays(9), Today.AddDays(-6).AddHours(10));
                Add(workspace.Id, null, "Theo Lindgren", "theo.lindgren@northwind.example", SaasMemberRole.Member, SaasMemberStatus.Active, created.AddDays(15), Today.AddHours(7).AddMinutes(55));
                Add(workspace.Id, null, "Priya Shah", "priya.shah@northwind.example", SaasMemberRole.Member, SaasMemberStatus.Active, created.AddDays(40), Today.AddDays(-2).AddHours(13));
                Add(workspace.Id, null, "Marcus Bell", "marcus.bell@northwind.example", SaasMemberRole.Member, SaasMemberStatus.Active, created.AddDays(70), Today.AddDays(-3).AddHours(9));
                Add(workspace.Id, null, "Hana Sato", "hana.sato@northwind.example", SaasMemberRole.Member, SaasMemberStatus.Active, new DateTime(2026, 5, 4), Today.AddDays(-1).AddHours(11));
                Add(workspace.Id, null, "Owen Clarke", "owen.clarke@northwind.example", SaasMemberRole.Member, SaasMemberStatus.Active, new DateTime(2026, 5, 4), Today.AddDays(-12).AddHours(16));
                Add(workspace.Id, null, "Ines Duarte", "ines.duarte@northwind.example", SaasMemberRole.Member, SaasMemberStatus.Invited, Today.AddDays(-2).AddHours(15), null);
                continue;
            }

            var index = workspace.Id;
            var domain = $"{workspace.Subdomain.Replace("-", string.Empty, StringComparison.Ordinal)}.example";
            var wanted = workspace.Status == SaasWorkspaceStatus.Trial ? 1 + Pick(index, 11, 4)
                : workspace.PlanId switch
                {
                    "starter" => 1 + Pick(index, 11, 3),
                    "enterprise" => 6 + Pick(index, 11, 15),
                    _ => 2 + Pick(index, 11, 8),
                };
            var count = Math.Min(workspace.Seats, wanted);
            var firstOffset = Pick(index, 12, firstNames.Length);
            var lastOffset = Pick(index, 13, lastNames.Length);
            var activeUntil = workspace.Status == SaasWorkspaceStatus.Archived ? CancelledOn(workspace) : Today;

            for (var position = 0; position < count; position++)
            {
                // Steps of 5 and 7 through 24 names never repeat within a team, so each email is unique.
                var first = firstNames[(firstOffset + (position * 5)) % firstNames.Length];
                var last = lastNames[(lastOffset + (position * 7)) % lastNames.Length];
                var role = position switch
                {
                    0 => SaasMemberRole.Owner,
                    1 => SaasMemberRole.Admin,
                    2 when index % 2 == 0 => SaasMemberRole.Billing,
                    _ => SaasMemberRole.Member,
                };
                var invited = position == count - 1 && position > 1 && workspace.Status != SaasWorkspaceStatus.Archived && Pick(index, 14, 3) == 0;
                var addedOn = Min(workspace.CreatedOn.AddDays(position * 3), activeUntil);

                Add(workspace.Id, null, $"{first} {last}", $"{first}.{last}@{domain}".ToLowerInvariant(), role,
                    invited ? SaasMemberStatus.Invited : SaasMemberStatus.Active, addedOn,
                    invited ? null : Max(addedOn, activeUntil.AddDays(-((index + position) % 9))).AddHours(9 + (position % 8)));
            }
        }

        return members;
    }

    /// <summary>
    /// The seeded notifications: the customer's (an upcoming renewal, usage, an invitation, a product update) and the
    /// operator's (new leads and sign-ups, overdue invoices, a tenant out of seats), the two newest of each unread.
    /// </summary>
    public static List<SaasNotice> Notices(IReadOnlyList<SaasWorkspace> workspaces, IReadOnlyList<SaasMember> members, IReadOnlyList<SaasInvoice> invoices)
    {
        var notices = new List<SaasNotice>
        {
            new(1, SeededCustomerId, "Your plan renews on Nov 14", "Growth, 12 seats, billed annually, to the Visa ending in 4242.", "/showcase/saas/app/billing", IconName.CalendarCheck, Today.AddHours(8), false),
            new(2, SeededCustomerId, "Events at 72% of your plan", "3.6M of 5M events tracked this month.", "/showcase/saas/app", IconName.GraphUpArrow, Today.AddDays(-1).AddHours(12), false),
            new(3, SeededCustomerId, "Jonah Pike invited Ines Duarte", "Ines joins as a member when she accepts. The invitation holds a seat until then.", "/showcase/saas/app/team", IconName.PersonPlus, Today.AddDays(-2).AddHours(15), true),
            new(4, SeededCustomerId, "Warehouse sync runs hourly now", "Hourly sync is included in Growth and Enterprise.", null, IconName.ArrowRepeat, Today.AddDays(-5).AddHours(11), true),
        };
        var operatorNotices = new List<(string Title, string Text, string Link, IconName Icon, DateTime At)>
        {
            ("New lead from Meridian Air", "Hiro Tanaka, 501–2,000 people: SSO and an EU data region for 900 analysts.", "/showcase/saas/admin/leads?lead=1", IconName.Inbox, Today.AddDays(-1).AddHours(14).AddMinutes(1)),
            ("New lead from Coral Bank", "Grace Mbeki, 101–500 people: a security review in December.", "/showcase/saas/admin/leads?lead=2", IconName.Inbox, Today.AddDays(-4).AddHours(11).AddMinutes(1)),
        };

        // The newest trials signed up themselves.
        foreach (var trial in workspaces.Where(workspace => workspace.Status == SaasWorkspaceStatus.Trial).OrderByDescending(workspace => workspace.CreatedOn).ThenBy(workspace => workspace.Id).Take(2))
        {
            operatorNotices.Add(($"New sign-up: {trial.Name}", $"{PlanName(trial.PlanId)} trial until {trial.TrialEndsOn!.Value.ToString("MMM d", CultureInfo.InvariantCulture)}, {trial.Country}.",
                TenantUrl(trial.Id), IconName.PersonPlus, trial.CreatedOn.AddHours(10)));
        }

        var pastDue = workspaces.Where(workspace => workspace.Status == SaasWorkspaceStatus.PastDue).Select(workspace => workspace.Id).ToHashSet();

        foreach (var open in invoices.Where(invoice => invoice.Status == SaasInvoiceStatus.Open && pastDue.Contains(invoice.WorkspaceId)).OrderByDescending(invoice => invoice.IssuedOn).Take(2))
        {
            var name = workspaces.First(workspace => workspace.Id == open.WorkspaceId).Name;
            operatorNotices.Add(($"Invoice {open.Number} is overdue", $"{name}: {Money(open.Amount)} with no card on file. The tenant is past due.", InvoiceUrl(open.Number),
                IconName.ExclamationTriangle, open.IssuedOn.AddHours(6)));
        }

        // A paying tenant whose members and invitations fill every seat: a chance to sell more.
        var seatCounts = members.GroupBy(member => member.WorkspaceId).ToDictionary(group => group.Key, group => group.Count());

        if (workspaces.FirstOrDefault(workspace => workspace.PlanId == "growth" && workspace.Status == SaasWorkspaceStatus.Active && seatCounts.GetValueOrDefault(workspace.Id) == workspace.Seats) is { } full)
        {
            operatorNotices.Add(($"{full.Name} uses all its seats", string.Create(CultureInfo.InvariantCulture, $"{full.Seats} of {full.Seats} seats on Growth are taken by members and invitations."),
                TenantUrl(full.Id), IconName.People, Today.AddDays(-3).AddHours(16)));
        }

        // The two newest are unread, the rest read.
        var ordered = operatorNotices.Where(notice => notice.At <= SeedTime).OrderByDescending(notice => notice.At).ToList();

        for (var index = 0; index < ordered.Count; index++)
        {
            var notice = ordered[index];
            notices.Add(new SaasNotice(notices.Count + 1, OperatorId, notice.Title, notice.Text, notice.Link, notice.Icon, notice.At, IsRead: index >= 2));
        }

        return notices;
    }

    /// <summary>Returns "billed monthly" or "billed annually".</summary>
    public static string PeriodLabel(SaasBillingPeriod period) => period == SaasBillingPeriod.Annual ? "billed annually" : "billed monthly";

    /// <summary>The plans. The operator can edit them; the pricing page, sign-up, and billing read them from state.</summary>
    public static List<SaasPlan> Plans() => new()
    {
        new("starter", "Starter", "For side projects and small teams getting started.", 0m, false,
            new[] { "100k events / month", "5 dashboards", "3 team seats", "90-day history" }, "Start for free", 3, 100_000, 5, true),
        new("growth", "Growth", "For product teams that make decisions with data every day.", 49m, true,
            new[] { "5M events / month", "Unlimited dashboards", "AI insights", "Warehouse sync", "Priority support" }, "Start 14-day trial", 25, 5_000_000, 0, true),
        new("enterprise", "Enterprise", "For organizations with advanced security and scale needs.", 199m, false,
            new[] { "Unlimited events", "SAML SSO and SCIM", "Real-time warehouse sync", "99.99% uptime SLA", "Dedicated manager" }, "Talk to sales", 0, 0, 0, false),
    };

    /// <summary>The roles of the site.</summary>
    public static IReadOnlyList<ShowcaseRole> Roles() => new List<ShowcaseRole>
    {
        new(CustomerRole, "Customer", "/showcase/saas/app", "/showcase/saas/app/account", "Uses Lumora in their company's workspace: usage, team, billing, and settings."),
        new(OperatorRole, "Operator", "/showcase/saas/admin", "/showcase/saas/admin/account", "Runs Lumora: tenants, revenue, and analytics."),
    };

    /// <summary>
    /// Returns the subdomain made from a workspace name: lower case, every run of other characters a hyphen, at most 30
    /// characters, such as <c>fable-finch</c> for "Fable &amp; Finch".
    /// </summary>
    public static string SubdomainFrom(string name)
    {
        var builder = new System.Text.StringBuilder();

        foreach (var character in (name ?? string.Empty).Trim().ToLowerInvariant())
        {
            if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
                builder.Append(character);
            else if (builder.Length > 0 && builder[^1] != '-')
                builder.Append('-');
        }

        var slug = builder.ToString().Trim('-');

        return slug.Length > 30 ? slug[..30].TrimEnd('-') : slug;
    }

    /// <summary>Customer quotes.</summary>
    public static IReadOnlyList<SaasTestimonial> Testimonials() => new List<SaasTestimonial>
    {
        new("We replaced three tools with Lumora in a week. Our Monday metrics meeting went from forty minutes to ten.", "Amara Okafor", "Head of Product", "Northwind Outfitters", 5, "75%", "shorter metrics meetings"),
        new("The AI insights caught a checkout bug on a Saturday morning before a single customer complained.", "Daniel Reyes", "Engineering Manager", "Orbitworks", 5, "2.4×", "fewer late deliveries"),
        new("Our clinic managers finally have numbers they trust, and they don't need an analyst to get them.", "Priya Natarajan", "Director of Operations", "Brightlane Health", 5, "21%", "shorter patient waits"),
        new("Funnels that used to take a sprint now take a coffee break. Our editors build their own reports.", "Lena Hoffmann", "Growth Lead", "Fable & Finch", 4, "11 h", "saved per editor each month"),
    };

    /// <summary>
    /// The <see cref="TenantCount" /> customer workspaces: the demo workspace first, then a mix created over the last two and
    /// a half years: about three in five active, and the rest on trial, past due, suspended, or cancelled (archived); a
    /// quarter on Starter, most on Growth, and about one in seven on Enterprise, set up by sales with an account manager.
    /// </summary>
    public static List<SaasWorkspace> Workspaces()
    {
        var workspaces = new List<SaasWorkspace>
        {
            new(DemoWorkspaceId, workspaceNames[0], "growth", SaasBillingPeriod.Annual, 12, SaasWorkspaceStatus.Active, new DateTime(2024, 11, 14), null,
                new DateTime(2026, 11, 14), "United States", 3_620_000, 18, 0m, "Visa", "4242", 8, 2028,
                new SaasWorkspaceSettings("America/New_York", "Monday", "United States", WeeklyDigest: true, SecurityAlerts: true, BillingEmails: true, ProductUpdates: false))
            {
                Subdomain = SubdomainFrom(workspaceNames[0]),
                ManagerId = OperatorId,
            },
        };
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { workspaceNames[0] };
        var subdomains = new HashSet<string>(StringComparer.Ordinal) { SubdomainFrom(workspaceNames[0]) };
        var candidate = 0;

        for (var index = 1; index < TenantCount; index++)
        {
            string name;

            do
            {
                name = candidate < workspaceNames.Length
                    ? workspaceNames[candidate]
                    : $"{tenantPrefixes[candidate % tenantPrefixes.Length]} {tenantSuffixes[((candidate * 7) + (candidate / tenantPrefixes.Length)) % tenantSuffixes.Length]}";
                candidate++;
            }
            while (!names.Add(name) || !subdomains.Add(SubdomainFrom(name)));

            workspaces.Add(Generated(index, name));
        }

        return workspaces;
    }

    #endregion

    #region Private Methods

    /// <summary>One generated workspace (any but the demo workspace); <paramref name="index" /> is its ID less one.</summary>
    private static SaasWorkspace Generated(int index, string name)
    {
        var id = index + 1;
        var country = Countries[Pick(index, 7, Countries.Count)];
        var lifecycle = Pick(index, 1, 100);
        var status = lifecycle switch
        {
            < 8 => SaasWorkspaceStatus.Trial,
            < 13 => SaasWorkspaceStatus.PastDue,
            < 17 => SaasWorkspaceStatus.Suspended,
            < 26 => SaasWorkspaceStatus.Archived,
            _ => SaasWorkspaceStatus.Active,
        };
        var mix = Pick(index, 2, 100);
        var planId = status == SaasWorkspaceStatus.Trial ? TrialPlanId
            : mix < 25 && status is SaasWorkspaceStatus.Active or SaasWorkspaceStatus.Archived ? "starter"
            : mix < 86 ? "growth"
            : "enterprise";
        var period = planId == "starter" || status == SaasWorkspaceStatus.Trial || Pick(index, 3, 100) >= 35 ? SaasBillingPeriod.Monthly : SaasBillingPeriod.Annual;
        var seats = status == SaasWorkspaceStatus.Trial ? 5
            : planId switch
            {
                "starter" => 1 + Pick(index, 4, 3),
                "growth" => 3 + Pick(index, 4, 23),
                _ => 30 + Pick(index, 4, 271),
            };

        DateTime createdOn;
        DateTime? trialEndsOn = null;
        DateTime renewsOn;

        if (status == SaasWorkspaceStatus.Trial)
        {
            trialEndsOn = Today.AddDays(1 + Pick(index, 5, 13));
            createdOn = trialEndsOn.Value.AddDays(-TrialDays);
            renewsOn = trialEndsOn.Value;
        }
        else
        {
            createdOn = Today.AddDays(-(40 + Pick(index, 5, 860)));
            renewsOn = NextRenewal(createdOn, period);
        }

        var limit = planId switch { "starter" => 100_000L, "growth" => 5_000_000L, _ => 20_000_000L };
        var events = status == SaasWorkspaceStatus.Archived ? 0 : limit * Pick(index, 8, 96) / 100;
        var hasCard = planId != "starter" && status switch
        {
            SaasWorkspaceStatus.Trial => Pick(index, 9, 2) == 0,
            SaasWorkspaceStatus.PastDue or SaasWorkspaceStatus.Suspended => false,
            _ => true,
        };
        var card = Pick(index, 10, cardBrands.Length);
        var managerId = planId switch
        {
            "enterprise" => AccountManagers[Pick(index, 17, AccountManagers.Count)].Id,
            "growth" when Pick(index, 18, 5) < 3 => AccountManagers[Pick(index, 17, AccountManagers.Count)].Id,
            _ => null,
        };

        return new SaasWorkspace(id, name, planId, period, seats, status, createdOn, trialEndsOn, renewsOn, country, events,
            planId == "starter" ? 1 + Pick(index, 19, 5) : 2 + Pick(index, 19, 39), 0m,
            hasCard ? cardBrands[card] : null, hasCard ? cardLast4[card] : null,
            hasCard ? 1 + Pick(index, 20, 12) : 0, hasCard ? 2027 + Pick(index, 20, 3) : 0, DefaultSettings(country))
        {
            Subdomain = SubdomainFrom(name),
            ManagerId = managerId,
        };
    }

    private static string InvoiceTarget(string number) => $"Invoice {number}";

    private static string InvoiceUrl(string number) => $"/showcase/saas/admin/invoices/{number}";

    private static string LeadTarget(SaasLead lead) => $"Lead: {lead.Company}";

    private static DateTime Max(DateTime first, DateTime second) => first >= second ? first : second;

    private static DateTime Min(DateTime first, DateTime second) => first <= second ? first : second;

    private static string Money(decimal amount) => amount.ToString("$#,0.00", CultureInfo.InvariantCulture);

    private static DateTime NextRenewal(DateTime createdOn, SaasBillingPeriod period)
    {
        var renewal = createdOn.Date;

        while (renewal <= Today)
            renewal = SaasPricing.PeriodEnd(renewal, period);

        return renewal;
    }

    /// <summary>
    /// A fixed pseudo-random number from 0 to <paramref name="modulo" /> less one for an index and a salt (a hash, so the seed
    /// is varied but the same in every tab).
    /// </summary>
    private static int Pick(int index, int salt, int modulo)
    {
        var value = unchecked(((uint)index * 2654435761u) + ((uint)salt * 2246822519u));
        value ^= value >> 15;
        value = unchecked(value * 2246822519u);
        value ^= value >> 13;

        return (int)(value % (uint)modulo);
    }

    private static string PlanName(string planId) => Plans().First(plan => plan.Id == planId).Name;

    private static string Slug(string name) => new(name.Where(char.IsLetter).ToArray());

    private static string TenantUrl(int workspaceId) => string.Create(CultureInfo.InvariantCulture, $"/showcase/saas/admin/tenants/{workspaceId}");

    #endregion

    #endregion
}
