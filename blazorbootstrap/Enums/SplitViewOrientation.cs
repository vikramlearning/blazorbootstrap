namespace BlazorBootstrap;

/// <summary>
/// Defines the available layout orientations for the <see cref="SplitView" /> component.
/// </summary>
public enum SplitViewOrientation
{
    /// <summary>
    /// Arranges panes side by side with a vertical divider.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Arranges panes side by side with a vertical divider.")]
    Horizontal,

    /// <summary>
    /// Arranges panes top to bottom with a horizontal divider.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Arranges panes top to bottom with a horizontal divider.")]
    Vertical,
}