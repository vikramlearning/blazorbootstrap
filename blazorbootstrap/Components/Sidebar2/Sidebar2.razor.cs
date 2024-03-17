namespace BlazorBootstrap;

public partial class Sidebar2 : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool collapseNavMenu = true;

    private bool collapseSidebar = false;

    private bool isMobile = false;

    private IEnumerable<NavItem>? items = null;

    private DotNetObjectReference<Sidebar2> objRef = default!;

    private bool requestInProgress = false;

    #endregion

    #region Methods

    protected override void BuildClasses()
    {
        this.AddClass("bb-sidebar2");
        this.AddClass("collapsed", collapseSidebar);
        this.AddClass("expanded", !collapseSidebar);

        base.BuildClasses();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var width = await JS.InvokeAsync<int>("window.blazorBootstrap.sidebar.windowSize");
            bsWindowResize(width);
            await RefreshDataAsync(firstRender);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        Attributes ??= new Dictionary<string, object>();
        objRef ??= DotNetObjectReference.Create(this);

        await base.OnInitializedAsync();

        QueueAfterRenderAction(async () => await JS.InvokeVoidAsync("window.blazorBootstrap.sidebar.initialize", ElementId, objRef), new RenderPriority());
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
            var request = new Sidebar2DataProviderRequest();
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

        classList.Add("bb-sidebar2-content nav-scrollable bb-scrollbar");

        if (collapseSidebar)
            classList.Add("bb-scrollbar-hidden");

        return string.Join(" ", classList);
    }

    private void ToggleNavMenu() => collapseNavMenu = !collapseNavMenu;

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
    /// The provider should always return an instance of <see cref="Sidebar2DataProviderResult"/>, and 'null' is not allowed.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public Sidebar2DataProviderDelegate? DataProvider { get; set; } = default!;

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
    public string? Title { get; set; } = default!;

    #endregion
}
