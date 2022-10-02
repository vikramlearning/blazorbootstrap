using Microsoft.JSInterop;

namespace BlazorBootstrap;

public partial class BarChart : BaseChart
{
    #region Constructors

    public BarChart()
    {
        chartType = ChartType.Bar;
    }

    #endregion Constructors

    #region Members

    #endregion Members

    #region Methods

    public override async Task UpdateAsync(ChartData chartData, ChartOptions chartOptions)
    {
        if (chartData is not null && chartData.Datasets is not null)
        {
            var datasets = chartData.Datasets.Where(x => x is BarChartDataset).Select(x => (BarChartDataset)x);

            var data = new { Labels = chartData.Labels, Datasets = datasets };
            await JS.InvokeVoidAsync("window.blazorChart.bar.update", ElementId, GetChartType(), data, chartOptions);
        }
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    #endregion Properties
}
