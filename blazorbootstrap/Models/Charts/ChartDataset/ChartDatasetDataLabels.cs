namespace BlazorBootstrap;

/// <summary>
/// Highly customizable Chart.js plugin that displays labels on data for any type of charts.
/// <see href="https://chartjs-plugin-datalabels.netlify.app/guide/options.html#options" />.
/// </summary>
[AddedVersion("3.0.0")]
public class ChartDatasetDataLabels
{
    #region Fields and Constants

    private Alignment alignment;

    private Anchor anchor;

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the data labels alignment. 
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Alignment.None"/>.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [JsonIgnore]
    public Alignment Alignment
    {
        get => alignment;
        set
        {
            alignment = value;
            DataLabelsAlignment = value.ToChartDatasetDataLabelAlignmentString();
        }
    }

    /// <summary>
    /// Gets or sets the data labels anchor.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Anchor.None"/>.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [JsonIgnore]
    public Anchor Anchor
    {
        get => anchor;
        set
        {
            anchor = value;
            DataLabelsAnchor = value.ToChartDatasetDataLabelAnchorString();
        }
    }

    //public string? BackgroundColor { get; set; }

    [AddedVersion("3.0.0")]
    public double BorderWidth { get; set; } = 2;

    /// <summary>
    /// Gets or sets the data labels alignment.
    /// Possible values: start, center, and end.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null"/>.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [JsonPropertyName("align")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DataLabelsAlignment { get; private set; }

    /// <summary>
    /// Gets or sets the data labels anchor.
    /// Possible values: start, center, and end.
    /// </summary>
    [AddedVersion("3.0.0")]
    [JsonPropertyName("anchor")]
    public string? DataLabelsAnchor { get; private set; }

    #endregion
}
