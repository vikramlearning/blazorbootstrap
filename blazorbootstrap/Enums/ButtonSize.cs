namespace BlazorBootstrap;

/// <summary>
/// Defines the supported size options for a BlazorBootstrap button.
/// </summary>
public enum ButtonSize
{
    /// <summary>
    /// Indicates that the default size is used.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates that the default size is used.")]
    None,

    /// <summary>
    /// Indicates an extra-small size option.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates an extra-small size option.")]
    ExtraSmall,

    /// <summary>
    /// Indicates a small size option.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates a small size option.")]
    Small,

    /// <summary>
    /// Indicates a medium size option.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates a medium size option.")]
    Medium,

    /// <summary>
    /// Indicates a large size option.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates a large size option.")]
    Large,

    /// <summary>
    /// Indicates an extra-large size option.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates an extra-large size option.")]
    ExtraLarge
}
