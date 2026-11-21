using System.Text.Json;
using System.Text.Json.Serialization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

/// <summary>
/// The tab snapshot of <see cref="HrState" /> (devspec finding F-4, work item 26112201): only what differs from a fresh tab is
/// stored, as for the other kept sites. The restore rebuilds the HR portal from a fresh tab plus the stored changes, then
/// validates and applies it exactly as before.
/// </summary>
public sealed partial class HrState
{
    #region Fields and Constants

    /// <summary>A fresh tab's records as JSON, by key: what <see cref="CaptureData" /> compares the state with.</summary>
    private static readonly Lazy<HrBaseline> baseline = new(() => new HrBaseline(new HrState().CaptureFull()));

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the snapshot: the ID counters and, of every list, only what differs from a fresh tab.</summary>
    public object CaptureData()
    {
        var current = CaptureFull();
        var seed = baseline.Value;

        var (audit, removedAudit) = ShowcaseSnapshotDelta.Diff(current.Audit!, seed.Audit, item => item.Id, snapshotOptions);
        var (cycles, removedCycles) = ShowcaseSnapshotDelta.Diff(current.Cycles!, seed.Cycles, item => item.Id, snapshotOptions);
        var (employees, removedEmployees) = ShowcaseSnapshotDelta.Diff(current.Employees!, seed.Employees, item => item.Id, snapshotOptions);
        var (leaveRequests, removedLeaveRequests) = ShowcaseSnapshotDelta.Diff(current.LeaveRequests!, seed.LeaveRequests, item => item.Id, snapshotOptions);
        var (notifications, removedNotifications) = ShowcaseSnapshotDelta.Diff(current.Notifications!, seed.Notifications, item => item.Id, snapshotOptions);
        var (onboardings, removedOnboardings) = ShowcaseSnapshotDelta.Diff(current.Onboardings!, seed.Onboardings, item => item.Id, snapshotOptions);
        var (policies, removedPolicies) = ShowcaseSnapshotDelta.Diff(current.Policies!, seed.Policies, item => item.Id, snapshotOptions);
        var (reviews, removedReviews) = ShowcaseSnapshotDelta.Diff(current.Reviews!, seed.Reviews, item => item.Id, snapshotOptions);
        var (templates, removedTemplates) = ShowcaseSnapshotDelta.Diff(current.Templates!, seed.Templates, item => item.Id, snapshotOptions);


        return new HrDelta
        {
            Audit = audit,
            RemovedAuditIds = removedAudit,
            Cycles = cycles,
            RemovedCycleIds = removedCycles,
            Employees = employees,
            RemovedEmployeeIds = removedEmployees,
            LeaveRequests = leaveRequests,
            RemovedLeaveRequestIds = removedLeaveRequests,
            Notifications = notifications,
            RemovedNotificationIds = removedNotifications,
            Onboardings = onboardings,
            RemovedOnboardingIds = removedOnboardings,
            Policies = policies,
            RemovedPolicyIds = removedPolicies,
            Reviews = reviews,
            RemovedReviewIds = removedReviews,
            Templates = templates,
            RemovedTemplateIds = removedTemplates,
            NextAuditId = current.NextAuditId,
            NextCycleId = current.NextCycleId,
            NextEmployeeId = current.NextEmployeeId,
            NextLeaveId = current.NextLeaveId,
            NextNotificationId = current.NextNotificationId,
            NextOnboardingId = current.NextOnboardingId,
            NextPolicyId = current.NextPolicyId,
            NextReviewId = current.NextReviewId,
            NextTemplateId = current.NextTemplateId,
        };
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Rebuilds the full snapshot from a stored change list and a fresh tab; <see langword="null" /> when the stored data can't
    /// be read. An inconsistent change list leaves a list <see langword="null" />, which the validation rejects.
    /// </summary>
    private static HrSnapshot? RebuildFromDelta(JsonElement data)
    {
        HrDelta? delta;

        try
        {
            delta = data.Deserialize<HrDelta>(snapshotOptions);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException or ArgumentException)
        {
            return null;
        }

        if (delta is null)
            return null;

        // Fresh records, not the cached baseline's, so the restored state never shares objects with another tab.
        var fresh = new HrState().CaptureFull();

        return new HrSnapshot
        {
            Audit = ShowcaseSnapshotDelta.Overlay(fresh.Audit!, delta.Audit, delta.RemovedAuditIds, item => item.Id),
            Cycles = ShowcaseSnapshotDelta.Overlay(fresh.Cycles!, delta.Cycles, delta.RemovedCycleIds, item => item.Id),
            Employees = ShowcaseSnapshotDelta.Overlay(fresh.Employees!, delta.Employees, delta.RemovedEmployeeIds, item => item.Id),
            LeaveRequests = ShowcaseSnapshotDelta.Overlay(fresh.LeaveRequests!, delta.LeaveRequests, delta.RemovedLeaveRequestIds, item => item.Id),
            Notifications = ShowcaseSnapshotDelta.Overlay(fresh.Notifications!, delta.Notifications, delta.RemovedNotificationIds, item => item.Id),
            Onboardings = ShowcaseSnapshotDelta.Overlay(fresh.Onboardings!, delta.Onboardings, delta.RemovedOnboardingIds, item => item.Id),
            Policies = ShowcaseSnapshotDelta.Overlay(fresh.Policies!, delta.Policies, delta.RemovedPolicyIds, item => item.Id),
            Reviews = ShowcaseSnapshotDelta.Overlay(fresh.Reviews!, delta.Reviews, delta.RemovedReviewIds, item => item.Id),
            Templates = ShowcaseSnapshotDelta.Overlay(fresh.Templates!, delta.Templates, delta.RemovedTemplateIds, item => item.Id),
            NextAuditId = delta.NextAuditId,
            NextCycleId = delta.NextCycleId,
            NextEmployeeId = delta.NextEmployeeId,
            NextLeaveId = delta.NextLeaveId,
            NextNotificationId = delta.NextNotificationId,
            NextOnboardingId = delta.NextOnboardingId,
            NextPolicyId = delta.NextPolicyId,
            NextReviewId = delta.NextReviewId,
            NextTemplateId = delta.NextTemplateId,
        };
    }

    #endregion

    #endregion

    #region Nested Types

    /// <summary>A fresh tab's records as JSON, by key.</summary>
    private sealed class HrBaseline
    {
        #region Constructors and Finalizers

        public HrBaseline(HrSnapshot fresh)
        {
            Audit = ShowcaseSnapshotDelta.Index(fresh.Audit!, item => item.Id, snapshotOptions);
            Cycles = ShowcaseSnapshotDelta.Index(fresh.Cycles!, item => item.Id, snapshotOptions);
            Employees = ShowcaseSnapshotDelta.Index(fresh.Employees!, item => item.Id, snapshotOptions);
            LeaveRequests = ShowcaseSnapshotDelta.Index(fresh.LeaveRequests!, item => item.Id, snapshotOptions);
            Notifications = ShowcaseSnapshotDelta.Index(fresh.Notifications!, item => item.Id, snapshotOptions);
            Onboardings = ShowcaseSnapshotDelta.Index(fresh.Onboardings!, item => item.Id, snapshotOptions);
            Policies = ShowcaseSnapshotDelta.Index(fresh.Policies!, item => item.Id, snapshotOptions);
            Reviews = ShowcaseSnapshotDelta.Index(fresh.Reviews!, item => item.Id, snapshotOptions);
            Templates = ShowcaseSnapshotDelta.Index(fresh.Templates!, item => item.Id, snapshotOptions);
        }

        #endregion

        #region Properties, Indexers

        public Dictionary<int, string> Audit { get; }

        public Dictionary<int, string> Cycles { get; }

        public Dictionary<int, string> Employees { get; }

        public Dictionary<int, string> LeaveRequests { get; }

        public Dictionary<int, string> Notifications { get; }

        public Dictionary<int, string> Onboardings { get; }

        public Dictionary<int, string> Policies { get; }

        public Dictionary<int, string> Reviews { get; }

        public Dictionary<int, string> Templates { get; }

        #endregion
    }

    /// <summary>The state's part of the tab snapshot: what differs from a fresh tab, and the ID counters.</summary>
    private sealed class HrDelta
    {
        #region Properties, Indexers

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ShowcaseAuditEntry>? Audit { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HrReviewCycle>? Cycles { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HrEmployee>? Employees { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HrLeaveRequest>? LeaveRequests { get; set; }

        public int NextAuditId { get; set; }

        public int NextCycleId { get; set; }

        public int NextEmployeeId { get; set; }

        public int NextLeaveId { get; set; }

        public int NextNotificationId { get; set; }

        public int NextOnboardingId { get; set; }

        public int NextPolicyId { get; set; }

        public int NextReviewId { get; set; }

        public int NextTemplateId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ShowcaseNotification>? Notifications { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HrOnboarding>? Onboardings { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HrLeavePolicy>? Policies { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedAuditIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedCycleIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedEmployeeIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedLeaveRequestIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedNotificationIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedOnboardingIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedPolicyIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedReviewIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedTemplateIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HrReview>? Reviews { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HrOnboardingTemplate>? Templates { get; set; }

        #endregion
    }

    #endregion
}
