namespace BlazorBootstrap;

public class BubbleChartPlugins : ChartPlugins
{
    #region Properties, Indexers

    public BubbleChartDataLabels Datalabels { get; set; } = new();

    #endregion
}

public class BubbleChartDataLabels
{
    #region Properties, Indexers

    public double BorderRadius { get; set; } = 4;

    public string? Color { get; set; } = "white";

    public BubbleChartDataLabelsFont Font { get; set; } = new();

    public double Padding { get; set; } = 6;

    #endregion
}

public class BubbleChartDataLabelsFont
{
    #region Properties, Indexers

    public string? Weight { get; set; } = "bold";

    #endregion
}