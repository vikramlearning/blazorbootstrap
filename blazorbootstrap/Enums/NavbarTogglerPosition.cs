namespace BlazorBootstrap;

/// <summary>
/// Defines where the toggler of a <see cref="Navbar" /> is placed relative to the brand.
/// </summary>
public enum NavbarTogglerPosition
{
    /// <summary>
    /// Places the toggler after the brand, at the end of the navbar.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Places the toggler after the brand, at the end of the navbar.")]
    End,

    /// <summary>
    /// Places the toggler before the brand, at the start of the navbar.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Places the toggler before the brand, at the start of the navbar.")]
    Start
}
