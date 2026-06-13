namespace BlazorBootstrap;

/// <summary>
/// The bar chart allows a number of properties to be specified for each dataset.
/// These are used to set display properties for a specific dataset.
/// <see href="https://www.chartjs.org/docs/latest/charts/bar.html#dataset-properties" />
/// <seealso href="https://www.chartjs.org/docs/latest/charts/bar.html#general" />
/// </summary>
[AddedVersion("1.0.0")]
public class BarChartDataset : ChartDataset<double?>
{
    #region Constructors

    public BarChartDataset()
    {
        Type = "bar";
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// The bar background color.
    /// </summary>
    /// <remarks>
    /// Default value is 'rgba(0, 0, 0, 0.1)'.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue("rgba(0, 0, 0, 0.1)")]
    [Description("Gets or sets the bar background color.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? BackgroundColor { get; set; }

    /// <summary>
    /// Percent (0-1) of the available width each bar should be within the category width.
    /// 1.0 will take the whole category width and put the bars right next to each other.
    /// </summary>
    /// <remarks>
    /// Default value is 0.9.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(0.9d)]
    [Description("Gets or sets the percentage of the category width used by each bar.")]
    public double BarPercentage { get; set; } = 0.9;

    /// <summary>
    /// It is applied to the width of each bar, in pixels.
    /// When this is enforced, barPercentage and categoryPercentage are ignored.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the bar thickness in pixels.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? BarThickness { get; set; }

    /// <summary>
    /// The bar border color.
    /// </summary>
    /// <remarks>
    /// Default value is 'rgba(0, 0, 0, 0.1)'.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue("rgba(0, 0, 0, 0.1)")]
    [Description("Gets or sets the bar border color.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? BorderColor { get; set; }

    /// <summary>
    /// Border radius
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(0)]
    [Description("Gets or sets the bar border radius.")]
    public List<double>? BorderRadius { get; set; }

    /// <summary>
    /// Gets or sets the border width (in pixels).
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(0)]
    [Description("Gets or sets the border width in pixels.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? BorderWidth { get; set; }

    //BorderSkipped
    //https://www.chartjs.org/docs/latest/api/interfaces/BarControllerDatasetOptions.html#borderskipped

    /// <summary>
    /// Percent (0-1) of the available width each category should be within the sample width.
    /// </summary>
    /// <remarks>
    /// Default value is 0.8.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(0.8d)]
    [Description("Gets or sets the percentage of the sample width used by each category.")]
    public double CategoryPercentage { get; set; } = 0.8;

    [AddedVersion("1.10.2")]
    [Description("Gets or sets the dataset data label configuration.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public BarChartDatasetDataLabels Datalabels { get; set; } = new(); // TODO: add the reference link

    /// <summary>
    /// Should the bars be grouped on index axis.
    /// When <see langword="true" />, all the datasets at same index value will be placed next to each other centering on that
    /// index value.
    /// When <see langword="false" />, each bar is placed on its actual index-axis value.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(true)]
    [Description("Gets or sets a value indicating whether bars are grouped on the index axis.")]
    public bool Grouped { get; set; } = true;

    /// <summary>
    /// The bar background color when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the bar background color when hovered.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? HoverBackgroundColor { get; set; }

    /// <summary>
    /// The bar border color when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the bar border color when hovered.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? HoverBorderColor { get; set; }

    /// <summary>
    /// The bar border radius when hovered (in pixels).
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(0)]
    [Description("Gets or sets the hovered bar border radius in pixels.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? HoverBorderRadius { get; set; }

    /// <summary>
    /// The bar border width when hovered (in pixels).
    /// </summary>
    /// <remarks>
    /// Default value is 1.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(1)]
    [Description("Gets or sets the hovered bar border width in pixels.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? HoverBorderWidth { get; set; }

    /// <summary>
    /// The base axis of the chart. 'x' for vertical charts and 'y' for horizontal charts.
    /// Supported values are 'x' and 'y'.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the base axis of the dataset.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IndexAxis { get; set; }

    //InflateAmount
    //https://www.chartjs.org/docs/latest/charts/bar.html#inflateamount

    /// <summary>
    /// Set this to ensure that bars are not sized thicker than this.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the maximum bar thickness.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? MaxBarThickness { get; set; }

    /// <summary>
    /// Set this to ensure that bars have a minimum length in pixels.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the minimum bar length in pixels.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? MinBarLength { get; set; }

    //PointStyle
    //https://www.chartjs.org/docs/latest/configuration/elements.html#point-styles

    /// <summary>
    /// If <see langword="true" />, null or undefined values will not be used for spacing calculations when determining bar
    /// size.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether null values are skipped during bar size calculation.")]
    public bool SkipNull { get; set; }

    //Stack
    //https://www.chartjs.org/docs/latest/charts/bar.html#general

    /// <summary>
    /// The ID of the x axis to plot this dataset on.
    /// </summary>
    /// <remarks>
    /// Default value is first x axis.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue("first x axis")]
    [Description("Gets or sets the x-axis identifier for the dataset.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? XAxisID { get; set; }

    /// <summary>
    /// The ID of the y axis to plot this dataset on.
    /// </summary>
    /// <remarks>
    /// Default value is first y axis.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue("first y axis")]
    [Description("Gets or sets the y-axis identifier for the dataset.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? YAxisID { get; set; }

    #endregion
}

/// <summary>
/// Represents data label options for a bar chart dataset.
/// </summary>
[AddedVersion("1.10.2")]
public class BarChartDatasetDataLabels : ChartDatasetDataLabels { }
