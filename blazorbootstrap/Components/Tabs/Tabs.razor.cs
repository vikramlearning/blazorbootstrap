namespace BlazorBootstrap;

public partial class Tabs : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private Tab activeTab = default!;

    private bool isDefaultActiveTabSet = false;

    private DotNetObjectReference<Tabs> objRef = default!;

    private List<Tab>? tabs = new();

    #endregion

    #region Methods

    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Nav());
        builder.Append(BootstrapClassProvider.NavTabs(), NavStyle == NavStyle.Tabs);
        builder.Append(BootstrapClassProvider.NavPills(), NavStyle is (NavStyle.Pills or NavStyle.VerticalPills));
        builder.Append(BootstrapClassProvider.NavUnderline(), NavStyle is (NavStyle.Underline or NavStyle.VerticalUnderline));
        builder.Append("flex-column", isVertical);

        base.BuildClasses(builder);
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing) tabs = null;

        await base.DisposeAsync(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Set active tab
        if (firstRender && !isDefaultActiveTabSet)
            await SetDefaultActiveTabAsync();

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();

        ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.tabs.initialize", ElementId, objRef); });
    }

    [JSInvokable]
    public async Task bsHiddenTab(string activeTabId, string previousActiveTabId)
    {
        var activeTabTitle = tabs?.FirstOrDefault(x => x.ElementId == activeTabId)?.Title;
        var previousActiveTabTitle = tabs?.FirstOrDefault(x => x.ElementId == previousActiveTabId)?.Title;

        var args = new TabsEventArgs(activeTabTitle, previousActiveTabTitle);
        await OnHidden.InvokeAsync(args);
    }

    [JSInvokable]
    public async Task bsHideTab(string activeTabId, string previousActiveTabId)
    {
        var activeTabTitle = tabs?.FirstOrDefault(x => x.ElementId == activeTabId)?.Title;
        var previousActiveTabTitle = tabs?.FirstOrDefault(x => x.ElementId == previousActiveTabId)?.Title;

        var args = new TabsEventArgs(activeTabTitle, previousActiveTabTitle);
        await OnHiding.InvokeAsync(args);
    }

    [JSInvokable]
    public async Task bsShownTab(string activeTabId, string previousActiveTabId)
    {
        var activeTabTitle = tabs?.FirstOrDefault(x => x.ElementId == activeTabId)?.Title;
        var previousActiveTabTitle = tabs?.FirstOrDefault(x => x.ElementId == previousActiveTabId)?.Title;

        var args = new TabsEventArgs(activeTabTitle, previousActiveTabTitle);
        await OnShown.InvokeAsync(args);
    }

    [JSInvokable]
    public async Task bsShowTab(string activeTabId, string previousActiveTabId)
    {
        var activeTabTitle = tabs?.FirstOrDefault(x => x.ElementId == activeTabId)?.Title;
        var previousActiveTabTitle = tabs?.FirstOrDefault(x => x.ElementId == previousActiveTabId)?.Title;

        var args = new TabsEventArgs(activeTabTitle, previousActiveTabTitle);
        await OnShowing.InvokeAsync(args);
    }

    /// <summary>
    /// Selects the first tab and show its associated pane.
    /// </summary>
    public async Task ShowFirstTabAsync()
    {
        var tab = tabs.FirstOrDefault(x => !x.Disabled);

        if (tab != null)
            await ShowTabAsync(tab.ElementId);
    }

    /// <summary>
    /// Selects the last tab and show its associated pane.
    /// </summary>
    public async Task ShowLastTabAsync()
    {
        var tab = tabs.LastOrDefault(x => !x.Disabled);

        if (tab != null)
            await ShowTabAsync(tab.ElementId);
    }

    /// <summary>
    /// Selects the tab by index and show its associated pane.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    public async Task ShowTabByIndexAsync(int index)
    {
        if (index < 0 || index >= tabs.Count) throw new IndexOutOfRangeException();

        var tab = tabs[index];

        if (tab != null && !tab.Disabled)
            await ShowTabAsync(tab.ElementId);
    }

    /// <summary>
    /// Selects the tab by name and show its associated pane.
    /// </summary>
    /// <param name="tabName"></param>
    public async Task ShowTabByNameAsync(string tabName)
    {
        var tab = tabs.LastOrDefault(x => x.Name == tabName && !x.Disabled);

        if (tab != null)
            await ShowTabAsync(tab.ElementId);
    }

    internal void AddTab(Tab tab)
    {
        if (tab != null)
        {
            tabs?.Add(tab);

            if (tab.IsActive && !tab.Disabled) activeTab = tab;

            StateHasChanged(); // This is mandatory
        }
    }

    /// <summary>
    /// Sets default active tab.
    /// </summary>
    internal async Task SetDefaultActiveTabAsync()
    {
        activeTab ??= tabs.FirstOrDefault(x => !x.Disabled);

        if (activeTab != null)
            await ShowTabAsync(activeTab.ElementId);
    }

    private async Task OnTabClickAsync(string tabElementId) => await ShowTabAsync(tabElementId);

    private async Task ShowTabAsync(string elementId)
    {
        if (!isDefaultActiveTabSet)
            isDefaultActiveTabSet = true;

        await JS.InvokeVoidAsync("window.blazorBootstrap.tabs.show", elementId);
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside this.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the tabs fade effect.
    /// </summary>
    [Parameter]
    public bool EnableFadeEffect { get; set; }

    private bool isVertical =>
        NavStyle == NavStyle.Vertical
        || NavStyle == NavStyle.VerticalPills
        || NavStyle == NavStyle.VerticalUnderline;

    private string? navParentDivCssClass => isVertical ? "d-flex" : default;

    /// <summary>
    /// Get or sets the nav style.
    /// </summary>
    [Parameter]
    public NavStyle NavStyle { get; set; } = NavStyle.Tabs;

    /// <summary>
    /// This event fires after a new tab is shown (and thus the previous active tab is hidden).
    /// </summary>
    [Parameter]
    public EventCallback<TabsEventArgs> OnHidden { get; set; }

    /// <summary>
    /// This event fires when a new tab is to be shown (and thus the previous active tab is to be hidden).
    /// </summary>
    [Parameter]
    public EventCallback<TabsEventArgs> OnHiding { get; set; }

    /// <summary>
    /// This event fires on tab show, but before the new tab has been shown.
    /// </summary>
    [Parameter]
    public EventCallback<TabsEventArgs> OnShowing { get; set; }

    /// <summary>
    /// This event fires on tab show after a tab has been shown.
    /// </summary>
    [Parameter]
    public EventCallback<TabsEventArgs> OnShown { get; set; }

    private string? tabContentCssClass => isVertical ? "tab-content flex-grow-1" : "tab-content";

    #endregion
}
