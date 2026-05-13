namespace BlazorBootstrap;

public class ChartData
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the datasets.
    /// </summary>
    [AddedVersion("1.0.0")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public List<IChartDataset>? Datasets { get; set; }

    /// <summary>
    /// Gets or sets the labels.
    /// </summary>
    [AddedVersion("1.0.0")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public List<string>? Labels { get; set; }

    #endregion
}
