namespace BlazorBootstrap;

/// <summary>
/// Represents the data payload for a chart.
/// </summary>
[AddedVersion("1.0.0")]
public class ChartData
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the datasets.
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the chart datasets.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public List<IChartDataset>? Datasets { get; set; }

    /// <summary>
    /// Gets or sets the labels.
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the chart labels.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public List<string>? Labels { get; set; }

    #endregion
}
