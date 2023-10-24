using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace BlazorBootstrap;

public partial class Sidebar : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool collapseNavMenu = true;

    private bool collapseSidebar = false;

    private bool isMobile = false;

    private DotNetObjectReference<Sidebar> objRef = default!;

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
        {
            var width = await JS.InvokeAsync<int>("window.blazorBootstrap.sidebar.windowSize");
            await bsWindowResize(width);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        Attributes ??= new Dictionary<string, object>();
        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();

        ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.sidebar.initialize", ElementId, objRef); });
    }

    [JSInvokable]
    public async Task bsWindowResize(int width)
    {
        if (width < 641) // mobile
            isMobile = true;
        else
            isMobile = false;
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

        classList.Add("bb-sidebar-content nav-scrollable bb-scrollbar");

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
    /// Gets or sets the collection of <see cref="NavItem" /> which should be displayed by the sidebar.
    /// </summary>
    [Parameter, EditorRequired]
    public IEnumerable<NavItem> Items { get; set; } = Enumerable.Empty<NavItem>();

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
