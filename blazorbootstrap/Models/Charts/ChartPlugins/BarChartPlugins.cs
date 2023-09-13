namespace BlazorBootstrap;

public class BarChartPlugins : ChartPlugins
{
    public BarChartDataLabels Datalabels { get; set; } = new();
}

public class BarChartDataLabels
{
    public string? Color { get; set; } = "white";
    public BarChartDataLabelsFont Font { get; set; } = new();
}

public class BarChartDataLabelsFont
{
    public string? Weight { get; set; } = "bold";
}
