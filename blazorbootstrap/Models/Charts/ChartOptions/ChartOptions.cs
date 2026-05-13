namespace BlazorBootstrap;

public interface IChartOptions { }

/// <summary>
///     <see href="https://www.chartjs.org/docs/latest/general/options.html" />
/// </summary>
[AddedVersion("1.0.0")]
public class ChartOptions : IChartOptions
{
    #region Properties, Indexers

    /// <summary>
    /// Chart.js animates charts out of the box.
    /// A number of options are provided to configure how the animation looks and how long it takes.
    /// <see href="https://www.chartjs.org/docs/latest/configuration/animations.html" />.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the chart animation configuration.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartAnimation? Animation { get; set; }

    /// <summary>
    /// Canvas aspect ratio (i.e. <c>width / height</c>).
    /// <see href="https://www.chartjs.org/docs/latest/configuration/responsive.html#configuration-options" />.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the canvas aspect ratio.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? AspectRatio { get; set; }

    /// <summary>
    /// Gets or sets the locale.
    /// By default, the chart is using the default locale of the platform which is running on.
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the locale.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Locale { get; set; }

    /// <summary>
    /// Maintain the original canvas aspect ratio (width / height) when resizing.
    /// <see href="https://www.chartjs.org/docs/latest/configuration/responsive.html#configuration-options" />.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [AddedVersion("3.0.0")]
    [DefaultValue(true)]
    [Description("Gets or sets a value indicating whether the original canvas aspect ratio is maintained while resizing.")]
    public bool MaintainAspectRatio { get; set; } = true;

    //onResize
    //https://www.chartjs.org/docs/latest/configuration/responsive.html#configuration-options

    /// <summary>
    /// Delay the resize update by the given amount of milliseconds.
    /// <see href="https://www.chartjs.org/docs/latest/configuration/responsive.html#configuration-options" />.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the resize delay in milliseconds.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ResizeDelay { get; set; }

    /// <summary>
    /// Resizes the chart canvas when its container does.
    /// <see href="https://www.chartjs.org/docs/latest/configuration/responsive.html#configuration-options" />.    ///
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether the chart resizes with its container.")]
    public bool Responsive { get; set; }

    #endregion
}

/// <summary>
/// Namespace: options.layout, the global options for the chart layout is defined in Chart.defaults.layout.
/// <see href="https://www.chartjs.org/docs/latest/configuration/layout.html#layout" />.
/// </summary>
[AddedVersion("1.0.0")]
public class ChartLayout
{
    #region Properties, Indexers

    /// <summary>
    /// Apply automatic padding so visible elements are completely drawn.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [DefaultValue(true)]
    [Description("Gets or sets a value indicating whether automatic padding is applied.")]
    public bool AutoPadding { get; set; } = true;

    /// <summary>
    /// The padding to add inside the chart.
    /// </summary>
    [DefaultValue(0)]
    [Description("Gets or sets the padding inside the chart.")]
    public int Padding { get; set; } = 0;

    #endregion
}

/// <summary>
/// Namespace: options.interaction, the global interaction configuration is at Chart.defaults.interaction.
/// <see href="https://www.chartjs.org/docs/latest/configuration/interactions.html#interactions" />.
/// </summary>
[AddedVersion("1.0.0")]
public class Interaction
{
    #region Fields and Constants

    private InteractionAxis? axis;
    private InteractionMode mode;

    #endregion

    #region Constructors

    public Interaction()
    {
        Mode = InteractionMode.Nearest;
    }

    #endregion

    #region Methods

    private void SetMode(InteractionMode interactionMode) =>
        ChartInteractionMode = interactionMode switch
                               {
                                   InteractionMode.Dataset => "dataset",
                                   InteractionMode.Index => "index",
                                   InteractionMode.Nearest => "nearest",
                                   InteractionMode.Point => "point",
                                   InteractionMode.X => "x",
                                   InteractionMode.Y => "y",
                                   _ => ""
                               };

    private void SetAxis(InteractionAxis? interactionAxis) =>
        ChartInteractionAxis = interactionAxis switch
                               {
                                   InteractionAxis.X => "x",
                                   InteractionAxis.Y => "y",
                                   InteractionAxis.XY => "xy",
                                   InteractionAxis.R => "r",
                                   _ => null
                               };

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Defines which directions are used in calculating distances.
    /// Supported values are <c>x</c>, <c>y</c>, <c>xy</c>, and <c>r</c>.
    /// <see href="https://www.chartjs.org/docs/latest/configuration/interactions.html#interactions" />.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets the serialized interaction axis value.")]
    [JsonPropertyName("axis")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ChartInteractionAxis { get; private set; }

    /// <summary>
    /// Sets which elements appear in the interaction.
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue("nearest")]
    [Description("Gets the serialized interaction mode value.")]
    [JsonPropertyName("mode")]
    public string ChartInteractionMode { get; private set; } = string.Empty;

    /// <summary>
    /// if <see langword="true" />, the interaction mode only applies when the mouse position intersects an item on the chart.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(true)]
    [Description("Gets or sets a value indicating whether interactions require intersection with a chart item.")]
    public bool Intersect { get; set; } = true;

    /// <summary>
    /// If <see langword="true" />, invisible points that are outside of the chart area are included when evaluating interactions.
    /// <see href="https://www.chartjs.org/docs/latest/configuration/interactions.html#interactions" />.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a value indicating whether invisible points are included in interactions.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IncludeInvisible { get; set; }

    /// <summary>
    /// Defines which directions are used in calculating distances for interactions.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the interaction axis.")]
    [JsonIgnore]
    public InteractionAxis? Axis
    {
        get => axis;
        set
        {
            axis = value;
            SetAxis(value);
        }
    }

    /// <summary>
    /// Sets which elements appear in the tooltip. See Interaction Modes for details.
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(InteractionMode.Nearest)]
    [Description("Gets or sets the interaction mode.")]
    [JsonIgnore]
    public InteractionMode Mode
    {
        get => mode;
        set
        {
            mode = value;
            SetMode(value);
        }
    }

    #endregion
}

public enum InteractionAxis
{
    /// <summary>
    /// Calculate interaction distances on the x-axis only.
    /// </summary>
    [AddedVersion("4.0.0")]
    X,

    /// <summary>
    /// Calculate interaction distances on the y-axis only.
    /// </summary>
    [AddedVersion("4.0.0")]
    Y,

    /// <summary>
    /// Calculate interaction distances on both x-axis and y-axis.
    /// </summary>
    [AddedVersion("4.0.0")]
    XY,

    /// <summary>
    /// Calculate interaction distances on the radial axis.
    /// </summary>
    [AddedVersion("4.0.0")]
    R
}

public enum InteractionMode
{
  /// <summary>
  /// Finds items in the same dataset.
  /// </summary>
    Dataset,

  /// <summary>
  /// Finds item at the same index. 
  /// </summary>
    Index,

  /// <summary>
  /// Gets the items that are at the nearest distance to the point. 
  /// </summary>
    Nearest,

  /// <summary>
  /// Finds all of the items that intersect the point
  /// </summary>
    Point,

  /// <summary>
  /// Returns all items that would intersect based on the X coordinate of the position only. Would be useful for a vertical cursor implementation. Note that this only applies to cartesian charts.
  /// </summary>
    X,

  /// <summary>
  /// Returns all items that would intersect based on the Y coordinate of the position. This would be useful for a horizontal cursor implementation. Note that this only applies to cartesian charts.
  /// </summary>
    Y
}

/// <summary>
/// Represents the cartesian axis configuration for a chart.
/// </summary>
[AddedVersion("1.0.0")]
public class Scales
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the x-axis configuration.
    /// </summary>
    [Description("Gets or sets the x-axis configuration.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ChartAxes? X { get; set; } = new();

    /// <summary>
    /// Gets or sets the y-axis configuration.
    /// </summary>
    [Description("Gets or sets the y-axis configuration.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ChartAxes? Y { get; set; } = new();

    #endregion
}

/// <summary>
/// Provides built-in chart axis type names.
/// </summary>
[AddedVersion("1.0.0")]
public class ChartAxesType
{
    #region Fields and Constants

    public static readonly string Linear = "linear";
    public static readonly string Logarithmic = "logarithmic";
    public static readonly string Category = "category";
    public static readonly string Time = "time";
    public static readonly string Timeseries = "timeseries";

    #endregion
}

/// <summary>
/// Represents the configuration for a chart axis.
/// </summary>
[AddedVersion("1.0.0")]
public class ChartAxes
{
    #region Properties, Indexers

    /// <summary>
    /// If <see langword="true" />, the scale begins at zero.
    /// </summary>
    [DefaultValue(true)]
    [Description("Gets or sets a value indicating whether the scale begins at zero.")]
    public bool BeginAtZero { get; set; } = true;

    /// <summary>
    /// Define options for the border that run perpendicular to the axis.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the border configuration.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartAxesBorder? Border { get; set; }

    /// <summary>
    /// Gets or sets the grid configuration.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the grid configuration.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartAxesGrid? Grid { get; set; }

    /// <summary>
    /// User defined maximum number for the scale, overrides maximum value from data.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the maximum scale value.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Max { get; set; }

    /// <summary>
    /// User defined minimum number for the scale, overrides minimum value from data.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the minimum scale value.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Min { get; set; }

    /// <summary>
    /// Should the data be stacked.
    /// By default data is not stacked.
    /// If the stacked option of the value scale (y-axis on horizontal chart) is true, positive and negative values are stacked
    /// separately.
    /// </summary>
    /// <summary>
    /// If <see langword="true" />, the axis values are stacked.
    /// </summary>
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether the axis values are stacked.")]
    public bool Stacked { get; set; }

    /// <summary>
    /// Adjustment used when calculating the maximum data value.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the suggested maximum scale value.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? SuggestedMax { get; set; }

    /// <summary>
    /// Adjustment used when calculating the minimum data value.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the suggested minimum scale value.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? SuggestedMin { get; set; }

    /// <summary>
    /// Gets or sets the tick configuration.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the tick configuration.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartAxesTicks? Ticks { get; set; }

    /// <summary>
    /// Gets or sets the title configuration.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the axis title configuration.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartAxesTitle? Title { get; set; }

    /// <summary>
    /// Gets or sets the index scale type. See <see cref="ChartAxesType" />.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [DefaultValue(null)]
    [Description("Gets or sets the axis type.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }

    #endregion
}

/// <summary>
/// Define options for the border that run perpendicular to the axis.
/// <see href="https://www.chartjs.org/docs/latest/axes/styling.html" />
/// </summary>
[AddedVersion("1.0.0")]
public class ChartAxesBorder
{
    #region Properties, Indexers

    /// <summary>
    /// The color of the border line.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the border line color.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; }

    /// <summary>
    /// Length and spacing of dashes on grid lines
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CanvasRenderingContext2D/setLineDash" />
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the border dash pattern.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<int>? Dash { get; set; }

    /// <summary>
    /// Offset for line dashes.
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CanvasRenderingContext2D/lineDashOffset" />
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the border dash offset.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? DashOffset { get; set; }

    /// <summary>
    /// If <see langword="true" />, draw a border at the edge between the axis and the chart area.
    /// </summary>
    [DefaultValue(true)]
    [Description("Gets or sets a value indicating whether the axis border is displayed.")]
    public bool Display { get; set; } = true;

    /// <summary>
    /// The width of the border line.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the border width.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Width { get; set; }

    /// <summary>
    /// z-index of the border layer. Values <= 0 are drawn under datasets, > 0 on top.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the border z-index.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Z { get; set; }

    #endregion
}

/// <summary>
/// Defines options for the grid lines that run perpendicular to the axis.
/// <see href="https://www.chartjs.org/docs/latest/samples/scale-options/grid.html" />
/// </summary>
[AddedVersion("1.0.0")]
public class ChartAxesGrid
{
    #region Properties, Indexers

    /// <summary>
    /// If <see langword="true" />, gridlines are circular (on radar and polar area charts only).
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets a value indicating whether grid lines are circular.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Circular { get; set; }

    /// <summary>
    /// Color of the grid axis lines. Here one can write a CSS method or even a JavaScript method for a dynamic color.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the grid line color.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; }

    /// <summary>
    /// If false, do not display grid lines for this axis.
    /// </summary>
    [DefaultValue(true)]
    [Description("Gets or sets a value indicating whether grid lines are displayed.")]
    public bool Display { get; set; } = true;

    /// <summary>
    /// If <see langword="true" />, draw lines on the chart area inside the axis lines. This is useful when there are multiple
    /// axes and you need to control which grid lines are drawn.
    /// </summary>
    [DefaultValue(true)]
    [Description("Gets or sets a value indicating whether grid lines are drawn on the chart area.")]
    public bool DrawOnChartArea { get; set; } = true;

    /// <summary>
    /// If <see langword="true" />, draw lines beside the ticks in the axis area beside the chart.
    /// </summary>
    [DefaultValue(true)]
    [Description("Gets or sets a value indicating whether tick lines are drawn.")]
    public bool DrawTicks { get; set; } = true;

    /// <summary>
    /// Stroke width of grid lines.
    /// </summary>
    [DefaultValue(1)]
    [Description("Gets or sets the grid line width.")]
    public int LineWidth { get; set; } = 1;

    /// <summary>
    /// If <see langword="true" />, grid lines will be shifted to be between labels. This is set to true for a bar chart by
    /// default.
    /// </summary>
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether grid lines are offset between labels.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Offset { get; set; } = false;

    /// <summary>
    /// Length and spacing of the tick mark line.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the tick border dash pattern.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<int>? TickBorderDash { get; set; }

    /// <summary>
    /// Offset for the line dash of the tick mark.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the tick border dash offset.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TickBorderDashOffset { get; set; }

    /// <summary>
    /// Color of the tick line. If unset, defaults to the grid line color.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the tick line color.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TickColor { get; set; }

    /// <summary>
    /// Length in pixels that the grid lines will draw into the axis area.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the tick length in pixels.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TickLength { get; set; }

    /// <summary>
    /// Width of the tick mark in pixels.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the tick width in pixels.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TickWidth { get; set; }

    /// <summary>
    /// z-index of the gridline layer. Values <= 0 are drawn under datasets, > 0 on top.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the grid line layer z-index.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Z { get; set; }

    #endregion
}

public enum TicksAlignment
{
    Center, // default
    Start,
    End
}

public enum TitleAlignment
{
    Center, // default
    Start,
    End
}

/// <summary>
/// Chart axes tick styling
/// <see href="https://www.chartjs.org/docs/latest/samples/scale-options/ticks.html" />
/// </summary>
[AddedVersion("1.0.0")]
public class ChartAxesTicks
{
    #region Fields and Constants

    private TicksAlignment ticksAlignment;

    #endregion

    #region Methods

    private void SetTicksAlignment(TicksAlignment interactionMode) =>
        Alignment = interactionMode switch
                    {
                        TicksAlignment.Center => "center",
                        TicksAlignment.Start => "start",
                        TicksAlignment.End => "end",
                        _ => null
                    };

    #endregion

    #region Properties, Indexers

    [DefaultValue(null)]
    [Description("Gets the serialized tick alignment value.")]
    [JsonPropertyName("align")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Alignment { get; private set; }

    /// <summary>
    /// Color of label backdrops
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the tick backdrop color.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BackdropColor { get; set; }

    /// <summary>
    /// Padding of label backdrop
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the tick backdrop padding.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? BackdropPadding { get; set; }

    /// <summary>
    /// Color of ticks
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the tick color.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; }

    /// <summary>
    /// If <see langword="true" />, show tick labels.
    /// </summary>
    [DefaultValue(true)]
    [Description("Gets or sets a value indicating whether tick labels are displayed.")]
    public bool Display { get; set; } = true;

    /// <summary>
    /// defines options for the major tick marks that are generated by the axis.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the major tick configuration.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartAxesTicksMajor? Major { get; set; }

    /// <summary>
    /// Sets the offset of the tick labels from the axis
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the tick label padding.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Padding { get; set; }

    /// <summary>
    /// If <see langword="true" />, draw a background behind the tick labels.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets a value indicating whether a backdrop is drawn behind tick labels.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ShowLabelBackdrop { get; set; }

    /// <summary>
    /// The color of the stroke around the text.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the tick text stroke color.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TextStrokeColor { get; set; }

    /// <summary>
    /// Stroke width around the text.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the tick text stroke width.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TextStrokeWidth { get; set; }

    [DefaultValue(TicksAlignment.Center)]
    [Description("Gets or sets the tick alignment.")]
    [JsonIgnore]
    public TicksAlignment TicksAlignment
    {
        get => ticksAlignment;
        set
        {
            ticksAlignment = value;
            SetTicksAlignment(value);
        }
    }

    #endregion
}

/// <summary>
/// Defines options for the major tick marks that are generated by the axis.
/// </summary>
[AddedVersion("1.0.0")]
public class ChartAxesTicksMajor
{
    #region Properties, Indexers

    /// <summary>
    /// If <see langword="true" />, major ticks are generated. A major tick will affect auto skipping and major will be defined
    /// on ticks in the scriptable options context.
    /// </summary>
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether major ticks are generated.")]
    public bool Enabled { get; set; } = false;

    #endregion
}

/// <summary>
/// The chart title defines text to draw at the top of the chart.
/// <see href="https://www.chartjs.org/docs/latest/configuration/title.html" />
/// </summary>
[AddedVersion("1.0.0")]
public class ChartAxesTitle
{
    #region Fields and Constants

    private TitleAlignment titleAlignment;

    #endregion

    #region Methods

    private void SetTitleAlignment(TitleAlignment interactionMode) =>
        Alignment = interactionMode switch
                    {
                        TitleAlignment.Center => "center", // default
                        TitleAlignment.Start => "start",
                        TitleAlignment.End => "end",
                        _ => null
                    };

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Alignment of the title.
    /// Options are: 'start', 'center', and 'end'
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets the serialized title alignment value.")]
    [JsonPropertyName("align")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Alignment { get; private set; }

    /// <summary>
    /// Color of text.
    /// </summary>
    [DefaultValue("black")]
    [Description("Gets or sets the title text color.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; } = "black";

    /// <summary>
    /// Is the title shown?
    /// </summary>
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether the title is displayed.")]
    public bool Display { get; set; }

    /// <summary>
    /// Gets or sets the font used to render the title.
    /// </summary>
    [Description("Gets or sets the font used to render the title.")]
    public ChartFont? Font { get; set; } = new();

    //fullSize
    //padding
    //position

    /// <summary>
    /// Gets or sets the title text.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the title text.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Text { get; set; }

    [DefaultValue(TitleAlignment.Center)]
    [Description("Gets or sets the title alignment.")]
    [JsonIgnore]
    public TitleAlignment TitleAlignment
    {
        get => titleAlignment;
        set
        {
            titleAlignment = value;
            SetTitleAlignment(value);
        }
    }

    #endregion
}

/// <summary>
///     <see href="https://www.chartjs.org/docs/latest/general/fonts.html" />
/// </summary>
[AddedVersion("1.0.0")]
public class ChartFont
{
    #region Properties, Indexers

    /// <summary>
    /// Default font family for all text, follows CSS font-family options.
    /// 'Helvetica Neue', 'Helvetica', 'Arial', sans-serif
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the font family.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Family { get; set; }

    /// <summary>
    /// Height of an individual line of text
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/line-height" />
    /// </summary>
    [DefaultValue(1.2d)]
    [Description("Gets or sets the font line height.")]
    public double LineHeight { get; set; } = 1.2;

    /// <summary>
    /// Default font size (in px) for text. Does not apply to radialLinear scale point labels.
    /// </summary>
    [DefaultValue(12)]
    [Description("Gets or sets the font size in pixels.")]
    public int Size { get; set; } = 12;

    /// <summary>
    /// Default font style. Does not apply to tooltip title or footer. Does not apply to chart title.
    /// Follows CSS font-style options (i.e. normal, italic, oblique, initial, inherit).
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the font style.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Style { get; set; }

    /// <summary>
    /// Default font weight (boldness).
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/font-weight" />
    /// </summary>
    [DefaultValue("bold")]
    [Description("Gets or sets the font weight.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Weight { get; set; } = "bold";

    #endregion
}

/// <summary>
/// Chart.js animates charts out of the box.
/// A number of options are provided to configure how the animation looks and how long it takes.
/// <see href="https://www.chartjs.org/docs/latest/configuration/animations.html#animations" />.
/// </summary>
[AddedVersion("4.0.0")]
public class ChartAnimation
{
    /// <summary>
    /// Delay before starting the animations.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the animation delay.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Delay { get; set; }

    /// <summary>
    /// The number of milliseconds an animation takes.
    /// </summary>
    [DefaultValue(1000d)]
    [Description("Gets or sets the animation duration in milliseconds.")]
    public double Duration { get; set; } = 1000;

    /// <summary>
    /// Easing function to use.
    /// </summary>
    [DefaultValue("easeOutQuart")]
    [Description("Gets or sets the animation easing function.")]
    public string Easing { get; set; } = "easeOutQuart";

    /// <summary>
    /// If <see langword="true" />, the animations loop endlessly.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets a value indicating whether the animation loops.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Loop { get; set; }
}
