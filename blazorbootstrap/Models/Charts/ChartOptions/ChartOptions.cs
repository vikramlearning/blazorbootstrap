namespace BlazorBootstrap;

public interface IChartOptions { }

/// <summary>
///     <see cref="https://www.chartjs.org/docs/latest/general/options.html" />
/// </summary>
public class ChartOptions : IChartOptions
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the locale.
    /// By default, the chart is using the default locale of the platform which is running on.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Locale { get; set; }

    /// <summary>
    ///     <see cref="https://www.chartjs.org/docs/latest/configuration/responsive.html#configuration-options" />
    /// </summary>
    public bool Responsive { get; set; }

    #endregion
}

public class ChartLayout
{
    #region Properties, Indexers

    /// <summary>
    /// Apply automatic padding so visible elements are completely drawn.
    /// </summary>
    public bool AutoPadding { get; set; } = false;

    /// <summary>
    /// The padding to add inside the chart.
    /// </summary>
    public int Padding { get; set; } = 0;

    #endregion
}

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

    /// <summary>
    /// Sets which elements appear in the interaction.
    /// </summary>
    [JsonPropertyName("mode")]
    public string ChartInteractionMode { get; private set; }

    /// <summary>
    /// if true, the interaction mode only applies when the mouse position intersects an item on the chart.
    /// </summary>
    public bool Intersect { get; set; }

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

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ChartAxes? X { get; set; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ChartAxes? Y { get; set; } = new();

    #endregion
}

public class ChartAxes
{
    #region Properties, Indexers

    // Stacked
    public bool BeginAtZero { get; set; } = true;

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

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public ChartAxesTitle? Title { get; set; } = new();

    #endregion
}

/// <summary>
/// The chart title defines text to draw at the top of the chart.
/// <see cref="https://www.chartjs.org/docs/latest/configuration/title.html" />
/// </summary>
public class ChartAxesTitle
{
    #region Properties, Indexers

    /// <summary>
    /// Alignment of the title.
    /// Options are: 'start', 'center', and 'end'
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Align { get; set; } = "center";

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

    #endregion
}

/// <summary>
///     <see cref="https://www.chartjs.org/docs/latest/general/fonts.html" />
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
    /// <see cref="https://developer.mozilla.org/en-US/docs/Web/CSS/line-height" />
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
    /// <see cref="https://developer.mozilla.org/en-US/docs/Web/CSS/font-weight" />
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Weight { get; set; } = "bold";

    #endregion
}
