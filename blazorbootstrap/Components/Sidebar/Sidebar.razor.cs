namespace BlazorBootstrap;

public partial class Sidebar : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool collapseNavMenu = true;

    private bool collapseSidebar = false;

    private IEnumerable<NavItem>? items = null;

    private bool requestInProgress = false;

    #endregion

    #region Methods

    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append("bb-sidebar");
        builder.Append("collapsed", collapseSidebar);
        builder.Append("expanded", !collapseSidebar);

        base.BuildClasses(builder);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await RefreshDataAsync(firstRender);

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        Attributes ??= new Dictionary<string, object>();

        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Refresh the sidebar data.
    /// </summary>
    /// <returns>Task</returns>
    public async Task RefreshDataAsync(bool firstRender = false)
    {
        if (requestInProgress)
            return;

        requestInProgress = true;

        if (DataProvider != null)
        {
            var request = new SidebarDataProviderRequest();
            var result = await DataProvider.Invoke(request);
            items = result != null ? result.Data : new List<NavItem>();
        }

        requestInProgress = false;

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Toggles sidebar.
    /// </summary>
    public void ToggleSidebar()
    {
        collapseSidebar = !collapseSidebar;
        DirtyClasses();
        StateHasChanged();
    }

    private string GetNavMenuCssClass()
    {
        var sb = new StringBuilder();

        if (collapseNavMenu)
            sb.Append(" collapse");

        sb.Append(" bb-sidebar-content nav-scrollable bb-scrollbar");

        if (collapseSidebar)
            sb.Append(" bb-scrollbar-hidden");

        return sb.ToString();
    }

    private void ToggleNavMenu()
    {
        collapseNavMenu = !collapseNavMenu;
        Console.WriteLine($"ToggleNavMenu: collapseNavMenu={collapseNavMenu}");
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets the badge text.
    /// </summary>
    [Parameter]
    public string? BadgeText { get; set; }

    /// <summary>
    /// Gets or sets the custom icon name.
    /// </summary>
    [Parameter]
    public string? CustomIconName { get; set; }

    /// <summary>
    /// DataProvider is for items to render.
    /// The provider should always return an instance of 'SidebarDataProviderResult', and 'null' is not allowed.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public SidebarDataProviderDelegate? DataProvider { get; set; }

    /// <summary>
    /// Gets or sets the IconName.
    /// </summary>
    [Parameter]
    public IconName IconName { get; set; }

    /// <summary>
    /// Gets or sets the logo.
    /// </summary>
    [Parameter]
    public string? ImageSrc { get; set; }

    private string? navMenuCssClass => GetNavMenuCssClass();

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public string? Title { get; set; }

    #endregion
}
