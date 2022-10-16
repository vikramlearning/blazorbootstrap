namespace BlazorBootstrap;

public partial class DoughnutChart : BaseChart
{
    #region Constructors

    public DoughnutChart()
    {
        chartType = ChartType.Doughnut;
    }

    #endregion Constructors

    #region Members

    #endregion Members

    #region Methods

    public override async Task InitializeAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is not null && chartData.Datasets is not null)
        {
            var datasets = chartData.Datasets.Where(x => x is DoughnutChartDataset).Select(x => (DoughnutChartDataset)x);
            var data = new { Labels = chartData.Labels, Datasets = datasets };
            await JS.InvokeVoidAsync("window.blazorChart.doughnut.initialize", ElementId, GetChartType(), data, (DoughnutChartOptions)chartOptions);
        }
    }

    public override async Task UpdateAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is not null && chartData.Datasets is not null)
        {
            var datasets = chartData.Datasets.Where(x => x is DoughnutChartDataset).Select(x => (DoughnutChartDataset)x);
            var data = new { Labels = chartData.Labels, Datasets = datasets };
            await JS.InvokeVoidAsync("window.blazorChart.doughnut.update", ElementId, GetChartType(), data, (DoughnutChartOptions)chartOptions);
        }
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    #endregion Properties
}
