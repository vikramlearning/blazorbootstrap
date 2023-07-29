namespace BlazorBootstrap;

public record LineChartDatasetData : ChartDatasetData
{
    public LineChartDatasetData(string? datasetLabel, double data) : base(datasetLabel, data)
    {
    }
}
