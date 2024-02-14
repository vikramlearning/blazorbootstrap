namespace BlazorBootstrap;

public partial class Toast : BlazorBootstrapComponentBase, IDisposable
{
    #region Fields and Constants

    private string? customIconName;

    private DotNetObjectReference<Toast> objRef = default!;

    private ProgressBar toastProgressBar = default!;

    private double toastProgressBarWidth = 100;

    #endregion

    #region Methods

    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Toast());
        builder.Append(BootstrapClassProvider.BackgroundColor(BackgroundColor.White));

        base.BuildClasses(builder);
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (Rendered)
                    await JS.InvokeVoidAsync("window.blazorBootstrap.toasts.dispose", ElementId);
            }
            catch (JSDisconnectedException)
            {
                // do nothing
            }
            
            objRef?.Dispose();
        }

        await base.DisposeAsync(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender && AutoHide && Delay > 0)
        {
            var decrementWidth = (double)10000 / Delay;
            using var periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));

            while (await periodicTimer.WaitForNextTickAsync())
            {
                if (toastProgressBarWidth == 0)
                    break;

                if (decrementWidth is < 0 or > 100)
                    continue;

                if (toastProgressBarWidth - decrementWidth < 0)
                    toastProgressBarWidth = 0;
                else
                    toastProgressBarWidth -= decrementWidth;

                StateHasChanged();
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        customIconName = ToastMessage.CustomIconName;
        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();

        QueueAfterRenderAction(async () => { await ShowAsync(); });
    }

    [JSInvokable]
    public async Task bsHiddenToast() => await Hidden.InvokeAsync(new ToastEventArgs(ToastMessage.Id, ElementId!));

    [JSInvokable]
    public async Task bsHideToast() => await Hiding.InvokeAsync(new ToastEventArgs(ToastMessage.Id, ElementId!));

    [JSInvokable]
    public async Task bsShownToast() => await Shown.InvokeAsync(new ToastEventArgs(ToastMessage.Id, ElementId!));

    [JSInvokable]
    public async Task bsShowToast() => await Showing.InvokeAsync(new ToastEventArgs(ToastMessage.Id, ElementId!));

    /// <summary>
    /// Hides an element’s toast.
    /// </summary>
    public async Task HideAsync() => await JS.InvokeVoidAsync("window.blazorBootstrap.toasts.hide", ElementId);

    /// <summary>
    /// Reveals an element’s toast.
    /// </summary>
    public async Task ShowAsync() => await JS.InvokeVoidAsync("window.blazorBootstrap.toasts.show", ElementId, AutoHide, Delay, objRef);

    private string GetIconClass() =>
        ToastMessage.Type switch
        {
            ToastType.Primary => BootstrapClassProvider.TextColor(TextColor.Primary),
            ToastType.Secondary => BootstrapClassProvider.TextColor(TextColor.Secondary),
            ToastType.Success => BootstrapClassProvider.TextColor(TextColor.Success),
            ToastType.Danger => BootstrapClassProvider.TextColor(TextColor.Danger),
            ToastType.Warning => BootstrapClassProvider.TextColor(TextColor.Warning),
            ToastType.Info => BootstrapClassProvider.TextColor(TextColor.Info),
            ToastType.Light => BootstrapClassProvider.TextColor(TextColor.Light),
            ToastType.Dark => BootstrapClassProvider.TextColor(TextColor.Dark),
            _ => ""
        };

    private ProgressColor GetProgressColor() =>
        ToastMessage.Type switch
        {
            ToastType.Primary => ProgressColor.Primary,
            ToastType.Secondary => ProgressColor.Secondary,
            ToastType.Success => ProgressColor.Success,
            ToastType.Danger => ProgressColor.Danger,
            ToastType.Warning => ProgressColor.Warning,
            ToastType.Info => ProgressColor.Info,
            ToastType.Dark => ProgressColor.Dark,
            _ => ProgressColor.Primary
        };

    private IconName GetToastIconName() =>
        string.IsNullOrWhiteSpace(ToastMessage.CustomIconName)
        && ToastMessage.IconName != IconName.None
            ? ToastMessage.IconName
            : ToastMessage.Type switch
              {
                  ToastType.Primary => IconName.LightbulbFill,
                  ToastType.Secondary => IconName.ExclamationTriangleFill,
                  ToastType.Success => IconName.CheckCircleFill,
                  ToastType.Danger => IconName.Fire,
                  ToastType.Warning => IconName.ExclamationTriangleFill,
                  ToastType.Info => IconName.InfoCircleFill,
                  ToastType.Light => IconName.ExclamationTriangleFill,
                  ToastType.Dark => IconName.ExclamationTriangleFill,
                  _ => IconName.BellFill
              };

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Automatically hide the toast after the delay.
    /// </summary>
    [Parameter]
    public bool AutoHide { get; set; }

    /// <summary>
    /// Delay in milliseconds before hiding the toast.
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

    private string iconClass => $"{GetIconClass()} me-2".Trim();

    private IconName iconName => GetToastIconName();

    private ProgressColor progressColor => GetProgressColor();

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

    [Parameter] public ToastMessage ToastMessage { get; set; } = default!;

    #endregion
}
