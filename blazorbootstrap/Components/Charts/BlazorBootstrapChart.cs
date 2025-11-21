namespace BlazorBootstrap;

public class BlazorBootstrapChart : BlazorBootstrapComponentBase, IDisposable, IAsyncDisposable
{
    #region Fields and Constants

    internal ChartType chartType;
    
    protected DotNetObjectReference<BlazorBootstrapChart> objRef;

    #endregion

    #region Methods

    //public async Task Stop() { }

    //public async Task ToBase64Image() { }

    //public async Task ToBase64Image(string type, double quality) { }

    public virtual async Task<ChartData> AddDataAsync(ChartData chartData, string dataLabel, IChartDatasetData data) => await Task.FromResult(chartData);

    public virtual async Task<ChartData> AddDataAsync(ChartData chartData, string dataLabel, IReadOnlyCollection<IChartDatasetData> data) => await Task.FromResult(chartData);

    public virtual async Task<ChartData> AddDatasetAsync(ChartData chartData, IChartDataset chartDataset, IChartOptions chartOptions) => await Task.FromResult(chartData);

    /// <inheritdoc />
    public new virtual void Dispose() => Dispose(true);

    /// <inheritdoc />
    public new virtual async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore(true);
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

            var dotNetReference = DotNetObjectReference.Create(this);
            if (dotNetReference is null) {
                Console.WriteLine("ERROR!");
            } else {
                Console.WriteLine("SUCCESS!");
            }
            if (chartType == ChartType.Bar)
                await JSRuntime.InvokeVoidAsync("window.blazorChart.bar.initialize", Id, GetChartType(), _data, (BarChartOptions)chartOptions, plugins, objRef);
            else if (chartType == ChartType.Line)
                await JSRuntime.InvokeVoidAsync("window.blazorChart.line.initialize", Id, GetChartType(), _data, (LineChartOptions)chartOptions, plugins, dotNetReference);
            else
                await JSRuntime.InvokeVoidAsync("window.blazorChart.initialize", Id, GetChartType(), _data, chartOptions, plugins, dotNetReference);
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
        await JSRuntime.InvokeVoidAsync("window.blazorChart.resize", Id, widthWithUnit, heightWithUnit);
    }

    /// <summary>
    /// Update chart by reapplying all chart data and options.
    /// If animation is enabled, this will animate the datasets from scratch.
    /// </summary>
    /// <param name="chartData"></param>
    /// <param name="chartOptions"></param>
    public virtual async Task UpdateAsync(ChartData chartData, IChartOptions chartOptions)
    {
        if (chartData is not null && chartData.Datasets is not null && chartData.Datasets.Any())
        {
            var data = GetChartDataObject(chartData);

            if (chartType == ChartType.Bar)
                await JSRuntime.InvokeVoidAsync("window.blazorChart.bar.update", Id, GetChartType(), data, (BarChartOptions)chartOptions, objRef);
            else if (chartType == ChartType.Line)
                await JSRuntime.InvokeVoidAsync("window.blazorChart.line.update", Id, GetChartType(), data, (LineChartOptions)chartOptions, objRef);
            else
                await JSRuntime.InvokeVoidAsync("window.blazorChart.update", Id, GetChartType(), data, chartOptions, objRef);
        }
    }

    /// <summary>
    /// Update only data labels and values. If animation is enabled, this will animate the datapoints.
    /// Changes to the options will not be applied.
    /// </summary>
    /// <param name="chartData">The updated chart data. Only dataset labels and values will be applied.</param>
    public virtual async Task UpdateValuesAsync(ChartData chartData)
    {
        if (chartData is not null && chartData.Datasets is not null && chartData.Datasets.Any())
        {
            var data = GetChartDataObject(chartData);

            if (chartType == ChartType.Bar)
                await JSRuntime.InvokeVoidAsync("window.blazorChart.bar.updateDataValues", Id, data);
            else if (chartType == ChartType.Line)
                await JSRuntime.InvokeVoidAsync("window.blazorChart.line.updateDataValues", Id, data);
            else
                await JSRuntime.InvokeVoidAsync("window.blazorChart.updateDataValues", Id, data);
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
            style += $"width:{Width.Value.ToString(CultureInfo.InvariantCulture)}{WidthUnit.ToCssString()};";

        if (Height > 0)
            style += $"height:{Height.Value.ToString(CultureInfo.InvariantCulture)}{HeightUnit.ToCssString()};";

        return style;
    }

    private object GetChartDataObject(ChartData chartData)
    {
        var datasets = new List<object>();

        if (chartData?.Datasets?.Any() ?? false)
            foreach (var dataset in chartData.Datasets)
                if (dataset is BarChartDataset)
                    datasets.Add((BarChartDataset)dataset);
                else if (dataset is DoughnutChartDataset)
                    datasets.Add((DoughnutChartDataset)dataset);
                else if (dataset is LineChartDataset)
                    datasets.Add((LineChartDataset)dataset);
                else if (dataset is PieChartDataset)
                    datasets.Add((PieChartDataset)dataset);
                else if (dataset is PolarAreaChartDataset)
                    datasets.Add((PolarAreaChartDataset)dataset);

        var data = new { chartData?.Labels, Datasets = datasets };

        return data;
    }

    [JSInvokable]
    public async Task ClickEvent(string item, int index) {
        await OnClick.InvokeAsync(new ChartClickArgs(item, index));
    }

    #endregion

    #region Properties, Indexers

    internal string ContainerStyle => GetChartContainerSizeAsStyle();

    /// <summary>
    /// Gets or sets chart container height.
    /// The default unit of measure is <see cref="Unit.Px" />.
    /// To change the unit of measure see <see cref="HeightUnit" />.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets chart container height unit of measure.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Unit.Px" />.
    /// </remarks>
    [Parameter]
    public Unit HeightUnit { get; set; } = Unit.Px;

    /// <summary>
    /// Get or sets chart container width.
    /// The default unit of measure is <see cref="Unit.Px" />.
    /// To change the unit of measure see <see cref="WidthUnit" />.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets chart container width unit of measure.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Unit.Px" />.
    /// </remarks>
    [Parameter]
    public Unit WidthUnit { get; set; } = Unit.Px;

    
    [Parameter]
    public EventCallback<ChartClickArgs> OnClick { get; set; }
    #endregion
}
