namespace BlazorBootstrap;

/// <summary>
/// Represents a pie chart data item for dynamic updates.
/// </summary>
[AddedVersion("1.10.0")]
public record PieChartDatasetData : ChartDatasetData
{
    #region Constructors

    public PieChartDatasetData(string? datasetLabel, double data, string? backgroundColor) : base(datasetLabel, data)
    {
        BackgroundColor = backgroundColor;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the background color for the generated arc.
    /// </summary>
    [AddedVersion("1.10.0")]
    public string? BackgroundColor { get; init; }

    #endregion
}
