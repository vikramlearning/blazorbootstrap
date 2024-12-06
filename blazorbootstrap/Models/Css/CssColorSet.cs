using BlazorBootstrap.Models.Css; 

namespace BlazorBootstrap;

/// <summary>
/// Represents a set of colors: primary, secondary, danger, dark, light, info, success, and warning, <br/>
/// to be applied to a specific component or section of the application.
/// </summary>
public sealed class CssColorSet
{
    /// <summary>
    /// Opacity of the color set, to be applied if the color hasn't been overridden.
    /// </summary>
    /// <remarks>
    /// Default value is 1.0
    /// </remarks>
    public CssOpacity? Opacity;

    /// <summary>
    /// Primary color
    /// </summary>
    public Color? Primary;

    /// <summary>
    /// Secondary color
    /// </summary>
    public Color? Secondary;

    /// <summary>
    /// Danger color
    /// </summary>
    public Color? Danger;

    /// <summary>
    /// Dark color
    /// </summary>
    public Color? Dark;

    /// <summary>
    /// Light color
    /// </summary>
    public Color? Light;

    /// <summary>
    /// Info color
    /// </summary>
    public Color? Info;

    /// <summary>
    /// Success color
    /// </summary>
    public Color? Success;

    /// <summary>
    /// Warning color
    /// </summary>
    public Color? Warning;
}
