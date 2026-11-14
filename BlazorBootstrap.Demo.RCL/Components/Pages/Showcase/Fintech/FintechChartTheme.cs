using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Chart colors and shared options of the Tallymint site for the light and dark color modes. Charts draw on a canvas, so
/// CSS variables do not reach them; pages re-apply these options when the theme changes.
/// </summary>
public static class FintechChartTheme
{
    #region Fields and Constants

    private const string FontFamily = "'Manrope', system-ui, -apple-system, 'Segoe UI', Roboto, sans-serif";

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Applies the legend and tooltip colors of the color mode to doughnut chart options.</summary>
    public static void Apply(DoughnutChartOptions options, bool isDark) => ApplyPlugins(options.Plugins, isDark);

    /// <summary>Applies the axis, grid, legend, and tooltip colors of the color mode to line chart options.</summary>
    public static void Apply(LineChartOptions options, bool isDark)
    {
        ApplyPlugins(options.Plugins, isDark);

        foreach (var axis in new[] { options.Scales.X!, options.Scales.Y! })
        {
            axis.Ticks = new ChartAxesTicks { Color = Text(isDark) };
            axis.Grid = new ChartAxesGrid { Color = isDark ? "rgba(148, 163, 184, 0.14)" : "rgba(15, 23, 42, 0.07)", DrawTicks = false };
            axis.Border = new ChartAxesBorder { Display = false };
        }

        options.Scales.X!.Grid!.Display = false;
        options.Scales.Y!.BeginAtZero = false;
    }

    /// <summary>Returns the brand color of the color mode with an alpha value.</summary>
    public static string Primary(bool isDark, double alpha = 1) =>
        isDark ? FormattableString.Invariant($"rgba(52, 211, 153, {alpha})") : FormattableString.Invariant($"rgba(4, 120, 87, {alpha})");

    /// <summary>Returns the category series colors of the color mode.</summary>
    public static IReadOnlyList<string> Series(bool isDark) => isDark
        ? new[] { "#34d399", "#60a5fa", "#fbbf24", "#f472b6", "#a78bfa", "#fb923c", "#2dd4bf", "#f87171" }
        : new[] { "#047857", "#2563eb", "#d97706", "#db2777", "#7c3aed", "#ea580c", "#0d9488", "#dc2626" };

    /// <summary>Returns the page surface color, used to separate doughnut slices.</summary>
    public static string Surface(bool isDark) => isDark ? "#0f1a17" : "#ffffff";

    #endregion

    #region Private Methods

    private static void ApplyPlugins(ChartPlugins plugins, bool isDark)
    {
        plugins.Legend.Position = "bottom";
        plugins.Legend.Align = "start";
        plugins.Legend.Labels = new ChartPluginsLegendLabels
        {
            Color = Text(isDark),
            Font = new ChartFont { Family = FontFamily, Size = 12, Weight = "500" },
            UsePointStyle = true,
            PointStyle = "circle",
            BoxWidth = 8,
            BoxHeight = 8,
            Padding = 14,
        };

        plugins.Tooltip = new ChartPluginsTooltip
        {
            Enabled = true,
            BackgroundColor = isDark ? "#e5e7eb" : "#0f172a",
            TitleColor = isDark ? "#0f172a" : "#ffffff",
            BodyColor = isDark ? "#1f2937" : "#e2e8f0",
            DisplayColors = true,
        };
    }

    private static string Text(bool isDark) => isDark ? "#cbd5e1" : "#475569";

    #endregion

    #endregion
}
