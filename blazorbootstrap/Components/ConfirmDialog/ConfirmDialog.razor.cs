namespace BlazorBootstrap;

public partial class ConfirmDialog : BaseComponent
{
    #region Members

    private bool isVisible;

    private string? title;
    private string? message1;
    private string? message2;

    private Type? childComponent;
    private Dictionary<string, object>? parameters;

    private string? dialogCssClass;
    private bool dismissable;
    private string? headerCssClass;
    private string? scrollable;
    private string? verticallyCentered;
    private string? modalSize;
    private string? noButtonColor;
    private string? noButtonText;
    private string? yesButtonColor;
    private string? yesButtonText;

    private bool showBackdrop;

    private TaskCompletionSource<bool>? taskCompletionSource;

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(ClassProvider.Modal());
        builder.Append(ClassProvider.ConfirmationModal());
        builder.Append(ClassProvider.ModalFade());
        builder.Append(ClassProvider.Show(), showBackdrop);

        base.BuildClasses(builder);
    }

    protected override void BuildStyles(StyleBuilder builder)
    {
        builder.Append("display:block", showBackdrop);
        builder.Append("display:none", !showBackdrop);

        base.BuildStyles(builder);
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
        modalSize = ClassProvider.ToDialogSize(confirmDialogOptions.Size);
        yesButtonColor = confirmDialogOptions.YesButtonColor.ToButtonClass();
        yesButtonText = confirmDialogOptions.YesButtonText;

        isVisible = true;
        showBackdrop = true;

        DirtyClasses();
        DirtyStyles();

        Task.Run(() => JS.InvokeVoidAsync("window.blazorBootstrap.confirmDialog.show"));

        StateHasChanged();

        return task;
    }

    /// <summary>
    /// Shows confirm dialog.
    /// </summary>
    /// <param name="title">title for the confirm dialog</param>
    /// <param name="message1">message1 for the confirmation dialog.</param>
    /// <param name="confirmDialogOptions">options for the confirmation dialog.</param>
    /// <returns>bool</returns>
    public Task<bool> ShowAsync(string title, string message1, ConfirmDialogOptions? confirmDialogOptions = null) => Show(title: title, message1: message1, message2: null, type: null, parameters: null, confirmDialogOptions: confirmDialogOptions);

    /// <summary>
    /// Shows confirm dialog.
    /// </summary>
    /// <param name="title">title for the confirm dialog</param>
    /// <param name="message1">message1 for the confirmation dialog.</param>
    /// <param name="message2">message2 for the confirmation dialog. This is optional.</param>
    /// <param name="confirmDialogOptions">options for the confirmation dialog.</param>
    /// <returns>bool</returns>
    public Task<bool> ShowAsync(string title, string message1, string message2, ConfirmDialogOptions? confirmDialogOptions = null) => Show(title: title, message1: message1, message2: message2, type: null, parameters: null, confirmDialogOptions: confirmDialogOptions);

    /// <summary>
    /// Shows confirm dialog.
    /// </summary>
    /// <typeparam name="T">Component</typeparam>
    /// <param name="title"></param>
    /// <param name="parameters"></param>
    /// <param name="confirmDialogOptions"></param>
    /// <returns>bool</returns>
    public Task<bool> ShowAsync<T>(string title, Dictionary<string, object>? parameters = null, ConfirmDialogOptions? confirmDialogOptions = null) where T : ComponentBase => Show(title: title, message1: null, message2: null, type: typeof(T), parameters: parameters, confirmDialogOptions: confirmDialogOptions);

    /// <summary>
    /// Hides confirm dialog.
    /// </summary>
    private void Hide()
    {
        isVisible = false;
        showBackdrop = false;

        DirtyClasses();
        DirtyStyles();

        Task.Run(() => JS.InvokeVoidAsync("window.blazorBootstrap.confirmDialog.hide"));

        StateHasChanged();
    }

    private void OnYesClick()
    {
        Hide();
        taskCompletionSource.SetResult(true);
    }

    private void OnNoClick()
    {
        Hide();
        taskCompletionSource.SetResult(false);
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    #endregion Properties
}
