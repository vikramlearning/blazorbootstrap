namespace BlazorBootstrap;

/// <summary>
/// The doughnut/pie chart allows a number of properties to be specified for each dataset.
/// These are used to set display properties for a specific dataset.
/// <see href="https://www.chartjs.org/docs/latest/charts/doughnut.html#dataset-properties" />.
/// </summary>
[AddedVersion("1.0.0")]
public class DoughnutChartDataset : ChartDataset<double?>
{
    #region Properties, Indexers

    /// <summary>
    /// Arc background color.
    /// </summary>
    /// <remarks>
    /// Default value is 'rgba(0, 0, 0, 0.1)'.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue("rgba(0, 0, 0, 0.1)")]
    [Description("Gets or sets the arc background color.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? BackgroundColor { get; set; }

    /// <summary>
    /// Supported values are 'center' and 'inner'.
    /// When 'center' is set, the borders of arcs next to each other will overlap.
    /// When 'inner' is set, it is guaranteed that all borders will not overlap.
    /// </summary>
    /// <remarks>
    /// Default value is 'center'.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue("center")]
    [Description("Gets or sets the arc border alignment.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? BorderAlign { get; set; } // TODO: change this to enum

    /// <summary>
    /// Arc border color.
    /// </summary>
    /// <remarks>
    /// Default value is '#fff'.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue("#fff")]
    [Description("Gets or sets the arc border color.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? BorderColor { get; set; }

    /// <summary>
    /// Arc border length and spacing of dashes.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the arc border dash pattern.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? BorderDash { get; set; }

    /// <summary>
    /// Arc border offset for line dashes.
    /// </summary>
    /// <remarks>
    /// Default value is 0.0.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(0d)]
    [Description("Gets or sets the arc border dash offset.")]
    public double BorderDashOffset { get; set; }

    /// <summary>
    /// Arc border join style.
    /// Supported values are 'round', 'bevel', 'miter'.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the arc border join style.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? BorderJoinStyle { get; set; } // TODO: change this to enum

    /// <summary>
    /// It is applied to all corners of the arc (outerStart, outerEnd, innerStart, innerRight).
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(0)]
    [Description("Gets or sets the arc border radius.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? BorderRadius { get; set; }

    /// <summary>
    /// Arc border width (in pixels).
    /// </summary>
    /// <remarks>
    /// Default value is 2.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(2)]
    [Description("Gets or sets the arc border width in pixels.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? BorderWidth { get; set; }

    /// <summary>
    /// Per-dataset override for the sweep that the arcs cover.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the sweep covered by the arcs.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Circumference { get; set; }

    [AddedVersion("1.10.2")]
    [Description("Gets or sets the dataset data label configuration.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public DoughnutChartDatasetDataLabels Datalabels { get; set; } = new(); // TODO: add the reference link

    /// <summary>
    /// Arc background color when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the arc background color when hovered.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? HoverBackgroundColor { get; set; }

    /// <summary>
    /// Arc border color when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the arc border color when hovered.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? HoverBorderColor { get; set; }

    /// <summary>
    /// Arc border length and spacing of dashes when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the hovered arc border dash pattern.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? HoverBorderDash { get; set; }

    /// <summary>
    /// Arc border offset for line dashes when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the hovered arc border dash offset.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? HoverBorderDashOffset { get; set; }

    /// <summary>
    /// Arc border join style when hovered.
    /// Supported values are 'round', 'bevel', 'miter'.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the hovered arc border join style.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? HoverBorderJoinStyle { get; set; } // TODO: change this to enum

    /// <summary>
    /// Arc border width when hovered (in pixels).
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the hovered arc border width in pixels.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? HoverBorderWidth { get; set; }

    /// <summary>
    /// Arc offset when hovered (in pixels).
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(0)]
    [Description("Gets or sets the hovered arc offset in pixels.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? HoverOffset { get; set; }

    /// <summary>
    /// Arc offset (in pixels).
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(0)]
    [Description("Gets or sets the arc offset in pixels.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? Offset { get; set; }

    /// <summary>
    /// Per-dataset override for the starting angle to draw arcs from.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the starting rotation angle for the arcs.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Rotation { get; set; }

    /// <summary>
    /// Fixed arc offset (in pixels). Similar to <see cref="Offset" /> but applies to all arcs.
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(0)]
    [Description("Gets or sets the fixed arc spacing in pixels.")]
    public double Spacing { get; set; }

    /// <summary>
    /// The relative thickness of the dataset.
    /// Providing a value for weight will cause the pie or doughnut dataset to be drawn
    /// with a thickness relative to the sum of all the dataset weight values.
    /// </summary>
    /// <remarks>
    /// Default value is 1.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(1d)]
    [Description("Gets or sets the relative dataset thickness.")]
    public double Weight { get; set; } = 1;

    #endregion
}

/// <summary>
/// Represents data label options for a doughnut chart dataset.
/// </summary>
[AddedVersion("1.10.2")]
public class DoughnutChartDatasetDataLabels : ChartDatasetDataLabels { }
