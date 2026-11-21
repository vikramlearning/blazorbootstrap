namespace BlazorBootstrap;

public class GridSettings
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the column keys (each column's <c>PropertyName</c>, or its <c>HeaderText</c> when it has none;
    /// a later column with the same name gets <c>#2</c>, <c>#3</c>, and so on) in display order.
    /// <see langword="null" /> keeps the order in which the columns are declared. It applies whether or not the grid's column chooser is on.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the column keys in display order. <b>null</b> keeps the declared order.")]
    public IEnumerable<string>? ColumnOrder { get; set; }

    /// <summary>
    /// Gets or sets the filters.
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the filters.")]
    public IEnumerable<FilterItem>? Filters { get; set; }

    /// <summary>
    /// Gets or sets the keys of the columns the user has hidden. <see langword="null" /> hides none.
    /// It applies whether or not the grid's column chooser is on.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the keys of the columns the user has hidden. <b>null</b> hides none.")]
    public IEnumerable<string>? HiddenColumns { get; set; }

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
