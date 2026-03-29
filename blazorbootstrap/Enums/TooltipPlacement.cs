namespace BlazorBootstrap;

/// <summary>
/// Defines the supported placement options for a BlazorBootstrap tooltip.
/// </summary>
public enum TooltipPlacement
{
    /// <summary>
    /// Dynamically reorient the tooltip.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Dynamically reorient the tooltip.")]
    Auto,

    /// <summary>
    /// Top side.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Top side.")]
    Top,

    /// <summary>
    /// Bottom side.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Bottom side.")]
    Bottom,

    /// <summary>
    /// Left side.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Left side.")]
    Left,

    /// <summary>
    /// Right side.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Right side.")]
    Right
}
