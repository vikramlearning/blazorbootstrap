namespace BlazorBootstrap;

public class DoughnutChartDataset : ChartDataset
{
    #region Properties, Indexers

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DoughnutChartDatasetDataLabels Datalabels { get; set; } = new();

    /// <summary>
    /// The label for the dataset which appears in the legend and tooltips.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    #endregion
}

public class DoughnutChartDatasetDataLabels
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the data label anchor.
    /// Possible values: start, center, and end.
    /// </summary>
    public string? Anchor { get; set; } = "start";

    //public string? BackgroundColor { get; set; }

    public double? BorderWidth { get; set; } = 2;

    #endregion
}
