namespace BlazorBootstrap;

public class GridSettings
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the filters.
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the filters.")]
    public IEnumerable<FilterItem>? Filters { get; set; }

    /// <summary>
    /// Gets or sets the page number.
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(0)]
    [Description("Gets or sets the page number.")]
    public int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(0)]
    [Description("Gets or sets the page size.")]
    public int PageSize { get; set; }

    #endregion
}
