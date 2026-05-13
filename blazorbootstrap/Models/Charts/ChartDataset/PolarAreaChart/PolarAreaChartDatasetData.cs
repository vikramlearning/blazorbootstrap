namespace BlazorBootstrap;

/// <summary>
/// Represents a polar area chart data item for dynamic updates.
/// </summary>
[AddedVersion("3.0.0")]
public record PolarAreaChartDatasetData : ChartDatasetData
{
    #region Constructors

    public PolarAreaChartDatasetData(
        string? datasetLabel, 
        double data, 
        string? backgroundColor, 
        string? borderColor) : base(datasetLabel, data)
    {
        BackgroundColor = backgroundColor;
        BorderColor = borderColor;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the background color for the generated segment.
    /// </summary>
    [AddedVersion("3.0.0")]
    public string? BackgroundColor { get; init; }
    /// <summary>
    /// Gets the border color for the generated segment.
    /// </summary>
    [AddedVersion("3.0.0")]
    public string? BorderColor { get; init; }

    #endregion
}
