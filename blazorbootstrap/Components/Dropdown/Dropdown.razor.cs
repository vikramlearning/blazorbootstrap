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
            if (IsRenderComplete)
                await SafeInvokeVoidAsync("window.blazorBootstrap.dropdown.dispose", Id);

            objRef?.Dispose();
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await SafeInvokeVoidAsync("window.blazorBootstrap.dropdown.initialize", Id, objRef);

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
    /// <returns>Task</returns>
    [AddedVersion("1.10.0")]
    [Description("Hides the dropdown menu of a given navbar or tabbed navigation.")]
    public async Task HideAsync() => await SafeInvokeVoidAsync("window.blazorBootstrap.dropdown.hide", Id);

    /// <summary>
    /// Shows the dropdown menu of a given navbar or tabbed navigation.
    /// </summary>
    /// <returns>Task</returns>
    [AddedVersion("1.10.0")]
    [Description("Shows the dropdown menu of a given navbar or tabbed navigation.")]
    public async Task ShowAsync() => await SafeInvokeVoidAsync("window.blazorBootstrap.dropdown.show", Id);

    /// <summary>
    /// Toggles the dropdown menu of a given navbar or tabbed navigation.
    /// </summary>
    /// <returns>Task</returns>
    [AddedVersion("1.10.0")]
    [Description("Toggles the dropdown menu of a given navbar or tabbed navigation.")]
    public async Task ToggleAsync() => await SafeInvokeVoidAsync("window.blazorBootstrap.dropdown.toggle", Id);

    /// <summary>
    /// Updates the position of an element’s dropdown.
    /// </summary>
    /// <returns>Task</returns>
    [AddedVersion("1.10.0")]
    [Description("Updates the position of an element’s dropdown.")]
    public async Task UpdateAsync() => await SafeInvokeVoidAsync("window.blazorBootstrap.dropdown.update", Id);

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.ButtonGroup, Split),
            (Direction.ToDropdownDirectionClass(), true));

    /// <summary>
    /// If <see langword="true" />, enables the auto close.
    /// <para>
    /// Default value is true.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(true)]
    [Description("If <b>true</b>, enables the auto close.")]
    [Parameter]
    public bool AutoClose { get; set; } = true;

    /// <summary>
    /// Gets or sets the auto close behavior of the dropdown.
    /// <para>
    /// Default value is <see cref="DropdownAutoCloseBehavior.Both" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(DropdownAutoCloseBehavior.Both)]
    [Description("Gets or sets the auto close behavior of the dropdown.")]
    [Parameter]
    public DropdownAutoCloseBehavior AutoCloseBehavior { get; set; } = DropdownAutoCloseBehavior.Both;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the dropdown color.
    /// <para>
    /// Default value is <see cref="DropdownColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(DropdownColor.None)]
    [Description("Gets or sets the dropdown color.")]
    [Parameter]
    public DropdownColor Color { get; set; } = DropdownColor.None;

    /// <summary>
    /// Gets or sets the direction of the dropdown menu.
    /// <para>
    /// Default value is <see cref="DropdownDirection.Dropdown" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(DropdownDirection.Dropdown)]
    [Description("Gets or sets the direction of the dropdown menu.")]
    [Parameter]
    public DropdownDirection Direction { get; set; } = DropdownDirection.Dropdown;

    /// <summary>
    /// If <see langword="true" />, dropdown will be disabled.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.3")]
    [DefaultValue(false)]
    [Description("If <b>true</b>, dropdown will be disabled.")]
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// This event is fired when an dropdown element has been hidden from the user (will wait for CSS transitions to complete).
    /// </summary>
    [AddedVersion("1.10.0")]
    [Description("This event is fired when an dropdown element has been hidden from the user (will wait for CSS transitions to complete).")]
    [Parameter]
    public EventCallback OnHidden { get; set; }

    /// <summary>
    /// This event is fired immediately when the hide method has been called.
    /// </summary>
    [AddedVersion("1.10.0")]
    [Description("This event is fired immediately when the hide method has been called.")]
    [Parameter]
    public EventCallback OnHiding { get; set; }

    /// <summary>
    /// This event fires immediately when the show instance method is called.
    /// </summary>
    [AddedVersion("1.10.0")]
    [Description("This event fires immediately when the show instance method is called.")]
    [Parameter]
    public EventCallback OnShowing { get; set; }

    /// <summary>
    /// This event is fired when an dropdown element has been made visible to the user (will wait for CSS transitions to
    /// complete).
    /// </summary>
    [AddedVersion("1.10.0")]
    [Description("This event is fired when an dropdown element has been made visible to the user (will wait for CSS transitions to")]
    [Parameter]
    public EventCallback OnShown { get; set; }

    /// <summary>
    /// Gets or sets the dropdown size.
    /// <para>
    /// Default value is <see cref="DropdownSize.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.3")]
    [DefaultValue(DropdownSize.None)]
    [Description("Gets or sets the dropdown size.")]
    [Parameter]
    public DropdownSize Size { get; set; } = DropdownSize.None;

    /// <summary>
    /// Gets or sets the toggle button split behavior.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the toggle button split behavior.")]
    [Parameter]
    public bool Split { get; set; }

    #endregion
}
