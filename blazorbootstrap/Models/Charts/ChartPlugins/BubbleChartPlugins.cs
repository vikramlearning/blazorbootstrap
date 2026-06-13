namespace BlazorBootstrap;

/// <summary>
/// Represents plugin configuration for a bubble chart.
/// </summary>
[AddedVersion("4.0.0")]
public class BubbleChartPlugins : ChartPlugins
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the data label plugin configuration.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Gets or sets the data label plugin configuration.")]
    public BubbleChartDataLabels Datalabels { get; set; } = new();

    #endregion
}

/// <summary>
/// Represents the data label plugin configuration for a bubble chart.
/// </summary>
[AddedVersion("4.0.0")]
public class BubbleChartDataLabels
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the label border radius.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(4d)]
    [Description("Gets or sets the label border radius.")]
    public double BorderRadius { get; set; } = 4;

    /// <summary>
    /// Gets or sets the label text color.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue("white")]
    [Description("Gets or sets the label text color.")]
    public string? Color { get; set; } = "white";

    /// <summary>
    /// Gets or sets the font configuration used for labels.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Gets or sets the font configuration used for labels.")]
    public BubbleChartDataLabelsFont Font { get; set; } = new();

    /// <summary>
    /// Gets or sets the label padding.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(6d)]
    [Description("Gets or sets the label padding.")]
    public double Padding { get; set; } = 6;

    #endregion
}

/// <summary>
/// Represents the font configuration for bubble chart data labels.
/// </summary>
[AddedVersion("4.0.0")]
public class BubbleChartDataLabelsFont
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the label font weight.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue("bold")]
    [Description("Gets or sets the label font weight.")]
    public string? Weight { get; set; } = "bold";

    #endregion
}