namespace BlazorBootstrap;

/// <summary>
/// Defines the contextual background color of a <see cref="Navbar" />, applied with Bootstrap's <c>text-bg-*</c> classes.
/// </summary>
public enum NavbarColor
{
    /// <summary>
    /// Indicates no contextual color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates no contextual color.")]
    None,

    /// <summary>
    /// Indicates the primary color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the primary color.")]
    Primary,

    /// <summary>
    /// Indicates the secondary color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the secondary color.")]
    Secondary,

    /// <summary>
    /// Indicates the success color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the success color.")]
    Success,

    /// <summary>
    /// Indicates the danger color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the danger color.")]
    Danger,

    /// <summary>
    /// Indicates the warning color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the warning color.")]
    Warning,

    /// <summary>
    /// Indicates the info color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the info color.")]
    Info,

    /// <summary>
    /// Indicates the light color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the light color.")]
    Light,

    /// <summary>
    /// Indicates the dark color.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the dark color.")]
    Dark
}
