namespace BlazorBootstrap;

/// <summary>
/// Defines the fullscreen of an <see cref="Modal" /> element.
/// </summary>
public enum ModalFullscreen
{
    /// <summary>
    /// Fullscreen mode disabled
    /// </summary>
    Disabled,

    /// <summary>
    /// Always fullscreen modal
    /// </summary>
    Always,

    /// <summary>
    /// Fullscreen for viewports bellow "small" breakpoint (576px).
    /// </summary>
    SmallDown,

    /// <summary>
    /// Fullscreen for viewports bellow "medium" breakpoint (768px).
    /// </summary>
    MediumDown,

    /// <summary>
    /// Fullscreen for viewports bellow "large" breakpoint (992px).
    /// </summary>
    LargeDown,

    /// <summary>
    /// Fullscreen for viewports bellow "extra-large" breakpoint (1200px).
    /// </summary>
    ExtraLargeDown,

    /// <summary>
    /// Fullscreen for viewports bellow "XXL" breakpoint (1400px).
    /// </summary>
    ExtraExtraLargeDown
}