namespace BlazorBootstrap;

/// <summary>
/// Defines where the line and markers of a vertical <see cref="Timeline" /> sit, and on which side its items' content shows.
/// </summary>
public enum TimelinePlacement
{
    /// <summary>
    /// Puts the line and markers at the start edge, with each item's content beside them.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Puts the line and markers at the start edge, with each item's content beside them.")]
    Start,

    /// <summary>
    /// Puts the line and markers at the end edge, with each item's content beside them, aligned toward the line.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Puts the line and markers at the end edge, with each item's content beside them, aligned toward the line.")]
    End,

    /// <summary>
    /// Centers the line and places the items on alternating sides, starting on the end side.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Centers the line and places the items on alternating sides, starting on the end side.")]
    Alternate,

    /// <summary>
    /// Centers the line and places the items on alternating sides, starting on the start side.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Centers the line and places the items on alternating sides, starting on the start side.")]
    AlternateReverse
}
