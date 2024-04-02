namespace BlazorBootstrap.Demo.RCL;

public partial class MainLayout : LayoutComponentBase
{
    private string version = default!;
    private Sidebar3 sidebar = default!;
    private IEnumerable<Sidebar3NavItem> navItems = default!;

    [Inject] public IConfiguration Configuration { get; set; } = default!;

    protected override void OnInitialized()
    {
        version = $"v{Configuration["version"]}"; // example: v0.6.1
        base.OnInitialized();
    }

    private async Task<Sidebar3DataProviderResult> SidebarDataProvider(Sidebar3DataProviderRequest request)
    {
        if (navItems is null)
            navItems = GetNavItems();

        return await Task.FromResult(request.ApplyTo(navItems));
    }

    private IEnumerable<Sidebar3NavItem> GetNavItems()
    {
        return new List<Sidebar3NavItem>
        {

            new ()
            {
                Text = "Getting Started",
                Href = "/getting-started",
                IconName = IconName.HouseDoorFill
            },
            new ()
            {
                Text = "Layout",
                IconName = IconName.LayoutTextWindowReverse,
                IconColor = IconColor.Success,
                ChildItems = new List<Sidebar3NavItem>()
                {
                    new()
                    {
                        Text = "Blazor WebAssembly",
                        Href = "/layout-setup/blazor-webassembly",
                        IconName = IconName.BrowserEdge
                    },
                    new()
                    {
                        Text = "Blazor Server",
                        Href = "/layout-setup/blazor-server",
                        IconName = IconName.Server
                    }
                }
            },
            new ()
            { 
                Text = "Content", 
                IconName = IconName.BodyText, 
                IconColor = IconColor.Primary,
                ChildItems = new List<Sidebar3NavItem>
                {
                    new()
                    {
                        Text = "Icons", 
                        Href = "/icons", 
                        IconName = IconName.PersonSquare
                    }
                }
            },
            new ()
            { 
                Text = "Forms", 
                IconName = IconName.InputCursorText, 
                IconColor = IconColor.Success,
                ChildItems = new List<Sidebar3NavItem>()
                {
                    new ()
                    { 
                        Text = "Auto Complete", 
                        Href = "/autocomplete", 
                        IconName = IconName.InputCursorText
                    },
                    new ()
                    { 
                        Text = "Currency Input", 
                        Href = "/form/currency-input", 
                        IconName = IconName.CurrencyDollar
                    },
                    new ()
                    { 
                        Text = "Date Input", 
                        Href = "/form/date-input", 
                        IconName = IconName.CalendarDate
                    },
                    new ()
                    { 
                        Text = "Number Input", 
                        Href = "/form/number-input", 
                        IconName = IconName.InputCursor
                    },
                    new ()
                    { 
                        Text = "Range Input", 
                        Href = "/form/range-input", 
                        IconName = IconName.Sliders 
                    },
                    new ()
                    { 
                        Text = "Switch", 
                        Href = "/form/switch", 
                        IconName = IconName.ToggleOn 
                    },
                    new ()
                    { 
                        Text = "Time Input", 
                        Href = "/form/time-input", 
                        IconName = IconName.ClockFill
                    },

                }
            },
            new ()
            {  
                Text = "Components", 
                IconName = IconName.GearFill, 
                IconColor = IconColor.Danger,
                ChildItems = new List<Sidebar3NavItem>()
                {
                    new ()
                    { 
                        Text = "Accordion",
                        Href = "/accordion",
                        IconName = IconName.ChevronBarExpand
                    },
                    new ()
                    { 
                        Text = "Alerts",
                        Href = "/alerts",
                        IconName = IconName.CheckCircleFill
                    },
                    new ()
                    { 
                        Text = "Badge",
                        Href = "/badge",
                        IconName = IconName.AppIndicator
                    },
                    new ()
                    { 
                        Text = "Breadcrumb",
                        Href = "/breadcrumb",
                        IconName = IconName.SegmentedNav
                    },
                    new ()
                    { 
                        Text = "Buttons",
                        Href = "/buttons",
                        IconName = IconName.ToggleOn
                    },
                    new ()
                    { 
                        Text = "Callout",
                        Href = "/callout",
                        IconName = IconName.StickyFill
                    },
                    new ()
                    { 
                        Text = "Card",
                        Href = "/card",
                        IconName = IconName.CardHeading
                    },
                    new ()
                    { 
                        Text = "Charts",
                        Href = "/charts",
                        IconName = IconName.BarChartLineFill, 
                        Match = NavLinkMatch.All 
                    },
                    new ()
                    { 
                        Text = "Collapse",
                        Href = "/collapse",
                        IconName = IconName.ArrowsCollapse
                    },
                    new ()
                    { 
                        Text = "Confirm Dialog", 
                        Href = "/confirm-dialog", 
                        IconName = IconName.QuestionDiamondFill
                    },
                    new ()
                    { 
                        Text = "Dropdown",
                        Href = "/dropdown",
                        IconName = IconName.MenuButtonWideFill
                    },
                    new ()
                    { 
                        Text = "Grid",
                        Href = "/grid",
                        IconName = IconName.Grid
                    },
                    new ()
                    { 
                        Text = "Modals",
                        Href = "/modals",
                        IconName = IconName.WindowStack
                    },
                    new ()
                    { 
                        Text = "Offcanvas",
                        Href = "/offcanvas",
                        IconName = IconName.LayoutSidebarReverse
                    },
                    new ()
                    { 
                        Text = "Pagination",
                        Href = "/pagination",
                        IconName = IconName.ThreeDots
                    },
                    new ()
                    { 
                        Text = "Pdf Viewer", 
                        Href = "/pdf-viewer", 
                        IconName = IconName.FilePdfFill, 
                    },
                    new ()
                    { 
                        Text = "Placeholders",
                        Href = "/placeholders",
                        IconName = IconName.ColumnsGap
                    },
                    new ()
                    { 
                        Text = "Preload",
                        Href = "/preload",
                        IconName = IconName.ArrowClockwise 
                    },
                    new ()
                    { 
                        Text = "Progress",
                        Href = "/progress",
                        IconName = IconName.UsbC },
                    new ()
                    { 
                        Text = "Script Loader", 
                        Href = "/script-loader", 
                        IconName = IconName.CodeSlash
                    },
                    new ()
                    { 
                        Text = "Sidebar",
                        Href = "/sidebar",
                        IconName = IconName.LayoutSidebar
                    },
                    new ()
                    { 
                        Text = "Sidebar 2", 
                        Href = "/sidebar2", 
                        IconName = IconName.ListNested
                    },
                    new ()
                    { 
                        Text = "Spinner",
                        Href = "/spinners",
                        IconName = IconName.ArrowRepeat 
                    },
                    new ()
                    { 
                        Text = "Tabs",
                        Href = "/tabs",
                        IconName = IconName.WindowPlus 
                    },
                    new ()
                    { 
                        Text = "Toasts",
                        Href = "/toasts",
                        IconName = IconName.ExclamationTriangleFill 
                    },
                    new ()
                    { 
                        Text = "Tooltips",
                        Href = "/tooltips",
                        IconName = IconName.ChatSquareDotsFill
                    },

                }
            },
            new ()
            { 
                Text = "Data Visualization", 
                IconName = IconName.BarChartFill, 
                IconColor = IconColor.Warning,
                ChildItems = new List<Sidebar3NavItem>()
                {
                    new ()
                    { 
                        Text = "Bar Chart", 
                        Href = "/charts/bar-chart", 
                        IconName = IconName.BarChartFill, 
                        Match = NavLinkMatch.All 
                    },
                    new ()
                    { 
                        Text = "Doughnut Chart", 
                        Href = "/charts/doughnut-chart", 
                        IconName = IconName.CircleFill,
                        Match = NavLinkMatch.All 
                    },
                    new ()
                    { 
                        Text = "Line Chart", 
                        Href = "/charts/line-chart", 
                        IconName = IconName.GraphUp, 
                        Match = NavLinkMatch.All 
                    },
                    new ()
                    { 
                        Text = "Pie Chart", 
                        Href = "/charts/pie-chart", 
                        IconName = IconName.PieChartFill,
                        Match = NavLinkMatch.All 
                    },

                }
            },
            new()
            { 
                Text = "Services", 
                IconName = IconName.WrenchAdjustableCircleFill, 
                IconColor = IconColor.Success,
                ChildItems = new List<Sidebar3NavItem>()
                {
                    new ()
                    { 
                        Text = "Modal Service", 
                        Href = "/modal-service", 
                        IconName = IconName.WindowStack 
                    },
                }
            },
        };
      
    }
}
