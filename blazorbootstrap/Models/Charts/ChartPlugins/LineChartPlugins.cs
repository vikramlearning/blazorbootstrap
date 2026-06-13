namespace BlazorBootstrap;

/// <summary>
/// Represents plugin configuration for a line chart.
/// </summary>
[AddedVersion("1.10.2")]
public class LineChartPlugins : ChartPlugins
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the data label plugin configuration.
    /// </summary>
    [AddedVersion("1.10.2")]
    [Description("Gets or sets the data label plugin configuration.")]
    public LineChartDataLabels Datalabels { get; set; } = new();

    #endregion
}

/// <summary>
/// Represents the data label plugin configuration for a line chart.
/// </summary>
[AddedVersion("1.10.2")]
public class LineChartDataLabels
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the label border radius.
    /// </summary>
    [AddedVersion("1.10.2")]
    [DefaultValue(4d)]
    [Description("Gets or sets the label border radius.")]
    public double BorderRadius { get; set; } = 4;

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
    public LineChartDataLabelsFont Font { get; set; } = new();

    /// <summary>
    /// Gets or sets the label padding.
    /// </summary>
    [AddedVersion("1.10.2")]
    [DefaultValue(6d)]
    [Description("Gets or sets the label padding.")]
    public double Padding { get; set; } = 6;

    #endregion
}

/// <summary>
/// Represents the font configuration for line chart data labels.
/// </summary>
[AddedVersion("1.10.2")]
public class LineChartDataLabelsFont
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
