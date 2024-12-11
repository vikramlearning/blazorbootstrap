namespace BlazorBootstrap;

/// <summary>
/// Push notifications to your visitors with a toast, a lightweight and easily customizable alert message. <br/>
/// For more information, visit the <see href="https://getbootstrap.com/docs/5.0/components/toasts/">Bootstrap Toasts</see> documentation.
/// </summary>
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

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (ToastService is not null)
            ToastService.OnNotify += OnNotify;

        base.OnInitialized();
    }

    private void OnNotify(ToastMessage? toastMessage)
    {
        if (toastMessage is null)
            return;

        Messages ??= new List<ToastMessage>();

        Messages.Add(toastMessage);

        StateHasChanged();
    }

    private void OnToastHiddenAsync(ToastEventArgs args)
    {
        if (Messages is null || Messages.Count == 0)
            return;

        var message = Messages.FirstOrDefault(x => x.Id == args.ToastId);

        if (message is not null)
            Messages.Remove(message);
    }

    private async Task OnToastShownAsync(ToastEventArgs args)
    {
        if (Messages is null || Messages.Count == 0)
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
                    await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.toasts.hide", message.ElementId);
            }
        }
    }


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
                case nameof(Id): Id = (string)parameter.Value!; break;
                case nameof(Messages): Messages = (List<ToastMessage>)parameter.Value; break;
                case nameof(Placement): Placement = (ToastsPlacement)parameter.Value; break;
                case nameof(ShowCloseButton): ShowCloseButton = (bool)parameter.Value; break;
                case nameof(StackLength): StackLength = (int)parameter.Value; break;
 

                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers
     
    /// <summary>
    /// Gets or sets the auto hide state.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool AutoHide { get; set; }

    /// <summary>
    /// Gets or sets the delay in milliseconds before hiding the toast.
    /// </summary>
    /// <remarks>
    /// Default value is 5000.
    /// </remarks>
    [Parameter] public int Delay { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the toast messages.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public List<ToastMessage>? Messages { get; set; }

    /// <summary>
    /// Gets or sets the toast placement.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ToastsPlacement.TopRight" />.
    /// </remarks>
    [Parameter] public ToastsPlacement Placement { get; set; } = ToastsPlacement.TopRight;

    /// <summary>
    /// If <see langword="true" />, shows the close button.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [Parameter] public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// Gets or sets the toast container maximum capacity.
    /// </summary>
    /// <remarks>
    /// Default value is 5.
    /// </remarks>
    [Parameter] public int StackLength { get; set; } = 5;

    /// <summary>
    /// Dependency injected Toast Service
    /// </summary>
    [Inject] public ToastService ToastService { get; set; } = default!;

    #endregion
}
