namespace BlazorBootstrap;

/// <summary>
/// Defines where a <see cref="Navbar" /> is placed on the page.
/// </summary>
public enum NavbarPlacement
{
    /// <summary>
    /// Places the navbar in the normal page flow.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Places the navbar in the normal page flow.")]
    Static,

    /// <summary>
    /// Fixes the navbar to the top of the viewport.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Fixes the navbar to the top of the viewport.")]
    FixedTop,

    /// <summary>
    /// Fixes the navbar to the bottom of the viewport.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Fixes the navbar to the bottom of the viewport.")]
    FixedBottom,

    /// <summary>
    /// Makes the navbar stick to the top of the viewport after scrolling past it.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Makes the navbar stick to the top of the viewport after scrolling past it.")]
    StickyTop,

    /// <summary>
    /// Makes the navbar stick to the bottom of the viewport.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Makes the navbar stick to the bottom of the viewport.")]
    StickyBottom
}
