namespace BlazorBootstrap;

public partial class Ribbon : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private RibbonTab activeTab = default!;

    private bool isDefaultActiveTabSet = false;

    private DotNetObjectReference<Ribbon> objRef = default!;

    private List<RibbonTab>? tabs = new();

    #endregion

    #region Methods

    protected override void BuildClasses()
    {
        this.AddClass(BootstrapClassProvider.Nav);
        this.AddClass(BootstrapClassProvider.NavTabs, NavStyle == NavStyle.Tabs);
        this.AddClass(BootstrapClassProvider.NavPills, NavStyle is (NavStyle.Pills or NavStyle.VerticalPills));
        this.AddClass(BootstrapClassProvider.NavUnderline, NavStyle is (NavStyle.Underline or NavStyle.VerticalUnderline));
        this.AddClass("flex-column", IsVertical);

        base.BuildClasses();
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

        Attributes ??= new Dictionary<string, object>();

        if (IsVertical)
            Attributes.Add("aria-orientation", "vertical");

        await base.OnInitializedAsync();

        QueueAfterRenderAction(async () => await JS.InvokeVoidAsync("window.blazorBootstrap.tabs.initialize", ElementId, objRef), new RenderPriority());
    }

    [JSInvokable]
    public async Task bsHiddenTab(string activeTabId, string previousActiveTabId)
    {
        var activeTabTitle = tabs?.FirstOrDefault(x => x.ElementId == activeTabId)?.Title;
        var previousActiveTabTitle = tabs?.FirstOrDefault(x => x.ElementId == previousActiveTabId)?.Title;

        var args = new RibbonEventArgs(activeTabTitle!, previousActiveTabTitle!);
        await OnHidden.InvokeAsync(args);
    }

    [JSInvokable]
    public async Task bsHideTab(string activeTabId, string previousActiveTabId)
    {
        var activeTabTitle = tabs?.FirstOrDefault(x => x.ElementId == activeTabId)?.Title;
        var previousActiveTabTitle = tabs?.FirstOrDefault(x => x.ElementId == previousActiveTabId)?.Title;

        var args = new RibbonEventArgs(activeTabTitle!, previousActiveTabTitle!);
        await OnHiding.InvokeAsync(args);
    }

    [JSInvokable]
    public async Task bsShownTab(string activeTabId, string previousActiveTabId)
    {
        var activeTabTitle = tabs?.FirstOrDefault(x => x.ElementId == activeTabId)?.Title;
        var previousActiveTabTitle = tabs?.FirstOrDefault(x => x.ElementId == previousActiveTabId)?.Title;

        var args = new RibbonEventArgs(activeTabTitle!, previousActiveTabTitle!);
        await OnShown.InvokeAsync(args);
    }

    [JSInvokable]
    public async Task bsShowTab(string activeTabId, string previousActiveTabId)
    {
        var activeTabTitle = tabs?.FirstOrDefault(x => x.ElementId == activeTabId)?.Title;
        var previousActiveTabTitle = tabs?.FirstOrDefault(x => x.ElementId == previousActiveTabId)?.Title;

        var args = new RibbonEventArgs(activeTabTitle!, previousActiveTabTitle!);
        await OnShowing.InvokeAsync(args);
    }
    
    /// <summary>
    /// Removes the tab by index.
    /// </summary>
    /// <param name="tabIndex"></param>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public void RemoveTabByIndex(int tabIndex)
    {
        if (!tabs?.Any() ?? true) return;

        if (tabIndex < 0 || tabIndex >= tabs!.Count) throw new IndexOutOfRangeException();

        var tab = tabs[tabIndex];

        if (tab is null) return;

        tabs!.Remove(tab);

        QueueAfterRenderAction(async () => { await ShowNextAvailableTabAsync(tabIndex); }, new RenderPriority());
    }

    /// <summary>
    /// Removes the tab by name.
    /// </summary>
    /// <param name="tabName"></param>
    public void RemoveTabByName(string tabName)
    {
        if (!tabs?.Any() ?? true) return;

        var tabIndex = tabs!.FindIndex(x => x.Name == tabName);

        if (tabIndex == -1) return;

        var tab = tabs[tabIndex];

        tabs!.Remove(tab);

        QueueAfterRenderAction(async () => { await ShowNextAvailableTabAsync(tabIndex); }, new RenderPriority());
    }

    /// <summary>
    /// Selects the first tab and show its associated pane.
    /// </summary>
    public async Task ShowFirstTabAsync()
    {
        if (!tabs?.Any() ?? true) return;

        var tab = tabs!.FirstOrDefault(x => !x.Disabled);

        if (tab is { Disabled: false })
            await ShowTabAsync(tab);
    }

    /// <summary>
    /// Selects the last tab and show its associated pane.
    /// </summary>
    public async Task ShowLastTabAsync()
    {
        if (!tabs?.Any() ?? true) return;

        var tab = tabs!.LastOrDefault(x => !x.Disabled);

        if (tab is { Disabled: false })
            await ShowTabAsync(tab);
    }

    /// <summary>
    /// Shows the recently added tab.
    /// </summary>
    public void ShowRecentTab()
    {
        QueueAfterRenderAction(async () => { await ShowLastTabAsync(); }, new RenderPriority());
    }

    /// <summary>
    /// Selects the tab by index and show its associated pane.
    /// </summary>
    /// <param name="tabIndex">The zero-based index of the element to get or set.</param>
    public async Task ShowTabByIndexAsync(int tabIndex)
    {
        if (!tabs?.Any() ?? true) return;

        if (tabIndex < 0 || tabIndex >= tabs!.Count) throw new IndexOutOfRangeException();

        var tab = tabs[tabIndex];

        if (tab is { Disabled: false })
            await ShowTabAsync(tab);
    }

    /// <summary>
    /// Selects the tab by name and show its associated pane.
    /// </summary>
    /// <param name="tabName">The name of the tab to select.</param>
    public async Task ShowTabByNameAsync(string tabName)
    {
        if (!tabs?.Any() ?? true) return;

        var tab = tabs!.LastOrDefault(x => x.Name == tabName && !x.Disabled);

        if (tab is not null)
            await ShowTabAsync(tab);
    }

    internal void AddTab(RibbonTab tab)
    {
        tabs!.Add(tab);

        if (tab is { IsActive: true, Disabled: false })
            activeTab = tab;

        StateHasChanged(); // This is mandatory to reflect changes in UI
    }

    internal async Task OnRibbonItemClick(RibbonItemEventArgs args)
    {
        if (OnClick.HasDelegate)
            await OnClick.InvokeAsync(args);
    }

    /// <summary>
    /// Sets the default active tab.
    /// </summary>
    internal async Task SetDefaultActiveTabAsync()
    {
        if (!tabs?.Any() ?? true) return;

        activeTab ??= tabs!.FirstOrDefault(x => !x.Disabled)!;

        if (activeTab is not null)
            await ShowTabAsync(activeTab);
    }

    private async Task OnTabClickAsync(RibbonTab tab) => await ShowTabAsync(tab);

    private async Task ShowNextAvailableTabAsync(int removedTabIndex)
    {
        if (!tabs?.Any() ?? true) return;

        if (removedTabIndex < 0 || removedTabIndex > tabs!.Count) throw new IndexOutOfRangeException();

        var tabIndex = 0;

        if (removedTabIndex == tabs!.Count)
            tabIndex = tabs!.Count - 1;
        else if (removedTabIndex < tabs!.Count)
            tabIndex = removedTabIndex;

        var tab = tabs[tabIndex];

        if (tab is { Disabled: false })
            await ShowTabAsync(tab);
    }

    private async Task ShowTabAsync(RibbonTab tab)
    {
        if (!isDefaultActiveTabSet)
            isDefaultActiveTabSet = true;

        await JS.InvokeVoidAsync("window.blazorBootstrap.tabs.show", tab.ElementId);

        if (tab?.OnClick.HasDelegate ?? false)
            await tab.OnClick.InvokeAsync(new TabEventArgs(tab!.Name, tab.Title));
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

    /// <summary>
    /// Determines whether the Ribbon is rendered vertically.
    /// </summary>
    private bool IsVertical =>
        NavStyle == NavStyle.Vertical
        || NavStyle == NavStyle.VerticalPills
        || NavStyle == NavStyle.VerticalUnderline;

    /// <summary>
    /// CSS class applied to the parent div of the tab content when vertical.
    /// </summary>
    private string? NavParentDivCssClass => IsVertical ? "d-flex" : default;

    /// <summary>
    /// Get or sets the nav style.
    /// </summary>
    //[Parameter]
    private NavStyle NavStyle { get; set; } = NavStyle.Underline;

    /// <summary>
    /// This event fires when the user clicks the corresponding <see cref="RibbonItem" />.
    /// </summary>
    [Parameter]
    public EventCallback<RibbonItemEventArgs> OnClick { get; set; }

    /// <summary>
    /// This event fires after a new tab is shown (and thus the previous active tab is hidden).
    /// </summary>
    [Parameter]
    public EventCallback<RibbonEventArgs> OnHidden { get; set; }

    /// <summary>
    /// This event fires when a new tab is to be shown (and thus the previous active tab is to be hidden).
    /// </summary>
    [Parameter]
    public EventCallback<RibbonEventArgs> OnHiding { get; set; }

    /// <summary>
    /// This event fires on tab show, but before the new tab has been shown.
    /// </summary>
    [Parameter]
    public EventCallback<RibbonEventArgs> OnShowing { get; set; }

    /// <summary>
    /// This event fires on tab show after a tab has been shown.
    /// </summary>
    [Parameter]
    public EventCallback<RibbonEventArgs> OnShown { get; set; }

    /// <summary>
    /// CSS class applied to the tab content container.
    /// </summary>
    private string? TabContentCssClass => IsVertical ? "tab-content flex-grow-1" : "tab-content";

    #endregion
}
