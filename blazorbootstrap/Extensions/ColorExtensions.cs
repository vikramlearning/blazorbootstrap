using System.Drawing;

namespace BlazorBootstrap.Extensions;

public static class ColorExtensions
{
    /// <summary>
    /// Converts System.Drawing.Color to #RRGGBB format string.
    /// <see cref="https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color?view=net-6.0"/>
    /// </summary>
    /// <param name="c"></param>
    /// <returns>#RRGGBB format string</returns>
    public static string ToHexString(this System.Drawing.Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    /// <summary>
    /// Converts System.Drawing.Color to #RRGGBBAA format string.
    /// <see cref="https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color?view=net-6.0"/>
    /// </summary>
    /// <param name="c"></param>
    /// <returns>#RRGGBBAA format string</returns>
    public static string ToHexaString(this Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}{c.A:X2}";

    /// <summary>
    /// Converts System.Drawing.Color to RGB(R, G, B) format string.
    /// <see cref="https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color?view=net-6.0"/>
    /// </summary>
    /// <param name="c"></param>
    /// <returns>RGB(R, G, B) format string</returns>
    public static string ToRgbString(this Color c) => $"RGB({c.R}, {c.G}, {c.B})";

    /// <summary>
    /// Converts System.Drawing.Color to RGBA(R, G, B, A) format string.
    /// <see cref="https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color?view=net-6.0"/>
    /// </summary>
    /// <param name="c">System.Drawing.Color</param>
    /// <param name="alpha">The alpha parameter is a number between 0.0 (fully transparent) and 1.0 (fully opaque).</param>
    /// <returns>RGBA(R, G, B, A) format string</returns>
    public static string ToRgbaString(this Color c, double alpha = 0.2) => $"RGBA({c.R}, {c.G}, {c.B}, {alpha})";
}
