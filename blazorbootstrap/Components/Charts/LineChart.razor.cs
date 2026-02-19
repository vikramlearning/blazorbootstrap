namespace BlazorBootstrap;

public partial class LineChart : BlazorBootstrapChart
{
    #region Constructors

    public LineChart()
    {
        chartType = ChartType.Line;
    }

    #endregion

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
            if (data is LineChartDatasetData lineChartDatasetData)
                if (dataset is LineChartDataset lineChartDataset && lineChartDataset.Label == lineChartDatasetData.DatasetLabel)
                {
                    chartData.Labels?.Add(dataLabel);
                    lineChartDataset.Data?.Add(lineChartDatasetData.Data as double?);
                }


        await SafeInvokeVoidAsync("window.blazorChart.line.addDatasetData", Id, dataLabel, data);

        return chartData;
    }

    public override async Task<ChartData> AddDataAsync(ChartData chartData, string dataLabel, IReadOnlyCollection<IChartDatasetData> data)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentException("chartData.Datasets must not be null", nameof(chartData));

        if (chartData.Labels is null)
            throw new ArgumentException("chartData.Labels must not be null", nameof(chartData));

        if (dataLabel is null)
            throw new ArgumentNullException(nameof(dataLabel));

        if (string.IsNullOrWhiteSpace(dataLabel))
            throw new Exception($"{nameof(dataLabel)} cannot be empty.");

        if (data is null)
            throw new ArgumentNullException(nameof(data));

        if (!data.Any())
            throw new ArgumentException($"{nameof(data)} cannot be empty.", nameof(data));

        if (chartData.Datasets.Count != data.Count)
            throw new InvalidDataException("The chart dataset count and the new data points count do not match.");

        if (chartData.Labels.Contains(dataLabel))
            throw new Exception($"{dataLabel} already exists.");

        chartData.Labels.Add(dataLabel);

        foreach (var dataset in chartData.Datasets)
            if (dataset is LineChartDataset lineChartDataset)
            {
                var chartDatasetData = data.FirstOrDefault(x => x is LineChartDatasetData lineChartDatasetData && lineChartDatasetData.DatasetLabel == lineChartDataset.Label);

                if (chartDatasetData is LineChartDatasetData lineChartDatasetData)
                    lineChartDataset.Data?.Add(lineChartDatasetData.Data as double?);
            }

        await SafeInvokeVoidAsync("window.blazorChart.line.addDatasetsData", Id, dataLabel, data?.Select(x => (LineChartDatasetData)x));

        return chartData;
    }

    public override async Task<ChartData> AddDatasetAsync(ChartData chartData, IChartDataset chartDataset, IChartOptions chartOptions)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentException("chartData.Datasets must not be null", nameof(chartData));

        if (chartDataset is null)
            throw new ArgumentNullException(nameof(chartDataset));

        if (chartDataset is LineChartDataset)
        {
            chartData.Datasets.Add(chartDataset);
            await SafeInvokeVoidAsync("window.blazorChart.line.addDataset", Id, (LineChartDataset)chartDataset);
        }

        return chartData;
    }

    public override async Task InitializeAsync(ChartData chartData, IChartOptions chartOptions, string[]? plugins = null)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentException("chartData.Datasets must not be null", nameof(chartData));

        if (chartOptions is null)
            throw new ArgumentNullException(nameof(chartOptions));

        var datasets = chartData.Datasets.OfType<LineChartDataset>();
        var data = new { chartData.Labels, Datasets = datasets };
        await SafeInvokeVoidAsync("window.blazorChart.line.initialize", Id, GetChartType(), data, (LineChartOptions)chartOptions, plugins);
    }

    public override async Task UpdateAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentException("chartData.Datasets must not be null", nameof(chartData));

        if (chartOptions is null)
            throw new ArgumentNullException(nameof(chartOptions));

        var datasets = chartData.Datasets.OfType<LineChartDataset>();
        var data = new { chartData.Labels, Datasets = datasets };
        await SafeInvokeVoidAsync("window.blazorChart.line.update", Id, GetChartType(), data, (LineChartOptions)chartOptions);
    }

    #endregion
}
