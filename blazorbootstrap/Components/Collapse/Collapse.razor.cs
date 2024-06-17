namespace BlazorBootstrap;

/// <summary>
/// Toggle the visibility of content across your project with the Blazor Bootstrap Collapse component.
/// This component is based on the <see href="https://getbootstrap.com/docs/5.0/components/collapse/">Bootstrap Collapse</see> component.
/// </summary>
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
                    await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.collapse.dispose", Id);
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
            await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.collapse.initialize", Id, Parent, Toggle, objRef);

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Invoked when a collapse element has been hidden from the user (will wait for CSS transitions to complete).
    /// </summary> 
    [JSInvokable("bsHiddenCollapse")]
    public Task BsHiddenCollapse() => OnHidden.InvokeAsync();

    /// <summary>
    /// Invoked immediately when the hide method has been called.
    /// </summary> 
    [JSInvokable("bsHideCollapse")]
    public Task BsHideCollapse() => OnHiding.InvokeAsync();

    /// <summary>
    /// Invoked immediately when the show method has been called.
    /// </summary> 
    [JSInvokable("bsShowCollapse")]
    public Task BsShowCollapse() => OnShowing.InvokeAsync();

    /// <summary>
    /// Invoked when a collapse element has been made visible to the user (will wait for CSS transitions to complete).
    /// </summary> 
    [JSInvokable("bsShownCollapse")]
    public Task BsShownCollapse() => OnShown.InvokeAsync();

    /// <summary>
    /// Hides a collapsible element.
    /// </summary>
    public ValueTask HideAsync() => JsRuntime.InvokeVoidAsync("window.blazorBootstrap.collapse.hide", Id);

    /// <summary>
    /// Shows a collapsible element.
    /// </summary>
    public ValueTask ShowAsync() => JsRuntime.InvokeVoidAsync("window.blazorBootstrap.collapse.show", Id);

    /// <summary>
    /// Toggles a collapsible element to shown or hidden.
    /// </summary>
    public ValueTask ToggleAsync() => JsRuntime.InvokeVoidAsync("window.blazorBootstrap.collapse.toggle", Id);

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
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(ChildContent): ChildContent = (RenderFragment)parameter.Value; break;
                case nameof(Horizontal): Horizontal = (bool)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value; break;
                case nameof(OnHidden): OnHidden = (EventCallback)parameter.Value; break;
                case nameof(OnHiding): OnHiding = (EventCallback)parameter.Value; break;
                case nameof(OnShowing): OnShowing = (EventCallback)parameter.Value; break;
                case nameof(OnShown): OnShown = (EventCallback)parameter.Value; break;
                case nameof(Parent): Parent = parameter.Value; break;
                case nameof(Style): Style = (string)parameter.Value; break;
                case nameof(Toggle): Toggle = (bool)parameter.Value; break;
                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
                    break;
            }
        }
        
        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Collapse, true),
            (BootstrapClass.CollapseHorizontal, Horizontal));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    [EditorRequired]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the horizontal collapsing.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
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
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool Toggle { get; set; } = false;

    #endregion
}
