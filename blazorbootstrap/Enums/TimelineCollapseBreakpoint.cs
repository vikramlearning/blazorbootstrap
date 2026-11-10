namespace BlazorBootstrap;

/// <summary>
/// Defines the width below which a horizontal or alternate <see cref="Timeline" /> shows as a single vertical column.
/// </summary>
public enum TimelineCollapseBreakpoint
{
    /// <summary>
    /// Never collapses.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Never collapses.")]
    None,

    /// <summary>
    /// Collapses below the small breakpoint (576px).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Collapses below the small breakpoint (576px).")]
    Small,

    /// <summary>
    /// Collapses below the medium breakpoint (768px).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Collapses below the medium breakpoint (768px).")]
    Medium,

    /// <summary>
    /// Collapses below the large breakpoint (992px).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Collapses below the large breakpoint (992px).")]
    Large,

    /// <summary>
    /// Collapses below the extra large breakpoint (1200px).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Collapses below the extra large breakpoint (1200px).")]
    ExtraLarge,

    /// <summary>
    /// Collapses below the extra extra large breakpoint (1400px).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Collapses below the extra extra large breakpoint (1400px).")]
    ExtraExtraLarge
}
