﻿namespace BlazorBootstrap;

/// <summary>
/// The line chart allows a number of properties to be specified for each dataset. 
/// These are used to set display properties for a specific dataset. 
/// <see href="https://www.chartjs.org/docs/latest/charts/line.html#dataset-properties" />.
/// </summary>
public class LineChartDataset : ChartDataset
{
    #region Properties, Indexers

    /// <summary>
    /// Get or sets the line fill color.
    /// </summary>
    /// <remarks>
    /// Default value is 'rgba(0, 0, 0, 0.1)'.
    /// </remarks>
    public string BackgroundColor { get; set; } = "rgba(0, 0, 0, 0.1)";

    /// <summary>
    /// Cap style of the line.
    /// Supported values are 'butt', 'round', and 'square'.
    /// </summary>
    /// <remarks>
    /// Default value is 'butt'.
    /// </remarks>
    public string BorderCapStyle { get; set; } = "butt";

    /// <summary>
    /// Get or sets the line color.
    /// </summary>
    /// <remarks>
    /// Default value is 'rgba(0, 0, 0, 0.1)'.
    /// </remarks>
    public string BorderColor { get; set; } = "rgba(0, 0, 0, 0.1)";

    /// <summary>
    /// Gets or sets the length and spacing of dashes.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null"/>.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? BorderDash { get; set; }

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
    /// Gets or sets the line width (in pixels).
    /// </summary>
    /// <remarks>
    /// Default value is 3.
    /// </remarks>
    public double BorderWidth { get; set; } = 3;

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
    /// Default value is <see langword="null"/>.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<bool>? DrawActiveElementsOnTop { get; set; }

    /// <summary>
    /// How to fill the area under the line.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false"/>.
    /// </remarks>
    public bool Fill { get; set; }

    /// <summary>
    /// The line fill color when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null"/>.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HoverBackgroundColor { get; set; }

    /// <summary>
    /// Cap style of the line when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null"/>.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HoverBorderCapStyle { get; set; }

    /// <summary>
    /// The line color when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null"/>.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HoverBorderColor { get; set; }

    /// <summary>
    /// Gets or sets the length and spacing of dashes when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null"/>.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? HoverBorderDash { get; set; }

    /// <summary>
    /// Offset for line dashes when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null"/>.
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
    /// The bar border width when hovered (in pixels) when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null"/>.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? HoverBorderWidth { get; set; }

    /// <summary>
    /// The base axis of the dataset. 'x' for horizontal lines and 'y' for vertical lines.
    /// </summary>
    /// <remarks>
    /// Default value is 'x'.
    /// </remarks>
    public string IndexAxis { get; set; } = "x";

    /// <summary>
    /// The fill color for points.
    /// </summary>
    /// <remarks>
    /// Default value is 'rgba(0, 0, 0, 0.1)'.
    /// </remarks>
    public string PointBackgroundColor { get; set; } = "rgba(0, 0, 0, 0.1)";

    /// <summary>
    /// The border color for points.
    /// </summary>
    /// <remarks>
    /// Default value is 'rgba(0, 0, 0, 0.1)'.
    /// </remarks>
    public string PointBorderColor { get; set; } = "rgba(0, 0, 0, 0.1)";

    /// <summary>
    /// The width of the point border in pixels.
    /// </summary>
    /// <remarks>
    /// Default value is 1.
    /// </remarks>
    public double PointBorderWidth { get; set; } = 1;

    /// <summary>
    /// The pixel size of the non-displayed point that reacts to mouse events.
    /// </summary>
    /// <remarks>
    /// Default value is 1.
    /// </remarks>
    public double PointHitRadius { get; set; } = 1;

    /// <summary>
    /// Point background color when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null"/>.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PointHoverBackgroundColor { get; set; }

    /// <summary>
    /// Point border color when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null"/>.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PointHoverBorderColor { get; set; }

    /// <summary>
    /// Border width of point when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is 1.
    /// </remarks>
    public double PointHoverBorderWidth { get; set; } = 1;

    /// <summary>
    /// The radius of the point when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is 4.
    /// </remarks>
    public double PointHoverRadius { get; set; } = 4;

    /// <summary>
    /// The radius of the point shape. If set to 0, the point is not rendered.
    /// </summary>
    /// <remarks>
    /// Default value is 3.
    /// </remarks>
    public double PointRadius { get; set; } = 3;

    /// <summary>
    /// The rotation of the point in degrees.
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    public double PointRotation { get; set; } = 0;

    /// <summary>
    /// Style of the point.
    /// Supported values are 'circle', 'cross', 'crossRot', 'dash', 'line', 'rect', 'rectRounded', 'rectRot', 'star', and 'triangle' to style.
    /// the point.
    /// </summary>
    /// <remarks>
    /// Default value is 'circle'.
    /// </remarks>
    public string PointStyle { get; set; } = "circle";

    //segment
    //https://www.chartjs.org/docs/latest/charts/line.html#segment

    /// <summary>
    /// If <see langword="false" />, the lines between points are not drawn.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true"/>.
    /// </remarks>
    public bool ShowLine { get; set; } = true;

    /// <summary>
    /// If <see langword="true" />, lines will be drawn between points with no or null data.
    /// If <see langword="false" />, points with null data will create a break in the line.
    /// Can also be a number specifying the maximum gap length to span.
    /// The unit of the value depends on the scale used.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null"/>.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SpanGaps { get; set; }

    //stack
    //https://www.chartjs.org/docs/latest/charts/line.html#general

    /// <summary>
    /// true to show the line as a stepped line (tension will be ignored).
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false"/>.
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

public class LineChartDatasetDataLabels
{
    #region Fields and Constants

    private Alignment alignment;

    private Anchor anchor;

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the data labels alignment. 
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Alignment.Center"/>.
    /// </remarks>
    [JsonIgnore]
    public Alignment Alignment
    {
        get => alignment;
        set
        {
            alignment = value;
            DataLabelsAlignment = value.ToChartDatasetDataLabelAlignmentString();
        }
    }

    /// <summary>
    /// Gets or sets the data labels anchor.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Anchor.None"/>.
    /// </remarks>
    [JsonIgnore]
    public Anchor Anchor
    {
        get => anchor;
        set
        {
            anchor = value;
            DataLabelsAnchor = value.ToChartDatasetDataLabelAnchorString();
        }
    }

    /// <summary>
    /// Gets or sets the data labels alignment.
    /// Possible values: start, center, and end.
    /// </summary>
    [JsonPropertyName("align")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DataLabelsAlignment { get; private set; }

    /// <summary>
    /// Gets or sets the data labels anchor.
    /// Possible values: start, center, and end.
    /// </summary>
    [JsonPropertyName("anchor")]
    public string? DataLabelsAnchor { get; private set; }

    #endregion
}
