namespace BlazorBootstrap;

public partial class Sidebar2ItemGroup : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.FlexColumn)
            .Build();

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
