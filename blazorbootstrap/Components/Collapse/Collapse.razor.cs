namespace BlazorBootstrap;

public partial class Collapse : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private DotNetObjectReference<Collapse>? objRef;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (IsRenderComplete)
                    await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.collapse.dispose", Id);
            }
            catch (JSDisconnectedException)
            {
                // do nothing
            }

            objRef?.Dispose();
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.collapse.initialize", Id, Parent, Toggle, objRef);

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        await base.OnInitializedAsync();
    }

    [JSInvokable]
    public async Task bsHiddenCollapse() => await OnHidden.InvokeAsync();

    [JSInvokable]
    public async Task bsHideCollapse() => await OnHiding.InvokeAsync();

    [JSInvokable]
    public async Task bsShowCollapse() => await OnShowing.InvokeAsync();

    [JSInvokable]
    public async Task bsShownCollapse() => await OnShown.InvokeAsync();

    /// <summary>
    /// Hides a collapsible element.
    /// </summary>
    public async Task HideAsync() => await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.collapse.hide", Id);

    /// <summary>
    /// Shows a collapsible element.
    /// </summary>
    public async Task ShowAsync() => await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.collapse.show", Id);

    /// <summary>
    /// Toggles a collapsible element to shown or hidden.
    /// </summary>
    public async Task ToggleAsync() => await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.collapse.toggle", Id);

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.Collapse)
            .AddClass(BootstrapClass.CollapseHorizontal, Horizontal)
            .Build();

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the horizontal.
    /// </summary>
    [Parameter]
    public bool Horizontal { get; set; }

    /// <summary>
    /// This event is fired when a collapse element has been hidden from the user (will wait for CSS transitions to complete).
    /// </summary>
    [Parameter]
    public EventCallback OnHidden { get; set; }

    /// <summary>
    /// This event is fired immediately when the hide method has been called.
    /// </summary>
    [Parameter]
    public EventCallback OnHiding { get; set; }

    /// <summary>
    /// This event fires immediately when the show instance method is called.
    /// </summary>
    [Parameter]
    public EventCallback OnShowing { get; set; }

    /// <summary>
    /// This event is fired when a collapse element has been made visible to the user (will wait for CSS transitions to
    /// complete).
    /// </summary>
    [Parameter]
    public EventCallback OnShown { get; set; }

    /// <summary>
    /// Gets or sets the parent selector, DOM element.
    /// If parent is provided, then all collapsible elements under the specified parent will be closed when this collapsible
    /// item is shown. (similar to traditional accordion behavior - this is dependent on the card class).
    /// The attribute has to be set on the target collapsible area.
    /// </summary>
    [Parameter]
    public object Parent { get; set; } = default!;

    /// <summary>
    /// Toggles the collapsible element on invocation.
    /// </summary>
    [Parameter]
    public bool Toggle { get; set; } = false;

    #endregion
}
