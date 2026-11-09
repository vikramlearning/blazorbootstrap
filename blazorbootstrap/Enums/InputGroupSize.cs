namespace BlazorBootstrap;

/// <summary>
/// Defines the supported sizes for an <see cref="InputGroup" />.
/// </summary>
public enum InputGroupSize
{
    /// <summary>
    /// Indicates the default input group size.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the default input group size.")]
    Default,

    /// <summary>
    /// Indicates a small input group.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates a small input group.")]
    Small,

    /// <summary>
    /// Indicates a large input group.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates a large input group.")]
    Large
}
