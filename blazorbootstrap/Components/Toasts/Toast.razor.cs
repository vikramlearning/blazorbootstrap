namespace BlazorBootstrap;

/// <summary>
/// Push notifications to your visitors with a toast, a lightweight and easily customizable alert message. <br/>
/// For more information, visit the <see href="https://getbootstrap.com/docs/5.0/components/toasts/">Bootstrap Toasts</see> documentation.
/// </summary>
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
                    await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.toasts.dispose", Id);
            }
            catch (JSDisconnectedException)
            {
                // do nothing
            }

            objRef?.Dispose();
        }

        await base.DisposeAsyncCore(disposing);
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
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

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        customIconName = ToastMessage.CustomIconName;

        await base.OnInitializedAsync();
    }

    [JSInvokable("bsHiddenToast")]
    public Task BsHiddenToast() => Hidden.InvokeAsync(new ToastEventArgs(ToastMessage.Id, Id!));

    [JSInvokable("bsHideToast")]
    public Task BsHideToast() => Hiding.InvokeAsync(new ToastEventArgs(ToastMessage.Id, Id!));

    [JSInvokable("bsShownToast")]
    public Task BsShownToast() => Shown.InvokeAsync(new ToastEventArgs(ToastMessage.Id, Id!));

    [JSInvokable("bsShowToast")]
    public Task BsShowToast() => Showing.InvokeAsync(new ToastEventArgs(ToastMessage.Id, Id!));

    /// <summary>
    /// Hides an element’s toast.
    /// </summary>
    public ValueTask HideAsync() => JsRuntime.InvokeVoidAsync("window.blazorBootstrap.toasts.hide", Id);

    /// <summary>
    /// Reveals an element’s toast.
    /// </summary>
    public ValueTask ShowAsync() => JsRuntime.InvokeVoidAsync("window.blazorBootstrap.toasts.show", Id, AutoHide, Delay, objRef);

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


    /// <summary>
    /// Parameters are loaded manually for sake of performance.
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/performance#implement-setparametersasync-manually"/>
    /// </summary> 
    public override Task SetParametersAsync(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(AutoHide): AutoHide = (bool)parameter.Value; break;
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(Delay): Delay = (int)parameter.Value; break;
                case nameof(Hidden): Hidden = (EventCallback<ToastEventArgs>)parameter.Value; break;
                case nameof(Hiding): Hiding = (EventCallback<ToastEventArgs>)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value!; break;
                case nameof(ShowCloseButton): ShowCloseButton = (bool)parameter.Value; break;
                case nameof(Showing): Showing = (EventCallback<ToastEventArgs>)parameter.Value; break;
                case nameof(Shown): Shown = (EventCallback<ToastEventArgs>)parameter.Value; break;
                case nameof(Style): Style = (string)parameter.Value; break;
                case nameof(ToastMessage): ToastMessage = (ToastMessage)parameter.Value; break;

                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Toast, true),
            (BackgroundColor.White.ToBackgroundClass(), true));

    /// <summary>
    /// Gets or sets the auto hide state.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
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

    private string IconClass => $"{GetIconClass()} me-2".Trim();

    private IconName IconName => GetToastIconName();

    private ProgressColor ProgressColor => GetProgressColor();

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
    public ToastMessage ToastMessage { get; set; } = default!;

    #endregion
}
