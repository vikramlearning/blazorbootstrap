namespace BlazorBootstrap;

public class NavItem
{
    internal IEnumerable<NavItem>? ChildItems { get; set; }
    public string CustomIconName { get; set; }
    internal bool HasChilds { get; set; }
    public string Href { get; set; }
    public string Id { get; set; }
    public IconName IconName { get; set; }
    public NavLinkMatch Match { get; set; }
    public string ParentId { get; set; }
    public int Sequence { get; set; }
    public Target Target { get; set; }
    public string Text { get; set; }
}