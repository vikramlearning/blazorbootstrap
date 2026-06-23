namespace BlazorBootstrap;

/// <summary>
/// Represents a scatter chart data item for dynamic updates.
/// </summary>
[AddedVersion("3.0.0")]
public record ScatterChartDatasetData : ChartDatasetData
{
    #region Constructors

    public ScatterChartDatasetData(string? datasetLabel, ScatterChartDataPoint? data) : base(datasetLabel, data) { }

    #endregion
}
