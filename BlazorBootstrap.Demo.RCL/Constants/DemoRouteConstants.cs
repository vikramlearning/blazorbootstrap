namespace BlazorBootstrap.Demo.RCL;

public static class DemoRouteConstants
{
    public const string Blog_URL_Prefix = "/blog";
    public const string Demos_URL_Prefix = "/demos";
    public const string Docs_URL_Prefix = "/docs";

    #region Demos

    // Getting Started
    public const string Demos_URL_GettingStarted = Demos_URL_Prefix + "/getting-started";

    // Layout
    public const string Demos_URL_Layout_Prefix = Demos_URL_Prefix + "/layout-setup";
    public const string Demos_URL_Layout_BlazorWebAssembly = Demos_URL_Layout_Prefix + "/blazor-webassembly";
    public const string Demos_URL_Layout_Blazor_Server = Demos_URL_Layout_Prefix + "/blazor-server";

    // AI
    //public const string Demos_URL_AI_Prefix = Demos_URL_Prefix + "/ai";
    //public const string Demos_URL_AI_Chat = Demos_URL_AI_Prefix + "/open-ai-chat";

    // Content
    public const string Demos_URL_Icons = Demos_URL_Prefix + "/icons";
    public const string Demos_URL_Images = Demos_URL_Prefix + "/images";

    // Forms
    public const string Demos_URL_Forms_Prefix = Demos_URL_Prefix + "/form";
    public const string Demos_URL_AutoComplete = Demos_URL_Forms_Prefix + "/autocomplete";
    public const string Demos_URL_CheckboxInput = Demos_URL_Forms_Prefix + "/checkbox-input";
    public const string Demos_URL_CurrencyInput = Demos_URL_Forms_Prefix + "/currency-input";
    public const string Demos_URL_DateInput = Demos_URL_Forms_Prefix + "/date-input";
    public const string Demos_URL_EnumInput = Demos_URL_Forms_Prefix + "/enum-input";
    public const string Demos_URL_NumberInput = Demos_URL_Forms_Prefix + "/number-input";
    public const string Demos_URL_PasswordInput = Demos_URL_Forms_Prefix + "/password-input";
    public const string Demos_URL_RadioInput = Demos_URL_Forms_Prefix + "/radio-input";
    public const string Demos_URL_RangeInput = Demos_URL_Forms_Prefix + "/range-input";
    public const string Demos_URL_SelectInput = Demos_URL_Forms_Prefix + "/select-input";
    public const string Demos_URL_Switch = Demos_URL_Forms_Prefix + "/switch";
    public const string Demos_URL_TextInput = Demos_URL_Forms_Prefix + "/text-input";
    public const string Demos_URL_TextAreaInput = Demos_URL_Forms_Prefix + "/text-area-input";
    public const string Demos_URL_TimeInput = Demos_URL_Forms_Prefix + "/time-input";

    // Components
    public const string Demos_URL_Accordion = Demos_URL_Prefix + "/accordion";
    public const string Demos_URL_Alerts = Demos_URL_Prefix + "/alerts";
    public const string Demos_URL_Badge = Demos_URL_Prefix + "/badge";
    public const string Demos_URL_Breadcrumb = Demos_URL_Prefix + "/breadcrumb";
    public const string Demos_URL_Buttons = Demos_URL_Prefix + "/buttons";
    public const string Demos_URL_Callout = Demos_URL_Prefix + "/callout";
    public const string Demos_URL_Card = Demos_URL_Prefix + "/card";
    public const string Demos_URL_Carousel = Demos_URL_Prefix + "/carousel";
    public const string Demos_URL_Charts = Demos_URL_Prefix + "/charts";
    public const string Demos_URL_Collapse = Demos_URL_Prefix + "/collapse";
    public const string Demos_URL_ConfirmDialog = Demos_URL_Prefix + "/confirm-dialog";
    public const string Demos_URL_Dropdown = Demos_URL_Prefix + "/dropdown";
    public const string Demos_URL_GoogleMap = Demos_URL_Prefix + "/google-map";

    #region Grid
    public const string Demos_URL_Grid_Prefix = Demos_URL_Prefix + "/grid";
    public const string Demos_URL_Grid_Overview = Demos_URL_Grid_Prefix + "/overview";
    public const string Demos_URL_Grid_Alignment = Demos_URL_Grid_Prefix + "/alignment";
    public const string Demos_URL_Grid_CustomCSSClass = Demos_URL_Grid_Prefix + "/custom-css-class";
    public const string Demos_URL_Grid_DataBinding = Demos_URL_Grid_Prefix + "/data-binding";
    public const string Demos_URL_Grid_DetailView = Demos_URL_Grid_Prefix + "/detail-view";
    public const string Demos_URL_Grid_Events = Demos_URL_Grid_Prefix + "/events";
    public const string Demos_URL_Grid_Filters = Demos_URL_Grid_Prefix + "/filters";
    public const string Demos_URL_Grid_FixedHeader = Demos_URL_Grid_Prefix + "/fixed-header";
    public const string Demos_URL_Grid_FreezeColumns = Demos_URL_Grid_Prefix + "/freeze-columns";
    public const string Demos_URL_Grid_Settings = Demos_URL_Grid_Prefix + "/settings";
    public const string Demos_URL_Grid_NestedGrid = Demos_URL_Grid_Prefix + "/nested-grid";
    public const string Demos_URL_Grid_Paging = Demos_URL_Grid_Prefix + "/paging";
    public const string Demos_URL_Grid_Selection = Demos_URL_Grid_Prefix + "/selection";
    public const string Demos_URL_Grid_Sorting = Demos_URL_Grid_Prefix + "/sorting";
    public const string Demos_URL_Grid_Summary = Demos_URL_Grid_Prefix + "/summary";
    public const string Demos_URL_Grid_Translations = Demos_URL_Grid_Prefix + "/translations";
    public const string Demos_URL_Grid_OtherExamples = Demos_URL_Grid_Prefix + "/other";
    #endregion Grid

    public const string Demos_URL_Markdown = Demos_URL_Prefix + "/markdown";
    public const string Demos_URL_Modal = Demos_URL_Prefix + "/modals";
    public const string Demos_URL_Offcanvas = Demos_URL_Prefix + "/offcanvas";
    public const string Demos_URL_Pagination = Demos_URL_Prefix + "/pagination";
    public const string Demos_URL_PDFViewer = Demos_URL_Prefix + "/pdf-viewer";
    public const string Demos_URL_Placeholders = Demos_URL_Prefix + "/placeholders";
    public const string Demos_URL_Preload = Demos_URL_Prefix + "/preload";
    public const string Demos_URL_Progress = Demos_URL_Prefix + "/progress";
    public const string Demos_URL_Ribbon = Demos_URL_Prefix + "/ribbon";
    public const string Demos_URL_ScriptLoader = Demos_URL_Prefix + "/script-loader";
    public const string Demos_URL_Sidebar = Demos_URL_Prefix + "/sidebar";
    public const string Demos_URL_Sidebar2 = Demos_URL_Prefix + "/sidebar2";
    public const string Demos_URL_SortableList = Demos_URL_Prefix + "/sortable-list";
    public const string Demos_URL_Spinners = Demos_URL_Prefix + "/spinners";
    public const string Demos_URL_Tabs = Demos_URL_Prefix + "/tabs";
    public const string Demos_URL_ThemeSwitcher = Demos_URL_Prefix + "/theme-switcher";
    public const string Demos_URL_Toasts = Demos_URL_Prefix + "/toasts";
    public const string Demos_URL_Tooltips = Demos_URL_Prefix + "/tooltips";

    // Data Visualization
    public const string Demos_URL_Charts_Prefix = Demos_URL_Prefix + "/charts";
    public const string Demos_URL_BarChart = Demos_URL_Charts_Prefix + "/bar-chart";
    public const string Demos_URL_DoughnutChart = Demos_URL_Charts_Prefix + "/doughnut-chart";
    public const string Demos_URL_LineChart = Demos_URL_Charts_Prefix + "/line-chart";
    public const string Demos_URL_PieChart = Demos_URL_Charts_Prefix + "/pie-chart";
    public const string Demos_URL_PolarAreaChart = Demos_URL_Charts_Prefix + "/polar-area-chart";
    public const string Demos_URL_RadarChart = Demos_URL_Charts_Prefix + "/radar-chart";
    public const string Demos_URL_ScatterChart = Demos_URL_Charts_Prefix + "/scatter-chart";

    // Services
    public const string Demos_URL_Services_Prefix = Demos_URL_Prefix + "/services";
    public const string Demos_URL_ModalService = Demos_URL_Services_Prefix + "/modal-service";

    // Utilities
    public const string Demos_URL_Utils_Prefix = Demos_URL_Prefix + "/utils";
    public const string Demos_URL_ColorUtils = Demos_URL_Utils_Prefix + "/color-utility";

    #endregion Demos

    #region Docs

    // Getting Started
    public const string Docs_URL_GettingStarted = Docs_URL_Prefix + "/getting-started";

    // Layout
    public const string Docs_URL_Layout = Docs_URL_Prefix + "/layout";

    // AI
    //public const string Docs_URL_AI_Prefix = Docs_URL_Prefix + "/ai";
    //public const string Docs_URL_AI_Chat = Docs_URL_Prefix + "/open-ai-chat";

    // Content
    public const string Docs_URL_Icons = Docs_URL_Prefix + "/icons";
    public const string Docs_URL_Images = Docs_URL_Prefix + "/images";

    // Forms
    public const string Docs_URL_Forms_Prefix = Docs_URL_Prefix + "/form";
    public const string Docs_URL_AutoComplete = Docs_URL_Forms_Prefix + "/autocomplete";
    public const string Docs_URL_CheckboxInput = Docs_URL_Forms_Prefix + "/checkbox-input";
    public const string Docs_URL_CurrencyInput = Docs_URL_Forms_Prefix + "/currency-input";
    public const string Docs_URL_DateInput = Docs_URL_Forms_Prefix + "/date-input";
    public const string Docs_URL_EnumInput = Docs_URL_Forms_Prefix + "/enum-input";
    public const string Docs_URL_NumberInput = Docs_URL_Forms_Prefix + "/number-input";
    public const string Docs_URL_PasswordInput = Docs_URL_Forms_Prefix + "/password-input";
    public const string Docs_URL_RadioInput = Docs_URL_Forms_Prefix + "/radio-input";
    public const string Docs_URL_RangeInput = Docs_URL_Forms_Prefix + "/range-input";
    public const string Docs_URL_SelectInput = Docs_URL_Forms_Prefix + "/select-input";
    public const string Docs_URL_Switch = Docs_URL_Forms_Prefix + "/switch";
    public const string Docs_URL_TextInput = Docs_URL_Forms_Prefix + "/text-input";
    public const string Docs_URL_TextAreaInput = Docs_URL_Forms_Prefix + "/text-area-input";
    public const string Docs_URL_TimeInput = Docs_URL_Forms_Prefix + "/time-input";

    // Components
    public const string Docs_URL_Accordion = Docs_URL_Prefix + "/accordion";
    public const string Docs_URL_Alerts = Docs_URL_Prefix + "/alerts";
    public const string Docs_URL_Badge = Docs_URL_Prefix + "/badge";
    public const string Docs_URL_Breadcrumb = Docs_URL_Prefix + "/breadcrumb";
    public const string Docs_URL_Buttons = Docs_URL_Prefix + "/buttons";
    public const string Docs_URL_Callout = Docs_URL_Prefix + "/callout";
    public const string Docs_URL_Card = Docs_URL_Prefix + "/card";
    public const string Docs_URL_Carousel = Docs_URL_Prefix + "/carousel";
    public const string Docs_URL_Charts = Docs_URL_Prefix + "/charts";
    public const string Docs_URL_Collapse = Docs_URL_Prefix + "/collapse";
    public const string Docs_URL_ConfirmDialog = Docs_URL_Prefix + "/confirm-dialog";
    public const string Docs_URL_Dropdown = Docs_URL_Prefix + "/dropdown";
    public const string Docs_URL_GoogleMap = Docs_URL_Prefix + "/google-map";
    public const string Docs_URL_Grid = Docs_URL_Prefix + "/grid";
    public const string Docs_URL_Markdown = Docs_URL_Prefix + "/markdown";
    public const string Docs_URL_Modal = Docs_URL_Prefix + "/modals";
    public const string Docs_URL_Offcanvas = Docs_URL_Prefix + "/offcanvas";
    public const string Docs_URL_Pagination = Docs_URL_Prefix + "/pagination";
    public const string Docs_URL_PDFViewer = Docs_URL_Prefix + "/pdf-viewer";
    public const string Docs_URL_Placeholders = Docs_URL_Prefix + "/placeholders";
    public const string Docs_URL_Preload = Docs_URL_Prefix + "/preload";
    public const string Docs_URL_Progress = Docs_URL_Prefix + "/progress";
    public const string Docs_URL_Ribbon = Docs_URL_Prefix + "/ribbon";
    public const string Docs_URL_ScriptLoader = Docs_URL_Prefix + "/script-loader";
    public const string Docs_URL_Sidebar = Docs_URL_Prefix + "/sidebar";
    public const string Docs_URL_Sidebar2 = Docs_URL_Prefix + "/sidebar2";
    public const string Docs_URL_SortableList = Docs_URL_Prefix + "/sortable-list";
    public const string Docs_URL_Spinners = Docs_URL_Prefix + "/spinners";
    public const string Docs_URL_Tabs = Docs_URL_Prefix + "/tabs";
    public const string Docs_URL_ThemeSwitcher = Docs_URL_Prefix + "/theme-switcher";
    public const string Docs_URL_Toasts = Docs_URL_Prefix + "/toasts";
    public const string Docs_URL_Tooltips = Docs_URL_Prefix + "/tooltips";

    // Data Visualization
    public const string Docs_URL_Charts_Prefix = Docs_URL_Prefix + "/charts";
    public const string Docs_URL_BarChart = Docs_URL_Charts_Prefix + "/bar-chart";
    public const string Docs_URL_DoughnutChart = Docs_URL_Charts_Prefix + "/doughnut-chart";
    public const string Docs_URL_LineChart = Docs_URL_Charts_Prefix + "/line-chart";
    public const string Docs_URL_PieChart = Docs_URL_Charts_Prefix + "/pie-chart";
    public const string Docs_URL_PolarAreaChart = Docs_URL_Charts_Prefix + "/polar-area-chart";
    public const string Docs_URL_RadarChart = Docs_URL_Charts_Prefix + "/radar-chart";
    public const string Docs_URL_ScatterChart = Docs_URL_Charts_Prefix + "/scatter-chart";

    // Services
    public const string Docs_URL_Services_Prefix = Docs_URL_Prefix + "/services";
    public const string Docs_URL_ModalService = Docs_URL_Services_Prefix + "/modal-service";

    // Utilities
    public const string Docs_URL_Utils_Prefix = Docs_URL_Prefix + "/utils";
    public const string Docs_URL_ColorUtils = Docs_URL_Utils_Prefix + "/color-utility";

    #endregion Docs
}
