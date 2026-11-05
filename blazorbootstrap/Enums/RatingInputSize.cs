namespace BlazorBootstrap;

/// <summary>
/// Defines the supported sizes for a <see cref="RatingInput{TValue}" />.
/// </summary>
public enum RatingInputSize
{
    /// <summary>
    /// Indicates the default rating input size.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the default rating input size.")]
    Default,

    /// <summary>
    /// Indicates a small rating input.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates a small rating input.")]
    Small,

    /// <summary>
    /// Indicates a large rating input.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates a large rating input.")]
    Large
}
