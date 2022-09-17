namespace BlazorBootstrap;

public class BarChartDataset : ChartDataset
{
    public double BarPercentage { get; set; } = 0.8;
    public double CategoryPercentage { get; set; } = 0.8;
    public string Label { get; set; }
}
