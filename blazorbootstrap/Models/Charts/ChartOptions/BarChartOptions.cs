namespace BlazorBootstrap;

public class BarChartOptions : ChartOptions
{
    //hover -> mode, intersect
    //maintainAspectRatio
    //plugins -> title -> display, text

    /// <summary>
    /// The base axis of the chart. 'x' for vertical charts and 'y' for horizontal charts.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string IndexAxis { get; set; } = "x";

    public Interaction Interaction { get; set; } = new Interaction();

    public ChartLayout Layout { get; set; } = new ChartLayout();

    public Plugins Plugins { get; set; } = new Plugins();

    public Scales Scales { get; set; } = new Scales();

    //tooltips -> mode, intersect
}
