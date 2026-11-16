using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// Chart colors and shared chart options of the Quillpath Academy instructor pages. Charts draw on a canvas, so CSS variables
/// do not reach them, and the learning state does not report the color mode. Every color here is therefore picked to read on
/// both the light panel (#fff) and the dark panel (#1c1912). The completion funnel's stages are ordered, so they are one amber
/// hue stepped from light to dark (each step clears 2:1 against both panels, the darker steps far more); the rating bars are a
/// mid amber that keeps 3:1 on both; axis text is about 4.1:1; and the tooltip is a mid-dark box with light text.
/// </summary>
public static class LearningChartTheme
{
    #region Fields and Constants

    /// <summary>The funnel stage "Completed" (amber 800: 7.1:1 on white, 2.5:1 on dark).</summary>
    public const string FunnelCompleted = "#92400e";

    /// <summary>The funnel stage "Enrolled" (amber 500: 2.2:1 on white, 8.2:1 on dark).</summary>
    public const string FunnelEnrolled = "#f59e0b";

    /// <summary>The funnel stage "Halfway" (amber 700: 5.0:1 on white, 3.5:1 on dark).</summary>
    public const string FunnelHalfway = "#b45309";

    /// <summary>The funnel stage "Started" (amber 600: 3.2:1 on white, 5.5:1 on dark).</summary>
    public const string FunnelStarted = "#d97706";

    /// <summary>The color of the review counts per star rating (amber 600: 3.2:1 on white, 5.5:1 on dark).</summary>
    public const string Ratings = "#d97706";

    private const string FontFamily = "'Source Sans 3', system-ui, -apple-system, 'Segoe UI', Roboto, sans-serif";

    private const string GridLine = "rgba(128, 128, 136, 0.22)";

    /// <summary>Axis and legend text: a neutral gray that reads on the light and the dark panel (4.1:1 and 4.2:1).</summary>
    private const string Text = "#7c7c84";

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Builds one bar series in a color.</summary>
    public static BarChartDataset Bars(string label, IEnumerable<double> values, string color) => new()
    {
        Label = label,
        Data = values.Select(value => (double?)value).ToList(),
        BackgroundColor = new List<string> { color },
        BorderColor = new List<string> { color },
        BorderWidth = new List<double> { 0 },
        BorderRadius = new List<double> { 2 },
        MaxBarThickness = 36,
    };

    /// <summary>Builds the options of a bar chart of counts (learners or reviews), bars side by side, with the site's colors.</summary>
    /// <param name="valueTitle">The title of the value axis, such as "Learners".</param>
    /// <param name="showLegend">Whether to show the legend: on for several series, off for one (the heading names it).</param>
    public static BarChartOptions CountBarOptions(string valueTitle, bool showLegend)
    {
        var options = new BarChartOptions
        {
            Responsive = true,
            MaintainAspectRatio = false,
            Interaction = new Interaction { Mode = InteractionMode.Index, Intersect = false },
        };

        ApplyPlugins(options.Plugins, showLegend);
        ApplyScales(options.Scales);

        options.Scales.Y!.BeginAtZero = true;

        // Counts are whole numbers: a value axis of at least 0–10 keeps Chart.js from choosing fractional steps for small counts.
        options.Scales.Y.SuggestedMax = 10;
        options.Scales.Y.Title = new ChartAxesTitle { Display = true, Text = valueTitle, Color = Text, Font = new ChartFont { Family = FontFamily, Size = 12 } };

        return options;
    }

    #endregion

    #region Private Methods

    private static void ApplyPlugins(ChartPlugins plugins, bool showLegend)
    {
        plugins.Legend.Display = showLegend;
        plugins.Legend.Position = "bottom";
        plugins.Legend.Align = "start";
        plugins.Legend.Labels = new ChartPluginsLegendLabels
        {
            Color = Text,
            Font = new ChartFont { Family = FontFamily, Size = 12, Weight = "500" },
            UsePointStyle = true,
            PointStyle = "circle",
            BoxWidth = 8,
            BoxHeight = 8,
            Padding = 16,
        };

        // Slate 700 with near-white text (about 10:1): distinct from the white panel and from the dark one.
        plugins.Tooltip = new ChartPluginsTooltip
        {
            Enabled = true,
            BackgroundColor = "#334155",
            TitleColor = "#ffffff",
            BodyColor = "#f1f5f9",
            DisplayColors = true,
            CaretSize = 6,
            CaretPadding = 6,
        };
    }

    private static void ApplyScales(Scales scales)
    {
        foreach (var axis in new[] { scales.X!, scales.Y! })
        {
            axis.Ticks = new ChartAxesTicks { Color = Text };
            axis.Grid = new ChartAxesGrid { Color = GridLine, DrawTicks = false };
            axis.Border = new ChartAxesBorder { Display = false };
        }

        scales.X!.Grid!.Display = false;
    }

    #endregion

    #endregion
}
