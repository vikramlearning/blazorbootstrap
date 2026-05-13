namespace BlazorBootstrap;

[AddedVersion("4.0.0")]
public class BubbleChartOptions : ChartOptions
{
    #region Properties, Indexers

    [AddedVersion("4.0.0")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IndexAxis { get; set; }

    [AddedVersion("4.0.0")]
    public Interaction Interaction { get; set; } = new();

    [AddedVersion("4.0.0")]
    public ChartLayout Layout { get; set; } = new();

    [AddedVersion("4.0.0")]
    public BubbleChartPlugins Plugins { get; set; } = new();

    [AddedVersion("4.0.0")]
    public Scales Scales { get; set; } = new();

    #endregion
}