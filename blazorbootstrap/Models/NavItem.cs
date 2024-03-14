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
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the name of the custom icon to display.
    /// </summary>
    public string? CustomIconName { get; set; }

    /// <summary>
    /// Gets or sets a Boolean value indicating whether the navigation item has child items.
    /// </summary>
    internal bool HasChildItems { get; set; }

    /// <summary>
    /// Gets or sets the HyperText Reference (href).
    /// </summary>
    public string? Href { get; set; }

    /// <summary>
    /// Gets or sets the icon color.
    /// </summary>
    public IconColor IconColor { get; set; }

    /// <summary>
    /// Gets or sets the icon name.
    /// </summary>
    public IconName IconName { get; set; }

    /// <summary>
    /// Gets or sets the Id.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the item level.
    /// </summary>
    public int Level { get; set; } = 0;

    /// <summary>
    /// Gets or sets the URL matching behavior.
    /// </summary>
    public NavLinkMatch Match { get; set; }

    /// <summary>
    /// Gets or sets the parent Id.
    /// </summary>
    public string? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the sequence.
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// Gets or sets the target.
    /// </summary>
    public Target Target { get; set; }

    /// <summary>
    /// Gets or sets the navigation link text.
    /// </summary>
    public string? Text { get; set; }

    #endregion
}
