namespace BlazorBootstrap;

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

    protected override void BuildClasses()
    {
        this.AddClass(BootstrapClassProvider.Modal);
        this.AddClass(BootstrapClassProvider.ConfirmationModal);
        this.AddClass(BootstrapClassProvider.ModalFade);

        base.BuildClasses();
    }

    protected override void BuildStyles()
    {
        this.AddStyle("display:block", showBackdrop);
        this.AddStyle("display:none", !showBackdrop);

        base.BuildStyles();
    }

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

        DirtyClasses();
        DirtyStyles();

        StateHasChanged();

        Task.Run(() => JS.InvokeVoidAsync("window.blazorBootstrap.confirmDialog.hide", ElementId));
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
        noButtonColor = confirmDialogOptions.NoButtonColor.ToButtonClass();
        noButtonText = confirmDialogOptions.NoButtonText;
        modalSize = BootstrapClassProvider.ToDialogSize(confirmDialogOptions.Size);
        yesButtonColor = confirmDialogOptions.YesButtonColor.ToButtonClass();
        yesButtonText = confirmDialogOptions.YesButtonText;

        isVisible = true;
        showBackdrop = true;

        DirtyClasses();
        DirtyStyles();

        StateHasChanged();

        Task.Run(() => JS.InvokeVoidAsync("window.blazorBootstrap.confirmDialog.show", ElementId));

        return task;
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    #endregion
}
