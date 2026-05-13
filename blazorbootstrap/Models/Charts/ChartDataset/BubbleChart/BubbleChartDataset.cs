namespace BlazorBootstrap;

/// <summary>
/// A bubble chart is used to display three dimensions of data at the same time.
/// The location of the bubble is determined by the first two dimensions and the corresponding horizontal and vertical axes.
/// The third dimension is represented by the size of the individual bubbles.
/// <see href="https://www.chartjs.org/docs/latest/charts/bubble.html#dataset-properties" />.
/// </summary>
public class BubbleChartDataset : ChartDataset<BubbleChartDataPoint>
{
    #region Constructors

    public BubbleChartDataset()
    {
        Type = "bubble";
    }

    #endregion

    #region Properties, Indexers

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? BackgroundColor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? BorderColor { get; set; }

    public double BorderWidth { get; set; } = 3;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public BubbleChartDatasetDataLabels Datalabels { get; set; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<bool>? DrawActiveElementsOnTop { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? HoverBackgroundColor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? HoverBorderColor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? HoverBorderWidth { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? HitRadius { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? HoverRadius { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? PointStyle { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? Radius { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? Rotation { get; set; }

    #endregion
}

public class BubbleChartDatasetDataLabels : ChartDatasetDataLabels { }