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

    public override async Task<ChartData> AddDataAsync(ChartData chartData, string dataLabel, IChartDatasetData data)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentNullException(nameof(chartData.Datasets));

        if (data is null)
            throw new ArgumentNullException(nameof(data));

        foreach (var dataset in chartData.Datasets)
        {
            if (dataset is BarChartDataset barChartDataset && barChartDataset.Label == dataLabel)
            {
                if (data is BarChartDatasetData barChartDatasetData)
                    barChartDataset.Data?.Add(barChartDatasetData.Data);
            }
        }

        await JS.InvokeVoidAsync("window.blazorChart.bar.addDatasetData", ElementId, dataLabel, data);

        return chartData;
    }

    public override async Task<ChartData> AddDataAsync(ChartData chartData, string dataLabel, List<IChartDatasetData> data)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentNullException(nameof(chartData.Datasets));

        if (chartData.Labels is null)
            throw new ArgumentNullException(nameof(chartData.Labels));

        if (dataLabel is null)
            throw new ArgumentNullException(nameof(dataLabel));

        if (string.IsNullOrWhiteSpace(dataLabel))
            throw new Exception($"{nameof(dataLabel)} cannot be empty.");

        if (data is null)
            throw new ArgumentNullException(nameof(data));

        if (!data.Any())
            throw new Exception($"{nameof(data)} cannot be empty.");

        if (chartData.Datasets.Count != data.Count)
            throw new InvalidDataException("The chart dataset count and the new data points count do not match.");

        if (chartData.Labels.Contains(dataLabel))
            throw new Exception($"{dataLabel} already exists.");

        chartData.Labels.Add(dataLabel);

        foreach (var dataset in chartData.Datasets)
        {
            if (dataset is BarChartDataset barChartDataset)
            {
                var chartDatasetData = data.FirstOrDefault(x => x is BarChartDatasetData barChartDatasetData && barChartDatasetData.DatasetLabel == barChartDataset.Label);

                if (chartDatasetData is BarChartDatasetData barChartDatasetData)
                    barChartDataset.Data?.Add(barChartDatasetData.Data);
            }
        }

        await JS.InvokeVoidAsync("window.blazorChart.bar.addDatasetsData", ElementId, dataLabel, data?.Select(x => (BarChartDatasetData)x));

        return chartData;
    }

    public override async Task<ChartData> AddDatasetAsync(ChartData chartData, IChartDataset chartDataset, IChartOptions chartOptions)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentNullException(nameof(chartData.Datasets));

        if (chartDataset is null)
            throw new ArgumentNullException(nameof(chartDataset));

        if (chartDataset is BarChartDataset)
        {
            chartData.Datasets.Add(chartDataset);
            await JS.InvokeVoidAsync("window.blazorChart.bar.addDataset", ElementId, (BarChartDataset)chartDataset);
        }

        return chartData;
    }

    public override async Task InitializeAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is not null && chartData.Datasets is not null)
        {
            var datasets = chartData.Datasets.OfType<BarChartDataset>();
            var data = new { chartData.Labels, Datasets = datasets };
            await JS.InvokeVoidAsync("window.blazorChart.bar.initialize", ElementId, GetChartType(), data, (BarChartOptions)chartOptions);
        }
    }

    public override async Task UpdateAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is not null && chartData.Datasets is not null)
        {
            var datasets = chartData.Datasets.OfType<BarChartDataset>();
            var data = new { chartData.Labels, Datasets = datasets };
            await JS.InvokeVoidAsync("window.blazorChart.bar.update", ElementId, GetChartType(), data, (BarChartOptions)chartOptions);
        }
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    #endregion Properties
}
