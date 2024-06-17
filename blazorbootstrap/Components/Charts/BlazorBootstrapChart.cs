namespace BlazorBootstrap;

/// <summary>
/// The abstract base class for all chart components.
/// </summary>
public abstract class BlazorBootstrapChart : BlazorBootstrapComponentBase, IDisposable, IAsyncDisposable
{
    #region Fields and Constants

    internal ChartType ChartType;

    #endregion

    #region Methods

    /// <summary>
    /// Adds data to the chart.
    /// </summary>
    /// <param name="chartData">Data to add to the chart</param>
    /// <param name="dataLabel">Name of the label</param>
    /// <param name="data">Data for front-end population</param>
    /// <returns></returns>
    public virtual Task<ChartData> AddDataAsync(ChartData chartData, string dataLabel, IChartDatasetData data) => Task.FromResult(chartData);
    
    /// <summary>
    /// Adds data to the chart.
    /// </summary>
    /// <param name="chartData">Data to add to the chart</param>
    /// <param name="dataLabel">Name of the label</param>
    /// <param name="data">Data for front-end population</param>
    public virtual Task<ChartData> AddDataAsync(ChartData chartData, string dataLabel, IReadOnlyCollection<IChartDatasetData> data) => Task.FromResult(chartData);

    /// <summary>
    /// Adds data to the chart.
    /// </summary>
    /// <param name="chartData">Data to add to the chart</param>
    /// <param name="chartDataset">Data for front-end population</param>
    /// <param name="chartOptions">Frontend options for chart behavior and styling</param>
    public virtual Task<ChartData> AddDatasetAsync(ChartData chartData, IChartDataset chartDataset, IChartOptions chartOptions) => Task.FromResult(chartData);

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
    /// Initializes the Chart.
    /// </summary>
    /// <param name="chartData">Data to populate the chart with</param>
    /// <param name="chartOptions">Options for behavior and styling of the chart</param>
    /// <param name="plugins">Plugins usage (in JS)</param>
    public virtual async Task InitializeAsync(ChartData chartData, IChartOptions chartOptions, string[]? plugins = null)
    {
        if (chartData?.Datasets != null && chartData.Datasets.Any())
        {
            var data = GetChartDataObject(chartData);

            switch (ChartType)
            {
                case ChartType.Bar:
                    await JsRuntime.InvokeVoidAsync("window.blazorChart.bar.initialize", Id, GetChartType(), data, (BarChartOptions)chartOptions, plugins);
                    break;
                case ChartType.Line:
                    await JsRuntime.InvokeVoidAsync("window.blazorChart.line.initialize", Id, GetChartType(), data, (LineChartOptions)chartOptions, plugins);
                    break;
                default:
                    await JsRuntime.InvokeVoidAsync("window.blazorChart.initialize", Id, GetChartType(), data, chartOptions, plugins);
                    break;
            }
        }
    } 

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
        await JsRuntime.InvokeVoidAsync("window.blazorChart.resize", Id, widthWithUnit, heightWithUnit);
    }

    /// <summary>
    /// Updates the chart with new data.
    /// </summary>
    /// <param name="chartData">Data to populate the chart with. If left empty, no update will be invoked.</param>
    /// <param name="chartOptions">Options for chart behavior and styling</param>
    public virtual async Task UpdateAsync(ChartData? chartData, IChartOptions chartOptions)
    {
        if (chartData?.Datasets != null && chartData.Datasets.Any())
        {
            var data = GetChartDataObject(chartData);

            switch (ChartType)
            {
                case ChartType.Bar:
                    await JsRuntime.InvokeVoidAsync("window.blazorChart.bar.update", Id, GetChartType(), data, (BarChartOptions)chartOptions);
                    break;
                case ChartType.Line:
                    await JsRuntime.InvokeVoidAsync("window.blazorChart.line.update", Id, GetChartType(), data, (LineChartOptions)chartOptions);
                    break;
                default:
                    await JsRuntime.InvokeVoidAsync("window.blazorChart.update", Id, GetChartType(), data, chartOptions);
                    break;
            }
        }
    }

    /// <summary>
    /// Returns the chart type as a string.
    /// </summary> 
    protected string GetChartType() =>
        ChartType switch
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

    private static object GetChartDataObject(ChartData chartData)
    {
        var datasets = new List<IChartDataset>();

        if (chartData?.Datasets?.Any() ?? false)
            foreach (var dataset in chartData.Datasets)
                switch (dataset)
                {
                    case BarChartDataset barChartDataset: datasets.Add(barChartDataset); break;
                    case BubbleChartDataset bubbleChartDataset: datasets.Add(bubbleChartDataset); break;
                    case DoughnutChartDataset doughnutChartDataset: datasets.Add(doughnutChartDataset); break;
                    case LineChartDataset lineChartDataset: datasets.Add(lineChartDataset); break;
                    case PieChartDataset pieChartDataset: datasets.Add(pieChartDataset); break;
                    default: throw new NotImplementedException();
                }

        var data = new { chartData?.Labels, Datasets = datasets };

        return data;
    }

    /// <summary>
    /// Parameters are loaded manually for sake of performance.
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/performance#implement-setparametersasync-manually"/>
    /// </summary> 
    public override Task SetParametersAsync(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(Height): Height = (int?)parameter.Value; break;
                case nameof(HeightUnit): HeightUnit = (Unit)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value; break;
                case nameof(Style): Style = (string)parameter.Value; break;
                case nameof(Width): Width = (int?)parameter.Value; break;
                case nameof(WidthUnit): WidthUnit = (Unit)parameter.Value; break;
                default: AdditionalAttributes![parameter.Name] = parameter.Value; break;
            }
        }
        
        return base.SetParametersAsync(ParameterView.Empty);
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
    /// Default value is <see langword="null" />.
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
    /// Default value is <see langword="null" />.
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

    #endregion
}
