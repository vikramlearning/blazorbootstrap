namespace BlazorBootstrap.Demo.RCL.Components.Pages.Demos.Charts;

internal static class ChartColorExtensions
{
    internal static System.Drawing.Color ToChartColor(this string value) => BlazorExpress.ChartJS.ColorExtensions.ToColor(value);

    internal static string ToChartHexString(this System.Drawing.Color value) => BlazorExpress.ChartJS.ColorExtensions.ToHexString(value);

    internal static string ToChartHexaString(this System.Drawing.Color value) => BlazorExpress.ChartJS.ColorExtensions.ToHexaString(value);

    internal static string ToChartRgbString(this System.Drawing.Color value) => BlazorExpress.ChartJS.ColorExtensions.ToRgbString(value);

    internal static string ToChartRgbaString(this System.Drawing.Color value, double alpha = 0.2) => BlazorExpress.ChartJS.ColorExtensions.ToRgbaString(value, alpha);
}