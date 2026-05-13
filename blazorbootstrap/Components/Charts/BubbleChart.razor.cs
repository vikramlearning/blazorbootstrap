namespace BlazorBootstrap;

public partial class BubbleChart : BlazorBootstrapChart
{
    #region Fields and Constants

    private const string _jsObjectName = "window.blazorChart.bubble";

    #endregion

    #region Constructors

    public BubbleChart()
    {
        chartType = ChartType.Bubble;
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
            if (dataset is BubbleChartDataset bubbleChartDataset && bubbleChartDataset.Label == dataLabel)
                if (data is BubbleChartDatasetData bubbleChartDatasetData && bubbleChartDatasetData.Data is BubbleChartDataPoint bubbleChartDataPoint)
                    bubbleChartDataset.Data?.Add(bubbleChartDataPoint);

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
            throw new ArgumentException($"{nameof(data)} cannot be empty.", nameof(data));

        if (chartData.Datasets.Count != data.Count)
            throw new InvalidDataException("The chart dataset count and the new data points count do not match.");

        if (chartData.Labels.Contains(dataLabel))
            throw new Exception($"{dataLabel} already exists.");

        chartData.Labels.Add(dataLabel);

        foreach (var dataset in chartData.Datasets)
            if (dataset is BubbleChartDataset bubbleChartDataset)
            {
                var chartDatasetData = data.FirstOrDefault(x => x is BubbleChartDatasetData bubbleChartDatasetData && bubbleChartDatasetData.DatasetLabel == bubbleChartDataset.Label);

                if (chartDatasetData is BubbleChartDatasetData bubbleChartDatasetData && bubbleChartDatasetData.Data is BubbleChartDataPoint bubbleChartDataPoint)
                    bubbleChartDataset.Data?.Add(bubbleChartDataPoint);
            }

        await SafeInvokeVoidAsync($"{_jsObjectName}.addDatasetsData", Id, dataLabel, data?.Select(x => (BubbleChartDatasetData)x));

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

        if (chartDataset is BubbleChartDataset)
        {
            chartData.Datasets.Add(chartDataset);
            await SafeInvokeVoidAsync($"{_jsObjectName}.addDataset", Id, (BubbleChartDataset)chartDataset);
        }

        return chartData;
    }

    public override async Task InitializeAsync(ChartData chartData, IChartOptions chartOptions, string[]? plugins = null)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentNullException(nameof(chartData.Datasets));

        if (chartOptions is null)
            throw new ArgumentNullException(nameof(chartOptions));

        var datasets = chartData.Datasets.OfType<BubbleChartDataset>();
        var data = new { chartData.Labels, Datasets = datasets };
        await SafeInvokeVoidAsync($"{_jsObjectName}.initialize", Id, GetChartType(), data, (BubbleChartOptions)chartOptions, plugins, ObjRef);
    }

    public override async Task UpdateAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentNullException(nameof(chartData.Datasets));

        if (chartOptions is null)
            throw new ArgumentNullException(nameof(chartOptions));

        var datasets = chartData.Datasets.OfType<BubbleChartDataset>();
        var data = new { chartData.Labels, Datasets = datasets };
        await SafeInvokeVoidAsync($"{_jsObjectName}.update", Id, GetChartType(), data, (BubbleChartOptions)chartOptions, ObjRef);
    }

    #endregion
}