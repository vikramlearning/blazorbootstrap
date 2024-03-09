namespace BlazorBootstrap;

public partial class Sidebar2ItemGroup : BlazorBootstrapComponentBase
{
    #region Methods

    protected override void BuildClasses()
    {
        this.AddClass("flex-column");

        base.BuildClasses();
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    [CascadingParameter] public bool CollapseSidebar { get; set; }

    /// <summary>
    /// Gets or sets a value representing the URL matching behavior.
    /// </summary>
    [Parameter]
    public NavLinkMatch Match { get; set; }

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter] public IEnumerable<NavItem>? NavItems { get; set; }

    #endregion
}
