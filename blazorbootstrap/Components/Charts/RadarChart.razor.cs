namespace BlazorBootstrap;

public partial class RadarChart : BlazorBootstrapChart
{
    #region Fields and Constants

    private const string _jsObjectName = "window.blazorChart.radar";

    private DotNetObjectReference<RadarChart> objRef;
    
    #endregion

    #region Constructors

    public RadarChart()
    {
        chartType = ChartType.Radar;
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
            throw new ArgumentException("chartData.Datasets must not be null", nameof(chartData));

        if (data is null)
            throw new ArgumentNullException(nameof(data));

        foreach (var dataset in chartData.Datasets)
            if (dataset is RadarChartDataset radarChartDataset && radarChartDataset.Label == dataLabel)
                if (data is RadarChartDatasetData radarChartDatasetData)
                    radarChartDataset.Data?.Add(radarChartDatasetData.Data as double?);

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
            throw new Exception($"{nameof(data)} cannot be empty.");

        if (chartData.Datasets.Count != data.Count)
            throw new InvalidDataException("The chart dataset count and the new data points count do not match.");

        if (chartData.Labels.Contains(dataLabel))
            throw new Exception($"{dataLabel} already exists.");

        chartData.Labels.Add(dataLabel);

        foreach (var dataset in chartData.Datasets)
            if (dataset is RadarChartDataset radarChartDataset)
            {
                var chartDatasetData = data.FirstOrDefault(x => x is RadarChartDatasetData radarChartDatasetData && radarChartDatasetData.DatasetLabel == radarChartDataset.Label);

                if (chartDatasetData is RadarChartDatasetData radarChartDatasetData)
                    radarChartDataset.Data?.Add(radarChartDatasetData.Data as double?);
            }

        await JSRuntime.InvokeVoidAsync($"{_jsObjectName}.addDatasetsData", Id, dataLabel, data?.Select(x => (RadarChartDatasetData)x));

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

        if (chartDataset is RadarChartDataset)
        {
            chartData.Datasets.Add(chartDataset);
            await JSRuntime.InvokeVoidAsync($"{_jsObjectName}.addDataset", Id, (RadarChartDataset)chartDataset);
        }

        return chartData;
    }

    public override async Task InitializeAsync(ChartData chartData, IChartOptions chartOptions, string[]? plugins = null)
    {
        if (chartData is not null && chartData.Datasets is not null)
        {
            var datasets = chartData.Datasets.OfType<RadarChartDataset>();
            var data = new { chartData.Labels, Datasets = datasets };
            await JSRuntime.InvokeVoidAsync($"{_jsObjectName}.initialize", Id, GetChartType(), data, (RadarChartOptions)chartOptions, plugins, objRef);
        }
    }

    public override async Task UpdateAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is not null && chartData.Datasets is not null)
        {
            var datasets = chartData.Datasets.OfType<RadarChartDataset>();
            var data = new { chartData.Labels, Datasets = datasets };
            await JSRuntime.InvokeVoidAsync($"{_jsObjectName}.update", Id, GetChartType(), data, (RadarChartOptions)chartOptions);
        }
    }

    #endregion
}
