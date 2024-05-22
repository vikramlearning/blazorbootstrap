namespace BlazorBootstrap;

public partial class Sidebar : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool collapseNavMenu = true;

    private bool collapseSidebar = false;

    private bool isMobile = false;

    private IEnumerable<NavItem>? items = null;

    private DotNetObjectReference<Sidebar> objRef = default!;

    private bool requestInProgress = false;

    #endregion

    #region Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.sidebar.initialize", Id, objRef);

            var width = await JSRuntime.InvokeAsync<int>("window.blazorBootstrap.sidebar.windowSize");

            bsWindowResize(width);

            await RefreshDataAsync(firstRender);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        AdditionalAttributes ??= new Dictionary<string, object>();

        await base.OnInitializedAsync();
    }

    [JSInvokable]
    public void bsWindowResize(int width)
    {
        if (width < 641) // mobile
            isMobile = true;
        else
            isMobile = false;
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
        StateHasChanged();
    }

    internal void HideNavMenuOnMobile()
    {
        if (isMobile && !collapseNavMenu)
            collapseNavMenu = true;
    }

    private string GetNavMenuCssClass()
    {
        var classList = new HashSet<string>();

        if (collapseNavMenu)
            classList.Add("collapse");

        classList.Add("bb-sidebar-content nav-scrollable bb-scrollbar");

        if (collapseSidebar)
            classList.Add("bb-scrollbar-hidden");

        return string.Join(" ", classList);
    }

    private void ToggleNavMenu() => collapseNavMenu = !collapseNavMenu;

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass("bb-sidebar")
            .AddClass("collapsed", collapseSidebar)
            .AddClass("expanded", !collapseSidebar)
            .Build();

    /// <summary>
    /// Gets or sets the badge text.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string? BadgeText { get; set; }

    /// <summary>
    /// Gets or sets the custom icon name.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string? CustomIconName { get; set; }

    /// <summary>
    /// Gets or sets the data provider.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    [EditorRequired]
    public SidebarDataProviderDelegate? DataProvider { get; set; } = default!;

    /// <summary>
    /// Gets or sets the IconName.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconName.None" />.
    /// </remarks>
    [Parameter]
    public IconName IconName { get; set; } = IconName.None;

    /// <summary>
    /// Gets or sets the sidebar logo.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string? ImageSrc { get; set; }

    private string? navMenuCssClass => GetNavMenuCssClass();

    /// <summary>
    /// Gets or sets the sidebar title.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    [EditorRequired]
    public string? Title { get; set; } = default!;

    [Parameter]
    public string? Href { get; set; }

    #endregion
}
