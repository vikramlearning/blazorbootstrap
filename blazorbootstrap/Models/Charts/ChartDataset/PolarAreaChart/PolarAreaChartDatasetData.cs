namespace BlazorBootstrap;

public record PolarAreaChartDatasetData : ChartDatasetData
{
    #region Constructors

    public PolarAreaChartDatasetData(string? datasetLabel, double data, string? backgroundColor, string? borderColor) : base(datasetLabel, data)
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
