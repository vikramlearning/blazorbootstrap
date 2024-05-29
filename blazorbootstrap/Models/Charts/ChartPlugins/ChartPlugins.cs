namespace BlazorBootstrap;

public class ChartPlugins
{
    #region Properties, Indexers

    /// <summary>
    /// The chart legend displays data about the datasets that are appearing on the chart.
    /// </summary>
    public ChartPluginsLegend Legend { get; set; } = new();

    /// <summary>
    /// The chart title defines text to draw at the top of the chart.
    /// <see href="https://www.chartjs.org/docs/latest/configuration/title.html"/> 
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartPluginsTitle? Title { get; set; } = new();

    /// <summary>
    /// Tooltip for the element.
    /// <see href="https://www.chartjs.org/docs/latest/configuration/tooltip.html"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartPluginsTooltip? Tooltip { get; set; }
    


    #endregion
}

public class ChartPluginsLegend
{
    #region Properties, Indexers

    /// <summary>
    /// Alignment of the legend. Default values is 'center'. Other possible values 'start' and 'end'.
    /// </summary>
    public string Align { get; set; } = "center";

    /// <summary>
    /// Is the legend shown? Default value is 'true'.
    /// </summary>
    public bool Display { get; set; } = true;

    /// <summary>
    /// If <see langword="true" />, Marks that this box should take the full width/height of the canvas (moving other boxes). This is unlikely to need to be changed in day-to-day use.
    /// </summary>
    public bool FullSize { get; set; } = true;

    /// <summary>
    /// Label settings for the legend.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartPluginsLegendLabels? Labels { get; set; }

    /// <summary>
    /// Maximum height of the legend, in pixels.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxHeight { get; set; }

    /// <summary>
    /// Maximum width of the legend, in pixels.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxWidth { get; set; }

    /// <summary>
    /// Position of the legend. Default value is 'top'. Other possible value is 'bottom'.
    /// </summary>
    public string Position { get; set; } = "top";

    /// <summary>
    /// If <see langword="true" />, the Legend will show datasets in reverse order.
    /// </summary>
    public bool Reverse { get; set; } = false;

    /// <summary>
    /// If <see langword="true" />, for rendering of the legends will go from right to left.
    /// </summary>
    public bool Rtl { get; set; } = false;

    /// <summary>
    /// This will force the text direction 'rtl' or 'ltr' on the canvas for rendering the legend, regardless of the css specified on the canvas
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TextDirection { get; set; }
    
    /// <summary>
    /// Title object
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartPluginsLegendTitle? Title { get; set; }

    #endregion
}



public class ChartPluginsLegendTitle
{
    /// <summary>
    /// Color of the legend. Default value is 'black'.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; }

    /// <summary>
    /// Is the legend title displayed.
    /// </summary>
    public bool Display { get; set; } = true;

    /// <summary>
    /// Padding around the title.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Padding { get; set; }
    
    /// <summary>
    /// The string title
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Text { get; set; }
}

/// <summary>
/// The chart label settings
/// <see href="https://www.chartjs.org/docs/latest/configuration/legend.html#legend-label-configuration" />
/// </summary>
public class ChartPluginsLegendLabels
{
    /// <summary>
    /// Width of coloured box.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? BoxWidth { get; set; }

    /// <summary>
    /// Height of the coloured box
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? BoxHeight { get; set; }

    /// <summary>
    /// Override the borderRadius to use.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? BorderRadius { get; set; }
    
    /// <summary>
    /// Color of label and the strikethrough.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartFont? Font { get; set; }
    
    /// <summary>
    /// Padding between rows of colored boxes.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Padding { get; set; }

    /// <summary>
    /// If specified, this style of point is used for the legend. Only used if <see cref="UsePointStyle"/>> is true.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PointStyle { get; set; }

    /// <summary>
    /// If <see cref="UsePointStyle"/> is <see langword="true" />, the width of the point style used for the legend.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PointStyleWidth { get; set; }
    
    /// <summary>
    /// Label borderRadius will match corresponding <see cref="BorderRadius"/>.
    /// </summary>
    public bool UseBorderRadius { get; set; } = false;

    /// <summary>
    /// If <see langword="true"/>, Label style will match corresponding point style (size is based on pointStyleWidth or the minimum value between <see cref="BoxWidth"/> and <see cref="Font"/> -> Size).
    /// </summary>
    public bool UsePointStyle { get; set; } = false;

}

/// <summary>
/// The chart title defines text to draw at the top of the chart.
/// <see href="https://www.chartjs.org/docs/latest/configuration/title.html" />
/// </summary>
public class ChartPluginsTitle
{
    #region Properties, Indexers

    /// <summary>
    /// Alignment of the title.
    /// Options are: 'start', 'center', and 'end'
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Align { get; set; }
    /// <summary>
    /// Color of text.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; }

    /// <summary>
    /// Is the title shown?
    /// </summary>
    public bool Display { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartFont? Font { get; set; }

    //fullSize
    //padding
    //position

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Text { get; set; }

    #endregion
}

/// <summary>
/// Tooltip for bubble, doughnut, pie, polar area, and scatter charts
/// <see href="https://www.chartjs.org/docs/latest/configuration/tooltip.html" />
/// </summary>
public class ChartPluginsTooltip
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

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartPluginsTooltipFont? BodyFont { get; set; }

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
    /// If <see langword="true" />, color boxes are shown in the tooltip.
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

    public ChartPluginsTooltipFont FooterFont { get; set; } = new();

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

    public ChartPluginsTooltipFont TitleFont { get; set; } = new();

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

/// <summary>
///     <see href="https://www.chartjs.org/docs/latest/general/fonts.html" />
/// </summary>
public class ChartPluginsTooltipFont
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
