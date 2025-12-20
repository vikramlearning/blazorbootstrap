namespace BlazorBootstrap;

/// <summary>
/// Specifies the available color variants for the `Badge` component.
/// </summary>
public enum BadgeColor
{
    /// <summary>
    /// No color styling is applied.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("No color styling is applied.")]
    None,

    /// <summary>
    /// Applies the primary theme color.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Applies the primary theme color.")]
    Primary,

    /// <summary>
    /// Applies the secondary theme color.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Applies the secondary theme color.")]
    Secondary,

    /// <summary>
    /// Applies the success theme color.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Applies the success theme color.")]
    Success,

    /// <summary>
    /// Applies the danger theme color.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Applies the danger theme color.")]
    Danger,

    /// <summary>
    /// Applies the warning theme color.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Applies the warning theme color.")]
    Warning,

    /// <summary>
    /// Applies the info theme color.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Applies the info theme color.")]
    Info,

    /// <summary>
    /// Applies the light theme color.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Applies the light theme color.")]
    Light,

    /// <summary>
    /// Applies the dark theme color.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Applies the dark theme color.")]
    Dark
}
