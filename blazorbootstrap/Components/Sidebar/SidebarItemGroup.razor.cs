namespace BlazorBootstrap;

public partial class SidebarItemGroup : BlazorBootstrapComponentBase
{
    #region Methods

    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append("flex-column");

        base.BuildClasses(builder);
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    [CascadingParameter] public bool CollapseSidebar { get; set; }

    [Parameter] public IEnumerable<NavItem>? NavItems { get; set; }

    #endregion
}