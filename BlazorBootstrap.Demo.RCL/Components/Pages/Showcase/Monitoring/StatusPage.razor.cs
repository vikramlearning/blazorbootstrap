using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// The public status page (what customers see): the overall status as a banner (icon, color, and words), the ongoing
/// published incidents with their public updates newest first, the maintenance windows in progress and coming up (time,
/// affected components, description), and every component with its public status ("Under maintenance" while a window
/// runs), its 90-day
/// uptime, and a 90-day bar. The bar is one group of day images (status in each one's name and tooltip, no tab stops);
/// a text summary and a "Days with incidents" table give the same facts to keyboard and screen reader users. Only
/// published facts are shown: no internal notes, no authors (not even who scheduled maintenance), no hidden services, and
/// no engineers-only incidents.
/// </summary>
public partial class StatusPage : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The route of the incident history.</summary>
    internal const string HistoryPath = MonitoringLayout.Base + "/history";

    /// <summary>The route of the subscribe page.</summary>
    internal const string SubscribePath = MonitoringLayout.Base + "/subscribe";

    private const int UptimeDayCount = 90;

    private static readonly MonitoringPublicStatus[] LegendStatuses =
    {
        MonitoringPublicStatus.Operational, MonitoringPublicStatus.Maintenance, MonitoringPublicStatus.Degraded, MonitoringPublicStatus.PartialOutage, MonitoringPublicStatus.MajorOutage,
    };

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the component status a published incident of a severity shows (critical is a major outage, major a partial outage, minor degraded).</summary>
    internal static MonitoringPublicStatus ImpactStatus(MonitoringSeverity severity) => severity switch
    {
        MonitoringSeverity.Critical => MonitoringPublicStatus.MajorOutage,
        MonitoringSeverity.Major => MonitoringPublicStatus.PartialOutage,
        _ => MonitoringPublicStatus.Degraded,
    };

    /// <summary>Returns an incident's status page updates, newest first (never its internal entries).</summary>
    internal static IReadOnlyList<MonitoringIncidentUpdate> PublicUpdates(MonitoringIncident incident) =>
        incident.Updates.Where(update => update.IsPublic).OrderByDescending(update => update.Time).ThenByDescending(update => update.Id).ToList();

    /// <summary>Returns the CSS suffix of a public status (operational, maintenance, degraded, partial, major).</summary>
    internal static string StatusClass(MonitoringPublicStatus status) => status switch
    {
        MonitoringPublicStatus.Maintenance => "maintenance",
        MonitoringPublicStatus.MajorOutage => "major",
        MonitoringPublicStatus.PartialOutage => "partial",
        MonitoringPublicStatus.Degraded => "degraded",
        _ => "operational",
    };

    #endregion

    #region Private Methods

    /// <summary>A day's accessible name and tooltip, like "Nov 22: Partial outage, incident #300".</summary>
    private static string DayLabel(MonitoringUptimeDay day)
    {
        var date = day.Date.ToString("MMM d", CultureInfo.InvariantCulture);
        var status = MonitoringDisplay.PublicStatusLabel(day.Status);

        return day.IncidentIds.Count switch
        {
            0 => $"{date}: {status}, no incidents",
            1 => $"{date}: {status}, incident #{day.IncidentIds[0].ToString(CultureInfo.InvariantCulture)}",
            _ => $"{date}: {status}, incidents {string.Join(", ", day.IncidentIds.Select(id => "#" + id.ToString(CultureInfo.InvariantCulture)))}",
        };
    }

    private static string IncidentDaysText(int count) => count switch
    {
        0 => "no incidents in the last 90 days",
        1 => "1 day with an incident",
        _ => $"{count.ToString(CultureInfo.InvariantCulture)} days with incidents",
    };

    private string IncidentLinkText(int incidentId) =>
        State.FindPublicIncident(incidentId) is { } incident ? $"#{incidentId.ToString(CultureInfo.InvariantCulture)}: {incident.Title}" : $"#{incidentId.ToString(CultureInfo.InvariantCulture)}";

    private static string MaintenanceDaysText(int count) => count switch
    {
        0 => string.Empty,
        1 => " · 1 day with maintenance",
        _ => $" · {count.ToString(CultureInfo.InvariantCulture)} days with maintenance",
    };

    /// <summary>A window's times, like "02:00 to 03:00", or "22:00 to Fri, Nov 27, 02:00" across midnight.</summary>
    private static string MaintenanceRange(MonitoringMaintenance window) =>
        window.Start.ToString("HH:mm", CultureInfo.InvariantCulture) + " to "
        + window.End.ToString(window.End.Date == window.Start.Date ? "HH:mm" : "ddd, MMM d, HH:mm", CultureInfo.InvariantCulture);

    /// <summary>The banner's note about maintenance, like " · Maintenance in progress", or nothing.</summary>
    private static string MaintenanceSummary(IReadOnlyList<MonitoringMaintenance> windows, DateTime now)
    {
        var running = windows.Count(window => window.StatusAt(now) == MonitoringMaintenanceStatus.InProgress);
        var upcoming = windows.Count - running;

        return running > 0 ? " · Maintenance in progress"
            : upcoming == 1 ? " · 1 maintenance window coming up"
            : upcoming > 1 ? $" · {upcoming.ToString(CultureInfo.InvariantCulture)} maintenance windows coming up"
            : string.Empty;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private static string OngoingText(int count) => count switch
    {
        0 => "No ongoing incidents",
        1 => "1 ongoing incident",
        _ => $"{count.ToString(CultureInfo.InvariantCulture)} ongoing incidents",
    };

    /// <summary>The window's components that are on the status page (hidden services aren't named).</summary>
    private string PublicServiceNames(MonitoringMaintenance window) =>
        string.Join(", ", window.ServiceIds.Select(id => State.FindService(id)).Where(service => service is { IsPublic: true }).Select(service => service!.Name));

    private string ServiceName(string serviceId) => State.FindService(serviceId)?.Name ?? serviceId;

    /// <summary>When a running window ends: the time on today's date, else the date too.</summary>
    private static string UntilText(MonitoringMaintenance window, DateTime now) =>
        window.End.ToString(window.End.Date == now.Date ? "HH:mm" : "MMM d, HH:mm", CultureInfo.InvariantCulture);

    private static string UptimeText(double percent) => percent.ToString("0.00", CultureInfo.InvariantCulture) + "%";

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion
}
