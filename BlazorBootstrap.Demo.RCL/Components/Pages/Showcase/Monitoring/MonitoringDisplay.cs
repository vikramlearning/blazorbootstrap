using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Formatting and focus helpers shared by the Stackwatch screens. Formatting uses the invariant culture, so the Server
/// and WebAssembly hosts show the same text whatever the browser locale. Every colored badge also carries its meaning in
/// words.
/// </summary>
public static class MonitoringDisplay
{
    #region Methods

    #region Public Methods

    /// <summary>Formats how long ago a time was, relative to <paramref name="now" />, like "18 min ago" or "Nov 22".</summary>
    public static string Ago(DateTime time, DateTime now)
    {
        var age = now - time;

        return age.TotalMinutes < 1 ? "just now"
            : age.TotalMinutes < 60 ? FormattableString.Invariant($"{(int)age.TotalMinutes} min ago")
            : age.TotalHours < 24 ? FormattableString.Invariant($"{(int)age.TotalHours} h ago")
            : time.ToString("MMM d", CultureInfo.InvariantCulture);
    }

    /// <summary>Formats a duration like "45 min", "2 h 15 min", or "1 d 3 h".</summary>
    public static string Duration(TimeSpan span)
    {
        var minutes = (int)Math.Round(span.TotalMinutes);

        return minutes < 60 ? FormattableString.Invariant($"{minutes} min")
            : minutes < 24 * 60 ? (minutes % 60 == 0 ? FormattableString.Invariant($"{minutes / 60} h") : FormattableString.Invariant($"{minutes / 60} h {minutes % 60} min"))
            : FormattableString.Invariant($"{minutes / (24 * 60)} d {minutes % (24 * 60) / 60} h");
    }

    /// <summary>Formats an error rate like "0.30%".</summary>
    public static string ErrorRate(double percent) => percent.ToString("0.00", CultureInfo.InvariantCulture) + "%";

    /// <summary>Moves focus to an element, ignoring one that has left the page or a closed circuit (WCAG 2.4.3).</summary>
    public static async Task FocusAsync(ElementReference? element)
    {
        if (element is not ElementReference target)
            return;

        try
        {
            await target.FocusAsync();
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
        {
            // Nothing left to focus.
        }
    }

    /// <summary>Returns the route of an incident's page in the console.</summary>
    public static string IncidentUrl(int incidentId) => FormattableString.Invariant($"{ConsoleLayout.Base}/incidents/{incidentId}");

    /// <summary>Returns a service kind in words, like "API" or "Database".</summary>
    public static string KindLabel(MonitoringServiceKind kind) => kind switch
    {
        MonitoringServiceKind.Api => "API",
        MonitoringServiceKind.Worker => "Worker",
        MonitoringServiceKind.Database => "Database",
        MonitoringServiceKind.Cache => "Cache",
        _ => "Devices",
    };

    /// <summary>Returns the route of a published incident on the public status page.</summary>
    public static string PublicIncidentUrl(int incidentId) => FormattableString.Invariant($"{MonitoringLayout.Base}/incidents/{incidentId}");

    /// <summary>Returns the badge color of a component's public status.</summary>
    public static BadgeColor PublicStatusColor(MonitoringPublicStatus status) => status switch
    {
        MonitoringPublicStatus.MajorOutage => BadgeColor.Danger,
        MonitoringPublicStatus.PartialOutage => BadgeColor.Warning,
        MonitoringPublicStatus.Degraded => BadgeColor.Info,
        // Primary is this site's green, the same as Operational; maintenance gets its own neutral tone (the words carry it).
        MonitoringPublicStatus.Maintenance => BadgeColor.Dark,
        _ => BadgeColor.Success,
    };

    /// <summary>Returns the icon of a component's public status.</summary>
    public static IconName PublicStatusIcon(MonitoringPublicStatus status) => status switch
    {
        MonitoringPublicStatus.MajorOutage => IconName.XCircleFill,
        MonitoringPublicStatus.PartialOutage => IconName.ExclamationTriangleFill,
        MonitoringPublicStatus.Degraded => IconName.DashCircleFill,
        MonitoringPublicStatus.Maintenance => IconName.Tools,
        _ => IconName.CheckCircleFill,
    };

    /// <summary>Returns a component's public status in words, like "Partial outage".</summary>
    public static string PublicStatusLabel(MonitoringPublicStatus status) => status switch
    {
        MonitoringPublicStatus.MajorOutage => "Major outage",
        MonitoringPublicStatus.PartialOutage => "Partial outage",
        MonitoringPublicStatus.Degraded => "Degraded performance",
        MonitoringPublicStatus.Maintenance => "Under maintenance",
        _ => "Operational",
    };

    /// <summary>Returns the status page's overall headline for its worst component status.</summary>
    public static string OverallStatusText(MonitoringPublicStatus status) => status switch
    {
        MonitoringPublicStatus.MajorOutage => "Major outage",
        MonitoringPublicStatus.PartialOutage => "Partial outage",
        MonitoringPublicStatus.Degraded => "Some systems are slower than usual",
        MonitoringPublicStatus.Maintenance => "Scheduled maintenance in progress",
        _ => "All systems operational",
    };

    /// <summary>Returns an alert rule's metric in words.</summary>
    public static string MetricLabel(MonitoringAlertMetric metric) => metric switch
    {
        MonitoringAlertMetric.ErrorRate => "Error rate",
        MonitoringAlertMetric.Latency => "p95 latency",
        _ => "CPU",
    };

    /// <summary>Formats a metric value with its unit, like "2%" or "300 ms".</summary>
    public static string MetricValue(MonitoringAlertMetric metric, double value) =>
        metric == MonitoringAlertMetric.Latency ? Latency(value) : value.ToString("0.##", CultureInfo.InvariantCulture) + "%";

    /// <summary>Returns a sample's value of an alert rule's metric.</summary>
    public static double MetricOf(MonitoringAlertMetric metric, MonitoringSample sample) => metric switch
    {
        MonitoringAlertMetric.ErrorRate => sample.ErrorRate,
        MonitoringAlertMetric.Latency => sample.LatencyMs,
        _ => sample.Cpu,
    };

    /// <summary>Returns a maintenance window's status in words.</summary>
    public static string MaintenanceStatusLabel(MonitoringMaintenanceStatus status) => status switch
    {
        MonitoringMaintenanceStatus.Scheduled => "Scheduled",
        MonitoringMaintenanceStatus.InProgress => "In progress",
        MonitoringMaintenanceStatus.Completed => "Completed",
        _ => "Cancelled",
    };

    /// <summary>Returns the badge color of a maintenance window's status.</summary>
    public static BadgeColor MaintenanceStatusColor(MonitoringMaintenanceStatus status) => status switch
    {
        MonitoringMaintenanceStatus.Scheduled => BadgeColor.Info,
        MonitoringMaintenanceStatus.InProgress => BadgeColor.Dark,
        MonitoringMaintenanceStatus.Completed => BadgeColor.Success,
        _ => BadgeColor.Secondary,
    };

    /// <summary>Returns an impact (what an incident or a failure drill does to the metrics) in words.</summary>
    public static string ImpactLabel(MonitoringImpact impact) => impact switch
    {
        MonitoringImpact.Errors => "Elevated errors",
        MonitoringImpact.Latency => "Slow responses",
        _ => "Outage",
    };

    /// <summary>Returns a public update's stage in words.</summary>
    public static string StageLabel(MonitoringPublicStage stage) => stage switch
    {
        MonitoringPublicStage.Investigating => "Investigating",
        MonitoringPublicStage.Identified => "Identified",
        MonitoringPublicStage.Monitoring => "Monitoring",
        _ => "Resolved",
    };

    /// <summary>Returns the route of a trace in the console.</summary>
    public static string TraceUrl(string traceId) => $"{ConsoleLayout.Base}/traces/{Uri.EscapeDataString(traceId)}";

    /// <summary>Returns what a timeline entry is, in words.</summary>
    public static string UpdateKindLabel(MonitoringUpdateKind kind) => kind switch
    {
        MonitoringUpdateKind.Alert => "Alert",
        MonitoringUpdateKind.Public => "Status page",
        MonitoringUpdateKind.Status => "Status",
        MonitoringUpdateKind.Severity => "Severity",
        MonitoringUpdateKind.Page => "Page",
        _ => "Note",
    };

    /// <summary>Returns the badge color of an incident status.</summary>
    public static BadgeColor IncidentStatusColor(MonitoringIncidentStatus status) => status switch
    {
        MonitoringIncidentStatus.Open => BadgeColor.Danger,
        MonitoringIncidentStatus.Acknowledged => BadgeColor.Warning,
        _ => BadgeColor.Success,
    };

    /// <summary>Returns the icon of a service kind.</summary>
    public static IconName KindIcon(MonitoringServiceKind kind) => kind switch
    {
        MonitoringServiceKind.Api => IconName.Globe2,
        MonitoringServiceKind.Worker => IconName.Stack,
        MonitoringServiceKind.Database => IconName.Database,
        MonitoringServiceKind.Cache => IconName.Lightning,
        _ => IconName.Broadcast,
    };

    /// <summary>Formats a latency like "85 ms", "0.8 ms", or "1.24 s".</summary>
    public static string Latency(double ms) => ms switch
    {
        < 10 => ms.ToString("0.0", CultureInfo.InvariantCulture) + " ms",
        < 1000 => ms.ToString("0", CultureInfo.InvariantCulture) + " ms",
        _ => (ms / 1000).ToString("0.00", CultureInfo.InvariantCulture) + " s",
    };

    /// <summary>Returns the badge color of a log level.</summary>
    public static BadgeColor LevelColor(MonitoringLogLevel level) => level switch
    {
        MonitoringLogLevel.Error => BadgeColor.Danger,
        MonitoringLogLevel.Warning => BadgeColor.Warning,
        _ => BadgeColor.Secondary,
    };

    /// <summary>Formats a number with thousands separators and no decimals.</summary>
    public static string Number(double value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    /// <summary>Formats a percentage like "46%".</summary>
    public static string Percent(double value) => value.ToString("0", CultureInfo.InvariantCulture) + "%";

    /// <summary>Returns the unit of a service's throughput.</summary>
    public static string RequestUnit(MonitoringServiceKind kind) => kind switch
    {
        MonitoringServiceKind.Api => "requests/s",
        MonitoringServiceKind.Worker => "jobs/s",
        MonitoringServiceKind.Database => "queries/s",
        MonitoringServiceKind.Cache => "operations/s",
        _ => "messages/s",
    };

    /// <summary>Returns the route of a service's page in the console.</summary>
    public static string ServiceUrl(string serviceId) => $"{ConsoleLayout.Base}/services/{Uri.EscapeDataString(serviceId)}";

    /// <summary>Returns the badge color of a severity.</summary>
    public static BadgeColor SeverityColor(MonitoringSeverity severity) => severity switch
    {
        MonitoringSeverity.Critical => BadgeColor.Danger,
        MonitoringSeverity.Major => BadgeColor.Warning,
        _ => BadgeColor.Info,
    };

    /// <summary>Returns the badge color of a service status.</summary>
    public static BadgeColor StatusColor(MonitoringStatus status) => status switch
    {
        MonitoringStatus.Down => BadgeColor.Danger,
        MonitoringStatus.Degraded => BadgeColor.Warning,
        _ => BadgeColor.Success,
    };

    /// <summary>Returns the icon of a service status.</summary>
    public static IconName StatusIcon(MonitoringStatus status) => status switch
    {
        MonitoringStatus.Down => IconName.XCircleFill,
        MonitoringStatus.Degraded => IconName.ExclamationTriangleFill,
        _ => IconName.CheckCircleFill,
    };

    /// <summary>Formats a time of day like "10:31:04".</summary>
    public static string Time(DateTime time) => time.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

    /// <summary>Formats a date and time like "Nov 24, 10:12", with the year for another year than the site's, like "Dec 3, 2025, 14:05".</summary>
    public static string Stamp(DateTime time) =>
        time.ToString(time.Year == MonitoringSeedData.Now.Year ? "MMM d, HH:mm" : "MMM d, yyyy, HH:mm", CultureInfo.InvariantCulture);

    #endregion

    #endregion
}
