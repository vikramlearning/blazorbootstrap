using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// Chart colors and shared chart options of the Driftwood Stays host pages. Charts draw on a canvas, so CSS variables do
/// not reach them, and the travel state does not report the color mode. Every color here is therefore picked to read on
/// both the light panel (#fff) and the dark panel (#0e1c27): series colors keep at least 3:1 against both, axis text about
/// 4.1:1, and the tooltip is a mid-dark box with light text that stands out from either page.
/// </summary>
public static class TravelChartTheme
{
    #region Fields and Constants

    /// <summary>The color of money already paid out (sky blue, 4.1:1 on white, 4.2:1 on dark).</summary>
    public const string Paid = "#0284c7";

    /// <summary>The color of money still to be paid out (orange, 3.6:1 on white, 4.9:1 on dark).</summary>
    public const string Upcoming = "#ea580c";

    private const string FontFamily = "'DM Sans', system-ui, -apple-system, 'Segoe UI', Roboto, sans-serif";

    private const string GridLine = "rgba(128, 128, 136, 0.22)";

    /// <summary>Axis and legend text: a neutral gray that reads on the light and the dark panel.</summary>
    private const string Text = "#7c7c84";

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Builds one bar series in a color.</summary>
    public static BarChartDataset Bars(string label, IEnumerable<double> values, string color, string? stack = null) => new()
    {
        Label = label,
        Data = values.Select(value => (double?)value).ToList(),
        BackgroundColor = new List<string> { color },
        BorderColor = new List<string> { color },
        BorderWidth = new List<double> { 0 },
        BorderRadius = new List<double> { 2 },
        MaxBarThickness = 36,
        Stack = stack,
    };

    /// <summary>Builds the options of a bar chart of monthly amounts, stacked or side by side, with the site's colors.</summary>
    /// <param name="valueTitle">The title of the value axis, such as "Payouts (USD)".</param>
    /// <param name="stacked">Whether the series stack into one bar per month.</param>
    public static BarChartOptions MonthlyBarOptions(string valueTitle, bool stacked)
    {
        var options = new BarChartOptions
        {
            Responsive = true,
            MaintainAspectRatio = false,
            Interaction = new Interaction { Mode = InteractionMode.Index, Intersect = false },
        };

        ApplyPlugins(options.Plugins);
        ApplyScales(options.Scales);

        options.Scales.X!.Stacked = stacked;
        options.Scales.Y!.Stacked = stacked;
        options.Scales.Y.BeginAtZero = true;
        options.Scales.Y.Title = new ChartAxesTitle { Display = true, Text = valueTitle, Color = Text, Font = new ChartFont { Family = FontFamily, Size = 12 } };

        return options;
    }

    #endregion

    #region Private Methods

    private static void ApplyPlugins(ChartPlugins plugins)
    {
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
