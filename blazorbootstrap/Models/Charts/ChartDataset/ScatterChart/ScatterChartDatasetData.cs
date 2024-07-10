namespace BlazorBootstrap;

public record ScatterChartDatasetData : ChartDatasetData
{
    #region Constructors

    public ScatterChartDatasetData(string? datasetLabel, ScatterChartDataPoint? data) : base(datasetLabel, data) { }

    #endregion
}
