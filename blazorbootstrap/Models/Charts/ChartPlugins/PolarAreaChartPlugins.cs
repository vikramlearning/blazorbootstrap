namespace BlazorBootstrap;

/// <summary>
/// Represents plugin configuration for a polar area chart.
/// </summary>
[AddedVersion("3.0.0")]
public class PolarAreaChartPlugins : ChartPlugins
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the data label plugin configuration.
    /// </summary>
    [AddedVersion("3.0.0")]
    [Description("Gets or sets the data label plugin configuration.")]
    public PolarAreaChartDataLabels Datalabels { get; set; } = new();

    #endregion
}

/// <summary>
/// Represents the data label plugin configuration for a polar area chart.
/// </summary>
[AddedVersion("3.0.0")]
public class PolarAreaChartDataLabels
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the label border color.
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue("white")]
    [Description("Gets or sets the label border color.")]
    public string? BorderColor { get; set; } = "white";

    /// <summary>
    /// Gets or sets the label border radius.
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(25d)]
    [Description("Gets or sets the label border radius.")]
    public double BorderRadius { get; set; } = 25;

    /// <summary>
    /// Gets or sets the label border width.
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(2d)]
    [Description("Gets or sets the label border width.")]
    public double BorderWidth { get; set; } = 2;

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
    public PolarAreaChartDataLabelsFont Font { get; set; } = new();

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
/// Represents the font configuration for polar area chart data labels.
/// </summary>
[AddedVersion("3.0.0")]
public class PolarAreaChartDataLabelsFont
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
