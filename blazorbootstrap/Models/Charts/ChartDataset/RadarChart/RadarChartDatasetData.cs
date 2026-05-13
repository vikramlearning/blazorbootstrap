namespace BlazorBootstrap;

/// <summary>
/// Represents a radar chart data item for dynamic updates.
/// </summary>
[AddedVersion("3.0.0")]
public record RadarChartDatasetData : ChartDatasetData
{
    #region Constructors

    public RadarChartDatasetData(
        string? datasetLabel, 
        double data,
        string? backgroundColor = null,
        string? borderColor = null) : base(datasetLabel, data)
    {
        BackgroundColor = backgroundColor;
        BorderColor = borderColor;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the background color for the generated point.
    /// </summary>
    [AddedVersion("3.0.0")]
    public string? BackgroundColor { get; init; }
    /// <summary>
    /// Gets the border color for the generated point.
    /// </summary>
    [AddedVersion("3.0.0")]
    public string? BorderColor { get; init; }

    #endregion
}
