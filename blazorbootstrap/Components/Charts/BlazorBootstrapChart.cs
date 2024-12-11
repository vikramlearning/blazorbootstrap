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
    public async Task ResizeAsync(CssPropertyValue width, CssPropertyValue height)
    {
        var widthWithUnit = $"width:{width.ToString()}";
        var heightWithUnit = $"height:{height.ToString()}";
        await JsRuntime.InvokeVoidAsync("window.blazorChart.resize", Id, widthWithUnit, heightWithUnit);
    }

    /// <summary>
    /// Update chart by reapplying all chart data and options.
    /// If animation is enabled, this will animate the datasets from scratch.
    /// </summary>
    /// <param name="chartData">Data to populate the chart with. If left empty, no update will be invoked.</param>
    /// <param name="chartOptions">Options for chart behavior and styling</param>
    public virtual async Task UpdateAsync(ChartData? chartData, IChartOptions chartOptions)
    {
        if (chartData?.Datasets != null && chartData.Datasets.Any())
        {
            var data = GetChartDataObject(chartData);

            if (ChartType == ChartType.Bar)
                await JsRuntime.InvokeVoidAsync("window.blazorChart.bar.update", Id, GetChartType(), data, (BarChartOptions)chartOptions);
            else if (ChartType == ChartType.Line)
                await JsRuntime.InvokeVoidAsync("window.blazorChart.line.update", Id, GetChartType(), data, (LineChartOptions)chartOptions);
            else
                await JsRuntime.InvokeVoidAsync("window.blazorChart.update", Id, GetChartType(), data, chartOptions);
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

            if (ChartType == ChartType.Bar)
                await JsRuntime.InvokeVoidAsync("window.blazorChart.bar.updateDataValues", Id, data);
            else if (ChartType == ChartType.Line)
                await JsRuntime.InvokeVoidAsync("window.blazorChart.line.updateDataValues", Id, data);
            else
                await JsRuntime.InvokeVoidAsync("window.blazorChart.updateDataValues", Id, data);
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

        if (Width.HasValue)
            style += $"width:{Width.ToString()};";

        if (Height.HasValue)
            style += $"height:{Height.ToString()};";

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
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Height), StringComparison.OrdinalIgnoreCase): Height = (CssPropertyValue?)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Width), StringComparison.OrdinalIgnoreCase): Width = (CssPropertyValue?)parameter.Value; break;
                default: AdditionalAttributes[parameter.Name] = parameter.Value; break;
            }
        }
        
        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers

    internal string ContainerStyle => GetChartContainerSizeAsStyle();

    /// <summary>
    /// Gets or sets chart container height.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public CssPropertyValue? Height { get; set; }
    
    /// <summary>
    /// Get or sets chart container width.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public CssPropertyValue? Width { get; set; }

    /// <summary>
    /// Dependency injected Javascript Runtime
    /// </summary>
    [Inject] protected IJSRuntime JsRuntime { get; set; } = default!;

    #endregion
}
