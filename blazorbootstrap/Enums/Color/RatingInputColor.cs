namespace BlazorBootstrap;

/// <summary>
/// Defines the supported icon colors for a <see cref="RatingInput{TValue}" />.
/// </summary>
public enum RatingInputColor
{
    /// <summary>
    /// Indicates no color is applied. Default color is inherited from parent element.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates no color is applied. Default color is inherited from parent element.")]
    None,

    /// <summary>
    /// Indicates the rating icons should use the primary color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the rating icons should use the primary color.")]
    Primary,

    /// <summary>
    /// Indicates the rating icons should use the secondary color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the rating icons should use the secondary color.")]
    Secondary,

    /// <summary>
    /// Indicates the rating icons should use the success color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the rating icons should use the success color.")]
    Success,

    /// <summary>
    /// Indicates the rating icons should use the danger color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the rating icons should use the danger color.")]
    Danger,

    /// <summary>
    /// Indicates the rating icons should use the warning color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the rating icons should use the warning color.")]
    Warning,

    /// <summary>
    /// Indicates the rating icons should use the info color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the rating icons should use the info color.")]
    Info,

    /// <summary>
    /// Indicates the rating icons should use the light color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the rating icons should use the light color.")]
    Light,

    /// <summary>
    /// Indicates the rating icons should use the dark color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the rating icons should use the dark color.")]
    Dark,

    /// <summary>
    /// Indicates the rating icons should use the body text color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the rating icons should use the body text color.")]
    Body,

    /// <summary>
    /// Indicates the rating icons should use the muted text color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the rating icons should use the muted text color.")]
    Muted,

    /// <summary>
    /// Indicates the rating icons should use the white color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the rating icons should use the white color.")]
    White
}
