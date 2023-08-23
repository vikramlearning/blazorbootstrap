namespace BlazorBootstrap;

public class NavItem
{
    #region Properties, Indexers

    internal IEnumerable<NavItem>? ChildItems { get; set; }

    /// <summary>
    /// Gets or sets the custom icon name.
    /// </summary>
    public string? CustomIconName { get; set; }

    internal bool HasChilds { get; set; }

    /// <summary>
    /// Gets or sets the href.
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