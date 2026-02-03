namespace BlazorBootstrap;

public enum OffcanvasResponsive
{
    /// <summary>
    /// Always hide content offcanvas.
    /// </summary>
    Always,

    /// <summary>
    /// Hide content offcanvas bellow "small" breakpoint (576px).
    /// </summary>
    SmallDown,

    /// <summary>
    /// Hide content offcanvas bellow "medium" breakpoint (768px).
    /// </summary>
    MediumDown,

    /// <summary>
    /// Hide content offcanvas bellow "large" breakpoint (992px).
    /// </summary>
    LargeDown,

    /// <summary>
    /// Hide content offcanvas bellow "extra-large" breakpoint (1200px).
    /// </summary>
    ExtraLargeDown,

    /// <summary>
    /// Hide content offcanvas bellow "XXL" breakpoint (1400px).
    /// </summary>
    ExtraExtraLargeDown
}
