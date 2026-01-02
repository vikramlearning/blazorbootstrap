namespace BlazorBootstrap;

public enum ToastsPlacement
{
    TopLeft,

    /// <summary>
    /// Top center
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Top center")]
    TopCenter,

    /// <summary>
    /// Top right
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Top right")]
    TopRight,

    /// <summary>
    /// Middle left
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Middle left")]
    MiddleLeft,

    /// <summary>
    /// Middle center
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Middle center")]
    MiddleCenter,

    /// <summary>
    /// Middle right
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Middle right")]
    MiddleRight,

    /// <summary>
    /// Bottom left
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Bottom left")]
    BottomLeft,

    /// <summary>
    /// Bottom center
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Bottom center")]
    BottomCenter,

    /// <summary>
    /// Bottom right
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Bottom right")]
    BottomRight
}
