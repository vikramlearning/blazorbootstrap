using System;

namespace BlazorBootstrap;

public partial class LineChart : BaseChart
{
    #region Constructors

    public LineChart()
    {
        chartType = ChartType.Line;
    }

    #endregion Constructors

    #region Members

    #endregion Members

    #region Methods

    public override async Task<ChartData> AddDataAsync(ChartData chartData, string label, List<double> data)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentNullException(nameof(chartData.Datasets));

        if (label is null)
            throw new ArgumentNullException(nameof(label));

        if (string.IsNullOrWhiteSpace(label))
            throw new Exception($"{nameof(label)} cannot be empty.");

        if (data is null)
            throw new ArgumentNullException(nameof(data));

        if (!data.Any())
            throw new Exception($"{nameof(data)} cannot be empty.");

        if (chartData.Datasets.Count != data.Count)
            throw new InvalidDataException("The chart dataset count and the new data points count do not match.");

        for (int index = 0; index < chartData.Datasets.Count; index++)
        {
            if (chartData.Datasets[index] is LineChartDataset lineChartDataset)
            {
                lineChartDataset.Data?.Add(data[index]);
                chartData.Datasets[index] = lineChartDataset;
            }
        }

        await JS.InvokeVoidAsync("window.blazorChart.line.addData", ElementId, label, data);

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

        if (chartDataset is LineChartDataset)
        {
            chartData.Datasets.Add(chartDataset);
            await JS.InvokeVoidAsync("window.blazorChart.line.addDataset", ElementId, (LineChartDataset)chartDataset);
        }

        return chartData;
    }

    public override async Task InitializeAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentNullException(nameof(chartData.Datasets));

        if (chartOptions is null)
            throw new ArgumentNullException(nameof(chartOptions));

        var datasets = chartData.Datasets.OfType<LineChartDataset>();
        var data = new { chartData.Labels, Datasets = datasets };
        await JS.InvokeVoidAsync("window.blazorChart.line.initialize", ElementId, GetChartType(), data, (LineChartOptions)chartOptions);
    }

    public override async Task UpdateAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is null)
            throw new ArgumentNullException(nameof(chartData));

        if (chartData.Datasets is null)
            throw new ArgumentNullException(nameof(chartData.Datasets));

        if (chartOptions is null)
            throw new ArgumentNullException(nameof(chartOptions));

        var datasets = chartData.Datasets.OfType<LineChartDataset>();
        var data = new { chartData.Labels, Datasets = datasets };
        await JS.InvokeVoidAsync("window.blazorChart.line.update", ElementId, GetChartType(), data, (LineChartOptions)chartOptions);
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    #endregion Properties
}
