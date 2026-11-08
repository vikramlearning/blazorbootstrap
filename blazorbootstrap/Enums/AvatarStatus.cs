namespace BlazorBootstrap;

/// <summary>
/// Defines the presence status shown by an <see cref="Avatar" />.
/// </summary>
public enum AvatarStatus
{
    /// <summary>
    /// No status indicator is shown.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("No status indicator is shown.")]
    None,

    /// <summary>
    /// Indicates the person is online.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the person is online.")]
    Online,

    /// <summary>
    /// Indicates the person is away.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the person is away.")]
    Away,

    /// <summary>
    /// Indicates the person is busy.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the person is busy.")]
    Busy,

    /// <summary>
    /// Indicates the person is offline.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the person is offline.")]
    Offline
}
