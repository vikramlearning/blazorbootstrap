namespace BlazorBootstrap.Demo.RCL;

public partial class MainLayout : MainLayoutBase
{
    internal override IEnumerable<NavItem> GetNavItems()
    {
        navItems ??= new List<NavItem>
        {
            new (){ Id = "1", Text = "Getting Started", Href = RouteConstants.Demos_GettingStarted_Documentation, IconName = IconName.HouseDoorFill },

            new (){ Id = "2", Text = "Layout", IconName = IconName.LayoutTextWindowReverse, IconColor = IconColor.Success },
            new (){ Id = "200", Text = "Blazor WebAssembly", Href = "/layout-setup/blazor-webassembly", IconName = IconName.BrowserEdge, ParentId = "2" },
            new (){ Id = "201", Text = "Blazor Server", Href = "/layout-setup/blazor-server", IconName = IconName.Server, ParentId = "2" },

            new (){ Id = "3", Text = "Content", IconName = IconName.BodyText, IconColor = IconColor.Primary },
            new (){ Id = "300", Text = "Icons", Href = RouteConstants.Demos_Icons_Documentation, IconName = IconName.PersonSquare, ParentId = "3" },
            new (){ Id = "301", Text = "Images", Href = RouteConstants.Demos_Images_Documentation, IconName = IconName.Image, ParentId = "3" },

            new (){ Id = "4", Text = "Forms", IconName = IconName.InputCursorText, IconColor = IconColor.Success },
            new (){ Id = "400", Text = "Auto Complete", Href = RouteConstants.Demos_AutoComplete_Documentation, IconName = IconName.InputCursorText, ParentId = "4" },
            new (){ Id = "400", Text = "Checkbox Input", Href = RouteConstants.Demos_CheckboxInput_Documentation, IconName = IconName.CheckSquareFill, ParentId = "4" },
            new (){ Id = "401", Text = "Currency Input", Href = RouteConstants.Demos_CurrencyInput_Documentation, IconName = IconName.CurrencyDollar, ParentId = "4" },
            new (){ Id = "402", Text = "Date Input", Href = RouteConstants.Demos_DateInput_Documentation, IconName = IconName.CalendarDate, ParentId = "4" },
            new (){ Id = "403", Text = "Number Input", Href = RouteConstants.Demos_NumberInput_Documentation, IconName = IconName.InputCursor, ParentId = "4" },
            new (){ Id = "403", Text = "Password Input", Href = RouteConstants.Demos_PasswordInput_Documentation, IconName = IconName.EyeSlashFill, ParentId = "4" },
            new (){ Id = "403", Text = "Radio Input", Href = RouteConstants.Demos_RadioInput_Documentation, IconName = IconName.RecordCircle, ParentId = "4" },
            new (){ Id = "404", Text = "Range Input", Href = RouteConstants.Demos_RangeInput_Documentation, IconName = IconName.Sliders, ParentId = "4" },
            //new (){ Id = "404", Text = "Select Input", Href = RouteConstants.Demos_SelectInput_Documentation, IconName = IconName.MenuButtonWideFill, ParentId = "4" },
            new (){ Id = "405", Text = "Switch", Href = RouteConstants.Demos_Switch_Documentation, IconName = IconName.ToggleOn, ParentId = "4" },
            new (){ Id = "406", Text = "Text Input", Href = RouteConstants.Demos_TextInput_Documentation, IconName = IconName.InputCursorText, ParentId = "4" },
            new (){ Id = "407", Text = "Text Area Input", Href = RouteConstants.Demos_TextAreaInput_Documentation, IconName = IconName.InputCursorText, ParentId = "4" },
            new (){ Id = "408", Text = "Time Input", Href = RouteConstants.Demos_TimeInput_Documentation, IconName = IconName.ClockFill, ParentId = "4" },

            new (){ Id = "5", Text = "Components", IconName = IconName.GearFill, IconColor = IconColor.Danger },
            new (){ Id = "500", Text = "Accordion", Href = RouteConstants.Demos_Accordion_Documentation, IconName = IconName.ChevronBarExpand, ParentId = "5" },
            new (){ Id = "501", Text = "Alerts", Href = RouteConstants.Demos_Alerts_Documentation, IconName = IconName.CheckCircleFill, ParentId = "5" },
            new (){ Id = "502", Text = "Badge", Href = RouteConstants.Demos_Badge_Documentation, IconName = IconName.AppIndicator, ParentId = "5" },
            new (){ Id = "503", Text = "Breadcrumb", Href = RouteConstants.Demos_Breadcrumb_Documentation, IconName = IconName.SegmentedNav, ParentId = "5" },
            new (){ Id = "504", Text = "Buttons", Href = RouteConstants.Demos_Buttons_Documentation, IconName = IconName.ToggleOn, ParentId = "5" },
            new (){ Id = "505", Text = "Callout", Href = RouteConstants.Demos_Callout_Documentation, IconName = IconName.StickyFill, ParentId = "5" },
            new (){ Id = "506", Text = "Card", Href = RouteConstants.Demos_Card_Documentation, IconName = IconName.CardHeading, ParentId = "5" },
            new (){ Id = "507", Text = "Carousel", Href = RouteConstants.Demos_Carousel_Documentation, IconName = IconName.CollectionPlayFill, ParentId = "5" },
            new (){ Id = "508", Text = "Charts", Href = RouteConstants.Demos_Charts_Documentation, IconName = IconName.BarChartLineFill, ParentId = "5", Match = NavLinkMatch.All },
            new (){ Id = "509", Text = "Collapse", Href = RouteConstants.Demos_Collapse_Documentation, IconName = IconName.ArrowsCollapse, ParentId = "5" },
            new (){ Id = "510", Text = "Confirm Dialog", Href = RouteConstants.Demos_ConfirmDialog_Documentation, IconName = IconName.QuestionDiamondFill, ParentId = "5" },
            new (){ Id = "511", Text = "Dropdown", Href = RouteConstants.Demos_Dropdown_Documentation, IconName = IconName.MenuButtonWideFill, ParentId = "5" },
            new (){ Id = "512", Text = "Google Map", Href = RouteConstants.Demos_GoogleMap_Documentation, IconName = IconName.Map, ParentId = "5" },

            #region Grid

            new (){ Id = "513", Text = "Grid", IconName = IconName.Grid, ParentId = "5" },
            new (){ Id = "51301", Text = "Overview", Href = RouteConstants.Demos_Grid_Overview_Documentation, IconName = IconName.Grid, ParentId = "513" }, // first item - do not change
            new (){ Id = "51302", Text = "Alignment", Href = RouteConstants.Demos_Grid_Alignment_Documentation, IconName = IconName.Justify, ParentId = "513" },
            new (){ Id = "51303", Text = "Custom CSS Class", Href = RouteConstants.Demos_Grid_CustomCSSClass_Documentation, IconName = IconName.FileTypeCss, ParentId = "513" },
            new (){ Id = "51304", Text = "Data Binding", Href = RouteConstants.Demos_Grid_DataBinding_Documentation, IconName = IconName.GridFill, ParentId = "513" },
            new (){ Id = "51306", Text = "Detail View", Href = RouteConstants.Demos_Grid_DetailView_Documentation, IconName = IconName.ListNested, ParentId = "513" },
            new (){ Id = "51307", Text = "Events", Href = RouteConstants.Demos_Grid_Events_Documentation, IconName = IconName.LightningChargeFill, ParentId = "513" },
            new (){ Id = "51307", Text = "Filters", Href = RouteConstants.Demos_Grid_Filters_Documentation, IconName = IconName.FunnelFill, ParentId = "513" },
            new (){ Id = "51308", Text = "Fixed Header", Href = RouteConstants.Demos_Grid_FixedHeader_Documentation, IconName = IconName.Table, ParentId = "513" },
            new (){ Id = "51309", Text = "Freeze Columns", Href = RouteConstants.Demos_Grid_FreezeColumns_Documentation, IconName = IconName.LayoutThreeColumns, ParentId = "513" },
            new (){ Id = "51310", Text = "Grid Settings", Href = RouteConstants.Demos_Grid_Settings_Documentation, IconName = IconName.GearFill, ParentId = "513" },
            new (){ Id = "51311", Text = "Nested Grid", Href = RouteConstants.Demos_Grid_NestedGrid_Documentation, IconName = IconName.Pip, ParentId = "513" },
            new (){ Id = "51312", Text = "Paging", Href = RouteConstants.Demos_Grid_Paging_Documentation, IconName = IconName.ChevronBarRight, ParentId = "513" },
            new (){ Id = "51313", Text = "Selection", Href = RouteConstants.Demos_Grid_Selection_Documentation, IconName = IconName.CheckSquareFill, ParentId = "513" },
            new (){ Id = "51314", Text = "Sorting", Href = RouteConstants.Demos_Grid_Sorting_Documentation, IconName = IconName.ArrowDownUp, ParentId = "513" },
            new (){ Id = "51315", Text = "Summary", Href = RouteConstants.Demos_Grid_Summary_Documentation, IconName = IconName.Calculator, ParentId = "513" },
            new (){ Id = "51316", Text = "Translations", Href = RouteConstants.Demos_Grid_Translations_Documentation, IconName = IconName.Translate, ParentId = "513" },
            new (){ Id = "51399", Text = "Other", Href = RouteConstants.Demos_Grid_OtherExamples_Documentation, IconName = IconName.PlusSquareFill, ParentId = "513" }, // last item - do not change

            #endregion Grid
                        
            new (){ Id = "514", Text = "Markdown", Href = RouteConstants.Demos_Markdown_Documentation, IconName = IconName.MarkdownFill, ParentId = "5" },
            new (){ Id = "514", Text = "Modals", Href = RouteConstants.Demos_Modal_Documentation, IconName = IconName.WindowStack, ParentId = "5" },
            new (){ Id = "515", Text = "Offcanvas", Href = RouteConstants.Demos_Offcanvas_Documentation, IconName = IconName.LayoutSidebarReverse, ParentId = "5" },
            new (){ Id = "516", Text = "Pagination", Href = RouteConstants.Demos_Pagination_Documentation, IconName = IconName.ThreeDots, ParentId = "5" },
            new (){ Id = "517", Text = "PDF Viewer", Href = RouteConstants.Demos_PDFViewer_Documentation, IconName = IconName.FilePdfFill, ParentId = "5" },
            new (){ Id = "518", Text = "Placeholders", Href = RouteConstants.Demos_Placeholders_Documentation, IconName = IconName.ColumnsGap, ParentId = "5" },
            new (){ Id = "519", Text = "Preload", Href = RouteConstants.Demos_Preload_Documentation, IconName = IconName.ArrowClockwise, ParentId = "5" },
            new (){ Id = "520", Text = "Progress", Href = RouteConstants.Demos_Progress_Documentation, IconName = IconName.UsbC, ParentId = "5" },
            new (){ Id = "521", Text = "Ribbon", Href = RouteConstants.Demos_Ribbon_Documentation, IconName = IconName.WindowStack, ParentId = "5" },
            new (){ Id = "522", Text = "Script Loader", Href = RouteConstants.Demos_ScriptLoader_Documentation, IconName = IconName.CodeSlash, ParentId = "5" },
            new (){ Id = "523", Text = "Sidebar", Href = RouteConstants.Demos_Sidebar_Documentation, IconName = IconName.LayoutSidebar, ParentId = "5" },
            new (){ Id = "524", Text = "Sidebar 2", Href = RouteConstants.Demos_Sidebar2_Documentation, IconName = IconName.ListNested, ParentId = "5" },
            new (){ Id = "525", Text = "Sortable List", Href = RouteConstants.Demos_SortableList_Documentation, IconName = IconName.ArrowsMove, ParentId = "5" },
            new (){ Id = "526", Text = "Spinner", Href = RouteConstants.Demos_Spinners_Documentation, IconName = IconName.ArrowRepeat, ParentId = "5" },
            new (){ Id = "527", Text = "Tabs", Href = RouteConstants.Demos_Tabs_Documentation, IconName = IconName.WindowPlus, ParentId = "5" },
            new (){ Id = "528", Text = "Theme Switcher", Href = RouteConstants.Demos_ThemeSwitcher_Documentation, IconName = IconName.NintendoSwitch, ParentId = "5" },
            new (){ Id = "529", Text = "Toasts", Href = RouteConstants.Demos_Toasts_Documentation, IconName = IconName.ExclamationTriangleFill, ParentId = "5" },
            new (){ Id = "530", Text = "Tooltips", Href = RouteConstants.Demos_Tooltips_Documentation, IconName = IconName.ChatSquareDotsFill, ParentId = "5" },

            new (){ Id = "6", Text = "Data Visualization", IconName = IconName.BarChartFill, IconColor = IconColor.Warning },
            new (){ Id = "600", Text = "Bar Chart", Href = RouteConstants.Demos_BarChart_Documentation, IconName = IconName.BarChartFill, ParentId = "6", Match = NavLinkMatch.All },
            new (){ Id = "601", Text = "Doughnut Chart", Href = RouteConstants.Demos_DoughnutChart_Documentation, IconName = IconName.CircleFill, ParentId = "6", Match = NavLinkMatch.All },
            new (){ Id = "602", Text = "Line Chart", Href = RouteConstants.Demos_LineChart_Documentation, IconName = IconName.GraphUp, ParentId = "6", Match = NavLinkMatch.All },
            new (){ Id = "603", Text = "Pie Chart", Href = RouteConstants.Demos_PieChart_Documentation, IconName = IconName.PieChartFill, ParentId = "6", Match = NavLinkMatch.All },
            new (){ Id = "604", Text = "Polar Area Chart", Href = RouteConstants.Demos_PolarAreaChart_Documentation, IconName = IconName.PieChartFill, ParentId = "6", Match = NavLinkMatch.All },
            new (){ Id = "605", Text = "Radar Chart", Href = RouteConstants.Demos_RadarChart_Documentation, IconName = IconName.Radar, ParentId = "6", Match = NavLinkMatch.All },
            new (){ Id = "605", Text = "Scatter Chart", Href = RouteConstants.Demos_ScatterChart_Documentation, IconName = IconName.GraphUpArrow, ParentId = "6", Match = NavLinkMatch.All },

            new(){ Id = "7", Text = "Services", IconName = IconName.WrenchAdjustableCircleFill, IconColor = IconColor.Success },
            new (){ Id = "700", Text = "Modal Service", Href = RouteConstants.Demos_ModalService_Documentation, IconName = IconName.WindowStack, ParentId = "7" },

            new(){ Id = "19", Text = "Utilities", IconName = IconName.GearWideConnected, IconColor = IconColor.Info },
            new (){ Id = "1900", Text = "Color Utility", Href = RouteConstants.Demos_ColorUtils_Documentation, IconName = IconName.Palette2, ParentId = "19" },
        };

        return navItems;
    }

    private async ValueTask OnThemeChanged(string themeName)
        => await JS.InvokeVoidAsync("updateDemoCodeThemeCss", themeName);
}
