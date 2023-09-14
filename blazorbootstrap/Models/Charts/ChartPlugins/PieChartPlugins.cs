namespace BlazorBootstrap;

public class PieChartPlugins : ChartPlugins
{
    #region Properties, Indexers

    public PieChartDataLabels Datalabels { get; set; } = new();

    #endregion
}

public class PieChartDataLabels
{
    #region Properties, Indexers

    public string? BorderColor { get; set; } = "white";
    public double BorderRadius { get; set; } = 25;
    public double BorderWidth { get; set; } = 2;
    public string? Color { get; set; } = "white";
    public PieChartDataLabelsFont Font { get; set; } = new();
    public double Padding { get; set; } = 6;

    #endregion
}

public class PieChartDataLabelsFont
{
    #region Properties, Indexers

    public string? Weight { get; set; } = "bold";

    #endregion
}
