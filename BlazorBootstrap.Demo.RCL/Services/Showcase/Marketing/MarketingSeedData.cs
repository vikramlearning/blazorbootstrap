using BlazorBootstrap.Demo.RCL.Models.Showcase.Marketing;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Marketing;

/// <summary>
/// Fixed, fictitious content for the "Lumora" marketing site.
/// </summary>
public static class MarketingSeedData
{
    #region Fields and Constants

    /// <summary>Annual billing discount (two months free).</summary>
    public const decimal AnnualDiscount = 0.20m;

    /// <summary>The fictitious product name.</summary>
    public const string ProductName = "Lumora";

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Customer stories.</summary>
    public static IReadOnlyList<MarketingCaseStudy> CaseStudies() => new List<MarketingCaseStudy>
    {
        new("Northwind Outfitters", "Retail", "Unified 14 storefront dashboards into one", "38% faster weekly reporting", IconName.Shop),
        new("Brightlane Health", "Healthcare", "Gave clinic managers live wait-time insights", "21% shorter patient waits", IconName.HeartPulse),
        new("Orbitworks", "Logistics", "Spotted route delays before customers did", "2.4× fewer late deliveries", IconName.Truck),
        new("Fable & Finch", "Publishing", "Replaced spreadsheets with self-serve funnels", "11 hours saved per editor / month", IconName.Book),
    };

    /// <summary>Plan comparison rows.</summary>
    public static IReadOnlyList<MarketingComparisonRow> Comparison() => new List<MarketingComparisonRow>
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

    /// <summary>Frequently asked questions.</summary>
    public static IReadOnlyList<MarketingFaq> Faqs() => new List<MarketingFaq>
    {
        new("trial", "Billing", "Is there a free trial?", "Yes. Every plan starts with a 14-day trial with all Growth features unlocked. No card is needed to start."),
        new("switch", "Billing", "Can I switch plans later?", "Upgrade or downgrade at any time. Changes are prorated to the day, and annual plans can move to monthly at renewal."),
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
    public static IReadOnlyList<MarketingFeature> Features() => new List<MarketingFeature>
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

    /// <summary>Pricing plans.</summary>
    public static IReadOnlyList<MarketingPlan> Plans() => new List<MarketingPlan>
    {
        new("starter", "Starter", "For side projects and small teams getting started.", 0m, false,
            new[] { "100k events / month", "5 dashboards", "3 team seats", "90-day history" }, "Start for free"),
        new("growth", "Growth", "For product teams that make decisions with data every day.", 49m, true,
            new[] { "5M events / month", "Unlimited dashboards", "AI insights", "Warehouse sync", "Priority support" }, "Start 14-day trial"),
        new("enterprise", "Enterprise", "For organizations with advanced security and scale needs.", 199m, false,
            new[] { "Unlimited events", "SAML SSO and SCIM", "Real-time warehouse sync", "99.99% uptime SLA", "Dedicated manager" }, "Talk to sales"),
    };

    /// <summary>Customer quotes.</summary>
    public static IReadOnlyList<MarketingTestimonial> Testimonials() => new List<MarketingTestimonial>
    {
        new("We replaced three tools with Lumora in a week. Our Monday metrics meeting went from forty minutes to ten.", "Amara Okafor", "Head of Product", "Northwind Outfitters", 5, "75%", "shorter metrics meetings"),
        new("The AI insights caught a checkout bug on a Saturday morning before a single customer complained.", "Daniel Reyes", "Engineering Manager", "Orbitworks", 5, "2.4×", "fewer late deliveries"),
        new("Our clinic managers finally have numbers they trust, and they don't need an analyst to get them.", "Priya Natarajan", "Director of Operations", "Brightlane Health", 5, "21%", "shorter patient waits"),
        new("Funnels that used to take a sprint now take a coffee break. Our editors build their own reports.", "Lena Hoffmann", "Growth Lead", "Fable & Finch", 4, "11 h", "saved per editor each month"),
    };

    #endregion

    #endregion
}
