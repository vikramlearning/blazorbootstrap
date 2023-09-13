namespace BlazorBootstrap;

public class Plugins
{
    #region Properties, Indexers

    public Legend Legend { get; set; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public PluginsTitle? Title { get; set; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public PluginsTooltip? Tooltip { get; set; }

    #endregion
}

public class Legend
{
    #region Properties, Indexers

    /// <summary>
    /// Alignment of the legend. Default values is 'center'. Other possible values 'start' and 'end'.
    /// </summary>
    public string? Align { get; set; } = "center";

    /// <summary>
    /// Is the legend shown? Default value is 'true'.
    /// </summary>
    public bool Display { get; set; } = true;

    /// <summary>
    /// Position of the legend. Default value is 'top'. Other possible value is 'bottom'.
    /// </summary>
    public string? Position { get; set; } = "top";

    #endregion
}

/// <summary>
/// The chart title defines text to draw at the top of the chart.
/// <see cref="https://www.chartjs.org/docs/latest/configuration/title.html" />
/// </summary>
public class PluginsTitle
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
/// Tooltip for bubble, doughnut, pie, polar area, and scatter charts
/// <see cref="https://www.chartjs.org/docs/latest/configuration/tooltip.html" />
/// </summary>
public class PluginsTooltip
{
    #region Properties, Indexers

    /// <summary>
    /// Background color of the tooltip.
    /// </summary>
    public string BackgroundColor { get; set; } = "rgba(0, 0, 0, 0.8)";

    /// <summary>
    /// Horizontal alignment of the body text lines. left/right/center.
    /// </summary>
    public string BodyAlign { get; set; } = "left";

    /// <summary>
    /// Color of body text.
    /// </summary>
    public string BodyColor { get; set; } = "#fff";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ChartFont? BodyFont { get; set; }

    /// <summary>
    /// Spacing to add to top and bottom of each tooltip item.
    /// </summary>
    public int BodySpacing { get; set; } = 2;

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
    /// Are on-canvas tooltips enabled?
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Horizontal alignment of the footer text lines. left/right/center.
    /// </summary>
    public string FooterAlign { get; set; } = "left";

    /// <summary>
    /// Color of footer text.
    /// </summary>
    public string FooterColor { get; set; } = "#fff";

    public ChartFont FooterFont { get; set; } = new();

    /// <summary>
    /// Margin to add before drawing the footer.
    /// </summary>
    public int FooterMarginTop { get; set; } = 6;

    /// <summary>
    /// Spacing to add to top and bottom of each footer line.
    /// </summary>
    public int FooterSpacing { get; set; } = 2;

    /// <summary>
    /// Horizontal alignment of the title text lines. left/right/center.
    /// </summary>
    public string TitleAlign { get; set; } = "left";

    /// <summary>
    /// Color of title text.
    /// </summary>
    public string TitleColor { get; set; } = "#fff";

    public ChartFont TitleFont { get; set; } = new();

    /// <summary>
    /// Margin to add on bottom of title section.
    /// </summary>
    public int TitleMarginBottom { get; set; } = 6;

    /// <summary>
    /// Spacing to add to top and bottom of each title line.
    /// </summary>
    public int TitleSpacing { get; set; } = 2;

    /// <summary>
    /// Position of the tooltip caret in the X direction. left/center/right.
    /// </summary>
    public string? XAlign { get; set; }

    /// <summary>
    /// Position of the tooltip caret in the Y direction. top/center/bottom.
    /// </summary>
    public string? YAlign { get; set; }

    #endregion
}
