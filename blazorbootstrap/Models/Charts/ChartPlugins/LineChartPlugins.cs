namespace BlazorBootstrap;

public class LineChartPlugins : ChartPlugins
{
    #region Properties, Indexers

    public LineChartDataLabels Datalabels { get; set; } = new();

    #endregion
}

public class LineChartDataLabels
{
    #region Properties, Indexers

    public double BorderRadius { get; set; } = 4;
    public string? Color { get; set; } = "white";
    public LineChartDataLabelsFont Font { get; set; } = new();
    public double Padding { get; set; } = 6;

    #endregion
}

public class LineChartDataLabelsFont
{
    #region Properties, Indexers

    public string? Weight { get; set; } = "bold";

    #endregion
}
