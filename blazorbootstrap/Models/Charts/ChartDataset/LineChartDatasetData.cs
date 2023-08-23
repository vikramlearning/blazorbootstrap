namespace BlazorBootstrap;

public record LineChartDatasetData : ChartDatasetData
{
    #region Constructors

    public LineChartDatasetData(string? datasetLabel, double data) : base(datasetLabel, data) { }

    #endregion
}