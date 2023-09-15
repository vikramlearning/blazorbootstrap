using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorBootstrap.Demo.Server.Shared;

public partial class MainLayout : LayoutComponentBase
{
    private string version = default!;
    private Sidebar sidebar = default!;
    private IEnumerable<NavItem> navItems = default!;

    [Inject] public IConfiguration Configuration { get; set; } = default!;

    protected override void OnInitialized()
    {
        version = $"v{Configuration.GetValue<string>("version")}"; // example: v0.6.1
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
            new (){ Id = "1", Text = "Getting Started", IconName = IconName.HouseDoorFill },
            new (){ Id = "100", Text = "Blazor WebAssembly", Href = "/getting-started/blazor-webassembly", IconName = IconName.BrowserChrome, ParentId = "1" },
            new (){ Id = "101", Text = "Blazor Server", Href = "/getting-started/blazor-server", IconName = IconName.Server, ParentId = "1" },
            new (){ Id = "102", Text = "MAUI Blazor", Href = "/getting-started/maui-blazor", IconName = IconName.PhoneFill, ParentId = "1" },

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
            new (){ Id = "404", Text = "Switch", Href = "/form/switch", IconName = IconName.ToggleOn, ParentId = "4" },
            new (){ Id = "405", Text = "Time Input", Href = "/form/time-input", IconName = IconName.ClockFill, ParentId = "4" },

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
            new (){ Id = "515", Text = "Placeholders", Href = "/placeholders", IconName = IconName.ColumnsGap, ParentId = "5" },
            new (){ Id = "516", Text = "Preload", Href = "/preload", IconName = IconName.ArrowClockwise, ParentId = "5" },
            new (){ Id = "517", Text = "Progress", Href = "/progress", IconName = IconName.UsbC, ParentId = "5" },
            new (){ Id = "518", Text = "Sidebar", Href = "/sidebar", IconName = IconName.LayoutSidebar, ParentId = "5" },
            new (){ Id = "519", Text = "Tabs", Href = "/tabs", IconName = IconName.WindowPlus, ParentId = "5" },
            new (){ Id = "520", Text = "Toasts", Href = "/toasts", IconName = IconName.ExclamationTriangleFill, ParentId = "5" },
            new (){ Id = "521", Text = "Tooltips", Href = "/tooltips", IconName = IconName.ChatSquareDotsFill, ParentId = "5" },

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
