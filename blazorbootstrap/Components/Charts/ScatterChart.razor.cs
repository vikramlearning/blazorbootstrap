namespace BlazorBootstrap;

public partial class ScatterChart : BlazorBootstrapChart
{
    #region Fields and Constants

    private const string _jsObjectName = "window.blazorChart.scatter";

    private DotNetObjectReference<ScatterChart> objRef;
    
    #endregion

    #region Constructors

    public ScatterChart()
    {
        chartType = ChartType.Scatter;
    }

    #endregion

    #region Methods
    
    protected override async Task OnInitializedAsync() {
        await base.OnInitializedAsync();

        objRef ??= DotNetObjectReference.Create(this);
    }

    // TODO: May be this method is not required
    public override async Task<ChartData> AddDataAsync(ChartData chartData, string dataLabel, IChartDatasetData data)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentNullException(nameof(chartData.Datasets));

        if (data is null)
            throw new ArgumentNullException(nameof(data));

        foreach (var dataset in chartData.Datasets)
            if (dataset is ScatterChartDataset scatterChartDataset && scatterChartDataset.Label == dataLabel)
                if (data is ScatterChartDatasetData scatterChartDatasetData && scatterChartDatasetData.Data is ScatterChartDataPoint scatterChartDataPoint)
                    scatterChartDataset.Data?.Add(scatterChartDataPoint);

        await JSRuntime.InvokeVoidAsync($"{_jsObjectName}.addDatasetData", Id, dataLabel, data);

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
            if (dataset is ScatterChartDataset scatterChartDataset)
            {
                var chartDatasetData = data.FirstOrDefault(x => x is ScatterChartDatasetData scatterChartDatasetData && scatterChartDatasetData.DatasetLabel == scatterChartDataset.Label);

                if (chartDatasetData is ScatterChartDatasetData scatterChartDatasetData && scatterChartDatasetData.Data is ScatterChartDataPoint scatterChartDataPoint)
                    scatterChartDataset.Data?.Add(scatterChartDataPoint);
            }

        await JSRuntime.InvokeVoidAsync($"{_jsObjectName}.addDatasetsData", Id, dataLabel, data?.Select(x => (ScatterChartDatasetData)x));

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

        if (chartDataset is ScatterChartDataset)
        {
            chartData.Datasets.Add(chartDataset);
            await JSRuntime.InvokeVoidAsync($"{_jsObjectName}.addDataset", Id, (ScatterChartDataset)chartDataset);
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

        var datasets = chartData.Datasets.OfType<ScatterChartDataset>();
        var data = new { chartData.Labels, Datasets = datasets };
        await JSRuntime.InvokeVoidAsync($"{_jsObjectName}.initialize", Id, GetChartType(), data, (ScatterChartOptions)chartOptions, plugins, objRef);
    }

    public override async Task UpdateAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentException("chartData.Datasets must not be null", nameof(chartData));

        if (chartOptions is null)
            throw new ArgumentNullException(nameof(chartOptions));

        var datasets = chartData.Datasets.OfType<ScatterChartDataset>();
        var data = new { chartData.Labels, Datasets = datasets };
        await JSRuntime.InvokeVoidAsync($"{_jsObjectName}.update", Id, GetChartType(), data, (ScatterChartOptions)chartOptions);
    }

    #endregion
}
