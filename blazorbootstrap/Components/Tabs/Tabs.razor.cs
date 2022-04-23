using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBootstrap;

public partial class Tabs : BaseComponent
{
    #region Members

    private DotNetObjectReference<Tabs> objRef;

    private List<Tab> tabs = new List<Tab>();

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Nav);
        builder.Append(BootstrapClassProvider.NavTabs);

        base.BuildClasses(builder);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();

        ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.tabs.initialize", ElementId, objRef); });
    }

    internal void AddTab(Tab tab)
    {
        if (tabs != null)
        {
            tabs.Add(tab);
            StateHasChanged(); // This is mandatory
        }
    }

    [JSInvokable] public async Task bsShowTab() => await OnShowing.InvokeAsync();
    [JSInvokable] public async Task bsShownTab() => await OnShown.InvokeAsync();
    [JSInvokable] public async Task bsHideTab() => await OnHiding.InvokeAsync();
    [JSInvokable] public async Task bsHiddenTab() => await OnHidden.InvokeAsync();

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside this.
    /// </summary>
    [Parameter] public RenderFragment ChildContent { get; set; }

    /// <summary>
    /// This event fires on tab show, but before the new tab has been shown.
    /// </summary>
    [Parameter] public EventCallback OnShowing { get; set; }

    /// <summary>
    /// This event fires on tab show after a tab has been shown.
    /// </summary>
    [Parameter] public EventCallback OnShown { get; set; }

    /// <summary>
    /// This event fires when a new tab is to be shown (and thus the previous active tab is to be hidden).
    /// </summary>
    [Parameter] public EventCallback OnHiding { get; set; }

    /// <summary>
    /// This event fires after a new tab is shown (and thus the previous active tab is hidden).
    /// </summary>
    [Parameter] public EventCallback OnHidden { get; set; }

    #endregion Properties
}
