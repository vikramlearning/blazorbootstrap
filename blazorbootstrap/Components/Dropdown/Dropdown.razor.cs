namespace BlazorBootstrap;

/// <summary>
/// Dropdowns are toggleable, contextual overlays for displaying lists of links and more. <br/>
/// They are toggled by clicking, not by hovering; this is an intentional design decision by bootstrap. <br/>
/// For more information, visit <see href="https://getbootstrap.com/docs/5.0/components/dropdowns/"/>.
/// </summary>
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
                    await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.dropdown.dispose", Id);
            }
            catch (JSDisconnectedException)
            {
                // do nothing
            }

            objRef?.Dispose();
        }

        await base.DisposeAsyncCore(disposing);
    }
    
    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.dropdown.initialize", Id, objRef);

        await base.OnAfterRenderAsync(firstRender);
    }
    
    /// <inheritdoc />
    protected override void OnInitialized()
    {
        objRef ??= DotNetObjectReference.Create(this);

        base.OnInitialized();
    }

    /// <summary>
    /// Invoked when the dropdown menu is hidden from the user (will wait for CSS transitions to complete).
    /// </summary> 
    [JSInvokable("bsHiddenDropdown")]
    public Task BsHiddenDropdown() => OnHidden.InvokeAsync();

    /// <summary>
    /// Invoked immediately when the hide method has been called.
    /// </summary>
    [JSInvokable("bsHideDropdown")]
    public Task BsHideDropdown() => OnHiding.InvokeAsync();

    /// <summary>
    /// Invoked immediately when the show instance method is called.
    /// </summary>
    [JSInvokable("bsShowDropdown")]
    public Task BsShowDropdown() => OnShowing.InvokeAsync();

    /// <summary>
    /// Invoked when the dropdown menu is made visible to the user (will wait for CSS transitions to complete).
    /// </summary>
    [JSInvokable("bsShownDropdown")]
    public Task BsShownDropdown() => OnShown.InvokeAsync();

    /// <summary>
    /// Hides the dropdown menu of a given navbar or tabbed navigation.
    /// </summary>
    /// <returns></returns>
    public ValueTask HideAsync() => JsRuntime.InvokeVoidAsync("window.blazorBootstrap.dropdown.hide", Id);

    /// <summary>
    /// Shows the dropdown menu of a given navbar or tabbed navigation.
    /// </summary>
    /// <returns></returns>
    public ValueTask ShowAsync() => JsRuntime.InvokeVoidAsync("window.blazorBootstrap.dropdown.show", Id);

    /// <summary>
    /// Toggles the dropdown menu of a given navbar or tabbed navigation.
    /// </summary>
    /// <returns></returns>
    public ValueTask ToggleAsync() => JsRuntime.InvokeVoidAsync("window.blazorBootstrap.dropdown.toggle", Id);

    /// <summary>
    /// Updates the position of an element’s dropdown.
    /// </summary>
    /// <returns></returns>
    public ValueTask UpdateAsync() => JsRuntime.InvokeVoidAsync("window.blazorBootstrap.dropdown.update", Id);


    /// <summary>
    /// Parameters are loaded manually for sake of performance.
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/performance#implement-setparametersasync-manually"/>
    /// </summary> 
    public override Task SetParametersAsync(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(AutoClose): AutoClose = (bool)parameter.Value!; break;
                case nameof(AutoCloseBehavior): AutoCloseBehavior = (DropdownAutoCloseBehavior)parameter.Value!; break;
                case nameof(ChildContent): ChildContent = (RenderFragment)parameter.Value!; break;
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(Color): Color = (DropdownColor)parameter.Value!; break;
                case nameof(Direction): Direction = (DropdownDirection)parameter.Value!; break;
                case nameof(Disabled): Disabled = (bool)parameter.Value!; break;
                case nameof(Id): Id = (string)parameter.Value!; break;
                case nameof(OnHidden): OnHidden = (EventCallback)parameter.Value!; break;
                case nameof(OnHiding): OnHiding = (EventCallback)parameter.Value!; break;
                case nameof(OnShowing): OnShowing = (EventCallback)parameter.Value!; break;
                case nameof(OnShown): OnShown = (EventCallback)parameter.Value!; break;
                case nameof(Size): Size = (DropdownSize)parameter.Value!; break;
                case nameof(Split): Split = (bool)parameter.Value!; break;

                default: AdditionalAttributes![parameter.Name] = parameter.Value!; break;
            }
        }
        
        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers
     
    /// <summary>
    /// If <see langword="true" />, enables the auto close.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [Parameter] public bool AutoClose { get; set; } = true;

    /// <summary>
    /// Gets or sets the auto close behavior of the dropdown.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="DropdownAutoCloseBehavior.Both" />.
    /// </remarks>
    [Parameter] public DropdownAutoCloseBehavior AutoCloseBehavior { get; set; } = DropdownAutoCloseBehavior.Both;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public RenderFragment? ChildContent { get; set; } 

    /// <summary>
    /// Gets or sets the dropdown color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="DropdownColor.None" />.
    /// </remarks>
    [Parameter] public DropdownColor Color { get; set; } = DropdownColor.None;

    /// <summary>
    /// Gets or sets the direction of the dropdown menu.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="DropdownDirection.Dropdown" />.
    /// </remarks>
    [Parameter] public DropdownDirection Direction { get; set; } = DropdownDirection.Dropdown;

    /// <summary>
    /// If <see langword="true" />, dropdown will be disabled.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// This event is fired when a dropdown element has been hidden from the user (will wait for CSS transitions to complete).
    /// </summary>
    [Parameter] public EventCallback OnHidden { get; set; }

    /// <summary>
    /// This event is fired immediately when the hide method has been called.
    /// </summary>
    [Parameter] public EventCallback OnHiding { get; set; }

    /// <summary>
    /// This event fires immediately when the show instance method is called.
    /// </summary>
    [Parameter] public EventCallback OnShowing { get; set; }

    /// <summary>
    /// This event is fired when a dropdown element has been made visible to the user (will wait for CSS transitions to
    /// complete).
    /// </summary>
    [Parameter] public EventCallback OnShown { get; set; }

    /// <summary>
    /// Gets or sets the dropdown size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="DropdownSize.None" />.
    /// </remarks>
    [Parameter] public DropdownSize Size { get; set; } = DropdownSize.None;

    /// <summary>
    /// Gets or sets the toggle button split behavior.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool Split { get; set; }

    #endregion
}
