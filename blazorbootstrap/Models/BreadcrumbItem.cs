namespace BlazorBootstrap;

/// <summary>
/// Represents a single breadcrumb navigation item.
/// </summary>
public class BreadcrumbItem
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the navigation URL for the breadcrumb item.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the navigation URL for the breadcrumb item.")]
    [ParameterTypeName("string?")]
    public string? Href { get; set; }

    /// <summary>
    /// Gets or sets whether this item represents the current page.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false"/>.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether this item represents the current page.")]
    public bool IsCurrentPage { get; set; }

    /// <summary>
    /// Gets or sets the display text for the breadcrumb item.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the display text for the breadcrumb item.")]
    [ParameterTypeName("string?")]
    public string? Text { get; set; }

    #endregion
}
