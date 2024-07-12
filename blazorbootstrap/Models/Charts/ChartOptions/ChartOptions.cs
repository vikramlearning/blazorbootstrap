namespace BlazorBootstrap;

public interface IChartOptions { }

/// <summary>
///     <see href="https://www.chartjs.org/docs/latest/general/options.html" />
/// </summary>
public class ChartOptions : IChartOptions
{
    #region Properties, Indexers

    //aspectRatio
    //https://www.chartjs.org/docs/latest/configuration/responsive.html#configuration-options

    /// <summary>
    /// Gets or sets the locale.
    /// By default, the chart is using the default locale of the platform which is running on.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Locale { get; set; }

    /// <summary>
    /// Maintain the original canvas aspect ratio (width / height) when resizing.
    /// <see href="https://www.chartjs.org/docs/latest/configuration/responsive.html#configuration-options" />.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true"/>.
    /// </remarks>
    public bool MaintainAspectRatio { get; set; } = true;

    //onResize
    //https://www.chartjs.org/docs/latest/configuration/responsive.html#configuration-options

    //resizeDelay
    //https://www.chartjs.org/docs/latest/configuration/responsive.html#configuration-options

    /// <summary>
    /// Resizes the chart canvas when its container does.
    /// <see href="https://www.chartjs.org/docs/latest/configuration/responsive.html#configuration-options" />.    /// 
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false"/>.
    /// </remarks>
    public bool Responsive { get; set; }

    #endregion
}

/// <summary>
/// Namespace: options.layout, the global options for the chart layout is defined in Chart.defaults.layout.
/// <see href="https://www.chartjs.org/docs/latest/configuration/layout.html#layout" />.
/// </summary>
public class ChartLayout
{
    #region Properties, Indexers

    /// <summary>
    /// Apply automatic padding so visible elements are completely drawn.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true"/>.
    /// </remarks>
    public bool AutoPadding { get; set; } = true;

    /// <summary>
    /// The padding to add inside the chart.
    /// </summary>
    public int Padding { get; set; } = 0;

    #endregion
}

/// <summary>
/// Namespace: options.interaction, the global interaction configuration is at Chart.defaults.interaction. 
/// <see href="https://www.chartjs.org/docs/latest/configuration/interactions.html#interactions" />.
/// </summary>
public class Interaction
{
    #region Fields and Constants

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

    #endregion

    #region Properties, Indexers

    //axis
    //https://www.chartjs.org/docs/latest/configuration/interactions.html#interactions

    /// <summary>
    /// Sets which elements appear in the interaction.
    /// </summary>
    [JsonPropertyName("mode")]
    public string ChartInteractionMode { get; private set; }

    /// <summary>
    /// if <see langword="true" />, the interaction mode only applies when the mouse position intersects an item on the chart.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true"/>.
    /// </remarks>
    public bool Intersect { get; set; } = true;

    /// <summary>
    /// Sets which elements appear in the tooltip. See Interaction Modes for details.
    /// </summary>
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

    //includeInvisible
    //https://www.chartjs.org/docs/latest/configuration/interactions.html#interactions

    #endregion
}

public enum InteractionMode
{
    Dataset,
    Index,
    Nearest,
    Point,
    X,
    Y
}

public class Scales
{
    #region Properties, Indexers

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public ChartAxes? X { get; set; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public ChartAxes? Y { get; set; } = new();

    #endregion
}

public class ChartAxes
{
    #region Properties, Indexers

    public bool BeginAtZero { get; set; } = true;

    /// <summary>
    /// Define options for the border that run perpendicular to the axis.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartAxesBorder? Border { get; set; }

    /// <summary>
    /// Gets or sets the grid configuration.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartAxesGrid? Grid { get; set; }

    /// <summary>
    /// User defined maximum number for the scale, overrides maximum value from data.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Max { get; set; }

    /// <summary>
    /// User defined minimum number for the scale, overrides minimum value from data.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Min { get; set; }

    /// <summary>
    /// Should the data be stacked.
    /// By default data is not stacked.
    /// If the stacked option of the value scale (y-axis on horizontal chart) is true, positive and negative values are stacked
    /// separately.
    /// </summary>
    public bool Stacked { get; set; }

    /// <summary>
    /// Adjustment used when calculating the maximum data value.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? SuggestedMax { get; set; }

    /// <summary>
    /// Adjustment used when calculating the minimum data value.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? SuggestedMin { get; set; }

    /// <summary>
    /// Gets or sets the tick configuration.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartAxesTicks? Ticks { get; set; }

    /// <summary>
    /// Gets or sets the title configuration.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartAxesTitle? Title { get; set; }

    /// <summary>
    /// Gets or sets the index scale type.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null"/>.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }

    #endregion
}

/// <summary>
/// Define options for the border that run perpendicular to the axis.
/// <see href="https://www.chartjs.org/docs/latest/axes/styling.html" />
/// </summary>
public class ChartAxesBorder
{
    #region Properties, Indexers

    /// <summary>
    /// The color of the border line.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; }

    /// <summary>
    /// Length and spacing of dashes on grid lines
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CanvasRenderingContext2D/setLineDash" />
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<int>? Dash { get; set; }

    /// <summary>
    /// Offset for line dashes.
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CanvasRenderingContext2D/lineDashOffset" />
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? DashOffset { get; set; }

    /// <summary>
    /// If <see langword="true" />, draw a border at the edge between the axis and the chart area.
    /// </summary>
    public bool Display { get; set; } = true;

    /// <summary>
    /// The width of the border line.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Width { get; set; }

    /// <summary>
    /// z-index of the border layer. Values <= 0 are drawn under datasets, > 0 on top.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Z { get; set; }

    #endregion
}

/// <summary>
/// Defines options for the grid lines that run perpendicular to the axis.
/// <see href="https://www.chartjs.org/docs/latest/samples/scale-options/grid.html" />
/// </summary>
public class ChartAxesGrid
{
    #region Properties, Indexers

    /// <summary>
    /// If <see langword="true" />, gridlines are circular (on radar and polar area charts only).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Circular { get; set; }

    /// <summary>
    /// Color of the grid axis lines. Here one can write a CSS method or even a JavaScript method for a dynamic color.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; }

    /// <summary>
    /// If false, do not display grid lines for this axis.
    /// </summary>
    public bool Display { get; set; } = true;

    /// <summary>
    /// If <see langword="true" />, draw lines on the chart area inside the axis lines. This is useful when there are multiple
    /// axes and you need to control which grid lines are drawn.
    /// </summary>
    public bool DrawOnChartArea { get; set; } = true;

    /// <summary>
    /// If <see langword="true" />, draw lines beside the ticks in the axis area beside the chart.
    /// </summary>
    public bool DrawTicks { get; set; } = true;

    /// <summary>
    /// Stroke width of grid lines.
    /// </summary>
    public int LineWidth { get; set; } = 1;

    /// <summary>
    /// If <see langword="true" />, grid lines will be shifted to be between labels. This is set to true for a bar chart by
    /// default.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Offset { get; set; } = false;

    /// <summary>
    /// Length and spacing of the tick mark line.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<int>? TickBorderDash { get; set; }

    /// <summary>
    /// Offset for the line dash of the tick mark.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TickBorderDashOffset { get; set; }

    /// <summary>
    /// Color of the tick line. If unset, defaults to the grid line color.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TickColor { get; set; }

    /// <summary>
    /// Length in pixels that the grid lines will draw into the axis area.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TickLength { get; set; }

    /// <summary>
    /// Width of the tick mark in pixels.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TickWidth { get; set; }

    /// <summary>
    /// z-index of the gridline layer. Values <= 0 are drawn under datasets, > 0 on top.
    /// </summary>
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

    [JsonPropertyName("align")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Alignment { get; private set; }

    /// <summary>
    /// Color of label backdrops
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BackdropColor { get; set; }

    /// <summary>
    /// Padding of label backdrop
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? BackdropPadding { get; set; }

    /// <summary>
    /// Color of ticks
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; }

    /// <summary>
    /// If <see langword="true" />, show tick labels.
    /// </summary>
    public bool Display { get; set; } = true;

    /// <summary>
    /// defines options for the major tick marks that are generated by the axis.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartAxesTicksMajor? Major { get; set; }

    /// <summary>
    /// Sets the offset of the tick labels from the axis
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Padding { get; set; }

    /// <summary>
    /// If <see langword="true" />, draw a background behind the tick labels.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ShowLabelBackdrop { get; set; }

    /// <summary>
    /// The color of the stroke around the text.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TextStrokeColor { get; set; }

    /// <summary>
    /// Stroke width around the text.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TextStrokeWidth { get; set; }

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
public class ChartAxesTicksMajor
{
    #region Properties, Indexers

    /// <summary>
    /// If <see langword="true" />, major ticks are generated. A major tick will affect auto skipping and major will be defined
    /// on ticks in the scriptable options context.
    /// </summary>
    public bool Enabled { get; set; } = false;

    #endregion
}

/// <summary>
/// The chart title defines text to draw at the top of the chart.
/// <see href="https://www.chartjs.org/docs/latest/configuration/title.html" />
/// </summary>
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
    [JsonPropertyName("align")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Alignment { get; private set; }

    /// <summary>
    /// Color of text.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; } = "black";

    /// <summary>
    /// Is the title shown?
    /// </summary>
    public bool Display { get; set; }

    public ChartFont? Font { get; set; } = new();

    //fullSize
    //padding
    //position

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Text { get; set; }

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
public class ChartFont
{
    #region Properties, Indexers

    /// <summary>
    /// Default font family for all text, follows CSS font-family options.
    /// 'Helvetica Neue', 'Helvetica', 'Arial', sans-serif
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Family { get; set; }

    /// <summary>
    /// Height of an individual line of text
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/line-height" />
    /// </summary>
    public double LineHeight { get; set; } = 1.2;

    /// <summary>
    /// Default font size (in px) for text. Does not apply to radialLinear scale point labels.
    /// </summary>
    public int Size { get; set; } = 12;

    /// <summary>
    /// Default font style. Does not apply to tooltip title or footer. Does not apply to chart title.
    /// Follows CSS font-style options (i.e. normal, italic, oblique, initial, inherit).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Style { get; set; }

    /// <summary>
    /// Default font weight (boldness).
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/font-weight" />
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Weight { get; set; } = "bold";

    #endregion
}
