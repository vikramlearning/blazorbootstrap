namespace BlazorBootstrap;

public record PieChartDatasetData : ChartDatasetData
{
    public PieChartDatasetData(string? datasetLabel, double data, string? backgroundColor) : base(datasetLabel, data)
    {
        BackgroundColor = backgroundColor;
    }

    public string? BackgroundColor { get; init; }
}
