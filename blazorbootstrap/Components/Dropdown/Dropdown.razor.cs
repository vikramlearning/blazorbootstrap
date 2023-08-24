namespace BlazorBootstrap;

public partial class Dropdown
{
    #region Fields and Constants

    private DotNetObjectReference<Dropdown> objRef = default!;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.ButtonGroup());
        builder.Append(BootstrapClassProvider.DropdownDirection(Direction));

        base.BuildClasses(builder);
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.dropdown.dispose", ElementId); });
            objRef?.Dispose();
        }

        await base.DisposeAsync(disposing);
    }

    protected override void OnInitialized()
    {
        objRef ??= DotNetObjectReference.Create(this);

        base.OnInitialized();

        ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.dropdown.initialize", ElementId, objRef); });
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
    public async Task HideAsync() => await JS.InvokeVoidAsync("window.blazorBootstrap.dropdown.hide", ElementId);

    /// <summary>
    /// Shows the dropdown menu of a given navbar or tabbed navigation.
    /// </summary>
    /// <returns></returns>
    public async Task ShowAsync() => await JS.InvokeVoidAsync("window.blazorBootstrap.dropdown.show", ElementId);

    /// <summary>
    /// Toggles the dropdown menu of a given navbar or tabbed navigation.
    /// </summary>
    /// <returns></returns>
    public async Task ToggleAsync() => await JS.InvokeVoidAsync("window.blazorBootstrap.dropdown.toggle", ElementId);

    /// <summary>
    /// Updates the position of an element’s dropdown.
    /// </summary>
    /// <returns></returns>
    public async Task UpdateAsync() => await JS.InvokeVoidAsync("window.blazorBootstrap.dropdown.update", ElementId);

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

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
    /// Specifies the content to be rendered inside this <see cref="ChildContent" />.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the dropdown direction.
    /// </summary>
    [Parameter]
    public DropdownDirection Direction { get; set; } = DropdownDirection.Dropdown;

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
    /// Gets or sets the toggle button split behavior.
    /// </summary>
    [Parameter]
    public bool Split { get; set; }

    #endregion
}