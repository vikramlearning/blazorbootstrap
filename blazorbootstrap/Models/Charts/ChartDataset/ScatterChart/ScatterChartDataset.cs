﻿namespace BlazorBootstrap;

/// <summary>
/// Scatter charts are based on basic line charts with the x-axis changed to a linear axis.
/// To use a scatter chart, data must be passed as objects containing X and Y properties.
/// <see href="https://www.chartjs.org/docs/latest/charts/scatter.html#dataset-properties" />.
/// The scatter chart supports all the same properties as the line chart.
/// By default, the scatter chart will override the showLine property of the line chart to <see langword="false" />.
/// </summary>
public class ScatterChartDataset : ChartDataset<ScatterChartDataPoint>
{
    #region Properties, Indexers

    /// <summary>
    /// Cap style of the line.
    /// Supported values are 'butt', 'round', and 'square'.
    /// </summary>
    /// <remarks>
    /// Default value is 'butt'.
    /// </remarks>
    public string BorderCapStyle { get; set; } = "butt";

    /// <summary>
    /// Gets or sets the length and spacing of dashes.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<double>? BorderDash { get; set; }

    /// <summary>
    /// Offset for line dashes.
    /// </summary>
    /// <remarks>
    /// Default value is 0.0.
    /// </remarks>
    public double BorderDashOffset { get; set; }

    /// <summary>
    /// Line joint style.
    /// There are three possible values for this property: 'round', 'bevel', and 'miter'.
    /// </summary>
    /// <remarks>
    /// Default value is 'miter'.
    /// </remarks>
    public string BorderJoinStyle { get; set; } = "miter";

    /// <summary>
    /// <see href="https://www.chartjs.org/docs/latest/charts/line.html#cubicinterpolationmode" />.
    /// Supported values are 'default', and 'monotone'.
    /// </summary>
    /// <remarks>
    /// Default value is 'default'.
    /// </remarks>
    public string CubicInterpolationMode { get; set; } = "default";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public LineChartDatasetDataLabels Datalabels { get; set; } = new(); // TODO: add the reference link

    /// <summary>
    /// Draw the active points of a dataset over the other points of the dataset.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<bool>? DrawActiveElementsOnTop { get; set; }

    /// <summary>
    /// How to fill the area under the line.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    public bool Fill { get; set; }

    /// <summary>
    /// The line fill color when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HoverBackgroundColor { get; set; }

    /// <summary>
    /// Cap style of the line when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HoverBorderCapStyle { get; set; }

    /// <summary>
    /// The line color when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HoverBorderColor { get; set; }

    /// <summary>
    /// Gets or sets the length and spacing of dashes when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<double>? HoverBorderDash { get; set; }

    /// <summary>
    /// Offset for line dashes when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? HoverBorderDashOffset { get; set; }

    /// <summary>
    /// Line joint style.
    /// There are three possible values for this property: 'round', 'bevel', and 'miter'.
    /// </summary>
    /// <remarks>
    /// Default value is 'miter'.
    /// </remarks>
    public string HoverBorderJoinStyle { get; set; } = "miter";

    /// <summary>
    /// The base axis of the dataset. 'x' for horizontal lines and 'y' for vertical lines.
    /// </summary>
    /// <remarks>
    /// Default value is 'x'.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IndexAxis { get; set; }

    /// <summary>
    /// The fill color for points.
    /// </summary>
    /// <remarks>
    /// Default value is 'rgba(0, 0, 0, 0.1)'.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<string>? PointBackgroundColor { get; set; }

    /// <summary>
    /// The border color for points.
    /// </summary>
    /// <remarks>
    /// Default value is 'rgba(0, 0, 0, 0.1)'.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<string>? PointBorderColor { get; set; }

    /// <summary>
    /// The width of the point border in pixels.
    /// </summary>
    /// <remarks>
    /// Default value is 1.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<double>? PointBorderWidth { get; set; }

    /// <summary>
    /// The pixel size of the non-displayed point that reacts to mouse events.
    /// </summary>
    /// <remarks>
    /// Default value is 1.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<double>? PointHitRadius { get; set; }

    /// <summary>
    /// Point background color when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<string>? PointHoverBackgroundColor { get; set; }

    /// <summary>
    /// Point border color when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<string>? PointHoverBorderColor { get; set; }

    /// <summary>
    /// Border width of point when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is 1.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<double>? PointHoverBorderWidth { get; set; }

    /// <summary>
    /// The radius of the point when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is 4.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<double>? PointHoverRadius { get; set; }

    /// <summary>
    /// The radius of the point shape. If set to 0, the point is not rendered.
    /// </summary>
    /// <remarks>
    /// Default value is 3.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<double>? PointRadius { get; set; }

    /// <summary>
    /// The rotation of the point in degrees.
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<double>? PointRotation { get; set; }

    /// <summary>
    /// Style of the point.
    /// Supported values are 'circle', 'cross', 'crossRot', 'dash', 'line', 'rect', 'rectRounded', 'rectRot', 'star', and
    /// 'triangle' to style.
    /// the point.
    /// </summary>
    /// <remarks>
    /// Default value is 'circle'.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<string>? PointStyle { get; set; }

    //segment
    //https://www.chartjs.org/docs/latest/charts/line.html#segment

    /// <summary>
    /// If <see langword="false" />, the lines between points are not drawn.
    /// By default, the scatter chart will override the showLine property of the line chart to false.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    public bool ShowLine { get; } = false;

    /// <summary>
    /// If <see langword="true" />, lines will be drawn between points with no or null data.
    /// If <see langword="false" />, points with null data will create a break in the line.
    /// Can also be a number specifying the maximum gap length to span.
    /// The unit of the value depends on the scale used.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SpanGaps { get; set; }

    //stack
    //https://www.chartjs.org/docs/latest/charts/line.html#general

    /// <summary>
    /// true to show the line as a stepped line (tension will be ignored).
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    public bool Stepped { get; set; }

    /// <summary>
    /// Bezier curve tension of the line. Set to 0 to draw straight lines.
    /// This option is ignored if monotone cubic interpolation is used.
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    public double Tension { get; set; }

    /// <summary>
    /// The ID of the x axis to plot this dataset on.
    /// </summary>
    /// <remarks>
    /// Default value is 'first x axis'.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? XAxisID { get; set; }

    /// <summary>
    /// The ID of the y axis to plot this dataset on.
    /// </summary>
    /// <remarks>
    /// Default value is 'first y axis'.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? YAxisID { get; set; }

    #endregion
}

public class ScatterChartDatasetDataLabels : ChartDatasetDataLabels { }
