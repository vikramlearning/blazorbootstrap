using System.Runtime.InteropServices;

namespace BlazorBootstrap.Models.Css;

/// <summary>
/// Represents the opacity of a color or CSS element based on a provided value. <br/>
/// This structure exists so both integers ranging from 0 to 255 and doubles ranging from 0 to 1 can be used interchangeably.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct CssOpacity
{
    [FieldOffset(0)]
    private readonly byte _opacity;

    /// <summary>
    /// Default constructor for integer input.
    /// </summary>
    /// <param name="opacity">opacity to be shown.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="opacity"/> is below 0 or above 255</exception>
    public CssOpacity(int opacity)
    {
        if (opacity is < 0 or > 255)
        {
                       throw new ArgumentOutOfRangeException(nameof(opacity), "Opacity must be between 0 and 255.");
        }
        _opacity = (byte)opacity;
                  }

    /// <summary>
    /// Default constructor for double input.
    /// </summary>
    /// <param name="opacity">Opacity to be shown</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="opacity"/> is below 0.0 and above 1.0</exception>
    public CssOpacity(double opacity)
    {
        if (opacity is < 0 or > 1)
        {
                       throw new ArgumentOutOfRangeException(nameof(opacity), "Opacity must be between 0 and 1.");
        }
        _opacity = (byte)(opacity * 255);
                  }

    /// <summary>
    /// Implicit conversion from <see langword="int"/> to <see cref="CssOpacity"/>
    /// </summary>
    /// <param name="opacity">Opacity value</param>
    public static implicit operator CssOpacity(int opacity) => new(opacity);

    /// <summary>
    /// Implicit conversion from <see cref="CssOpacity"/> to <see cref="int"/>
    /// </summary>
    /// <param name="opacity">Int to be returned.</param>
    public static implicit operator int(CssOpacity opacity) => opacity._opacity;

    /// <summary>
    /// Implicit conversion from <see langword="double"/> to <see cref="CssOpacity"/> 
    /// </summary>
    /// <param name="opacity">Opacity value</param>
    public static implicit operator CssOpacity(double opacity) => new(opacity);

    /// <summary>
    /// Implicit conversion from <see cref="CssOpacity"/> to <see langword="double"/>
    /// </summary>
    /// <param name="opacity">Opacity to be converted</param>
    public static implicit operator double(CssOpacity opacity) => opacity._opacity / 255.0;

    /// <summary>
    /// Implicit conversion from <see cref="CssOpacity"/> to <see cref="string"/>
    /// </summary>
    /// <param name="opacity">Opacity to display</param>
    public static implicit operator string(CssOpacity opacity) => opacity._opacity.ToString("G");

    /// <inheritdoc />
    public override string ToString() => _opacity.ToString("G");
    
}
