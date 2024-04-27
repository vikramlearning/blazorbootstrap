namespace BlazorBootstrap;

/// <summary>
/// Specifies the available styles for navigation elements.
/// </summary>
public enum NavStyle
{
    /// <summary>
    /// Applies the .nav-tabs class to create a tabbed interface.
    /// </summary>
    Tabs,

    /// <summary>
    /// Applies the .nav-pills class to create a tabbed interface with pill-shaped elements.
    /// </summary>
    Pills,

    /// <summary>
    /// Applies the .nav-underline class to create an underlined navigation style.
    /// </summary>
    Underline,

    /// <summary>
    /// Creates a vertical navigation style using the appropriate Bootstrap classes.
    /// </summary>
    Vertical,

    /// <summary>
    /// Creates a vertical navigation style with pill-shaped elements using the appropriate Bootstrap classes.
    /// </summary>
    VerticalPills,

    /// <summary>
    /// Creates a vertical navigation style with an underlined active link using the appropriate Bootstrap classes.
    /// </summary>
    VerticalUnderline
}
