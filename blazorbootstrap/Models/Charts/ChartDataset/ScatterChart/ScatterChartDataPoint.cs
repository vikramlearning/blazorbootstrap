namespace BlazorBootstrap;

/// <summary>
/// Represents a scatter chart data point.
/// </summary>
/// <param name="X">The x-axis value.</param>
/// <param name="Y">The y-axis value.</param>
[AddedVersion("3.0.0")]
public record ScatterChartDataPoint(double X, double Y);
