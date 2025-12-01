namespace BlazorBootstrap;

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
                    await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.alert.dispose", Id);
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
            await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.alert.initialize", Id, objRef);

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        await base.OnInitializedAsync();
    }

    [JSInvokable]
    public async Task bsCloseAlert() => await OnClose.InvokeAsync();

    [JSInvokable]
    public async Task bsClosedAlert() => await OnClosed.InvokeAsync();

    /// <summary>
    /// Closes an alert by removing it from the DOM.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Closes an alert by removing it from the DOM.")]
    public async Task CloseAsync() => await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.alert.close", Id);

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Alert, true),
            (Color.ToAlertColorClass(), Color != AlertColor.None),
            (BootstrapClass.AlertDismisable, Dismissable));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [ParameterTypeName("RenderFragment?")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the alert color.
    /// <para>
    /// Default value is <see cref="AlertColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(AlertColor.None)]
    [Description("Gets or sets the alert color.")]
    [Parameter]
    public AlertColor Color { get; set; } = AlertColor.None;

    /// <summary>
    /// If <see langword="true" />, shows an inline close button.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("If <b>true</b>, shows an inline close button.")]
    [Parameter]
    public bool Dismissable { get; set; }

    /// <summary>
    /// Fires immediately when the close instance method is called.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Fires immediately when the close instance method is called.")]
    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>
    /// Fired when the alert has been closed and CSS transitions have completed.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Fired when the alert has been closed and CSS transitions have completed.")]
    [Parameter]
    public EventCallback OnClosed { get; set; }

    #endregion

    // TODO: Review
    // https://getbootstrap.com/docs/5.1/components/alerts/#live-example
    // https://getbootstrap.com/docs/5.1/components/alerts/#additional-content
}
