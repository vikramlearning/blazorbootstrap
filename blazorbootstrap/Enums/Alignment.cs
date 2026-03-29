namespace BlazorBootstrap;

/// <summary>
/// Defines generic alignment options used by BlazorBootstrap components (for example, content alignment and text alignment).
/// </summary>
public enum Alignment
{
    /// <summary>
    /// Indicates that no explicit alignment option is selected.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates that no explicit alignment option is selected.")]
    None,

    /// <summary>
    /// Indicates alignment toward the start edge.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates alignment toward the start edge.")]
    Start,

    /// <summary>
    /// Indicates centered alignment.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates centered alignment.")]
    Center,

    /// <summary>
    /// Indicates alignment toward the end edge.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates alignment toward the end edge.")]
    End
}
