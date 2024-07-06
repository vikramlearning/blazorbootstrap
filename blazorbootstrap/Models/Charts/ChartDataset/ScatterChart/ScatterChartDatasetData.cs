namespace BlazorBootstrap;

public record ScatterChartDatasetData : ChartDatasetData
{
    #region Constructors

    public ScatterChartDatasetData(string? datasetLabel, double data) : base(datasetLabel, data) { }

    #endregion
}
