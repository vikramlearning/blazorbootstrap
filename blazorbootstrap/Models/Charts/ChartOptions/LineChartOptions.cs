namespace BlazorBootstrap;

public class LineChartOptions : ChartOptions
{
    #region Properties, Indexers

    //hover -> mode, intersect
    //maintainAspectRatio
    //plugins -> title -> display, text

    /// <summary>
    /// The base axis of the dataset. 'x' for horizontal lines and 'y' for vertical lines.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IndexAxis { get; set; }

    public Interaction Interaction { get; set; } = new();

    public ChartLayout Layout { get; set; } = new();

    public LineChartPlugins Plugins { get; set; } = new();

    public LineChartScales Scales { get; set; } = new();

    #endregion

    //tooltips -> mode, intersect
}
