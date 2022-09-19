namespace BlazorBootstrap;

public class LineChartDataset : ChartDataset
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public new string BackgroundColor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public new string BorderColor { get; set; }

    public new double BorderWidth { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public new string HoverBackgroundColor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public new string HoverBorderColor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public new string HoverBorderWidth { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Label { get; set; }

    /// <summary>
    /// The fill color for points.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public new List<string> PointBackgroundColor { get; set; } = new List<string> { "rgba(0, 0, 0, 0.1)" };

    /// <summary>
    /// The border color for points.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public new List<string> PointBorderColor { get; set; } = new List<string> { "rgba(0, 0, 0, 0.1)" };

    /// <summary>
    /// The width of the point border in pixels
    /// </summary>
    public new List<int> PointBorderWidth { get; set; } = new List<int> { 1 };

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public new string PointHoverBackgroundColor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public new string PointHoverBorderColor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public new List<int> PointHoverBorderWidth { get; set; } = new List<int> { 1 };

    public List<int> PointHoverRadius { get; set; } = new List<int> { 4 };

    /// <summary>
    /// The radius of the point shape. If set to 0, the point is not rendered.
    /// Default: 3
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<int> PointRadius { get; set; } = new List<int> { 3 };

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<int> PointRotation { get; set; } = new List<int> { 0 };

    public double Tension { get; set; } = 0.5;
}
