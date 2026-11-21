using BlazorBootstrap.Demo.RCL.Models.Showcase;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase;

/// <summary>
/// The list of Showcase sample websites and the static assets committed for them.
/// </summary>
public static class ShowcaseCatalog
{
    #region Fields and Constants

    private const string AssetPrefix = "_content/BlazorBootstrap.Demo.RCL/showcase";

    // Gallery filter order (work item 26112002, D-012): what the buyer runs, from internal tools to industry back offices.
    private static readonly string[] categoryOrder = { "Business operations", "SaaS and AI", "Industry" };

    // The home page's four Showcase cards, highest enterprise value first
    // (ranking in blazorbootstrap.sdd devspec/research/showcase-enterprise-review.md).
    private static readonly string[] featuredSlugs = { "help-desk", "hr-portal", "monitoring", "saas" };

    private static readonly IReadOnlyList<ShowcaseSite> sites = new List<ShowcaseSite>
    {
        new("ai-assistant", "AI Assistant Workspace", "Members chat with streaming answers, attachments, and a prompt library; the Owner, Admins, and prompt editors each get the admin pages their role allows: members and roles, shared prompts, model policy, monthly spend caps that warn at 80% and pause chat at 100%, a conversation retention window, sensitive-data protection that redacts or blocks card numbers, ID numbers, IBANs, and API keys with an admin review queue, and an audit log.", "Roles, spend caps, retention, and data protection", "SaaS and AI", IconName.Stars, "#7c3aed", "AiAssistant",
            new[] { "Grid", "Markdown", "Sidebar", "Modal", "Charts", "Offcanvas", "Progress", "Badge", "Placeholders", "Avatar", "Form inputs" }),
        new("saas", "SaaS Product", "Public site and trial sign-up; customers run their team and billing and fix failed payments; operators manage tenants, invoices, dunning with retries and grace periods, plans, leads, and an audit log.", "Tenants, billing with dunning, and an audit log", "SaaS and AI", IconName.Lightning, "#db2777", "Saas",
            new[] { "Grid", "Sidebar", "Charts", "Card", "Modal", "Timeline", "Switch", "Accordion", "Progress", "Form inputs" }),
        new("ecommerce", "Commerce Back Office", "A retailer's back office fulfils orders with bulk packing and shipping, decides returns and refunds, sets prices and offers in bulk, restocks products below their reorder point through purchase orders that are approved (a second approver over $5,000) and received in full or in part, manages products, suppliers, customers, and reviews, reads sales analytics, and keeps an audit log; shoppers browse the storefront, check out with a test card, and track orders, returns, and reviews.", "Orders, purchasing, returns, and pricing", "Industry", IconName.Bag, "#ea580c", "Ecommerce",
            new[] { "Grid", "Sidebar", "Modal", "Charts", "Badge", "Dropdown", "Navbar", "Carousel", "Card", "RatingInput", "RangeInput", "Offcanvas", "Tabs", "Timeline", "Form inputs", "Pagination", "Toasts" }),
        new("project-board", "Project & Client Board", "Team members work tasks on boards and log their hours; project managers run projects, workload, the roadmap, and reports and approve the timesheets; account managers handle clients, the deal pipeline, and the forecast and turn approved hours into client invoices; managers read the audit log.", "Task boards, timesheets, and client invoices", "Business operations", IconName.Kanban, "#4f46e5", "ProjectBoard",
            new[] { "Grid", "SortableList", "MindMap", "Sidebar", "Charts", "Modal", "Progress", "Badge", "Form inputs" }),
        new("fintech", "Banking Operations", "A bank's back office works the held-transfer review queue with bulk release, triages AML alerts into cases that a senior reviewer approves (four-eyes), decides disputes, follows customers and scheduled postings, and reads the audit log; customers bank with a one-time-code sign-in, transfers, payees, budgets, and disputes.", "Transfer review, AML cases, and disputes", "Industry", IconName.Wallet2, "#059669", "Fintech",
            new[] { "Grid", "Sidebar", "Navbar", "Card", "Charts", "Modal", "CurrencyInput", "Badge", "Progress", "Alert", "Breadcrumb", "Form inputs" }),
        new("healthcare", "Healthcare Clinic Portal", "Patients book visits, read records and lab reports, and pay what insurance leaves; doctors run visits, charts, lab results, and coding; the front desk checks patients in and keeps doctors' hours; billing works insurance claims from submission through denials, corrections, and appeals; and an audit log records who did what.", "Appointments, records, and insurance claims", "Industry", IconName.HeartPulse, "#0891b2", "Healthcare",
            new[] { "Grid", "PdfViewer", "GoogleMap", "Sidebar", "Tabs", "Modal", "Form inputs" }),
        new("learning", "Online Learning", "Learners enroll, take scored quizzes, ask questions, and earn verifiable certificates; instructors build and publish courses and quizzes, answer questions, and reply to reviews.", "Courses, quizzes, and certificates", "Industry", IconName.Mortarboard, "#ca8a04", "Learning",
            new[] { "Card", "Accordion", "Progress", "Markdown", "RichTextEditor", "PdfViewer", "Sidebar", "Charts", "Modal" }),
        new("travel", "Travel & Stay Booking", "Guests search, book, and manage trips; hosts answer requests and run calendars, listings, inbox, reviews, and earnings.", "Stay search, booking, and hosting", "Industry", IconName.Airplane, "#0284c7", "Travel",
            new[] { "GoogleMap", "Carousel", "Sidebar", "Charts", "DateInput", "RatingInput", "Modal", "Form inputs" }),
        new("help-desk", "Help Desk & Ticketing", "Help center for customers, an agent desk with SLAs and escalation rules, knowledge base authoring, reports, an audit log, and admin.", "Help center, agent desk, and reports", "Business operations", IconName.Headset, "#dc2626", "HelpDesk",
            new[] { "Grid", "Sidebar", "Charts", "RichTextEditor", "Modal", "Tabs", "Badge", "Breadcrumb", "Toasts" }),
        new("monitoring", "IoT & DevOps Monitoring", "A public status page with history and subscriptions; engineers work the incident queue, get paged by escalation policies and acknowledge, publish updates, and follow live logs and traces; admins run alert rules and drills, escalation policies, maintenance, services, and an audit log.", "Status page, incidents, and paging", "Business operations", IconName.Activity, "#16a34a", "Monitoring",
            new[] { "Grid", "Charts", "Sidebar", "Modal", "Badge", "Breadcrumb", "Progress", "Alert", "Callout", "Tooltip", "Toasts" }),
        new("hr-portal", "HR & Employee Portal", "Employees find colleagues, request leave, write self reviews, and work through onboarding; managers approve leave, review their team, and follow new hires; HR admins run performance review cycles with calibration and manage people, leave policies, onboarding templates, headcount analytics, and an audit log.", "Directory, leave, onboarding, and reviews", "Business operations", IconName.People, "#9333ea", "HrPortal",
            new[] { "Grid", "MindMap", "Sidebar", "Charts", "Modal", "Progress", "Badge", "Form inputs" }),
    };

    /// <summary>
    /// Static asset paths (relative to the app base) that are committed to the repository; each is listed in
    /// <c>wwwroot/showcase/CREDITS.md</c>. <see cref="Components.Pages.Showcase.Shared.ShowcaseImage" /> shows a
    /// placeholder for any path not listed here, so a site keeps working until its licensed assets are supplied.
    /// Declared after <c>sites</c> because it lists each site's gallery screenshots.
    /// </summary>
    private static readonly HashSet<string> committedAssets = new(
        Enumerable.Range(101, 31).Select(id => $"{AssetPrefix}/ecommerce/products/{id}.webp")
            .Concat(Enumerable.Range(1, 3).Select(slide => $"{AssetPrefix}/ecommerce/hero-{slide}.webp"))
            .Concat(Enumerable.Range(201, 9).Select(id => $"{AssetPrefix}/learning/course-{id}.webp"))
            .Concat(new[] { "cape-town", "kyoto", "lisbon", "vancouver" }.Select(id => $"{AssetPrefix}/travel/destinations/{id}.webp"))
            .Concat(Enumerable.Range(301, 18).SelectMany(id => Enumerable.Range(1, 3).Select(photo => $"{AssetPrefix}/travel/stays/{id}-{photo}.webp")))
            .Concat(new[] { "okoye", "raman", "chen", "rossi", "whitaker", "bello" }.Select(id => $"{AssetPrefix}/healthcare/doctors/{id}.webp"))
            .Concat(new[] { "downtown", "lakeview", "northfield" }.Select(id => $"{AssetPrefix}/healthcare/clinics/{id}.webp"))
            .Concat(sites.SelectMany(site => new[] { site.ScreenshotLight, site.ScreenshotDark })),
        StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Finds a site by slug.
    /// </summary>
    public static ShowcaseSite Get(string slug) => sites.First(site => site.Slug == slug);

    /// <summary>
    /// Returns whether a static asset path is committed to the repository.
    /// </summary>
    public static bool IsAssetAvailable(string? path) => !string.IsNullOrWhiteSpace(path) && committedAssets.Contains(path);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the gallery categories in filter order; a category no site uses is left out.
    /// </summary>
    public static IReadOnlyList<string> Categories { get; } = categoryOrder.Where(category => sites.Any(site => site.Category == category))
        .Concat(sites.Select(site => site.Category).Distinct().Except(categoryOrder)).ToList();

    /// <summary>
    /// Gets the sites the home page features, in display order. Declared after <c>sites</c> so it is initialized after it.
    /// </summary>
    public static IReadOnlyList<ShowcaseSite> FeaturedSites { get; } = featuredSlugs.Select(Get).ToList();

    /// <summary>
    /// Gets whether the Showcase is advertised from the home page and the site navigation. Set to
    /// <see langword="false" /> to hide the entry points again; <c>/showcase</c> stays reachable by direct URL either way.
    /// </summary>
    public static bool IsPublished { get; } = true;

    /// <summary>
    /// Gets every Showcase site in gallery order.
    /// </summary>
    public static IReadOnlyList<ShowcaseSite> Sites => sites;

    #endregion
}
