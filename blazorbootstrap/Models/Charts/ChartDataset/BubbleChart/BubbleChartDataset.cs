namespace BlazorBootstrap;

/// <summary>
/// A bubble chart is used to display three dimensions of data at the same time.
/// The location of the bubble is determined by the first two dimensions and the corresponding horizontal and vertical axes.
/// The third dimension is represented by the size of the individual bubbles.
/// <see href="https://www.chartjs.org/docs/latest/charts/bubble.html#dataset-properties" />.
/// </summary>
[AddedVersion("4.0.0")]
public class BubbleChartDataset : ChartDataset<BubbleChartDataPoint>
{
    #region Constructors

    public BubbleChartDataset()
    {
        Type = "bubble";
    }

    #endregion

    #region Properties, Indexers

    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the bubble background colors.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? BackgroundColor { get; set; }

    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the bubble border colors.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? BorderColor { get; set; }

    [AddedVersion("4.0.0")]
    [DefaultValue(3d)]
    [Description("Gets or sets the bubble border width.")]
    public double BorderWidth { get; set; } = 3;

    [AddedVersion("4.0.0")]
    [Description("Gets or sets the dataset data label configuration.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public BubbleChartDatasetDataLabels Datalabels { get; set; } = new();

    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a value indicating whether active elements are drawn on top.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<bool>? DrawActiveElementsOnTop { get; set; }

    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the bubble background colors when hovered.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? HoverBackgroundColor { get; set; }

    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the bubble border colors when hovered.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? HoverBorderColor { get; set; }

    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the bubble border widths when hovered.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? HoverBorderWidth { get; set; }

    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the bubble hit radii.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? HitRadius { get; set; }

    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the bubble hover radii.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? HoverRadius { get; set; }

    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the bubble point styles.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? PointStyle { get; set; }

    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the bubble radii.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? Radius { get; set; }

    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the bubble rotations.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? Rotation { get; set; }

    #endregion
}

/// <summary>
/// Represents data label options for a bubble chart dataset.
/// </summary>
[AddedVersion("4.0.0")]
public class BubbleChartDatasetDataLabels : ChartDatasetDataLabels { }