namespace BlazorBootstrap;

/// <summary>
/// Push notifications to your visitors with a toast, a lightweight and easily customizable alert message. <br/>
/// For more information, visit the <see href="https://getbootstrap.com/docs/5.0/components/toasts/">Bootstrap Toasts</see> documentation.
/// </summary>
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
            await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.toasts.dispose", Id);
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

    [JSInvokable("bsHiddenToast")]
    public Task BsHiddenToast() => Hidden.InvokeAsync(new ToastEventArgs(ToastMessage!.Id, Id!));

    [JSInvokable("bsHideToast")]
    public Task BsHideToast() => Hiding.InvokeAsync(new ToastEventArgs(ToastMessage!.Id, Id!));

    [JSInvokable("bsShownToast")]
    public Task BsShownToast() => Shown.InvokeAsync(new ToastEventArgs(ToastMessage!.Id, Id!));

    [JSInvokable("bsShowToast")]
    public Task BsShowToast() => Showing.InvokeAsync(new ToastEventArgs(ToastMessage!.Id, Id!));

    /// <summary>
    /// Hides an element’s toast.
    /// </summary>
    public ValueTask HideAsync() => JsRuntime.InvokeVoidAsync("window.blazorBootstrap.toasts.hide", Id);

    /// <summary>
    /// Reveals an element’s toast.
    /// </summary>
    public ValueTask ShowAsync() => JsRuntime.InvokeVoidAsync("window.blazorBootstrap.toasts.show", Id, AutoHide, Delay, objRef);

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.Toast)
            .AddClass(ToastMessage!.Type.ToToastTextColorClass(), ToastMessage is not null)
            .AddClass(ToastMessage!.Type.ToToastBackgroundColorClass(), ToastMessage is not null)
            .Build();

    /// <summary>
    /// Gets or sets the auto hide state.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [Parameter]
    public bool AutoHide { get; set; } = true;

    private string CloseButtonClass => $"btn-close-{ToastMessage!.Type.ToToastTextColorClass()}";

    /// <summary>
    /// Gets or sets the delay in milliseconds before hiding the toast.
    /// </summary>
    /// <remarks>
    /// Default value is 5000.
    /// </remarks>
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
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
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
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public ToastMessage? ToastMessage { get; set; }

    #endregion
}
