using System.Text.Json;
using System.Text.Json.Serialization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

/// <summary>
/// The tab snapshot of <see cref="MonitoringState" /> (devspec finding F-4, work item 26112201): only what differs from a fresh tab is
/// stored, as for the other kept sites. The restore rebuilds the monitoring console from a fresh tab plus the stored changes, then
/// validates and applies it exactly as before.
/// </summary>
public sealed partial class MonitoringState
{
    #region Fields and Constants

    /// <summary>A fresh tab's records as JSON, by key: what <see cref="CaptureData" /> compares the state with.</summary>
    private static readonly Lazy<MonitoringBaseline> baseline = new(() => new MonitoringBaseline(new MonitoringState().CaptureFull()));

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the snapshot: the ID counters and, of every list, only what differs from a fresh tab.</summary>
    public object CaptureData()
    {
        var current = CaptureFull();
        var seed = baseline.Value;

        var (audit, removedAudit) = ShowcaseSnapshotDelta.Diff(current.Audit!, seed.Audit, item => item.Id, snapshotOptions);
        var (emails, removedEmails) = ShowcaseSnapshotDelta.Diff(current.Emails!, seed.Emails, item => item.Id, snapshotOptions);
        var (faults, removedFaults) = ShowcaseSnapshotDelta.Diff(current.Faults!, seed.Faults, item => item.Id, snapshotOptions);
        var (incidents, removedIncidents) = ShowcaseSnapshotDelta.Diff(current.Incidents!, seed.Incidents, item => item.Id, snapshotOptions);
        var (maintenance, removedMaintenance) = ShowcaseSnapshotDelta.Diff(current.Maintenance!, seed.Maintenance, item => item.Id, snapshotOptions);
        var (notifications, removedNotifications) = ShowcaseSnapshotDelta.Diff(current.Notifications!, seed.Notifications, item => item.Id, snapshotOptions);
        var (overrides, removedOverrides) = ShowcaseSnapshotDelta.Diff(current.Overrides!, seed.Overrides, item => item.Id, snapshotOptions);
        var (policies, removedPolicies) = ShowcaseSnapshotDelta.Diff(current.Policies!, seed.Policies, item => item.Id, snapshotOptions);
        var (rules, removedRules) = ShowcaseSnapshotDelta.Diff(current.Rules!, seed.Rules, item => item.Id, snapshotOptions);
        var (services, removedServices) = ShowcaseSnapshotDelta.Diff(current.Services!, seed.Services, item => item.Id, snapshotOptions);
        var (subscriptions, removedSubscriptions) = ShowcaseSnapshotDelta.Diff(current.Subscriptions!, seed.Subscriptions, item => item.Id, snapshotOptions);
        var preferencesChanged = JsonSerializer.Serialize(current.Preferences, snapshotOptions) != seed.Preferences;

        return new MonitoringDelta
        {
            Audit = audit,
            RemovedAuditIds = removedAudit,
            Emails = emails,
            RemovedEmailIds = removedEmails,
            Faults = faults,
            RemovedFaultIds = removedFaults,
            Incidents = incidents,
            RemovedIncidentIds = removedIncidents,
            Maintenance = maintenance,
            RemovedMaintenanceIds = removedMaintenance,
            Notifications = notifications,
            RemovedNotificationIds = removedNotifications,
            Overrides = overrides,
            RemovedOverrideIds = removedOverrides,
            Policies = policies,
            RemovedPolicyIds = removedPolicies,
            Rules = rules,
            RemovedRuleIds = removedRules,
            Services = services,
            RemovedServiceIds = removedServices,
            Subscriptions = subscriptions,
            RemovedSubscriptionIds = removedSubscriptions,
            Preferences = preferencesChanged ? current.Preferences : null,
            NextAuditId = current.NextAuditId,
            NextEmailId = current.NextEmailId,
            NextFaultId = current.NextFaultId,
            NextIncidentId = current.NextIncidentId,
            NextMaintenanceId = current.NextMaintenanceId,
            NextRuleId = current.NextRuleId,
            NextNotificationId = current.NextNotificationId,
            NextOverrideId = current.NextOverrideId,
            NextPolicyId = current.NextPolicyId,
            NextSubscriptionId = current.NextSubscriptionId,
            NextUpdateId = current.NextUpdateId,
        };
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Rebuilds the full snapshot from a stored change list and a fresh tab; <see langword="null" /> when the stored data can't
    /// be read. An inconsistent change list leaves a list <see langword="null" />, which the validation rejects.
    /// </summary>
    private static MonitoringSnapshot? RebuildFromDelta(JsonElement data)
    {
        MonitoringDelta? delta;

        try
        {
            delta = data.Deserialize<MonitoringDelta>(snapshotOptions);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException or ArgumentException)
        {
            return null;
        }

        if (delta is null)
            return null;

        // Fresh records, not the cached baseline's, so the restored state never shares objects with another tab.
        var fresh = new MonitoringState().CaptureFull();

        return new MonitoringSnapshot
        {
            Audit = ShowcaseSnapshotDelta.Overlay(fresh.Audit!, delta.Audit, delta.RemovedAuditIds, item => item.Id),
            Emails = ShowcaseSnapshotDelta.Overlay(fresh.Emails!, delta.Emails, delta.RemovedEmailIds, item => item.Id),
            Faults = ShowcaseSnapshotDelta.Overlay(fresh.Faults!, delta.Faults, delta.RemovedFaultIds, item => item.Id),
            Incidents = ShowcaseSnapshotDelta.Overlay(fresh.Incidents!, delta.Incidents, delta.RemovedIncidentIds, item => item.Id),
            Maintenance = ShowcaseSnapshotDelta.Overlay(fresh.Maintenance!, delta.Maintenance, delta.RemovedMaintenanceIds, item => item.Id),
            Notifications = ShowcaseSnapshotDelta.Overlay(fresh.Notifications!, delta.Notifications, delta.RemovedNotificationIds, item => item.Id),
            Overrides = ShowcaseSnapshotDelta.Overlay(fresh.Overrides!, delta.Overrides, delta.RemovedOverrideIds, item => item.Id),
            Policies = ShowcaseSnapshotDelta.Overlay(fresh.Policies!, delta.Policies, delta.RemovedPolicyIds, item => item.Id),
            Rules = ShowcaseSnapshotDelta.Overlay(fresh.Rules!, delta.Rules, delta.RemovedRuleIds, item => item.Id),
            Services = ShowcaseSnapshotDelta.Overlay(fresh.Services!, delta.Services, delta.RemovedServiceIds, item => item.Id),
            Subscriptions = ShowcaseSnapshotDelta.Overlay(fresh.Subscriptions!, delta.Subscriptions, delta.RemovedSubscriptionIds, item => item.Id),
            Preferences = delta.Preferences ?? fresh.Preferences,
            NextAuditId = delta.NextAuditId,
            NextEmailId = delta.NextEmailId,
            NextFaultId = delta.NextFaultId,
            NextIncidentId = delta.NextIncidentId,
            NextMaintenanceId = delta.NextMaintenanceId,
            NextRuleId = delta.NextRuleId,
            NextNotificationId = delta.NextNotificationId,
            NextOverrideId = delta.NextOverrideId,
            NextPolicyId = delta.NextPolicyId,
            NextSubscriptionId = delta.NextSubscriptionId,
            NextUpdateId = delta.NextUpdateId,
        };
    }

    #endregion

    #endregion

    #region Nested Types

    /// <summary>A fresh tab's records as JSON, by key.</summary>
    private sealed class MonitoringBaseline
    {
        #region Constructors and Finalizers

        public MonitoringBaseline(MonitoringSnapshot fresh)
        {
            Audit = ShowcaseSnapshotDelta.Index(fresh.Audit!, item => item.Id, snapshotOptions);
            Emails = ShowcaseSnapshotDelta.Index(fresh.Emails!, item => item.Id, snapshotOptions);
            Faults = ShowcaseSnapshotDelta.Index(fresh.Faults!, item => item.Id, snapshotOptions);
            Incidents = ShowcaseSnapshotDelta.Index(fresh.Incidents!, item => item.Id, snapshotOptions);
            Maintenance = ShowcaseSnapshotDelta.Index(fresh.Maintenance!, item => item.Id, snapshotOptions);
            Notifications = ShowcaseSnapshotDelta.Index(fresh.Notifications!, item => item.Id, snapshotOptions);
            Overrides = ShowcaseSnapshotDelta.Index(fresh.Overrides!, item => item.Id, snapshotOptions);
            Policies = ShowcaseSnapshotDelta.Index(fresh.Policies!, item => item.Id, snapshotOptions);
            Rules = ShowcaseSnapshotDelta.Index(fresh.Rules!, item => item.Id, snapshotOptions);
            Services = ShowcaseSnapshotDelta.Index(fresh.Services!, item => item.Id, snapshotOptions);
            Subscriptions = ShowcaseSnapshotDelta.Index(fresh.Subscriptions!, item => item.Id, snapshotOptions);
            Preferences = JsonSerializer.Serialize(fresh.Preferences, snapshotOptions);
        }

        #endregion

        #region Properties, Indexers

        public Dictionary<int, string> Audit { get; }

        public Dictionary<int, string> Emails { get; }

        public Dictionary<int, string> Faults { get; }

        public Dictionary<int, string> Incidents { get; }

        public Dictionary<int, string> Maintenance { get; }

        public Dictionary<int, string> Notifications { get; }

        public Dictionary<int, string> Overrides { get; }

        public Dictionary<int, string> Policies { get; }

        public Dictionary<int, string> Rules { get; }

        public Dictionary<string, string> Services { get; }

        public Dictionary<int, string> Subscriptions { get; }

        public string Preferences { get; }

        #endregion
    }

    /// <summary>The state's part of the tab snapshot: what differs from a fresh tab, and the ID counters.</summary>
    private sealed class MonitoringDelta
    {
        #region Properties, Indexers

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ShowcaseAuditEntry>? Audit { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<MonitoringEmail>? Emails { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<MonitoringFault>? Faults { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<MonitoringIncident>? Incidents { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<MonitoringMaintenance>? Maintenance { get; set; }

        public int NextAuditId { get; set; }

        public int NextEmailId { get; set; }

        public int NextFaultId { get; set; }

        public int NextIncidentId { get; set; }

        public int NextMaintenanceId { get; set; }

        public int NextNotificationId { get; set; }

        public int NextOverrideId { get; set; }

        public int NextPolicyId { get; set; }

        public int NextRuleId { get; set; }

        public int NextSubscriptionId { get; set; }

        public int NextUpdateId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<MonitoringNotification>? Notifications { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<MonitoringOverride>? Overrides { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<MonitoringEscalationPolicy>? Policies { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, MonitoringNotificationPreferences>? Preferences { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedAuditIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedEmailIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedFaultIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedIncidentIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedMaintenanceIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedNotificationIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedOverrideIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedPolicyIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedRuleIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? RemovedServiceIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedSubscriptionIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<MonitoringAlertRule>? Rules { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<MonitoringService>? Services { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<MonitoringSubscription>? Subscriptions { get; set; }

        #endregion
    }

    #endregion
}
