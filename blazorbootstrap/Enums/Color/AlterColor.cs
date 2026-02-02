namespace BlazorBootstrap;

public enum AlertColor
{
    /// <summary>
    /// Represents the absence of a color variant.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("No color variant.")]
    None,

    /// <summary>
    /// Represents the primary color variant.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Primary color variant.")]
    Primary,

    /// <summary>
    /// Represents the secondary color variant.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Secondary color variant.")]
    Secondary,

    /// <summary>
    /// Represents the success color variant, typically used to indicate successful operations or positive outcomes.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Success color variant.")]
    Success,

    /// <summary>
    /// Represents the danger color variant, typically used to indicate errors or critical actions.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Danger color variant.")]
    Danger,

    /// <summary>
    /// Represents the warning color variant, typically used to indicate caution or potential issues in user interface
    /// elements.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Warning color variant.")]
    Warning,

    /// <summary>
    /// Represents the informational color variant, typically used to indicate neutral or informational messages.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Info color variant.")]
    Info,

    /// <summary>
    /// Represents the light color variant.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Light color variant.")]
    Light,

    /// <summary>
    /// Represents the dark color variant.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Dark color variant.")]
    Dark
}