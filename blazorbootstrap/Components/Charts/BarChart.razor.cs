namespace BlazorBootstrap;

[AddedVersion("1.0.0")]
public partial class BarChart : BlazorBootstrapChart
{
    #region Fields and Constants

    private const string _jsObjectName = "window.blazorChart.bar";

    #endregion

    #region Constructors

    public BarChart()
    {
        chartType = ChartType.Bar;
    }

    #endregion

    #region Methods

    [AddedVersion("1.10.0")]
    [Description("Adds data to chart.")]
    public override async Task<ChartData> AddDataAsync(ChartData chartData, string dataLabel, IChartDatasetData data)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentNullException(nameof(chartData.Datasets));

        if (data is null)
            throw new ArgumentNullException(nameof(data));

        foreach (var dataset in chartData.Datasets)
            BarLineChartSupport.AppendDataPoint(dataset, data);

        await SafeInvokeVoidAsync($"{_jsObjectName}.addDatasetData", Id, dataLabel, data);

        return chartData;
    }

    [AddedVersion("1.10.0")]
    [Description("Adds dataset to chart.")]
    public override async Task<ChartData> AddDataAsync(ChartData chartData, string dataLabel, IReadOnlyCollection<IChartDatasetData> data)
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
            var chartDatasetData = data.FirstOrDefault(x => x is ChartDatasetData chartDataPoint && chartDataPoint.DatasetLabel == (dataset as dynamic).Label);

            if (chartDatasetData is not null)
                BarLineChartSupport.AppendDataPoint(dataset, chartDatasetData);
        }

        await SafeInvokeVoidAsync($"{_jsObjectName}.addDatasetsData", Id, dataLabel, data?.OfType<ChartDatasetData>());

        return chartData;
    }

    [AddedVersion("1.10.0")]
    [Description("Adds dataset to chart.")]
    public override async Task<ChartData> AddDatasetAsync(ChartData chartData, IChartDataset chartDataset, IChartOptions chartOptions)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentNullException(nameof(chartData.Datasets));

        if (chartDataset is null)
            throw new ArgumentNullException(nameof(chartDataset));

        if (BarLineChartSupport.IsSupportedDataset(chartDataset))
        {
            chartData.Datasets.Add(chartDataset);
            await SafeInvokeVoidAsync($"{_jsObjectName}.addDataset", Id, chartDataset);
        }

        return chartData;
    }

    [AddedVersion("1.0.0")]
    [Description("Initializes the chart.")]
    public override async Task InitializeAsync(ChartData chartData, IChartOptions chartOptions, string[]? plugins = null)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentNullException(nameof(chartData.Datasets));

        if (chartOptions is null)
            throw new ArgumentNullException(nameof(chartOptions));

        var data = new { chartData.Labels, Datasets = BarLineChartSupport.GetSupportedDatasets(chartData) };
        await SafeInvokeVoidAsync($"{_jsObjectName}.initialize", Id, GetChartType(), data, (BarChartOptions)chartOptions, plugins, ObjRef);
    }

    [AddedVersion("1.0.0")]
    [Description("Updates the chart.")]
    public override async Task UpdateAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentNullException(nameof(chartData.Datasets));

        if (chartOptions is null)
            throw new ArgumentNullException(nameof(chartOptions));

        var data = new { chartData.Labels, Datasets = BarLineChartSupport.GetSupportedDatasets(chartData) };
        await SafeInvokeVoidAsync($"{_jsObjectName}.update", Id, GetChartType(), data, (BarChartOptions)chartOptions, ObjRef);
    }

    #endregion
}
