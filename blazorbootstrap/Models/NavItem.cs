namespace BlazorBootstrap;

public class NavItem
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the collection of child navigation items.
    /// </summary>
    internal List<NavItem>? ChildItems { get; set; }

    /// <summary>
    /// Gets or sets an additional CSS class.
    /// </summary>
    [AddedVersion("1.10.3")]
    [Description("Gets or sets an additional CSS class.")]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the name of the custom icon to display.
    /// </summary>
    [AddedVersion("1.4.0")]
    [Description("Gets or sets the name of the custom icon to display.")]
    public string? CustomIconName { get; set; }

    /// <summary>
    /// Gets or sets a Boolean value indicating whether the navigation item has child items.
    /// </summary>
    internal bool HasChildItems { get; set; }

    /// <summary>
    /// Gets or sets the HyperText Reference (href).
    /// </summary>
    [AddedVersion("1.4.0")]
    [Description("Gets or sets the HyperText Reference (href).")]
    public string? Href { get; set; }

    /// <summary>
    /// Gets or sets the icon color.
    /// </summary>
    [AddedVersion("1.4.0")]
    [Description("Gets or sets the icon color.")]
    public IconColor IconColor { get; set; }

    /// <summary>
    /// Gets or sets the icon name.
    /// </summary>
    [AddedVersion("1.4.0")]
    [Description("Gets or sets the icon name.")]
    public IconName IconName { get; set; }

    /// <summary>
    /// Gets or sets the Id.
    /// </summary>
    [AddedVersion("1.4.0")]
    [Description("Gets or sets the Id.")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the item level.
    /// </summary>
    [AddedVersion("1.4.0")]
    [Description("Gets or sets the item level.")]
    public int Level { get; set; }

    /// <summary>
    /// Gets or sets the URL matching behavior.
    /// </summary>
    [AddedVersion("2.1.0")]
    [Description("Gets or sets the URL matching behavior.")]
    public NavLinkMatch Match { get; set; }

    /// <summary>
    /// Gets or sets the parent Id.
    /// </summary>
    [AddedVersion("1.4.0")]
    [Description("Gets or sets the parent Id.")]
    public string? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the sequence.
    /// </summary>
    [AddedVersion("1.4.0")]
    [Description("Gets or sets the sequence.")]
    public int Sequence { get; set; }

    /// <summary>
    /// Gets or sets the target.
    /// </summary>
    [AddedVersion("1.4.0")]
    [Description("Gets or sets the target.")]
    public Target Target { get; set; }

    /// <summary>
    /// Gets or sets the navigation link text.
    /// </summary>
    [AddedVersion("1.4.0")]
    [Description("Gets or sets the navigation link text.")]
    public string? Text { get; set; }

    #endregion
}
