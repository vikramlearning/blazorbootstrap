namespace BlazorBootstrap;

/// <summary>
/// Predefined set of contextual colors.
/// </summary>
public enum CalloutType
{
    /// <summary>
    /// No color will be applied to an element.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Danger color.
    /// </summary>
    Danger,

    /// <summary>
    /// Warning color.
    /// </summary>
    Warning,

    /// <summary>
    /// Info color.
    /// </summary>
    Info,

    /// <summary>
    /// Success color.
    /// </summary>
    [Obsolete("This enum value is obsolete. Use `Success` instead.")]
    Tip,

    /// <summary>
    /// Success color.
    /// </summary>
    Success
}
