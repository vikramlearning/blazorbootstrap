namespace BlazorBootstrap;

public partial class Dropdown : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private DotNetObjectReference<Dropdown> objRef = default!;

    #endregion

    #region Methods

    protected string? ClassNames => new CssClassBuilder(Class)
        .AddClass(BootstrapClass.ButtonGroup)
        .AddClass(Direction.ToDropdownDirectionClass())
        .Build();

    protected string? StyleNames => new CssStyleBuilder(Style).Build();

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (IsRenderComplete)
                    await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.dropdown.dispose", Id);
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
            await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.dropdown.initialize", Id, objRef);

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        objRef ??= DotNetObjectReference.Create(this);

        base.OnInitialized();        
    }

    [JSInvokable]
    public async Task bsHiddenDropdown() => await OnHidden.InvokeAsync();

    [JSInvokable]
    public async Task bsHideDropdown() => await OnHiding.InvokeAsync();

    [JSInvokable]
    public async Task bsShowDropdown() => await OnShowing.InvokeAsync();

    [JSInvokable]
    public async Task bsShownDropdown() => await OnShown.InvokeAsync();

    /// <summary>
    /// Hides the dropdown menu of a given navbar or tabbed navigation.
    /// </summary>
    /// <returns></returns>
    public async Task HideAsync() => await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.dropdown.hide", Id);

    /// <summary>
    /// Shows the dropdown menu of a given navbar or tabbed navigation.
    /// </summary>
    /// <returns></returns>
    public async Task ShowAsync() => await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.dropdown.show", Id);

    /// <summary>
    /// Toggles the dropdown menu of a given navbar or tabbed navigation.
    /// </summary>
    /// <returns></returns>
    public async Task ToggleAsync() => await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.dropdown.toggle", Id);

    /// <summary>
    /// Updates the position of an element’s dropdown.
    /// </summary>
    /// <returns></returns>
    public async Task UpdateAsync() => await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.dropdown.update", Id);

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Enables or disables the auto close.
    /// </summary>
    [Parameter]
    public bool AutoClose { get; set; } = true;

    /// <summary>
    /// Gets or sets the auto close behavior of the dropdown.
    /// </summary>
    [Parameter]
    public DropdownAutoCloseBehavior AutoCloseBehavior { get; set; } = DropdownAutoCloseBehavior.Both;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the direction of the dropdown menu.
    /// </summary>
    [Parameter]
    public DropdownDirection Direction { get; set; } = DropdownDirection.Dropdown;

    /// <summary>
    /// Gets or sets whether the dropdown menu is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// This event is fired when an dropdown element has been hidden from the user (will wait for CSS transitions to complete).
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
    /// This event is fired when an dropdown element has been made visible to the user (will wait for CSS transitions to
    /// complete).
    /// </summary>
    [Parameter]
    public EventCallback OnShown { get; set; }

    /// <summary>
    /// Gets or sets the size of the <see cref="Dropdown" />.
    /// </summary>
    [Parameter]
    public Size Size { get; set; }

    /// <summary>
    /// Gets or sets the toggle button split behavior.
    /// </summary>
    [Parameter]
    public bool Split { get; set; }

    #endregion
}
