using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;
using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>One series of bars of a <see cref="ForecastChart" />.</summary>
/// <param name="Label">The name of the series (in the legend and tooltip).</param>
/// <param name="Values">The value of each bar, in the order of the chart's labels.</param>
/// <param name="SeriesIndex">Which color of <see cref="PbChartTheme.Series" /> the bars use; ignored when <paramref name="Muted" />.</param>
/// <param name="Muted">Whether the bars are a target or comparison, in <see cref="PbChartTheme.Muted" />.</param>
public sealed record ForecastChartSeries(string Label, IReadOnlyList<double> Values, int SeriesIndex = 0, bool Muted = false);

/// <summary>
/// Grouped bars for the forecast page, in the colors of <see cref="PbChartTheme" />, with a legend. It draws after its first
/// render, redraws when the theme changes (<see cref="PbState.ThemeChanged" />), and redraws when the page passes other
/// values (a deal won, the month moving on). The chart is a picture (<c>role="img"</c> with <see cref="AriaLabel" />); the
/// page gives the same figures in words and in a table.
/// </summary>
public partial class ForecastChart : ComponentBase, IDisposable
{
    #region Fields and Constants

    private BarChart chart = default!;

    private bool chartReady;

    private bool disposed;

    /// <summary>The theme the chart was last drawn in.</summary>
    private bool drawnDark;

    /// <summary>The labels and values the chart was last drawn with, as one string.</summary>
    private string drawnSignature = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            drawnDark = State.IsDarkTheme;
            drawnSignature = Signature;

            try
            {
                await chart.InitializeAsync(Data(), Options());
                chartReady = true;
            }
            catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException or TaskCanceledException)
            {
                // The page is leaving, or the circuit closed.
            }
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
        var isDark = State.IsDarkTheme;
        var palette = PbChartTheme.Series(isDark);

        return new ChartData
        {
            Labels = Labels.ToList(),
            Datasets = Series.Select(series =>
            {
                var color = series.Muted ? PbChartTheme.Muted(isDark) : palette[series.SeriesIndex % palette.Count];

                return (IChartDataset)new BarChartDataset
                {
                    Label = series.Label,
                    Data = series.Values.Select(value => (double?)value).ToList(),
                    BackgroundColor = new List<string> { color },
                    BorderColor = new List<string> { color },
                    BorderWidth = new List<double> { 0 },
                    BorderRadius = new List<double> { 4 },
                    MaxBarThickness = 18,
                };
            }).ToList(),
        };
    }

    private void OnThemeChanged() => InvokeAsync(async () =>
    {
        if (chartReady && !disposed)
            await RedrawAsync();
    });

    private BarChartOptions Options()
    {
        var options = new BarChartOptions
        {
            Responsive = true,
            MaintainAspectRatio = false,
            Locale = "en-US",
            Interaction = new Interaction { Mode = InteractionMode.Index, Intersect = false },
        };

        PbChartTheme.Apply(options, State.IsDarkTheme);

        return options;
    }

    private async Task RedrawAsync()
    {
        drawnDark = State.IsDarkTheme;
        drawnSignature = Signature;

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
    [Parameter] public int Height { get; set; } = 280;

    /// <summary>Gets or sets the label under each group of bars.</summary>
    [Parameter, EditorRequired] public IReadOnlyList<string> Labels { get; set; } = Array.Empty<string>();

    /// <summary>Gets or sets the series, each with one value per label.</summary>
    [Parameter, EditorRequired] public IReadOnlyList<ForecastChartSeries> Series { get; set; } = Array.Empty<ForecastChartSeries>();

    private bool IsStale => drawnDark != State.IsDarkTheme || drawnSignature != Signature;

    /// <summary>The labels, series names, and values as one string, to tell when they changed.</summary>
    private string Signature =>
        string.Join("|", Labels) + "#" + string.Join("#", Series.Select(series => series.Label + ":" + string.Join(",", series.Values.Select(value => value.ToString(System.Globalization.CultureInfo.InvariantCulture)))));

    [Inject] private PbState State { get; set; } = default!;

    #endregion
}
