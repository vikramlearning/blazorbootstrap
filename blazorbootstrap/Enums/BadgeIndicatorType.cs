namespace BlazorBootstrap;

/// <summary>
/// Specifies the available indicator shapes for the `Badge` component.
/// </summary>
public enum BadgeIndicatorType
{
    /// <summary>
    /// No indicator styling is applied.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("No indicator styling is applied.")]
    None,

    /// <summary>
    /// Applies a pill-shaped (fully rounded) indicator.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Applies a pill-shaped (fully rounded) indicator.")]
    RoundedPill,

    /// <summary>
    /// Applies a circular (fully rounded) indicator.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Applies a circular (fully rounded) indicator.")]
    RoundedCircle
}
