namespace BlazorBootstrap;

public record BubbleChartDatasetData : ChartDatasetData
{
    #region Constructors

    public BubbleChartDatasetData(string? datasetLabel, BubbleChartDataPoint data) : base(datasetLabel, data) { }

    #endregion
}