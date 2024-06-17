namespace BlazorBootstrap;

public class LineChartDataset : ChartDataset
{
    #region Properties, Indexers

    /// <summary>
    /// Line dash.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<int>? BorderDash { get; set; }

    /// <summary>
    /// Line dash offset.
    /// </summary>
    public double BorderDashOffset { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LineChartDatasetDataLabels Datalabels { get; set; } = new();

    /// <summary>
    /// Both line and radar charts support a fill option on the dataset object
    /// which can be used to create area between two datasets or a dataset and
    /// a boundary, i.e. the scale origin, start or end.
    /// </summary>
    public bool Fill { get; set; }

    /// <summary>
    /// Configures the visibility state of the dataset. Set it to true, to hide the dataset from the chart.
    /// </summary>
    public new bool Hidden { get; set; }

    /// <summary>
    /// Hover line dash.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<int>? HoverBorderDash { get; set; }

    /// <summary>
    /// The label for the dataset which appears in the legend and tooltips.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    /// <summary>
    /// The fill color for points.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<string> PointBackgroundColor { get; set; } = new[] { "rgba(0, 0, 0, 0.1)" };

    /// <summary>
    /// The border color for points.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<string> PointBorderColor { get; set; } = new[] { "rgba(0, 0, 0, 0.1)" };

    /// <summary>
    /// The width of the point border in pixels.
    /// </summary>
    public IReadOnlyCollection<double> PointBorderWidth { get; set; } = new[] { 1d };

    /// <summary>
    /// The pixel size of the non-displayed point that reacts to mouse events.
    /// </summary>
    public IReadOnlyCollection<double> PointHitRadius { get; set; } = new[] { 1d };

    /// <summary>
    /// Point background color when hovered.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<string>? PointHoverBackgroundColor { get; set; }

    /// <summary>
    /// Point border color when hovered.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<string>? PointHoverBorderColor { get; set; }

    /// <summary>
    /// Border width of point when hovered.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<double> PointHoverBorderWidth { get; set; } = new[] { 1d };

    /// <summary>
    /// The radius of the point when hovered.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<int> PointHoverRadius { get; set; } = new[] { 1 }; // Default: 4

    /// <summary>
    /// The radius of the point shape. If set to 0, the point is not rendered.
    /// Default: 3
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<int> PointRadius { get; set; } = new[] { 1 }; // Default: 3

    /// <summary>
    /// The rotation of the point in degrees.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<int> PointRotation { get; set; } = new[] { 0 };

    /// <summary>
    /// Style of the point.
    /// Use 'circle', 'cross', 'crossRot', 'dash', 'line', 'rect', 'rectRounded', 'rectRot', 'star', and 'triangle' to style
    /// the point.
    /// </summary>
    public IReadOnlyCollection<string> PointStyle { get; set; } = new[] { "circle" };

    // Segment
    // https://www.chartjs.org/docs/latest/api/interfaces/LineControllerDatasetOptions.html#segment

    /// <summary>
    /// If false, the lines between points are not drawn.
    /// </summary>
    public bool ShowLine { get; set; } = true;

    /// <summary>
    /// If <see langword="true" />, lines will be drawn between points with no or null data.
    /// If false, points with null data will create a break in the line.
    /// Can also be a number specifying the maximum gap length to span.
    /// The unit of the value depends on the scale used.
    /// </summary>
    public bool SpanGaps { get; set; }

    /// <summary>
    /// <see langword="true" /> to show the line as a stepped line (tension will be ignored).
    /// </summary>
    public bool Stepped { get; set; }

    /// <summary>
    /// Bezier curve tension of the line. Set to 0 to draw straightlines.
    /// This option is ignored if monotone cubic interpolation is used.
    /// </summary>
    public double Tension { get; set; } = 0.2;

    /// <summary>
    /// The ID of the x axis to plot this dataset on.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? XAxisId { get; set; }

    /// <summary>
    /// The ID of the y axis to plot this dataset on.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? YAxisId { get; set; }

    #endregion
}

public class LineChartDatasetDataLabels
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the data label alignment.
    /// Possible values: start, center, and end.
    /// </summary>
    public string? Align { get; set; } = "start";

    /// <summary>
    /// Gets or sets the data label anchor.
    /// Possible values: start, center, and end.
    /// </summary>
    public string? Anchor { get; set; } = "start";

    #endregion
}
