namespace BlazorBootstrap.Demo.RCL.Components.Pages.Home;

/// <summary>
/// The components the home page lists, grouped by the demo navigation's categories.
/// Keep this list in sync with the demo navigation (rules.md: component discovery and navigation synchronization).
/// </summary>
internal static class HomeCatalog
{
    #region Properties, Indexers

    /// <summary>
    /// Gets the catalog categories in navigation order.
    /// </summary>
    public static IReadOnlyList<HomeCatalogCategory> Categories { get; } = new List<HomeCatalogCategory>
    {
        new("Content", new List<HomeCatalogItem>
        {
            new("Icons", DemoRouteConstants.Demos_URL_Icons, IconName.PersonSquare, "Bootstrap Icons by name.", HomeCatalogBadge.None),
            new("Images", DemoRouteConstants.Demos_URL_Images, IconName.Image, "Responsive and rounded images.", HomeCatalogBadge.None)
        }),
        new("Forms", new List<HomeCatalogItem>
        {
            new("Auto Complete", DemoRouteConstants.Demos_URL_AutoComplete, IconName.InputCursorText, "Suggestions as you type.", HomeCatalogBadge.None),
            new("Checkbox Input", DemoRouteConstants.Demos_URL_CheckboxInput, IconName.CheckSquareFill, "A bound true or false value.", HomeCatalogBadge.None),
            new("Currency Input", DemoRouteConstants.Demos_URL_CurrencyInput, IconName.CurrencyDollar, "Money amounts for any locale.", HomeCatalogBadge.Updated),
            new("Date Input", DemoRouteConstants.Demos_URL_DateInput, IconName.CalendarDate, "Typed or picked dates.", HomeCatalogBadge.None),
            new("Drag and Drop File Input", DemoRouteConstants.Demos_URL_DragAndDropFileInput, IconName.FileEarmarkArrowUp, "Drop files to upload them.", HomeCatalogBadge.New),
            new("Input Group", DemoRouteConstants.Demos_URL_InputGroup, IconName.UiChecksGrid, "Text and buttons around inputs.", HomeCatalogBadge.New),
            new("Enum Input", DemoRouteConstants.Demos_URL_EnumInput, IconName.MenuButtonWideFill, "Pick a value from an enum.", HomeCatalogBadge.None),
            new("File Input", DemoRouteConstants.Demos_URL_FileInput, IconName.FileEarmarkArrowUp, "Upload files with validation.", HomeCatalogBadge.New),
            new("Number Input", DemoRouteConstants.Demos_URL_NumberInput, IconName.InputCursor, "Numbers with limits and steps.", HomeCatalogBadge.None),
            new("OTP Input", DemoRouteConstants.Demos_URL_OTPInput, IconName.Asterisk, "One-time code entry boxes.", HomeCatalogBadge.New),
            new("Password Input", DemoRouteConstants.Demos_URL_PasswordInput, IconName.EyeSlashFill, "Passwords you can show or hide.", HomeCatalogBadge.None),
            new("Radio Input", DemoRouteConstants.Demos_URL_RadioInput, IconName.RecordCircle, "One choice from several.", HomeCatalogBadge.None),
            new("Rating Input", DemoRouteConstants.Demos_URL_RatingInput, IconName.StarHalf, "Star ratings.", HomeCatalogBadge.New),
            new("Range Input", DemoRouteConstants.Demos_URL_RangeInput, IconName.Sliders, "A slider for a value.", HomeCatalogBadge.None),
            new("RichTextEditor", DemoRouteConstants.Demos_URL_RichTextEditor, IconName.Type, "Formatted text editing.", HomeCatalogBadge.New),
            new("Switch", DemoRouteConstants.Demos_URL_Switch, IconName.ToggleOn, "An on and off toggle.", HomeCatalogBadge.None),
            new("Text Area Input", DemoRouteConstants.Demos_URL_TextAreaInput, IconName.InputCursorText, "Multi-line text.", HomeCatalogBadge.None),
            new("Text Input", DemoRouteConstants.Demos_URL_TextInput, IconName.InputCursorText, "Single-line text.", HomeCatalogBadge.None),
            new("Time Input", DemoRouteConstants.Demos_URL_TimeInput, IconName.ClockFill, "Typed or picked times.", HomeCatalogBadge.None)
        }),
        new("Components", new List<HomeCatalogItem>
        {
            new("Accordion", DemoRouteConstants.Demos_URL_Accordion, IconName.ChevronBarExpand, "Collapsible content panels.", HomeCatalogBadge.None),
            new("Alerts", DemoRouteConstants.Demos_URL_Alerts, IconName.CheckCircleFill, "Contextual feedback messages.", HomeCatalogBadge.None),
            new("Avatar", DemoRouteConstants.Demos_URL_Avatar, IconName.PersonCircle, "Initials, images and groups.", HomeCatalogBadge.New),
            new("Badge", DemoRouteConstants.Demos_URL_Badge, IconName.AppIndicator, "Counts and labels.", HomeCatalogBadge.None),
            new("Breadcrumb", DemoRouteConstants.Demos_URL_Breadcrumb, IconName.SegmentedNav, "Show where you are.", HomeCatalogBadge.None),
            new("Buttons", DemoRouteConstants.Demos_URL_Buttons, IconName.ToggleOn, "Actions in every style.", HomeCatalogBadge.None),
            new("Button Group", DemoRouteConstants.Demos_URL_ButtonGroup, IconName.Toggles, "Buttons grouped together.", HomeCatalogBadge.New),
            new("Calendar", DemoRouteConstants.Demos_URL_Calendar, IconName.CalendarEvent, "Month, week and day views.", HomeCatalogBadge.New),
            new("Callout", DemoRouteConstants.Demos_URL_Callout, IconName.StickyFill, "Highlighted notes.", HomeCatalogBadge.None),
            new("Card", DemoRouteConstants.Demos_URL_Card, IconName.CardHeading, "A flexible content container.", HomeCatalogBadge.None),
            new("Carousel", DemoRouteConstants.Demos_URL_Carousel, IconName.CollectionPlayFill, "Slides with controls.", HomeCatalogBadge.None),
            new("Charts", DemoRouteConstants.Demos_URL_Charts, IconName.BarChartLineFill, "Chart.js charts in C#.", HomeCatalogBadge.Updated),
            new("Collapse", DemoRouteConstants.Demos_URL_Collapse, IconName.ArrowsCollapse, "Show and hide content.", HomeCatalogBadge.None),
            new("Confirm Dialog", DemoRouteConstants.Demos_URL_ConfirmDialog, IconName.QuestionDiamondFill, "Ask before an action runs.", HomeCatalogBadge.Updated),
            new("Dropdown", DemoRouteConstants.Demos_URL_Dropdown, IconName.MenuButtonWideFill, "Menus from a toggle.", HomeCatalogBadge.Updated),
            new("Google Map", DemoRouteConstants.Demos_URL_GoogleMap, IconName.Map, "Maps with markers.", HomeCatalogBadge.Updated),
            new("Grid", DemoRouteConstants.Demos_URL_Grid_Overview, IconName.Grid, "Paging, sorting and filters.", HomeCatalogBadge.Updated),
            new("List Group", DemoRouteConstants.Demos_URL_ListGroup, IconName.ListUl, "Lists of items and actions.", HomeCatalogBadge.New),
            new("Markdown", DemoRouteConstants.Demos_URL_Markdown, IconName.MarkdownFill, "Render Markdown as HTML.", HomeCatalogBadge.None),
            new("Mind Map", DemoRouteConstants.Demos_URL_MindMap, IconName.Diagram3, "Ideas as linked nodes.", HomeCatalogBadge.None),
            new("Modals", DemoRouteConstants.Demos_URL_Modal, IconName.WindowStack, "Dialogs over the page.", HomeCatalogBadge.None),
            new("Navbar", DemoRouteConstants.Demos_URL_Navbar, IconName.LayoutTextWindow, "A responsive top bar.", HomeCatalogBadge.New),
            new("Offcanvas", DemoRouteConstants.Demos_URL_Offcanvas, IconName.LayoutSidebarReverse, "Panels that slide in.", HomeCatalogBadge.None),
            new("Pagination", DemoRouteConstants.Demos_URL_Pagination, IconName.ThreeDots, "Page through long lists.", HomeCatalogBadge.None),
            new("PDF Viewer", DemoRouteConstants.Demos_URL_PDFViewer, IconName.FilePdfFill, "View PDFs in the app.", HomeCatalogBadge.Updated),
            new("Placeholders", DemoRouteConstants.Demos_URL_Placeholders, IconName.ColumnsGap, "Loading skeletons.", HomeCatalogBadge.None),
            new("Popover", DemoRouteConstants.Demos_URL_Popover, IconName.ChatSquareText, "Rich pop-up content.", HomeCatalogBadge.New),
            new("Preload", DemoRouteConstants.Demos_URL_Preload, IconName.ArrowClockwise, "A page loading overlay.", HomeCatalogBadge.None),
            new("Progress", DemoRouteConstants.Demos_URL_Progress, IconName.UsbC, "Progress bars.", HomeCatalogBadge.None),
            new("Ribbon", DemoRouteConstants.Demos_URL_Ribbon, IconName.WindowStack, "An Office-style toolbar.", HomeCatalogBadge.None),
            new("Script Loader", DemoRouteConstants.Demos_URL_ScriptLoader, IconName.CodeSlash, "Load scripts on demand.", HomeCatalogBadge.None),
            new("Sidebar", DemoRouteConstants.Demos_URL_Sidebar, IconName.LayoutSidebar, "App side navigation.", HomeCatalogBadge.Updated),
            new("Sidebar 2", DemoRouteConstants.Demos_URL_Sidebar2, IconName.ListNested, "Nested side navigation.", HomeCatalogBadge.Updated),
            new("Sortable List", DemoRouteConstants.Demos_URL_SortableList, IconName.ArrowsMove, "Drag items to reorder.", HomeCatalogBadge.Updated),
            new("Spinner", DemoRouteConstants.Demos_URL_Spinners, IconName.ArrowRepeat, "Busy indicators.", HomeCatalogBadge.None),
            new("Split View", DemoRouteConstants.Demos_URL_SplitView, IconName.LayoutSplit, "Resizable panes.", HomeCatalogBadge.New),
            new("Table", DemoRouteConstants.Demos_URL_Table, IconName.Table, "Read-only data tables.", HomeCatalogBadge.New),
            new("Tabs", DemoRouteConstants.Demos_URL_Tabs, IconName.WindowPlus, "Tabbed content.", HomeCatalogBadge.None),
            new("Theme Switcher", DemoRouteConstants.Demos_URL_ThemeSwitcher, IconName.NintendoSwitch, "Light, dark and auto themes.", HomeCatalogBadge.None),
            new("Timeline", DemoRouteConstants.Demos_URL_Timeline, IconName.ClockHistory, "Events in order.", HomeCatalogBadge.New),
            new("Toasts", DemoRouteConstants.Demos_URL_Toasts, IconName.ExclamationTriangleFill, "Brief notifications.", HomeCatalogBadge.Updated),
            new("Tooltips", DemoRouteConstants.Demos_URL_Tooltips, IconName.ChatSquareDotsFill, "Hints on hover and focus.", HomeCatalogBadge.Updated)
        }),
        new("Data Visualization", new List<HomeCatalogItem>
        {
            new("Bar Chart", DemoRouteConstants.Demos_URL_BarChart, IconName.BarChartFill, "Compare values by category.", HomeCatalogBadge.None),
            new("Bubble Chart", DemoRouteConstants.Demos_URL_BubbleChart, IconName.CircleFill, "Points with a third value.", HomeCatalogBadge.None),
            new("Doughnut Chart", DemoRouteConstants.Demos_URL_DoughnutChart, IconName.CircleFill, "Parts of a whole.", HomeCatalogBadge.None),
            new("Line Chart", DemoRouteConstants.Demos_URL_LineChart, IconName.GraphUp, "Trends over time.", HomeCatalogBadge.None),
            new("Pie Chart", DemoRouteConstants.Demos_URL_PieChart, IconName.PieChartFill, "Shares of a total.", HomeCatalogBadge.None),
            new("Polar Area Chart", DemoRouteConstants.Demos_URL_PolarAreaChart, IconName.PieChartFill, "Values on a radial scale.", HomeCatalogBadge.None),
            new("Radar Chart", DemoRouteConstants.Demos_URL_RadarChart, IconName.Radar, "Compare across many axes.", HomeCatalogBadge.None),
            new("Scatter Chart", DemoRouteConstants.Demos_URL_ScatterChart, IconName.GraphUpArrow, "Points on two axes.", HomeCatalogBadge.None)
        }),
        new("Services", new List<HomeCatalogItem>
        {
            new("Breadcrumb Service", DemoRouteConstants.Demos_URL_BreadcrumbService, IconName.SegmentedNav, "Set breadcrumbs from code.", HomeCatalogBadge.None),
            new("Modal Service", DemoRouteConstants.Demos_URL_ModalService, IconName.WindowStack, "Open modals from code.", HomeCatalogBadge.None),
            new("Preload Service", DemoRouteConstants.Demos_URL_PreloadService, IconName.ArrowRepeat, "Show loading from code.", HomeCatalogBadge.None),
            new("Toast Service", DemoRouteConstants.Demos_URL_ToastService, IconName.InfoCircleFill, "Show toasts from code.", HomeCatalogBadge.None)
        }),
        new("Utilities", new List<HomeCatalogItem>
        {
            new("Color Utility", DemoRouteConstants.Demos_URL_ColorUtils, IconName.Palette2, "Color helpers for components.", HomeCatalogBadge.None)
        })
    };

    /// <summary>
    /// Gets the number of components in the catalog.
    /// </summary>
    public static int TotalCount { get; } = Categories.Sum(category => category.Items.Count);

    #endregion
}

/// <summary>
/// A catalog category and its components.
/// </summary>
internal sealed record HomeCatalogCategory(string Name, IReadOnlyList<HomeCatalogItem> Items);

/// <summary>
/// One component in the home page catalog.
/// </summary>
internal sealed record HomeCatalogItem(string Name, string Href, IconName Icon, string Description, HomeCatalogBadge Badge);

/// <summary>
/// The status badge shown on a catalog card.
/// </summary>
internal enum HomeCatalogBadge
{
    None,
    New,
    Updated
}
