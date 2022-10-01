using Microsoft.JSInterop;

namespace BlazorBootstrap;

public class BaseChart : BaseComponent
{
    #region Members

    internal ChartType chartType;

    internal string chartContainerStyle => GetChartContainerSizeAsStyle();

    private DotNetObjectReference<BaseChart> objRef;

    #endregion Members

    #region Methods

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();
    }

    public async Task Clear() { }

    //public virtual async Task InitializeAsync(ChartData data, ChartOptions options)
    //{
    //    if (data is not null && data.Datasets is not null && data.Datasets.Any())
    //    {
    //        var _data = GetChartDataObject(data);

    //        if (chartType == ChartType.Line)
    //            await JS.InvokeVoidAsync("window.blazorChart.line.initialize", ElementId, GetChartType(), _data, options);
    //        else
    //            await JS.InvokeVoidAsync("window.blazorChart.initialize", ElementId, GetChartType(), _data, options);
    //    }
    //}

    public async Task Render() { }

    public async Task Reset() { }

    public async Task Resize(int width, int height)
    {
        await JS.InvokeVoidAsync("window.blazorChart.resize", ElementId, width, height);
    }

    public async Task Stop() { }

    public async Task ToBase64Image() { }

    public async Task ToBase64Image(string type, double quality) { }

    public virtual async Task UpdateAsync(ChartData data, ChartOptions options)
    {
        if (data is not null && data.Datasets is not null && data.Datasets.Any())
        {
            var _data = GetChartDataObject(data);

            if (chartType == ChartType.Line)
                await JS.InvokeVoidAsync("window.blazorChart.line.update", ElementId, GetChartType(), _data, options);
            else
                await JS.InvokeVoidAsync("window.blazorChart.update", ElementId, GetChartType(), _data, options);
        }
    }

    private object GetChartDataObject(ChartData chartData)
    {
        var datasets = new List<object>();
        if (chartData is not null && chartData.Datasets is not null && chartData.Datasets.Any())
        {
            foreach (var dataset in chartData.Datasets)
            {
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
            }
        }

        var data = new
        {
            Labels = chartData.Labels,
            Datasets = datasets
        };

        return data;
    }

    protected string GetChartType()
    {
        return chartType switch
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
    }

    private string GetChartContainerSizeAsStyle()
    {
        var style = "";
        if (Width > 0)
            style += $"width:{Width}px;";

        if (Height > 0)
            style += $"height:{Height}px;";

        return style;
    }

    #endregion Methods

    #region Properties

    /// <summary>
    /// Get or sets chart width.
    /// </summary>
    [Parameter]
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets chart height.
    /// </summary>
    [Parameter]
    public int Height { get; set; }

    #endregion Properties
}