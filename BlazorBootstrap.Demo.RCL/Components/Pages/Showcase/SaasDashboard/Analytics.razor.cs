using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.SaasDashboard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.SaasDashboard;
using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.SaasDashboard;

/// <summary>
/// Analytics screen: a date-range filter that redraws the sessions, sources, and sign-up charts and the top-pages table.
/// </summary>
public partial class Analytics : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly (SaasAnalyticsRange Value, string Text, string Description)[] ranges =
    {
        (SaasAnalyticsRange.Last7Days, "7 days", "the last 7 days"),
        (SaasAnalyticsRange.Last30Days, "30 days", "the last 30 days"),
        (SaasAnalyticsRange.Last90Days, "90 days", "the last 90 days, by week"),
        (SaasAnalyticsRange.Last12Months, "12 months", "the last 12 months, by month"),
    };

    private bool chartsReady;

    private LineChart sessionsChart = default!;

    private BarChart signupsChart = default!;

    private DoughnutChart sourcesChart = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var initializedDark = State.IsDarkTheme;

            await sessionsChart.InitializeAsync(SessionsData(), SessionsOptions());
            await sourcesChart.InitializeAsync(SourcesData(), SourcesOptions());
            await signupsChart.InitializeAsync(SignupsData(), SignupsOptions());
            chartsReady = true;

            // The theme switcher can report the color mode while the charts are still initializing; catch up once ready.
            if (State.IsDarkTheme != initializedDark)
                await RedrawChartsAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;
        State.ThemeChanged += OnThemeChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and theme changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        State.ThemeChanged -= OnThemeChanged;
    }

    #endregion

    #region Private Methods

    private static string Count(double value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static string Percent(double value, string format) => value.ToString(format, CultureInfo.InvariantCulture) + "%";

    private void OnRangeChanged(SaasAnalyticsRange range) => State.AnalyticsRange = range;

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        StateHasChanged();
        await RedrawChartsAsync();
    });

    private void OnThemeChanged() => InvokeAsync(RedrawChartsAsync);

    private async Task RedrawChartsAsync()
    {
        if (!chartsReady)
            return;

        await sessionsChart.UpdateAsync(SessionsData(), SessionsOptions());
        await sourcesChart.UpdateAsync(SourcesData(), SourcesOptions());
        await signupsChart.UpdateAsync(SignupsData(), SignupsOptions());
    }

    private ChartData SessionsData()
    {
        var isDark = State.IsDarkTheme;
        var sessions = new LineChartDataset
        {
            Label = "Sessions",
            Data = Snapshot.Sessions.Select(point => (double?)point.Value).ToList(),
            BorderColor = SaasChartTheme.Primary(isDark),
            BackgroundColor = SaasChartTheme.Primary(isDark, .12),
            BorderWidth = 2.5,
            Tension = .35,
            PointRadius = new List<double> { 0 },
            PointHoverRadius = new List<double> { 5 },
            PointBackgroundColor = new List<string> { SaasChartTheme.Primary(isDark) },
        };
        sessions.FillToOrigin();

        return new ChartData { Labels = Snapshot.Sessions.Select(point => point.Label).ToList(), Datasets = new List<IChartDataset> { sessions } };
    }

    private LineChartOptions SessionsOptions()
    {
        var options = new LineChartOptions
        {
            Responsive = true,
            MaintainAspectRatio = false,
            Interaction = new Interaction { Mode = InteractionMode.Index, Intersect = false },
        };

        SaasChartTheme.Apply(options, State.IsDarkTheme);
        options.Plugins.Legend.Display = false;

        return options;
    }

    private ChartData SignupsData()
    {
        var color = SaasChartTheme.Series(State.IsDarkTheme)[1];

        return new ChartData
        {
            Labels = Snapshot.Signups.Select(point => point.Label).ToList(),
            Datasets = new List<IChartDataset>
            {
                new BarChartDataset
                {
                    Label = "Sign-ups",
                    Data = Snapshot.Signups.Select(point => (double?)point.Value).ToList(),
                    BackgroundColor = new List<string> { color },
                    BorderColor = new List<string> { color },
                    BorderWidth = new List<double> { 0 },
                    BorderRadius = new List<double> { 6 },
                    MaxBarThickness = 28,
                },
            },
        };
    }

    private BarChartOptions SignupsOptions()
    {
        var options = new BarChartOptions { Responsive = true, MaintainAspectRatio = false };

        SaasChartTheme.Apply(options, State.IsDarkTheme);
        options.Plugins.Legend.Display = false;

        return options;
    }

    private ChartData SourcesData()
    {
        var isDark = State.IsDarkTheme;
        var colors = SaasChartTheme.Series(isDark);
        var sources = Snapshot.Sources;

        return new ChartData
        {
            Labels = sources.Select(source => source.Name).ToList(),
            Datasets = new List<IChartDataset>
            {
                new DoughnutChartDataset
                {
                    Label = "Sessions",
                    Data = sources.Select(source => (double?)source.Sessions).ToList(),
                    BackgroundColor = sources.Select((_, index) => colors[index % colors.Count]).ToList(),
                    BorderColor = sources.Select(_ => SaasChartTheme.Surface(isDark)).ToList(),
                    BorderWidth = sources.Select(_ => 3d).ToList(),
                    HoverOffset = sources.Select(_ => 6d).ToList(),
                },
            },
        };
    }

    private DoughnutChartOptions SourcesOptions()
    {
        var options = new DoughnutChartOptions { Responsive = true, MaintainAspectRatio = false };
        SaasChartTheme.Apply(options, State.IsDarkTheme);

        return options;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string CurrentRangeText => ranges.First(range => range.Value == State.AnalyticsRange).Description;

    private string SessionsChartLabel =>
        $"Line chart of sessions over {CurrentRangeText}: lowest {Count(Snapshot.Sessions.Min(point => point.Value))}, highest {Count(Snapshot.Sessions.Max(point => point.Value))}.";

    private string SignupsChartLabel =>
        $"Bar chart of sign-ups over {CurrentRangeText}, {Count(Snapshot.Signups.Sum(point => point.Value))} in total.";

    private SaasAnalyticsSnapshot Snapshot => SaasDashboardSeedData.Analytics(State.AnalyticsRange);

    private string SourcesChartLabel
    {
        get
        {
            var total = Snapshot.Sources.Sum(source => source.Sessions);

            return "Traffic sources: " + string.Join(", ", Snapshot.Sources.Select(source => $"{source.Name} {Percent(source.Sessions / total * 100, "0")}"));
        }
    }

    [Inject] private SaasDashboardState State { get; set; } = default!;

    private IEnumerable<Total> Totals
    {
        get
        {
            var snapshot = Snapshot;
            var sessions = snapshot.Sessions.Sum(point => point.Value);
            var signups = snapshot.Signups.Sum(point => point.Value);

            return new[]
            {
                new Total("Sessions", Count(sessions), "All channels"),
                new Total("Sign-ups", Count(signups), "New workspaces"),
                new Total("Conversion", Percent(signups / sessions * 100, "0.00"), "Sessions to sign-up"),
                new Total("Bounce rate", Percent(snapshot.BounceRatePercent, "0.0"), $"Avg. session {snapshot.AverageSessionMinutes.ToString("0.0", CultureInfo.InvariantCulture)} min"),
            };
        }
    }

    #endregion

    #region Nested Types

    private sealed record Total(string Label, string Value, string Note);

    #endregion
}
