namespace BlazorBootstrap;

[AddedVersion("1.0.0")]
public class PieChartOptions : ChartOptions
{
    #region Properties, Indexers

    [AddedVersion("1.10.2")]
    [Description("Gets or sets the plugin configuration.")]
    public PieChartPlugins Plugins { get; set; } = new();

    #endregion
}
