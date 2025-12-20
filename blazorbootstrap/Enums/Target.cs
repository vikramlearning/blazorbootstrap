namespace BlazorBootstrap;

/// <summary>
/// Defines the supported link target options used when rendering navigational elements.
/// </summary>
public enum Target
{
    /// <summary>
    /// Indicates that no target value is specified.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates that no target value is specified.")]
    None,

    /// <summary>
    /// Indicates that the link should open in a new browsing context.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates that the link should open in a new browsing context.")]
    Blank,

    /// <summary>
    /// Indicates that the link should open in the parent browsing context.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates that the link should open in the parent browsing context.")]
    Parent,

    /// <summary>
    /// Indicates that the link should open in the current browsing context.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates that the link should open in the current browsing context.")]
    Self,

    /// <summary>
    /// Indicates that the link should open in the top-level browsing context.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates that the link should open in the top-level browsing context.")]
    Top
}
