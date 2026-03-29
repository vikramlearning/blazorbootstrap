namespace BlazorBootstrap;

public partial class Tabs : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private Tab activeTab = default!;

    private bool isDefaultActiveTabSet = false;

    private DotNetObjectReference<Tabs> objRef = default!;

    private int removedTabIndex = -1;

    private bool showLastTab = false;

    private List<Tab> tabs = new();

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing && tabs is not null)
        {
            foreach (var tab in tabs)
            {
                try
                {
                    if (tab is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync();
                    }
                    else if (tab is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception while disposing tab '{tab?.Name}': {ex}");
                }
            }

            tabs.Clear();
            tabs = null!;
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await SafeInvokeVoidAsync("window.blazorBootstrap.tabs.initialize", Id, objRef);

        // Set active tab
        if (firstRender && !isDefaultActiveTabSet)
            await SetDefaultActiveTabAsync();

        // Show last tab
        if (showLastTab)
        {
            await ShowLastTabAsync();
            showLastTab = false;
        }

        // Show next available tab
        if (removedTabIndex > -1 || removedTabIndex == -99)
        {
            await ShowNextAvailableTabAsync(removedTabIndex);
            removedTabIndex = -1;
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        AdditionalAttributes ??= new Dictionary<string, object>();

        if (IsVertical)
            AdditionalAttributes.Add("aria-orientation", "vertical");

        await base.OnInitializedAsync();
    }

    [JSInvokable]
    public async Task bsHiddenTab(string activeTabId, string previousActiveTabId)
    {
        var activeTab = tabs.FirstOrDefault(x => x.Id == activeTabId);
        var previousActiveTab = tabs.FirstOrDefault(x => x.Id == previousActiveTabId);

        var args = new TabsEventArgs(activeTab?.Name!, activeTab?.Title!, previousActiveTab?.Name!, previousActiveTab?.Title!);
        await OnHidden.InvokeAsync(args);
    }

    [JSInvokable]
    public async Task bsHideTab(string activeTabId, string previousActiveTabId)
    {
        var activeTab = tabs.FirstOrDefault(x => x.Id == activeTabId);
        var previousActiveTab = tabs.FirstOrDefault(x => x.Id == previousActiveTabId);

        var args = new TabsEventArgs(activeTab?.Name!, activeTab?.Title!, previousActiveTab?.Name!, previousActiveTab?.Title!);
        await OnHiding.InvokeAsync(args);
    }

    [JSInvokable]
    public async Task bsShownTab(string activeTabId, string previousActiveTabId)
    {
        var activeTab = tabs.FirstOrDefault(x => x.Id == activeTabId);
        var previousActiveTab = tabs.FirstOrDefault(x => x.Id == previousActiveTabId);

        var args = new TabsEventArgs(activeTab?.Name!, activeTab?.Title!, previousActiveTab?.Name!, previousActiveTab?.Title!);
        await OnShown.InvokeAsync(args);
    }

    [JSInvokable]
    public async Task bsShowTab(string activeTabId, string previousActiveTabId)
    {
        var activeTab = tabs.FirstOrDefault(x => x.Id == activeTabId);
        var previousActiveTab = tabs.FirstOrDefault(x => x.Id == previousActiveTabId);

        var args = new TabsEventArgs(activeTab?.Name!, activeTab?.Title!, previousActiveTab?.Name!, previousActiveTab?.Title!);
        await OnShowing.InvokeAsync(args);
    }

    /// <summary>
    /// Gets the active tab.
    /// </summary>
    /// <returns>Returns the cuurent active <see cref="Tab"/>.</returns>
    [AddedVersion("3.0.0")]
    [Description("Gets the active tab.")]
    public Tab GetActiveTab() => activeTab;

    /// <summary>
    /// Removes the tab by index.
    /// </summary>
    /// <param name="tabIndex"></param>
    [AddedVersion("2.2.0")]
    [Description("Removes the tab by index.")]
    public void RemoveTabByIndex(int tabIndex)
    {
        var tab = tabs.ElementAtOrDefault(tabIndex);
        if (tab is null)
            return;

        // If active tab is removed then select the next available tab.
        if (activeTab.Id == tab.Id)
            removedTabIndex = tabIndex;
        else
            removedTabIndex = -99;

        tabs.Remove(tab);
    }

    /// <summary>
    /// Removes the tab by name.
    /// </summary>
    /// <param name="tabName"></param>
    [AddedVersion("2.2.0")]
    [Description("Removes the tab by name.")]
    public void RemoveTabByName(string tabName)
    {
        var tabIndex = tabs.FindIndex(x => x.Name == tabName);
        if (tabIndex == -1) return;

        var tab = tabs[tabIndex];

        // If active tab is removed then select the next available tab.
        if (activeTab.Id == tab.Id)
            removedTabIndex = tabIndex;
        else
            removedTabIndex = -99;

        tabs.Remove(tab);
    }

    /// <summary>
    /// Selects the first tab and show its associated pane.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Selects the first tab and show its associated pane.")]
    public async Task ShowFirstTabAsync()
    {
        var tab = tabs.FirstOrDefault(x => !x.Disabled);
        if (tab is null)
            return;

        if (tab is { Disabled: false })
            await ShowTabAsync(tab);
    }

    /// <summary>
    /// Selects the last tab and show its associated pane.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Selects the last tab and show its associated pane.")]
    public async Task ShowLastTabAsync()
    {
        if (tabs.Count == 0) return;

        var tab = tabs.LastOrDefault(x => !x.Disabled);

        if (tab is { Disabled: false })
            await ShowTabAsync(tab);
    }

    /// <summary>
    /// Shows the recently added tab.
    /// </summary>
    [AddedVersion("2.2.0")]
    [Description("Shows the recently added tab.")]
    public void ShowRecentTab() => showLastTab = true;

    /// <summary>
    /// Selects the tab by index and show its associated pane.
    /// </summary>
    /// <param name="tabIndex">The zero-based index of the element to get or set.</param>
    [AddedVersion("1.0.0")]
    [Description("Selects the tab by index and show its associated pane.")]
    public async Task ShowTabByIndexAsync(int tabIndex)
    {
        if (tabs.Count == 0) return;

        if (tabIndex < 0 || tabIndex >= tabs!.Count) throw new IndexOutOfRangeException();

        var tab = tabs[tabIndex];

        if (tab is { Disabled: false })
            await ShowTabAsync(tab);
    }

    /// <summary>
    /// Selects the tab by name and show its associated pane.
    /// </summary>
    /// <param name="tabName">The name of the tab to select.</param>
    [AddedVersion("1.0.0")]
    [Description("Selects the tab by name and show its associated pane.")]
    public async Task ShowTabByNameAsync(string tabName)
    {
        if (tabs.Count == 0) return;

        var tab = tabs.LastOrDefault(x => x.Name == tabName && !x.Disabled);

        if (tab is not null)
            await ShowTabAsync(tab);
    }

    internal void AddTab(Tab tab)
    {
        tabs.Add(tab);

        if (tab is { Active: true, Disabled: false })
            activeTab = tab;

        StateHasChanged(); // This is mandatory
    }

    /// <summary>
    /// Sets default active tab.
    /// </summary>
    internal async Task SetDefaultActiveTabAsync()
    {
        if (tabs.Count == 0) return;

        activeTab ??= tabs.FirstOrDefault(x => !x.Disabled)!;

        if (activeTab is not null)
            await ShowTabAsync(activeTab);
    }

    private async Task OnTabClickAsync(Tab tab) => await ShowTabAsync(tab);

    private async Task ShowNextAvailableTabAsync(int removedTabIndex)
    {
        if (tabs.Count == 0) return;

        // Inactive tab is removed, just show the active tab.
        if (removedTabIndex == -99)
        {
            await ShowTabAsync(activeTab);
            return;
        }

        var tabIndex = 0;

        if (removedTabIndex == tabs.Count)
            tabIndex = tabs.Count - 1;
        else if (removedTabIndex < tabs.Count)
            tabIndex = removedTabIndex;

        var tab = tabs[tabIndex];

        if (tab is { Disabled: false })
            await ShowTabAsync(tab);
    }

    private async Task ShowTabAsync(Tab tab)
    {
        if (!isDefaultActiveTabSet)
            isDefaultActiveTabSet = true;

        queuedTasks.Enqueue(async () => await SafeInvokeVoidAsync("window.blazorBootstrap.tabs.show", tab.Id));

        if (tab?.OnClick.HasDelegate ?? false)
            await tab.OnClick.InvokeAsync(new TabEventArgs(tab.Name!, tab.Title!));

        activeTab = tab!;
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Nav, true),
            (BootstrapClass.NavTabs, NavStyle == NavStyle.Tabs),
            (BootstrapClass.NavPills, NavStyle is (NavStyle.Pills or NavStyle.VerticalPills)),
            (BootstrapClass.NavUnderline, NavStyle is (NavStyle.Underline or NavStyle.VerticalUnderline)),
            (BootstrapClass.FlexColumn, IsVertical));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [EditorRequired]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the tabs fade effect.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the tabs fade effect.")]
    [Parameter]
    public bool EnableFadeEffect { get; set; }

    private bool IsVertical =>
        NavStyle == NavStyle.Vertical
        || NavStyle == NavStyle.VerticalPills
        || NavStyle == NavStyle.VerticalUnderline;

    private string? NavParentDivCssClass => IsVertical ? "d-flex" : default;

    /// <summary>
    /// Get or sets the nav style.
    /// <para>
    /// Default value is <see cref="NavStyle.Tabs" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(NavStyle.Tabs)]
    [Description("Get or sets the nav style.")]
    [Parameter]
    public NavStyle NavStyle { get; set; } = NavStyle.Tabs;

    /// <summary>
    /// This event fires after a new tab is shown (and thus the previous active tab is hidden).
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("This event fires after a new tab is shown (and thus the previous active tab is hidden).")]
    [Parameter]
    public EventCallback<TabsEventArgs> OnHidden { get; set; }

    /// <summary>
    /// This event fires when a new tab is to be shown (and thus the previous active tab is to be hidden).
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("This event fires when a new tab is to be shown (and thus the previous active tab is to be hidden).")]
    [Parameter]
    public EventCallback<TabsEventArgs> OnHiding { get; set; }

    /// <summary>
    /// This event fires on tab show, but before the new tab has been shown.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("This event fires on tab show, but before the new tab has been shown.")]
    [Parameter]
    public EventCallback<TabsEventArgs> OnShowing { get; set; }

    /// <summary>
    /// This event fires on tab show after a tab has been shown.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("This event fires on tab show after a tab has been shown.")]
    [Parameter]
    public EventCallback<TabsEventArgs> OnShown { get; set; }

    private string? TabContentCssClass => IsVertical ? "tab-content flex-grow-1" : "tab-content";

    #endregion
}
