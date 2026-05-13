namespace BlazorBootstrap;

/// <summary>
/// Represents configuration options for a polar area chart.
/// </summary>
[AddedVersion("3.0.0")]
public class PolarAreaChartOptions : ChartOptions
{
    #region Properties, Indexers

    [AddedVersion("3.0.0")]
    [Description("Gets or sets the plugin configuration.")]
    public PolarAreaChartPlugins Plugins { get; set; } = new();

    #endregion
}
