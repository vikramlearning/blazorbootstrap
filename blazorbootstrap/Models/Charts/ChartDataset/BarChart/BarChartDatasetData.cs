namespace BlazorBootstrap;

/// <summary>
/// Represents a bar chart data item for dynamic updates.
/// </summary>
[AddedVersion("1.10.0")]
public record BarChartDatasetData : ChartDatasetData
{
    #region Constructors

    public BarChartDatasetData(string? datasetLabel, double data) : base(datasetLabel, data) { }

    #endregion
}
