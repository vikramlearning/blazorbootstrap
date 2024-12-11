namespace BlazorBootstrap;

/// <summary>
/// Build hidden sidebars into your project for navigation, shopping carts, and more with Blazor Bootstrap offcanvas component.
/// For more information, visit the <see href="https://getbootstrap.com/docs/5.0/components/offcanvas/">Bootstrap Offcanvas</see> documentation.
/// </summary>
public partial class Offcanvas : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private Type? childComponent;

    private DotNetObjectReference<Offcanvas> objRef = default!;

    private Dictionary<string, object> offcanvasParameters = default!;

    private string title = default!;

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
                    await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.offcanvas.dispose", Id);
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
            await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.offcanvas.initialize", Id, UseStaticBackdrop, CloseOnEscape, IsScrollable, objRef);

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        title = Title;

        await base.OnInitializedAsync();
    }

    [JSInvokable("bsHiddenOffcanvas")]
    public Task BsHiddenOffCanvas() => OnHidden.InvokeAsync();

    [JSInvokable("bsHideOffcanvas")]
    public Task BsHideOffCanvas() => OnHiding.InvokeAsync();

    [JSInvokable("bsShownOffcanvas")]
    public Task BsShownOffCanvas() => OnShown.InvokeAsync();

    [JSInvokable("bsShowOffcanvas")]
    public Task BsShowOffcanvas() => OnShowing.InvokeAsync();

    /// <summary>
    /// Hides an offcanvas.
    /// </summary>
    public ValueTask HideAsync() => JsRuntime.InvokeVoidAsync("window.blazorBootstrap.offcanvas.hide", Id);

    /// <summary>
    /// Shows an offcanvas.
    /// </summary>
    public Task ShowAsync() => ShowAsync(null, null, null);

    /// <summary>
    /// Opens a offcanvas.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="title"></param>
    /// <param name="parameters"></param>
    public Task ShowAsync<T>(string title, Dictionary<string, object>? parameters = null) => ShowAsync(title, typeof(T), parameters);

    private async Task ShowAsync(string? title, Type? type, Dictionary<string, object>? parameters)
    {
        if (!string.IsNullOrWhiteSpace(title))
            this.title = title;

        childComponent = type;
        this.offcanvasParameters = parameters!;
        await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.offcanvas.show", Id);
        await InvokeAsync(StateHasChanged);
    }



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
                case var _ when String.Equals(parameter.Name, nameof(BodyCssClass), StringComparison.OrdinalIgnoreCase): BodyCssClass = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(BodyTemplate), StringComparison.OrdinalIgnoreCase): BodyTemplate = (RenderFragment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(CloseOnEscape), StringComparison.OrdinalIgnoreCase): CloseOnEscape = (bool)parameter.Value; break;
                 case var _ when String.Equals(parameter.Name, nameof(FooterCssClass), StringComparison.OrdinalIgnoreCase): FooterCssClass = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FooterTemplate), StringComparison.OrdinalIgnoreCase): FooterTemplate = (RenderFragment)parameter.Value; break;
                 case var _ when String.Equals(parameter.Name, nameof(HeaderCssClass), StringComparison.OrdinalIgnoreCase): HeaderCssClass = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(HeaderTemplate), StringComparison.OrdinalIgnoreCase): HeaderTemplate = (RenderFragment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(IsScrollable), StringComparison.OrdinalIgnoreCase): IsScrollable = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(OnHidden), StringComparison.OrdinalIgnoreCase): OnHidden = (EventCallback)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(OnHiding), StringComparison.OrdinalIgnoreCase): OnHiding = (EventCallback)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(OnShowing), StringComparison.OrdinalIgnoreCase): OnShowing = (EventCallback)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(OnShown), StringComparison.OrdinalIgnoreCase): OnShown = (EventCallback)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Placement), StringComparison.OrdinalIgnoreCase): Placement = (Placement)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(ShowCloseButton), StringComparison.OrdinalIgnoreCase): ShowCloseButton = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Size), StringComparison.OrdinalIgnoreCase): Size = (OffcanvasSize)parameter.Value; break;
                
                case var _ when String.Equals(parameter.Name, nameof(TabIndex), StringComparison.OrdinalIgnoreCase): TabIndex = (int)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Title), StringComparison.OrdinalIgnoreCase): Title = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(UseStaticBackdrop), StringComparison.OrdinalIgnoreCase): UseStaticBackdrop = (bool)parameter.Value; break;

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
    /// Gets or sets the body CSS class.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string BodyCssClass { get; set; } = default!;

    /// <summary>
    /// Gets or sets the body template.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public RenderFragment? BodyTemplate { get; set; } 

    /// <summary>
    /// If <see langword="true" />, offcanvas closes when escape key is pressed.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [Parameter] public bool CloseOnEscape { get; set; } = true;

    /// <summary>
    /// Gets or sets the footer CSS class.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string FooterCssClass { get; set; } = default!;

    /// <summary>
    /// Gets or sets the footer template.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public RenderFragment? FooterTemplate { get; set; } 

    /// <summary>
    /// Gets or sets the header CSS class.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string HeaderCssClass { get; set; } = default!;

    /// <summary>
    /// Gets or sets the header template.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public RenderFragment? HeaderTemplate { get; set; } 

    /// <summary>
    /// Indicates whether body scrolling is allowed while offcanvas is open.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool IsScrollable { get; set; }

    /// <summary>
    /// This event is fired when an offcanvas element has been hidden from the user (will wait for CSS transitions to
    /// complete).
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
    /// This event is fired when an offcanvas element has been made visible to the user (will wait for CSS transitions to
    /// complete).
    /// </summary>
    [Parameter] public EventCallback OnShown { get; set; }

    /// <summary>
    /// Gets or sets the offcanvas placement.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Placement.End" />.
    /// </remarks>
    [Parameter] public Placement Placement { get; set; } = Placement.End;

    /// <summary>
    /// If <see langword="true" />, modal shows close button in the header.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [Parameter] public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// Gets or sets the offcanvas size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="OffcanvasSize.Regular" />.
    /// </remarks>
    [Parameter] public OffcanvasSize Size { get; set; } = OffcanvasSize.Regular;

    /// <summary>
    /// Gets or sets the tab index.
    /// </summary>
    /// <remarks>
    /// Default value is -1.
    /// </remarks>
    [Parameter] public int TabIndex { get; set; } = -1;

    /// <summary>
    /// Gets or sets the offcanvas title.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string Title { get; set; } = default!;
    
    /// <summary>
    /// When `UseStaticBackdrop` is set to true, the offcanvas will not close when clicking outside of it.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool UseStaticBackdrop { get; set; }

    /// <summary>
    /// Dependency injected Javascript Runtime
    /// </summary>
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    #endregion
}
