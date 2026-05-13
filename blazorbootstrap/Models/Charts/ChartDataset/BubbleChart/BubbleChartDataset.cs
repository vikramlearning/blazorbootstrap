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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? BackgroundColor { get; set; }

    [AddedVersion("4.0.0")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? BorderColor { get; set; }

    [AddedVersion("4.0.0")]
    public double BorderWidth { get; set; } = 3;

    [AddedVersion("4.0.0")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public BubbleChartDatasetDataLabels Datalabels { get; set; } = new();

    [AddedVersion("4.0.0")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<bool>? DrawActiveElementsOnTop { get; set; }

    [AddedVersion("4.0.0")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? HoverBackgroundColor { get; set; }

    [AddedVersion("4.0.0")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? HoverBorderColor { get; set; }

    [AddedVersion("4.0.0")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? HoverBorderWidth { get; set; }

    [AddedVersion("4.0.0")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? HitRadius { get; set; }

    [AddedVersion("4.0.0")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? HoverRadius { get; set; }

    [AddedVersion("4.0.0")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? PointStyle { get; set; }

    [AddedVersion("4.0.0")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? Radius { get; set; }

    [AddedVersion("4.0.0")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? Rotation { get; set; }

    #endregion
}

[AddedVersion("4.0.0")]
public class BubbleChartDatasetDataLabels : ChartDatasetDataLabels { }