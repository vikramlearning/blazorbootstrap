using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Chart colors and shared options of the Stackwatch site for the light and dark color modes. Charts draw on a canvas,
/// so CSS variables do not reach them; pages re-apply these options when the theme changes. Animation is off because the
/// charts redraw every two seconds.
/// </summary>
public static class MonitoringChartTheme
{
    #region Fields and Constants

    private const string FontFamily = "'IBM Plex Sans', system-ui, -apple-system, 'Segoe UI', Roboto, sans-serif";

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Builds line chart options with the axis, grid, legend, and tooltip colors of the color mode.</summary>
    public static LineChartOptions LineOptions(bool isDark, string yTitle, bool showLegend)
    {
        var options = new LineChartOptions
        {
            Responsive = true,
            MaintainAspectRatio = false,
            Interaction = new Interaction { Mode = InteractionMode.Index, Intersect = false },
        };

        options.Animation = new ChartAnimation { Duration = 0 };

        var plugins = options.Plugins;
        plugins.Legend.Display = showLegend;
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

        foreach (var axis in new[] { options.Scales.X!, options.Scales.Y! })
        {
            axis.Ticks = new ChartAxesTicks { Color = Text(isDark) };
            axis.Grid = new ChartAxesGrid { Color = isDark ? "rgba(148, 163, 184, 0.14)" : "rgba(15, 23, 42, 0.07)", DrawTicks = false };
            axis.Border = new ChartAxesBorder { Display = false };
        }

        options.Scales.X!.Grid!.Display = false;
        options.Scales.Y!.BeginAtZero = true;
        options.Scales.Y.Title = new ChartAxesTitle { Display = true, Text = yTitle, Color = Text(isDark) };

        return options;
    }

    /// <summary>Builds a line dataset in a series color.</summary>
    public static LineChartDataset Line(string label, IEnumerable<double?> values, string color, bool fill = false)
    {
        var line = new LineChartDataset
        {
            Label = label,
            Data = values.ToList(),
            BorderColor = color,
            BackgroundColor = fill ? WithAlpha(color, 0.12) : color,
            BorderWidth = 2,
            Tension = 0.35,
            PointRadius = new List<double> { 0 },
            PointHoverRadius = new List<double> { 4 },
        };

        if (fill)
            line.FillToStart();

        return line;
    }

    /// <summary>Returns the series colors of the color mode.</summary>
    public static IReadOnlyList<string> Series(bool isDark) => isDark
        ? new[] { "#4ade80", "#60a5fa", "#fbbf24", "#f472b6" }
        : new[] { "#15803d", "#2563eb", "#b45309", "#be185d" };

    #endregion

    #region Private Methods

    private static string Text(bool isDark) => isDark ? "#cbd5e1" : "#475569";

    private static string WithAlpha(string hex, double alpha)
    {
        var r = Convert.ToInt32(hex.Substring(1, 2), 16);
        var g = Convert.ToInt32(hex.Substring(3, 2), 16);
        var b = Convert.ToInt32(hex.Substring(5, 2), 16);

        return FormattableString.Invariant($"rgba({r}, {g}, {b}, {alpha})");
    }

    #endregion

    #endregion
}
