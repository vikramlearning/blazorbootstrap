using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBootstrap;

public partial class Tabs : BaseComponent
{
    #region Members

    private DotNetObjectReference<Tabs> objRef;

    private List<Tab> tabs = new List<Tab>();

    private Tab activeTab;

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Nav);

        if (this.NavStyle == NavStyle.Tabs)
            builder.Append(BootstrapClassProvider.NavTabs);
        else
            builder.Append(BootstrapClassProvider.NavPills);

        base.BuildClasses(builder);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();

        ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.tabs.initialize", ElementId, objRef); });
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        // Set active tab
        if (firstRender)
            await SetDefaultActiveTabAsync();
    }

    /// <summary>
    /// Shows an offcanvas.
    /// </summary>
    public async Task SetDefaultActiveTabAsync()
    {
        activeTab = activeTab ?? tabs.FirstOrDefault(x => !x.Disabled);

        if (activeTab != null)
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.tabs.show", activeTab.ElementId);
        }
    }

    internal void AddTab(Tab tab)
    {
        if (tab != null)
        {
            tabs?.Add(tab);

            if (tab.IsActive && !tab.Disabled)
            {
                activeTab = tab;
            }

            StateHasChanged(); // This is mandatory
        }
    }

    [JSInvokable]
    public async Task bsShowTab(string activeTabId, string previousActiveTabId)
    {
        string activeTabTitle = tabs?.FirstOrDefault(x => x.ElementId == activeTabId)?.Title;
        string previousActiveTabTitle = tabs?.FirstOrDefault(x => x.ElementId == previousActiveTabId)?.Title;

        args = new TabsEventArgs(activeTabTitle, previousActiveTabTitle);
        await OnShowing.InvokeAsync(args);
    }

    [JSInvokable]
    public async Task bsShownTab(string activeTabId, string previousActiveTabId)
    {
        string activeTabTitle = tabs?.FirstOrDefault(x => x.ElementId == activeTabId)?.Title;
        string previousActiveTabTitle = tabs?.FirstOrDefault(x => x.ElementId == previousActiveTabId)?.Title;

        args = new TabsEventArgs(activeTabTitle, previousActiveTabTitle);
        await OnShown.InvokeAsync(args);
    }

    [JSInvokable]
    public async Task bsHideTab(string activeTabId, string previousActiveTabId)
    {
        string activeTabTitle = tabs?.FirstOrDefault(x => x.ElementId == activeTabId)?.Title;
        string previousActiveTabTitle = tabs?.FirstOrDefault(x => x.ElementId == previousActiveTabId)?.Title;

        args = new TabsEventArgs(activeTabTitle, previousActiveTabTitle);
        await OnHiding.InvokeAsync(args);
    }

    [JSInvokable]
    public async Task bsHiddenTab(string activeTabId, string previousActiveTabId)
    {
        string activeTabTitle = tabs?.FirstOrDefault(x => x.ElementId == activeTabId)?.Title;
        string previousActiveTabTitle = tabs?.FirstOrDefault(x => x.ElementId == previousActiveTabId)?.Title;

        args = new TabsEventArgs(activeTabTitle, previousActiveTabTitle);
        await OnHidden.InvokeAsync(args);
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            tabs = null;
        }

        await base.DisposeAsync(disposing);
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Get or sets the nav style.
    /// </summary>
    [Parameter] public NavStyle NavStyle { get; set; } = NavStyle.Tabs;

    /// <summary>
    /// Specifies the content to be rendered inside this.
    /// </summary>
    [Parameter] public RenderFragment ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the tabs fade effect.
    /// </summary>
    [Parameter] public bool EnableFadeEffect { get; set; }

    /// <summary>
    /// This event fires on tab show, but before the new tab has been shown.
    /// </summary>
    [Parameter] public EventCallback<TabsEventArgs> OnShowing { get; set; }

    /// <summary>
    /// This event fires on tab show after a tab has been shown.
    /// </summary>
    [Parameter] public EventCallback<TabsEventArgs> OnShown { get; set; }

    /// <summary>
    /// This event fires when a new tab is to be shown (and thus the previous active tab is to be hidden).
    /// </summary>
    [Parameter] public EventCallback<TabsEventArgs> OnHiding { get; set; }

    /// <summary>
    /// This event fires after a new tab is shown (and thus the previous active tab is hidden).
    /// </summary>
    [Parameter] public EventCallback<TabsEventArgs> OnHidden { get; set; }

    public TabsEventArgs args { get; set; }

    #endregion Properties
}