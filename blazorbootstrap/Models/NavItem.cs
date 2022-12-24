namespace BlazorBootstrap;

public class NavItem
{
    public string Id { get; set; }
    public IReadOnlyList<NavItem>? ChildItems { get; set; }
    public bool HasChilds { get; set; }
    public string Href { get; set; }
    public NavLinkMatch Match { get; set; }
    public string ParentId { get; set; }
    public IconName PrefixIconName { get; set; }
    public int Sequence { get; set; }
    public string Text { get; set; }
}