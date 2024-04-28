namespace BlazorBootstrap;

public partial class SidebarItemGroup : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.FlexColumn)
            .Build();

    [CascadingParameter] public bool CollapseSidebar { get; set; }

    [Parameter] public IEnumerable<NavItem>? NavItems { get; set; }

    #endregion
}
