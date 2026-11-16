using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// The public status page (what customers see): the overall status as a banner (icon, color, and words), the ongoing
/// published incidents with their public updates newest first, and every component with its public status, its 90-day
/// uptime, and a 90-day bar. The bar is one group of day images (status in each one's name and tooltip, no tab stops);
/// a text summary and a "Days with incidents" table give the same facts to keyboard and screen reader users. Only
/// published facts are shown: no internal notes, no authors, and no engineers-only incidents.
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
        MonitoringPublicStatus.Operational, MonitoringPublicStatus.Degraded, MonitoringPublicStatus.PartialOutage, MonitoringPublicStatus.MajorOutage,
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

    /// <summary>Returns the CSS suffix of a public status (operational, degraded, partial, major).</summary>
    internal static string StatusClass(MonitoringPublicStatus status) => status switch
    {
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

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private static string OngoingText(int count) => count switch
    {
        0 => "No ongoing incidents",
        1 => "1 ongoing incident",
        _ => $"{count.ToString(CultureInfo.InvariantCulture)} ongoing incidents",
    };

    private string ServiceName(string serviceId) => State.FindService(serviceId)?.Name ?? serviceId;

    private static string UptimeText(double percent) => percent.ToString("0.00", CultureInfo.InvariantCulture) + "%";

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion
}
