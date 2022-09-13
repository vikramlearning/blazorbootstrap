using Microsoft.JSInterop;

namespace BlazorBootstrap;

public class BaseChart : BaseComponent
{
    #region Members

    internal ChartType chartType;

    private DotNetObjectReference<BaseChart> objRef;

    #endregion Members

    #region Methods

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();

        ExecuteAfterRender(async () =>
        {
            var data = new ChartData
            {
                Labels = new List<string> { "Team 1", "Team 2", "Team 3", "Team 4", "Team 5", "Team 6", "Team 7" },
                Datasets = new List<ChartDataset>()
                {
                    new ChartDataset()
                    {
                        Label = "Test 1",
                        Data = new List<int>{ 189, 170, 111, 215, 89, 155, 198},
                        BackgroundColor = new List<string> { "rgb(255, 99, 132)" },
                        BorderColor = new List<string> { "rgb(255, 99, 132)" },
                        BorderWidth = 1,
                    }
                }
            };

            var options = new ChartOptions();

            await JS.InvokeVoidAsync("window.blazorChart.barchart.initialize", ElementId, GetChartType(), data, options);
        });
    }

    public async Task Clear() {  }

    public async Task Render() { }

    public async Task Reset() { }

    public async Task Resize() { }

    public async Task Resize(int width, int height) { }

    public async Task Stop() { }

    public async Task ToBase64Image() { }

    public async Task ToBase64Image(string type, double quality) { }

    public async Task Update() { }

    private string GetChartType()
    {
        return chartType switch
        {
            ChartType.Bar => "bar",
            ChartType.Line => "line",
            //TODO: update this
            _ => ""
        };
    }

    #endregion Methods

    #region Properties

    /// <summary>
    /// Get or sets chart width.
    /// </summary>
    [Parameter] public int Width { get; set; }

    /// <summary>
    /// Gets or sets chart height.
    /// </summary>
    [Parameter] public int Height { get; set; }

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