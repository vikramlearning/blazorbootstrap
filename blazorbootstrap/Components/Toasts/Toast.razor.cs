namespace BlazorBootstrap;

public partial class Toast : BaseComponent, IDisposable
{
    #region Members

    private DotNetObjectReference<Toast> objRef;

    private IconName iconName => GetToastIconName();

    private string customIconName;

    private string iconClass => $"{GetIconClass()} me-2".Trim();

    private string toastProgressClass => $"{GetToastProgressBorder()}";

    #endregion Members

    #region Methods

    protected override void OnInitialized()
    {
        customIconName = this.ToastMessage.CustomIconName;

        base.OnInitialized();
    }

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Toast());
        builder.Append(BootstrapClassProvider.BackgroundColor(BackgroundColor.White));

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

    private string GetIconClass()
    {
        return this.ToastMessage.Type switch
        {
            ToastType.Primary => BootstrapClassProvider.TextColor(TextColor.Primary),
            ToastType.Secondary => BootstrapClassProvider.TextColor(TextColor.Secondary),
            ToastType.Success => BootstrapClassProvider.TextColor(TextColor.Success),
            ToastType.Danger => BootstrapClassProvider.TextColor(TextColor.Danger),
            ToastType.Warning => BootstrapClassProvider.TextColor(TextColor.Warning),
            ToastType.Info => BootstrapClassProvider.TextColor(TextColor.Info),
            ToastType.Light => BootstrapClassProvider.TextColor(TextColor.Light),
            ToastType.Dark => BootstrapClassProvider.TextColor(TextColor.Dark),
            _ => "",
        };
    }

    private IconName GetToastIconName()
    {
        if (string.IsNullOrWhiteSpace(this.ToastMessage.CustomIconName)
            && this.ToastMessage.IconName != IconName.None)
            return this.ToastMessage.IconName;

        return this.ToastMessage.Type switch
        {
            ToastType.Primary => IconName.LightbulbFill,
            ToastType.Secondary => IconName.ExclamationTriangleFill,
            ToastType.Success => IconName.CheckCircleFill,
            ToastType.Danger => IconName.Fire,
            ToastType.Warning => IconName.ExclamationTriangleFill,
            ToastType.Info => IconName.InfoCircleFill,
            ToastType.Light => IconName.ExclamationTriangleFill,
            ToastType.Dark => IconName.ExclamationTriangleFill,
            _ => IconName.BellFill,
        };
    }

    private string GetToastProgressBorder()
    {
        return this.ToastMessage.Type switch
        {
            ToastType.Primary => BootstrapClassProvider.BackgroundColor(BackgroundColor.Primary),
            ToastType.Secondary => BootstrapClassProvider.BackgroundColor(BackgroundColor.Secondary),
            ToastType.Success => BootstrapClassProvider.BackgroundColor(BackgroundColor.Success),
            ToastType.Danger => BootstrapClassProvider.BackgroundColor(BackgroundColor.Danger),
            ToastType.Warning => BootstrapClassProvider.BackgroundColor(BackgroundColor.Warning),
            ToastType.Info => BootstrapClassProvider.BackgroundColor(BackgroundColor.Info),
            ToastType.Light => BootstrapClassProvider.BackgroundColor(BackgroundColor.Light),
            ToastType.Dark => BootstrapClassProvider.BackgroundColor(BackgroundColor.Dark),
            _ => "",
        };
    }

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
    [Parameter] public bool AutoHide { get; set; }

    /// <summary>
    /// Show the close button.
    /// </summary>
    [Parameter] public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// Delay hiding the toast (ms)
    /// </summary>
    [Parameter] public int Delay { get; set; } = 5000;

    #endregion Properties
}
