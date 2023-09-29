namespace BlazorBootstrap;

public class BlazorBootstrapChart : BlazorBootstrapComponentBase, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlazorBootstrapComponentBase" /> class.
    /// </summary>
    public BlazorBootstrapChart()
    {
        ContainerStyleBuilder = new CssStyleBuilder(BuildContainerStyles);
    }

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
    /// <param name="plugins"></param>
    public virtual async Task InitializeAsync(ChartData chartData, IChartOptions chartOptions, string[] plugins = null)
    {
        if (chartData is not null && chartData.Datasets is not null && chartData.Datasets.Any())
        {
            var _data = GetChartDataObject(chartData);

            if (chartType == ChartType.Bar)
                await JS.InvokeVoidAsync("window.blazorChart.bar.initialize", ElementId, GetChartType(), _data, (BarChartOptions)chartOptions, plugins);
            else if (chartType == ChartType.Line)
                await JS.InvokeVoidAsync("window.blazorChart.line.initialize", ElementId, GetChartType(), _data, (LineChartOptions)chartOptions, plugins);
            else
                await JS.InvokeVoidAsync("window.blazorChart.initialize", ElementId, GetChartType(), _data, chartOptions, plugins);
        }
    }

    //public async Task Render() { }

    //public async Task Reset() { }

    /// <summary>
    /// Resize the chart.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public async Task ResizeAsync(string width, string height) => await JS.InvokeVoidAsync("window.blazorChart.resize", ElementId, width, height);

    /// <summary>
    /// Update chart.
    /// </summary>
    /// <param name="chartData"></param>
    /// <param name="chartOptions"></param>
    /// <param name="plugins"></param>
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

    protected virtual void BuildContainerStyles(CssStyleBuilder builder)
    {
        if (!string.IsNullOrWhiteSpace(Width) || !string.IsNullOrWhiteSpace(Height))
            builder.Append("position:relative");

        if (!string.IsNullOrWhiteSpace(Width))
            builder.Append($"width:{Width}");

        if (!string.IsNullOrWhiteSpace(Height))
            builder.Append($"height:{Height}");
    }

    /// <inheritdoc />
    public new virtual void Dispose() => Dispose(true);

    /// <inheritdoc />
    public new virtual async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        Dispose(false);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing"></param>
    protected new void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {
                ContainerStyleBuilder = null;
            }

            Disposed = true;
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing"></param>
    protected new ValueTask DisposeAsync(bool disposing)
    {
        try
        {
            if (!AsyncDisposed)
            {
                if (disposing)
                {
                    ContainerStyleBuilder = null;
                }

                AsyncDisposed = true;
            }

            return base.DisposeAsync(disposing);
        }
        catch (Exception exc)
        {
            return new ValueTask(Task.FromException(exc));
        }
    }

    /// <summary>
    /// Gets the style mapper.
    /// </summary>
    protected CssStyleBuilder? ContainerStyleBuilder { get; private set; }

    /// <summary>
    /// Gets the built styles based on all the rules set by the component parameters.
    /// </summary>
    public string? ContainerStyles => ContainerStyleBuilder!.Styles;

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Indicates if the component is already fully disposed (asynchronously).
    /// </summary>
    protected new bool AsyncDisposed { get; private set; }

    /// <summary>
    /// Indicates if the component is already fully disposed.
    /// </summary>
    protected new bool Disposed { get; private set; }

    /// <summary>
    /// Gets or sets chart container height.
    /// </summary>
    [Parameter]
    public string? Height { get; set; }

    /// <summary>
    /// Get or sets chart container width.
    /// </summary>
    [Parameter]
    public string? Width { get; set; }

    #endregion
}
