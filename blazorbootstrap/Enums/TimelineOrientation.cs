namespace BlazorBootstrap;

/// <summary>
/// Defines the direction in which a <see cref="Timeline" /> lays out its items.
/// </summary>
public enum TimelineOrientation
{
    /// <summary>
    /// Lays the items out top to bottom.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Lays the items out top to bottom.")]
    Vertical,

    /// <summary>
    /// Lays the items out left to right.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Lays the items out left to right.")]
    Horizontal
}
