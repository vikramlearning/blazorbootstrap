namespace BlazorBootstrap;

public static class ColorExtensions
{
    #region Methods

    /// <summary>
    /// Converts an Html color representation to a GDI+ <see cref='Color' />.
    /// </summary>
    /// <param name="hex"></param>
    /// <returns>Converts #RRGGBB string to <see cref='Color' />.</returns>
    public static Color ToColor(this string hex) => ColorTranslator.FromHtml(hex);

    /// <summary>
    /// Converts System.Drawing.Color to #RRGGBBAA format string.
    /// <see cref="https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color?view=net-6.0" />
    /// </summary>
    /// <param name="c"></param>
    /// <returns>#RRGGBBAA format string</returns>
    public static string ToHexaString(this Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}{c.A:X2}";

    /// <summary>
    /// Converts System.Drawing.Color to #RRGGBB format string.
    /// <see cref="https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color?view=net-6.0" />
    /// </summary>
    /// <param name="c"></param>
    /// <returns>#RRGGBB format string</returns>
    public static string ToHexString(this Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    /// <summary>
    /// Converts System.Drawing.Color to RGBA(R, G, B, A) format string.
    /// <see cref="https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color?view=net-6.0" />
    /// </summary>
    /// <param name="c">System.Drawing.Color</param>
    /// <param name="alpha">The alpha parameter is a number between 0.0 (fully transparent) and 1.0 (fully opaque).</param>
    /// <returns>RGBA(R, G, B, A) format string</returns>
    public static string ToRgbaString(this Color c, double alpha = 0.2) => $"RGBA({c.R}, {c.G}, {c.B}, {alpha})";

    /// <summary>
    /// Converts System.Drawing.Color to RGB(R, G, B) format string.
    /// <see cref="https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color?view=net-6.0" />
    /// </summary>
    /// <param name="c"></param>
    /// <returns>RGB(R, G, B) format string</returns>
    public static string ToRgbString(this Color c) => $"RGB({c.R}, {c.G}, {c.B})";

    #endregion
}