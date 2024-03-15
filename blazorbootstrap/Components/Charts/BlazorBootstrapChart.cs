namespace BlazorBootstrap;

public class BlazorBootstrapChart : BlazorBootstrapComponentBase, IDisposable, IAsyncDisposable
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

    /// <inheritdoc />
    public new virtual void Dispose() => Dispose(true);

    /// <inheritdoc />
    public new virtual async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        Dispose(false);
    }

    //public async Task Clear() { }

    /// <summary>
    /// Initialize Bar Chart.
    /// </summary>
    /// <param name="chartData"></param>
    /// <param name="chartOptions"></param>
    /// <param name="plugins"></param>
    public virtual async Task InitializeAsync(ChartData chartData, IChartOptions chartOptions, string[]? plugins = null)
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
    /// <param name="widthUnit"></param>
    /// <param name="heightUnit"></param>
    public async Task ResizeAsync(int width, int height, Unit widthUnit = Unit.Px, Unit heightUnit = Unit.Px)
    {
        var widthWithUnit = $"width:{width.ToString(CultureInfo.InvariantCulture)}{widthUnit.ToCssString()}";
        var heightWithUnit = $"height:{height.ToString(CultureInfo.InvariantCulture)}{heightUnit.ToCssString()}";
        await JS.InvokeVoidAsync("window.blazorChart.resize", ElementId, widthWithUnit, heightWithUnit);
    }

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
    
    private string GetChartContainerSizeAsStyle()
    {
        var style = "";

        if (Width > 0)
            style += $"width:{Width.Value.ToString(CultureInfo.InvariantCulture)}{WidthUnit.ToCssString()};";

        if (Height > 0)
            style += $"height:{Height.Value.ToString(CultureInfo.InvariantCulture)}{HeightUnit.ToCssString()};";

        return style;
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

        if (chartData?.Datasets?.Any() ?? false)
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

        var data = new { chartData?.Labels, Datasets = datasets };

        return data;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets chart container height.
    /// </summary>
    /// <remarks>
    /// The default unit of measure is <see cref="Unit.Px" />.
    /// To change the unit of measure see <see cref="HeightUnit" />.
    /// </remarks>
    [Parameter]
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets chart container height unit of measure.
    /// </summary>
    [Parameter]
    public Unit HeightUnit { get; set; } = Unit.Px;

    /// <summary>
    /// Get or sets chart container width.
    /// </summary>
    /// <remarks>
    /// The default unit of measure is <see cref="Unit.Px" />.
    /// To change the unit of measure see <see cref="WidthUnit" />.
    /// </remarks>
    [Parameter]
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets chart container width unit of measure.
    /// </summary>
    [Parameter]
    public Unit WidthUnit { get; set; } = Unit.Px;

    internal string ContainerStyle => GetChartContainerSizeAsStyle();

    #endregion
}
