namespace BlazorBootstrap;

/// <summary>
/// Represents a bubble chart data item for dynamic updates.
/// </summary>
[AddedVersion("4.0.0")]
public record BubbleChartDatasetData : ChartDatasetData
{
    #region Constructors

    public BubbleChartDatasetData(string? datasetLabel, BubbleChartDataPoint data) : base(datasetLabel, data) { }

    #endregion
}