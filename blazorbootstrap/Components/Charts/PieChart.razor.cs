namespace BlazorBootstrap;

/// <summary>
/// Pie charts are probably the most commonly used charts. <br/>
/// They are divided into segments, the arc of each segment shows the proportional value of each piece of data. <br/>
/// For more information, visit <see href="https://www.chartjs.org/docs/latest/charts/doughnut.html"/>
/// </summary>
public partial class PieChart : BlazorBootstrapChart
{
    #region Constructors

    /// <summary>
    /// Default constructor
    /// </summary>
    public PieChart()
    {
        ChartType = ChartType.Pie;
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
            if (dataset is PieChartDataset pieChartDataset && pieChartDataset.Label == dataLabel)
                if (data is PieChartDatasetData pieChartDatasetData)
                {
                    pieChartDataset.Data?.Add(pieChartDatasetData.Data);
                    pieChartDataset.BackgroundColor?.Add(pieChartDatasetData.BackgroundColor!);
                }

        await JsRuntime.InvokeVoidAsync("window.blazorChart.pie.addDatasetData", Id, dataLabel, data);

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
            if (dataset is PieChartDataset pieChartDataset)
            {
                var chartDatasetData = data.FirstOrDefault(x => x is PieChartDatasetData pieChartDatasetSubData && pieChartDatasetSubData.DatasetLabel == pieChartDataset.Label);

                if (chartDatasetData is PieChartDatasetData pieChartDatasetData)
                {
                    pieChartDataset.Data?.Add(pieChartDatasetData.Data);
                    pieChartDataset.BackgroundColor?.Add(pieChartDatasetData.BackgroundColor!);
                }
            }

        await JsRuntime.InvokeVoidAsync("window.blazorChart.pie.addDatasetsData", Id, dataLabel, data?.Select(x => (PieChartDatasetData)x));

        return chartData;
    }

    /// <inheritdoc />
    public override async Task<ChartData> AddDatasetAsync(ChartData? chartData, IChartDataset chartDataset, IChartOptions chartOptions)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentException("chartData.Datasets cannot be null", nameof(chartData));

        if (chartDataset is null)
            throw new ArgumentNullException(nameof(chartDataset));

        if (chartDataset is PieChartDataset pieChartDataset)
        {
            chartData.Datasets.Add(pieChartDataset);
            await JsRuntime.InvokeVoidAsync("window.blazorChart.pie.addDataset", Id, pieChartDataset);
        }

        return chartData;
    }

    /// <inheritdoc />
    public override async Task InitializeAsync(ChartData? chartData, IChartOptions chartOptions, string[]? plugins = null)
    {
        if (chartData?.Datasets != null)
        {
            var datasets = chartData.Datasets.OfType<PieChartDataset>();
            var data = new { chartData.Labels, Datasets = datasets };
            await JsRuntime.InvokeVoidAsync("window.blazorChart.pie.initialize", Id, GetChartType(), data, (PieChartOptions)chartOptions, plugins);
        }
    }

    /// <inheritdoc />
    public override async Task UpdateAsync(ChartData? chartData, IChartOptions chartOptions)
    {
        if (chartData?.Datasets != null)
        {
            var datasets = chartData.Datasets.OfType<PieChartDataset>();
            var data = new { chartData.Labels, Datasets = datasets };
            await JsRuntime.InvokeVoidAsync("window.blazorChart.pie.update", Id, GetChartType(), data, (PieChartOptions)chartOptions);
        }
    }

    #endregion
}
