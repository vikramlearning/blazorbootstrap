namespace BlazorBootstrap;

/// <summary>
/// Defines the width of the container inside a <see cref="Navbar" />.
/// </summary>
public enum NavbarContainer
{
    /// <summary>
    /// Spans the full width of the navbar.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Spans the full width of the navbar.")]
    Fluid,

    /// <summary>
    /// Uses the responsive fixed-width container.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Uses the responsive fixed-width container.")]
    Default,

    /// <summary>
    /// Spans the full width until the small breakpoint.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Spans the full width until the small breakpoint.")]
    Small,

    /// <summary>
    /// Spans the full width until the medium breakpoint.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Spans the full width until the medium breakpoint.")]
    Medium,

    /// <summary>
    /// Spans the full width until the large breakpoint.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Spans the full width until the large breakpoint.")]
    Large,

    /// <summary>
    /// Spans the full width until the extra large breakpoint.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Spans the full width until the extra large breakpoint.")]
    ExtraLarge,

    /// <summary>
    /// Spans the full width until the extra extra large breakpoint.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Spans the full width until the extra extra large breakpoint.")]
    ExtraExtraLarge
}
