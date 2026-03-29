namespace BlazorBootstrap;

public class ChartClickEventArgs
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the dataset index of the clicked chart item.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(0)]
    [Description("Gets or sets the dataset index of the clicked chart item.")]
    public int DatasetIndex { get; set; }

    /// <summary>
    /// Gets or sets the dataset label of the clicked chart item.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the dataset label of the clicked chart item.")]
    public string? DatasetLabel { get; set; }

    /// <summary>
    /// Gets or sets the data index of the clicked chart item.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(0)]
    [Description("Gets or sets the data index of the clicked chart item.")]
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the data label of the clicked chart item.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the data label of the clicked chart item.")]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the raw value of the clicked chart item.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the raw value of the clicked chart item.")]
    public System.Text.Json.JsonElement? Value { get; set; }

    #endregion
}
