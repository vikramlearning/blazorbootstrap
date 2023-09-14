namespace BlazorBootstrap;

public  class DoughnutChartPlugins : ChartPlugins
{
    public DoughnutChartDataLabels Datalabels { get; set; } = new();
}

public class DoughnutChartDataLabels
{
    public string? BorderColor { get; set; } = "white";
    public double BorderRadius { get; set; } = 25;
    public double BorderWidth { get; set; } = 2;
    public string? Color { get; set; } = "white";
    public DoughnutChartDataLabelsFont Font { get; set; } = new();
    public double Padding { get; set; } = 6;
}

public class DoughnutChartDataLabelsFont
{
    public string? Weight { get; set; } = "bold";
}
