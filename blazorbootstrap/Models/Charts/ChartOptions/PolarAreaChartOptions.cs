namespace BlazorBootstrap;

[AddedVersion("3.0.0")]
public class PolarAreaChartOptions : ChartOptions
{
    #region Properties, Indexers

    [AddedVersion("3.0.0")]
    public PolarAreaChartPlugins Plugins { get; set; } = new();

    #endregion
}
