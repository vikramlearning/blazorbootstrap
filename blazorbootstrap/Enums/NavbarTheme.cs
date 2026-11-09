namespace BlazorBootstrap;

/// <summary>
/// Defines the color theme of a <see cref="Navbar" />, set through Bootstrap's <c>data-bs-theme</c> attribute.
/// </summary>
public enum NavbarTheme
{
    /// <summary>
    /// Inherits the theme of the page.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Inherits the theme of the page.")]
    None,

    /// <summary>
    /// Uses the light theme: dark text for light backgrounds.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Uses the light theme: dark text for light backgrounds.")]
    Light,

    /// <summary>
    /// Uses the dark theme: light text for dark backgrounds.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Uses the dark theme: light text for dark backgrounds.")]
    Dark
}
