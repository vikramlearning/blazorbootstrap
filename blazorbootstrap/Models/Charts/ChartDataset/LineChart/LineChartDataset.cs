namespace BlazorBootstrap;

/// <summary>
/// The line chart allows a number of properties to be specified for each dataset.
/// These are used to set display properties for a specific dataset.
/// <see href="https://www.chartjs.org/docs/latest/charts/line.html#dataset-properties" />.
/// </summary>
public class LineChartDataset : ChartDataset<double?>
{
    #region Methods

    /// <summary>
    /// Fill between this dataset and the other dataset, specified by absolute index (zero based) or relative index.
    /// </summary>
    /// <param name="index">The index of the dataset to fill to</param>
    /// <param name="relativeIndex">Whether the specified index is relative or absolute (zero based)</param>
    /// <returns>The dataset, for method chaining</returns>
    /// <exception cref="ArgumentException">If the relative index is zero.</exception>
    public LineChartDataset FillToDataset(int index, bool relativeIndex = false)
    {
        if (relativeIndex && index == 0)
            throw new ArgumentException("The relative index must be non-zero.");

        Fill = relativeIndex ? index.ToString("+0;-0", CultureInfo.InvariantCulture) : index;

        return this;
    }

    /// <summary>
    /// Fill between this dataset and the other dataset, specified by passing a dataset in the same chart.
    /// </summary>
    /// <param name="chartData">The chart data of the chart both datasets live in.</param>
    /// <param name="dataset">The other dataset to fill to.</param>
    /// <param name="relativeIndex">Whether to specify the fill index relative ("+/-n" string) or absolute (as zero-based int)</param>
    /// <returns>The dataset, for method chaining</returns>
    /// <exception cref="ArgumentException">If any of the datasets is not in the chart data, or if both datasets are the same.</exception>
    public LineChartDataset FillToDataset(ChartData chartData, IChartDataset dataset, bool relativeIndex = false)
    {
        var index = chartData?.Datasets?.IndexOf(dataset) ?? -1;

        if (index < 0)
            throw new ArgumentException("The dataset is not in the chart data.");

        if (relativeIndex)
        {
            var myIndex = relativeIndex ? chartData.Datasets.IndexOf(this) : 0;

            if (myIndex < 0)
                throw new ArgumentException("The dataset is not in the chart data.");

            if (myIndex == index)
                throw new ArgumentException("The dataset is the same as this dataset.");

            Fill = (index - myIndex).ToString("+0;-0", CultureInfo.InvariantCulture);
        }
        else
        {
            Fill = index;
        }

        return this;
    }

    /// <summary>
    /// Fills between the current dataset and the top of the chart (fill: 'end').
    /// </summary>
    /// <returns>The dataset, for method chaining</returns>
    public LineChartDataset FillToEnd()
    {
        Fill = "end";

        return this;
    }

    /// <summary>
    /// Fills between the current dataset and the origin. For legacy reasons, this is the same as fill: true.
    /// </summary>
    /// <returns>The dataset, for method chaining</returns>
    public LineChartDataset FillToOrigin()
    {
        Fill = "origin";

        return this;
    }

    /// <summary>
    /// Fill to the line below the current dataset (fill: 'stack').
    /// </summary>
    /// <returns>The dataset, for method chaining</returns>
    public LineChartDataset FillToStackedValueBelow()
    {
        Fill = "stack";

        return this;
    }

    /// <summary>
    /// Fills between the current dataset and the start (fill: 'start').
    /// </summary>
    /// <returns>The dataset, for method chaining</returns>
    public LineChartDataset FillToStart()
    {
        Fill = "start";

        return this;
    }

    /// <summary>
    /// Fill to the line of the given constant value.
    /// </summary>
    /// <param name="value">The value to fill to</param>
    /// <returns>The dataset, for method chaining</returns>
    public LineChartDataset FillToValue(double value)
    {
        Fill = new { value };

        return this;
    }

    #endregion

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
    /// Default value is <see langword="null" />.
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
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<bool>? DrawActiveElementsOnTop { get; set; }

    /// <summary>
    /// How to fill the area under the line.
    /// <see href="https://www.chartjs.org/docs/latest/charts/line.html#line-styling" />
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    public object Fill { get; set; } = false;

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
    public List<double>? HoverBorderDash { get; set; }

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
    /// The bar border width when hovered (in pixels) when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? HoverBorderWidth { get; set; }

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
    public List<string>? PointBackgroundColor { get; set; }

    /// <summary>
    /// The border color for points.
    /// </summary>
    /// <remarks>
    /// Default value is 'rgba(0, 0, 0, 0.1)'.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? PointBorderColor { get; set; }

    /// <summary>
    /// The width of the point border in pixels.
    /// </summary>
    /// <remarks>
    /// Default value is 1.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? PointBorderWidth { get; set; }

    /// <summary>
    /// The pixel size of the non-displayed point that reacts to mouse events.
    /// </summary>
    /// <remarks>
    /// Default value is 1.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? PointHitRadius { get; set; }

    /// <summary>
    /// Point background color when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? PointHoverBackgroundColor { get; set; }

    /// <summary>
    /// Point border color when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? PointHoverBorderColor { get; set; }

    /// <summary>
    /// Border width of point when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is 1.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? PointHoverBorderWidth { get; set; }

    /// <summary>
    /// The radius of the point when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is 4.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? PointHoverRadius { get; set; }

    /// <summary>
    /// The radius of the point shape. If set to 0, the point is not rendered.
    /// </summary>
    /// <remarks>
    /// Default value is 3.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? PointRadius { get; set; }

    /// <summary>
    /// The rotation of the point in degrees.
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? PointRotation { get; set; }

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
    public List<string>? PointStyle { get; set; }

    //segment
    //https://www.chartjs.org/docs/latest/charts/line.html#segment

    /// <summary>
    /// If <see langword="false" />, the lines between points are not drawn.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    public bool ShowLine { get; set; } = true;

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

public class LineChartDatasetDataLabels : ChartDatasetDataLabels { }
