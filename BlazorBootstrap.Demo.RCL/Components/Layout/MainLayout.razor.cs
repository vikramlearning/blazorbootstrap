namespace BlazorBootstrap.Demo.RCL;

public partial class MainLayout : MainLayoutBase
{
    internal override IEnumerable<NavItem> GetNavItems()
    {
        navItems ??= new List<NavItem>
        {
            new (){ Id = "1", Text = "Getting Started", Href = DemoRouteConstants.Demos_URL_GettingStarted, IconName = IconName.HouseDoorFill },

            new (){ Id = "2", Text = "Layout", IconName = IconName.LayoutTextWindowReverse, IconColor = IconColor.Success },
            new (){ Id = "200", Text = "Blazor WebAssembly", Href = DemoRouteConstants.Demos_URL_Layout_BlazorWebAssembly, IconName = IconName.BrowserEdge, ParentId = "2" },
            new (){ Id = "201", Text = "Blazor Server", Href = DemoRouteConstants.Demos_URL_Layout_Blazor_Server, IconName = IconName.Server, ParentId = "2" },

            new (){ Id = "3", Text = "Content", IconName = IconName.BodyText, IconColor = IconColor.Primary },
            new (){ Id = "300", Text = "Icons", Href = DemoRouteConstants.Demos_URL_Icons, IconName = IconName.PersonSquare, ParentId = "3" },
            new (){ Id = "301", Text = "Images", Href = DemoRouteConstants.Demos_URL_Images, IconName = IconName.Image, ParentId = "3" },

            new (){ Id = "4", Text = "Forms", IconName = IconName.InputCursorText, IconColor = IconColor.Success },
            new (){ Id = "400", Text = "Auto Complete", Href = DemoRouteConstants.Demos_URL_AutoComplete, IconName = IconName.InputCursorText, ParentId = "4" },
            new (){ Id = "401", Text = "Checkbox Input", Href = DemoRouteConstants.Demos_URL_CheckboxInput, IconName = IconName.CheckSquareFill, ParentId = "4" },
            new (){ Id = "402", Text = "Currency Input", Href = DemoRouteConstants.Demos_URL_CurrencyInput, IconName = IconName.CurrencyDollar, ParentId = "4" },
            new (){ Id = "403", Text = "Date Input", Href = DemoRouteConstants.Demos_URL_DateInput, IconName = IconName.CalendarDate, ParentId = "4" },
            new (){ Id = "404", Text = "Enum Input", Href = DemoRouteConstants.Demos_URL_EnumInput, IconName = IconName.MenuButtonWideFill, ParentId = "4" },
            new (){ Id = "405", Text = "Number Input", Href = DemoRouteConstants.Demos_URL_NumberInput, IconName = IconName.InputCursor, ParentId = "4" },
            new (){ Id = "406", Text = "Password Input", Href = DemoRouteConstants.Demos_URL_PasswordInput, IconName = IconName.EyeSlashFill, ParentId = "4" },
            new (){ Id = "407", Text = "Radio Input", Href = DemoRouteConstants.Demos_URL_RadioInput, IconName = IconName.RecordCircle, ParentId = "4" },
            new (){ Id = "408", Text = "Range Input", Href = DemoRouteConstants.Demos_URL_RangeInput, IconName = IconName.Sliders, ParentId = "4" },
            //new (){ Id = "404", Text = "Select Input", Href = DemoRouteConstants.Demos_URL_SelectInput, IconName = IconName.MenuButtonWideFill, ParentId = "4" },
            new (){ Id = "409", Text = "Switch", Href = DemoRouteConstants.Demos_URL_Switch, IconName = IconName.ToggleOn, ParentId = "4" },
            new (){ Id = "410", Text = "Text Input", Href = DemoRouteConstants.Demos_URL_TextInput, IconName = IconName.InputCursorText, ParentId = "4" },
            new (){ Id = "411", Text = "Text Area Input", Href = DemoRouteConstants.Demos_URL_TextAreaInput, IconName = IconName.InputCursorText, ParentId = "4" },
            new (){ Id = "412", Text = "Time Input", Href = DemoRouteConstants.Demos_URL_TimeInput, IconName = IconName.ClockFill, ParentId = "4" },

            new (){ Id = "5", Text = "Components", IconName = IconName.GearFill, IconColor = IconColor.Danger },
            new (){ Id = "500", Text = "Accordion", Href = DemoRouteConstants.Demos_URL_Accordion, IconName = IconName.ChevronBarExpand, ParentId = "5" },
            new (){ Id = "501", Text = "Alerts", Href = DemoRouteConstants.Demos_URL_Alerts, IconName = IconName.CheckCircleFill, ParentId = "5" },
            new (){ Id = "502", Text = "Badge", Href = DemoRouteConstants.Demos_URL_Badge, IconName = IconName.AppIndicator, ParentId = "5" },
            new (){ Id = "503", Text = "Breadcrumb", Href = DemoRouteConstants.Demos_URL_Breadcrumb, IconName = IconName.SegmentedNav, ParentId = "5" },
            new (){ Id = "504", Text = "Buttons", Href = DemoRouteConstants.Demos_URL_Buttons, IconName = IconName.ToggleOn, ParentId = "5" },
            new (){ Id = "505", Text = "Callout", Href = DemoRouteConstants.Demos_URL_Callout, IconName = IconName.StickyFill, ParentId = "5" },
            new (){ Id = "506", Text = "Card", Href = DemoRouteConstants.Demos_URL_Card, IconName = IconName.CardHeading, ParentId = "5" },
            new (){ Id = "507", Text = "Carousel", Href = DemoRouteConstants.Demos_URL_Carousel, IconName = IconName.CollectionPlayFill, ParentId = "5" },
            new (){ Id = "508", Text = "Charts", Href = DemoRouteConstants.Demos_URL_Charts, IconName = IconName.BarChartLineFill, ParentId = "5", Match = NavLinkMatch.All },
            new (){ Id = "509", Text = "Collapse", Href = DemoRouteConstants.Demos_URL_Collapse, IconName = IconName.ArrowsCollapse, ParentId = "5" },
            new (){ Id = "510", Text = "Confirm Dialog", Href = DemoRouteConstants.Demos_URL_ConfirmDialog, IconName = IconName.QuestionDiamondFill, ParentId = "5" },
            new (){ Id = "511", Text = "Dropdown", Href = DemoRouteConstants.Demos_URL_Dropdown, IconName = IconName.MenuButtonWideFill, ParentId = "5" },
            new (){ Id = "512", Text = "Google Map", Href = DemoRouteConstants.Demos_URL_GoogleMap, IconName = IconName.Map, ParentId = "5" },

            #region Grid

            new (){ Id = "513", Text = "Grid", IconName = IconName.Grid, ParentId = "5" },
            new (){ Id = "51301", Text = "Overview", Href = DemoRouteConstants.Demos_URL_Grid_Overview, IconName = IconName.Grid, ParentId = "513" }, // first item - do not change
            new (){ Id = "51302", Text = "Alignment", Href = DemoRouteConstants.Demos_URL_Grid_Alignment, IconName = IconName.Justify, ParentId = "513" },
            new (){ Id = "51303", Text = "Custom CSS Class", Href = DemoRouteConstants.Demos_URL_Grid_CustomCSSClass, IconName = IconName.FileTypeCss, ParentId = "513" },
            new (){ Id = "51304", Text = "Data Binding", Href = DemoRouteConstants.Demos_URL_Grid_DataBinding, IconName = IconName.GridFill, ParentId = "513" },
            new (){ Id = "51306", Text = "Detail View", Href = DemoRouteConstants.Demos_URL_Grid_DetailView, IconName = IconName.ListNested, ParentId = "513" },
            new (){ Id = "51307", Text = "Events", Href = DemoRouteConstants.Demos_URL_Grid_Events, IconName = IconName.LightningChargeFill, ParentId = "513" },
            new (){ Id = "51307", Text = "Filters", Href = DemoRouteConstants.Demos_URL_Grid_Filters, IconName = IconName.FunnelFill, ParentId = "513" },
            new (){ Id = "51308", Text = "Fixed Header", Href = DemoRouteConstants.Demos_URL_Grid_FixedHeader, IconName = IconName.Table, ParentId = "513" },
            new (){ Id = "51309", Text = "Freeze Columns", Href = DemoRouteConstants.Demos_URL_Grid_FreezeColumns, IconName = IconName.LayoutThreeColumns, ParentId = "513" },
            new (){ Id = "51310", Text = "Grid Settings", Href = DemoRouteConstants.Demos_URL_Grid_Settings, IconName = IconName.GearFill, ParentId = "513" },
            new (){ Id = "51311", Text = "Nested Grid", Href = DemoRouteConstants.Demos_URL_Grid_NestedGrid, IconName = IconName.Pip, ParentId = "513" },
            new (){ Id = "51312", Text = "Paging", Href = DemoRouteConstants.Demos_URL_Grid_Paging, IconName = IconName.ChevronBarRight, ParentId = "513" },
            new (){ Id = "51313", Text = "Selection", Href = DemoRouteConstants.Demos_URL_Grid_Selection, IconName = IconName.CheckSquareFill, ParentId = "513" },
            new (){ Id = "51314", Text = "Sorting", Href = DemoRouteConstants.Demos_URL_Grid_Sorting, IconName = IconName.ArrowDownUp, ParentId = "513" },
            new (){ Id = "51315", Text = "Summary", Href = DemoRouteConstants.Demos_URL_Grid_Summary, IconName = IconName.Calculator, ParentId = "513" },
            new (){ Id = "51316", Text = "Translations", Href = DemoRouteConstants.Demos_URL_Grid_Translations, IconName = IconName.Translate, ParentId = "513" },
            new (){ Id = "51399", Text = "Other", Href = DemoRouteConstants.Demos_URL_Grid_OtherExamples, IconName = IconName.PlusSquareFill, ParentId = "513" }, // last item - do not change

            #endregion Grid
                        
            new (){ Id = "514", Text = "Markdown", Href = DemoRouteConstants.Demos_URL_Markdown, IconName = IconName.MarkdownFill, ParentId = "5" },
            new (){ Id = "514", Text = "Modals", Href = DemoRouteConstants.Demos_URL_Modal, IconName = IconName.WindowStack, ParentId = "5" },
            new (){ Id = "515", Text = "Offcanvas", Href = DemoRouteConstants.Demos_URL_Offcanvas, IconName = IconName.LayoutSidebarReverse, ParentId = "5" },
            new (){ Id = "516", Text = "Pagination", Href = DemoRouteConstants.Demos_URL_Pagination, IconName = IconName.ThreeDots, ParentId = "5" },
            new (){ Id = "517", Text = "PDF Viewer", Href = DemoRouteConstants.Demos_URL_PDFViewer, IconName = IconName.FilePdfFill, ParentId = "5" },
            new (){ Id = "518", Text = "Placeholders", Href = DemoRouteConstants.Demos_URL_Placeholders, IconName = IconName.ColumnsGap, ParentId = "5" },
            new (){ Id = "519", Text = "Preload", Href = DemoRouteConstants.Demos_URL_Preload, IconName = IconName.ArrowClockwise, ParentId = "5" },
            new (){ Id = "520", Text = "Progress", Href = DemoRouteConstants.Demos_URL_Progress, IconName = IconName.UsbC, ParentId = "5" },
            new (){ Id = "521", Text = "Ribbon", Href = DemoRouteConstants.Demos_URL_Ribbon, IconName = IconName.WindowStack, ParentId = "5" },
            new (){ Id = "522", Text = "Script Loader", Href = DemoRouteConstants.Demos_URL_ScriptLoader, IconName = IconName.CodeSlash, ParentId = "5" },
            new (){ Id = "523", Text = "Sidebar", Href = DemoRouteConstants.Demos_URL_Sidebar, IconName = IconName.LayoutSidebar, ParentId = "5" },
            new (){ Id = "524", Text = "Sidebar 2", Href = DemoRouteConstants.Demos_URL_Sidebar2, IconName = IconName.ListNested, ParentId = "5" },
            new (){ Id = "525", Text = "Sortable List", Href = DemoRouteConstants.Demos_URL_SortableList, IconName = IconName.ArrowsMove, ParentId = "5" },
            new (){ Id = "526", Text = "Spinner", Href = DemoRouteConstants.Demos_URL_Spinners, IconName = IconName.ArrowRepeat, ParentId = "5" },
            new (){ Id = "527", Text = "Tabs", Href = DemoRouteConstants.Demos_URL_Tabs, IconName = IconName.WindowPlus, ParentId = "5" },
            new (){ Id = "528", Text = "Theme Switcher", Href = DemoRouteConstants.Demos_URL_ThemeSwitcher, IconName = IconName.NintendoSwitch, ParentId = "5" },
            new (){ Id = "529", Text = "Toasts", Href = DemoRouteConstants.Demos_URL_Toasts, IconName = IconName.ExclamationTriangleFill, ParentId = "5" },
            new (){ Id = "530", Text = "Tooltips", Href = DemoRouteConstants.Demos_URL_Tooltips, IconName = IconName.ChatSquareDotsFill, ParentId = "5" },

            new (){ Id = "6", Text = "Data Visualization", IconName = IconName.BarChartFill, IconColor = IconColor.Warning },
            new (){ Id = "600", Text = "Bar Chart", Href = DemoRouteConstants.Demos_URL_BarChart, IconName = IconName.BarChartFill, ParentId = "6", Match = NavLinkMatch.All },
            new (){ Id = "601", Text = "Doughnut Chart", Href = DemoRouteConstants.Demos_URL_DoughnutChart, IconName = IconName.CircleFill, ParentId = "6", Match = NavLinkMatch.All },
            new (){ Id = "602", Text = "Line Chart", Href = DemoRouteConstants.Demos_URL_LineChart, IconName = IconName.GraphUp, ParentId = "6", Match = NavLinkMatch.All },
            new (){ Id = "603", Text = "Pie Chart", Href = DemoRouteConstants.Demos_URL_PieChart, IconName = IconName.PieChartFill, ParentId = "6", Match = NavLinkMatch.All },
            new (){ Id = "604", Text = "Polar Area Chart", Href = DemoRouteConstants.Demos_URL_PolarAreaChart, IconName = IconName.PieChartFill, ParentId = "6", Match = NavLinkMatch.All },
            new (){ Id = "605", Text = "Radar Chart", Href = DemoRouteConstants.Demos_URL_RadarChart, IconName = IconName.Radar, ParentId = "6", Match = NavLinkMatch.All },
            new (){ Id = "605", Text = "Scatter Chart", Href = DemoRouteConstants.Demos_URL_ScatterChart, IconName = IconName.GraphUpArrow, ParentId = "6", Match = NavLinkMatch.All },

            new(){ Id = "7", Text = "Services", IconName = IconName.WrenchAdjustableCircleFill, IconColor = IconColor.Success },
            new (){ Id = "700", Text = "Modal Service", Href = DemoRouteConstants.Demos_URL_ModalService, IconName = IconName.WindowStack, ParentId = "7" },

            new(){ Id = "19", Text = "Utilities", IconName = IconName.GearWideConnected, IconColor = IconColor.Info },
            new (){ Id = "1900", Text = "Color Utility", Href = DemoRouteConstants.Demos_URL_ColorUtils, IconName = IconName.Palette2, ParentId = "19" },
        };

        return navItems;
    }
}
