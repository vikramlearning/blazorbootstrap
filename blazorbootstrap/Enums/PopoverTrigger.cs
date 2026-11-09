namespace BlazorBootstrap;

/// <summary>
/// Defines how a <see cref="Popover" /> opens. Click, Hover, and Focus can be combined; Manual cannot, and wins when combined.
/// </summary>
[Flags]
public enum PopoverTrigger
{
    /// <summary>
    /// Opens and closes on click.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Opens and closes on click.")]
    Click = 1,

    /// <summary>
    /// Opens while the pointer is over the trigger. Pair it with Focus, so keyboard users can open it too.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Opens while the pointer is over the trigger. Pair it with Focus, so keyboard users can open it too.")]
    Hover = 2,

    /// <summary>
    /// Opens while the trigger has focus, and closes on the next click elsewhere.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Opens while the trigger has focus, and closes on the next click elsewhere.")]
    Focus = 4,

    /// <summary>
    /// Opens and closes only from code: ShowAsync, HideAsync, and ToggleAsync.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Opens and closes only from code: ShowAsync, HideAsync, and ToggleAsync.")]
    Manual = 8
}
