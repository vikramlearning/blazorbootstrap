namespace BlazorBootstrap;

public record DoughnutChartDatasetData : ChartDatasetData
{
    public DoughnutChartDatasetData(string? datasetLabel, double data, string? backgroundColor) : base(datasetLabel, data)
    {
        BackgroundColor = backgroundColor;
    }

    public string? BackgroundColor { get; init; }
}
