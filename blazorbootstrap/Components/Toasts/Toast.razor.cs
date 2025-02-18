namespace BlazorBootstrap;

public partial class Toast : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private string? customIconName;

    private DotNetObjectReference<Toast> objRef = default!;

    private ProgressBar toastProgressBar = default!;

    private double toastProgressBarWidth = 100;

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
                    await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.toasts.dispose", Id);
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
        //await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
            await ShowAsync();

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

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        customIconName = ToastMessage.CustomIconName;

        await base.OnInitializedAsync();
    }

    [JSInvokable]
    public async Task bsHiddenToast() => await Hidden.InvokeAsync(new ToastEventArgs(ToastMessage.Id, Id!));

    [JSInvokable]
    public async Task bsHideToast() => await Hiding.InvokeAsync(new ToastEventArgs(ToastMessage.Id, Id!));

    [JSInvokable]
    public async Task bsShownToast() => await Shown.InvokeAsync(new ToastEventArgs(ToastMessage.Id, Id!));

    [JSInvokable]
    public async Task bsShowToast() => await Showing.InvokeAsync(new ToastEventArgs(ToastMessage.Id, Id!));

    /// <summary>
    /// Hides an element’s toast.
    /// </summary>
    public async Task HideAsync() => await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.toasts.hide", Id);

    /// <summary>
    /// Reveals an element’s toast.
    /// </summary>
    public async Task ShowAsync() => await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.toasts.show", Id, AutoHide, Delay, objRef);

    private string GetIconClass() =>
        ToastMessage.Type switch
        {
            ToastType.Primary => TextColor.Primary.ToTextColorClass(),
            ToastType.Secondary => TextColor.Secondary.ToTextColorClass(),
            ToastType.Success => TextColor.Success.ToTextColorClass(),
            ToastType.Danger => TextColor.Danger.ToTextColorClass(),
            ToastType.Warning => TextColor.Warning.ToTextColorClass(),
            ToastType.Info => TextColor.Info.ToTextColorClass(),
            ToastType.Light => TextColor.Light.ToTextColorClass(),
            ToastType.Dark => TextColor.Dark.ToTextColorClass(),
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

    protected override string? ClassNames =>
        BuildClassNames(Class, (BootstrapClass.Toast, true));

    /// <summary>
    /// Gets or sets the auto hide state.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool AutoHide { get; set; }

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

    private string iconClass => $"{GetIconClass()} me-2".Trim();

    private IconName iconName => GetToastIconName();

    private ProgressColor progressColor => GetProgressColor();

    /// <summary>
    /// If <see langword="true" />, shows the close button.
    /// </summary>
    /// <remarks>
    /// Default value is true.
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
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public ToastMessage ToastMessage { get; set; } = default!;

    #endregion
}
