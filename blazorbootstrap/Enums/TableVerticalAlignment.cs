namespace BlazorBootstrap;

/// <summary>
/// Defines the vertical alignment of a <see cref="Table{TItem}" />'s cells, or of one column's cells (Bootstrap <c>align-*</c>).
/// </summary>
public enum TableVerticalAlignment
{
    /// <summary>
    /// Indicates no alignment class: the table uses Bootstrap's default, and a column follows its table.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates no alignment class: the table uses Bootstrap's default, and a column follows its table.")]
    None,

    /// <summary>
    /// Indicates top alignment (<c>align-top</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates top alignment (align-top).")]
    Top,

    /// <summary>
    /// Indicates middle alignment (<c>align-middle</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates middle alignment (align-middle).")]
    Middle,

    /// <summary>
    /// Indicates bottom alignment (<c>align-bottom</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates bottom alignment (align-bottom).")]
    Bottom
}
