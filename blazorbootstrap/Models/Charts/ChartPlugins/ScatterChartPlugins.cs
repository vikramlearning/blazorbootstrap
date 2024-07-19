namespace BlazorBootstrap;

public class ScatterChartPlugins : ChartPlugins
{
    #region Properties, Indexers

    public ScatterChartDataLabels Datalabels { get; set; } = new();

    #endregion
}

public class ScatterChartDataLabels
{
    #region Properties, Indexers

    public double BorderRadius { get; set; } = 4;
    public string? Color { get; set; } = "white";
    public ScatterChartDataLabelsFont Font { get; set; } = new();
    public double Padding { get; set; } = 6;

    #endregion
}

public class ScatterChartDataLabelsFont
{
    #region Properties, Indexers

    public string? Weight { get; set; } = "bold";

    #endregion
}
