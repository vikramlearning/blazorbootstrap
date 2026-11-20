using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

/// <summary>
/// The enterprise baseline of <see cref="MonitoringState" /> (rule WR-001): the audit log, the header's notifications (the
/// same list as the console's Notifications page, so the bell and the page always agree), the header's global search,
/// and the incident queue's assign and bulk actions. Audit entries and notifications link only to pages on this site.
/// </summary>
public sealed partial class MonitoringState
{
    #region Fields and Constants

    /// <summary>The most audit entries kept; the oldest go first.</summary>
    private const int MaxAuditEntries = 400;

    /// <summary>The longest audit text (action, record, details).</summary>
    private const int MaxAuditTextLength = 200;

    /// <summary>The site's base route (the same as <c>MonitoringLayout.Base</c>), the public status page.</summary>
    private const string SiteBase = DemoRouteConstants.Showcase_URL_Monitoring;

    /// <summary>The console's base route (the same as <c>ConsoleLayout.Base</c>).</summary>
    private const string ConsoleBase = SiteBase + "/console";

    /// <summary>The admins' alert rules and drills page.</summary>
    private const string AlertsLink = ConsoleBase + "/admin/alerts";

    /// <summary>The admins' maintenance page.</summary>
    private const string MaintenanceLink = ConsoleBase + "/admin/maintenance";

    /// <summary>The engineers' notification settings page.</summary>
    private const string NotificationsLink = ConsoleBase + "/notifications";

    /// <summary>The on-call page.</summary>
    private const string OnCallLink = ConsoleBase + "/on-call";

    /// <summary>The admins' services list.</summary>
    private const string ServicesLink = ConsoleBase + "/admin/services";

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Assigns an active incident to an engineer on the rotation (engineers and admins). Its status doesn't change: an open
    /// incident stays open until someone acknowledges it. The assignee is notified, unless they assigned it to themselves.
    /// </summary>
    public MonitoringResult Assign(int incidentId, string engineerId)
    {
        if (Engineer() is not { } account)
            return MonitoringResult.NotAllowed;

        if (FindIncident(incidentId) is not { } incident || FindEngineer(engineerId) is not { } engineer)
            return MonitoringResult.NotFound;

        if (incident.Status == MonitoringIncidentStatus.Resolved)
            return MonitoringResult.WrongState;

        if (incident.Assignee == engineer.Name)
            return MonitoringResult.NoChange;

        if (incident.Updates.Count >= MaxUpdatesPerIncident - ReservedUpdates || !TryAddUpdate(incident, account, $"Assigned to {engineer.Name}.", MonitoringUpdateKind.Status, null))
            return MonitoringResult.Invalid;

        incident.Assignee = engineer.Name;
        Record("Assigned", IncidentTarget(incident.Id), IncidentLink(incident.Id), $"To {engineer.Name}");
        Notify(engineer.Id, MonitoringNotificationKind.Assigned, FormattableString.Invariant($"Incident #{incident.Id} was assigned to you"), $"{account.Name} assigned you {incident.Title}.", incident.Id);
        Commit();

        return MonitoringResult.Ok;
    }

    /// <summary>
    /// Changes several incidents at once from the incident queue (engineers and admins): assigns them to an engineer, sets
    /// their severity (a reason is required, and goes on each timeline), acknowledges the open ones, or resolves them.
    /// Resolved incidents are left as they are. A published incident isn't resolved here, because its status page needs a
    /// "Resolved" message of its own; its other changes still apply. Each change is audited like the single action.
    /// </summary>
    public MonitoringBulkOutcome BulkUpdate(IReadOnlyCollection<int> incidentIds, string? assigneeId, MonitoringSeverity? severity, MonitoringIncidentStatus? status, string? reason)
    {
        if (Engineer() is not { } account)
            return new MonitoringBulkOutcome(MonitoringResult.NotAllowed, 0, 0, 0);

        reason = reason?.Trim() ?? string.Empty;
        var engineer = assigneeId is null ? null : FindEngineer(assigneeId);
        var ids = (incidentIds ?? Array.Empty<int>()).Distinct().ToList();

        if (assigneeId is not null && engineer is null)
            return new MonitoringBulkOutcome(MonitoringResult.NotFound, 0, 0, 0);

        if (ids.Count == 0 || (engineer is null && severity is null && status is null) || (severity is { } chosen && !Enum.IsDefined(chosen))
            || status is not (null or MonitoringIncidentStatus.Acknowledged or MonitoringIncidentStatus.Resolved)
            || (severity is not null && reason.Length == 0) || reason.Length > MaxUpdateLength)
            return new MonitoringBulkOutcome(MonitoringResult.Invalid, 0, 0, 0);

        // One size check up front instead of one per timeline entry: each incident gains at most four short entries.
        if (SnapshotLength() + (ids.Count * 4 * (MaxUpdateLength + 200)) > MaxSnapshotLength)
            return new MonitoringBulkOutcome(MonitoringResult.Invalid, 0, 0, 0);

        int changed = 0, unchanged = 0, published = 0;

        foreach (var incident in ids.Select(FindIncident).OfType<MonitoringIncident>())
        {
            if (incident.Status == MonitoringIncidentStatus.Resolved || incident.Updates.Count >= MaxUpdatesPerIncident - ReservedUpdates)
            {
                unchanged++;
                continue;
            }

            var any = false;
            var keptPublic = false;
            var target = IncidentTarget(incident.Id);
            var link = IncidentLink(incident.Id);

            if (engineer is not null && incident.Assignee != engineer.Name && AddBulkUpdate(incident, account, $"Assigned to {engineer.Name}.", MonitoringUpdateKind.Status))
            {
                incident.Assignee = engineer.Name;
                Record("Assigned", target, link, $"To {engineer.Name}");
                Notify(engineer.Id, MonitoringNotificationKind.Assigned, FormattableString.Invariant($"Incident #{incident.Id} was assigned to you"), $"{account.Name} assigned you {incident.Title}.", incident.Id);
                any = true;
            }

            if (severity is { } newSeverity && incident.Severity != newSeverity
                && AddBulkUpdate(incident, account, $"Severity changed from {incident.Severity} to {newSeverity}: {reason}", MonitoringUpdateKind.Severity))
            {
                Record("Changed severity", target, link, $"{incident.Severity} → {newSeverity}: {reason}");
                NotifyFollower(incident, account, FormattableString.Invariant($"{account.Name} changed #{incident.Id} to {SeverityWord(newSeverity).ToLowerInvariant()}"), reason);
                incident.Severity = newSeverity;
                any = true;
            }

            if (status == MonitoringIncidentStatus.Acknowledged && incident.Status == MonitoringIncidentStatus.Open && AddBulkUpdate(incident, account, "Acknowledged.", MonitoringUpdateKind.Status))
            {
                incident.Status = MonitoringIncidentStatus.Acknowledged;
                incident.Assignee ??= account.Name;
                Record("Acknowledged", target, link, incident.Title);
                any = true;
            }

            if (status == MonitoringIncidentStatus.Resolved)
            {
                if (incident.IsPublished)
                {
                    published++;
                    keptPublic = true;
                }
                else if (AddBulkUpdate(incident, account, reason.Length == 0 ? "Resolved." : $"Resolved: {reason}", MonitoringUpdateKind.Status))
                {
                    incident.Status = MonitoringIncidentStatus.Resolved;
                    incident.ResolvedAt = Clock.Now;
                    incident.Assignee ??= account.Name;
                    Record("Resolved", target, link, reason.Length == 0 ? incident.Title : reason);
                    NotifyFollower(incident, account, FormattableString.Invariant($"{account.Name} resolved #{incident.Id}"), reason.Length == 0 ? incident.Title : reason);
                    any = true;
                }
            }

            // An incident left open only because it is public is counted once, as such.
            if (any)
                changed++;
            else if (!keptPublic)
                unchanged++;
        }

        if (changed == 0)
            return new MonitoringBulkOutcome(MonitoringResult.NoChange, 0, unchanged, published);

        Commit();

        return new MonitoringBulkOutcome(MonitoringResult.Ok, changed, unchanged, published);
    }

    /// <summary>Returns the signed-in engineer's notifications for the header's menu, newest first; the same list as <see cref="Notifications" />.</summary>
    public IReadOnlyList<ShowcaseNotification> GetNotificationsForCurrentAccount() =>
        Notifications.Select(notification => new ShowcaseNotification
        {
            Id = notification.Id,
            AccountId = notification.AccountId,
            At = notification.Time,
            IsRead = notification.IsRead,
            Title = notification.Title,
            Text = notification.Text,
            Url = NotificationUrl(notification),
        }).ToList();

    /// <summary>Returns whether another alert rule already has this name, ignoring case (the rule being edited doesn't count).</summary>
    public bool IsRuleNameTaken(string? name, int exceptRuleId = 0) =>
        rules.Any(rule => rule.Id != exceptRuleId && string.Equals(rule.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns whether the email address already has a subscription in this tab, ignoring case.</summary>
    public bool IsSubscribed(string? email) => subscriptions.Any(subscription => string.Equals(subscription.Email, email?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Marks every notification of the signed-in engineer as read (the header's "Mark all as read").</summary>
    public void MarkAllNotificationsRead() => MarkAllRead();

    /// <summary>Marks one notification of the signed-in engineer as read; ignores an unknown ID or another account's notification.</summary>
    public void MarkNotificationRead(int id) => MarkRead(id);

    /// <summary>
    /// Returns what is wrong with an alert threshold for a service, or <see langword="null" />: it must be above the
    /// service's normal level of the metric, or the rule would fire on an ordinary day.
    /// </summary>
    public static string? NormalRangeProblem(MonitoringService service, MonitoringAlertMetric metric, double threshold)
    {
        var normal = metric switch
        {
            MonitoringAlertMetric.ErrorRate => service.BaseErrorRate,
            MonitoringAlertMetric.Latency => service.BaseLatencyMs,
            _ => service.BaseCpu,
        };

        return threshold > normal
            ? null
            : $"Set the threshold above {service.Name}'s normal {MetricLabel(metric).ToLowerInvariant()}, {MetricValue(metric, normal)}: at or below it, the rule would fire on an ordinary day.";
    }

    /// <summary>
    /// Returns a scheduled or running maintenance window that shares a service with these and overlaps the time from
    /// <paramref name="start" /> to <paramref name="end" />, or <see langword="null" />. The window being edited doesn't count.
    /// </summary>
    public MonitoringMaintenance? OverlappingMaintenance(IEnumerable<string> serviceIds, DateTime start, DateTime end, int? exceptId)
    {
        var ids = serviceIds.ToHashSet(StringComparer.Ordinal);

        return maintenance
            .Where(window => window.Id != exceptId && window.StatusAt(Clock.Now) is MonitoringMaintenanceStatus.Scheduled or MonitoringMaintenanceStatus.InProgress)
            .Where(window => window.ServiceIds.Any(ids.Contains) && window.Start < end && start < window.End)
            .OrderBy(window => window.Start)
            .FirstOrDefault();
    }

    /// <summary>
    /// The header's global search, best matches first (an exact number or name, then names that start with the term, then
    /// names that contain it), and among equal matches the active incidents, then services, a trace, alert rules,
    /// maintenance windows, and resolved incidents, newest first. Engineers and admins find incidents (by number or title),
    /// services (by name or ID), and a trace by its full ID while its log line is shown; admins also find alert rules and
    /// maintenance windows, whose pages are theirs. Returns nothing for a blank term or when nobody from the console is signed in.
    /// </summary>
    public IReadOnlyList<ShowcaseSearchResult> Search(string term, int maxResults)
    {
        term = term?.Trim() ?? string.Empty;

        if (term.Length == 0 || maxResults <= 0 || Engineer() is null)
            return Array.Empty<ShowcaseSearchResult>();

        var number = term.TrimStart('#');

        // 0: exactly the term, 1: starts with it, 2: contains it; null: no match.
        int? Rank(params string?[] values) => values
            .Select(value => value is null ? (int?)null
                : string.Equals(value, term, StringComparison.OrdinalIgnoreCase) ? 0
                : value.StartsWith(term, StringComparison.OrdinalIgnoreCase) ? 1
                : value.Contains(term, StringComparison.OrdinalIgnoreCase) ? 2
                : null)
            .Where(rank => rank is not null)
            .DefaultIfEmpty(null)
            .Min();

        int? NumberRank(int id)
        {
            var text = id.ToString(CultureInfo.InvariantCulture);

            return number.Length == 0 ? null : text == number ? 0 : text.StartsWith(number, StringComparison.Ordinal) ? 1 : text.Contains(number, StringComparison.Ordinal) ? 2 : null;
        }

        // Kind: 0 an active incident, 1 a service, 2 a trace, 3 an alert rule, 4 a maintenance window, 5 a resolved incident.
        var matches = new List<(int Rank, int Kind, long Order, ShowcaseSearchResult Result)>();

        foreach (var incident in incidents)
        {
            if (new[] { NumberRank(incident.Id), Rank(incident.Title) }.Min() is not { } found)
                continue;

            matches.Add((found, incident.Status == MonitoringIncidentStatus.Resolved ? 5 : 0, -incident.Id, new ShowcaseSearchResult("Incident", FormattableString.Invariant($"#{incident.Id} {incident.Title}"),
                $"{SeverityWord(incident.Severity)} · {incident.Status} · {FindService(incident.ServiceId)?.Name ?? incident.ServiceId}", IncidentLink(incident.Id))));
        }

        foreach (var service in services)
        {
            if (Rank(service.Name, service.Id) is { } found)
                matches.Add((found, 1, 0, new ShowcaseSearchResult("Service", service.Name, $"{KindWord(service.Kind)} · {service.Team}{(service.IsPublic ? string.Empty : " · engineers only")}",
                    $"{ConsoleBase}/services/{Uri.EscapeDataString(service.Id)}")));
        }

        // A trace is found by its whole ID, as engineers paste it from a log line or an error report.
        if (FindTrace(term.ToLowerInvariant()) is { } trace)
            matches.Add((0, 2, 0, new ShowcaseSearchResult("Trace", trace.TraceId, $"{FindService(trace.RootServiceId)?.Name ?? trace.RootServiceId} · {(trace.HasError ? "failed" : "succeeded")}",
                $"{ConsoleBase}/traces/{trace.TraceId}")));

        // Alert rules and maintenance windows are managed on admin pages, so only admins find them.
        if (IsAdmin)
        {
            foreach (var rule in rules)
            {
                if (Rank(rule.Name) is { } found)
                    matches.Add((found, 3, 0, new ShowcaseSearchResult("Alert rule", rule.Name, $"{FindService(rule.ServiceId)?.Name ?? rule.ServiceId} · {(rule.IsEnabled ? "enabled" : "disabled")}", AlertsLink)));
            }

            foreach (var window in maintenance)
            {
                if (Rank(window.Title) is { } found)
                    matches.Add((found, 4, -window.Start.Ticks, new ShowcaseSearchResult("Maintenance", window.Title,
                        FormattableString.Invariant($"{MaintenanceWord(window.StatusAt(Clock.Now))} · {window.Start:MMM d, yyyy}"), MaintenanceLink)));
            }
        }

        return matches
            .OrderBy(match => match.Rank)
            .ThenBy(match => match.Kind)
            .ThenBy(match => match.Order)
            .ThenBy(match => match.Result.Label, StringComparer.OrdinalIgnoreCase)
            .Take(maxResults)
            .Select(match => match.Result)
            .ToList();
    }

    #endregion

    #region Private Methods

    /// <summary>Adds a timeline entry for a bulk change without a size check per entry (<see cref="BulkUpdate" /> checks once).</summary>
    private bool AddBulkUpdate(MonitoringIncident incident, ShowcaseAccount account, string text, MonitoringUpdateKind kind)
    {
        if (incident.Updates.Count >= MaxUpdatesPerIncident - ReservedUpdates || nextUpdateId >= NextIdLimit)
            return false;

        incident.Updates.Add(new MonitoringIncidentUpdate(Clock.Now, Clip(account.Name, ShowcaseIdentity.MaxNameLength), text) { Id = nextUpdateId++, Kind = kind, AccountId = account.Id });

        return true;
    }

    /// <summary>
    /// Tells this week's on-call engineers once when a maintenance window starts (the monitoring system says so, so it
    /// reaches the signed-in person too), and records it. A window that started and ended unseen is only marked. Returns
    /// whether anything changed.
    /// </summary>
    private bool AnnounceMaintenanceStarts()
    {
        var changed = false;

        foreach (var window in maintenance.Where(window => !window.StartNotified && window.CancelledAt is null && window.Start <= Clock.Now).ToList())
        {
            window.StartNotified = true;
            changed = true;

            if (window.StatusAt(Clock.Now) != MonitoringMaintenanceStatus.InProgress)
                continue;

            Record(window.Start, MonitoringSeedData.SystemAuthor, "Maintenance started", MaintenanceTarget(window), MaintenanceLink, WindowText(window));
            NotifyMaintenance(window, $"Maintenance started: {window.Title}",
                FormattableString.Invariant($"Alert rules for {ServiceNamesText(window.ServiceIds)} stay quiet until {window.End:HH:mm}."), fromSystem: true);
        }

        return changed;
    }

    /// <summary>Says where alerts reach an engineer, for the audit log, like "Email, push; incident updates on".</summary>
    private static string ChannelsText(MonitoringNotificationPreferences value)
    {
        var channels = new List<string>();

        if (value.Email)
            channels.Add("email");

        if (value.Push)
            channels.Add("push");

        if (value.SmsForCritical)
            channels.Add("text for critical");

        var reach = channels.Count == 0 ? "Console only" : "Alerts by " + string.Join(", ", channels);

        return reach + (value.IncidentUpdates ? "; incident updates on" : "; incident updates off");
    }

    /// <summary>Shortens a text to the audit log's limit.</summary>
    private static string ClipAudit(string text) => text.Length <= MaxAuditTextLength ? text : string.Concat(text.AsSpan(0, MaxAuditTextLength - 1), "…");

    /// <summary>The services a subscription follows, for the audit log.</summary>
    private string FollowsText(IReadOnlyCollection<string> serviceIds) => serviceIds.Count == 0 ? "Follows every component" : $"Follows {ServiceNamesText(serviceIds)}";

    private static string ImpactWord(MonitoringImpact impact) => impact switch
    {
        MonitoringImpact.Errors => "Elevated errors",
        MonitoringImpact.Latency => "Slow responses",
        _ => "Outage",
    };

    private static string IncidentLink(int incidentId) => FormattableString.Invariant($"{ConsoleBase}/incidents/{incidentId}");

    private static string IncidentTarget(int incidentId) => FormattableString.Invariant($"Incident #{incidentId}");

    private static bool IsAdminAccount(string accountId) => MonitoringSeedData.DemoAccounts().Any(account => account.Id == accountId && account.RoleId == MonitoringSeedData.AdminRole);

    /// <summary>Whether the engineer is primary or secondary on call this week or next.</summary>
    private bool IsOnCallSoon(string engineerId)
    {
        var week = WeekIndex(Clock.Now);

        return new[] { ShiftForWeek(week), ShiftForWeek(week + 1) }.Any(shift => shift.PrimaryId == engineerId || shift.SecondaryId == engineerId);
    }

    /// <summary>Checks a stored audit or notification link: none, or the status page or a path under it, without spaces, quotes, or a second slash.</summary>
    private static bool IsSiteLink(string? url) =>
        url is null || (url.Length <= 200 && (url == SiteBase || url.StartsWith(SiteBase + "/", StringComparison.Ordinal)) && !url.Contains("//", StringComparison.Ordinal)
            && url.All(character => !char.IsWhiteSpace(character) && !char.IsControl(character) && character is not ('"' or '\'' or '<' or '>' or '\\')));

    private static string KindWord(MonitoringServiceKind kind) => kind switch
    {
        MonitoringServiceKind.Api => "API",
        MonitoringServiceKind.Worker => "Worker",
        MonitoringServiceKind.Database => "Database",
        MonitoringServiceKind.Cache => "Cache",
        _ => "Devices",
    };

    private static string MaintenanceTarget(MonitoringMaintenance window) => $"Maintenance: {window.Title}";

    private static string MaintenanceWord(MonitoringMaintenanceStatus status) => status switch
    {
        MonitoringMaintenanceStatus.Scheduled => "Scheduled",
        MonitoringMaintenanceStatus.InProgress => "In progress",
        MonitoringMaintenanceStatus.Completed => "Completed",
        _ => "Cancelled",
    };

    /// <summary>The page a notification opens: its own link, its incident, or the on-call page for an on-call notice.</summary>
    private static string? NotificationUrl(MonitoringNotification notification) =>
        notification.Url ?? (notification.IncidentId is { } incidentId ? IncidentLink(incidentId) : notification.Kind == MonitoringNotificationKind.OnCall ? OnCallLink : null);

    /// <summary>
    /// Tells an incident's assignee that someone else changed it or posted on it, when they have a demo sign-in and keep
    /// "Updates to incidents you're working on" on (rule WR-001: an update on an incident you follow).
    /// </summary>
    private void NotifyFollower(MonitoringIncident incident, ShowcaseAccount actor, string title, string text)
    {
        if (Engineers.FirstOrDefault(engineer => engineer.Name == incident.Assignee) is not { } follower || follower.Id == actor.Id)
            return;

        var wants = !preferences.TryGetValue(follower.Id, out var saved) || saved.IncidentUpdates;

        if (wants)
            Notify(follower.Id, MonitoringNotificationKind.Update, title, text, incident.Id);
    }

    /// <summary>Tells this week's on-call engineers about a maintenance window: alerts for its services stay quiet while it runs.</summary>
    private void NotifyMaintenance(MonitoringMaintenance window, string title, string text, bool fromSystem)
    {
        var shift = ShiftForWeek(WeekIndex(Clock.Now));

        foreach (var engineerId in new[] { shift.PrimaryId, shift.SecondaryId }.Distinct())
            Notify(engineerId, MonitoringNotificationKind.Maintenance, title, text, null, IsAdminAccount(engineerId) ? MaintenanceLink : SiteBase, fromSystem);
    }

    /// <summary>
    /// Adds an audit entry (rule WR-001): the signed-in person did <paramref name="action" /> (past tense) to
    /// <paramref name="target" />, now, on the site's clock.
    /// </summary>
    private void Record(string action, string target, string? targetUrl, string? details = null) =>
        Record(Clock.Now, Identity.Current?.Name ?? MonitoringSeedData.ProductName, action, target, targetUrl, details);

    /// <summary>Adds an audit entry for <paramref name="actor" /> at <paramref name="at" />: the alert rules act on their own, not as the signed-in person. The log keeps the newest <see cref="MaxAuditEntries" />.</summary>
    private void Record(DateTime at, string actor, string action, string target, string? targetUrl, string? details)
    {
        if (nextAuditId >= NextIdLimit)
            return;

        audit.Add(new ShowcaseAuditEntry
        {
            Id = nextAuditId++,
            At = at,
            Actor = Clip(actor, ShowcaseIdentity.MaxNameLength),
            Action = ClipAudit(action),
            Target = ClipAudit(target),
            TargetUrl = targetUrl,
            Details = string.IsNullOrWhiteSpace(details) ? null : ClipAudit(details),
        });

        if (audit.Count > MaxAuditEntries)
            audit.RemoveAt(0);
    }

    /// <summary>Adds an audit entry now for <paramref name="actor" />, who may be signed out (a status page visitor).</summary>
    private void RecordAs(string actor, string action, string target, string? targetUrl, string? details) => Record(Clock.Now, actor, action, target, targetUrl, details);

    private static string RoleWord(MonitoringOnCallRole role) => role == MonitoringOnCallRole.Primary ? "Primary" : "Secondary";

    private static string RuleTarget(string name) => $"Alert rule: {name}";

    /// <summary>Says what changed on a service, for the audit log; for a new service, what it is.</summary>
    private static string ServiceChanges(MonitoringService? before, MonitoringService after)
    {
        if (before is null)
            return FormattableString.Invariant($"{KindWord(after.Kind)} in {after.Region}, {(after.IsPublic ? "on the status page" : "engineers only")}");

        var changes = new List<string>();

        if (before.Name != after.Name)
            changes.Add($"Name {before.Name} → {after.Name}");

        if (before.IsPublic != after.IsPublic)
            changes.Add(after.IsPublic ? "Shown on the status page" : "Hidden from the status page");

        if (before.Kind != after.Kind)
            changes.Add($"Kind {KindWord(before.Kind)} → {KindWord(after.Kind)}");

        if (before.Region != after.Region || before.Team != after.Team)
            changes.Add($"{after.Region}, {after.Team}");

        if (before.BaseLatencyMs != after.BaseLatencyMs)
            changes.Add(FormattableString.Invariant($"Normal latency {before.BaseLatencyMs:0.#} → {after.BaseLatencyMs:0.#} ms"));

        if (before.BaseRequests != after.BaseRequests || before.Instances != after.Instances)
            changes.Add(FormattableString.Invariant($"Traffic {after.BaseRequests:0.#}, {after.Instances} {(after.Kind == MonitoringServiceKind.Devices ? "devices" : "instances")}"));

        if (before.Description != after.Description)
            changes.Add("Description");

        return string.Join("; ", changes);
    }

    private string ServiceNamesText(IEnumerable<string> serviceIds) => string.Join(", ", serviceIds.Select(id => FindService(id)?.Name ?? id));

    private static string ServiceEditLink(string serviceId) => $"{ServicesLink}/{Uri.EscapeDataString(serviceId)}";

    /// <summary>The length of the snapshot as it would be saved now, or the limit when it can't be serialized.</summary>
    private int SnapshotLength()
    {
        try
        {
            return System.Text.Json.JsonSerializer.Serialize(CaptureData(), snapshotOptions).Length;
        }
        catch (Exception ex) when (ex is System.Text.Json.JsonException or NotSupportedException)
        {
            return MaxSnapshotLength;
        }
    }

    private static string SubscriptionTarget(int subscriptionId) => FormattableString.Invariant($"Status page subscription #{subscriptionId}");

    /// <summary>The name a subscription change is recorded under: the signed-in person, or a status page visitor.</summary>
    private string VisitorName => Identity.Current?.Name ?? "A status page visitor";

    private static string WeekTarget(DateTime shiftStart) => FormattableString.Invariant($"On-call week of {shiftStart:MMM d}");

    /// <summary>A window's time and services, like "Nov 26, 02:00 to 03:00 on Orders database".</summary>
    private string WindowText(MonitoringMaintenance window)
    {
        var end = window.End.Date == window.Start.Date ? FormattableString.Invariant($"{window.End:HH:mm}") : FormattableString.Invariant($"{window.End:MMM d, HH:mm}");

        return FormattableString.Invariant($"{window.Start:MMM d, HH:mm} to {end} on {ServiceNamesText(window.ServiceIds)}");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the audit log, newest first, for engineers and admins; empty for anyone else.</summary>
    public IReadOnlyList<ShowcaseAuditEntry> AuditEntries =>
        IsEngineer ? audit.OrderByDescending(entry => entry.At).ThenByDescending(entry => entry.Id).ToList() : Array.Empty<ShowcaseAuditEntry>();

    #endregion
}
