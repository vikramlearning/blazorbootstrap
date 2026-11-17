using System.Globalization;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;
using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>The kind of an <see cref="AdminChart" />.</summary>
public enum AdminChartKind
{
    /// <summary>Vertical bars, one group per label and one bar per series.</summary>
    Bar,

    /// <summary>Horizontal bars, one group per label and one bar per series (for long labels).</summary>
    HorizontalBar,

    /// <summary>A doughnut of the first series, one slice per label.</summary>
    Doughnut,
}

/// <summary>One series of an <see cref="AdminChart" />.</summary>
/// <param name="Label">The name of the series (legend and tooltip).</param>
/// <param name="Values">The value for each label, in the order of the labels.</param>
/// <param name="ColorIndex">The color of <see cref="HrChartTheme.Series" /> it uses (for a doughnut, of its first slice), or -1 for the neutral color.</param>
public sealed record AdminChartSeries(string Label, IReadOnlyList<double> Values, int ColorIndex);

/// <summary>
/// A bar or doughnut chart of Teamhaven's headcount analytics, in the colors of <see cref="HrChartTheme" />. It draws after
/// its first render, redraws when the theme changes (<see cref="HrState.ThemeChanged" />), and redraws when the page passes
/// other labels or values (someone added, someone marked as left, leave approved). The chart is a picture
/// (<c>role="img"</c> with <see cref="AriaLabel" />); the page gives the same figures in words and in a table.
/// </summary>
public partial class AdminChart : ComponentBase, IDisposable
{
    #region Fields and Constants

    private BarChart? bar;

    private bool chartReady;

    private bool disposed;

    private DoughnutChart? doughnut;

    /// <summary>The theme, labels, and values the chart was last drawn with.</summary>
    private string drawn = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            drawn = Signature;

            try
            {
                if (Kind == AdminChartKind.Doughnut)
                    await doughnut!.InitializeAsync(DoughnutData(), DoughnutOptions());
                else
                    await bar!.InitializeAsync(BarData(), BarOptions());

                chartReady = true;
            }
            catch (Exception ex) when (ex is JSException or JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // The page or circuit went away while drawing.
            }
        }

        // The theme switcher can report the color mode while the chart is initializing, and values can change meanwhile.
        if (chartReady && !disposed && drawn != Signature)
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

    private ChartData BarData()
    {
        var isDark = State.IsDarkTheme;
        var colors = HrChartTheme.Series(isDark);

        return new ChartData
        {
            Labels = Labels.ToList(),
            Datasets = Series.Select(series =>
            {
                var color = series.ColorIndex < 0 ? HrChartTheme.Muted(isDark) : colors[series.ColorIndex % colors.Count];

                return (IChartDataset)new BarChartDataset
                {
                    Label = series.Label,
                    Data = series.Values.Select(value => (double?)value).ToList(),
                    BackgroundColor = new List<string> { color },
                    BorderColor = new List<string> { color },
                    BorderWidth = new List<double> { 0 },
                    BorderRadius = new List<double> { 4 },
                    MaxBarThickness = 28,
                };
            }).ToList(),
        };
    }

    private BarChartOptions BarOptions()
    {
        var options = new BarChartOptions
        {
            Responsive = true,
            MaintainAspectRatio = false,
            Interaction = new Interaction { Mode = InteractionMode.Index, Intersect = false },
        };

        if (Kind == AdminChartKind.HorizontalBar)
            options.IndexAxis = "y";

        HrChartTheme.Apply(options, State.IsDarkTheme);
        options.Plugins.Legend.Display = Series.Count > 1;

        // Counts start at zero on the value axis; a horizontal chart shows its category grid lines instead of its value ones.
        var valueAxis = Kind == AdminChartKind.HorizontalBar ? options.Scales.X! : options.Scales.Y!;
        valueAxis.BeginAtZero = true;

        if (Kind == AdminChartKind.HorizontalBar)
        {
            options.Scales.X!.Grid!.Display = true;
            options.Scales.Y!.Grid!.Display = false;
        }

        return options;
    }

    private ChartData DoughnutData()
    {
        var isDark = State.IsDarkTheme;
        var colors = HrChartTheme.Series(isDark);
        var series = Series.FirstOrDefault();
        var start = Math.Max(0, series?.ColorIndex ?? 0);

        return new ChartData
        {
            Labels = Labels.ToList(),
            Datasets = new List<IChartDataset>
            {
                new DoughnutChartDataset
                {
                    Label = series?.Label ?? string.Empty,
                    Data = (series?.Values ?? Array.Empty<double>()).Select(value => (double?)value).ToList(),
                    BackgroundColor = Labels.Select((_, index) => colors[(start + index) % colors.Count]).ToList(),
                    BorderColor = Labels.Select(_ => HrChartTheme.Surface(isDark)).ToList(),
                    BorderWidth = Labels.Select(_ => 3d).ToList(),
                    HoverOffset = Labels.Select(_ => 6d).ToList(),
                },
            },
        };
    }

    private DoughnutChartOptions DoughnutOptions()
    {
        var options = new DoughnutChartOptions { Responsive = true, MaintainAspectRatio = false };
        HrChartTheme.Apply(options, State.IsDarkTheme);

        return options;
    }

    private void OnThemeChanged() => InvokeAsync(async () =>
    {
        if (chartReady && !disposed)
            await RedrawAsync();
    });

    private async Task RedrawAsync()
    {
        drawn = Signature;

        try
        {
            if (Kind == AdminChartKind.Doughnut)
                await doughnut!.UpdateAsync(DoughnutData(), DoughnutOptions());
            else
                await bar!.UpdateAsync(BarData(), BarOptions());
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

    /// <summary>Gets or sets the kind of chart (it can't change after the first render).</summary>
    [Parameter] public AdminChartKind Kind { get; set; } = AdminChartKind.Bar;

    /// <summary>Gets or sets the label of each bar group or slice.</summary>
    [Parameter, EditorRequired] public IReadOnlyList<string> Labels { get; set; } = Array.Empty<string>();

    /// <summary>Gets or sets the series: one or more for bars, the first for a doughnut.</summary>
    [Parameter, EditorRequired] public IReadOnlyList<AdminChartSeries> Series { get; set; } = Array.Empty<AdminChartSeries>();

    /// <summary>Gets the theme, labels, and values as one string, to tell whether the chart needs a redraw.</summary>
    private string Signature =>
        string.Create(CultureInfo.InvariantCulture, $"{State.IsDarkTheme}|{string.Join('\u001f', Labels)}|")
        + string.Join('\u001e', Series.Select(series => series.Label + ":" + series.ColorIndex.ToString(CultureInfo.InvariantCulture) + ":"
                                                        + string.Join(',', series.Values.Select(value => value.ToString("R", CultureInfo.InvariantCulture)))));

    [Inject] private HrState State { get; set; } = default!;

    #endregion
}
