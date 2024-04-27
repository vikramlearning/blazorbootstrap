namespace BlazorBootstrap;

public interface IChartDataset { }

public class ChartDataset : IChartDataset
{
    #region Constructors

    public ChartDataset()
    {
        Oid = Guid.NewGuid();
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Get or sets the BackgroundColor.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? BackgroundColor { get; set; }

    /// <summary>
    /// Get or sets the BorderColor.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? BorderColor { get; set; }

    /// <summary>
    /// Get or sets the BorderWidth.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? BorderWidth { get; set; }

    /// <summary>
    /// How to clip relative to chartArea. Positive value allows overflow, negative value clips that many pixels inside
    /// chartArea. 0 = clip at chartArea.
    /// Clipping can also be configured per side: clip: {left: 5, top: false, right: -2, bottom: 0}
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Clip { get; set; }

    /// <summary>
    /// Get or sets the Data.
    /// </summary>
    public List<double>? Data { get; set; }

    /// <summary>
    /// Configures the visibility state of the dataset. Set it to true, to hide the dataset from the chart.
    /// </summary>
    public bool Hidden { get; set; }

    /// <summary>
    /// Get or sets the HoverBackgroundColor.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? HoverBackgroundColor { get; set; }

    /// <summary>
    /// Get or sets the HoverBorderColor.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? HoverBorderColor { get; set; }

    /// <summary>
    /// Get or sets the HoverBorderWidth.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? HoverBorderWidth { get; set; }

    /// <summary>
    /// Get unique object id.
    /// </summary>
    public Guid Oid { get; private set; }

    /// <summary>
    /// Gets or sets the chart type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; protected set; }

    #endregion
}
