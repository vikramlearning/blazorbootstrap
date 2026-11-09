using BlazorBootstrap.Demo.RCL.Models.Showcase;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase;

/// <summary>
/// The list of Showcase sample websites and the static assets committed for them.
/// </summary>
public static class ShowcaseCatalog
{
    #region Fields and Constants

    /// <summary>
    /// Static asset paths (relative to the app base) that are committed to the repository.
    /// <see cref="Components.Pages.Showcase.Shared.ShowcaseImage" /> shows a placeholder for any path not listed here,
    /// so a site keeps working until its licensed assets are supplied.
    /// </summary>
    private static readonly HashSet<string> committedAssets = new(StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyList<ShowcaseSite> sites = new List<ShowcaseSite>
    {
        new("ai-assistant", "AI Assistant Workspace", "Chat workspace with streaming answers, prompt library, and attachments.", "SaaS", IconName.Stars, "#7c3aed", "AiAssistant",
            new[] { "Sidebar2", "SplitView", "Markdown", "Placeholders", "Offcanvas", "Toasts", "Avatar", "FileInput", "ThemeSwitcher" }),
        new("saas-dashboard", "SaaS Admin & Analytics", "KPI tiles, revenue charts, user management, and settings.", "SaaS", IconName.Speedometer2, "#2563eb", "SaasDashboard",
            new[] { "Sidebar", "Charts", "Grid", "Card", "Badge", "Progress", "Dropdown", "Tabs", "Modal" }),
        new("ecommerce", "E-commerce Storefront", "Catalog filters, product pages, cart drawer, and checkout.", "Commerce", IconName.Bag, "#ea580c", "Ecommerce",
            new[] { "Carousel", "Card", "Ribbon", "RatingInput", "RangeInput", "Offcanvas", "Form inputs", "Pagination", "Toasts" }),
        new("crm", "CRM & Sales Pipeline", "Deal board, contacts, account timeline, and forecasts.", "SaaS", IconName.Kanban, "#0d9488", "Crm",
            new[] { "SortableList", "Grid", "AvatarGroup", "AutoComplete", "Modal", "Tabs", "Charts" }),
        new("project-board", "Project Management Board", "Kanban and list views, task drawer, workload, and roadmap.", "SaaS", IconName.ListCheck, "#4f46e5", "ProjectBoard",
            new[] { "SortableList", "Offcanvas", "Progress", "Tooltip", "Avatar", "MindMap", "DateInput", "ConfirmDialog" }),
        new("marketing", "SaaS Marketing Site", "Hero, features, pricing switch, testimonials, FAQ, and sign-up.", "Marketing", IconName.Megaphone, "#db2777", "Marketing",
            new[] { "Card", "Ribbon", "Switch", "Carousel", "Accordion", "Collapse", "Form inputs", "Callout" }),
        new("fintech", "Personal Finance", "Balances, spending charts, transactions, budgets, and transfers.", "Industry", IconName.Wallet2, "#059669", "Fintech",
            new[] { "Charts", "Grid", "CurrencyInput", "Progress", "ConfirmDialog", "Alert" }),
        new("healthcare", "Healthcare Clinic Portal", "Find doctors, book appointments, records, and lab reports.", "Industry", IconName.HeartPulse, "#0891b2", "Healthcare",
            new[] { "DateInput", "TimeInput", "Tabs", "Grid", "PdfViewer", "GoogleMap", "Avatar" }),
        new("learning", "Online Learning", "Course catalog, lesson player, progress, and certificates.", "Industry", IconName.Mortarboard, "#ca8a04", "Learning",
            new[] { "Card", "Accordion", "Progress", "Markdown", "PdfViewer", "RatingInput", "Badge" }),
        new("travel", "Travel & Stay Booking", "Search stays on a map, compare, book, and manage trips.", "Industry", IconName.Airplane, "#0284c7", "Travel",
            new[] { "GoogleMap", "Carousel", "DateInput", "NumberInput", "RangeInput", "RatingInput", "Image" }),
        new("help-desk", "Help Desk & Ticketing", "Ticket queue, SLA badges, rich-text replies, and knowledge base.", "Industry", IconName.Headset, "#dc2626", "HelpDesk",
            new[] { "Grid", "Tabs", "RichTextEditor", "Badge", "Breadcrumb", "Toasts", "Markdown" }),
        new("monitoring", "IoT & DevOps Monitoring", "Live metrics, service health, incidents, and logs.", "Industry", IconName.Activity, "#16a34a", "Monitoring",
            new[] { "Charts", "Progress", "Alert", "Callout", "Grid", "Spinner", "Tooltip" }),
        new("hr-portal", "HR & Employee Portal", "Directory, org chart, leave requests, and onboarding.", "Extras", IconName.People, "#9333ea", "HrPortal",
            new[] { "Grid", "Avatar", "Tabs", "MindMap", "DateInput", "CheckboxInput", "Progress" }),
        new("restaurant", "Restaurant Ordering", "Menu, quantity picker, cart, delivery map, and order tracking.", "Extras", IconName.CupHot, "#b45309", "Restaurant",
            new[] { "Card", "NumberInput", "Offcanvas", "GoogleMap", "Progress", "Toasts" }),
        new("creator-blog", "Creator Portfolio & Blog", "Markdown posts, image gallery, and pagination.", "Extras", IconName.PenFill, "#e11d48", "CreatorBlog",
            new[] { "Markdown", "Image", "Card", "Pagination", "Modal" }),
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
