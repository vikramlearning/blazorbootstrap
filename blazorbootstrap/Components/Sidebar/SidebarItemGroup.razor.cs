namespace BlazorBootstrap;

public partial class SidebarItemGroup : BlazorBootstrapComponentBase
{
    #region Methods

    protected string? ClassNames => new CssClassBuilder(Class)
        .AddClass(BootstrapClass.FlexColumn)
        .Build();

    protected string? StyleNames => new CssStyleBuilder(Style).Build();

    #endregion

    #region Properties, Indexers

    [CascadingParameter] public bool CollapseSidebar { get; set; }

    [Parameter] public IEnumerable<NavItem>? NavItems { get; set; }

    #endregion
}
