namespace BlazorBootstrap;

public partial class Toasts : BlazorBootstrapComponentBase
{
    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            Messages = null;

            if (ToastService is not null)
                ToastService.OnNotify -= OnNotifyAsync;
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override void OnInitialized()
    {
        if (ToastService is not null)
            ToastService.OnNotify += OnNotifyAsync;

        base.OnInitialized();
    }

    private async Task OnNotifyAsync(ToastMessage toastMessage)
    {
        if (toastMessage is null)
            return;

        Messages ??= new List<ToastMessage>();

        Messages.Add(toastMessage);

        await InvokeAsync(StateHasChanged);
    }

    private void OnToastHiddenAsync(ToastEventArgs args)
    {
        if (Messages is null || !Messages.Any())
            return;

        var message = Messages.FirstOrDefault(x => x.Id == args.ToastId);

        if (message is not null)
            Messages.Remove(message);
    }

    private async Task OnToastShownAsync(ToastEventArgs args)
    {
        if (Messages is null || !Messages.Any())
            return;

        Messages.ForEach(
            x =>
            {
                if (x.Id == args.ToastId)
                    x.SetElementId(args.ElementId);
            }
        );

        if (Messages.Count >= StackLength)
        {
            var deleteMessages = Messages.GetRange(0, Messages.Count - StackLength);

            foreach (var message in deleteMessages)
            {
                if (message is not null)
                {
                    if (string.IsNullOrWhiteSpace(message.ElementId))
                        Messages.Remove(message);
                    else
                        // Keep rendered toasts in the DOM until Bootstrap completes the hide transition.
                        // Removing them first can dispose the element before toast.js raises hidden.
                        await SafeInvokeVoidAsync("window.blazorBootstrap.toasts.hide", message.ElementId);
                }
            }
        }
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.ToastContainer, true),
            (BootstrapClass.PositionFixed, true),
            (Placement.ToToastsPlacementClass(), true));

    /// <summary>
    /// Gets or sets the auto hide state.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the auto hide state.")]
    [Parameter]
    public bool AutoHide { get; set; }

    /// <summary>
    /// Gets or sets the delay in milliseconds before hiding the toast.
    /// <para>
    /// Default value is 5000.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(5000)]
    [Description("Gets or sets the delay in milliseconds before hiding the toast.")]
    [Parameter]
    public int Delay { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the toast messages.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the toast messages.")]
    [Parameter]
    public List<ToastMessage>? Messages { get; set; }

    /// <summary>
    /// Gets or sets the toast placement.
    /// <para>
    /// Default value is <see cref="ToastsPlacement.TopRight" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(ToastsPlacement.TopRight)]
    [Description("Gets or sets the toast placement.")]
    [Parameter]
    public ToastsPlacement Placement { get; set; } = ToastsPlacement.TopRight;

    /// <summary>
    /// If <see langword="true" />, shows the close button.
    /// <para>
    /// Default value is <see langword="true"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(true)]
    [Description("If <b>true</b>, shows the close button.")]
    [Parameter]
    public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// Gets or sets the toast container maximum capacity.
    /// <para>
    /// Default value is 5.
    /// </para>
    /// <para>
    /// When the limit is exceeded, the oldest rendered toasts are dismissed first.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(5)]
    [Description("Gets or sets the toast container maximum capacity. When the limit is exceeded, the oldest rendered toasts are dismissed first.")]
    [Parameter]
    public int StackLength { get; set; } = 5;

    [Inject] 
    public ToastService ToastService { get; set; } = default!;

    #endregion
}
