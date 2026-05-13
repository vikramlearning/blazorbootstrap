namespace BlazorBootstrap;

[AddedVersion("1.0.0")]
public class PieChartOptions : ChartOptions
{
    #region Properties, Indexers

    [AddedVersion("1.10.2")]
    public PieChartPlugins Plugins { get; set; } = new();

    #endregion
}
