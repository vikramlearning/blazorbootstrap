namespace BlazorBootstrap;

public partial class SimpleToast : BlazorBootstrapComponentBase, IDisposable
{
    #region Fields and Constants

    private DotNetObjectReference<SimpleToast>? objRef;

    #endregion

    #region Methods

    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(ClassProvider.Toast());
        builder.Append($"text-{ClassProvider.ToToastTextColor(ToastMessage.Type)}");
        builder.Append($"bg-{ClassProvider.ToToastBackgroundColor(ToastMessage.Type)}");

        base.BuildClasses(builder);
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.toasts.dispose", ElementId); });
            objRef?.Dispose();
        }

        await base.DisposeAsync(disposing);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();

        ExecuteAfterRender(async () => { await ShowAsync(); });
    }

    [JSInvokable]
    public async Task bsHiddenToast() => await Hidden.InvokeAsync(new ToastEventArgs(ToastMessage.Id, ElementId));

    [JSInvokable]
    public async Task bsHideToast() => await Hiding.InvokeAsync(new ToastEventArgs(ToastMessage.Id, ElementId));

    [JSInvokable]
    public async Task bsShownToast() => await Shown.InvokeAsync(new ToastEventArgs(ToastMessage.Id, ElementId));

    [JSInvokable]
    public async Task bsShowToast() => await Showing.InvokeAsync(new ToastEventArgs(ToastMessage.Id, ElementId));

    /// <summary>
    /// Hides an element’s toast.
    /// </summary>
    public async Task HideAsync() => await JS.InvokeVoidAsync("window.blazorBootstrap.toasts.hide", ElementId);

    /// <summary>
    /// Reveals an element’s toast.
    /// </summary>
    public async Task ShowAsync() => await JS.InvokeVoidAsync("window.blazorBootstrap.toasts.show", ElementId, AutoHide, Delay, objRef);

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Auto hide the toast. Default is false.
    /// </summary>
    [Parameter]
    public bool AutoHide { get; set; } = true;

    private string CloseButtonClass => $"btn-close-{ClassProvider.ToToastTextColor(ToastMessage.Type)}";

    /// <summary>
    /// Delay hiding the toast (ms).
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
    /// Show the close button.
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

    [Parameter] public ToastMessage? ToastMessage { get; set; }

    #endregion
}