namespace BlazorBootstrap;

[AddedVersion("1.0.0")]
public class DoughnutChartOptions : ChartOptions
{
    #region Properties, Indexers

    [AddedVersion("1.10.2")]
    public DoughnutChartPlugins Plugins { get; set; } = new();

    #endregion
}
