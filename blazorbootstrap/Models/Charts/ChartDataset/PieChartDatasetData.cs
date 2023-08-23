namespace BlazorBootstrap;

public record PieChartDatasetData : ChartDatasetData
{
    #region Constructors

    public PieChartDatasetData(string? datasetLabel, double data, string? backgroundColor) : base(datasetLabel, data)
    {
        BackgroundColor = backgroundColor;
    }

    #endregion

    #region Properties, Indexers

    public string? BackgroundColor { get; init; }

    #endregion
}