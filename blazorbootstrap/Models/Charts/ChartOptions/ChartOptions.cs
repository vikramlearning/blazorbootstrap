namespace BlazorBootstrap;

public interface IChartOptions { }

/// <summary>
/// <see cref="https://www.chartjs.org/docs/latest/general/options.html"/>
/// </summary>
public class ChartOptions : IChartOptions
{
    /// <summary>
    /// <see cref="https://www.chartjs.org/docs/latest/configuration/responsive.html#configuration-options"/>
    /// </summary>
    public bool Responsive { get; set; }
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
        get => mode;
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

    private void SetMode(InteractionMode interactionMode) => ChartInteractionMode = interactionMode switch
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

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartTooltip? Tooltip { get; set; }

    public Legend Legend { get; set; } = new Legend();
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
    public Title? Title { get; set; } = new Title();

    /// <summary>
    /// Should the data be stacked. 
    /// By default data is not stacked. 
    /// If the stacked option of the value scale (y-axis on horizontal chart) is true, positive and negative values are stacked separately. 
    /// </summary>
    public bool Stacked { get; set; }
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

/// <summary>
/// Tooltip for bubble, doughnut, pie, polar area, and scatter charts
/// <see cref="https://www.chartjs.org/docs/latest/configuration/tooltip.html"/>
/// </summary>
public class ChartTooltip
{
    /// <summary>
    /// Are on-canvas tooltips enabled?
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Background color of the tooltip.
    /// </summary>
    public string BackgroundColor { get; set; } = "rgba(0, 0, 0, 0.8)";

    /// <summary>
    /// Color of title text.
    /// </summary>
    public string TitleColor { get; set; } = "#fff";

    public ChartFont TitleFont { get; set; } = new ChartFont();

    /// <summary>
    /// Horizontal alignment of the title text lines. left/right/center.
    /// </summary>
    public string TitleAlign { get; set; } = "left";

    /// <summary>
    /// Spacing to add to top and bottom of each title line.
    /// </summary>
    public int TitleSpacing { get; set; } = 2;

    /// <summary>
    /// Margin to add on bottom of title section.
    /// </summary>
    public int TitleMarginBottom { get; set; } = 6;

    /// <summary>
    /// Color of body text.
    /// </summary>
    public string BodyColor { get; set; } = "#fff";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartFont? BodyFont { get; set; }

    /// <summary>
    /// Horizontal alignment of the body text lines. left/right/center.
    /// </summary>
    public string BodyAlign { get; set; } = "left";

    /// <summary>
    /// Spacing to add to top and bottom of each tooltip item.
    /// </summary>
    public int BodySpacing { get; set; } = 2;

    /// <summary>
    /// Color of footer text.
    /// </summary>
    public string FooterColor { get; set; } = "#fff";

    public ChartFont FooterFont { get; set; } = new ChartFont();

    /// <summary>
    /// Horizontal alignment of the footer text lines. left/right/center.
    /// </summary>
    public string FooterAlign { get; set; } = "left";

    /// <summary>
    /// Spacing to add to top and bottom of each footer line.
    /// </summary>
    public int FooterSpacing { get; set; } = 2;

    /// <summary>
    /// Margin to add before drawing the footer.
    /// </summary>
    public int FooterMarginTop { get; set; } = 6;

    /// <summary>
    /// Extra distance to move the end of the tooltip arrow away from the tooltip point.
    /// </summary>
    public int CaretPadding { get; set; } = 2;

    /// <summary>
    /// Size, in px, of the tooltip arrow.
    /// </summary>
    public int CaretSize { get; set; } = 5;

    /// <summary>
    /// If true, color boxes are shown in the tooltip.
    /// </summary>
    public bool DisplayColors { get; set; } = true;

    /// <summary>
    /// Position of the tooltip caret in the X direction. left/center/right.
    /// </summary>
    public string? XAlign { get; set; }

    /// <summary>
    /// Position of the tooltip caret in the Y direction. top/center/bottom.
    /// </summary>
    public string? YAlign { get; set; }
}

public class Legend
{
    /// <summary>
    /// Alignment of the legend. Default values is 'center'. Other possible values 'start' and 'end'.
    /// </summary>
    public string? Align { get; set; } = "center";

    /// <summary>
    /// Position of the legend. Default value is 'top'. Other possible value is 'bottom'.
    /// </summary>
    public string? Position { get; set; } = "top";

    /// <summary>
    /// Is the legend shown? Default value is 'true'.
    /// </summary>
    public bool Display { get; set; } = true;
}
