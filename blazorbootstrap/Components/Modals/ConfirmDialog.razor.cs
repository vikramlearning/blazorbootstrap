using BlazorBootstrap.Extensions;

namespace BlazorBootstrap;

public partial class ConfirmDialog : BaseComponent
{
    #region Members

    private bool isVisible;

    private string title;
    private string message1;
    private string message2;

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

    /// <summary>
    /// Shows confirm dialog.
    /// </summary>
    /// <param name="title">title for the confirm dialog</param>
    /// <param name="message1">message1 for the confirmation dialog.</param>
    /// <param name="message2">message2 for the confirmation dialog. This is optional.</param>
    /// <param name="confirmDialogOptions">options for the confirmation dialog.</param>
    /// <returns>bool</returns>
    public Task<bool> Show(string title, string message1, string message2, ConfirmDialogOptions confirmDialogOptions = null)
    {
        taskCompletionSource = new TaskCompletionSource<bool>();
        Task<bool> task = taskCompletionSource.Task;

        this.title = title;
        this.message1 = message1;
        this.message2 = message2;

        if (confirmDialogOptions is null)
            confirmDialogOptions = new ConfirmDialogOptions();

        this.dialogCssClass = confirmDialogOptions.DialogCssClass;
        this.dismissable = confirmDialogOptions.Dismissable;
        this.headerCssClass = confirmDialogOptions.HeaderCssClass;
        this.scrollable = confirmDialogOptions.IsScrollable ? "modal-dialog-scrollable" : "";
        this.verticallyCentered = confirmDialogOptions.IsVerticallyCentered ? "modal-dialog-centered" : "";
        this.noButtonColor = confirmDialogOptions.NoButtonColor.ToButtonClass();
        this.noButtonText = confirmDialogOptions.NoButtonText;
        this.modalSize= BootstrapClassProvider.ToModalSize(confirmDialogOptions.Size);
        this.yesButtonColor = confirmDialogOptions.YesButtonColor.ToButtonClass();
        this.yesButtonText = confirmDialogOptions.YesButtonText;

        this.isVisible = true;
        this.showBackdrop = true;

        this.DirtyClasses();
        this.DirtyStyles();

        StateHasChanged();

        return task;
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
