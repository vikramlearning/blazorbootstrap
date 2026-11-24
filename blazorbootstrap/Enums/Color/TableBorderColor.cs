namespace BlazorBootstrap;

/// <summary>
/// Defines the border color of a bordered <see cref="Table{TItem}" /> (Bootstrap <c>border-*</c> utilities).
/// </summary>
public enum TableBorderColor
{
    /// <summary>
    /// Indicates no border color class: the table uses Bootstrap's default border color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates no border color class: the table uses Bootstrap's default border color.")]
    None,

    /// <summary>
    /// Indicates the primary border color (<c>border-primary</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the primary border color (border-primary).")]
    Primary,

    /// <summary>
    /// Indicates the primary subtle border color (<c>border-primary-subtle</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the primary subtle border color (border-primary-subtle).")]
    PrimarySubtle,

    /// <summary>
    /// Indicates the secondary border color (<c>border-secondary</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the secondary border color (border-secondary).")]
    Secondary,

    /// <summary>
    /// Indicates the secondary subtle border color (<c>border-secondary-subtle</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the secondary subtle border color (border-secondary-subtle).")]
    SecondarySubtle,

    /// <summary>
    /// Indicates the success border color (<c>border-success</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the success border color (border-success).")]
    Success,

    /// <summary>
    /// Indicates the success subtle border color (<c>border-success-subtle</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the success subtle border color (border-success-subtle).")]
    SuccessSubtle,

    /// <summary>
    /// Indicates the danger border color (<c>border-danger</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the danger border color (border-danger).")]
    Danger,

    /// <summary>
    /// Indicates the danger subtle border color (<c>border-danger-subtle</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the danger subtle border color (border-danger-subtle).")]
    DangerSubtle,

    /// <summary>
    /// Indicates the warning border color (<c>border-warning</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the warning border color (border-warning).")]
    Warning,

    /// <summary>
    /// Indicates the warning subtle border color (<c>border-warning-subtle</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the warning subtle border color (border-warning-subtle).")]
    WarningSubtle,

    /// <summary>
    /// Indicates the info border color (<c>border-info</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the info border color (border-info).")]
    Info,

    /// <summary>
    /// Indicates the info subtle border color (<c>border-info-subtle</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the info subtle border color (border-info-subtle).")]
    InfoSubtle,

    /// <summary>
    /// Indicates the light border color (<c>border-light</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the light border color (border-light).")]
    Light,

    /// <summary>
    /// Indicates the light subtle border color (<c>border-light-subtle</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the light subtle border color (border-light-subtle).")]
    LightSubtle,

    /// <summary>
    /// Indicates the dark border color (<c>border-dark</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the dark border color (border-dark).")]
    Dark,

    /// <summary>
    /// Indicates the dark subtle border color (<c>border-dark-subtle</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the dark subtle border color (border-dark-subtle).")]
    DarkSubtle,

    /// <summary>
    /// Indicates the black border color (<c>border-black</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the black border color (border-black).")]
    Black,

    /// <summary>
    /// Indicates the white border color (<c>border-white</c>).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the white border color (border-white).")]
    White
}
