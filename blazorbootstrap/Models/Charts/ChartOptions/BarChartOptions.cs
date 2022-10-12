namespace BlazorBootstrap;

public class BarChartOptions : ChartOptions
{
    /// <summary>
    /// The base axis of the chart. 'x' for vertical charts and 'y' for horizontal charts.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string IndexAxis { get; set; } = "x";
}
