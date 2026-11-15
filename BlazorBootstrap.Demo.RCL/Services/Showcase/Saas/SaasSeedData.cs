using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

/// <summary>
/// Fixed, fictitious content for Lumora, a product analytics SaaS: the public site's copy, the plans, the demo accounts,
/// and 24 customer workspaces with their teams, invoices, and history. Every value comes from constants and index
/// arithmetic, so each fresh tab starts from the same data.
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

    /// <summary>The fictitious product name.</summary>
    public const string ProductName = "Lumora";

    /// <summary>The ID of the seeded customer account.</summary>
    public const string SeededCustomerId = "amara";

    /// <summary>How long a trial lasts.</summary>
    public const int TrialDays = 14;

    /// <summary>The plan a trial runs on.</summary>
    public const string TrialPlanId = "growth";

    /// <summary>The fixed "today" every date on the site is relative to.</summary>
    public static readonly DateTime Today = new(2026, 11, 9);

    /// <summary>When a fresh tab's clock starts.</summary>
    public static readonly DateTime SeedTime = Today.AddHours(9);

    /// <summary>Countries offered when a workspace is created.</summary>
    public static readonly IReadOnlyList<string> Countries = new[]
    {
        "United States", "Germany", "India", "United Kingdom", "Canada", "Australia", "Brazil", "Japan", "Netherlands", "Singapore",
    };

    /// <summary>Data regions offered in workspace settings.</summary>
    public static readonly IReadOnlyList<string> DataRegions = new[] { "United States", "European Union", "Asia Pacific" };

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

    private static readonly string[] workspaceNames =
    {
        "Northwind Outfitters", "Brightlane Health", "Orbitworks", "Fable & Finch", "Cinderpeak Labs", "Harborline Freight", "Mossgrove Studio", "Quillstone Media",
        "Tidewater Robotics", "Lanternfield Co", "Pinecrest Foods", "Violet Harbor", "Stonebridge Legal", "Skylark Travel", "Emberwick Games", "Ridgeview Energy",
        "Copperleaf Bank", "Driftwood Apparel", "Glasshouse Design", "Kestrel Mobility", "Marigold Schools", "Nettlebrook Farms", "Saltmarsh Studios", "Wildfern Outdoors",
    };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Workspace activity: a few entries for each workspace.</summary>
    public static List<SaasActivity> Activities(IEnumerable<SaasWorkspace> workspaces)
    {
        var activities = new List<SaasActivity>();

        foreach (var workspace in workspaces)
        {
            var offset = workspace.Id % 5;

            activities.Add(new(workspace.Id, Today.AddDays(-(1 + offset)).AddHours(10), "Exported a report", "Weekly usage summary (CSV)", IconName.FileEarmarkArrowDown));
            activities.Add(new(workspace.Id, Today.AddDays(-(6 + (offset * 2))).AddHours(15), "Created a dashboard", "Checkout funnel", IconName.Grid1X2));
            activities.Add(new(workspace.Id, workspace.CreatedOn.AddHours(9), "Workspace created", $"Started on the {PlanName(workspace.PlanId)} plan", IconName.Flag));
        }

        return activities;
    }

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
        new("sam", OperatorRole, "Sam Ortega", "sam@lumora.example", "ops-demo-26", "SO"),
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

    /// <summary>Invoices of the paid workspaces: the last three months for monthly billing, the last renewal for annual billing.</summary>
    public static List<SaasInvoice> Invoices(IEnumerable<SaasWorkspace> workspaces)
    {
        var invoices = new List<SaasInvoice>();
        var plans = Plans();

        foreach (var workspace in workspaces.Where(workspace => workspace.Status != SaasWorkspaceStatus.Trial))
        {
            var plan = plans.First(candidate => candidate.Id == workspace.PlanId);

            if (plan.MonthlyPricePerSeat == 0)
                continue;

            var amount = SaasPricing.PeriodAmount(plan, workspace.Period, workspace.Seats);
            var periods = workspace.Period == SaasBillingPeriod.Annual ? 1 : 3;

            for (var back = periods; back >= 1; back--)
            {
                var start = workspace.Period == SaasBillingPeriod.Annual ? workspace.RenewsOn.AddYears(-back) : workspace.RenewsOn.AddMonths(-back);

                if (start < workspace.CreatedOn.Date)
                    continue;

                // A suspended workspace stopped paying: its latest invoice is still open.
                var status = workspace.Status == SaasWorkspaceStatus.Suspended && back == 1 ? SaasInvoiceStatus.Open : SaasInvoiceStatus.Paid;
                var sequence = invoices.Count(item => item.WorkspaceId == workspace.Id) + 1;
                invoices.Add(new SaasInvoice(InvoiceNumber(workspace.Id, sequence), workspace.Id, start, start, SaasPricing.PeriodEnd(start, workspace.Period),
                    $"{plan.Name} plan, {workspace.Seats} seats, {PeriodLabel(workspace.Period)}", amount, status,
                    status == SaasInvoiceStatus.Paid ? workspace.CardBrand : null, status == SaasInvoiceStatus.Paid ? workspace.CardLast4 : null, 0m));
            }
        }

        // The demo workspace bought 10 seats at its last renewal and added 2 more in May, prorated to the next renewal.
        var demoIndex = invoices.FindIndex(invoice => invoice.WorkspaceId == DemoWorkspaceId);
        var growth = plans.First(plan => plan.Id == "growth");
        var demo = invoices[demoIndex] with
        {
            Description = $"Growth plan, 10 seats, {PeriodLabel(SaasBillingPeriod.Annual)}",
            Amount = SaasPricing.PeriodAmount(growth, SaasBillingPeriod.Annual, 10),
        };
        invoices[demoIndex] = demo;

        var addedOn = new DateTime(2026, 5, 4);
        var share = SaasPricing.RemainingShare(addedOn, demo.PeriodEnd, SaasBillingPeriod.Annual);
        invoices.Add(new SaasInvoice(InvoiceNumber(DemoWorkspaceId, 2), DemoWorkspaceId, addedOn, addedOn, demo.PeriodEnd, "2 more seats on the Growth plan, prorated to renewal",
            SaasPricing.Cents(SaasPricing.PeriodAmount(growth, SaasBillingPeriod.Annual, 2) * share), SaasInvoiceStatus.Paid, "Visa", "4242", 0m));

        return invoices;
    }

    /// <summary>Messages already in the sales inbox.</summary>
    public static List<SaasLead> Leads() => new()
    {
        new(1, "Hiro Tanaka", "hiro.tanaka@meridianair.example", "Meridian Air", "501–2,000", "We need SSO and an EU data region for 900 analysts. Can we talk this week?", Today.AddDays(-1).AddHours(14), SaasLeadStatus.New),
        new(2, "Grace Mbeki", "grace@coralbank.example", "Coral Bank", "101–500", "Looking at Enterprise for a security review in December.", Today.AddDays(-4).AddHours(11), SaasLeadStatus.Contacted),
        new(3, "Tomás Ruiz", "tomas@lumenretail.example", "Lumen Retail", "26–100", "Is real-time warehouse sync available on a smaller plan?", Today.AddDays(-9).AddHours(16), SaasLeadStatus.Closed),
    };

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
            var domain = $"{Slug(workspace.Name)}.example".ToLowerInvariant();
            var count = Math.Min(workspace.Seats, 2 + (index % 5));

            for (var position = 0; position < count; position++)
            {
                var first = firstNames[((index * 5) + (position * 7)) % firstNames.Length];
                var last = lastNames[((index * 3) + (position * 11) + 2) % lastNames.Length];
                var role = position switch
                {
                    0 => SaasMemberRole.Owner,
                    1 => SaasMemberRole.Admin,
                    2 when index % 2 == 0 => SaasMemberRole.Billing,
                    _ => SaasMemberRole.Member,
                };
                var invited = position == count - 1 && position > 1 && index % 3 == 0;
                Add(workspace.Id, null, $"{first} {last}", $"{first}.{last}@{domain}".ToLowerInvariant(), role,
                    invited ? SaasMemberStatus.Invited : SaasMemberStatus.Active, workspace.CreatedOn.AddDays(position * 3),
                    invited ? null : Today.AddDays(-((index + position) % 9)).AddHours(9 + (position % 8)));
            }
        }

        return members;
    }

    /// <summary>The seeded customer's notifications.</summary>
    public static List<SaasNotice> Notices() => new()
    {
        new(1, SeededCustomerId, "Your plan renews on Nov 14", "Growth, 12 seats, billed annually, to the Visa ending in 4242.", "/showcase/saas/app/billing", IconName.CalendarCheck, Today.AddHours(8), false),
        new(2, SeededCustomerId, "Events at 72% of your plan", "3.6M of 5M events tracked this month.", "/showcase/saas/app", IconName.GraphUpArrow, Today.AddDays(-1).AddHours(12), false),
        new(3, SeededCustomerId, "Jonah Pike invited Ines Duarte", "Ines joins as a member when she accepts. The invitation holds a seat until then.", "/showcase/saas/app/team", IconName.PersonPlus, Today.AddDays(-2).AddHours(15), true),
        new(4, SeededCustomerId, "Warehouse sync runs hourly now", "Hourly sync is included in Growth and Enterprise.", null, IconName.ArrowRepeat, Today.AddDays(-5).AddHours(11), true),
    };

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

    /// <summary>Customer quotes.</summary>
    public static IReadOnlyList<SaasTestimonial> Testimonials() => new List<SaasTestimonial>
    {
        new("We replaced three tools with Lumora in a week. Our Monday metrics meeting went from forty minutes to ten.", "Amara Okafor", "Head of Product", "Northwind Outfitters", 5, "75%", "shorter metrics meetings"),
        new("The AI insights caught a checkout bug on a Saturday morning before a single customer complained.", "Daniel Reyes", "Engineering Manager", "Orbitworks", 5, "2.4×", "fewer late deliveries"),
        new("Our clinic managers finally have numbers they trust, and they don't need an analyst to get them.", "Priya Natarajan", "Director of Operations", "Brightlane Health", 5, "21%", "shorter patient waits"),
        new("Funnels that used to take a sprint now take a coffee break. Our editors build their own reports.", "Lena Hoffmann", "Growth Lead", "Fable & Finch", 4, "11 h", "saved per editor each month"),
    };

    /// <summary>The 24 customer workspaces: the demo workspace first, then a mix of plans, trials, and a few suspended ones.</summary>
    public static List<SaasWorkspace> Workspaces()
    {
        var workspaces = new List<SaasWorkspace>
        {
            new(DemoWorkspaceId, workspaceNames[0], "growth", SaasBillingPeriod.Annual, 12, SaasWorkspaceStatus.Active, new DateTime(2024, 11, 14), null,
                new DateTime(2026, 11, 14), "United States", 3_620_000, 18, 0m, "Visa", "4242", 8, 2028,
                new SaasWorkspaceSettings("America/New_York", "Monday", "United States", WeeklyDigest: true, SecurityAlerts: true, BillingEmails: true, ProductUpdates: false)),
        };

        for (var index = 1; index < workspaceNames.Length; index++)
        {
            var country = Countries[((index * 3) + 2) % Countries.Count];

            var (planId, status) = index % 7 == 3 ? ("growth", SaasWorkspaceStatus.Trial)
                : index % 11 == 5 ? ("growth", SaasWorkspaceStatus.Suspended)
                : index % 6 == 0 ? ("starter", SaasWorkspaceStatus.Active)
                : index % 9 == 4 ? ("enterprise", SaasWorkspaceStatus.Active)
                : ("growth", SaasWorkspaceStatus.Active);

            var period = planId == "starter" || status == SaasWorkspaceStatus.Trial || index % 3 != 0 ? SaasBillingPeriod.Monthly : SaasBillingPeriod.Annual;
            var seats = status == SaasWorkspaceStatus.Trial ? 5
                : planId switch
                {
                    "starter" => 3,
                    "growth" => 6 + ((index * 5) % 18),
                    _ => 40 + ((index * 7) % 60),
                };

            DateTime createdOn;
            DateTime? trialEndsOn = null;
            DateTime renewsOn;

            if (status == SaasWorkspaceStatus.Trial)
            {
                trialEndsOn = Today.AddDays(2 + (index % 12));
                createdOn = trialEndsOn.Value.AddDays(-TrialDays);
                renewsOn = trialEndsOn.Value;
            }
            else
            {
                createdOn = Today.AddDays(-(45 + ((index * 37) % 600)));
                renewsOn = NextRenewal(createdOn, period);
            }

            var limit = planId switch { "starter" => 100_000L, "growth" => 5_000_000L, _ => 20_000_000L };
            var events = limit * (((index * 17) + 23) % 96) / 100;
            var hasCard = planId != "starter" && (status != SaasWorkspaceStatus.Trial || index % 2 == 0);
            var card = index % cardBrands.Length;

            workspaces.Add(new SaasWorkspace(index + 1, workspaceNames[index], planId, period, seats, status, createdOn, trialEndsOn, renewsOn, country, events,
                planId == "starter" ? 1 + (index % 5) : 4 + ((index * 3) % 30), 0m,
                hasCard ? cardBrands[card] : null, hasCard ? cardLast4[card] : null,
                hasCard ? 1 + (index % 12) : 0, hasCard ? 2027 + (index % 3) : 0, DefaultSettings(country)));
        }

        return workspaces;
    }

    #endregion

    #region Private Methods

    private static DateTime NextRenewal(DateTime createdOn, SaasBillingPeriod period)
    {
        var renewal = createdOn.Date;

        while (renewal <= Today)
            renewal = SaasPricing.PeriodEnd(renewal, period);

        return renewal;
    }

    private static string PlanName(string planId) => Plans().First(plan => plan.Id == planId).Name;

    private static string Slug(string name) => new(name.Where(char.IsLetter).ToArray());

    #endregion

    #endregion
}
