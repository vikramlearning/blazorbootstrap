namespace BlazorBootstrap;

public class ConfirmDialogOptions
{
    #region Properties, Indexers

    /// <summary>
    /// Determines whether to focus on the yes button or not.
    /// </summary>
    public bool AutoFocusYesButton {  get; set; } = true;

    /// <summary>
    /// Additional CSS class for the dialog (div.modal-dialog element).
    /// </summary>
    public string? DialogCssClass { get; set; }

    /// <summary>
    /// Adds a dismissable close button to the confirm dialog.
    /// </summary>
    public bool Dismissable { get; set; } = true;

    /// <summary>
    /// Additional header CSS class (div.modal-header element).
    /// </summary>
    public string? HeaderCssClass { get; set; }

    /// <summary>
    /// Allows confirm dialog body to be scrollable.
    /// </summary>
    public bool IsScrollable { get; set; }

    /// <summary>
    /// Shows the confirm dialog vertically in the center of the page.
    /// </summary>
    public bool IsVerticallyCentered { get; set; }

    /// <summary>
    /// Gets or sets the 'No' button color. <see cref="ButtonColor" />
    /// </summary>
    public ButtonColor NoButtonColor { get; set; } = ButtonColor.Secondary;

    /// <summary>
    /// Gets or sets the 'No' button text.
    /// </summary>
    public string NoButtonText { get; set; } = "No";

    /// <summary>
    /// Size of the modal. Default is <see cref="ModalSize.Regular" />.
    /// </summary>
    public DialogSize Size { get; set; }

    /// <summary>
    /// Gets or sets the 'Yes' button color. <see cref="ButtonColor" />
    /// </summary>
    public ButtonColor YesButtonColor { get; set; } = ButtonColor.Primary;

    /// <summary>
    /// Gets or sets the 'Yes' button text.
    /// </summary>
    public string YesButtonText { get; set; } = "Yes";

    #endregion
}
