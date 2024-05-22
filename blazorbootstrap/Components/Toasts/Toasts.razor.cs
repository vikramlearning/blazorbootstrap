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
                ToastService.OnNotify -= OnNotify;
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override void OnInitialized()
    {
        if (ToastService is not null)
            ToastService.OnNotify += OnNotify;

        base.OnInitialized();
    }

    private void OnNotify(ToastMessage toastMessage)
    {
        if (toastMessage is null)
            return;

        Messages ??= new List<ToastMessage>();

        Messages.Add(toastMessage);

        StateHasChanged();
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
                    Messages.Remove(message);

                if (string.IsNullOrWhiteSpace(message!.ElementId))
                    await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.toasts.hide", message.ElementId);
            }
        }
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.ToastContainer)
            .AddClass(BootstrapClass.PositionFixed)
            .AddClass(Placement.ToToastsPlacementClass())
            .Build();

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
    /// Gets or sets the toast messages.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public List<ToastMessage>? Messages { get; set; } = default!;

    /// <summary>
    /// Gets or sets the toast placement.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ToastsPlacement.TopRight" />.
    /// </remarks>
    [Parameter]
    public ToastsPlacement Placement { get; set; } = ToastsPlacement.TopRight;

    /// <summary>
    /// If true, shows the close button.
    /// </summary>
    /// <remarks>
    /// Default value is true.
    /// </remarks>
    [Parameter]
    public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// Gets or sets the toast container maximum capacity.
    /// </summary>
    /// <remarks>
    /// Default value is 5.
    /// </remarks>
    [Parameter]
    public int StackLength { get; set; } = 5;

    [Inject] public ToastService ToastService { get; set; } = default!;

    #endregion
}
