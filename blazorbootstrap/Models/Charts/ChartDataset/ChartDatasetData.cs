namespace BlazorBootstrap;

public interface IChartDatasetData { }

public record ChartDatasetData : IChartDatasetData
{
    #region Constructors

    public ChartDatasetData(string? datasetLabel, object? data)
    {
        DatasetLabel = datasetLabel;
        Data = data;
    }

    #endregion

    #region Properties, Indexers

    public object? Data { get; init; }

    public string? DatasetLabel { get; init; }

    #endregion
}
