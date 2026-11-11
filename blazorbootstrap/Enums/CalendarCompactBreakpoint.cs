namespace BlazorBootstrap;

/// <summary>
/// Defines the width below which a <see cref="Calendar{TItem}" /> shows its compact layout.
/// </summary>
public enum CalendarCompactBreakpoint
{
    /// <summary>
    /// Never shows the compact layout.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Never shows the compact layout.")]
    None,

    /// <summary>
    /// Shows the compact layout below the small breakpoint (576px).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Shows the compact layout below the small breakpoint (576px).")]
    Small,

    /// <summary>
    /// Shows the compact layout below the medium breakpoint (768px).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Shows the compact layout below the medium breakpoint (768px).")]
    Medium,

    /// <summary>
    /// Shows the compact layout below the large breakpoint (992px).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Shows the compact layout below the large breakpoint (992px).")]
    Large,

    /// <summary>
    /// Shows the compact layout below the extra large breakpoint (1200px).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Shows the compact layout below the extra large breakpoint (1200px).")]
    ExtraLarge,

    /// <summary>
    /// Shows the compact layout below the extra extra large breakpoint (1400px).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Shows the compact layout below the extra extra large breakpoint (1400px).")]
    ExtraExtraLarge
}
