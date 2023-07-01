namespace BlazorBootstrap;

public partial class Toasts : BaseComponent, IDisposable
{
    #region Members

    private ToastsPlacement placement = ToastsPlacement.TopRight;

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.ToastContainer());
        builder.Append(BootstrapClassProvider.PositionFixed());
        builder.Append(BootstrapClassProvider.ToToastsPlacement(Placement));

        base.BuildClasses(builder);
    }

    protected override void OnInitialized()
    {
        if (ToastService is not null)
            ToastService.OnNotify += OnNotify;

        base.OnInitialized();
    }

    private async Task OnToastShownAsync(ToastEventArgs args)
    {
        if (Messages is null || !Messages.Any())
            return;

        Messages.ForEach(x =>
        {
            if (x.Id == args.ToastId)
                x.SetElementId(args.ElementId);
        });

        if (Messages.Count >= StackLength)
        {
            var deleteMessages = Messages.GetRange(0, Messages.Count - StackLength);

            foreach (var message in deleteMessages)
            {
                if (string.IsNullOrWhiteSpace(message.ElementId))
                    await JS.InvokeVoidAsync("window.blazorBootstrap.toasts.hide", message.ElementId);
            }
        }
    }

    private void OnToastHiddenAsync(ToastEventArgs args)
    {
        if (Messages is null || !Messages.Any())
            return;

        var message = Messages.FirstOrDefault(x => x.Id == args.ToastId);

        if (message is not null)
            Messages.Remove(message);
    }

    private void OnNotify(ToastMessage toastMessage)
    {
        if (toastMessage is null)
            return;

        if (Messages is null)
            Messages = new();

        Messages.Add(toastMessage);

        StateHasChanged();
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

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [Inject] public ToastService ToastService { get; set; } = default!;

    /// <summary>
    /// List of all the toasts.
    /// </summary>
    [Parameter] public List<ToastMessage> Messages { get; set; } = default!;

    /// <summary>
    /// Auto hide the toast. Default is false.
    /// </summary>
    [Parameter] public bool AutoHide { get; set; }

    /// <summary>
    /// Show the close button.
    /// </summary>
    [Parameter] public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// Delay hiding the toast (ms). Default is 5000 milli seconds.
    /// </summary>
    [Parameter] public int Delay { get; set; } = 5000;

    /// <summary>
    /// Specifies the toast container maximum capacity. Default is 5.
    /// </summary>
    [Parameter] public int StackLength { get; set; } = 5;

    /// <summary>
    /// Specifies the toasts placement. Default is top right.
    /// </summary>
    [Parameter]
    public ToastsPlacement Placement
    {
        get => placement; set
        {
            placement = value;
            DirtyClasses();
        }
    }

    #endregion Properties
}
