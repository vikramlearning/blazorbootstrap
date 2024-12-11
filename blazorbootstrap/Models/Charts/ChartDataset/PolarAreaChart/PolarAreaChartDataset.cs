﻿namespace BlazorBootstrap;

public class PolarAreaChartDataset : ChartDataset<double?>
{
    #region Properties, Indexers

    /// <summary>
    /// Supported values are 'center' and 'inner'.
    /// When 'center' is set, the borders of arcs next to each other will overlap.
    /// When 'inner' is set, it is guaranteed that all borders will not overlap.
    /// </summary>
    /// <remarks>
    /// Default value is 'center'.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<string>? BorderAlign { get; set; } // TODO: change this to enum

    /// <summary>
    /// Arc border length and spacing of dashes.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<double>? BorderDash { get; set; }

    /// <summary>
    /// Arc border offset for line dashes.
    /// </summary>
    /// <remarks>
    /// Default value is 0.0.
    /// </remarks>
    public double BorderDashOffset { get; set; }

    /// <summary>
    /// Arc border join style.
    /// Supported values are 'round', 'bevel', 'miter'.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<string>? BorderJoinStyle { get; set; } // TODO: change this to enum

    /// <summary>
    /// Arc border width (in pixels).
    /// </summary>
    /// <remarks>
    /// Default value is 2.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? BorderWidth { get; set; }

    /// <summary>
    /// By default the Arc is curved. If <see langword="false" />, the Arc will be flat.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    public bool Circular { get; set; } = true;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public PieChartDatasetDataLabels Datalabels { get; set; } = new();

    /// <summary>
    /// Arc background color when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? HoverBackgroundColor { get; set; }

    /// <summary>
    /// Arc border color when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? HoverBorderColor { get; set; }

    /// <summary>
    /// Arc border length and spacing of dashes when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<double>? HoverBorderDash { get; set; }

    /// <summary>
    /// Arc border offset for line dashes when hovered.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? HoverBorderDashOffset { get; set; }

    /// <summary>
    /// Arc border join style when hovered.
    /// Supported values are 'round', 'bevel', 'miter'.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? HoverBorderJoinStyle { get; set; } // TODO: change this to enum

    /// <summary>
    /// Arc border width when hovered (in pixels).
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? HoverBorderWidth { get; set; }

    #endregion
}

public class PolarAreaChartDatasetDataLabels : ChartDatasetDataLabels { }
