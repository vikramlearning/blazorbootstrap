namespace BlazorBootstrap;

/// <summary>
/// Defines the breakpoint at which a <see cref="Navbar" /> expands from its collapsed menu to a horizontal bar.
/// </summary>
public enum NavbarExpand
{
    /// <summary>
    /// Never expands; the menu is always collapsed behind the toggler.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Never expands; the menu is always collapsed behind the toggler.")]
    Never,

    /// <summary>
    /// Expands at the small breakpoint (576px) and up.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Expands at the small breakpoint (576px) and up.")]
    Small,

    /// <summary>
    /// Expands at the medium breakpoint (768px) and up.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Expands at the medium breakpoint (768px) and up.")]
    Medium,

    /// <summary>
    /// Expands at the large breakpoint (992px) and up.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Expands at the large breakpoint (992px) and up.")]
    Large,

    /// <summary>
    /// Expands at the extra large breakpoint (1200px) and up.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Expands at the extra large breakpoint (1200px) and up.")]
    ExtraLarge,

    /// <summary>
    /// Expands at the extra extra large breakpoint (1400px) and up.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Expands at the extra extra large breakpoint (1400px) and up.")]
    ExtraExtraLarge,

    /// <summary>
    /// Always expanded; the menu never collapses.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Always expanded; the menu never collapses.")]
    Always
}
