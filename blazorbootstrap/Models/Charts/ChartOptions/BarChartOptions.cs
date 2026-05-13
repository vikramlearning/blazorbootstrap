namespace BlazorBootstrap;

[AddedVersion("1.0.0")]
public class BarChartOptions : ChartOptions
{
    #region Properties, Indexers

    //hover -> mode, intersect
    //maintainAspectRatio
    //plugins -> title -> display, text

    /// <summary>
    /// The base axis of the chart. 'x' for vertical charts and 'y' for horizontal charts.
    /// Supported values are 'x' and 'y'.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null"/>.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the base axis of the chart.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IndexAxis { get; set; }

    [AddedVersion("1.0.0")]
    [Description("Gets or sets the interaction configuration.")]
    public Interaction Interaction { get; set; } = new();

    [AddedVersion("1.0.0")]
    [Description("Gets or sets the layout configuration.")]
    public ChartLayout Layout { get; set; } = new();

    [AddedVersion("1.10.2")]
    [Description("Gets or sets the plugin configuration.")]
    public BarChartPlugins Plugins { get; set; } = new();

    [AddedVersion("1.0.0")]
    [Description("Gets or sets the scale configuration.")]
    public Scales Scales { get; set; } = new();

    #endregion

    //tooltips -> mode, intersect
}
