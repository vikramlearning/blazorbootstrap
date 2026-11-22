namespace BlazorBootstrap.Demo.RCL;

/// <summary>
/// The demos site's search-indexing policy: which old URLs redirect, which pages point their canonical
/// at the docs site, and which Showcase pages are indexed. The page heads, both routers, the Server
/// redirect middleware and the build-time generator of <c>sitemap.xml</c> and <c>llms.txt</c> all read it
/// (rules.md "Demo site search indexing" and "Renamed demo routes redirect").
/// </summary>
public static class SeoRoutes
{
    #region Fields and Constants

    /// <summary>The demos site's public origin, used for canonical and sitemap URLs.</summary>
    public const string DemosSiteUrl = "https://demos.blazorbootstrap.com";

    /// <summary>The docs site's public origin.</summary>
    public const string DocsSiteUrl = "https://docs.blazorbootstrap.com";

    private static readonly Dictionary<string, string> docsSiteCanonical = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/docs/accordion"] = "/components/accordion",
        ["/docs/alerts"] = "/components/alerts",
        ["/docs/avatar"] = "/components/avatar",
        ["/docs/badge"] = "/components/badge",
        ["/docs/breadcrumb"] = "/components/breadcrumb",
        ["/docs/button-group"] = "/components/button-group",
        ["/docs/buttons"] = "/components/buttons",
        ["/docs/calendar"] = "/components/calendar",
        ["/docs/callout"] = "/components/callout",
        ["/docs/card"] = "/components/card",
        ["/docs/carousel"] = "/components/carousel",
        ["/docs/collapse"] = "/components/collapse",
        ["/docs/confirm-dialog"] = "/components/confirm-dialog",
        ["/docs/dropdown"] = "/components/dropdown",
        ["/docs/form/autocomplete"] = "/forms/autocomplete",
        ["/docs/form/checkbox-input"] = "/forms/checkbox-input",
        ["/docs/form/currency-input"] = "/forms/currency-input",
        ["/docs/form/date-input"] = "/forms/date-input",
        ["/docs/form/drag-and-drop-file-input"] = "/forms/drag-and-drop-file-input",
        ["/docs/form/enum-input"] = "/forms/enum-input",
        ["/docs/form/file-input"] = "/forms/file-input",
        ["/docs/form/input-group"] = "/components/input-group",
        ["/docs/form/number-input"] = "/forms/number-input",
        ["/docs/form/otp-input"] = "/forms/otp-input",
        ["/docs/form/password-input"] = "/forms/password-input",
        ["/docs/form/radio-input"] = "/forms/radio-input",
        ["/docs/form/range-input"] = "/forms/range-input",
        ["/docs/form/rating-input"] = "/forms/rating-input",
        ["/docs/form/rich-text-editor"] = "/forms/rich-text-editor",
        ["/docs/form/switch"] = "/forms/switch",
        ["/docs/form/text-area-input"] = "/forms/textarea-input",
        ["/docs/form/text-input"] = "/forms/text-input",
        ["/docs/form/time-input"] = "/forms/time-input",
        ["/docs/google-map"] = "/components/google-map",
        ["/docs/grid"] = "/components/grid",
        ["/docs/icons"] = "/content/icons",
        ["/docs/images"] = "/content/images",
        ["/docs/list-group"] = "/components/list-group",
        ["/docs/markdown"] = "/components/markdown",
        ["/docs/mind-map"] = "/components/mind-map",
        ["/docs/modals"] = "/components/modal",
        ["/docs/navbar"] = "/components/navbar",
        ["/docs/offcanvas"] = "/components/offcanvas",
        ["/docs/pagination"] = "/components/pagination",
        ["/docs/pdf-viewer"] = "/components/pdf-viewer",
        ["/docs/placeholders"] = "/components/placeholders",
        ["/docs/popover"] = "/components/popover",
        ["/docs/preload"] = "/components/preload",
        ["/docs/progress"] = "/components/progress",
        ["/docs/ribbon"] = "/components/ribbon",
        ["/docs/script-loader"] = "/components/script-loader",
        ["/docs/services/modal-service"] = "/services/modal",
        ["/docs/sidebar"] = "/components/sidebar",
        ["/docs/sidebar2"] = "/components/sidebar2",
        ["/docs/sortable-list"] = "/components/sortable-list",
        ["/docs/spinners"] = "/components/spinners",
        ["/docs/split-view"] = "/components/split-view",
        ["/docs/tabs"] = "/components/tabs",
        ["/docs/theme-switcher"] = "/components/theme-switcher",
        ["/docs/timeline"] = "/components/timeline",
        ["/docs/toasts"] = "/components/toasts",
        ["/docs/tooltips"] = "/components/tooltips",
    };

    private static readonly HashSet<string> indexableShowcaseRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        DemoRouteConstants.Showcase_URL_Prefix,
        DemoRouteConstants.Showcase_URL_AiAssistant,
        DemoRouteConstants.Showcase_URL_Ecommerce,
        DemoRouteConstants.Showcase_URL_Fintech,
        DemoRouteConstants.Showcase_URL_Healthcare,
        DemoRouteConstants.Showcase_URL_HelpDesk,
        DemoRouteConstants.Showcase_URL_HrPortal,
        DemoRouteConstants.Showcase_URL_Learning,
        DemoRouteConstants.Showcase_URL_Monitoring,
        DemoRouteConstants.Showcase_URL_ProjectBoard,
        DemoRouteConstants.Showcase_URL_Saas,
        DemoRouteConstants.Showcase_URL_Travel,
    };

    private static readonly Dictionary<string, string> legacyRedirects = new(StringComparer.OrdinalIgnoreCase)
    {
        // URLs from the 2024 hand-written sitemap that are no longer routes.
        ["/alerts"] = DemoRouteConstants.Demos_URL_Alerts,
        ["/breadcrumb"] = DemoRouteConstants.Demos_URL_Breadcrumb,
        ["/buttons"] = DemoRouteConstants.Demos_URL_Buttons,
        ["/callout"] = DemoRouteConstants.Demos_URL_Callout,
        ["/charts"] = DemoRouteConstants.Demos_URL_Charts,
        ["/confirm-dialog"] = DemoRouteConstants.Demos_URL_ConfirmDialog,
        ["/form/autocomplete"] = DemoRouteConstants.Demos_URL_AutoComplete,
        ["/form/currency-input"] = DemoRouteConstants.Demos_URL_CurrencyInput,
        ["/form/number-input"] = DemoRouteConstants.Demos_URL_NumberInput,
        ["/form/switch"] = DemoRouteConstants.Demos_URL_Switch,
        ["/getting-started"] = DemoRouteConstants.Demos_URL_GettingStarted,
        ["/icons"] = DemoRouteConstants.Demos_URL_Icons,
        ["/modals"] = DemoRouteConstants.Demos_URL_Modal,
        ["/offcanvas"] = DemoRouteConstants.Demos_URL_Offcanvas,
        ["/pagination"] = DemoRouteConstants.Demos_URL_Pagination,
        ["/placeholders"] = DemoRouteConstants.Demos_URL_Placeholders,
        ["/preload"] = DemoRouteConstants.Demos_URL_Preload,
        ["/progress"] = DemoRouteConstants.Demos_URL_Progress,
        ["/sidebar"] = DemoRouteConstants.Demos_URL_Sidebar,
        ["/tabs"] = DemoRouteConstants.Demos_URL_Tabs,
        ["/toasts"] = DemoRouteConstants.Demos_URL_Toasts,
        ["/tooltips"] = DemoRouteConstants.Demos_URL_Tooltips,

        // Old second routes that pages used to answer to.
        ["/autocomplete"] = DemoRouteConstants.Demos_URL_AutoComplete,
        ["/checkbox-input"] = DemoRouteConstants.Demos_URL_CheckboxInput,
        ["/currency-input"] = DemoRouteConstants.Demos_URL_CurrencyInput,
        ["/date-input"] = DemoRouteConstants.Demos_URL_DateInput,
        ["/drag-and-drop-file-input"] = DemoRouteConstants.Demos_URL_DragAndDropFileInput,
        ["/enum-input"] = DemoRouteConstants.Demos_URL_EnumInput,
        ["/file-input"] = DemoRouteConstants.Demos_URL_FileInput,
        ["/getting-started/blazor-server"] = DemoRouteConstants.Demos_URL_GettingStarted,
        ["/getting-started/blazor-webassembly"] = DemoRouteConstants.Demos_URL_GettingStarted,
        ["/getting-started/maui-blazor"] = DemoRouteConstants.Demos_URL_GettingStarted,
        ["/grid"] = DemoRouteConstants.Demos_URL_Grid_Overview,
        ["/modal-service"] = DemoRouteConstants.Demos_URL_ModalService,
        ["/number-input"] = DemoRouteConstants.Demos_URL_NumberInput,
        ["/otp-input"] = DemoRouteConstants.Demos_URL_OTPInput,
        ["/password-input"] = DemoRouteConstants.Demos_URL_PasswordInput,
        ["/radio-input"] = DemoRouteConstants.Demos_URL_RadioInput,
        ["/range-input"] = DemoRouteConstants.Demos_URL_RangeInput,
        ["/rich-text-editor"] = DemoRouteConstants.Demos_URL_RichTextEditor,
        ["/switch"] = DemoRouteConstants.Demos_URL_Switch,
        ["/text-area-input"] = DemoRouteConstants.Demos_URL_TextAreaInput,
        ["/text-input"] = DemoRouteConstants.Demos_URL_TextInput,
        ["/time-input"] = DemoRouteConstants.Demos_URL_TimeInput,
        ["/toast-service"] = DemoRouteConstants.Demos_URL_ToastService,
        ["/docs/getting-started/blazor-server"] = DemoRouteConstants.Docs_URL_GettingStarted,
        ["/docs/getting-started/blazor-webassembly"] = DemoRouteConstants.Docs_URL_GettingStarted,
        ["/docs/getting-started/maui-blazor"] = DemoRouteConstants.Docs_URL_GettingStarted,
    };

    private static readonly HashSet<string> sectionRootRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        // Section entry points that render a page whose canonical is another URL.
        DemoRouteConstants.Demos_URL_Prefix,
        DemoRouteConstants.Docs_URL_Prefix,
    };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Returns the canonical URL for a demos page: the matching docs-site page for a demos <c>/docs/...</c>
    /// page that has one, otherwise the page's own demos URL.
    /// </summary>
    public static string GetCanonicalUrl(string? pageUrl)
    {
        var path = Normalize(pageUrl);

        return docsSiteCanonical.TryGetValue(path, out var docsPath)
            ? DocsSiteUrl + docsPath
            : DemosSiteUrl + (path == "/" ? "/" : path);
    }

    /// <summary>Returns <see langword="true" /> when the page must carry <c>noindex</c>.</summary>
    public static bool IsNoIndex(string? path)
    {
        var normalized = Normalize(path);

        return normalized.StartsWith(DemoRouteConstants.Showcase_URL_Prefix + "/", StringComparison.OrdinalIgnoreCase)
               && !indexableShowcaseRoutes.Contains(normalized);
    }

    /// <summary>
    /// Returns <see langword="true" /> when a parameterless route belongs in the demos <c>sitemap.xml</c>:
    /// indexable, canonical on the demos site, and not an old URL.
    /// </summary>
    public static bool IsSitemapRoute(string? route)
    {
        if (string.IsNullOrWhiteSpace(route) || route.Contains('{'))
            return false;

        var path = Normalize(route);

        return !IsNoIndex(path)
               && !legacyRedirects.ContainsKey(path)
               && !docsSiteCanonical.ContainsKey(path)
               && !sectionRootRoutes.Contains(path);
    }

    /// <summary>
    /// Normalizes a path for policy lookups: drops the query and fragment and any trailing slash.
    /// Lookups are case-insensitive.
    /// </summary>
    public static string Normalize(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/";

        var end = path.IndexOfAny(['?', '#']);
        var value = end >= 0 ? path[..end] : path;

        if (!value.StartsWith('/'))
            value = "/" + value;

        return value.Length > 1 ? value.TrimEnd('/') : value;
    }

    /// <summary>Returns the current route an old URL redirects to, or <see langword="null" />.</summary>
    public static string? TryGetLegacyRedirect(string? path) =>
        legacyRedirects.TryGetValue(Normalize(path), out var target) ? target : null;

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Demos <c>/docs/...</c> routes mapped to their docs-site path.</summary>
    public static IReadOnlyDictionary<string, string> DocsSiteCanonical => docsSiteCanonical;

    /// <summary>The Showcase pages search engines may index: the gallery and each site's landing page.</summary>
    public static IReadOnlySet<string> IndexableShowcaseRoutes => indexableShowcaseRoutes;

    /// <summary>Old demos URLs mapped to the current route they permanently redirect to.</summary>
    public static IReadOnlyDictionary<string, string> LegacyRedirects => legacyRedirects;

    #endregion
}
