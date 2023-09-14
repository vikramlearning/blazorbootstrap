namespace BlazorBootstrap;

public class PieChartPlugins : ChartPlugins
{
    public PieChartDataLabels Datalabels { get; set; } = new();
}

public class PieChartDataLabels
{
    public string? BorderColor { get; set; } = "white";
    public double BorderRadius { get; set; } = 25;
    public double BorderWidth { get; set; } = 2;
    public string? Color { get; set; } = "white";
    public PieChartDataLabelsFont Font { get; set; } = new();
    public double Padding { get; set; } = 6;
}

public class PieChartDataLabelsFont
{
    public string? Weight { get; set; } = "bold";
}
