namespace BlazorBootstrap;

/// <summary>
/// Represents plugin configuration for a scatter chart.
/// </summary>
[AddedVersion("3.0.0")]
public class ScatterChartPlugins : ChartPlugins
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the data label plugin configuration.
    /// </summary>
    [AddedVersion("3.0.0")]
    [Description("Gets or sets the data label plugin configuration.")]
    public ScatterChartDataLabels Datalabels { get; set; } = new();

    #endregion
}

/// <summary>
/// Represents the data label plugin configuration for a scatter chart.
/// </summary>
[AddedVersion("3.0.0")]
public class ScatterChartDataLabels
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the label border radius.
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(4d)]
    [Description("Gets or sets the label border radius.")]
    public double BorderRadius { get; set; } = 4;

    /// <summary>
    /// Gets or sets the label text color.
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue("white")]
    [Description("Gets or sets the label text color.")]
    public string? Color { get; set; } = "white";

    /// <summary>
    /// Gets or sets the font configuration used for labels.
    /// </summary>
    [AddedVersion("3.0.0")]
    [Description("Gets or sets the font configuration used for labels.")]
    public ScatterChartDataLabelsFont Font { get; set; } = new();

    /// <summary>
    /// Gets or sets the label padding.
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(6d)]
    [Description("Gets or sets the label padding.")]
    public double Padding { get; set; } = 6;

    #endregion
}

/// <summary>
/// Represents the font configuration for scatter chart data labels.
/// </summary>
[AddedVersion("3.0.0")]
public class ScatterChartDataLabelsFont
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the label font weight.
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue("bold")]
    [Description("Gets or sets the label font weight.")]
    public string? Weight { get; set; } = "bold";

    #endregion
}
