﻿namespace BlazorBootstrap;

public partial class Ribbon : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private RibbonTab activeTab = default!;

    private bool isDefaultActiveTabSet = false;

    private DotNetObjectReference<Ribbon> objRef = default!;

    private int removedTabIndex = -1;

    private bool showLastTab = false;

    private List<RibbonTab>? tabs = new();

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing) tabs = null;

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.tabs.initialize", Id, objRef);

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
        if (removedTabIndex > -1)
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
        var activeTabTitle = tabs?.FirstOrDefault(x => x.Id == activeTabId)?.Title;
        var previousActiveTabTitle = tabs?.FirstOrDefault(x => x.Id == previousActiveTabId)?.Title;

        var args = new RibbonEventArgs(activeTabTitle!, previousActiveTabTitle!);
        await OnHidden.InvokeAsync(args);
    }

    [JSInvokable]
    public async Task bsHideTab(string activeTabId, string previousActiveTabId)
    {
        var activeTabTitle = tabs?.FirstOrDefault(x => x.Id == activeTabId)?.Title;
        var previousActiveTabTitle = tabs?.FirstOrDefault(x => x.Id == previousActiveTabId)?.Title;

        var args = new RibbonEventArgs(activeTabTitle!, previousActiveTabTitle!);
        await OnHiding.InvokeAsync(args);
    }

    [JSInvokable]
    public async Task bsShownTab(string activeTabId, string previousActiveTabId)
    {
        var activeTabTitle = tabs?.FirstOrDefault(x => x.Id == activeTabId)?.Title;
        var previousActiveTabTitle = tabs?.FirstOrDefault(x => x.Id == previousActiveTabId)?.Title;

        var args = new RibbonEventArgs(activeTabTitle!, previousActiveTabTitle!);
        await OnShown.InvokeAsync(args);
    }

    [JSInvokable]
    public async Task bsShowTab(string activeTabId, string previousActiveTabId)
    {
        var activeTabTitle = tabs?.FirstOrDefault(x => x.Id == activeTabId)?.Title;
        var previousActiveTabTitle = tabs?.FirstOrDefault(x => x.Id == previousActiveTabId)?.Title;

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

        removedTabIndex = tabIndex;
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

        removedTabIndex = tabIndex;
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
    public void ShowRecentTab() => showLastTab = true;

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

        if (tab is { Active: true, Disabled: false })
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

        await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.tabs.show", tab.Id);

        if (tab?.OnClick.HasDelegate ?? false)
            await tab.OnClick.InvokeAsync(new TabEventArgs(tab!.Name, tab.Title));
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.Nav)
            .AddClass(BootstrapClass.NavTabs, NavStyle == NavStyle.Tabs)
            .AddClass(BootstrapClass.NavPills, NavStyle is (NavStyle.Pills or NavStyle.VerticalPills))
            .AddClass(BootstrapClass.NavUnderline, NavStyle is (NavStyle.Underline or NavStyle.VerticalUnderline))
            .AddClass(BootstrapClass.FlexColumn, IsVertical)
            .Build();

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the tabs fade effect.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool EnableFadeEffect { get; set; }

    /// <summary>
    /// If <see langword="true" />, Ribbon will be rendered vertically.
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
    /// <remarks>
    /// Default value is <see cref="NavStyle.Underline" />.
    /// </remarks>
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
