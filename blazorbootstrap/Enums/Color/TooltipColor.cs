namespace BlazorBootstrap;

/// <summary>
/// Defines the supported visual color options for a BlazorBootstrap tooltip.
/// </summary>
public enum TooltipColor
{
    /// <summary>
    /// Indicates that no explicit color option is selected.
    /// </summary>
    [AddedVersion("1.10.0")]
    [Description("Indicates that no explicit color option is selected.")]
    None,

    /// <summary>
    /// Indicates the primary (emphasis) color option.
    /// </summary>
    [AddedVersion("1.10.0")]
    [Description("Indicates the primary (emphasis) color option.")]
    Primary,

    /// <summary>
    /// Indicates the secondary (less emphasized) color option.
    /// </summary>
    [AddedVersion("1.10.0")]
    [Description("Indicates the secondary (less emphasized) color option.")]
    Secondary,

    /// <summary>
    /// Indicates a success-state color option.
    /// </summary>
    [AddedVersion("1.10.0")]
    [Description("Indicates a success-state color option.")]
    Success,

    /// <summary>
    /// Indicates an error or danger-state color option.
    /// </summary>
    [AddedVersion("1.10.0")]
    [Description("Indicates an error or danger-state color option.")]
    Danger,

    /// <summary>
    /// Indicates a warning-state color option.
    /// </summary>
    [AddedVersion("1.10.0")]
    [Description("Indicates a warning-state color option.")]
    Warning,

    /// <summary>
    /// Indicates an informational-state color option.
    /// </summary>
    [AddedVersion("1.10.0")]
    [Description("Indicates an informational-state color option.")]
    Info,

    /// <summary>
    /// Indicates a light-toned color option.
    /// </summary>
    [AddedVersion("1.10.0")]
    [Description("Indicates a light-toned color option.")]
    Light,

    /// <summary>
    /// Indicates a dark-toned color option.
    /// </summary>
    [AddedVersion("1.10.0")]
    [Description("Indicates a dark-toned color option.")]
    Dark
}
