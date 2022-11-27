using BlazorBootstrap.Extensions;

namespace BlazorBootstrap;

public partial class ConfirmDialog : BaseComponent
{
    #region Members

    private bool isVisible;

    private string title;
    private string message1;
    private string message2;

    private Type? childComponent;
    private Dictionary<string, object> parametres;

    private string dialogCssClass;
    private bool dismissable;
    private string headerCssClass;
    private string scrollable;
    private string verticallyCentered;
    private string modalSize;
    private string noButtonColor;
    private string noButtonText;
    private string yesButtonColor;
    private string yesButtonText;

    private bool showBackdrop;

    private TaskCompletionSource<bool> taskCompletionSource;

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Modal());
        builder.Append(BootstrapClassProvider.ConfirmationModal());
        builder.Append(BootstrapClassProvider.ModalFade());
        builder.Append(BootstrapClassProvider.Show(), showBackdrop);

        base.BuildClasses(builder);
    }

    protected override void BuildStyles(StyleBuilder builder)
    {
        builder.Append("display:block", showBackdrop);
        builder.Append("display:none", !showBackdrop);

        base.BuildStyles(builder);
    }

    private Task<bool> Show(string title, string message1, string message2, Type? type, Dictionary<string, object> parametres, ConfirmDialogOptions confirmDialogOptions)
    {
        taskCompletionSource = new TaskCompletionSource<bool>();
        Task<bool> task = taskCompletionSource.Task;

        this.title = title;
        this.message1 = message1;
        this.message2 = message2;

        this.childComponent = type;
        this.parametres= parametres;

        if (confirmDialogOptions is null)
            confirmDialogOptions = new ConfirmDialogOptions();

        this.dialogCssClass = confirmDialogOptions.DialogCssClass;
        this.dismissable = confirmDialogOptions.Dismissable;
        this.headerCssClass = confirmDialogOptions.HeaderCssClass;
        this.scrollable = confirmDialogOptions.IsScrollable ? "modal-dialog-scrollable" : "";
        this.verticallyCentered = confirmDialogOptions.IsVerticallyCentered ? "modal-dialog-centered" : "";
        this.noButtonColor = confirmDialogOptions.NoButtonColor.ToButtonClass();
        this.noButtonText = confirmDialogOptions.NoButtonText;
        this.modalSize = BootstrapClassProvider.ToDialogSize(confirmDialogOptions.Size);
        this.yesButtonColor = confirmDialogOptions.YesButtonColor.ToButtonClass();
        this.yesButtonText = confirmDialogOptions.YesButtonText;

        this.isVisible = true;
        this.showBackdrop = true;

        this.DirtyClasses();
        this.DirtyStyles();

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
    public Task<bool> ShowAsync(string title, string message1, ConfirmDialogOptions confirmDialogOptions = null)
    {        
        return Show(title: title, message1: message1, message2: null, type: null, parametres: null, confirmDialogOptions: confirmDialogOptions);
    }

    /// <summary>
    /// Shows confirm dialog.
    /// </summary>
    /// <param name="title">title for the confirm dialog</param>
    /// <param name="message1">message1 for the confirmation dialog.</param>
    /// <param name="message2">message2 for the confirmation dialog. This is optional.</param>
    /// <param name="confirmDialogOptions">options for the confirmation dialog.</param>
    /// <returns>bool</returns>
    public Task<bool> ShowAsync(string title, string message1, string message2, ConfirmDialogOptions confirmDialogOptions = null)
    {
        return Show(title: title, message1: message1, message2: message2, type: null, parametres: null, confirmDialogOptions: confirmDialogOptions) ;
    }

    /// <summary>
    /// Shows confirm dialog.
    /// </summary>
    /// <typeparam name="T">Component</typeparam>
    /// <param name="title"></param>
    /// <param name="parametres"></param>
    /// <param name="confirmDialogOptions"></param>
    /// <returns>bool</returns>
    public Task<bool> ShowAsync<T>(string title, Dictionary<string, object> parametres = null, ConfirmDialogOptions confirmDialogOptions = null) where T : ComponentBase
    {
        return Show(title: title, message1: null, message2: null, type: typeof(T), parametres: parametres, confirmDialogOptions: confirmDialogOptions);
    }

    /// <summary>
    /// Hides confirm dialog.
    /// </summary>
    private void Hide()
    {
        this.isVisible= false;
        this.showBackdrop = false;

        this.DirtyClasses();
        this.DirtyStyles();

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
