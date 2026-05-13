namespace BlazorBootstrap;

public class BubbleChartOptions : ChartOptions
{
    #region Properties, Indexers

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IndexAxis { get; set; }

    public Interaction Interaction { get; set; } = new();

    public ChartLayout Layout { get; set; } = new();

    public BubbleChartPlugins Plugins { get; set; } = new();

    public Scales Scales { get; set; } = new();

    #endregion
}