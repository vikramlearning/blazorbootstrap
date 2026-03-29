namespace BlazorBootstrap;

/// <summary>
/// Specifies the placement position for a `Badge` (typically relative to its parent/container).
/// </summary>
public enum BadgePlacement
{
    /// <summary>
    /// No placement is applied.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("No placement is applied.")]
    None,

    /// <summary>
    /// Places the badge at the top-left.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Places the badge at the top-left.")]
    TopLeft,

    /// <summary>
    /// Places the badge at the top-center.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Places the badge at the top-center.")]
    TopCenter,

    /// <summary>
    /// Places the badge at the top-right.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Places the badge at the top-right.")]
    TopRight,

    /// <summary>
    /// Places the badge at the middle-left.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Places the badge at the middle-left.")]
    MiddleLeft,

    /// <summary>
    /// Places the badge at the middle-center.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Places the badge at the middle-center.")]
    MiddleCenter,

    /// <summary>
    /// Places the badge at the middle-right.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Places the badge at the middle-right.")]
    MiddleRight,

    /// <summary>
    /// Places the badge at the bottom-left.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Places the badge at the bottom-left.")]
    BottomLeft,

    /// <summary>
    /// Places the badge at the bottom-center.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Places the badge at the bottom-center.")]
    BottomCenter,

    /// <summary>
    /// Places the badge at the bottom-right.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("Places the badge at the bottom-right.")]
    BottomRight
}
