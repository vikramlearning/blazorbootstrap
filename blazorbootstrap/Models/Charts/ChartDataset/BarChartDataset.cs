using System.Text.Json.Serialization;

namespace BlazorBootstrap;

public class BarChartDataset : ChartDataset
{
    public double BarPercentage { get; set; } = 0.8;

    public int BorderRadius { get; set; }

    public double CategoryPercentage { get; set; } = 0.8;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Label { get; set; }
}
