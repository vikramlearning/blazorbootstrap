namespace BlazorBootstrap;

public class DoughnutChartPlugins : ChartPlugins
{
    #region Properties, Indexers

    public DoughnutChartDataLabels Datalabels { get; set; } = new();

    #endregion
}

public class DoughnutChartDataLabels
{
    #region Properties, Indexers

    public string? BorderColor { get; set; } = "white";
    public double BorderRadius { get; set; } = 25;
    public double BorderWidth { get; set; } = 2;
    public string? Color { get; set; } = "white";
    public DoughnutChartDataLabelsFont Font { get; set; } = new();
    public double Padding { get; set; } = 6;

    #endregion
}

public class DoughnutChartDataLabelsFont
{
    #region Properties, Indexers

    public string? Weight { get; set; } = "bold";

    #endregion
}
