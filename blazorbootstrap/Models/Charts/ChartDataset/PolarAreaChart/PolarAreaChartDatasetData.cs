namespace BlazorBootstrap;

public record PolarAreaChartDatasetData : ChartDatasetData
{
    #region Constructors

    public PolarAreaChartDatasetData(string? datasetLabel, double data, string? backgroundColor) : base(datasetLabel, data)
    {
        BackgroundColor = backgroundColor;
    }

    #endregion

    #region Properties, Indexers

    public string? BackgroundColor { get; init; }

    #endregion
}
