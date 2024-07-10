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

    public string? DatasetLabel { get; init; }

    public object? Data { get; init; }

    #endregion
}
