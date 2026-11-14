using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;
using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Service page: status, the active incident in a <see cref="Callout" />, latest values, live latency and error-rate
/// charts, resource <see cref="Progress" /> bars with <see cref="Tooltip" /> help, and recent logs. A timer adds a sample
/// every two seconds while the page is open (unless paused) and is disposed with the page. An unknown ID renders the
/// site's not-found screen.
/// </summary>
public partial class ServiceDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private bool chartsReady;

    private bool disposed;

    private LineChart errorChart = default!;

    private LineChart latencyChart = default!;

    private string? loadedId;

    private bool needsChartInit;

    private MonitoringLiveTimer? timer;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // A new service (first render, or another service's link) gets new chart components, keyed by service.
        if (needsChartInit && Service is not null)
        {
            needsChartInit = false;
            chartsReady = false;
            var initializedDark = State.IsDarkTheme;

            await latencyChart.InitializeAsync(LatencyData(), MonitoringChartTheme.LineOptions(State.IsDarkTheme, "ms", showLegend: false));
            await errorChart.InitializeAsync(ErrorData(), MonitoringChartTheme.LineOptions(State.IsDarkTheme, "%", showLegend: false));
            chartsReady = true;

            if (State.IsDarkTheme != initializedDark)
                await RedrawChartsAsync();
        }

        if (firstRender && !disposed)
            timer = new MonitoringLiveTimer(TimeSpan.FromSeconds(MonitoringSeedData.TickSeconds), OnTickAsync);

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;
        State.ThemeChanged += OnThemeChanged;
    }

    protected override void OnParametersSet()
    {
        if (loadedId == Id)
            return;

        loadedId = Id;
        needsChartInit = Service is not null;
        chartsReady = false;
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

    private ChartData ErrorData() => new()
    {
        Labels = Labels(),
        Datasets = new List<IChartDataset>
        {
            MonitoringChartTheme.Line("Error rate", State.History(Service!.Id).Select(sample => (double?)Math.Round(sample.ErrorRate, 2)), MonitoringChartTheme.Series(State.IsDarkTheme)[3], fill: true),
        },
    };

    private List<string> Labels() => State.History(Service!.Id).Select(sample => MonitoringDisplay.Time(sample.Time)).ToList();

    private ChartData LatencyData() => new()
    {
        Labels = Labels(),
        Datasets = new List<IChartDataset>
        {
            MonitoringChartTheme.Line("p95 latency", State.History(Service!.Id).Select(sample => (double?)Math.Round(sample.LatencyMs, 1)), MonitoringChartTheme.Series(State.IsDarkTheme)[1], fill: true),
        },
    };

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

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
        if (!chartsReady || disposed || Service is null)
            return;

        await latencyChart.UpdateAsync(LatencyData(), MonitoringChartTheme.LineOptions(State.IsDarkTheme, "ms", showLegend: false));
        await errorChart.UpdateAsync(ErrorData(), MonitoringChartTheme.LineOptions(State.IsDarkTheme, "%", showLegend: false));
    }

    private IEnumerable<(string Label, double Value, string Help)> Resources(MonitoringSample latest)
    {
        yield return ("CPU", latest.Cpu, "Average processor use across instances in the last sample.");
        yield return ("Memory", latest.Memory, "Average memory use across instances in the last sample.");
        yield return ("Disk", Service!.DiskUsed, "Share of provisioned storage in use.");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Overview", Href = MonitoringLayout.Base },
        new() { Text = Service?.Name, IsCurrentPage = true },
    };

    /// <summary>Gets or sets the service ID from the route.</summary>
    [Parameter] public string? Id { get; set; }

    private IReadOnlyList<MonitoringLogEntry> RecentLogs => State.Logs.Where(entry => entry.ServiceId == Service?.Id).Take(8).ToList();

    private MonitoringService? Service => State.FindService(Id);

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion
}
