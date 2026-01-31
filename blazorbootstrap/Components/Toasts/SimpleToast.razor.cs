namespace BlazorBootstrap;

public partial class SimpleToast : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private DotNetObjectReference<SimpleToast>? objRef;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.toasts.dispose", Id);
            }
            catch (Microsoft.JSInterop.JSDisconnectedException)
            {
                // Circuit already disconnected; ignore.
            }

            objRef?.Dispose();
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await ShowAsync();

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        await base.OnInitializedAsync();
    }

    [JSInvokable]
    public async Task bsHiddenToast() => await Hidden.InvokeAsync(new ToastEventArgs(ToastMessage!.Id, Id!));

    [JSInvokable]
    public async Task bsHideToast() => await Hiding.InvokeAsync(new ToastEventArgs(ToastMessage!.Id, Id!));

    [JSInvokable]
    public async Task bsShownToast() => await Shown.InvokeAsync(new ToastEventArgs(ToastMessage!.Id, Id!));

    [JSInvokable]
    public async Task bsShowToast() => await Showing.InvokeAsync(new ToastEventArgs(ToastMessage!.Id, Id!));

    /// <summary>
    /// Hides an element’s toast.
    /// </summary>
    public async Task HideAsync() => await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.toasts.hide", Id);

    /// <summary>
    /// Reveals an element’s toast.
    /// </summary>
    public async Task ShowAsync() => await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.toasts.show", Id, AutoHide, Delay, objRef);

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Toast, true),
            (ToastMessage!.Type.ToToastTextColorClass(), ToastMessage is not null),
            (ToastMessage!.Type.ToToastBackgroundColorClass(), ToastMessage is not null));

    /// <summary>
    /// Gets or sets the auto hide state.
    /// <para>
    /// Default value is <see langword="true"/>.
    /// </para>
    /// </summary>
    [Parameter]
    public bool AutoHide { get; set; } = true;

    private string CloseButtonClass => $"btn-close-{ToastMessage!.Type.ToToastTextColorClass()}";

    /// <summary>
    /// Gets or sets the delay in milliseconds before hiding the toast.
    /// <para>
    /// Default value is 5000.
    /// </para>
    /// </summary>
    [Parameter]
    public int Delay { get; set; } = 5000;

    /// <summary>
    /// This event is fired when the toast has finished being hidden from the user.
    /// </summary>
    [Parameter]
    public EventCallback<ToastEventArgs> Hidden { get; set; }

    /// <summary>
    /// This event is fired immediately when the hide instance method has been called.
    /// </summary>
    [Parameter]
    public EventCallback<ToastEventArgs> Hiding { get; set; }

    /// <summary>
    /// If <see langword="true" />, shows the close button.
    /// <para>
    /// Default value is <see langword="true"/>.
    /// </para>
    /// </summary>
    [Parameter]
    public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// This event fires immediately when the show instance method is called.
    /// </summary>
    [Parameter]
    public EventCallback<ToastEventArgs> Showing { get; set; }

    /// <summary>
    /// This event is fired when the toast has been made visible to the user.
    /// </summary>
    [Parameter]
    public EventCallback<ToastEventArgs> Shown { get; set; }

    /// <summary>
    /// Gets or sets the toast message.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [Parameter]
    public ToastMessage? ToastMessage { get; set; }

    #endregion
}
