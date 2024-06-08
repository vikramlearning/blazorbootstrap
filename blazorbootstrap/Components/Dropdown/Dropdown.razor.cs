namespace BlazorBootstrap;

public partial class Dropdown : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private DotNetObjectReference<Dropdown> objRef = default!;

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

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.ButtonGroup, true),
            (Direction.ToDropdownDirectionClass(), true));

    /// <summary>
    /// If <see langword="true" />, enables the auto close.
    /// </summary>
    /// <remarks>
    /// Default value is true.
    /// </remarks>
    [Parameter]
    public bool AutoClose { get; set; } = true;

    /// <summary>
    /// Gets or sets the auto close behavior of the dropdown.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="DropdownAutoCloseBehavior.Both" />.
    /// </remarks>
    [Parameter]
    public DropdownAutoCloseBehavior AutoCloseBehavior { get; set; } = DropdownAutoCloseBehavior.Both;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the dropdown color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="DropdownColor.None" />.
    /// </remarks>
    [Parameter]
    public DropdownColor Color { get; set; } = DropdownColor.None;

    /// <summary>
    /// Gets or sets the direction of the dropdown menu.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="DropdownDirection.Dropdown" />.
    /// </remarks>
    [Parameter]
    public DropdownDirection Direction { get; set; } = DropdownDirection.Dropdown;

    /// <summary>
    /// If <see langword="true" />, dropdown will be disabled.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
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
    /// Gets or sets the dropdown size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="DropdownSize.None" />.
    /// </remarks>
    [Parameter]
    public DropdownSize Size { get; set; } = DropdownSize.None;

    /// <summary>
    /// Gets or sets the toggle button split behavior.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool Split { get; set; }

    #endregion
}
