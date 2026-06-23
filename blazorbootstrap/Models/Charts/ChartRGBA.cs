namespace BlazorBootstrap;

/// <summary>
/// Represents an RGBA color value.
/// </summary>
/// <param name="R">The red channel value.</param>
/// <param name="G">The green channel value.</param>
/// <param name="B">The blue channel value.</param>
/// <param name="A">The alpha channel value.</param>
[AddedVersion("1.0.0")]
public record ChartRGBA(int R, int G, int B, double A);
