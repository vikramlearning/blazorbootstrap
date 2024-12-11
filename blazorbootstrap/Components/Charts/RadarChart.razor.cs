﻿namespace BlazorBootstrap;

/// <summary>
/// Radar charts are a way of showing multiple data points and the variation between them.
/// </summary>
public partial class RadarChart : BlazorBootstrapChart
{
    #region Fields and Constants

    private const string _jsObjectName = "window.blazorChart.radar";

    #endregion

    #region Constructors

    /// <summary>
    /// Default constructor
    /// </summary>
    public RadarChart()
    {
        ChartType = ChartType.Radar;
    }

    #endregion

    #region Methods

    // TODO: May be this method is not required
    /// <inheritdoc />
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

        await JsRuntime.InvokeVoidAsync($"{_jsObjectName}.addDatasetData", Id, dataLabel, data);

        return chartData;
    }

    /// <inheritdoc />
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

        await JsRuntime.InvokeVoidAsync($"{_jsObjectName}.addDatasetsData", Id, dataLabel, data?.Select(x => (RadarChartDatasetData)x));

        return chartData;
    }

    /// <inheritdoc />
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
            await JsRuntime.InvokeVoidAsync($"{_jsObjectName}.addDataset", Id, (RadarChartDataset)chartDataset);
        }

        return chartData;
    }

    /// <inheritdoc />
    public override async Task InitializeAsync(ChartData? chartData, IChartOptions chartOptions, string[]? plugins = null)
    {
        if (chartData?.Datasets != null)
        {
            var datasets = chartData.Datasets.OfType<RadarChartDataset>();
            var data = new { chartData.Labels, Datasets = datasets };
            await JsRuntime.InvokeVoidAsync($"{_jsObjectName}.initialize", Id, GetChartType(), data, (RadarChartOptions)chartOptions, plugins);
        }
    }

    /// <inheritdoc />
    public override async Task UpdateAsync(ChartData? chartData, IChartOptions chartOptions)
    {
        if (chartData?.Datasets != null)
        {
            var datasets = chartData.Datasets.OfType<RadarChartDataset>();
            var data = new { chartData.Labels, Datasets = datasets };
            await JsRuntime.InvokeVoidAsync($"{_jsObjectName}.update", Id, GetChartType(), data, (RadarChartOptions)chartOptions);
        }
    }

    #endregion
}
