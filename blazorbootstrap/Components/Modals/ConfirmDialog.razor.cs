using BlazorBootstrap.Extensions;

namespace BlazorBootstrap;

public partial class ConfirmDialog : BaseComponent
{
    #region Members

    private string title;
    private string message1;
    private string message2;

    private BackgroundColor _titleBackgroundColor = BackgroundColor.None;
    private ButtonColor _yesButtonColor = ButtonColor.Primary;
    private ButtonColor _noButtonColor = ButtonColor.Secondary;

    private string scrollable => IsScrollable ? "modal-dialog-scrollable" : "";
    private string titleBackgroundColor => TitleBackgroundColor.ToBackgroundAndTextClass();
    private string verticallyCentered => IsVerticallyCentered ? "modal-dialog-centered" : "";
    private string yesButtonColor => YesButtonColor.ToButtonClass();
    private string noButtonColor => NoButtonColor.ToButtonClass();

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

    protected override void OnInitialized()
    {
        this.title = Title;
        this.message1 = Message1;
        this.message2 = Message2;

        base.OnInitialized();
    }

    /// <summary>
    /// Shows confirm dialog.
    /// </summary>
    public void Show()
    {
        showBackdrop = true;

        this.DirtyClasses();
        this.DirtyStyles();

        StateHasChanged();
    }

    /// <summary>
    /// Shows confirm dialog.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message1"></param>
    /// <param name="message2"></param>
    public Task<bool> Show(string title, string message1, string message2)
    {
        taskCompletionSource = new TaskCompletionSource<bool>();
        Task<bool> task = taskCompletionSource.Task;

        this.title = title;
        this.message1 = message1;
        this.message2 = message2;

        this.Show();

        return task;
    }

    /// <summary>
    /// Hides confirm dialog.
    /// </summary>
    public void Hide()
    {
        showBackdrop = false;

        this.DirtyClasses();
        this.DirtyStyles();

        StateHasChanged();
    }

    private void Yes()
    {
        Hide();
        taskCompletionSource.SetResult(true);
    }

    private void No()
    {
        Hide();
        taskCompletionSource.SetResult(false);
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Allows confirm dialog body to be scrollable.
    /// </summary>
    [Parameter] public bool IsScrollable { get; set; }

    /// <summary>
    /// Shows the confirm dialog vertically in the center of the page.
    /// </summary>
    [Parameter] public bool IsVerticallyCentered { get; set; }

    /// <summary>
    /// Gets or sets the title of the confirm dialog.
    /// </summary>
    [Parameter] public string Title { get; set; }

    /// <summary>
    /// Gets or sets the background color of the confirm dialog title. <see cref="BackgroundColor"/>
    /// </summary>
    [Parameter]
    public BackgroundColor TitleBackgroundColor
    {
        get => _titleBackgroundColor; 
        set
        {
            _titleBackgroundColor = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Adds a dismissable close button to the confirm dialog.
    /// </summary>
    [Parameter] public bool Dismissable { get; set; } = true;

    /// <summary>
    /// Gets or sets the Message1 of the confirmation dialog.
    /// </summary>
    [Parameter] public string Message1 { get; set; }

    /// <summary>
    /// Gets or sets the Message2 of the confirmation dialog. This is optional.
    /// </summary>
    [Parameter] public string Message2 { get; set; }

    /// <summary>
    /// Gets or sets the 'Yes' button text.
    /// </summary>
    [Parameter] public string YesButtonText { get; set; } = "Yes";

    /// <summary>
    /// Gets or sets the 'Yes' button color. <see cref="ButtonColor"/>
    /// </summary>
    [Parameter]
    public ButtonColor YesButtonColor
    {
        get => _yesButtonColor; 
        set
        {
            _yesButtonColor = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Gets or sets the 'No' button text.
    /// </summary>
    [Parameter] public string NoButtonText { get; set; } = "No";

    /// <summary>
    /// Gets or sets the 'No' button color. <see cref="ButtonColor"/>
    /// </summary>
    [Parameter]
    public ButtonColor NoButtonColor
    {
        get => _noButtonColor; 
        set
        {
            _noButtonColor = value;
            DirtyClasses();
        }
    }

    #endregion Properties
}
