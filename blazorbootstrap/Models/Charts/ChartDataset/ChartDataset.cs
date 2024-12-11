﻿namespace BlazorBootstrap;

public interface IChartDataset { }

/// <summary>
/// <See href="https://www.chartjs.org/docs/latest/general/data-structures.html#dataset-configuration" />
/// </summary>
public class ChartDataset<TData> : IChartDataset
{
    #region Constructors

    public ChartDataset()
    {
        Oid = Guid.NewGuid();
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Get or sets the BackgroundColor.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? BackgroundColor { get; set; }

    /// <summary>
    /// Get or sets the BorderColor.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<string>? BorderColor { get; set; }

    /// <summary>
    /// Get or sets the BorderWidth.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<double>? BorderWidth { get; set; }

    /// <summary>
    /// How to clip relative to chartArea. Positive value allows overflow, negative value clips that many pixels inside
    /// chartArea. 0 = clip at chartArea.
    /// Clipping can also be configured per side: clip: {left: 5, top: false, right: -2, bottom: 0}
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Clip { get; set; }

    /// <summary>
    /// Get or sets the Data.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<TData>? Data { get; set; }

    /// <summary>
    /// Configures the visibility state of the dataset. Set it to <see langword="true" />, to hide the dataset from the chart.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    public bool Hidden { get; set; }

    /// <summary>
    /// The label for the dataset which appears in the legend and tooltips.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="string.Empty" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }
    
    /// <summary>
    /// The background color on hover
    /// </summary>
    public IReadOnlyCollection<string>? HoverBackgroundColor { get; set; }

    /// <summary>
    /// Get unique object id.
    /// </summary>
    public Guid Oid { get; private set; }
    
    /// <summary>
    /// The border color on hover
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<string>? HoverBorderColor { get; set; }

    /// <summary>
    /// The drawing order of dataset. Also affects order for stacking, tooltip and legend.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<double>? HoverBorderWidth { get; set; }
    
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    public int Order { get; set; }

    //Stack
    //https://www.chartjs.org/docs/latest/general/data-structures.html#dataset-configuration

    /// <summary>
    /// Gets or sets the chart type.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; protected set; }

    #endregion
}
