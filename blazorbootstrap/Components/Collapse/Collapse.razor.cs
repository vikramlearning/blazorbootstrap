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
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(ChildContent), StringComparison.OrdinalIgnoreCase): ChildContent = (RenderFragment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Horizontal), StringComparison.OrdinalIgnoreCase): Horizontal = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(OnHidden), StringComparison.OrdinalIgnoreCase): OnHidden = (EventCallback)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(OnHiding), StringComparison.OrdinalIgnoreCase): OnHiding = (EventCallback)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(OnShowing), StringComparison.OrdinalIgnoreCase): OnShowing = (EventCallback)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(OnShown), StringComparison.OrdinalIgnoreCase): OnShown = (EventCallback)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Parent), StringComparison.OrdinalIgnoreCase): Parent = parameter.Value; break;

                case var _ when String.Equals(parameter.Name, nameof(Toggle), StringComparison.OrdinalIgnoreCase): Toggle = (bool)parameter.Value; break;
                default:
                    AdditionalAttributes[parameter.Name] = parameter.Value;
                    break;
            }
        }
        
        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers
      
    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    [EditorRequired]
    public RenderFragment? ChildContent { get; set; }  

    /// <summary>
    /// Gets or sets the horizontal collapsing.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool Horizontal { get; set; }

    /// <summary>
    /// This event is fired when a collapse element has been hidden from the user (will wait for CSS transitions to complete).
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
    /// This event is fired when a collapse element has been made visible to the user (will wait for CSS transitions to
    /// complete).
    /// </summary>
    [Parameter] public EventCallback OnShown { get; set; }

    /// <summary>
    /// Gets or sets the parent selector, DOM element.
    /// If parent is provided, then all collapsible elements under the specified parent will be closed when this collapsible
    /// item is shown. (similar to traditional accordion behavior - this is dependent on the card class).
    /// The attribute has to be set on the target collapsible area.
    /// </summary>
    [Parameter] public object? Parent { get; set; } 

    /// <summary>
    /// Toggles the collapsible element on invocation.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool Toggle { get; set; }

    /// <summary>
    /// Dependency injected Javascript Runtime
    /// </summary>
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    #endregion
}
