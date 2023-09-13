namespace BlazorBootstrap;

public class BarChartOptions : ChartOptions
{
    #region Properties, Indexers

    //hover -> mode, intersect
    //maintainAspectRatio
    //plugins -> title -> display, text

    /// <summary>
    /// The base axis of the chart. 'x' for vertical charts and 'y' for horizontal charts.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string IndexAxis { get; set; } = "x";

    public Interaction Interaction { get; set; } = new();

    public ChartLayout Layout { get; set; } = new();

    public BarChartPlugins Plugins { get; set; } = new();

    public Scales Scales { get; set; } = new();

    #endregion

    //tooltips -> mode, intersect
}
