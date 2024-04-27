namespace BlazorBootstrap;

public class BarChartPlugins : ChartPlugins
{
    #region Properties, Indexers

    public BarChartDataLabels Datalabels { get; set; } = new();

    #endregion
}

public class BarChartDataLabels
{
    #region Properties, Indexers

    public string? Color { get; set; } = "white";
    public BarChartDataLabelsFont Font { get; set; } = new();

    #endregion
}

public class BarChartDataLabelsFont
{
    #region Properties, Indexers

    public string? Weight { get; set; } = "bold";

    #endregion
}
