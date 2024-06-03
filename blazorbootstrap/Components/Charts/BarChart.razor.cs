namespace BlazorBootstrap;

/// <summary>
/// A bar chart provides a way of showing data values represented as vertical bars. <br/>
/// It is sometimes used to show trend data, and the comparison of multiple data sets side by side. <br/>
/// For more information, visit <see href="https://www.chartjs.org/docs/latest/charts/bar.html"/>
/// </summary>
public partial class BarChart : BlazorBootstrapChart
{
    #region Constructors

    public BarChart()
    {
        ChartType = ChartType.Bar;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override async Task<ChartData> AddDataAsync(ChartData chartData, string dataLabel, IChartDatasetData data)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentException("chartData.Datasets cannot be null", nameof(chartData));

        if (data is null)
            throw new ArgumentNullException(nameof(data));

        foreach (var dataset in chartData.Datasets)
            if (dataset is BarChartDataset barChartDataset && barChartDataset.Label == dataLabel)
                if (data is BarChartDatasetData barChartDatasetData)
                    barChartDataset.Data?.Add(barChartDatasetData.Data);

        await JsRuntime.InvokeVoidAsync("window.blazorChart.bar.addDatasetData", Id, dataLabel, data);

        return chartData;
    }

    /// <inheritdoc />
    public override async Task<ChartData> AddDataAsync(ChartData chartData, string dataLabel, IReadOnlyCollection<IChartDatasetData> data)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentException("chartData.Datasets cannot be null", nameof(chartData));

        if (chartData.Labels is null)
            throw new ArgumentException("chartData.Labels cannot be null", nameof(chartData));

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
            if (dataset is BarChartDataset barChartDataset)
            {
                var chartDatasetData = data.FirstOrDefault(x => x is BarChartDatasetData barChartDatasetData && barChartDatasetData.DatasetLabel == barChartDataset.Label);

                if (chartDatasetData is BarChartDatasetData barChartDatasetData)
                    barChartDataset.Data?.Add(barChartDatasetData.Data);
            }

        await JsRuntime.InvokeVoidAsync("window.blazorChart.bar.addDatasetsData", Id, dataLabel, data?.Select(x => (BarChartDatasetData)x));

        return chartData;
    }

    /// <inheritdoc />
    public override async Task<ChartData> AddDatasetAsync(ChartData chartData, IChartDataset chartDataset, IChartOptions chartOptions)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentException("chartData.Datasets cannot be null", nameof(chartData));

        if (chartDataset is null)
            throw new ArgumentNullException(nameof(chartDataset));

        if (chartDataset is BarChartDataset barChartDataset)
        {
            chartData.Datasets.Add(barChartDataset);
            await JsRuntime.InvokeVoidAsync("window.blazorChart.bar.addDataset", Id, barChartDataset);
        }

        return chartData;
    }

    /// <inheritdoc />
    public override async Task InitializeAsync(ChartData? chartData, IChartOptions chartOptions, string[]? plugins = null)
    {
        if (chartData?.Datasets != null)
        {
            var datasets = chartData.Datasets.OfType<BarChartDataset>();
            var data = new { chartData.Labels, Datasets = datasets };
            await JsRuntime.InvokeVoidAsync("window.blazorChart.bar.initialize", Id, GetChartType(), data, (BarChartOptions)chartOptions, plugins);
        }
    }

    /// <inheritdoc />
    public override async Task UpdateAsync(ChartData? chartData, IChartOptions chartOptions)
    {
        if (chartData?.Datasets != null)
        {
            var datasets = chartData.Datasets.OfType<BarChartDataset>();
            var data = new { chartData.Labels, Datasets = datasets };
            await JsRuntime.InvokeVoidAsync("window.blazorChart.bar.update", Id, GetChartType(), data, (BarChartOptions)chartOptions);
        }
    }

    #endregion
}
