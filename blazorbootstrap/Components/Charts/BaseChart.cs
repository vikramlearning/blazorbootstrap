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

    public async Task Initialize(ChartData data, ChartOptions options)
    {
        await JS.InvokeVoidAsync("window.blazorChart.initialize", ElementId, GetChartType(), data, options);
    }

    public async Task Render() { }

    public async Task Reset() { }

    public async Task Resize(int width, int height)
    {
        await JS.InvokeVoidAsync("window.blazorChart.resize", ElementId, width, height);
    }

    public async Task Stop() { }

    public async Task ToBase64Image() { }

    public async Task ToBase64Image(string type, double quality) { }

    public async Task Update(ChartData data, ChartOptions options)
    {
        await JS.InvokeVoidAsync("window.blazorChart.update", ElementId, GetChartType(), data, options);
    }

    private string GetChartType()
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

public enum ChartType
{
    Line,
    Bar,
    Pie,
    Doughnut,
    PolarArea,
    Radar,
    Scatter,
    Bubble,
}

public class ChartData
{
    public List<string> Labels { get; set; }
    public List<ChartDataset> Datasets { get; set; }
}

public class ChartOptions
{

}

public class ChartLabel { }

public class ChartDataset
{
    public string Label { get; set; }
    public List<int> Data { get; set; }
    public List<string> BackgroundColor { get; set; }
    public List<string> BorderColor { get; set; }
    public double BorderWidth { get; set; }
}

public record ChartRGB(int R, int G, int B);

public record ChartRGBA(int R, int G, int B, double A);