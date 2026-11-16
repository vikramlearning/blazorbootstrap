using BlazorBootstrap.Demo.RCL.Models.Showcase;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase;

/// <summary>
/// The list of Showcase sample websites and the static assets committed for them.
/// </summary>
public static class ShowcaseCatalog
{
    #region Fields and Constants

    private const string AssetPrefix = "_content/BlazorBootstrap.Demo.RCL/showcase";

    /// <summary>
    /// Static asset paths (relative to the app base) that are committed to the repository; each is listed in
    /// <c>wwwroot/showcase/CREDITS.md</c>. <see cref="Components.Pages.Showcase.Shared.ShowcaseImage" /> shows a
    /// placeholder for any path not listed here, so a site keeps working until its licensed assets are supplied.
    /// </summary>
    private static readonly HashSet<string> committedAssets = new(
        Enumerable.Range(101, 31).Select(id => $"{AssetPrefix}/ecommerce/products/{id}.webp")
            .Concat(Enumerable.Range(1, 3).Select(slide => $"{AssetPrefix}/ecommerce/hero-{slide}.webp"))
            .Concat(Enumerable.Range(201, 8).Select(id => $"{AssetPrefix}/learning/course-{id}.webp"))
            .Concat(new[] { "cape-town", "kyoto", "lisbon", "vancouver" }.Select(id => $"{AssetPrefix}/travel/destinations/{id}.webp"))
            .Concat(Enumerable.Range(301, 18).SelectMany(id => Enumerable.Range(1, 3).Select(photo => $"{AssetPrefix}/travel/stays/{id}-{photo}.webp"))),
        StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyList<ShowcaseSite> sites = new List<ShowcaseSite>
    {
        new("ai-assistant", "AI Assistant Workspace", "Members chat with streaming answers, attachments, and a prompt library; workspace admins manage members, shared prompts, usage, and model policy.", "SaaS", IconName.Stars, "#7c3aed", "AiAssistant",
            new[] { "Markdown", "Offcanvas", "Sidebar", "Modal", "Charts", "Placeholders", "Progress", "Avatar", "Form inputs" }),
        new("saas", "SaaS Product", "Public site, trial sign-up, a customer workspace with team and billing, and an operator console.", "SaaS", IconName.Lightning, "#db2777", "Saas",
            new[] { "Sidebar", "Charts", "Card", "Switch", "Accordion", "Modal", "Tabs", "Progress", "Dropdown", "Form inputs" }),
        new("ecommerce", "E-commerce Storefront", "Catalog filters, product pages, cart drawer, and checkout.", "Commerce", IconName.Bag, "#ea580c", "Ecommerce",
            new[] { "Carousel", "Card", "Ribbon", "RatingInput", "RangeInput", "Offcanvas", "Form inputs", "Pagination", "Toasts" }),
        new("project-board", "Project & Client Board", "Team members work tasks on boards; project managers run projects, workload, the roadmap, and reports; account managers handle clients, the deal pipeline, and the forecast.", "SaaS", IconName.Kanban, "#4f46e5", "ProjectBoard",
            new[] { "SortableList", "MindMap", "Sidebar", "Charts", "Modal", "Progress", "Form inputs" }),
        new("fintech", "Personal Finance", "Customers bank with a one-time-code sign-in, transfers, payees, and disputes; operations review held transfers and disputes.", "Industry", IconName.Wallet2, "#059669", "Fintech",
            new[] { "Sidebar", "Charts", "CurrencyInput", "Progress", "Modal", "Alert", "Form inputs" }),
        new("healthcare", "Healthcare Clinic Portal", "Find doctors, book appointments, records, and lab reports.", "Industry", IconName.HeartPulse, "#0891b2", "Healthcare",
            new[] { "DateInput", "TimeInput", "Tabs", "Grid", "PdfViewer", "GoogleMap", "Avatar" }),
        new("learning", "Online Learning", "Learners enroll, take scored quizzes, ask questions, and earn verifiable certificates; instructors build and publish courses and quizzes, answer questions, and reply to reviews.", "Industry", IconName.Mortarboard, "#ca8a04", "Learning",
            new[] { "Card", "Accordion", "Progress", "Markdown", "RichTextEditor", "Sidebar", "Charts", "Modal" }),
        new("travel", "Travel & Stay Booking", "Guests search, book, and manage trips; hosts answer requests and run calendars, listings, inbox, reviews, and earnings.", "Industry", IconName.Airplane, "#0284c7", "Travel",
            new[] { "GoogleMap", "Carousel", "Sidebar", "Charts", "DateInput", "RatingInput", "Modal", "Form inputs" }),
        new("help-desk", "Help Desk & Ticketing", "Help center for customers, an agent desk with SLAs and saved replies, knowledge base authoring, reports, and admin.", "Industry", IconName.Headset, "#dc2626", "HelpDesk",
            new[] { "Sidebar", "Charts", "RichTextEditor", "Modal", "Tabs", "Badge", "Breadcrumb", "Toasts" }),
        new("monitoring", "IoT & DevOps Monitoring", "A public status page with history and subscriptions; engineers work incidents, publish updates, follow live logs and traces, and share on-call.", "Industry", IconName.Activity, "#16a34a", "Monitoring",
            new[] { "Charts", "Sidebar", "Progress", "Alert", "Callout", "Grid", "Modal", "Tooltip" }),
        new("hr-portal", "HR & Employee Portal", "Directory, org chart, leave requests, and onboarding.", "Extras", IconName.People, "#9333ea", "HrPortal",
            new[] { "Grid", "Avatar", "Tabs", "MindMap", "DateInput", "CheckboxInput", "Progress" }),
    };

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
    /// Gets whether the Showcase is advertised from the home page and the site navigation.
    /// Kept <see langword="false" /> until every sample site is complete; <c>/showcase</c> stays reachable by direct URL.
    /// </summary>
    public static bool IsPublished { get; } = false;

    /// <summary>
    /// Gets every Showcase site in gallery order.
    /// </summary>
    public static IReadOnlyList<ShowcaseSite> Sites => sites;

    #endregion
}
