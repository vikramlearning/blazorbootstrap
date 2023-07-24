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

    public override async Task AddDataAsync(List<double>? data)
    {
        if (data is not null && data.Count > 0)
        {
            await JS.InvokeVoidAsync("window.blazorChart.line.addData", ElementId, data);
        }
    }

    public override async Task AddDatasetAsync(IChartDataset chartDataset)
    {
        if (chartDataset is not null && chartDataset is LineChartDataset)
        {
            await JS.InvokeVoidAsync("window.blazorChart.line.addDataset", ElementId, (LineChartDataset)chartDataset);
        }
    }

    public override async Task InitializeAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is not null && chartData.Datasets is not null)
        {
            var datasets = chartData.Datasets.OfType<LineChartDataset>();
            var data = new { chartData.Labels, Datasets = datasets };
            await JS.InvokeVoidAsync("window.blazorChart.line.initialize", ElementId, GetChartType(), data, (LineChartOptions)chartOptions);
        }
    }

    public override async Task UpdateAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is not null && chartData.Datasets is not null)
        {
            var datasets = chartData.Datasets.OfType<LineChartDataset>();
            var data = new { chartData.Labels, Datasets = datasets };
            await JS.InvokeVoidAsync("window.blazorChart.line.update", ElementId, GetChartType(), data, (LineChartOptions)chartOptions);
        }
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    #endregion Properties
}
