using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;
using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Overview: active incident alerts, totals, live traffic and latency charts, and every service's health. A timer adds a
/// sample every two seconds while the page is open (unless paused) and is disposed with the page.
/// </summary>
public partial class Overview : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The services the latency chart prefers; admins can delete services, so only the ones that still exist are used.</summary>
    private static readonly string[] preferredLatencyServices = { "api-gateway", "search", "payments" };

    private bool chartsReady;

    private bool disposed;

    private LineChart latencyChart = default!;

    private MonitoringLiveTimer? timer;

    private LineChart trafficChart = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var initializedDark = State.IsDarkTheme;

            await trafficChart.InitializeAsync(TrafficData(), MonitoringChartTheme.LineOptions(State.IsDarkTheme, "req/s", showLegend: false));
            await latencyChart.InitializeAsync(LatencyData(), MonitoringChartTheme.LineOptions(State.IsDarkTheme, "ms", showLegend: true));
            chartsReady = true;

            // The theme switcher can report the color mode while the charts are still initializing; catch up once ready.
            if (State.IsDarkTheme != initializedDark)
                await RedrawChartsAsync();

            if (!disposed)
                timer = new MonitoringLiveTimer(TimeSpan.FromSeconds(MonitoringSeedData.TickSeconds), OnTickAsync);
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

    /// <summary>Stops the live timer and the event subscriptions, so leaving the page stops all updates.</summary>
    public void Dispose()
    {
        disposed = true;
        timer?.Dispose();
        State.Changed -= OnStateChanged;
        State.ThemeChanged -= OnThemeChanged;
    }

    #endregion

    #region Private Methods

    private ChartData LatencyData()
    {
        var colors = MonitoringChartTheme.Series(State.IsDarkTheme);
        var datasets = LatencyServices
            .Select((id, index) => (IChartDataset)MonitoringChartTheme.Line(State.FindService(id)!.Name, State.History(id).Select(sample => (double?)Math.Round(sample.LatencyMs)), colors[index]))
            .ToList();

        return new ChartData { Labels = Labels(), Datasets = datasets };
    }

    private List<string> Labels() => State.History(TrafficService.Id).Select(sample => MonitoringDisplay.Time(sample.Time)).ToList();

    /// <summary>Incident changes rebuild the metrics, so the charts redraw too (also while live updates are paused).</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        StateHasChanged();
        await RedrawChartsAsync();
    });

    private void OnThemeChanged() => InvokeAsync(RedrawChartsAsync);

    private async Task OnTickAsync()
    {
        if (disposed || State.IsPaused)
            return;

        try
        {
            await InvokeAsync(async () =>
            {
                if (disposed)
                    return;

                State.Tick();
                StateHasChanged();
                await RedrawChartsAsync();
            });
        }
        catch (Exception ex) when (ex is JSDisconnectedException or ObjectDisposedException or TaskCanceledException)
        {
            // The page or circuit went away between ticks.
        }
    }

    private async Task RedrawChartsAsync()
    {
        if (!chartsReady || disposed)
            return;

        var isDark = State.IsDarkTheme;

        await trafficChart.UpdateAsync(TrafficData(), MonitoringChartTheme.LineOptions(isDark, "req/s", showLegend: false));
        await latencyChart.UpdateAsync(LatencyData(), MonitoringChartTheme.LineOptions(isDark, "ms", showLegend: true));
    }

    private ChartData TrafficData() => new()
    {
        Labels = Labels(),
        Datasets = new List<IChartDataset>
        {
            MonitoringChartTheme.Line(TrafficService.Name, State.History(TrafficService.Id).Select(sample => (double?)Math.Round(sample.Requests)), MonitoringChartTheme.Series(State.IsDarkTheme)[0], fill: true),
        },
    };

    #endregion

    #endregion

    #region Properties, Indexers

    private IReadOnlyList<MonitoringIncident> ActiveIncidents => State.Incidents.Where(incident => incident.Status != MonitoringIncidentStatus.Resolved).ToList();

    /// <summary>The API services' error rate, weighted by their traffic.</summary>
    private double ApiErrorRate
    {
        get
        {
            var apis = State.Services.Where(service => service.Kind == MonitoringServiceKind.Api).Select(service => State.Latest(service.Id)).ToList();
            var total = apis.Sum(sample => sample.Requests);

            return total == 0 ? 0 : apis.Sum(sample => sample.ErrorRate * sample.Requests) / total;
        }
    }

    private MonitoringSample Gateway => State.Latest(TrafficService.Id);

    /// <summary>The services in the latency chart: the preferred ones that still exist, else the first three.</summary>
    private IReadOnlyList<string> LatencyServices
    {
        get
        {
            var ids = preferredLatencyServices.Where(id => State.FindService(id) is not null).ToList();

            return ids.Count > 0 ? ids : State.Services.Take(3).Select(service => service.Id).ToList();
        }
    }

    /// <summary>The service in the traffic chart and tile: the API gateway, else the first API, else the first service.</summary>
    private MonitoringService TrafficService =>
        State.FindService("api-gateway") ?? State.Services.FirstOrDefault(service => service.Kind == MonitoringServiceKind.Api) ?? State.Services[0];

    private string LatencyLabel => "Line chart of p95 latency over the last minute. Latest: "
        + string.Join(", ", LatencyServices.Select(id => $"{State.FindService(id)!.Name} {MonitoringDisplay.Latency(State.Latest(id).LatencyMs)}")) + ".";

    private int OperationalCount => State.Services.Count(service => State.Status(service) == MonitoringStatus.Operational);

    [Inject] private MonitoringState State { get; set; } = default!;

    private string TrafficLabel => $"Line chart of {TrafficService.Name} traffic over the last minute. Latest: {MonitoringDisplay.Number(Gateway.Requests)} requests per second.";

    #endregion
}
