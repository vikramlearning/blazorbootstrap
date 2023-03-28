namespace BlazorBootstrap;

public partial class Collapse
{
    #region Events

    /// <summary>
    /// This event fires immediately when the show instance method is called.
    /// </summary>
    [Parameter] public EventCallback OnShowing { get; set; }

    /// <summary>
    /// This event is fired when a collapse element has been made visible to the user (will wait for CSS transitions to complete).
    /// </summary>
    [Parameter] public EventCallback OnShown { get; set; }

    /// <summary>
    /// This event is fired immediately when the hide method has been called.
    /// </summary>
    [Parameter] public EventCallback OnHiding { get; set; }

    /// <summary>
    /// This event is fired when a collapse element has been hidden from the user (will wait for CSS transitions to complete).
    /// </summary>
    [Parameter] public EventCallback OnHidden { get; set; }

    #endregion

    #region Members

    private bool collapseHorizontal;

    private DotNetObjectReference<Collapse> objRef;

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Collapse());
        builder.Append(BootstrapClassProvider.CollapseHorizontal(), this.CollapseHorizontal);

        base.BuildClasses(builder);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();

        ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.collapse.initialize", ElementId, Parent, Toggle, objRef); });
    }

    /// <summary>
    /// Shows a collapsible element.
    /// </summary>
    private async Task ShowAsync()
    {
        await JS.InvokeVoidAsync("window.blazorBootstrap.collapse.show", ElementId);
    }

    /// <summary>
    /// Hides a collapsible element.
    /// </summary>
    public async Task HideAsync()
    {
        await JS.InvokeVoidAsync("window.blazorBootstrap.collapse.hide", ElementId);
    }

    /// <summary>
    /// Toggles a collapsible element to shown or hidden.
    /// </summary>
    public async Task ToggleAsync()
    {
        await JS.InvokeVoidAsync("window.blazorBootstrap.collapse.toggle", ElementId);
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.collapse.dispose", ElementId);
            objRef?.Dispose();
        }

        await base.DisposeAsync(disposing);
    }

    [JSInvokable] public async Task bsShowCollapse() => await OnShowing.InvokeAsync();
    [JSInvokable] public async Task bsShownCollapse() => await OnShown.InvokeAsync();
    [JSInvokable] public async Task bsHideCollapse() => await OnHiding.InvokeAsync();
    [JSInvokable] public async Task bsHiddenCollapse() => await OnHidden.InvokeAsync();

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="Collapse"/>.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the collapse horizontal.
    /// </summary>
    [Parameter]
    public bool CollapseHorizontal
    {
        get => collapseHorizontal;
        set
        {
            collapseHorizontal = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// selector, DOM element.
    /// If parent is provided, then all collapsible elements under the specified parent will be closed when this collapsible item is shown. (similar to traditional accordion behavior - this is dependent on the card class). 
    /// The attribute has to be set on the target collapsible area.
    /// </summary>
    [Parameter] public string Parent { get; set; } = default!;

    /// <summary>
    /// Toggles the collapsible element on invocation.
    /// </summary>
    [Parameter] public bool Toggle { get; set; } = false;

    #endregion
}

