namespace BlazorBootstrap;

/// <summary>
/// Defines the supported autoplay modes for the BlazorBootstrap carousel.
/// </summary>
public enum CarouselAutoPlay
{
    /// <summary>
    /// Indicates that the carousel does not start cycling automatically.
    /// </summary>
    [AddedVersion("3.0.0")]
    [Description("Indicates that the carousel does not start cycling automatically.")]
    None,

    /// <summary>
    /// Indicates that the carousel starts cycling as soon as the page is loaded.
    /// </summary>
    [AddedVersion("3.0.0")]
    [Description("Indicates that the carousel starts cycling as soon as the page is loaded.")]
    StartOnPageLoad,

    /// <summary>
    /// Indicates that the carousel starts cycling after the user interacts with it.
    /// </summary>
    [AddedVersion("3.0.0")]
    [Description("Indicates that the carousel starts cycling after the user interacts with it.")]
    StartAfterUserInteraction
}
