namespace BlazorBootstrap.Demo.RCL;

public partial class MainLayout : LayoutComponentBase
{
    private string version = default!;
    private string docsUrl = default!;
    private string blogUrl = default!;
    private string githubUrl = default!;
    private string twitterUrl = default!;
    private string linkedInUrl = default!;
    private string githubIssuesUrl = default!;
    private string githubDiscussionsUrl = default!;
    private string stackoverflowUrl = default!;

    private Sidebar sidebar = default!;
    private IEnumerable<NavItem> navItems = default!;

    [Inject] public IConfiguration Configuration { get; set; } = default!;

    protected override void OnInitialized()
    {
        version = $"v{Configuration["version"]}"; // example: v0.6.1
        docsUrl = $"{Configuration["urls:docs"]}";
        blogUrl = $"{Configuration["urls:blog"]}";
        githubUrl = $"{Configuration["urls:github"]}";
        twitterUrl = $"{Configuration["urls:twitter"]}";
        linkedInUrl = $"{Configuration["urls:linkedin"]}";
        githubIssuesUrl = $"{Configuration["urls:github_issues"]}";
        githubDiscussionsUrl = $"{Configuration["urls:github_discussions"]}";
        stackoverflowUrl = $"{Configuration["urls:stackoverflow"]}";
        base.OnInitialized();
    }

    private async Task<SidebarDataProviderResult> SidebarDataProvider(SidebarDataProviderRequest request)
    {
        if (navItems is null)
            navItems = GetNavItems();

        return await Task.FromResult(request.ApplyTo(navItems));
    }

    private IEnumerable<NavItem> GetNavItems()
    {
        navItems = new List<NavItem>
        {
            new (){ Id = "1", Text = "Getting Started", Href = "/getting-started", IconName = IconName.HouseDoorFill },

            new (){ Id = "2", Text = "Layout", IconName = IconName.LayoutTextWindowReverse, IconColor = IconColor.Success },
            new (){ Id = "200", Text = "Blazor WebAssembly", Href = "/layout-setup/blazor-webassembly", IconName = IconName.BrowserEdge, ParentId = "2" },
            new (){ Id = "201", Text = "Blazor Server", Href = "/layout-setup/blazor-server", IconName = IconName.Server, ParentId = "2" },

            new (){ Id = "3", Text = "Content", IconName = IconName.BodyText, IconColor = IconColor.Primary },
            new (){ Id = "300", Text = "Icons", Href = "/icons", IconName = IconName.PersonSquare, ParentId = "3" },

            new (){ Id = "4", Text = "Forms", IconName = IconName.InputCursorText, IconColor = IconColor.Success },
            new (){ Id = "400", Text = "Auto Complete", Href = "/autocomplete", IconName = IconName.InputCursorText, ParentId = "4" },
            new (){ Id = "401", Text = "Currency Input", Href = "/form/currency-input", IconName = IconName.CurrencyDollar, ParentId = "4" },
            new (){ Id = "402", Text = "Date Input", Href = "/form/date-input", IconName = IconName.CalendarDate, ParentId = "4" },
            new (){ Id = "403", Text = "Number Input", Href = "/form/number-input", IconName = IconName.InputCursor, ParentId = "4" },
            new (){ Id = "404", Text = "Range Input", Href = "/form/range-input", IconName = IconName.Sliders, ParentId = "4" },
            new (){ Id = "405", Text = "Switch", Href = "/form/switch", IconName = IconName.ToggleOn, ParentId = "4" },
            new (){ Id = "406", Text = "Time Input", Href = "/form/time-input", IconName = IconName.ClockFill, ParentId = "4" },

            new (){ Id = "5", Text = "Components", IconName = IconName.GearFill, IconColor = IconColor.Danger },
            new (){ Id = "500", Text = "Accordion", Href = "/accordion", IconName = IconName.ChevronBarExpand, ParentId = "5" },
            new (){ Id = "501", Text = "Alerts", Href = "/alerts", IconName = IconName.CheckCircleFill, ParentId = "5" },
            new (){ Id = "502", Text = "Badge", Href = "/badge", IconName = IconName.AppIndicator, ParentId = "5" },
            new (){ Id = "503", Text = "Breadcrumb", Href = "/breadcrumb", IconName = IconName.SegmentedNav, ParentId = "5" },
            new (){ Id = "504", Text = "Buttons", Href = "/buttons", IconName = IconName.ToggleOn, ParentId = "5" },
            new (){ Id = "505", Text = "Callout", Href = "/callout", IconName = IconName.StickyFill, ParentId = "5" },
            new (){ Id = "506", Text = "Card", Href = "/card", IconName = IconName.CardHeading, ParentId = "5" },
            new (){ Id = "507", Text = "Charts", Href = "/charts", IconName = IconName.BarChartLineFill, ParentId = "5", Match = NavLinkMatch.All },
            new (){ Id = "508", Text = "Collapse", Href = "/collapse", IconName = IconName.ArrowsCollapse, ParentId = "5" },
            new (){ Id = "509", Text = "Confirm Dialog", Href = "/confirm-dialog", IconName = IconName.QuestionDiamondFill, ParentId = "5" },
            new (){ Id = "510", Text = "Dropdown", Href = "/dropdown", IconName = IconName.MenuButtonWideFill, ParentId = "5" },
            new (){ Id = "511", Text = "Grid", Href = "/grid", IconName = IconName.Grid, ParentId = "5" },
            new (){ Id = "512", Text = "Modals", Href = "/modals", IconName = IconName.WindowStack, ParentId = "5" },
            new (){ Id = "513", Text = "Offcanvas", Href = "/offcanvas", IconName = IconName.LayoutSidebarReverse, ParentId = "5" },
            new (){ Id = "514", Text = "Pagination", Href = "/pagination", IconName = IconName.ThreeDots, ParentId = "5" },
            new (){ Id = "515", Text = "PDF Viewer", Href = "/pdf-viewer", IconName = IconName.FilePdfFill, ParentId = "5" },
            new (){ Id = "516", Text = "Placeholders", Href = "/placeholders", IconName = IconName.ColumnsGap, ParentId = "5" },
            new (){ Id = "517", Text = "Preload", Href = "/preload", IconName = IconName.ArrowClockwise, ParentId = "5" },
            new (){ Id = "518", Text = "Progress", Href = "/progress", IconName = IconName.UsbC, ParentId = "5" },
            new (){ Id = "519", Text = "Ribbon", Href = "/ribbon", IconName = IconName.WindowStack, ParentId = "5" },
            new (){ Id = "520", Text = "Script Loader", Href = "/script-loader", IconName = IconName.CodeSlash, ParentId = "5" },
            new (){ Id = "521", Text = "Sidebar", Href = "/sidebar", IconName = IconName.LayoutSidebar, ParentId = "5" },
            new (){ Id = "522", Text = "Sidebar 2", Href = "/sidebar2", IconName = IconName.ListNested, ParentId = "5" },
            new (){ Id = "523", Text = "Sortable List", Href = "/sortable-list", IconName = IconName.ArrowsMove, ParentId = "5" },
            new (){ Id = "524", Text = "Spinner", Href = "/spinners", IconName = IconName.ArrowRepeat, ParentId = "5" },
            new (){ Id = "525", Text = "Tabs", Href = "/tabs", IconName = IconName.WindowPlus, ParentId = "5" },
            new (){ Id = "526", Text = "Toasts", Href = "/toasts", IconName = IconName.ExclamationTriangleFill, ParentId = "5" },
            new (){ Id = "527", Text = "Tooltips", Href = "/tooltips", IconName = IconName.ChatSquareDotsFill, ParentId = "5" },

            new (){ Id = "6", Text = "Data Visualization", IconName = IconName.BarChartFill, IconColor = IconColor.Warning },
            new (){ Id = "600", Text = "Bar Chart", Href = "/charts/bar-chart", IconName = IconName.BarChartFill, ParentId = "6", Match = NavLinkMatch.All },
            new (){ Id = "601", Text = "Doughnut Chart", Href = "/charts/doughnut-chart", IconName = IconName.CircleFill, ParentId = "6", Match = NavLinkMatch.All },
            new (){ Id = "602", Text = "Line Chart", Href = "/charts/line-chart", IconName = IconName.GraphUp, ParentId = "6", Match = NavLinkMatch.All },
            new (){ Id = "603", Text = "Pie Chart", Href = "/charts/pie-chart", IconName = IconName.PieChartFill, ParentId = "6", Match = NavLinkMatch.All },

            new(){ Id = "7", Text = "Services", IconName = IconName.WrenchAdjustableCircleFill, IconColor = IconColor.Success },
            new (){ Id = "700", Text = "Modal Service", Href = "/modal-service", IconName = IconName.WindowStack, ParentId = "7" },
        };

        return navItems;
    }
}
