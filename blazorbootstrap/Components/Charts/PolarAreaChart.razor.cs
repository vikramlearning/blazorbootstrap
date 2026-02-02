namespace BlazorBootstrap;

public partial class PolarAreaChart : BlazorBootstrapChart
{
    #region Fields and Constants

    private const string _jsObjectName = "window.blazorChart.polarArea";

    #endregion

    #region Constructors

    public PolarAreaChart()
    {
        chartType = ChartType.PolarArea;
    }

    #endregion

    #region Methods

    // TODO: May be this method is not required
    public override async Task<ChartData> AddDataAsync(ChartData chartData, string dataLabel, IChartDatasetData data)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentException("chartData.Datasets must not be null", nameof(chartData));

        if (data is null)
            throw new ArgumentNullException(nameof(data));

        foreach (var dataset in chartData.Datasets)
            if (dataset is PolarAreaChartDataset barChartDataset && barChartDataset.Label == dataLabel)
                if (data is PolarAreaChartDatasetData barChartDatasetData)
                    barChartDataset.Data?.Add(barChartDatasetData.Data as double?);

        await SafeInvokeVoidAsync($"{_jsObjectName}.addDatasetData", Id, dataLabel, data);

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
            throw new Exception($"{nameof(data)} cannot be empty.");

        if (chartData.Datasets.Count != data.Count)
            throw new InvalidDataException("The chart dataset count and the new data points count do not match.");

        if (chartData.Labels.Contains(dataLabel))
            throw new Exception($"{dataLabel} already exists.");

        chartData.Labels.Add(dataLabel);

        foreach (var dataset in chartData.Datasets)
            if (dataset is PolarAreaChartDataset barChartDataset)
            {
                var chartDatasetData = data.FirstOrDefault(x => x is PolarAreaChartDatasetData barChartDatasetData && barChartDatasetData.DatasetLabel == barChartDataset.Label);

                if (chartDatasetData is PolarAreaChartDatasetData barChartDatasetData)
                    barChartDataset.Data?.Add(barChartDatasetData.Data as double?);
            }

        await SafeInvokeVoidAsync($"{_jsObjectName}.addDatasetsData", Id, dataLabel, data?.Select(x => (PolarAreaChartDatasetData)x));

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

        if (chartDataset is PolarAreaChartDataset)
        {
            chartData.Datasets.Add(chartDataset);
            await SafeInvokeVoidAsync($"{_jsObjectName}.addDataset", Id, (PolarAreaChartDataset)chartDataset);
        }

        return chartData;
    }

    public override async Task InitializeAsync(ChartData chartData, IChartOptions chartOptions, string[]? plugins = null)
    {
        if (chartData is not null && chartData.Datasets is not null)
        {
            var datasets = chartData.Datasets.OfType<PolarAreaChartDataset>();
            var data = new { chartData.Labels, Datasets = datasets };
            await SafeInvokeVoidAsync($"{_jsObjectName}.initialize", Id, GetChartType(), data, (PolarAreaChartOptions)chartOptions, plugins);
        }
    }

    public override async Task UpdateAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is not null && chartData.Datasets is not null)
        {
            var datasets = chartData.Datasets.OfType<PolarAreaChartDataset>();
            var data = new { chartData.Labels, Datasets = datasets };
            await SafeInvokeVoidAsync($"{_jsObjectName}.update", Id, GetChartType(), data, (PolarAreaChartOptions)chartOptions);
        }
    }

    #endregion
}
