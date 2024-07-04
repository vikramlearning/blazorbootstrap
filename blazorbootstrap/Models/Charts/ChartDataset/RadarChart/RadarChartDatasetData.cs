namespace BlazorBootstrap;

public record RadarChartDatasetData : ChartDatasetData
{
    #region Constructors

    public RadarChartDatasetData(string? datasetLabel, double data) : base(datasetLabel, data) { }

    #endregion
}
