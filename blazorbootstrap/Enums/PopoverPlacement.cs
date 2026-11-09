namespace BlazorBootstrap;

/// <summary>
/// Defines where a <see cref="Popover" /> opens relative to its trigger.
/// </summary>
public enum PopoverPlacement
{
    /// <summary>
    /// Picks the side with the most room.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Picks the side with the most room.")]
    Auto,

    /// <summary>
    /// Opens above the trigger.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Opens above the trigger.")]
    Top,

    /// <summary>
    /// Opens to the right of the trigger (to the left in right-to-left layouts).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Opens to the right of the trigger (to the left in right-to-left layouts).")]
    Right,

    /// <summary>
    /// Opens below the trigger.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Opens below the trigger.")]
    Bottom,

    /// <summary>
    /// Opens to the left of the trigger (to the right in right-to-left layouts).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Opens to the left of the trigger (to the right in right-to-left layouts).")]
    Left
}
