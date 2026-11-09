namespace BlazorBootstrap;

/// <summary>
/// Defines how a <see cref="Navbar" /> shows its menu below the expand breakpoint.
/// </summary>
public enum NavbarCollapseMode
{
    /// <summary>
    /// Shows the menu in a collapse region below the brand.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Shows the menu in a collapse region below the brand.")]
    Collapse,

    /// <summary>
    /// Shows the menu in an offcanvas panel.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Shows the menu in an offcanvas panel.")]
    Offcanvas
}
