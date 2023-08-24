namespace BlazorBootstrap;

public class BlazorBootstrapChart : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    internal ChartType chartType;

    #endregion

    #region Methods

    //public async Task Stop() { }

    //public async Task ToBase64Image() { }

    //public async Task ToBase64Image(string type, double quality) { }

    public virtual async Task<ChartData> AddDataAsync(ChartData chartData, string dataLabel, IChartDatasetData data) => await Task.FromResult(chartData);

    public virtual async Task<ChartData> AddDataAsync(ChartData chartData, string dataLabel, List<IChartDatasetData> data) => await Task.FromResult(chartData);

    public virtual async Task<ChartData> AddDatasetAsync(ChartData chartData, IChartDataset chartDataset, IChartOptions chartOptions) => await Task.FromResult(chartData);

    //public async Task Clear() { }

    /// <summary>
    /// Initialize Bar Chart.
    /// </summary>
    /// <param name="chartData"></param>
    /// <param name="chartOptions"></param>
    public virtual async Task InitializeAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is not null && chartData.Datasets is not null && chartData.Datasets.Any())
        {
            var _data = GetChartDataObject(chartData);

            if (chartType == ChartType.Bar)
                await JS.InvokeVoidAsync("window.blazorChart.bar.initialize", ElementId, GetChartType(), _data, (BarChartOptions)chartOptions);
            else if (chartType == ChartType.Line)
                await JS.InvokeVoidAsync("window.blazorChart.line.initialize", ElementId, GetChartType(), _data, (LineChartOptions)chartOptions);
            else
                await JS.InvokeVoidAsync("window.blazorChart.initialize", ElementId, GetChartType(), _data, chartOptions);
        }
    }

    //public async Task Render() { }

    //public async Task Reset() { }

    /// <summary>
    /// Resize the chart.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public async Task ResizeAsync(int width, int height) => await JS.InvokeVoidAsync("window.blazorChart.resize", ElementId, width, height);

    /// <summary>
    /// Update chart.
    /// </summary>
    /// <param name="chartData"></param>
    /// <param name="chartOptions"></param>
    public virtual async Task UpdateAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is not null && chartData.Datasets is not null && chartData.Datasets.Any())
        {
            var _data = GetChartDataObject(chartData);

            if (chartType == ChartType.Bar)
                await JS.InvokeVoidAsync("window.blazorChart.bar.update", ElementId, GetChartType(), _data, (BarChartOptions)chartOptions);
            else if (chartType == ChartType.Line)
                await JS.InvokeVoidAsync("window.blazorChart.line.update", ElementId, GetChartType(), _data, (LineChartOptions)chartOptions);
            else
                await JS.InvokeVoidAsync("window.blazorChart.update", ElementId, GetChartType(), _data, chartOptions);
        }
    }

    protected string GetChartType() =>
        chartType switch
        {
            ChartType.Bar => "bar",
            ChartType.Bubble => "bubble",
            ChartType.Doughnut => "doughnut",
            ChartType.Line => "line",
            ChartType.Pie => "pie",
            ChartType.PolarArea => "polarArea",
            ChartType.Radar => "radar",
            ChartType.Scatter => "scatter",
            _ => "line" // default
        };

    private string GetChartContainerSizeAsStyle()
    {
        var style = "";

        if (Width > 0)
            style += $"width:{Width}px;";

        if (Height > 0)
            style += $"height:{Height}px;";

        return style;
    }

    private object GetChartDataObject(ChartData chartData)
    {
        var datasets = new List<object>();

        if (chartData is not null && chartData.Datasets is not null && chartData.Datasets.Any())
            foreach (var dataset in chartData.Datasets)
                if (dataset is BarChartDataset)
                    datasets.Add((BarChartDataset)dataset);
                else if (dataset is BubbleChartDataset)
                    datasets.Add((BubbleChartDataset)dataset);
                else if (dataset is DoughnutChartDataset)
                    datasets.Add((DoughnutChartDataset)dataset);
                else if (dataset is LineChartDataset)
                    datasets.Add((LineChartDataset)dataset);
                else if (dataset is PieChartDataset)
                    datasets.Add((PieChartDataset)dataset);

        var data = new { chartData.Labels, Datasets = datasets };

        return data;
    }

    #endregion

    #region Properties, Indexers

    internal string chartContainerStyle => GetChartContainerSizeAsStyle();

    /// <summary>
    /// Gets or sets chart height.
    /// </summary>
    [Parameter]
    public int Height { get; set; }

    /// <summary>
    /// Get or sets chart width.
    /// </summary>
    [Parameter]
    public int Width { get; set; }

    #endregion
}
