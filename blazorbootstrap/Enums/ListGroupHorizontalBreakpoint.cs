namespace BlazorBootstrap;

/// <summary>
/// Defines the breakpoint from which a horizontal <see cref="ListGroup{TItem}" /> lays its items out in a row.
/// </summary>
public enum ListGroupHorizontalBreakpoint
{
    /// <summary>
    /// Lays the items out in a row at every width.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Lays the items out in a row at every width.")]
    Always,

    /// <summary>
    /// Lays the items out in a row from the small breakpoint (576px) up.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Lays the items out in a row from the small breakpoint (576px) up.")]
    Small,

    /// <summary>
    /// Lays the items out in a row from the medium breakpoint (768px) up.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Lays the items out in a row from the medium breakpoint (768px) up.")]
    Medium,

    /// <summary>
    /// Lays the items out in a row from the large breakpoint (992px) up.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Lays the items out in a row from the large breakpoint (992px) up.")]
    Large,

    /// <summary>
    /// Lays the items out in a row from the extra large breakpoint (1200px) up.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Lays the items out in a row from the extra large breakpoint (1200px) up.")]
    ExtraLarge,

    /// <summary>
    /// Lays the items out in a row from the extra extra large breakpoint (1400px) up.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Lays the items out in a row from the extra extra large breakpoint (1400px) up.")]
    ExtraExtraLarge
}
