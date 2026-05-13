namespace BlazorBootstrap;

[AddedVersion("1.0.0")]
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
    /// Default value is <see langword="null"/>.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IndexAxis { get; set; }

    public Interaction Interaction { get; set; } = new();

    public ChartLayout Layout { get; set; } = new();

    [AddedVersion("1.10.2")]
    public LineChartPlugins Plugins { get; set; } = new();

    public Scales Scales { get; set; } = new();

    #endregion

    //tooltips -> mode, intersect
}
