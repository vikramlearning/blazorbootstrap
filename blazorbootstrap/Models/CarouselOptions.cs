namespace BlazorBootstrap;

public class CarouselOptions
{
    /// <summary>
    /// The amount of time to delay between automatically cycling an item.
    /// </summary>
    /// <remarks>
    /// Default value is 5000.
    /// </remarks>
    public int? Interval { get; set; } = 5000;

    /// <summary>
    /// Whether the carousel should react to keyboard events.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true"/>.
    /// </remarks>
    public bool Keyboard { get; set; } = true;

    /// <summary>
    /// If set to "hover", pauses the cycling of the carousel on mouseenter and resumes the cycling of the carousel on mouseleave. 
    /// If set to <see langword="false"/>, hovering over the carousel won’t pause it. 
    /// On touch-enabled devices, when set to "hover", cycling will pause on touchend (once the user finished interacting with the carousel) for two intervals, before automatically resuming. 
    /// This is in addition to the mouse behavior.
    /// </summary>
    /// <remarks>
    /// Default value is "hover".
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Pause { get; set; } = "hover";

    /// <summary>
    /// If set to <see langword="true"/>, autoplays the carousel after the user manually cycles the first item. 
    /// If set to "carousel", autoplays the carousel on load.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false"/>.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Ride { get; set; } = false;

    /// <summary>
    /// Whether the carousel should support left/right swipe interactions on touchscreen devices.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true"/>.
    /// </remarks>
    public bool Touch { get; set; } = true;

    /// <summary>
    /// Whether the carousel should cycle continuously or have hard stops.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true"/>.
    /// </remarks>
    public bool Wrap { get; set; } = true;
}
