using System.Runtime.CompilerServices;

namespace BlazorBootstrap;

public static class ColorExtensions
{
    #region Methods

    /// <summary>
    /// Converts an Html color representation to a GDI+ <see cref='Color' />.
    /// </summary>
    /// <param name="hex"></param>
    /// <returns>Converts #RRGGBB string to <see cref='Color' />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Color ToColor(this string hex) => ColorTranslator.FromHtml(hex);

    /// <summary>
    /// Converts System.Drawing.Color to #RRGGBBAA format string.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color?view=net-6.0" />
    /// </summary>
    /// <param name="c"></param>
    /// <returns>#RRGGBBAA format string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static string ToHexaString(this Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}{c.A:X2}";

    /// <summary>
    /// Converts System.Drawing.Color to #RRGGBB format string.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color?view=net-6.0" />
    /// </summary>
    /// <param name="c"></param>
    /// <returns>#RRGGBB format string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static string ToHexString(this Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    /// <summary>
    /// Converts System.Drawing.Color to RGBA(R, G, B, A) format string.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color?view=net-6.0" />
    /// </summary>
    /// <param name="c">System.Drawing.Color</param>
    /// <param name="alpha">The alpha parameter is a number between 0.0 (fully transparent) and 1.0 (fully opaque).</param>
    /// <returns>RGBA(R, G, B, A) format string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static string ToRgbaString(this Color c, double alpha) => $"RGBA({c.R}, {c.G}, {c.B}, {alpha})";

    /// <summary>
    /// Converts System.Drawing.Color to RGBA(R, G, B, A) format string.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color?view=net-6.0" />
    /// </summary>
    /// <param name="c">System.Drawing.Color</param>
    /// <returns>RGBA(R, G, B, A) format string</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining)] public static string ToRgbaString(this Color c) => $"RGBA({c.R}, {c.G}, {c.B}, {c.A})";

    /// <summary>
    /// Converts System.Drawing.Color to RGB(R, G, B) format string.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color?view=net-6.0" />
    /// </summary>
    /// <param name="c"></param>
    /// <returns>RGB(R, G, B) format string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static string ToRgbString(this Color c) => $"RGB({c.R}, {c.G}, {c.B})";


    /// <summary>
    /// Converts System.Drawing.Color to R, G, B format string.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color?view=net-6.0" />
    /// </summary>
    /// <param name="c"></param>
    /// <returns>RGB(R, G, B) format string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static string ToRgbStringValues(this Color c) => $"{c.R}, {c.G}, {c.B}";

    /// <summary>
    /// Mixes two colors.
    /// </summary>
    /// <param name="c1">Color 1</param>
    /// <param name="c2">Color 2</param>
    /// <param name="weight">Weight (between 0 and 1, in favor of <paramref name="c2"/>)</param>
    /// <returns>Mixed color</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color Mix(this Color c1, Color c2, double weight = 0.5)
    {
               var w = weight;
                      var w1 = 1 - w;
                             var a = c1.A * w1 + c2.A * w;
                                    var r = c1.R * w1 + c2.R * w;
                                           var g = c1.G * w1 + c2.G * w;
                                                  var b = c1.B * w1 + c2.B * w;
                                                         return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
                                                            }

    /// <summary>
    /// Lightens a color towards <see cref="Color.White"/>
    /// </summary>
    /// <param name="c">Color to lighten</param>
    /// <param name="weight">Weight (1 means it will be fully white)</param>
    /// <returns>Lightened color</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Color TintColor(this Color c, double weight = 0.5) => c.Mix(Color.White, weight);

    /// <summary>
    /// Shades a color towards <see cref="Color.Black"/>
    /// </summary>
    /// <param name="c">Color to lighten</param>
    /// <param name="weight">Weight (1 means it will be fully black)</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Color ShadeColor(this Color c, double weight = 0.5) => c.Mix(Color.Black, weight);
    #endregion
}
