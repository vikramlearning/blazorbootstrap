namespace BlazorBootstrap;

public record DoughnutChartDatasetData : ChartDatasetData
{
    #region Constructors

    public DoughnutChartDatasetData(string? datasetLabel, double data, string? backgroundColor) : base(datasetLabel, data)
    {
        BackgroundColor = backgroundColor;
    }

    #endregion

    #region Properties, Indexers

    public string? BackgroundColor { get; init; }

    #endregion
}
