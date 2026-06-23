namespace BlazorBootstrap;

/// <summary>
/// <see href="https://www.chartjs.org/docs/latest/charts/bubble.html#data-structure" />
/// </summary>
[AddedVersion("4.0.0")]
public record BubbleChartDataPoint(double X, double Y, double R);