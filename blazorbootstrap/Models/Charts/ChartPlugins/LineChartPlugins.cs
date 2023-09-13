namespace BlazorBootstrap;

public class LineChartPlugins : ChartPlugins
{
    public LineChartDataLabels Datalabels { get; set; } = new();
}

public class LineChartDataLabels
{
    public double BorderRadius { get; set; } = 4;
    public string? Color { get; set; } = "white";
    public LineChartDataLabelsFont Font { get; set; } = new();
    public double padding { get; set; } = 6;
}

public class LineChartDataLabelsFont
{
    public string? Weight { get; set; } = "bold";
}
