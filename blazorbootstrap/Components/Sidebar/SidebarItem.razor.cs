namespace BlazorBootstrap;

public partial class SidebarItem : BaseComponent
{
    #region Events

    private bool expanded = false;

    private string navLinkCssClass => (HasChilds & expanded) ? "nav-link link-dark fw-bold" : "nav-link link-dark";

    private string liCssClass => (HasChilds & expanded) ? "nav-item nav-group expanded" : "nav-item";

    #endregion Events

    #region Members

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        base.BuildClasses(builder);
    }

    protected override async Task OnInitializedAsync()
    { 
        Attributes ??= new Dictionary<string, object>();

        await base.OnInitializedAsync();
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [Parameter] public IReadOnlyList<NavItem>? ChildItems { get; set; }

    [Parameter] public bool HasChilds { get; set; }

    [Parameter] public string Href { get; set; }

    [Parameter] public IconName PrefixIconName { get; set; }

    [Parameter] public string Text { get; set; }

    // TODO: add target support

    #endregion Properties
}

public class NavItem
{
    public string Id { get; set; }
    public IReadOnlyList<NavItem>? ChildItems { get; set; }
    public bool HasChilds { get; set; }
    public string Href { get; set; }
    public string ParentId { get; set; }
    public IconName PrefixIconName { get; set; }
    public int Sequence { get; set; }
    public string Text { get; set; }
}
