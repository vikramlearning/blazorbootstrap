namespace BlazorBootstrap;

/// <summary>
/// Doughnut charts are probably the most commonly used charts. <br/>
/// They are divided into segments, the arc of each segment shows the proportional value of each piece of data. <br/>
/// For more information, visit <see href="https://www.chartjs.org/docs/latest/charts/doughnut.html"/>
/// </summary>
public partial class DoughnutChart : BlazorBootstrapChart
{
    #region Fields and Constants

    private const string _jsObjectName = "window.blazorChart.doughnut";

    #endregion

    #region Constructors

    /// <summary>
    /// Default constructor
    /// </summary>
    public DoughnutChart()
    {
        ChartType = ChartType.Doughnut;
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
            if (dataset is DoughnutChartDataset doughnutChartDataset && doughnutChartDataset.Label == dataLabel)
                if (data is DoughnutChartDatasetData doughnutChartDatasetData)
                {
                    doughnutChartDataset.Data?.Add(doughnutChartDatasetData.Data as double?);
                    doughnutChartDataset.BackgroundColor?.Add(doughnutChartDatasetData.BackgroundColor!);
                }

        await JsRuntime.InvokeVoidAsync($"{_jsObjectName}.addDatasetData", Id, dataLabel, data);

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
            if (dataset is DoughnutChartDataset doughnutChartDataset)
            {
                var chartDatasetData = data.FirstOrDefault(x => x is DoughnutChartDatasetData doughnutChartDatasetData && doughnutChartDatasetData.DatasetLabel == doughnutChartDataset.Label);

                if (chartDatasetData is DoughnutChartDatasetData doughnutChartDatasetData)
                {
                    doughnutChartDataset.Data?.Add(doughnutChartDatasetData.Data as double?);
                    doughnutChartDataset.BackgroundColor?.Add(doughnutChartDatasetData.BackgroundColor!);
                }
            }

        await JsRuntime.InvokeVoidAsync($"{_jsObjectName}.addDatasetsData", Id, dataLabel, data?.Select(x => (DoughnutChartDatasetData)x));

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

        if (chartDataset is DoughnutChartDataset doughnutChartDataset)
        {
            chartData.Datasets.Add(doughnutChartDataset);
            await JsRuntime.InvokeVoidAsync($"{_jsObjectName}.addDataset", Id, doughnutChartDataset);
        }

        return chartData;
    }
    
    /// <inheritdoc />
    public override async Task InitializeAsync(ChartData? chartData, IChartOptions chartOptions, string[]? plugins = null)
    {
        if (chartData?.Datasets != null)
        {
            var datasets = chartData.Datasets.OfType<DoughnutChartDataset>();
            var data = new { chartData.Labels, Datasets = datasets };
            await JsRuntime.InvokeVoidAsync($"{_jsObjectName}.initialize", Id, GetChartType(), data, (DoughnutChartOptions)chartOptions, plugins);
        }
    }
    
    /// <inheritdoc />
    public override async Task UpdateAsync(ChartData? chartData, IChartOptions chartOptions)
    {
        if (chartData?.Datasets != null)
        {
            var datasets = chartData.Datasets.OfType<DoughnutChartDataset>();
            var data = new { chartData.Labels, Datasets = datasets };
            await JsRuntime.InvokeVoidAsync($"{_jsObjectName}.update", Id, GetChartType(), data, (DoughnutChartOptions)chartOptions);
        }
    }
    

    #endregion
}
