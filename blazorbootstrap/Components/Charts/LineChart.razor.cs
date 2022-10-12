namespace BlazorBootstrap;

public partial class LineChart : BaseChart
{
    #region Constructors

    public LineChart()
    {
        chartType = ChartType.Line;
    }

    #endregion Constructors

    #region Members

    #endregion Members

    #region Methods

    public override async Task UpdateAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is not null && chartData.Datasets is not null)
        {
            var datasets = chartData.Datasets.Where(x => x is LineChartDataset).Select(x => (LineChartDataset)x);

            var data = new { Labels = chartData.Labels, Datasets = datasets };
            await JS.InvokeVoidAsync("window.blazorChart.line.update", ElementId, GetChartType(), data, (LineChartOptions)chartOptions);
        }
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    #endregion Properties
}
