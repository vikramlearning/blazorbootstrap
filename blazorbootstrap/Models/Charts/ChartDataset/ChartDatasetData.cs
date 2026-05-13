namespace BlazorBootstrap;

public interface IChartDatasetData { }

/// <summary>
/// Represents a labeled data item that can be appended to a chart dataset.
/// </summary>
[AddedVersion("1.10.0")]
public record ChartDatasetData : IChartDatasetData
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ChartDatasetData" /> record.
    /// </summary>
    /// <param name="datasetLabel">The target dataset label.</param>
    /// <param name="data">The data value to append.</param>
    public ChartDatasetData(string? datasetLabel, object? data)
    {
        DatasetLabel = datasetLabel;
        Data = data;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the dataset label.
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(null)]
    [Description("Gets the dataset label.")]
    public string? DatasetLabel { get; init; }

    /// <summary>
    /// Gets the data value.
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(null)]
    [Description("Gets the data value.")]
    public object? Data { get; init; }

    #endregion
}
