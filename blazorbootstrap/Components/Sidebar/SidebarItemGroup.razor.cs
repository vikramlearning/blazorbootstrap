namespace BlazorBootstrap;

public partial class SidebarItemGroup : BaseComponent
{
    #region Events

    #endregion Events

    #region Members

    private IReadOnlyList<NavItem>? childItems;

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append("flex-column");

        base.BuildClasses(builder);
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [CascadingParameter] public bool CollapseSidebar { get; set; }

    [Parameter] public IReadOnlyList<NavItem>? NavItems { get; set; }

    #endregion Properties
}

