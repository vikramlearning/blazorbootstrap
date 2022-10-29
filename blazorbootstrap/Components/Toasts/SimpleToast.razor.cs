namespace BlazorBootstrap;

public partial class SimpleToast : BaseComponent, IDisposable
{
    #region Members

    private DotNetObjectReference<SimpleToast> objRef;

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Toast());
        builder.Append($"text-{BootstrapClassProvider.ToToastTextColor(ToastMessage.Type)}");
        builder.Append($"bg-{BootstrapClassProvider.ToToastBackgroundColor(ToastMessage.Type)}");

        base.BuildClasses(builder);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();

        ExecuteAfterRender(async () => { await ShowAsync(); });
    }

    /// <summary>
    /// Reveals an element’s toast.
    /// </summary>
    public async Task ShowAsync()
    {
        await JS.InvokeVoidAsync("window.blazorBootstrap.toasts.show", ElementId, AutoHide, Delay, objRef);
    }

    /// <summary>
    /// Hides an element’s toast.
    /// </summary>
    public async Task HideAsync()
    {
        await JS.InvokeVoidAsync("window.blazorBootstrap.toasts.hide", ElementId);
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.toasts.dispose", ElementId);
            objRef?.Dispose();
        }

        await base.DisposeAsync(disposing);
    }

    [JSInvokable] public async Task bsShowToast() => await Showing.InvokeAsync(this.ToastMessage.Id);
    [JSInvokable] public async Task bsShownToast() => await Shown.InvokeAsync(this.ToastMessage.Id);
    [JSInvokable] public async Task bsHideToast() => await Hiding.InvokeAsync(this.ToastMessage.Id);
    [JSInvokable] public async Task bsHiddenToast() => await Hidden.InvokeAsync(this.ToastMessage.Id);

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [Parameter] public ToastMessage ToastMessage { get; set; }

    /// <summary>
    /// This event fires immediately when the show instance method is called.
    /// </summary>
    [Parameter] public EventCallback<Guid> Showing { get; set; }

    /// <summary>
    /// This event is fired when the toast has been made visible to the user.
    /// </summary>
    [Parameter] public EventCallback<Guid> Shown { get; set; }

    /// <summary>
    /// This event is fired immediately when the hide instance method has been called.
    /// </summary>
    [Parameter] public EventCallback<Guid> Hiding { get; set; }

    /// <summary>
    /// This event is fired when the toast has finished being hidden from the user.
    /// </summary>
    [Parameter] public EventCallback<Guid> Hidden { get; set; }

    /// <summary>
    /// Auto hide the toast. Default is false.
    /// </summary>
    [Parameter] public bool AutoHide { get; set; } = true;

    /// <summary>
    /// Show the close button.
    /// </summary>
    [Parameter] public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// Delay hiding the toast (ms).
    /// </summary>
    [Parameter] public int Delay { get; set; } = 5000;

    private string CloseButtonClass
    {
        get { return $"btn-close-{BootstrapClassProvider.ToToastTextColor(ToastMessage.Type)}"; }
    }

    #endregion Properties
}