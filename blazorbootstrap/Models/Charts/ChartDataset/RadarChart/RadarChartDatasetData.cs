namespace BlazorBootstrap;

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

    public string? BackgroundColor { get; init; }
    public string? BorderColor { get; init; }

    #endregion
}
