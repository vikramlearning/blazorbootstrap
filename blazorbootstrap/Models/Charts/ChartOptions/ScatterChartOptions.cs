namespace BlazorBootstrap;

/// <summary>
/// Represents configuration options for a scatter chart.
/// </summary>
[AddedVersion("3.0.0")]
public class ScatterChartOptions : ChartOptions
{
    #region Properties, Indexers

    //hover -> mode, intersect
    //maintainAspectRatio
    //plugins -> title -> display, text

    /// <summary>
    /// The base axis of the dataset. 'x' for horizontal lines and 'y' for vertical lines.
    /// </summary>
    /// <remarks>
    /// Default value is 'x'.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the base axis of the dataset.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IndexAxis { get; set; }

    [AddedVersion("3.0.0")]
    [Description("Gets or sets the interaction configuration.")]
    public Interaction Interaction { get; set; } = new();

    [AddedVersion("3.0.0")]
    [Description("Gets or sets the layout configuration.")]
    public ChartLayout Layout { get; set; } = new();

    [AddedVersion("3.0.0")]
    [Description("Gets or sets the plugin configuration.")]
    public ScatterChartPlugins Plugins { get; set; } = new();

    [AddedVersion("3.0.0")]
    [Description("Gets or sets the scale configuration.")]
    public Scales Scales { get; set; } = new();

    #endregion

    //tooltips -> mode, intersect
}
