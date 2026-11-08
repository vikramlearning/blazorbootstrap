namespace BlazorBootstrap;

/// <summary>
/// Defines the supported shapes for an <see cref="Avatar" />.
/// </summary>
public enum AvatarShape
{
    /// <summary>
    /// Renders the avatar as a circle.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Renders the avatar as a circle.")]
    Circle,

    /// <summary>
    /// Renders the avatar as a square with rounded corners.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Renders the avatar as a square with rounded corners.")]
    Rounded,

    /// <summary>
    /// Renders the avatar as a square.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Renders the avatar as a square.")]
    Square
}
