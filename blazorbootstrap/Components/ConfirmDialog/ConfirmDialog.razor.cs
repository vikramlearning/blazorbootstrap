namespace BlazorBootstrap;

/// <summary>
/// A modal dialog to display if you want the user to verify or accept something. 
/// </summary>
public partial class ConfirmDialog : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private Type? childComponent;

    private string? dialogCssClass;
    private bool dismissable;
    private string? headerCssClass;

    private bool isVisible;
    private string? message1;
    private string? message2;
    private string? modalSize;
    private string? noButtonColor;
    private string? noButtonText;
    private Dictionary<string, object>? parameters;
    private string? scrollable;

    private bool showBackdrop;

    private TaskCompletionSource<bool>? taskCompletionSource;

    private string? title;
    private string? verticallyCentered;
    private string? yesButtonColor;
    private string? yesButtonText;

    #endregion

    #region Methods

    /// <summary>
    /// Shows confirm dialog.
    /// </summary>
    /// <param name="title">title for the confirm dialog</param>
    /// <param name="message1">message1 for the confirmation dialog.</param>
    /// <param name="confirmDialogOptions">options for the confirmation dialog.</param>
    /// <returns>bool</returns>
    public Task<bool> ShowAsync(string title, string message1, ConfirmDialogOptions? confirmDialogOptions = null) => Show(title, message1, null, null, null, confirmDialogOptions!);

    /// <summary>
    /// Shows confirm dialog.
    /// </summary>
    /// <param name="title">title for the confirm dialog</param>
    /// <param name="message1">message1 for the confirmation dialog.</param>
    /// <param name="message2">message2 for the confirmation dialog. This is optional.</param>
    /// <param name="confirmDialogOptions">options for the confirmation dialog.</param>
    /// <returns>bool</returns>
    public Task<bool> ShowAsync(string title, string message1, string message2, ConfirmDialogOptions? confirmDialogOptions = null) => Show(title, message1, message2, null, null, confirmDialogOptions!);

    /// <summary>
    /// Shows confirm dialog.
    /// </summary>
    /// <typeparam name="T">Component</typeparam>
    /// <param name="title"></param>
    /// <param name="parameters"></param>
    /// <param name="confirmDialogOptions"></param>
    /// <returns>bool</returns>
    public Task<bool> ShowAsync<T>(string title, Dictionary<string, object>? parameters = null, ConfirmDialogOptions? confirmDialogOptions = null) where T : ComponentBase => Show(title, null, null, typeof(T), parameters, confirmDialogOptions!);

    /// <summary>
    /// Hides confirm dialog.
    /// </summary>
    private void Hide()
    {
        isVisible = false;
        showBackdrop = false;

        StateHasChanged();

        Task.Run(() => JsRuntime.InvokeVoidAsync("window.blazorBootstrap.confirmDialog.hide", Id));
    }

    private void OnNoClick()
    {
        Hide();
        taskCompletionSource?.SetResult(false);
    }

    private void OnYesClick()
    {
        Hide();
        taskCompletionSource?.SetResult(true);
    }

    private Task<bool> Show(string title, string? message1, string? message2, Type? type, Dictionary<string, object>? parameters, ConfirmDialogOptions confirmDialogOptions)
    {
        taskCompletionSource = new TaskCompletionSource<bool>();
        var task = taskCompletionSource.Task;

        this.title = title;
        this.message1 = message1;
        this.message2 = message2;

        childComponent = type;
        this.parameters = parameters;

        confirmDialogOptions ??= new ConfirmDialogOptions();

        dialogCssClass = confirmDialogOptions.DialogCssClass;
        dismissable = confirmDialogOptions.Dismissable;
        headerCssClass = confirmDialogOptions.HeaderCssClass;
        scrollable = confirmDialogOptions.IsScrollable ? "modal-dialog-scrollable" : "";
        verticallyCentered = confirmDialogOptions.IsVerticallyCentered ? "modal-dialog-centered" : "";
        noButtonColor = $"{BootstrapClass.Button} {confirmDialogOptions.NoButtonColor.ToButtonColorClass()}";
        noButtonText = confirmDialogOptions.NoButtonText;
        modalSize = confirmDialogOptions.Size.ToDialogSizeClass();
        yesButtonColor = $"{BootstrapClass.Button} {confirmDialogOptions.YesButtonColor.ToButtonColorClass()}";
        yesButtonText = confirmDialogOptions.YesButtonText;

        isVisible = true;
        showBackdrop = true;

        StateHasChanged();

        Task.Run(() => JsRuntime.InvokeVoidAsync("window.blazorBootstrap.confirmDialog.show", Id));

        return task;
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
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value; break;
                case nameof(Style): Style = (string)parameter.Value; break;
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
            (BootstrapClass.Modal, true),
            (BootstrapClass.ConfirmationModal, true),
            (BootstrapClass.ModalFade, true));

    /// <inheritdoc />
    protected override string? StyleNames =>
        BuildStyleNames(Style,
            ("display:block", showBackdrop),
            ("display:none", !showBackdrop));

    #endregion
}
