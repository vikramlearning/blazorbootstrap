namespace BlazorBootstrap;

public enum BackgroundColor
{
    None,
    Primary,
    Secondary,
    Success,
    Danger,
    Warning,
    Info,
    Light,
    Dark,
    Body,
    White,
    Transparent,

    /// <summary>
    /// Indicates the secondary body background color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the secondary body background color.")]
    BodySecondary,

    /// <summary>
    /// Indicates the tertiary body background color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the tertiary body background color.")]
    BodyTertiary

    // TODO:Review
    // https://getbootstrap.com/docs/5.1/utilities/background/#background-gradient
    // https://getbootstrap.com/docs/5.1/utilities/background/#opacity
}
