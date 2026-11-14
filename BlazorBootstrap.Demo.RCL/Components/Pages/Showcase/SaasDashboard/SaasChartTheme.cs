using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.SaasDashboard;

/// <summary>
/// Chart colors and shared chart options of the Pulseboard dashboard, for the light and dark color modes.
/// Charts draw on a canvas, so CSS variables do not reach them; pages re-apply these options when the theme changes.
/// </summary>
public static class SaasChartTheme
{
    #region Fields and Constants

    private const string FontFamily = "'Plus Jakarta Sans', system-ui, -apple-system, 'Segoe UI', Roboto, sans-serif";

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Applies the axis, grid, legend, and tooltip colors of the color mode to bar chart options.</summary>
    public static void Apply(BarChartOptions options, bool isDark)
    {
        ApplyPlugins(options.Plugins, isDark);
        ApplyScales(options.Scales, isDark);
    }

    /// <summary>Applies the legend and tooltip colors of the color mode to doughnut chart options.</summary>
    public static void Apply(DoughnutChartOptions options, bool isDark) => ApplyPlugins(options.Plugins, isDark);

    /// <summary>Applies the axis, grid, legend, and tooltip colors of the color mode to line chart options.</summary>
    public static void Apply(LineChartOptions options, bool isDark)
    {
        ApplyPlugins(options.Plugins, isDark);
        ApplyScales(options.Scales, isDark);
    }

    /// <summary>Returns the brand color of the color mode, optionally with an alpha value.</summary>
    public static string Primary(bool isDark, double alpha = 1) => isDark ? Rgba(96, 165, 250, alpha) : Rgba(37, 99, 235, alpha);

    /// <summary>Returns the series colors of the color mode: brand blue first, then the supporting palette.</summary>
    public static IReadOnlyList<string> Series(bool isDark) => isDark
        ? new[] { "#60a5fa", "#34d399", "#fbbf24", "#f472b6", "#a78bfa" }
        : new[] { "#2563eb", "#059669", "#d97706", "#db2777", "#7c3aed" };

    /// <summary>Returns the neutral color used for a target or comparison series.</summary>
    public static string Muted(bool isDark) => isDark ? "rgba(148, 163, 184, 0.9)" : "rgba(100, 116, 139, 0.9)";

    /// <summary>Returns the page surface color, used to separate doughnut slices.</summary>
    public static string Surface(bool isDark) => isDark ? "#111827" : "#ffffff";

    #endregion

    #region Private Methods

    private static void ApplyPlugins(ChartPlugins plugins, bool isDark)
    {
        var text = Text(isDark);

        plugins.Legend.Position = "bottom";
        plugins.Legend.Align = "start";
        plugins.Legend.Labels = new ChartPluginsLegendLabels
        {
            Color = text,
            Font = new ChartFont { Family = FontFamily, Size = 12, Weight = "500" },
            UsePointStyle = true,
            PointStyle = "circle",
            BoxWidth = 8,
            BoxHeight = 8,
            Padding = 16,
        };

        plugins.Tooltip = new ChartPluginsTooltip
        {
            Enabled = true,
            BackgroundColor = isDark ? "#e5e7eb" : "#0f172a",
            TitleColor = isDark ? "#0f172a" : "#ffffff",
            BodyColor = isDark ? "#1f2937" : "#e2e8f0",
            DisplayColors = true,
            CaretSize = 6,
            CaretPadding = 6,
        };
    }

    private static void ApplyScales(Scales scales, bool isDark)
    {
        foreach (var axis in new[] { scales.X!, scales.Y! })
        {
            axis.Ticks = new ChartAxesTicks { Color = Text(isDark) };
            axis.Grid = new ChartAxesGrid { Color = isDark ? "rgba(148, 163, 184, 0.14)" : "rgba(15, 23, 42, 0.07)", DrawTicks = false };
            axis.Border = new ChartAxesBorder { Display = false };
        }

        scales.X!.Grid!.Display = false;
    }

    private static string Rgba(int red, int green, int blue, double alpha) =>
        FormattableString.Invariant($"rgba({red}, {green}, {blue}, {alpha})");

    private static string Text(bool isDark) => isDark ? "#cbd5e1" : "#475569";

    #endregion

    #endregion
}
