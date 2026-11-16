using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;
using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// One series of bars for the usage page, in the colors of <see cref="AiChartTheme" />. It draws after its first render,
/// redraws when the theme changes (<see cref="AiState.ThemeChanged" />), and redraws when the page passes other values (a
/// message sent, the day moving on). The chart is a picture (<c>role="img"</c> with <see cref="AriaLabel" />); the page
/// gives the same figures in words and in a table.
/// </summary>
public partial class UsageChart : ComponentBase, IDisposable
{
    #region Fields and Constants

    private BarChart chart = default!;

    private bool chartReady;

    private bool disposed;

    /// <summary>The theme the chart was last drawn in.</summary>
    private bool drawnDark;

    /// <summary>The labels and values the chart was last drawn with.</summary>
    private (string[] Labels, double[] Values) drawn = (Array.Empty<string>(), Array.Empty<double>());

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            drawnDark = State.IsDarkTheme;
            drawn = (Labels.ToArray(), Values.ToArray());
            await chart.InitializeAsync(Data(), Options());
            chartReady = true;
        }

        // The theme switcher can report the color mode while the chart is initializing, and values can change meanwhile.
        if (chartReady && !disposed && IsStale)
            await RedrawAsync();

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized() => State.ThemeChanged += OnThemeChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to theme changes.</summary>
    public void Dispose()
    {
        disposed = true;
        State.ThemeChanged -= OnThemeChanged;
    }

    #endregion

    #region Private Methods

    private ChartData Data()
    {
        var color = AiChartTheme.Series(State.IsDarkTheme)[SeriesIndex % AiChartTheme.Series(State.IsDarkTheme).Count];

        return new ChartData
        {
            Labels = Labels.ToList(),
            Datasets = new List<IChartDataset>
            {
                new BarChartDataset
                {
                    Label = SeriesLabel,
                    Data = Values.Select(value => (double?)value).ToList(),
                    BackgroundColor = new List<string> { color },
                    BorderColor = new List<string> { color },
                    BorderWidth = new List<double> { 0 },
                    BorderRadius = new List<double> { 4 },
                    MaxBarThickness = 24,
                },
            },
        };
    }

    private void OnThemeChanged() => InvokeAsync(async () =>
    {
        if (chartReady && !disposed)
            await RedrawAsync();
    });

    private BarChartOptions Options()
    {
        var options = new BarChartOptions { Responsive = true, MaintainAspectRatio = false };

        AiChartTheme.Apply(options, State.IsDarkTheme);
        options.Plugins.Legend.Display = false;

        return options;
    }

    private async Task RedrawAsync()
    {
        drawnDark = State.IsDarkTheme;
        drawn = (Labels.ToArray(), Values.ToArray());

        try
        {
            await chart.UpdateAsync(Data(), Options());
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
        {
            // The page or circuit went away while redrawing.
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the chart's text alternative: what it shows, in a sentence.</summary>
    [Parameter, EditorRequired] public string AriaLabel { get; set; } = string.Empty;

    /// <summary>Gets or sets the chart height, in pixels.</summary>
    [Parameter] public int Height { get; set; } = 260;

    /// <summary>Gets or sets the label under each bar.</summary>
    [Parameter, EditorRequired] public IReadOnlyList<string> Labels { get; set; } = Array.Empty<string>();

    /// <summary>Gets or sets which color of <see cref="AiChartTheme.Series" /> the bars use.</summary>
    [Parameter] public int SeriesIndex { get; set; }

    /// <summary>Gets or sets the name of the series (shown in the tooltip).</summary>
    [Parameter, EditorRequired] public string SeriesLabel { get; set; } = string.Empty;

    /// <summary>Gets or sets the value of each bar, in the order of <see cref="Labels" />.</summary>
    [Parameter, EditorRequired] public IReadOnlyList<double> Values { get; set; } = Array.Empty<double>();

    private bool IsStale => drawnDark != State.IsDarkTheme || !drawn.Labels.SequenceEqual(Labels) || !drawn.Values.SequenceEqual(Values);

    [Inject] private AiState State { get; set; } = default!;

    #endregion
}
