using Microsoft.JSInterop;

namespace BlazorBootstrap;

public partial class Alert
{
    #region Members

    private AlertColor color = AlertColor.None;

    private DotNetObjectReference<Alert> objRef;

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Alert());
        builder.Append(BootstrapClassProvider.AlertColor(Color), Color != AlertColor.None);
        builder.Append(BootstrapClassProvider.AlertDismisable(), Dismissable);

        base.BuildClasses(builder);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();

        ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.alert.initialize", ElementId, objRef); });
    }

    /// <summary>
    /// Closes an alert by removing it from the DOM.
    /// </summary>
    public async Task CloseAsync()
    {
        await JS.InvokeVoidAsync("window.blazorBootstrap.alert.close", ElementId);
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.alert.dispose", ElementId);
            objRef?.Dispose();
        }

        await base.DisposeAsync(disposing);
    }

    [JSInvokable] public async Task bsCloseAlert() => await OnClose.InvokeAsync();
    [JSInvokable] public async Task bsClosedAlert() => await OnClosed.InvokeAsync();

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets the alert color.
    /// </summary>
    [Parameter]
    public AlertColor Color
    {
        get => color;
        set
        {
            color = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Enables the alert to be closed by placing the padding for close button.
    /// </summary>
    [Parameter] public bool Dismissable { get; set; }

    /// <summary>
    /// Fires immediately when the close instance method is called.
    /// </summary>
    [Parameter] public EventCallback OnClose { get; set; }

    /// <summary>
    /// Fired when the alert has been closed and CSS transitions have completed.
    /// </summary>
    [Parameter] public EventCallback OnClosed { get; set; }

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="Alert"/>.
    /// </summary>
    [Parameter] public RenderFragment ChildContent { get; set; }

    #endregion

    // TODO: Review
    // https://getbootstrap.com/docs/5.1/components/alerts/#live-example
    // https://getbootstrap.com/docs/5.1/components/alerts/#additional-content
}
