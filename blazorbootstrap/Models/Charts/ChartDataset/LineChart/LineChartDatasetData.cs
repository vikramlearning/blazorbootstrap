namespace BlazorBootstrap;

/// <summary>
/// Represents a line chart data item for dynamic updates.
/// </summary>
[AddedVersion("1.10.0")]
public record LineChartDatasetData : ChartDatasetData
{
    #region Constructors

    public LineChartDatasetData(string? datasetLabel, double data) : base(datasetLabel, data) { }

    #endregion
}
