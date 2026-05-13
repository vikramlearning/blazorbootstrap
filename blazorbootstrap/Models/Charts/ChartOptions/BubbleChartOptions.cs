namespace BlazorBootstrap;

/// <summary>
/// Represents configuration options for a bubble chart.
/// </summary>
[AddedVersion("4.0.0")]
public class BubbleChartOptions : ChartOptions
{
    #region Properties, Indexers

    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the base axis of the dataset.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IndexAxis { get; set; }

    [AddedVersion("4.0.0")]
    [Description("Gets or sets the interaction configuration.")]
    public Interaction Interaction { get; set; } = new();

    [AddedVersion("4.0.0")]
    [Description("Gets or sets the layout configuration.")]
    public ChartLayout Layout { get; set; } = new();

    [AddedVersion("4.0.0")]
    [Description("Gets or sets the plugin configuration.")]
    public BubbleChartPlugins Plugins { get; set; } = new();

    [AddedVersion("4.0.0")]
    [Description("Gets or sets the scale configuration.")]
    public Scales Scales { get; set; } = new();

    #endregion
}