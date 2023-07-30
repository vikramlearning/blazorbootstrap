namespace BlazorBootstrap;

public record BarChartDatasetData : ChartDatasetData
{
    public BarChartDatasetData(string? datasetLabel, double data) : base(datasetLabel, data)
    {
    }
}
