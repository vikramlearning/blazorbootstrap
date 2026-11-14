using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

/// <summary>
/// In-memory state of the monitoring site for the current session: metric history, incidents, log lines, and the chart
/// color mode. <see cref="Tick" /> adds one sample per service; live pages call it from a timer they dispose. Nothing is
/// persisted or sent.
/// </summary>
public sealed class MonitoringState
{
    #region Fields and Constants

    private readonly Dictionary<string, List<MonitoringSample>> history = new();

    private readonly List<MonitoringIncident> incidents = MonitoringSeedData.Incidents();

    private readonly List<MonitoringLogEntry> logs;

    private bool isDarkTheme;

    private int nextLogId = 10_001;

    private int tick;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh session with a full chart window ending at tick 0.</summary>
    public MonitoringState()
    {
        logs = MonitoringSeedData.Logs(Services);

        foreach (var service in Services)
            history[service.Id] = Enumerable.Range(-(MonitoringSeedData.HistoryLength - 1), MonitoringSeedData.HistoryLength).Select(t => Sample(service, t)).ToList();
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Acknowledges an open incident.</summary>
    public void Acknowledge(MonitoringIncident incident)
    {
        if (incident.Status != MonitoringIncidentStatus.Open)
            return;

        incident.Status = MonitoringIncidentStatus.Acknowledged;
        incident.Updates.Add(new MonitoringIncidentUpdate(CurrentTime, MonitoringSeedData.UserName, "Acknowledged. Investigating."));
        Changed?.Invoke();
    }

    /// <summary>Returns the active incident of a service, if any.</summary>
    public MonitoringIncident? ActiveIncident(string serviceId) =>
        incidents.FirstOrDefault(incident => incident.ServiceId == serviceId && incident.Status != MonitoringIncidentStatus.Resolved);

    /// <summary>Finds an incident by its number, or returns <see langword="null" />.</summary>
    public MonitoringIncident? FindIncident(int incidentId) => incidents.FirstOrDefault(incident => incident.Id == incidentId);

    /// <summary>Finds a service by its ID, or returns <see langword="null" />.</summary>
    public MonitoringService? FindService(string? serviceId) => Services.FirstOrDefault(service => service.Id == serviceId);

    /// <summary>Returns the chart window of a service, oldest first.</summary>
    public IReadOnlyList<MonitoringSample> History(string serviceId) => history[serviceId];

    /// <summary>Returns the latest sample of a service.</summary>
    public MonitoringSample Latest(string serviceId) => history[serviceId][^1];

    /// <summary>Resolves an incident; the service's metrics return to their normal ranges from the next sample.</summary>
    public void Resolve(MonitoringIncident incident)
    {
        if (incident.Status == MonitoringIncidentStatus.Resolved)
            return;

        incident.Status = MonitoringIncidentStatus.Resolved;
        incident.ResolvedAt = CurrentTime;
        incident.Updates.Add(new MonitoringIncidentUpdate(CurrentTime, MonitoringSeedData.UserName, "Mitigated and resolved. Metrics are back in their normal range."));

        var service = FindService(incident.ServiceId);
        if (service is not null)
            logs.Insert(0, new MonitoringLogEntry(nextLogId++, CurrentTime, service.Id, MonitoringLogLevel.Info, $"Incident #{incident.Id} resolved; {service.Name} back to normal", "-"));

        Changed?.Invoke();
    }

    /// <summary>
    /// Adds the log lines written since the last refresh: three lines at the latest sample's time, from the same fixed
    /// templates, with errors for a service that has an open critical incident. Returns how many lines were added.
    /// </summary>
    public int RefreshLogs()
    {
        for (var i = 0; i < 3; i++)
        {
            var seed = nextLogId;
            var service = Services[(int)(MonitoringSeedData.Hash(seed, 17, i) * 1000) % Services.Count];
            var level = ActiveIncident(service.Id)?.Severity == MonitoringSeverity.Critical ? MonitoringLogLevel.Error : MonitoringLogLevel.Info;

            logs.Insert(0, new MonitoringLogEntry(nextLogId++, CurrentTime.AddMilliseconds(-i * 350), service.Id, level, MonitoringSeedData.Message(service, level, seed), "-"));
        }

        Changed?.Invoke();

        return 3;
    }

    /// <summary>Records the color mode the theme switcher applied, so charts can redraw in matching colors.</summary>
    public void SetTheme(string? themeName)
    {
        var isDark = string.Equals(themeName, "dark", StringComparison.OrdinalIgnoreCase);

        if (isDarkTheme == isDark)
            return;

        isDarkTheme = isDark;
        ThemeChanged?.Invoke();
    }

    /// <summary>Returns the health of a service from its latest sample.</summary>
    public MonitoringStatus Status(MonitoringService service)
    {
        var latest = Latest(service.Id);

        if (latest.ErrorRate >= 20)
            return MonitoringStatus.Down;

        return latest.ErrorRate >= 2 || latest.LatencyMs >= service.BaseLatencyMs * 2 || latest.Cpu >= 90
            ? MonitoringStatus.Degraded
            : MonitoringStatus.Operational;
    }

    /// <summary>Adds one sample per service and drops the oldest, so each chart keeps the same window.</summary>
    public void Tick()
    {
        tick++;

        foreach (var service in Services)
        {
            var samples = history[service.Id];
            samples.Add(Sample(service, tick));

            if (samples.Count > MonitoringSeedData.HistoryLength)
                samples.RemoveAt(0);
        }

        Ticked?.Invoke();
    }

    #endregion

    #region Private Methods

    /// <summary>A deterministic sample: a slow wave plus hash noise around the normal range, bent by any active incident.</summary>
    private MonitoringSample Sample(MonitoringService service, int at)
    {
        var index = Services.ToList().FindIndex(item => item.Id == service.Id);
        double Wave(int metric, double amplitude) =>
            1 + (amplitude * Math.Sin((at + (5 * index) + (3 * metric)) / 4.0)) + (amplitude * 0.7 * ((MonitoringSeedData.Hash(at, index, metric) * 2) - 1));

        var requests = service.BaseRequests * Wave(0, 0.08);
        var latency = service.BaseLatencyMs * Wave(1, 0.07);
        var errors = Math.Max(0, service.BaseErrorRate * Wave(2, 0.3));
        var cpu = Math.Clamp(service.BaseCpu * Wave(3, 0.06), 0, 100);
        var memory = Math.Clamp(service.BaseMemory * Wave(4, 0.02), 0, 100);

        // Active incidents bend the metrics they are about.
        var active = incidents.FirstOrDefault(incident => incident.ServiceId == service.Id && incident.Status != MonitoringIncidentStatus.Resolved);

        if (active?.Id == 301)
            errors = 6 + (1.5 * Math.Sin(at / 3.0)) + MonitoringSeedData.Hash(at, 3, 1);
        else if (active?.Id == 300)
        {
            latency *= 2.6;
            cpu = Math.Min(100, cpu * 1.5);
        }

        return new MonitoringSample(at, MonitoringSeedData.Now.AddSeconds(at * MonitoringSeedData.TickSeconds), requests, latency, errors, cpu, memory);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the time of the latest sample.</summary>
    public DateTime CurrentTime => MonitoringSeedData.Now.AddSeconds(tick * MonitoringSeedData.TickSeconds);

    /// <summary>Gets the incidents, newest first.</summary>
    public IReadOnlyList<MonitoringIncident> Incidents => incidents.OrderByDescending(incident => incident.StartedAt).ToList();

    /// <summary>Gets whether the theme switcher reported the dark color mode.</summary>
    public bool IsDarkTheme => isDarkTheme;

    /// <summary>Gets or sets whether live updates are paused; kept here so the choice holds across pages (WCAG 2.2.2).</summary>
    public bool IsPaused { get; set; }

    /// <summary>Gets the log lines, newest first.</summary>
    public IReadOnlyList<MonitoringLogEntry> Logs => logs;

    /// <summary>Gets the monitored services.</summary>
    public IReadOnlyList<MonitoringService> Services { get; } = MonitoringSeedData.Services();

    #endregion

    #region Events

    /// <summary>Raised when incidents or logs change.</summary>
    public event Action? Changed;

    /// <summary>Raised when the theme switcher changes the color mode.</summary>
    public event Action? ThemeChanged;

    /// <summary>Raised after <see cref="Tick" /> adds a sample.</summary>
    public event Action? Ticked;

    #endregion
}
