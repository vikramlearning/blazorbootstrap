namespace BlazorBootstrap;

/// <summary>
/// Provide contextual feedback messages for typical user actions with the handful of available and flexible Blazor Bootstrap alert messages. <br/>
/// This component is based on the <see href="https://getbootstrap.com/docs/5.0/components/alerts/">Bootstrap Alert</see> component.
/// </summary>
public partial class Alert : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private DotNetObjectReference<Alert>? objRef;

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
                    await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.alert.dispose", Id);
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
            await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.alert.initialize", Id, objRef);

        await base.OnAfterRenderAsync(firstRender);
    }
    
    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Invoked when the close button is clicked.
    /// </summary> 
    [JSInvokable("bsCloseAlert")]
    public Task BsCloseAlert() => OnClose.InvokeAsync();

    /// <summary>
    /// Invoked when the alert has been closed and CSS transitions have completed.
    /// </summary> 
    [JSInvokable("bsClosedAlert")]
    public Task BsClosedAlert() => OnClosed.InvokeAsync();

    /// <summary>
    /// Closes an alert by removing it from the DOM.
    /// </summary>
    public ValueTask CloseAsync() => JsRuntime.InvokeVoidAsync("window.blazorBootstrap.alert.close", Id);


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
                case nameof(ChildContent): ChildContent = (RenderFragment)parameter.Value; break;
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(Color): Color = (AlertColor)parameter.Value; break;
                case nameof(Dismissable): Dismissable = (bool)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value; break;
                case nameof(OnClose): OnClose = (EventCallback)parameter.Value; break;
                case nameof(OnClosed): OnClosed = (EventCallback)parameter.Value; break;
                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
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
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the alert color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="AlertColor.None" />.
    /// </remarks>
    [Parameter]
    public AlertColor Color { get; set; } = AlertColor.None;

    /// <summary>
    /// If <see langword="true" />, shows an inline close button.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool Dismissable { get; set; }

    /// <summary>
    /// Fires immediately when the close instance method is called.
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>
    /// Fired when the alert has been closed and CSS transitions have completed.
    /// </summary>
    [Parameter]
    public EventCallback OnClosed { get; set; }

    #endregion

    // TODO: Review
    // https://getbootstrap.com/docs/5.1/components/alerts/#live-example
    // https://getbootstrap.com/docs/5.1/components/alerts/#additional-content
}
