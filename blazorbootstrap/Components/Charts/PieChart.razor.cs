namespace BlazorBootstrap;

public partial class PieChart : BlazorBootstrapChart
{
    #region Constructors

    public PieChart()
    {
        chartType = ChartType.Pie;
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
            if (dataset is PieChartDataset pieChartDataset && pieChartDataset.Label == dataLabel)
                if (data is PieChartDatasetData pieChartDatasetData)
                {
                    pieChartDataset.Data?.Add(pieChartDatasetData.Data);
                    pieChartDataset.BackgroundColor?.Add(pieChartDatasetData.BackgroundColor);
                }

        await JS.InvokeVoidAsync("window.blazorChart.pie.addDatasetData", ElementId, dataLabel, data);

        return chartData;
    }

    public override async Task<ChartData> AddDataAsync(ChartData chartData, string dataLabel, List<IChartDatasetData> data)
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
            if (dataset is PieChartDataset pieChartDataset)
            {
                var chartDatasetData = data.FirstOrDefault(x => x is PieChartDatasetData pieChartDatasetData && pieChartDatasetData.DatasetLabel == pieChartDataset.Label);

                if (chartDatasetData is PieChartDatasetData pieChartDatasetData)
                {
                    pieChartDataset.Data?.Add(pieChartDatasetData.Data);
                    pieChartDataset.BackgroundColor?.Add(pieChartDatasetData.BackgroundColor);
                }
            }

        await JS.InvokeVoidAsync("window.blazorChart.pie.addDatasetsData", ElementId, dataLabel, data?.Select(x => (PieChartDatasetData)x));

        return chartData;
    }

    public override async Task<ChartData> AddDatasetAsync(ChartData chartData, IChartDataset chartDataset, IChartOptions chartOptions)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentNullException(nameof(chartData.Datasets));

        if (chartDataset is null)
            throw new ArgumentNullException(nameof(chartDataset));

        if (chartDataset is PieChartDataset pieChartDataset)
        {
            chartData.Datasets.Add(pieChartDataset);
            await JS.InvokeVoidAsync("window.blazorChart.pie.addDataset", ElementId, pieChartDataset);
        }

        return chartData;
    }

    public override async Task InitializeAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is not null && chartData.Datasets is not null)
        {
            var datasets = chartData.Datasets.OfType<PieChartDataset>();
            var data = new { chartData.Labels, Datasets = datasets };
            await JS.InvokeVoidAsync("window.blazorChart.pie.initialize", ElementId, GetChartType(), data, (PieChartOptions)chartOptions);
        }
    }

    public override async Task UpdateAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is not null && chartData.Datasets is not null)
        {
            var datasets = chartData.Datasets.OfType<PieChartDataset>();
            var data = new { chartData.Labels, Datasets = datasets };
            await JS.InvokeVoidAsync("window.blazorChart.pie.update", ElementId, GetChartType(), data, (PieChartOptions)chartOptions);
        }
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    #endregion
}