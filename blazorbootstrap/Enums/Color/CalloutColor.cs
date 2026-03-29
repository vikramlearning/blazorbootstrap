namespace BlazorBootstrap;

/// <summary>
/// Defines the supported color styles for a BlazorBootstrap callout.
/// </summary>
public enum CalloutColor
{
    /// <summary>
    /// Uses the default callout color style.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Uses the default callout color style.")]
    Default,

    /// <summary>
    /// Uses the danger callout color style.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Uses the danger callout color style.")]
    Danger,

    /// <summary>
    /// Uses the warning callout color style.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Uses the warning callout color style.")]
    Warning,

    /// <summary>
    /// Uses the info callout color style.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Uses the info callout color style.")]
    Info,

    /// <summary>
    /// Uses the success callout color style.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Uses the success callout color style.")]
    Success
}
