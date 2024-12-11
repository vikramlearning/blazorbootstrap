﻿namespace BlazorBootstrap;

public class ModalOption
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the footer button color.
    /// </summary>
    public ButtonColor FooterButtonColor { get; set; } = ButtonColor.Secondary;

    /// <summary>
    /// Gets or sets the footer button custom CSS class.
    /// </summary>
    public string FooterButtonCssClass { get; set; } = default!;

    /// <summary>
    /// Gets or sets the footer button text. Default text is `OK`.
    /// </summary>
    public string FooterButtonText { get; set; } = "OK";

    /// <summary>
    /// Gets or sets the IsVerticallyCentered.
    /// </summary>
    public bool IsVerticallyCentered { get; set; }

    /// <summary>
    /// Gets or sets the modal message.
    /// </summary>
    public string Message { get; set; } = default!;

    /// <summary>
    /// Shows or hides the footer button. Default value is `true`.
    /// </summary>
    public bool ShowFooterButton { get; set; } = true;

    /// <summary>
    /// Gets or sets the modal size.
    /// </summary>
    public ModalSize Size { get; set; }

    /// <summary>
    /// Gets or sets the modal title.
    /// </summary>
    public string Title { get; set; } = default!;

    /// <summary>
    /// Gets or sets the modal type. Default value is `ModalType.None`.
    /// </summary>
    public ModalType Type { get; set; } = ModalType.None;

    #endregion
}
