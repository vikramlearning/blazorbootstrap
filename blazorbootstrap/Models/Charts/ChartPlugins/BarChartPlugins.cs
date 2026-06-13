namespace BlazorBootstrap;

/// <summary>
/// Represents plugin configuration for a bar chart.
/// </summary>
[AddedVersion("1.10.2")]
public class BarChartPlugins : ChartPlugins
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the data label plugin configuration.
    /// </summary>
    [AddedVersion("1.10.2")]
    [Description("Gets or sets the data label plugin configuration.")]
    public BarChartDataLabels Datalabels { get; set; } = new();

    #endregion
}

/// <summary>
/// Represents the data label plugin configuration for a bar chart.
/// </summary>
[AddedVersion("1.10.2")]
public class BarChartDataLabels
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the label text color.
    /// </summary>
    [AddedVersion("1.10.2")]
    [DefaultValue("white")]
    [Description("Gets or sets the label text color.")]
    public string? Color { get; set; } = "white";

    /// <summary>
    /// Gets or sets the font configuration used for labels.
    /// </summary>
    [AddedVersion("1.10.2")]
    [Description("Gets or sets the font configuration used for labels.")]
    public BarChartDataLabelsFont Font { get; set; } = new();

    #endregion
}

/// <summary>
/// Represents the font configuration for bar chart data labels.
/// </summary>
[AddedVersion("1.10.2")]
public class BarChartDataLabelsFont
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the label font weight.
    /// </summary>
    [AddedVersion("1.10.2")]
    [DefaultValue("bold")]
    [Description("Gets or sets the label font weight.")]
    public string? Weight { get; set; } = "bold";

    #endregion
}
