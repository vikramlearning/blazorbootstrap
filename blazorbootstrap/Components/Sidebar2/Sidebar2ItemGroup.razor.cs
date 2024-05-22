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
    /// <remarks>
    /// Default value is <see cref="NavLinkMatch.Prefix" />.
    /// </remarks>
    [Parameter]
    public NavLinkMatch Match { get; set; } = NavLinkMatch.Prefix;

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// Gets or sets the nav items.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public IEnumerable<NavItem>? NavItems { get; set; }

    #endregion
}
