namespace BlazorBootstrap;

public class BarChartDataset : ChartDataset
{
    #region Properties, Indexers

    /// <summary>
    /// Percent (0-1) of the available width each bar should be within the category width.
    /// 1.0 will take the whole category width and put the bars right next to each other.
    /// </summary>
    public double BarPercentage { get; set; } = 0.8;

    /// <summary>
    /// Border radius
    /// </summary>
    public int BorderRadius { get; set; }

    //BarThickness
    //https://www.chartjs.org/docs/latest/api/interfaces/BarControllerDatasetOptions.html#barthickness

    //BorderSkipped
    //https://www.chartjs.org/docs/latest/api/interfaces/BarControllerDatasetOptions.html#borderskipped

    /// <summary>
    /// Percent (0-1) of the available width each category should be within the sample width.
    /// </summary>
    public double CategoryPercentage { get; set; } = 0.8;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public BarChartDatasetDataLabels Datalabels { get; set; } = new();

    /// <summary>
    /// The label for the dataset which appears in the legend and tooltips.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    //MaxBarThickness
    //https://www.chartjs.org/docs/latest/api/interfaces/BarControllerDatasetOptions.html#maxbarthickness

    //MinBarLength
    //https://www.chartjs.org/docs/latest/api/interfaces/BarControllerDatasetOptions.html#minbarlength

    /// <summary>
    /// The ID of the x axis to plot this dataset on.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? XAxisID { get; set; }

    /// <summary>
    /// The ID of the y axis to plot this dataset on.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? YAxisID { get; set; }

    #endregion
}

public class BarChartDatasetDataLabels
{
    #region Properties, Indexers

    public string? Align { get; set; } = "center";
    public string? Anchor { get; set; } = "center";

    #endregion
}
