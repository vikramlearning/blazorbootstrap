namespace BlazorBootstrap;

public partial class Toast : BaseComponent, IDisposable
{
    #region Members

    private ProgressBar toastProgressBar = default!;

    private double toastProgressBarWidth = 100;

    private DotNetObjectReference<Toast> objRef = default!;

    private IconName iconName => GetToastIconName();

    private string customIconName = default!;

    private string iconClass => $"{GetIconClass()} me-2".Trim();

    private ProgressColor progressColor => GetProgressColor();

    #endregion Members

    #region Methods

    protected override void OnInitialized()
    {
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
        customIconName = this.ToastMessage.CustomIconName;
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

    [JSInvokable] public async Task bsShowToast() => await Showing.InvokeAsync(new ToastEventArgs(this.ToastMessage.Id, this.ElementId));
    [JSInvokable] public async Task bsShownToast() => await Shown.InvokeAsync(new ToastEventArgs(this.ToastMessage.Id, this.ElementId));
    [JSInvokable] public async Task bsHideToast() => await Hiding.InvokeAsync(new ToastEventArgs(this.ToastMessage.Id, this.ElementId));
    [JSInvokable] public async Task bsHiddenToast() => await Hidden.InvokeAsync(new ToastEventArgs(this.ToastMessage.Id, this.ElementId));

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

    private ProgressColor GetProgressColor()
    {
        return this.ToastMessage.Type switch
        {
            ToastType.Primary => ProgressColor.Primary,
            ToastType.Secondary => ProgressColor.Secondary,
            ToastType.Success => ProgressColor.Success,
            ToastType.Danger => ProgressColor.Danger,
            ToastType.Warning => ProgressColor.Warning,
            ToastType.Info => ProgressColor.Info,
            ToastType.Dark => ProgressColor.Dark,
            _ => ProgressColor.Primary,
        };
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender && AutoHide && Delay > 0)
        {
            var decrementWidth = ((double)10000 / Delay);
            using var periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
            while (await periodicTimer.WaitForNextTickAsync())
            {
                if (toastProgressBarWidth == 0)
                    break;

                if (decrementWidth < 0 || decrementWidth > 100)
                    continue;
                else if (toastProgressBarWidth - decrementWidth < 0)
                    toastProgressBarWidth = 0;
                else
                    toastProgressBarWidth -= decrementWidth;

                StateHasChanged();
            }
        }
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [Parameter] public ToastMessage ToastMessage { get; set; } = default!;

    /// <summary>
    /// This event fires immediately when the show instance method is called.
    /// </summary>
    [Parameter] public EventCallback<ToastEventArgs> Showing { get; set; }

    /// <summary>
    /// This event is fired when the toast has been made visible to the user.
    /// </summary>
    [Parameter] public EventCallback<ToastEventArgs> Shown { get; set; }

    /// <summary>
    /// This event is fired immediately when the hide instance method has been called.
    /// </summary>
    [Parameter] public EventCallback<ToastEventArgs> Hiding { get; set; }

    /// <summary>
    /// This event is fired when the toast has finished being hidden from the user.
    /// </summary>
    [Parameter] public EventCallback<ToastEventArgs> Hidden { get; set; }

    /// <summary>
    /// Automatically hide the toast after the delay.
    /// </summary>
    [Parameter] public bool AutoHide { get; set; }

    /// <summary>
    /// Show the close button.
    /// </summary>
    [Parameter] public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// Delay in milliseconds before hiding the toast.
    /// </summary>
    [Parameter] public int Delay { get; set; } = 5000;

    #endregion Properties
}
