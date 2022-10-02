namespace BlazorBootstrap;

public interface IChartOptions { }

/// <summary>
/// <see cref="https://www.chartjs.org/docs/latest/general/options.html"/>
/// </summary>
public class ChartOptions
{
    //hover -> mode, intersect
    //maintainAspectRatio
    //plugins -> title -> display, text

    public Interaction Interaction { get; set; } = new Interaction();

    public ChartLayout Layout { get; set; } = new ChartLayout();

    public Plugins Plugins { get; set; } = new Plugins();

    /// <summary>
    /// <see cref="https://www.chartjs.org/docs/latest/configuration/responsive.html#configuration-options"/>
    /// </summary>
    public bool Responsive { get; set; }

    public Scales Scales { get; set; } = new Scales();

    //tooltips -> mode, intersect
}

public class ChartLayout
{
    /// <summary>
    /// Apply automatic padding so visible elements are completely drawn.
    /// </summary>
    public bool AutoPadding { get; set; } = false;

    /// <summary>
    /// The padding to add inside the chart.
    /// </summary>
    public int Padding { get; set; } = 0;
}

public class Interaction
{

    private InteractionMode mode;

    /// <summary>
    /// Sets which elements appear in the tooltip. See Interaction Modes for details.
    /// </summary>
    [JsonIgnore]
    public InteractionMode Mode
    {
        get { return mode; }
        set
        {
            mode = value;
            SetMode(value);
        }
    }

    /// <summary>
    /// if true, the interaction mode only applies when the mouse position intersects an item on the chart.
    /// </summary>
    public bool Intersect { get; set; }

    /// <summary>
    /// Sets which elements appear in the interaction.
    /// </summary>
    [JsonPropertyName("mode")]
    public string ChartInteractionMode { get; private set; }

    public Interaction()
    {
        Mode = InteractionMode.Nearest;
    }

    private void SetMode(InteractionMode interactionMode)
    {
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
    }
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

public class Plugins
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Title? Title { get; set; } = new Title();
}

public class Scales
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartAxes? X { get; set; } = new ChartAxes();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartAxes? Y { get; set; } = new ChartAxes();
}

public class ChartAxes
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Title? Title { get; set; } = new Title();
    // Stacked
    public bool BeginAtZero { get; set; } = true;
}

/// <summary>
/// The chart title defines text to draw at the top of the chart.
/// <see cref="https://www.chartjs.org/docs/latest/configuration/title.html"/>
/// </summary>
public class Title
{
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

    public ChartFont? Font { get; set; } = new ChartFont();

    //fullSize
    //padding
    //position

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Text { get; set; }
}

/// <summary>
/// <see cref="https://www.chartjs.org/docs/latest/general/fonts.html"/>
/// </summary>
public class ChartFont
{
    /// <summary>
    /// Default font family for all text, follows CSS font-family options.
    /// 'Helvetica Neue', 'Helvetica', 'Arial', sans-serif
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Family { get; set; }

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
    /// <see cref="https://developer.mozilla.org/en-US/docs/Web/CSS/font-weight"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Weight { get; set; } = "bold";

    /// <summary>
    /// Height of an individual line of text
    /// <see cref="https://developer.mozilla.org/en-US/docs/Web/CSS/line-height"/>
    /// </summary>
    public double LineHeight { get; set; } = 1.2;
}
