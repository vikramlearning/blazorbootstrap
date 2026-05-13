namespace BlazorBootstrap;

/// <summary>
/// Represents the shared plugin configuration for a chart.
/// </summary>
[AddedVersion("1.10.2")]
public class ChartPlugins
{
    #region Properties, Indexers

    /// <summary>
    /// The chart legend displays data about the datasets that are appearing on the chart.
    /// </summary>
    [Description("Gets or sets the legend configuration.")]
    public ChartPluginsLegend Legend { get; set; } = new();

    /// <summary>
    /// The chart title defines text to draw at the top of the chart.
    /// <see href="https://www.chartjs.org/docs/latest/configuration/title.html"/> 
    /// </summary>
    [Description("Gets or sets the title configuration.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartPluginsTitle? Title { get; set; } = new();

    /// <summary>
    /// Tooltip for the element.
    /// <see href="https://www.chartjs.org/docs/latest/configuration/tooltip.html"/>
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the tooltip configuration.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartPluginsTooltip? Tooltip { get; set; }
    


    #endregion
}

/// <summary>
/// Represents the legend configuration for a chart.
/// </summary>
[AddedVersion("1.10.2")]
public class ChartPluginsLegend
{
    #region Properties, Indexers

    /// <summary>
    /// Alignment of the legend. Default values is 'center'. Other possible values 'start' and 'end'.
    /// </summary>
    [DefaultValue("center")]
    [Description("Gets or sets the legend alignment.")]
    public string? Align { get; set; } = "center";

    /// <summary>
    /// Is the legend shown? Default value is 'true'.
    /// </summary>
    [DefaultValue(true)]
    [Description("Gets or sets a value indicating whether the legend is displayed.")]
    public bool Display { get; set; } = true;

    /// <summary>
    /// If <see langword="true" />, Marks that this box should take the full width/height of the canvas (moving other boxes). This is unlikely to need to be changed in day-to-day use.
    /// </summary>
    [DefaultValue(true)]
    [Description("Gets or sets a value indicating whether the legend takes the full chart area in its dimension.")]
    public bool FullSize { get; set; } = true;

    /// <summary>
    /// Label settings for the legend.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the legend label configuration.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartPluginsLegendLabels? Labels { get; set; }

    /// <summary>
    /// Maximum height of the legend, in pixels.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the maximum legend height in pixels.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxHeight { get; set; }

    /// <summary>
    /// Maximum width of the legend, in pixels.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the maximum legend width in pixels.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxWidth { get; set; }

    /// <summary>
    /// Position of the legend. Default value is 'top'. Other possible value is 'bottom'.
    /// </summary>
    [DefaultValue("top")]
    [Description("Gets or sets the legend position.")]
    public string Position { get; set; } = "top";

    /// <summary>
    /// If <see langword="true" />, the Legend will show datasets in reverse order.
    /// </summary>
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether legend items are rendered in reverse order.")]
    public bool Reverse { get; set; } = false;

    /// <summary>
    /// If <see langword="true" />, for rendering of the legends will go from right to left.
    /// </summary>
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether the legend is rendered right-to-left.")]
    public bool Rtl { get; set; } = false;

    /// <summary>
    /// This will force the text direction 'rtl' or 'ltr' on the canvas for rendering the legend, regardless of the css specified on the canvas
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the legend text direction.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TextDirection { get; set; }
    
    /// <summary>
    /// Title object
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the legend title configuration.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartPluginsLegendTitle? Title { get; set; }

    #endregion
}

/// <summary>
/// Represents the legend title configuration for a chart.
/// </summary>
[AddedVersion("1.10.2")]
public class ChartPluginsLegendTitle
{
    /// <summary>
    /// Color of the legend. Default value is 'black'.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the legend title color.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; }

    /// <summary>
    /// Is the legend title displayed.
    /// </summary>
    [DefaultValue(true)]
    [Description("Gets or sets a value indicating whether the legend title is displayed.")]
    public bool Display { get; set; } = true;

    /// <summary>
    /// Padding around the title.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the padding around the legend title.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Padding { get; set; }
    
    /// <summary>
    /// The string title
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the legend title text.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Text { get; set; }
}

/// <summary>
/// The chart label settings
/// <see href="https://www.chartjs.org/docs/latest/configuration/legend.html#legend-label-configuration" />
/// </summary>
[AddedVersion("1.10.2")]
public class ChartPluginsLegendLabels
{
    /// <summary>
    /// Width of coloured box.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the legend label box width.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? BoxWidth { get; set; }

    /// <summary>
    /// Height of the coloured box
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the legend label box height.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? BoxHeight { get; set; }

    /// <summary>
    /// Override the borderRadius to use.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the legend label border radius.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? BorderRadius { get; set; }
    
    /// <summary>
    /// Color of label and the strikethrough.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the legend label color.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; }
    
    /// <summary>
    /// Gets or sets the font used for legend labels.
    /// </summary>
    [DefaultValue(null)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartFont? Font { get; set; }
    
    /// <summary>
    /// Padding between rows of colored boxes.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the padding between legend rows.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Padding { get; set; }

    /// <summary>
    /// If specified, this style of point is used for the legend. Only used if <see cref="UsePointStyle"/>> is true.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the legend point style.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PointStyle { get; set; }

    /// <summary>
    /// If <see cref="UsePointStyle"/> is <see langword="true" />, the width of the point style used for the legend.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the legend point style width.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PointStyleWidth { get; set; }
    
    /// <summary>
    /// Label borderRadius will match corresponding <see cref="BorderRadius"/>.
    /// </summary>
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether legend border radii match dataset border radii.")]
    public bool UseBorderRadius { get; set; } = false;

    /// <summary>
    /// If <see langword="true"/>, Label style will match corresponding point style (size is based on pointStyleWidth or the minimum value between <see cref="BoxWidth"/> and <see cref="Font"/> -> Size).
    /// </summary>
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether legend labels use the dataset point style.")]
    public bool UsePointStyle { get; set; } = false;

}

/// <summary>
/// The chart title defines text to draw at the top of the chart.
/// <see href="https://www.chartjs.org/docs/latest/configuration/title.html" />
/// </summary>
[AddedVersion("1.10.2")]
public class ChartPluginsTitle
{
    #region Properties, Indexers

    /// <summary>
    /// Alignment of the title.
    /// Options are: 'start', 'center', and 'end'
    /// </summary>
    [DefaultValue("center")]
    [Description("Gets or sets the title alignment.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Align { get; set; } = "center";
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
    [DefaultValue(null)]
    [Description("Gets or sets the font used to render the title.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartFont? Font { get; set; }

    //fullSize
    //padding
    //position

    /// <summary>
    /// Gets or sets the title text.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the title text.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Text { get; set; }

    #endregion
}

/// <summary>
/// Tooltip for bubble, doughnut, pie, polar area, and scatter charts
/// <see href="https://www.chartjs.org/docs/latest/configuration/tooltip.html" />
/// </summary>
[AddedVersion("1.10.2")]
public class ChartPluginsTooltip
{
    #region Properties, Indexers

    /// <summary>
    /// Background color of the tooltip.
    /// </summary>
    [DefaultValue("rgba(0, 0, 0, 0.8)")]
    [Description("Gets or sets the tooltip background color.")]
    public string BackgroundColor { get; set; } = "rgba(0, 0, 0, 0.8)";

    /// <summary>
    /// Horizontal alignment of the body text lines. left/right/center.
    /// </summary>
    [DefaultValue("left")]
    [Description("Gets or sets the tooltip body text alignment.")]
    public string BodyAlign { get; set; } = "left";

    /// <summary>
    /// Color of body text.
    /// </summary>
    [DefaultValue("#fff")]
    [Description("Gets or sets the tooltip body text color.")]
    public string BodyColor { get; set; } = "#fff";

    /// <summary>
    /// Gets or sets the font used for tooltip body text.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the font used for tooltip body text.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartPluginsTooltipFont? BodyFont { get; set; }

    /// <summary>
    /// Spacing to add to top and bottom of each tooltip item.
    /// </summary>
    [DefaultValue(2)]
    [Description("Gets or sets the spacing around tooltip body items.")]
    public int BodySpacing { get; set; } = 2;

    /// <summary>
    /// Extra distance to move the end of the tooltip arrow away from the tooltip point.
    /// </summary>
    [DefaultValue(2)]
    [Description("Gets or sets the tooltip caret padding.")]
    public int CaretPadding { get; set; } = 2;

    /// <summary>
    /// Size, in px, of the tooltip arrow.
    /// </summary>
    [DefaultValue(5)]
    [Description("Gets or sets the tooltip caret size in pixels.")]
    public int CaretSize { get; set; } = 5;

    /// <summary>
    /// If <see langword="true" />, color boxes are shown in the tooltip.
    /// </summary>
    [DefaultValue(true)]
    [Description("Gets or sets a value indicating whether color boxes are shown in the tooltip.")]
    public bool DisplayColors { get; set; } = true;

    /// <summary>
    /// Are on-canvas tooltips enabled?
    /// </summary>
    [DefaultValue(true)]
    [Description("Gets or sets a value indicating whether on-canvas tooltips are enabled.")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Horizontal alignment of the footer text lines. left/right/center.
    /// </summary>
    [DefaultValue("left")]
    [Description("Gets or sets the tooltip footer text alignment.")]
    public string FooterAlign { get; set; } = "left";

    /// <summary>
    /// Color of footer text.
    /// </summary>
    [DefaultValue("#fff")]
    [Description("Gets or sets the tooltip footer text color.")]
    public string FooterColor { get; set; } = "#fff";

    /// <summary>
    /// Gets or sets the font used for tooltip footer text.
    /// </summary>
    [Description("Gets or sets the font used for tooltip footer text.")]
    public ChartPluginsTooltipFont FooterFont { get; set; } = new();

    /// <summary>
    /// Margin to add before drawing the footer.
    /// </summary>
    [DefaultValue(6)]
    [Description("Gets or sets the tooltip footer top margin.")]
    public int FooterMarginTop { get; set; } = 6;

    /// <summary>
    /// Spacing to add to top and bottom of each footer line.
    /// </summary>
    [DefaultValue(2)]
    [Description("Gets or sets the spacing around tooltip footer lines.")]
    public int FooterSpacing { get; set; } = 2;

    /// <summary>
    /// Horizontal alignment of the title text lines. left/right/center.
    /// </summary>
    [DefaultValue("left")]
    [Description("Gets or sets the tooltip title text alignment.")]
    public string TitleAlign { get; set; } = "left";

    /// <summary>
    /// Color of title text.
    /// </summary>
    [DefaultValue("#fff")]
    [Description("Gets or sets the tooltip title text color.")]
    public string TitleColor { get; set; } = "#fff";

    /// <summary>
    /// Gets or sets the font used for tooltip title text.
    /// </summary>
    [Description("Gets or sets the font used for tooltip title text.")]
    public ChartPluginsTooltipFont TitleFont { get; set; } = new();

    /// <summary>
    /// Margin to add on bottom of title section.
    /// </summary>
    [DefaultValue(6)]
    [Description("Gets or sets the tooltip title bottom margin.")]
    public int TitleMarginBottom { get; set; } = 6;

    /// <summary>
    /// Spacing to add to top and bottom of each title line.
    /// </summary>
    [DefaultValue(2)]
    [Description("Gets or sets the spacing around tooltip title lines.")]
    public int TitleSpacing { get; set; } = 2;

    /// <summary>
    /// Position of the tooltip caret in the X direction. left/center/right.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the tooltip caret x-axis alignment.")]
    public string? XAlign { get; set; }

    /// <summary>
    /// Position of the tooltip caret in the Y direction. top/center/bottom.
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the tooltip caret y-axis alignment.")]
    public string? YAlign { get; set; }

    #endregion
}

/// <summary>
///     <see href="https://www.chartjs.org/docs/latest/general/fonts.html" />
/// </summary>
[AddedVersion("1.10.2")]
public class ChartPluginsTooltipFont
{
    #region Properties, Indexers

    /// <summary>
    /// Default font family for all text, follows CSS font-family options.
    /// 'Helvetica Neue', 'Helvetica', 'Arial', sans-serif
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the tooltip font family.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Family { get; set; }

    /// <summary>
    /// Height of an individual line of text
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/line-height" />
    /// </summary>
    [DefaultValue(1.2d)]
    [Description("Gets or sets the tooltip font line height.")]
    public double LineHeight { get; set; } = 1.2;

    /// <summary>
    /// Default font size (in px) for text. Does not apply to radialLinear scale point labels.
    /// </summary>
    [DefaultValue(12)]
    [Description("Gets or sets the tooltip font size in pixels.")]
    public int Size { get; set; } = 12;

    /// <summary>
    /// Default font style. Does not apply to tooltip title or footer. Does not apply to chart title.
    /// Follows CSS font-style options (i.e. normal, italic, oblique, initial, inherit).
    /// </summary>
    [DefaultValue(null)]
    [Description("Gets or sets the tooltip font style.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Style { get; set; }

    /// <summary>
    /// Default font weight (boldness).
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/font-weight" />
    /// </summary>
    [DefaultValue("bold")]
    [Description("Gets or sets the tooltip font weight.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Weight { get; set; } = "bold";

    #endregion
}
