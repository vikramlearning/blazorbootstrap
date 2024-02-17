namespace BlazorBootstrap;

public partial class Toasts : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private ToastsPlacement placement = ToastsPlacement.TopRight;

    #endregion

    #region Methods

    protected override void BuildClasses()
    {
        this.AddClass(BootstrapClassProvider.ToastContainer);
        this.AddClass(BootstrapClassProvider.PositionFixed);
        this.AddClass(BootstrapClassProvider.ToToastsPlacement(Placement));

        base.BuildClasses();
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            Messages = null;

            if (ToastService is not null)
                ToastService.OnNotify -= OnNotify;
        }

        await base.DisposeAsync(disposing);
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
                if (string.IsNullOrWhiteSpace(message.ElementId))
                    await JS.InvokeVoidAsync("window.blazorBootstrap.toasts.hide", message.ElementId);
        }
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Auto hide the toast. Default is false.
    /// </summary>
    [Parameter]
    public bool AutoHide { get; set; }

    /// <summary>
    /// Delay hiding the toast (ms). Default is 5000 milli seconds.
    /// </summary>
    [Parameter]
    public int Delay { get; set; } = 5000;

    /// <summary>
    /// List of all the toasts.
    /// </summary>
    [Parameter]
    public List<ToastMessage>? Messages { get; set; } = default!;

    /// <summary>
    /// Specifies the toasts placement. Default is top right.
    /// </summary>
    [Parameter]
    public ToastsPlacement Placement
    {
        get => placement;
        set
        {
            placement = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Show the close button.
    /// </summary>
    [Parameter]
    public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// Specifies the toast container maximum capacity. Default is 5.
    /// </summary>
    [Parameter]
    public int StackLength { get; set; } = 5;

    [Inject] public ToastService ToastService { get; set; } = default!;

    #endregion
}
