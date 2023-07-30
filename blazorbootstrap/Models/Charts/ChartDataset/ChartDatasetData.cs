namespace BlazorBootstrap;

public interface IChartDatasetData { }

public record ChartDatasetData : IChartDatasetData
{
    public ChartDatasetData(string? datasetLabel, double data)
    {
        DatasetLabel = datasetLabel;
        Data = data;
    }

    public string? DatasetLabel { get; init; }
    public double Data { get; init; }
}
