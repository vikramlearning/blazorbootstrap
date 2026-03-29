namespace BlazorBootstrap;

public class ConfirmDialogOptions
{
    #region Properties, Indexers

    /// <summary>
    /// Determines whether to focus on the yes button or not.
    /// </summary>
    [AddedVersion("1.1.0")]
    [DefaultValue(true)]
    [Description("Determines whether to focus on the yes button or not.")]
    public bool AutoFocusYesButton {  get; set; } = true;

    /// <summary>
    /// Additional CSS class for the dialog (div.modal-dialog element).
    /// </summary>
    [AddedVersion("1.1.0")]
    [DefaultValue(null)]
    [Description("Additional CSS class for the dialog (div.modal-dialog element).")]
    [ParameterTypeName("string?")]
    public string? DialogCssClass { get; set; }

    /// <summary>
    /// Adds a dismissable close button to the confirm dialog.
    /// </summary>
    [AddedVersion("1.1.0")]
    [DefaultValue(true)]
    [Description("Adds a dismissable close button to the confirm dialog.")]
    public bool Dismissable { get; set; } = true;

    /// <summary>
    /// Additional header CSS class (div.modal-header element).
    /// </summary>
    [AddedVersion("1.1.0")]
    [DefaultValue(null)]
    [Description("Additional header CSS class (div.modal-header element).")]
    [ParameterTypeName("string?")]
    public string? HeaderCssClass { get; set; }

    /// <summary>
    /// Allows confirm dialog body to be scrollable.
    /// </summary>
    [AddedVersion("1.1.0")]
    [DefaultValue(false)]
    [Description("Allows confirm dialog body to be scrollable.")]
    public bool IsScrollable { get; set; }

    /// <summary>
    /// Shows the confirm dialog vertically in the center of the page.
    /// </summary>
    [AddedVersion("1.1.0")]
    [DefaultValue(false)]
    [Description("Shows the confirm dialog vertically in the center of the page.")]
    public bool IsVerticallyCentered { get; set; }

    /// <summary>
    /// Gets or sets the 'No' button color.
    /// <para>
    /// Default value is <see cref="ButtonColor.Secondary" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.1.0")]
    [DefaultValue(ButtonColor.Secondary)]
    [Description("Gets or sets the 'No' button color.")]
    public ButtonColor NoButtonColor { get; set; } = ButtonColor.Secondary;

    /// <summary>
    /// Gets or sets the 'No' button text.
    /// </summary>
    [AddedVersion("1.1.0")]
    [DefaultValue("No")]
    [Description("Gets or sets the 'No' button text.")]
    public string NoButtonText { get; set; } = "No";

    /// <summary>
    /// Gets or sets the size of the confirm dialog.
    /// <para>
    /// Default value is <see cref="DialogSize.Regular" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.1.0")]
    [DefaultValue(ModalSize.Regular)]
    [Description("Gets or sets the size of the modal dialog.")]
    public DialogSize Size { get; set; } = DialogSize.Regular;

    /// <summary>
    /// Gets or sets the 'Yes' button color.
    /// <para>
    /// Default value is <see cref="ButtonColor.Primary" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.1.0")]
    [DefaultValue(ButtonColor.Primary)]
    [Description("Gets or sets the 'Yes' button color.")]
    public ButtonColor YesButtonColor { get; set; } = ButtonColor.Primary;

    /// <summary>
    /// Gets or sets the 'Yes' button text.
    /// <para>
    /// Default value is "Yes".
    /// </para>
    /// </summary>
    [AddedVersion("1.1.0")]
    [DefaultValue("Yes")]
    [Description("Gets or sets the 'Yes' button text.")]
    public string YesButtonText { get; set; } = "Yes";

    #endregion
}
