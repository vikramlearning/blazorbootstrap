using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBootstrap;

public partial class Tabs : BaseComponent
{
    #region Members

    private DotNetObjectReference<Tabs> objRef;

    private List<Tab> tabs = new List<Tab>();

    private string previousActiveTabId;

    private string currentActiveTabId;

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Nav);
        builder.Append(BootstrapClassProvider.NavTabs);

        base.BuildClasses(builder);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        objRef ??= DotNetObjectReference.Create(this);

        if (firstRender)
        {
            StateHasChanged(); // This is mandatory
        }
    }

    private async Task OnTabClickAsync(EventArgs args, string elementId)
    {
        previousActiveTabId = currentActiveTabId;
        currentActiveTabId = elementId;

        Console.WriteLine($"Previous: {previousActiveTabId}");
        Console.WriteLine($"Current: {currentActiveTabId}");

        await JS.InvokeVoidAsync("window.blazorBootstrap.tabs.show", elementId, objRef);
    }

    internal void AddTab(Tab tab)
    {
        tabs.Add(tab);
    }

    [JSInvokable] public async Task bsShowTab() => await OnShowing.InvokeAsync();
    [JSInvokable] public async Task bsShownTab() => await OnShown.InvokeAsync();
    [JSInvokable] public async Task bsHideTab() => await OnHiding.InvokeAsync();
    [JSInvokable]
    public async Task bsHiddenTab()
    {
        if (!string.IsNullOrWhiteSpace(previousActiveTabId))
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.tabs.dispose", previousActiveTabId);
        }
        await OnHidden.InvokeAsync();
    }

    #endregion Methods

    #region Properties

    /// <summary>
    /// Specifies the content to be rendered inside this.
    /// </summary>
    [Parameter] public RenderFragment ChildContent { get; set; }

    /// <summary>
    /// This event fires immediately when the show instance method is called.
    /// </summary>
    [Parameter] public EventCallback OnShowing { get; set; }

    /// <summary>
    /// This event is fired when an offcanvas element has been made visible to the user (will wait for CSS transitions to complete).
    /// </summary>
    [Parameter] public EventCallback OnShown { get; set; }

    /// <summary>
    /// This event is fired immediately when the hide method has been called.
    /// </summary>
    [Parameter] public EventCallback OnHiding { get; set; }

    /// <summary>
    /// This event is fired when an offcanvas element has been hidden from the user (will wait for CSS transitions to complete).
    /// </summary>
    [Parameter] public EventCallback OnHidden { get; set; }

    #endregion Properties
}
