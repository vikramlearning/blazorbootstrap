namespace BlazorBootstrap;

/// <summary>
/// Defines a Bootstrap table color variant (<c>table-*</c>) for a <see cref="Table{TItem}" />, its head, a row, or a cell.
/// </summary>
public enum TableColor
{
    /// <summary>
    /// Indicates no color variant.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates no color variant.")]
    None,

    /// <summary>
    /// Indicates the primary variant (<c>table-primary</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the primary variant (table-primary).")]
    Primary,

    /// <summary>
    /// Indicates the secondary variant (<c>table-secondary</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the secondary variant (table-secondary).")]
    Secondary,

    /// <summary>
    /// Indicates the success variant (<c>table-success</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the success variant (table-success).")]
    Success,

    /// <summary>
    /// Indicates the danger variant (<c>table-danger</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the danger variant (table-danger).")]
    Danger,

    /// <summary>
    /// Indicates the warning variant (<c>table-warning</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the warning variant (table-warning).")]
    Warning,

    /// <summary>
    /// Indicates the info variant (<c>table-info</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the info variant (table-info).")]
    Info,

    /// <summary>
    /// Indicates the light variant (<c>table-light</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the light variant (table-light).")]
    Light,

    /// <summary>
    /// Indicates the dark variant (<c>table-dark</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the dark variant (table-dark).")]
    Dark
}
