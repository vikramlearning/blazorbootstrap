using System.Text.Json;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

/// <summary>
/// State of the "Stackwatch" monitoring site for the current browser tab: the monitored services, incidents with their
/// internal and public timelines, alert rules, injected failures, maintenance windows, status page subscriptions and the
/// demo email outbox, engineers' notifications and preferences, on-call overrides, and the demo sign-in. It is kept in the tab's <c>sessionStorage</c> through the showcase foundation; nothing
/// is sent. It also keeps the enterprise baseline (rule WR-001): an audit log of every create, change, acknowledge,
/// resolve, and delete, in-app notifications for the demo engineers (the console's Notifications page and the header's
/// bell read the same list), and the header's global search (see <c>MonitoringState.Enterprise.cs</c>). Its signature
/// workflow is escalation policies with paging (see <c>MonitoringState.Escalation.cs</c>).
/// </summary>
/// <remarks>
/// Metrics and log lines are not stored: they are generated deterministically for the sample ticks of the site's moving
/// clock (one every <see cref="MonitoringSeedData.TickSeconds" /> seconds, D-029), bent by the incidents and injected
/// failures active at each sample's time. <see cref="Tick" /> moves the shown window up to the clock and then evaluates the
/// alert rules on it; live pages call it from a timer, and pausing stops it. Admins have every engineer action plus the
/// admin ones (rules, failures, escalation policies, maintenance, services). The public status page reads only what engineers published: an incident appears once it has a public update,
/// and is ongoing there until a public update says it is resolved.
/// </remarks>
public sealed partial class MonitoringState : IShowcaseEnterpriseState
{
    #region Fields and Constants

    /// <summary>The longest postmortem action item.</summary>
    public const int MaxActionItemLength = 300;

    /// <summary>The most postmortem action items.</summary>
    public const int MaxActionItems = 10;

    /// <summary>The longest email address.</summary>
    public const int MaxEmailLength = 120;

    /// <summary>The longest injected failure, in seconds.</summary>
    public const int MaxFaultSeconds = 900;

    /// <summary>The longest maintenance window.</summary>
    public static readonly TimeSpan MaxMaintenanceLength = TimeSpan.FromHours(24);

    /// <summary>The longest maintenance description.</summary>
    public const int MaxMaintenanceDescriptionLength = 1000;

    /// <summary>The most maintenance windows the tab holds.</summary>
    public const int MaxMaintenanceWindows = 50;

    /// <summary>The longest maintenance title.</summary>
    public const int MaxMaintenanceTitleLength = 120;

    /// <summary>The longest alert rule name (it becomes the incident title).</summary>
    public const int MaxRuleNameLength = 80;

    /// <summary>The most alert rules.</summary>
    public const int MaxRules = 30;

    /// <summary>The longest service description.</summary>
    public const int MaxServiceDescriptionLength = 300;

    /// <summary>The longest service name.</summary>
    public const int MaxServiceNameLength = 60;

    /// <summary>The most monitored services.</summary>
    public const int MaxServices = 20;

    /// <summary>The longest region or team name.</summary>
    public const int MaxServiceTagLength = 40;

    /// <summary>The shortest injected failure, in seconds.</summary>
    public const int MinFaultSeconds = 20;

    /// <summary>The longest incident summary.</summary>
    public const int MaxIncidentSummaryLength = 300;

    /// <summary>The longest incident title.</summary>
    public const int MaxIncidentTitleLength = 120;

    /// <summary>The longest postmortem section (summary or root cause).</summary>
    public const int MaxPostmortemLength = 2000;

    /// <summary>The longest override reason.</summary>
    public const int MaxReasonLength = 200;

    /// <summary>The most status page subscriptions the tab holds.</summary>
    public const int MaxSubscriptions = 20;

    /// <summary>The longest timeline entry (note, public update, or reason).</summary>
    public const int MaxUpdateLength = 1000;

    private const int FirstIncidentId = 302;

    private const int LiveLogTicks = 160;

    private const int MaxEmails = 300;

    private const int MaxFaults = 100;

    private const int MaxIncidents = 400;

    private const int MaxLogLines = 500;

    private const int MaxNotifications = 300;

    /// <summary>The most instances a service other than a device fleet may have (a fleet counts devices, up to 10,000).</summary>
    public const int MaxServiceInstances = 500;

    private const int MaxOverrides = 100;

    /// <summary>The largest the site's snapshot may grow: the store's limit less room for its envelope.</summary>
    private const int MaxSnapshotLength = ShowcaseSessionStore.MaxSnapshotLength - (48 * 1024);

    private const int MaxUpdatesPerIncident = 200;

    private const int NextIdLimit = 1_000_000;

    /// <summary>Timeline entries kept free on every incident for resolving and reopening (notes and updates stop before them).</summary>
    private const int ReservedUpdates = 4;

    /// <summary>Room for the prefix of status and severity entries on top of <see cref="MaxUpdateLength" />.</summary>
    private const int UpdatePrefixRoom = 64;

    private static readonly List<string> seedServiceIds = MonitoringSeedData.Services().Select(service => service.Id).ToList();

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<ShowcaseAuditEntry> audit = new();

    private readonly List<MonitoringEmail> emails = new();

    private readonly List<MonitoringFault> faults = new();

    private readonly Dictionary<string, List<MonitoringSample>> history = new();

    private readonly List<MonitoringIncident> incidents = MonitoringSeedData.Incidents();

    private readonly List<MonitoringMaintenance> maintenance = MonitoringSeedData.Maintenance();

    private readonly List<MonitoringNotification> notifications = MonitoringSeedData.Notifications();

    private readonly List<MonitoringOverride> overrides = new();

    private readonly List<MonitoringEscalationPolicy> policies = MonitoringSeedData.EscalationPolicies();

    private readonly Dictionary<string, MonitoringNotificationPreferences> preferences = new();

    private readonly List<MonitoringAlertRule> rules = MonitoringSeedData.AlertRules();

    private readonly List<MonitoringLogEntry> seedLogs;

    private readonly List<MonitoringService> services = MonitoringSeedData.Services().ToList();

    private readonly List<MonitoringSubscription> subscriptions = new();

    /// <summary>The tick the shown window ends at; it moves to the clock's tick on <see cref="Tick" />.</summary>
    private int displayTick;

    private bool isDarkTheme;

    /// <summary>The log lines for the shown tick, and the incident version they were built with.</summary>
    private (int Tick, int Version, IReadOnlyList<MonitoringLogEntry> Lines)? logCache;

    private int nextAuditId;

    private int nextEmailId = 1;

    private int nextFaultId = 1;

    private int nextIncidentId = FirstIncidentId;

    private int nextMaintenanceId;

    private int nextNotificationId;

    private int nextOverrideId = 1;

    private int nextPolicyId;

    private int nextRuleId = 7;

    private int nextSubscriptionId = 1;

    private int nextUpdateId;

    /// <summary>Changes whenever incidents change, so derived metrics and logs are rebuilt.</summary>
    private int version;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out, with a full chart window ending at tick 0.</summary>
    public MonitoringState()
    {
        seedLogs = MonitoringSeedData.Logs(services);
        nextUpdateId = incidents.SelectMany(incident => incident.Updates).Max(update => update.Id) + 1;
        nextNotificationId = notifications.Max(notification => notification.Id) + 1;
        nextMaintenanceId = maintenance.Max(window => window.Id) + 1;
        nextPolicyId = policies.Count == 0 ? 1 : policies.Max(policy => policy.Id) + 1;
        audit.AddRange(MonitoringSeedData.AuditEntries(incidents, rules, maintenance, policies));
        nextAuditId = audit.Count == 0 ? 1 : audit.Max(entry => entry.Id) + 1;
        Identity = new ShowcaseIdentity(MonitoringSeedData.Roles(), MonitoringSeedData.DemoAccounts());
        Identity.Changed += () => Changed?.Invoke();
        RebuildHistory();
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Acknowledges an open incident: the signed-in engineer takes it.</summary>
    public MonitoringResult Acknowledge(int incidentId)
    {
        if (Engineer() is not { } account)
            return MonitoringResult.NotAllowed;

        if (FindIncident(incidentId) is not { } incident)
            return MonitoringResult.NotFound;

        if (incident.Status != MonitoringIncidentStatus.Open)
            return incident.Status == MonitoringIncidentStatus.Acknowledged ? MonitoringResult.NoChange : MonitoringResult.WrongState;

        var paging = incident.Paging is { IsActive: true } running ? running : null;

        if (!TryAddUpdate(incident, account, AcknowledgedText(account, paging), MonitoringUpdateKind.Status, null))
            return MonitoringResult.Invalid;

        incident.Status = MonitoringIncidentStatus.Acknowledged;
        incident.Assignee = account.Name;
        var pages = StopPaging(incident, MonitoringPagingStop.Acknowledged, account);
        Record("Acknowledged", IncidentTarget(incident.Id), IncidentLink(incident.Id), pages is { } count ? $"{incident.Title}; paging stopped after {Plural(count, "page")}" : incident.Title);
        Commit();

        return MonitoringResult.Ok;
    }

    /// <summary>Returns the worst active incident of a service, if any.</summary>
    public MonitoringIncident? ActiveIncident(string serviceId) =>
        incidents.Where(incident => incident.ServiceId == serviceId && incident.Status != MonitoringIncidentStatus.Resolved).OrderBy(incident => incident.Severity).FirstOrDefault();

    /// <summary>Adds an engineers-only note to an incident's timeline (plain text, up to <see cref="MaxUpdateLength" /> characters).</summary>
    public MonitoringResult AddNote(int incidentId, string text)
    {
        if (Engineer() is not { } account)
            return MonitoringResult.NotAllowed;

        if (FindIncident(incidentId) is not { } incident)
            return MonitoringResult.NotFound;

        text = text?.Trim() ?? string.Empty;

        if (!IsText(text, MaxUpdateLength) || incident.Updates.Count >= MaxUpdatesPerIncident - ReservedUpdates || !TryAddUpdate(incident, account, text, MonitoringUpdateKind.Note, null))
            return MonitoringResult.Invalid;

        Record("Added a note", IncidentTarget(incident.Id), IncidentLink(incident.Id), text);
        NotifyFollower(incident, account, FormattableString.Invariant($"{account.Name} added a note on #{incident.Id}"), text);
        Commit();

        return MonitoringResult.Ok;
    }

    /// <summary>Returns the snapshot: services, incidents, rules, failures, maintenance, subscriptions, the outbox, notifications, preferences, overrides, the audit log, and the ID counters.</summary>
    public object CaptureData() => new MonitoringSnapshot
    {
        Audit = audit.ToList(),
        Emails = emails.ToList(),
        Policies = policies.ToList(),
        Faults = faults.ToList(),
        Incidents = incidents.ToList(),
        Maintenance = maintenance.ToList(),
        NextAuditId = nextAuditId,
        NextEmailId = nextEmailId,
        NextFaultId = nextFaultId,
        NextIncidentId = nextIncidentId,
        NextMaintenanceId = nextMaintenanceId,
        NextRuleId = nextRuleId,
        NextNotificationId = nextNotificationId,
        NextOverrideId = nextOverrideId,
        NextPolicyId = nextPolicyId,
        NextSubscriptionId = nextSubscriptionId,
        NextUpdateId = nextUpdateId,
        Notifications = notifications.ToList(),
        Overrides = overrides.ToList(),
        Preferences = preferences.ToDictionary(pair => pair.Key, pair => pair.Value),
        Rules = rules.ToList(),
        Services = services.ToList(),
        Subscriptions = subscriptions.ToList(),
    };

    /// <summary>
    /// Declares an incident by hand on a service (an engineer noticed something the alerts didn't). It starts open and
    /// engineers-only; the on-call engineers are notified.
    /// </summary>
    public MonitoringResult DeclareIncident(string serviceId, string title, string summary, MonitoringSeverity severity, MonitoringImpact impact, out MonitoringIncident? incident)
    {
        incident = null;

        if (Engineer() is not { } account)
            return MonitoringResult.NotAllowed;

        if (FindService(serviceId) is null)
            return MonitoringResult.NotFound;

        title = title?.Trim() ?? string.Empty;
        summary = summary?.Trim() ?? string.Empty;

        if (!IsText(title, MaxIncidentTitleLength) || !IsText(summary, MaxIncidentSummaryLength) || !Enum.IsDefined(severity) || !Enum.IsDefined(impact)
            || incidents.Count >= MaxIncidents || nextIncidentId >= NextIdLimit || nextUpdateId >= NextIdLimit)
            return MonitoringResult.Invalid;

        var created = new MonitoringIncident
        {
            Id = nextIncidentId++, ServiceId = serviceId, Severity = severity, Impact = impact, Status = MonitoringIncidentStatus.Open, StartedAt = Clock.Now, Title = title, Summary = summary,
        };

        created.Updates.Add(new MonitoringIncidentUpdate(Clock.Now, account.Name, $"Declared by {account.Name}.") { Id = nextUpdateId++, Kind = MonitoringUpdateKind.Alert, AccountId = account.Id });
        incidents.Add(created);

        if (!FitsSnapshot())
        {
            incidents.Remove(created);

            return MonitoringResult.Invalid;
        }

        incident = created;
        Record("Declared", IncidentTarget(created.Id), IncidentLink(created.Id), $"{SeverityWord(severity)} on {FindService(serviceId)!.Name}: {title}");

        // Under an escalation policy the on-call engineers are paged instead (the page is their notification).
        var paged = StartPaging(created, Clock.Now);
        NotifyOnCall(created, $"{SeverityWord(severity)} incident declared: {title}", $"{account.Name} declared #{created.Id} on {FindService(serviceId)!.Name}.", onCall: !paged);
        AdvancePaging(Clock.Now);
        Commit();

        return MonitoringResult.Ok;
    }

    /// <summary>Returns the emails "sent" to a subscription, newest first (the demo outbox).</summary>
    public IReadOnlyList<MonitoringEmail> EmailsFor(int subscriptionId) => emails.Where(email => email.SubscriptionId == subscriptionId).OrderByDescending(email => email.SentAt).ThenByDescending(email => email.Id).ToList();

    /// <summary>Finds an engineer on the rotation by ID, or returns <see langword="null" />.</summary>
    public MonitoringEngineer? FindEngineer(string? engineerId) => Engineers.FirstOrDefault(engineer => engineer.Id == engineerId);

    /// <summary>Finds an incident by its number (engineers' view), or returns <see langword="null" />.</summary>
    public MonitoringIncident? FindIncident(int incidentId) => incidents.FirstOrDefault(incident => incident.Id == incidentId);

    /// <summary>Finds a published incident by its number (the status page's view), or returns <see langword="null" />.</summary>
    public MonitoringIncident? FindPublicIncident(int incidentId) => incidents.FirstOrDefault(incident => incident.Id == incidentId && incident.IsPublished && IsOnStatusPage(incident.ServiceId));

    /// <summary>Finds a service by its ID, or returns <see langword="null" />.</summary>
    public MonitoringService? FindService(string? serviceId) => Services.FirstOrDefault(service => service.Id == serviceId);

    /// <summary>Finds a trace by its ID among the log lines still shown; <see langword="null" /> when it isn't there (unknown or expired).</summary>
    public MonitoringTrace? FindTrace(string? traceId) =>
        string.IsNullOrEmpty(traceId) || !TraceIdPattern().IsMatch(traceId) ? null
        : Logs.FirstOrDefault(entry => entry.TraceId == traceId) is { } entry ? MonitoringSeedData.BuildTrace(entry, Services) : null;

    /// <summary>Returns the chart window of a service, oldest first.</summary>
    public IReadOnlyList<MonitoringSample> History(string serviceId) => history[serviceId];

    /// <summary>Returns the latest shown sample of a service.</summary>
    public MonitoringSample Latest(string serviceId) => history[serviceId][^1];

    /// <summary>Marks every notification of the signed-in engineer read.</summary>
    public MonitoringResult MarkAllRead()
    {
        if (Engineer() is not { } account)
            return MonitoringResult.NotAllowed;

        var unread = notifications.Where(notification => notification.AccountId == account.Id && !notification.IsRead).ToList();

        if (unread.Count == 0)
            return MonitoringResult.NoChange;

        unread.ForEach(notification => notification.IsRead = true);
        Changed?.Invoke();

        return MonitoringResult.Ok;
    }

    /// <summary>Marks one of the signed-in engineer's notifications read or unread.</summary>
    public MonitoringResult MarkRead(int notificationId, bool isRead = true)
    {
        if (Engineer() is not { } account)
            return MonitoringResult.NotAllowed;

        if (notifications.FirstOrDefault(notification => notification.Id == notificationId && notification.AccountId == account.Id) is not { } notification)
            return MonitoringResult.NotFound;

        if (notification.IsRead == isRead)
            return MonitoringResult.NoChange;

        notification.IsRead = isRead;
        Changed?.Invoke();

        return MonitoringResult.Ok;
    }

    /// <summary>Raises <see cref="Changed" /> after a snapshot was restored, and moves the shown window up to the clock.</summary>
    public void NotifyRestored()
    {
        version++;
        displayTick = LiveTick;

        // Pages that fell due while the tab was closed go out now, at their due times.
        AdvancePaging(Clock.Now);
        RebuildHistory();
        Changed?.Invoke();
    }

    /// <summary>
    /// Publishes an update on the status page (plain text) at a stage, and emails the subscribers who follow the service.
    /// The first public update makes the incident public. "Resolved" also resolves the incident; a resolved incident takes
    /// no more updates until it is reopened.
    /// </summary>
    public MonitoringResult PostPublicUpdate(int incidentId, MonitoringPublicStage stage, string text)
    {
        if (Engineer() is not { } account)
            return MonitoringResult.NotAllowed;

        if (FindIncident(incidentId) is not { } incident)
            return MonitoringResult.NotFound;

        if (incident.Status == MonitoringIncidentStatus.Resolved)
            return MonitoringResult.WrongState;

        text = text?.Trim() ?? string.Empty;

        // A resolving update may use the entries kept free for it; other updates leave them.
        if (!Enum.IsDefined(stage) || !IsText(text, MaxUpdateLength)
            || (stage != MonitoringPublicStage.Resolved && incident.Updates.Count >= MaxUpdatesPerIncident - ReservedUpdates))
            return MonitoringResult.Invalid;

        var added = new List<MonitoringIncidentUpdate>();

        if (!TryAddUpdate(incident, account, text, MonitoringUpdateKind.Public, stage, added))
            return MonitoringResult.Invalid;

        if (stage == MonitoringPublicStage.Resolved)
        {
            if (!TryAddUpdate(incident, account, "Resolved.", MonitoringUpdateKind.Status, null, added))
            {
                added.ForEach(update => incident.Updates.Remove(update));

                return MonitoringResult.Invalid;
            }

            incident.Status = MonitoringIncidentStatus.Resolved;
            incident.ResolvedAt = Clock.Now;
            StopPaging(incident, MonitoringPagingStop.Resolved, account);
        }

        Record("Posted on the status page", IncidentTarget(incident.Id), IncidentLink(incident.Id), $"{StageWord(stage)}: {text}");

        if (stage == MonitoringPublicStage.Resolved)
            Record("Resolved", IncidentTarget(incident.Id), IncidentLink(incident.Id), incident.Title);

        NotifyFollower(incident, account, FormattableString.Invariant($"{account.Name} posted on the status page about #{incident.Id}"), $"{StageWord(stage)}: {text}");
        EmailSubscribers(incident, stage, text);
        Commit();

        return MonitoringResult.Ok;
    }

    /// <summary>Returns how a service shows on the status page: from the published, ongoing incidents that affect it, and any maintenance in progress.</summary>
    public MonitoringPublicStatus PublicStatus(string serviceId)
    {
        var fromIncidents = PublicActiveIncidents.Where(incident => incident.ServiceId == serviceId).Select(incident => StatusOf(incident.Severity)).DefaultIfEmpty(MonitoringPublicStatus.Operational).Max();

        // A maintenance window shows as such, unless a published incident is worse.
        return ActiveMaintenance(serviceId) is not null && fromIncidents < MonitoringPublicStatus.Maintenance ? MonitoringPublicStatus.Maintenance : fromIncidents;
    }

    /// <summary>
    /// Reopens a resolved incident (a reason is required). A published incident goes back to "Investigating" on the
    /// status page, so <paramref name="publicText" /> is required for it and is emailed to subscribers.
    /// </summary>
    public MonitoringResult Reopen(int incidentId, string reason, string? publicText)
    {
        if (Engineer() is not { } account)
            return MonitoringResult.NotAllowed;

        if (FindIncident(incidentId) is not { } incident)
            return MonitoringResult.NotFound;

        if (incident.Status != MonitoringIncidentStatus.Resolved)
            return MonitoringResult.WrongState;

        reason = reason?.Trim() ?? string.Empty;
        publicText = publicText?.Trim() ?? string.Empty;

        if (!IsText(reason, MaxUpdateLength) || (incident.IsPublished && !IsText(publicText, MaxUpdateLength)))
            return MonitoringResult.Invalid;

        var added = new List<MonitoringIncidentUpdate>();

        if (!TryAddUpdate(incident, account, $"Reopened: {reason}", MonitoringUpdateKind.Status, null, added)
            || (incident.IsPublished && !TryAddUpdate(incident, account, publicText, MonitoringUpdateKind.Public, MonitoringPublicStage.Investigating, added)))
        {
            added.ForEach(update => incident.Updates.Remove(update));

            return MonitoringResult.Invalid;
        }

        incident.Status = MonitoringIncidentStatus.Acknowledged;
        incident.Assignee = account.Name;
        incident.ResolvedAt = null;

        // A postmortem describes a finished incident: a published one comes off the status page until it is resolved again.
        if (incident.Postmortem is { PublishedAt: not null } postmortem)
            postmortem.PublishedAt = null;

        if (incident.IsPublished)
            EmailSubscribers(incident, MonitoringPublicStage.Investigating, publicText);

        Record("Reopened", IncidentTarget(incident.Id), IncidentLink(incident.Id), reason);
        NotifyOnCall(incident, $"Reopened: {incident.Title}", $"{account.Name} reopened #{incident.Id}: {Clip(reason, 200)}");
        Commit();

        return MonitoringResult.Ok;
    }

    /// <summary>
    /// Resolves an incident with an engineers' note. A published incident must also say so on the status page:
    /// <paramref name="publicText" /> is then required and is posted as the "Resolved" update and emailed to subscribers.
    /// </summary>
    public MonitoringResult Resolve(int incidentId, string note, string? publicText)
    {
        if (Engineer() is not { } account)
            return MonitoringResult.NotAllowed;

        if (FindIncident(incidentId) is not { } incident)
            return MonitoringResult.NotFound;

        if (incident.Status == MonitoringIncidentStatus.Resolved)
            return MonitoringResult.NoChange;

        note = note?.Trim() ?? string.Empty;
        publicText = publicText?.Trim() ?? string.Empty;

        if ((note.Length > 0 && !IsText(note, MaxUpdateLength)) || (incident.IsPublished && !IsText(publicText, MaxUpdateLength)))
            return MonitoringResult.Invalid;

        var added = new List<MonitoringIncidentUpdate>();

        if ((incident.IsPublished && !TryAddUpdate(incident, account, publicText, MonitoringUpdateKind.Public, MonitoringPublicStage.Resolved, added))
            || !TryAddUpdate(incident, account, note.Length == 0 ? "Resolved." : $"Resolved: {note}", MonitoringUpdateKind.Status, null, added))
        {
            added.ForEach(update => incident.Updates.Remove(update));

            return MonitoringResult.Invalid;
        }

        var follower = incident.Assignee;
        incident.Status = MonitoringIncidentStatus.Resolved;
        incident.ResolvedAt = Clock.Now;
        incident.Assignee ??= account.Name;
        StopPaging(incident, MonitoringPagingStop.Resolved, account);

        if (incident.IsPublished)
            EmailSubscribers(incident, MonitoringPublicStage.Resolved, publicText);

        Record("Resolved", IncidentTarget(incident.Id), IncidentLink(incident.Id), note.Length == 0 ? incident.Title : note);

        if (follower is not null)
            NotifyFollower(incident, account, FormattableString.Invariant($"{account.Name} resolved #{incident.Id}"), note.Length == 0 ? incident.Title : note);

        Commit();

        return MonitoringResult.Ok;
    }

    /// <summary>Removes one of the signed-in engineer's on-call overrides, for a week that hasn't ended.</summary>
    public MonitoringResult RemoveOverride(int overrideId)
    {
        if (Engineer() is not { } account)
            return MonitoringResult.NotAllowed;

        if (overrides.FirstOrDefault(item => item.Id == overrideId && item.AccountId == account.Id) is not { } found)
            return MonitoringResult.NotFound;

        if (found.ShiftStart.AddDays(7) <= Clock.Now)
            return MonitoringResult.WrongState;

        overrides.Remove(found);
        var original = FindEngineer(found.OriginalId)?.Name ?? found.OriginalId;
        Record("Removed an on-call override", WeekTarget(found.ShiftStart), OnCallLink, $"{RoleWord(found.Role)}: back to {original}");
        Notify(found.OriginalId, MonitoringNotificationKind.OnCall, $"You're {RoleWord(found.Role).ToLowerInvariant()} on call again for {WeekTarget(found.ShiftStart).ToLowerInvariant()}",
            $"{account.Name} removed their override, so the shift is yours as the rotation planned.", null);
        Changed?.Invoke();

        return MonitoringResult.Ok;
    }

    /// <summary>
    /// Writes or edits the postmortem of a resolved incident (plain text): summary, root cause, and up to
    /// <see cref="MaxActionItems" /> action items. <paramref name="publish" /> shows it on the public incident page (or takes
    /// it off again).
    /// </summary>
    public MonitoringResult SavePostmortem(int incidentId, string summary, string rootCause, IReadOnlyList<string> actionItems, bool publish)
    {
        if (Engineer() is not { } account)
            return MonitoringResult.NotAllowed;

        if (FindIncident(incidentId) is not { } incident)
            return MonitoringResult.NotFound;

        if (incident.Status != MonitoringIncidentStatus.Resolved || (publish && !incident.IsPublished))
            return MonitoringResult.WrongState;

        summary = summary?.Trim() ?? string.Empty;
        rootCause = rootCause?.Trim() ?? string.Empty;
        var actions = (actionItems ?? Array.Empty<string>()).Select(item => item?.Trim() ?? string.Empty).Where(item => item.Length > 0).ToList();

        if (!IsText(summary, MaxPostmortemLength) || !IsText(rootCause, MaxPostmortemLength) || actions.Count > MaxActionItems || actions.Any(item => item.Length > MaxActionItemLength))
            return MonitoringResult.Invalid;

        var before = incident.Postmortem;

        if (before is not null && before.Summary == summary && before.RootCause == rootCause && before.ActionItems.SequenceEqual(actions) && (before.PublishedAt is not null) == publish)
            return MonitoringResult.NoChange;

        incident.Postmortem = new MonitoringPostmortem
        {
            Summary = summary, RootCause = rootCause, ActionItems = actions, Author = account.Name, WrittenAt = Clock.Now,
            PublishedAt = publish ? before?.PublishedAt ?? Clock.Now : null,
        };

        if (!FitsSnapshot())
        {
            incident.Postmortem = before;

            return MonitoringResult.Invalid;
        }

        Record(publish && before?.PublishedAt is null ? "Published the postmortem"
            : !publish && before?.PublishedAt is not null ? "Unpublished the postmortem"
            : before is null ? "Wrote the postmortem"
            : "Updated the postmortem", IncidentTarget(incident.Id), IncidentLink(incident.Id), incident.Title);
        Commit();

        return MonitoringResult.Ok;
    }

    /// <summary>Saves how the signed-in engineer wants to be reached.</summary>
    public MonitoringResult SavePreferences(MonitoringNotificationPreferences value)
    {
        if (Engineer() is not { } account)
            return MonitoringResult.NotAllowed;

        if (value is null)
            return MonitoringResult.Invalid;

        var current = Preferences;

        if (current.Email == value.Email && current.Push == value.Push && current.SmsForCritical == value.SmsForCritical && current.IncidentUpdates == value.IncidentUpdates)
            return MonitoringResult.NoChange;

        // Someone who is paged this week or next must be reachable outside the console.
        if (!value.Email && !value.Push && !value.SmsForCritical && IsOnCallSoon(account.Id))
            return MonitoringResult.WrongState;

        preferences[account.Id] = new MonitoringNotificationPreferences { Email = value.Email, Push = value.Push, SmsForCritical = value.SmsForCritical, IncidentUpdates = value.IncidentUpdates };
        Record("Changed notification settings", $"Notification settings: {account.Name}", NotificationsLink, ChannelsText(value));
        Changed?.Invoke();

        return MonitoringResult.Ok;
    }

    /// <summary>Changes an incident's severity, with a reason on the timeline (and on the status page's component status, if published).</summary>
    public MonitoringResult SetSeverity(int incidentId, MonitoringSeverity severity, string reason)
    {
        if (Engineer() is not { } account)
            return MonitoringResult.NotAllowed;

        if (FindIncident(incidentId) is not { } incident)
            return MonitoringResult.NotFound;

        if (!Enum.IsDefined(severity))
            return MonitoringResult.Invalid;

        if (incident.Severity == severity)
            return MonitoringResult.NoChange;

        if (incident.Status == MonitoringIncidentStatus.Resolved)
            return MonitoringResult.WrongState;

        reason = reason?.Trim() ?? string.Empty;

        if (!IsText(reason, MaxUpdateLength) || !TryAddUpdate(incident, account, $"Severity changed from {incident.Severity} to {severity}: {reason}", MonitoringUpdateKind.Severity, null))
            return MonitoringResult.Invalid;

        Record("Changed severity", IncidentTarget(incident.Id), IncidentLink(incident.Id), $"{incident.Severity} → {severity}: {reason}");
        NotifyFollower(incident, account, FormattableString.Invariant($"{account.Name} changed #{incident.Id} to {SeverityWord(severity).ToLowerInvariant()}"), reason);
        incident.Severity = severity;
        Commit();

        return MonitoringResult.Ok;
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

    /// <summary>Returns the on-call weeks from <paramref name="fromWeek" /> (0 is the current week) for <paramref name="count" /> weeks, with overrides applied.</summary>
    public IReadOnlyList<MonitoringShift> Shifts(int fromWeek, int count)
    {
        var current = WeekIndex(Clock.Now);

        return Enumerable.Range(current + fromWeek, Math.Clamp(count, 0, 26)).Select(ShiftForWeek).ToList();
    }

    /// <summary>Returns the health of a service from its latest shown sample.</summary>
    public MonitoringStatus Status(MonitoringService service)
    {
        var latest = Latest(service.Id);

        if (latest.ErrorRate >= 20)
            return MonitoringStatus.Down;

        return latest.ErrorRate >= 2 || latest.LatencyMs >= service.BaseLatencyMs * 2 || latest.Cpu >= 90
            ? MonitoringStatus.Degraded
            : MonitoringStatus.Operational;
    }

    /// <summary>
    /// Subscribes an email address to status page updates for some services (empty for every service). The address only
    /// feeds the demo outbox in this tab; nothing is sent.
    /// </summary>
    public MonitoringResult Subscribe(string email, IReadOnlyList<string> serviceIds, out MonitoringSubscription? subscription)
    {
        subscription = null;
        email = (email ?? string.Empty).Trim().ToLowerInvariant();
        var services = (serviceIds ?? Array.Empty<string>()).Distinct().ToList();

        // Subscribers only see, and so only follow, the services on the status page.
        if (!IsEmail(email) || services.Any(id => !IsOnStatusPage(id)))
            return MonitoringResult.Invalid;

        if (subscriptions.Any(item => item.Email == email))
            return MonitoringResult.Duplicate;

        if (subscriptions.Count >= MaxSubscriptions || nextSubscriptionId >= NextIdLimit)
            return MonitoringResult.Invalid;

        subscription = new MonitoringSubscription { Id = nextSubscriptionId++, Email = email, ServiceIds = services, CreatedAt = Clock.Now };
        subscriptions.Add(subscription);

        // The address itself stays out of the log: the subscription number identifies it.
        RecordAs(VisitorName, "Subscribed to status page updates", SubscriptionTarget(subscription.Id), null, FollowsText(services));
        Changed?.Invoke();

        return MonitoringResult.Ok;
    }

    /// <summary>
    /// Takes over an on-call week as primary or secondary for the signed-in engineer, from whoever has it. The week must not
    /// have ended; a position already overridden can't be taken again until that override is removed.
    /// </summary>
    public MonitoringResult TakeShift(DateTime shiftStart, MonitoringOnCallRole role, string reason, out MonitoringOverride? taken)
    {
        taken = null;

        if (Engineer() is not { } account)
            return MonitoringResult.NotAllowed;

        if (FindEngineer(account.Id) is null)
            return MonitoringResult.NotAllowed;

        if (!Enum.IsDefined(role) || !IsWeekStart(shiftStart))
            return MonitoringResult.NotFound;

        reason = reason?.Trim() ?? string.Empty;

        if (reason.Length > MaxReasonLength)
            return MonitoringResult.Invalid;

        if (shiftStart.AddDays(7) <= Clock.Now)
            return MonitoringResult.WrongState;

        var shift = ShiftForWeek(WeekIndex(shiftStart));
        var holder = role == MonitoringOnCallRole.Primary ? shift.PrimaryId : shift.SecondaryId;
        var other = role == MonitoringOnCallRole.Primary ? shift.SecondaryId : shift.PrimaryId;

        if (holder == account.Id)
            return MonitoringResult.NoChange;

        // Nobody is both primary and secondary for the same week, and an overridden position is changed by removing that override.
        if (other == account.Id || overrides.Any(item => item.ShiftStart == shiftStart && item.Role == role))
            return MonitoringResult.WrongState;

        if (overrides.Count >= MaxOverrides || nextOverrideId >= NextIdLimit)
            return MonitoringResult.Invalid;

        taken = new MonitoringOverride
        {
            Id = nextOverrideId++, AccountId = account.Id, EngineerId = account.Id, OriginalId = holder, Role = role, ShiftStart = shiftStart, Reason = reason, CreatedAt = Clock.Now,
        };
        overrides.Add(taken);
        var holderName = FindEngineer(holder)?.Name ?? holder;
        Record("Took an on-call shift", WeekTarget(shiftStart), OnCallLink, $"{RoleWord(role)} instead of {holderName}" + (reason.Length > 0 ? $": {reason}" : string.Empty));
        Notify(holder, MonitoringNotificationKind.OnCall, $"{account.Name} took your {RoleWord(role).ToLowerInvariant()} shift",
            $"{WeekTarget(shiftStart)}: you're off that shift." + (reason.Length > 0 ? $" Reason: {reason}" : string.Empty), null);
        Changed?.Invoke();

        return MonitoringResult.Ok;
    }

    /// <summary>Moves the shown window up to the clock's latest tick, so charts and logs show the newest samples, then evaluates the alert rules.</summary>
    public void Tick()
    {
        var target = LiveTick;

        // Pages follow the clock, so they are sent even between samples.
        if (target == displayTick)
        {
            if (AdvancePaging(Clock.Now) > 0)
                Commit();

            return;
        }

        displayTick = target;
        RebuildHistory();

        // Each runs every tick: a later one must not be skipped when an earlier one changed something. Paging runs last,
        // so an incident a rule just opened pages its primary on call in the same tick.
        var announced = AnnounceMaintenanceStarts();
        var fired = EvaluateRules();
        var paged = AdvancePaging(Clock.Now) > 0;

        if (announced || fired || paged)
            Commit();

        Ticked?.Invoke();
    }

    /// <summary>Validates a stored snapshot against its own data (never the live lists) and applies it only when every part is valid.</summary>
    public bool TryRestoreData(JsonElement data)
    {
        MonitoringSnapshot? snapshot;

        try
        {
            snapshot = data.Deserialize<MonitoringSnapshot>(snapshotOptions);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException or ArgumentException)
        {
            return false;
        }

        if (snapshot is not
            {
                Audit: { Count: <= MaxAuditEntries } storedAudit,
                Emails: { Count: <= MaxEmails } storedEmails,
                Faults: { Count: <= MaxFaults } storedFaults,
                Incidents: { Count: > 0 and <= MaxIncidents } storedIncidents,
                Maintenance: { Count: <= MaxMaintenanceWindows } storedMaintenance,
                Notifications: { Count: <= MaxNotifications } storedNotifications,
                Policies: { Count: <= MaxPolicies } storedPolicies,
                Overrides: { Count: <= MaxOverrides } storedOverrides,
                Preferences: { Count: <= 10 } storedPreferences,
                Rules: { Count: <= MaxRules } storedRules,
                Services: { Count: > 0 and <= MaxServices } storedServices,
                Subscriptions: { Count: <= MaxSubscriptions } storedSubscriptions,
            }
            || snapshot.NextIncidentId is < FirstIncidentId or > NextIdLimit
            || snapshot.NextAuditId is < 1 or > NextIdLimit
            || snapshot.NextUpdateId is < 1 or > NextIdLimit
            || snapshot.NextEmailId is < 1 or > NextIdLimit
            || snapshot.NextNotificationId is < 1 or > NextIdLimit
            || snapshot.NextOverrideId is < 1 or > NextIdLimit
            || snapshot.NextSubscriptionId is < 1 or > NextIdLimit
            || snapshot.NextRuleId is < 1 or > NextIdLimit
            || snapshot.NextFaultId is < 1 or > NextIdLimit
            || snapshot.NextMaintenanceId is < 1 or > NextIdLimit
            || snapshot.NextPolicyId is < 1 or > NextIdLimit)
            return false;

        if (storedServices.Any(service => !ValidService(service)) || !Unique(storedServices.Select(service => service.Id)))
            return false;

        var serviceIds = storedServices.Select(service => service.Id).ToHashSet();

        // Policies: their own limits, known services, unique names, one active default at most, and no service named by two active policies.
        if (storedPolicies.Any(policy => policy is null || policy.Id < 1 || policy.Id >= snapshot.NextPolicyId || policy.ServiceIds is null
                || PolicyProblem(policy.Name, policy.IsDefault, policy.ServiceIds, policy.SecondaryAfterMinutes, policy.ThirdEngineerId, policy.ThirdAfterMinutes, policy.Repeats) is not null
                || policy.ServiceIds.Any(id => !serviceIds.Contains(id)) || !Unique(policy.ServiceIds))
            || !Unique(storedPolicies.Select(policy => policy.Id)) || !Unique(storedPolicies.Select(policy => policy.Name.ToUpperInvariant()))
            || storedPolicies.Count(policy => policy.IsActive && policy.IsDefault) > 1
            || !Unique(storedPolicies.Where(policy => policy.IsActive && !policy.IsDefault).SelectMany(policy => policy.ServiceIds)))
            return false;

        if (storedIncidents.Any(incident => !ValidIncident(incident, snapshot, serviceIds)) || !Unique(storedIncidents.Select(incident => incident.Id))
            || !Unique(storedIncidents.SelectMany(incident => incident.Updates).Select(update => update.Id)))
            return false;

        var incidentIds = storedIncidents.Select(incident => incident.Id).ToHashSet();

        if (storedRules.Any(rule => rule is null || rule.Id < 1 || rule.Id >= snapshot.NextRuleId || !serviceIds.Contains(rule.ServiceId) || rule.PublicMessage is null
                || RuleProblem(rule.Name, rule.Metric, rule.Threshold, rule.ForSeconds, rule.Severity, rule.AutoPublish, rule.PublicMessage) is not null
                || (rule.LastFiredAt is { } fired && !IsPlausible(fired)) || (rule.LastIncidentId is { } last && !incidentIds.Contains(last)))
            || !Unique(storedRules.Select(rule => rule.Id))
            || storedIncidents.Any(incident => (incident.RuleId is { } ruleId && (ruleId < 1 || ruleId >= snapshot.NextRuleId))
                || (incident.FaultId is { } faultId && (faultId < 1 || faultId >= snapshot.NextFaultId))))
            return false;

        if (storedFaults.Any(fault => fault is null || fault.Id < 1 || fault.Id >= snapshot.NextFaultId || !serviceIds.Contains(fault.ServiceId) || !Enum.IsDefined(fault.Impact)
                || !IsText(fault.CreatedBy, ShowcaseIdentity.MaxNameLength) || !IsPlausible(fault.Start) || fault.End < fault.Start || (fault.End - fault.Start).TotalSeconds > MaxFaultSeconds)
            || !Unique(storedFaults.Select(fault => fault.Id)))
            return false;

        if (storedMaintenance.Any(window => window is null || window.Id < 1 || window.Id >= snapshot.NextMaintenanceId || !IsText(window.Title, MaxMaintenanceTitleLength)
                || !IsText(window.Description, MaxMaintenanceDescriptionLength) || !IsText(window.CreatedBy, ShowcaseIdentity.MaxNameLength)
                || window.ServiceIds is not { Count: > 0 } || window.ServiceIds.Any(id => !serviceIds.Contains(id)) || !Unique(window.ServiceIds)
                || !IsPlausible(window.Start) || window.End <= window.Start || window.End - window.Start > MaxMaintenanceLength
                || (window.CancelledAt is { } cancelled && !IsPlausible(cancelled)))
            || !Unique(storedMaintenance.Select(window => window.Id)))
            return false;

        var maintenanceIds = storedMaintenance.Select(window => window.Id).ToHashSet();

        if (storedSubscriptions.Any(subscription => subscription is null || subscription.Id < 1 || subscription.Id >= snapshot.NextSubscriptionId || !IsEmail(subscription.Email)
                || subscription.ServiceIds is not { Count: <= MaxServices } || subscription.ServiceIds.Any(id => !serviceIds.Contains(id)) || !Unique(subscription.ServiceIds)
                || !IsPlausible(subscription.CreatedAt))
            || !Unique(storedSubscriptions.Select(subscription => subscription.Id)) || !Unique(storedSubscriptions.Select(subscription => subscription.Email)))
            return false;

        var subscriptionIds = storedSubscriptions.Select(subscription => subscription.Id).ToHashSet();

        // An email is about exactly one incident or one maintenance window.
        if (storedEmails.Any(email => email is null || email.Id < 1 || email.Id >= snapshot.NextEmailId || !subscriptionIds.Contains(email.SubscriptionId)
                || (email.IncidentId is null) == (email.MaintenanceId is null)
                || (email.IncidentId is { } incidentId && !incidentIds.Contains(incidentId)) || (email.MaintenanceId is { } windowId && !maintenanceIds.Contains(windowId))
                || !IsEmail(email.To) || !IsText(email.Subject, 200) || !IsText(email.Body, MaxUpdateLength + MaxMaintenanceDescriptionLength + 600) || !IsPlausible(email.SentAt))
            || !Unique(storedEmails.Select(email => email.Id)))
            return false;

        if (storedNotifications.Any(notification => notification is null || notification.Id < 1 || notification.Id >= snapshot.NextNotificationId
                || !IsDemoAccount(notification.AccountId) || !Enum.IsDefined(notification.Kind) || !IsText(notification.Title, 200) || !IsText(notification.Text, 500)
                || !IsPlausible(notification.Time) || (notification.IncidentId is { } id && !incidentIds.Contains(id)) || !IsSiteLink(notification.Url))
            || !Unique(storedNotifications.Select(notification => notification.Id)))
            return false;

        if (storedAudit.Any(entry => entry is null || entry.Id < 1 || entry.Id >= snapshot.NextAuditId || !IsPlausible(entry.At)
                || !IsText(entry.Actor, ShowcaseIdentity.MaxNameLength) || !IsText(entry.Action, MaxAuditTextLength) || !IsText(entry.Target, MaxAuditTextLength)
                || (entry.Details is not null && !IsText(entry.Details, MaxAuditTextLength)) || !IsSiteLink(entry.TargetUrl))
            || !Unique(storedAudit.Select(entry => entry.Id)))
            return false;

        if (storedPreferences.Any(pair => !IsDemoAccount(pair.Key) || pair.Value is null))
            return false;

        if (storedOverrides.Any(item => item is null || item.Id < 1 || item.Id >= snapshot.NextOverrideId || !IsDemoAccount(item.AccountId)
                || FindEngineer(item.EngineerId) is null || FindEngineer(item.OriginalId) is null || item.EngineerId == item.OriginalId || item.EngineerId != item.AccountId
                || !Enum.IsDefined(item.Role)
                || !IsWeekStart(item.ShiftStart) || item.Reason is null || item.Reason.Length > MaxReasonLength || !IsPlausible(item.CreatedAt))
            || !Unique(storedOverrides.Select(item => item.Id)) || !Unique(storedOverrides.Select(item => (item.ShiftStart, item.Role))))
            return false;

        services.Clear();
        services.AddRange(storedServices);
        incidents.Clear();
        incidents.AddRange(storedIncidents);
        rules.Clear();
        rules.AddRange(storedRules);
        faults.Clear();
        faults.AddRange(storedFaults);
        maintenance.Clear();
        maintenance.AddRange(storedMaintenance);
        subscriptions.Clear();
        subscriptions.AddRange(storedSubscriptions);
        emails.Clear();
        emails.AddRange(storedEmails);
        notifications.Clear();
        notifications.AddRange(storedNotifications);
        overrides.Clear();
        overrides.AddRange(storedOverrides);
        policies.Clear();
        policies.AddRange(storedPolicies);
        nextPolicyId = snapshot.NextPolicyId;
        preferences.Clear();

        foreach (var pair in storedPreferences)
            preferences[pair.Key] = pair.Value;

        audit.Clear();
        audit.AddRange(storedAudit.OrderBy(entry => entry.Id));
        nextAuditId = snapshot.NextAuditId;

        nextIncidentId = snapshot.NextIncidentId;
        nextUpdateId = snapshot.NextUpdateId;
        nextEmailId = snapshot.NextEmailId;
        nextNotificationId = snapshot.NextNotificationId;
        nextOverrideId = snapshot.NextOverrideId;
        nextSubscriptionId = snapshot.NextSubscriptionId;
        nextRuleId = snapshot.NextRuleId;
        nextFaultId = snapshot.NextFaultId;
        nextMaintenanceId = snapshot.NextMaintenanceId;
        version++;
        RebuildHistory();

        return true;
    }

    /// <summary>Removes a subscription and its emails.</summary>
    public MonitoringResult Unsubscribe(int subscriptionId)
    {
        if (subscriptions.RemoveAll(subscription => subscription.Id == subscriptionId) == 0)
            return MonitoringResult.NotFound;

        emails.RemoveAll(email => email.SubscriptionId == subscriptionId);
        RecordAs(VisitorName, "Unsubscribed from status page updates", SubscriptionTarget(subscriptionId), null, null);
        Changed?.Invoke();

        return MonitoringResult.Ok;
    }

    /// <summary>Changes which services a subscription follows (empty for every service).</summary>
    public MonitoringResult UpdateSubscription(int subscriptionId, IReadOnlyList<string> serviceIds)
    {
        if (subscriptions.FirstOrDefault(subscription => subscription.Id == subscriptionId) is not { } subscription)
            return MonitoringResult.NotFound;

        var services = (serviceIds ?? Array.Empty<string>()).Distinct().ToList();

        if (services.Any(id => !IsOnStatusPage(id)))
            return MonitoringResult.Invalid;

        if (subscription.ServiceIds.Order().SequenceEqual(services.Order()))
            return MonitoringResult.NoChange;

        subscription.ServiceIds = services;
        RecordAs(VisitorName, "Changed a subscription", SubscriptionTarget(subscriptionId), null, FollowsText(services));
        Changed?.Invoke();

        return MonitoringResult.Ok;
    }

    /// <summary>Returns a service's last <paramref name="days" /> days on the status page, oldest first: each day's worst published status.</summary>
    public IReadOnlyList<MonitoringUptimeDay> UptimeDays(string serviceId, int days = 90)
    {
        var today = Clock.Today;
        var published = incidents.Where(incident => incident.ServiceId == serviceId && incident.IsPublished).ToList();
        days = Math.Clamp(days, 1, 90);

        return Enumerable.Range(0, days).Select(offset =>
        {
            var date = today.AddDays(offset - (days - 1));
            var hits = published.Where(incident => incident.StartedAt < date.AddDays(1) && (incident.ResolvedAt ?? Clock.Now) >= date).ToList();
            var status = hits.Select(incident => StatusOf(incident.Severity)).DefaultIfEmpty(MonitoringPublicStatus.Operational).Max();

            // A day with maintenance (that happened) shows it, unless an incident made the day worse.
            if (status == MonitoringPublicStatus.Operational && maintenance.Any(window => window.CancelledAt is null && window.ServiceIds.Contains(serviceId)
                    && window.Start < date.AddDays(1) && window.End > date && window.Start <= Clock.Now))
                status = MonitoringPublicStatus.Maintenance;

            return new MonitoringUptimeDay(date, status, hits.Select(incident => incident.Id).ToList());
        }).ToList();
    }

    /// <summary>Returns a service's uptime over the last 90 days as the status page counts it: minutes of published outages, weighted by severity.</summary>
    public double UptimePercent(string serviceId)
    {
        var from = Clock.Today.AddDays(-89);
        var total = (Clock.Now - from).TotalMinutes;
        var lost = incidents
            .Where(incident => incident.ServiceId == serviceId && incident.IsPublished)
            .Sum(incident =>
            {
                var start = incident.StartedAt < from ? from : incident.StartedAt;
                var end = incident.ResolvedAt ?? Clock.Now;
                var weight = incident.Severity switch { MonitoringSeverity.Critical => 1.0, MonitoringSeverity.Major => 0.5, _ => 0.1 };

                return end > start ? (end - start).TotalMinutes * weight : 0;
            });

        return total <= 0 ? 100 : Math.Max(0, 100 - (lost / total * 100));
    }


    /// <summary>Returns the injected failure running on a service now, if any.</summary>
    public MonitoringFault? ActiveFault(string serviceId) => faults.FirstOrDefault(fault => fault.ServiceId == serviceId && fault.Start <= Clock.Now && Clock.Now < fault.End);

    /// <summary>Returns the maintenance window in progress on a service now, if any.</summary>
    public MonitoringMaintenance? ActiveMaintenance(string serviceId) =>
        maintenance.FirstOrDefault(window => window.ServiceIds.Contains(serviceId) && window.StatusAt(Clock.Now) == MonitoringMaintenanceStatus.InProgress);

    /// <summary>Cancels a scheduled maintenance window (admins), optionally emailing the subscribers who follow its services.</summary>
    public MonitoringResult CancelMaintenance(int maintenanceId, bool notifySubscribers)
    {
        if (Admin() is null)
            return MonitoringResult.NotAllowed;

        if (maintenance.FirstOrDefault(window => window.Id == maintenanceId) is not { } window)
            return MonitoringResult.NotFound;

        if (window.StatusAt(Clock.Now) != MonitoringMaintenanceStatus.Scheduled)
            return MonitoringResult.WrongState;

        window.CancelledAt = Clock.Now;

        if (notifySubscribers)
            EmailMaintenance(window, "Maintenance cancelled");

        Record("Cancelled maintenance", MaintenanceTarget(window), MaintenanceLink, WindowText(window));
        NotifyMaintenance(window, $"Maintenance cancelled: {window.Title}", $"Alert rules for {ServiceNamesText(window.ServiceIds)} stay on at {FormattableString.Invariant($"{window.Start:MMM d, HH:mm}")}.", fromSystem: false);
        Commit();

        return MonitoringResult.Ok;
    }

    /// <summary>Deletes an alert rule (admins). Incidents it opened stay.</summary>
    public MonitoringResult DeleteRule(int ruleId)
    {
        if (Admin() is null)
            return MonitoringResult.NotAllowed;

        if (rules.FirstOrDefault(rule => rule.Id == ruleId) is not { } doomed)
            return MonitoringResult.NotFound;

        rules.Remove(doomed);
        Record("Deleted alert rule", RuleTarget(doomed.Name), AlertsLink, $"On {FindService(doomed.ServiceId)?.Name ?? doomed.ServiceId}");
        Changed?.Invoke();

        return MonitoringResult.Ok;
    }

    /// <summary>
    /// Deletes a monitored service (admins). A service with incidents, with subscribers who follow it by name, or in a
    /// scheduled or running maintenance window can't be deleted (hide it from the status page instead); its alert rules and
    /// injected failures go with it. The last service can't be deleted.
    /// </summary>
    public MonitoringResult DeleteService(string serviceId)
    {
        if (Admin() is null)
            return MonitoringResult.NotAllowed;

        if (FindService(serviceId) is not { } service)
            return MonitoringResult.NotFound;

        if (services.Count == 1 || incidents.Any(incident => incident.ServiceId == serviceId) || subscriptions.Any(subscription => subscription.ServiceIds.Contains(serviceId))
            || maintenance.Any(window => window.ServiceIds.Contains(serviceId) && window.StatusAt(Clock.Now) is MonitoringMaintenanceStatus.Scheduled or MonitoringMaintenanceStatus.InProgress))
            return MonitoringResult.WrongState;

        services.Remove(service);
        rules.RemoveAll(rule => rule.ServiceId == serviceId);
        faults.RemoveAll(fault => fault.ServiceId == serviceId);

        foreach (var window in maintenance)
            window.ServiceIds.Remove(serviceId);

        // A policy that named only this service has nothing left to cover; incidents already paging keep their copy.
        foreach (var policy in policies)
            policy.ServiceIds.Remove(serviceId);

        foreach (var emptiedPolicy in policies.Where(policy => !policy.IsDefault && policy.ServiceIds.Count == 0).ToList())
        {
            policies.Remove(emptiedPolicy);
            Record("Deleted escalation policy", PolicyTarget(emptiedPolicy.Name), EscalationLink, $"It covered only {service.Name}, which was deleted");
        }

        var emptied = maintenance.Where(window => window.ServiceIds.Count == 0).Select(window => window.Id).ToHashSet();
        maintenance.RemoveAll(window => emptied.Contains(window.Id));
        emails.RemoveAll(email => email.MaintenanceId is { } id && emptied.Contains(id));
        Record("Deleted service", $"Service: {service.Name}", ServicesLink, FormattableString.Invariant($"ID {service.Id}"));
        Commit();

        return MonitoringResult.Ok;
    }

    /// <summary>Ends a maintenance window that is in progress (admins), optionally emailing the subscribers who follow its services.</summary>
    public MonitoringResult EndMaintenanceNow(int maintenanceId, bool notifySubscribers)
    {
        if (Admin() is null)
            return MonitoringResult.NotAllowed;

        if (maintenance.FirstOrDefault(window => window.Id == maintenanceId) is not { } window)
            return MonitoringResult.NotFound;

        if (window.StatusAt(Clock.Now) != MonitoringMaintenanceStatus.InProgress)
            return MonitoringResult.WrongState;

        window.End = Clock.Now > window.Start ? Clock.Now : window.Start.AddSeconds(1);

        if (notifySubscribers)
            EmailMaintenance(window, "Maintenance completed");

        Record("Ended maintenance early", MaintenanceTarget(window), MaintenanceLink, WindowText(window));
        Commit();

        return MonitoringResult.Ok;
    }

    /// <summary>
    /// Injects a failure on a service for a while (admins; a drill): from now its metrics bend like a real incident with that
    /// impact, so alert rules can fire. One failure runs per service at a time.
    /// </summary>
    public MonitoringResult InjectFailure(string serviceId, MonitoringImpact impact, int seconds, out MonitoringFault? fault)
    {
        fault = null;

        if (Admin() is not { } account)
            return MonitoringResult.NotAllowed;

        if (FindService(serviceId) is null)
            return MonitoringResult.NotFound;

        if (!Enum.IsDefined(impact) || seconds is < MinFaultSeconds or > MaxFaultSeconds || nextFaultId >= NextIdLimit)
            return MonitoringResult.Invalid;

        if (ActiveFault(serviceId) is not null)
            return MonitoringResult.WrongState;

        // The oldest finished drills make room.
        while (faults.Count >= MaxFaults && faults.Where(item => item.End <= Clock.Now).OrderBy(item => item.End).FirstOrDefault() is { } oldest)
            faults.Remove(oldest);

        if (faults.Count >= MaxFaults)
            return MonitoringResult.Invalid;

        fault = new MonitoringFault { Id = nextFaultId++, ServiceId = serviceId, Impact = impact, Start = Clock.Now, End = Clock.Now.AddSeconds(seconds), CreatedBy = Clip(account.Name, ShowcaseIdentity.MaxNameLength) };
        faults.Add(fault);
        Record("Started a failure drill", $"Service: {FindService(serviceId)!.Name}", AlertsLink, FormattableString.Invariant($"{ImpactWord(impact)} for {seconds} s"));
        Commit();

        return MonitoringResult.Ok;
    }

    /// <summary>Returns what is wrong with an alert rule's settings, or <see langword="null" /> when they are valid (the same check the state applies).</summary>
    public static string? RuleProblem(string? name, MonitoringAlertMetric metric, double threshold, int forSeconds, MonitoringSeverity severity, bool autoPublish, string? publicMessage)
    {
        if (!IsText(name, MaxRuleNameLength) || name != name!.Trim())
            return FormattableString.Invariant($"Give the rule a name of 1 to {MaxRuleNameLength} characters.");

        if (!Enum.IsDefined(metric) || !Enum.IsDefined(severity))
            return "Choose a metric and a severity.";

        var (low, high) = ThresholdRange(metric);

        if (!double.IsFinite(threshold) || threshold < low || threshold > high)
            return FormattableString.Invariant($"Set a threshold from {low} to {high}.");

        if (forSeconds < MonitoringSeedData.TickSeconds * 2 || forSeconds > MonitoringSeedData.TickSeconds * MonitoringSeedData.HistoryLength || forSeconds % MonitoringSeedData.TickSeconds != 0)
            return FormattableString.Invariant($"Choose a duration from {MonitoringSeedData.TickSeconds * 2} to {MonitoringSeedData.TickSeconds * MonitoringSeedData.HistoryLength} seconds, in steps of {MonitoringSeedData.TickSeconds}.");

        if (autoPublish && (!IsText(publicMessage, MaxUpdateLength) || publicMessage != publicMessage!.Trim()))
            return FormattableString.Invariant($"Write the status page message (1 to {MaxUpdateLength} characters), or turn off posting on the status page.");

        return publicMessage is not null && publicMessage.Length > MaxUpdateLength ? FormattableString.Invariant($"Shorten the status page message to {MaxUpdateLength} characters.") : null;
    }

    /// <summary>
    /// Creates (<paramref name="ruleId" /> 0) or edits an alert rule (admins). See <see cref="RuleProblem" /> for the limits.
    /// Editing a rule doesn't touch incidents it already opened.
    /// </summary>
    public MonitoringResult SaveRule(int ruleId, string name, string serviceId, MonitoringAlertMetric metric, double threshold, int forSeconds, MonitoringSeverity severity,
        bool autoPublish, string publicMessage, bool isEnabled, out MonitoringAlertRule? saved)
    {
        saved = null;

        if (Admin() is null)
            return MonitoringResult.NotAllowed;

        var existing = ruleId == 0 ? null : rules.FirstOrDefault(rule => rule.Id == ruleId);

        if ((ruleId != 0 && existing is null) || FindService(serviceId) is null)
            return MonitoringResult.NotFound;

        name = name?.Trim() ?? string.Empty;
        publicMessage = publicMessage?.Trim() ?? string.Empty;
        var service = FindService(serviceId)!;

        if (RuleProblem(name, metric, threshold, forSeconds, severity, autoPublish, publicMessage) is not null || NormalRangeProblem(service, metric, threshold) is not null)
            return MonitoringResult.Invalid;

        if (autoPublish && !service.IsPublic)
            return MonitoringResult.WrongState;

        if (IsRuleNameTaken(name, ruleId))
            return MonitoringResult.Duplicate;

        var isNew = existing is null;

        if (existing is null)
        {
            if (rules.Count >= MaxRules || nextRuleId >= NextIdLimit)
                return MonitoringResult.Invalid;

            existing = new MonitoringAlertRule { Id = nextRuleId++ };
            rules.Add(existing);
        }
        else if (existing.Name == name && existing.ServiceId == serviceId && existing.Metric == metric && existing.Threshold == threshold && existing.ForSeconds == forSeconds
            && existing.Severity == severity && existing.AutoPublish == autoPublish && existing.PublicMessage == publicMessage && existing.IsEnabled == isEnabled)
        {
            saved = existing;

            return MonitoringResult.NoChange;
        }

        existing.Name = name;
        existing.ServiceId = serviceId;
        existing.Metric = metric;
        existing.Threshold = threshold;
        existing.ForSeconds = forSeconds;
        existing.Severity = severity;
        existing.AutoPublish = autoPublish;
        existing.PublicMessage = publicMessage;
        existing.IsEnabled = isEnabled;
        saved = existing;
        Record(isNew ? "Created alert rule" : "Updated alert rule", RuleTarget(name), AlertsLink,
            $"{MetricLabel(metric)} above {MetricValue(metric, threshold)} for {forSeconds} s on {service.Name}" + (isEnabled ? string.Empty : " (disabled)"));
        Changed?.Invoke();

        return MonitoringResult.Ok;
    }

    /// <summary>
    /// Adds (<paramref name="serviceId" /> null; its ID comes from the name) or edits a monitored service (admins): name,
    /// kind, region, team, description, normal latency and traffic, instances, and whether it is on the status page. The other
    /// normal ranges come from the kind.
    /// </summary>
    public MonitoringResult SaveService(string? serviceId, string name, MonitoringServiceKind kind, string region, string team, string description, double baseLatencyMs,
        double baseRequests, int instances, bool isPublic, out MonitoringService? saved)
    {
        saved = null;

        if (Admin() is null)
            return MonitoringResult.NotAllowed;

        var existing = serviceId is null ? null : FindService(serviceId);

        if (serviceId is not null && existing is null)
            return MonitoringResult.NotFound;

        name = name?.Trim() ?? string.Empty;
        region = region?.Trim() ?? string.Empty;
        team = team?.Trim() ?? string.Empty;
        description = description?.Trim() ?? string.Empty;

        if (!IsText(name, MaxServiceNameLength) || !Enum.IsDefined(kind) || !IsText(region, MaxServiceTagLength) || !IsText(team, MaxServiceTagLength)
            || !IsText(description, MaxServiceDescriptionLength) || !double.IsFinite(baseLatencyMs) || baseLatencyMs is < 0.1 or > 10_000
            || !double.IsFinite(baseRequests) || baseRequests is < 1 or > 100_000 || instances is < 1 or > 10_000
            || !RegionPattern().IsMatch(region) || (kind != MonitoringServiceKind.Devices && instances > MaxServiceInstances))
            return MonitoringResult.Invalid;

        if (services.Any(service => service.Id != serviceId && string.Equals(service.Name, name, StringComparison.OrdinalIgnoreCase)))
            return MonitoringResult.Duplicate;

        if (existing is null)
        {
            var id = Slug(name);

            // "new" is the route of the add-service page, so no service may have it as its ID.
            if (id.Length < 2 || id == "new" || services.Count >= MaxServices)
                return MonitoringResult.Invalid;

            if (FindService(id) is not null)
                return MonitoringResult.Duplicate;

            var (errors, cpu, memory, disk) = KindDefaults(kind);
            saved = new MonitoringService(id, name, kind, region, team, description, baseRequests, baseLatencyMs, errors, cpu, memory, disk, instances, 100) { IsPublic = isPublic };
            services.Add(saved);
        }
        else
        {
            saved = existing with
            {
                Name = name, Kind = kind, Region = region, Team = team, Description = description, BaseLatencyMs = baseLatencyMs, BaseRequests = baseRequests, Instances = instances,
                IsPublic = isPublic,
            };

            if (saved == existing)
                return MonitoringResult.NoChange;

            services[services.IndexOf(existing)] = saved;
        }

        if (!FitsSnapshot())
        {
            if (existing is null)
                services.Remove(saved);
            else
                services[services.IndexOf(saved)] = existing;

            saved = null;

            return MonitoringResult.Invalid;
        }

        Record(existing is null ? "Added service" : "Updated service", $"Service: {saved.Name}", ServiceEditLink(saved.Id), ServiceChanges(existing, saved));
        Commit();

        return MonitoringResult.Ok;
    }

    /// <summary>
    /// Schedules a maintenance window on some services (admins): it starts now or later (within 90 days) and lasts up to
    /// <see cref="MaxMaintenanceLength" />. The status page announces it; <paramref name="notifySubscribers" /> also emails the
    /// subscribers who follow its services. Alert rules don't fire for its services while it runs.
    /// </summary>
    public MonitoringResult ScheduleMaintenance(string title, string description, IReadOnlyList<string> serviceIds, DateTime start, DateTime end, bool notifySubscribers, out MonitoringMaintenance? window)
    {
        window = null;

        if (Admin() is not { } account)
            return MonitoringResult.NotAllowed;

        if (MaintenanceProblem(title, description, serviceIds, start, end, null) is { } problem)
            return problem;

        if (maintenance.Count >= MaxMaintenanceWindows || nextMaintenanceId >= NextIdLimit)
            return MonitoringResult.Invalid;

        window = new MonitoringMaintenance
        {
            Id = nextMaintenanceId++, Title = title.Trim(), Description = description.Trim(), ServiceIds = serviceIds.Distinct().ToList(), Start = start, End = end,
            CreatedBy = Clip(account.Name, ShowcaseIdentity.MaxNameLength),
        };
        maintenance.Add(window);

        if (!FitsSnapshot())
        {
            maintenance.Remove(window);
            window = null;

            return MonitoringResult.Invalid;
        }

        if (notifySubscribers)
            EmailMaintenance(window, "Scheduled maintenance");

        Record("Scheduled maintenance", MaintenanceTarget(window), MaintenanceLink, WindowText(window));
        NotifyMaintenance(window, $"Maintenance scheduled: {window.Title}", $"{WindowText(window)}. Alert rules for {ServiceNamesText(window.ServiceIds)} stay quiet while it runs.", fromSystem: false);
        Commit();

        return MonitoringResult.Ok;
    }

    /// <summary>Turns an alert rule on or off (admins).</summary>
    public MonitoringResult SetRuleEnabled(int ruleId, bool isEnabled)
    {
        if (Admin() is null)
            return MonitoringResult.NotAllowed;

        if (rules.FirstOrDefault(rule => rule.Id == ruleId) is not { } rule)
            return MonitoringResult.NotFound;

        if (rule.IsEnabled == isEnabled)
            return MonitoringResult.NoChange;

        rule.IsEnabled = isEnabled;
        Record(isEnabled ? "Enabled alert rule" : "Disabled alert rule", RuleTarget(rule.Name), AlertsLink, null);
        Changed?.Invoke();

        return MonitoringResult.Ok;
    }

    /// <summary>Stops an injected failure that is running (admins): the metrics return to normal from the next sample.</summary>
    public MonitoringResult StopFailure(int faultId)
    {
        if (Admin() is null)
            return MonitoringResult.NotAllowed;

        if (faults.FirstOrDefault(fault => fault.Id == faultId) is not { } fault)
            return MonitoringResult.NotFound;

        if (fault.End <= Clock.Now)
            return MonitoringResult.NoChange;

        fault.End = Clock.Now > fault.Start ? Clock.Now : fault.Start;
        Record("Stopped a failure drill", $"Service: {FindService(fault.ServiceId)?.Name ?? fault.ServiceId}", AlertsLink, ImpactWord(fault.Impact));
        Commit();

        return MonitoringResult.Ok;
    }

    /// <summary>Changes a scheduled maintenance window (admins); <paramref name="notifySubscribers" /> emails the subscribers who follow its services.</summary>
    public MonitoringResult UpdateMaintenance(int maintenanceId, string title, string description, IReadOnlyList<string> serviceIds, DateTime start, DateTime end, bool notifySubscribers)
    {
        if (Admin() is null)
            return MonitoringResult.NotAllowed;

        if (maintenance.FirstOrDefault(item => item.Id == maintenanceId) is not { } window)
            return MonitoringResult.NotFound;

        if (window.StatusAt(Clock.Now) != MonitoringMaintenanceStatus.Scheduled)
            return MonitoringResult.WrongState;

        if (MaintenanceProblem(title, description, serviceIds, start, end, maintenanceId) is { } problem)
            return problem;

        var ids = serviceIds.Distinct().ToList();

        if (window.Title == title.Trim() && window.Description == description.Trim() && window.ServiceIds.Order().SequenceEqual(ids.Order()) && window.Start == start && window.End == end)
            return MonitoringResult.NoChange;

        var before = (window.Title, window.Description, window.ServiceIds, window.Start, window.End);
        (window.Title, window.Description, window.ServiceIds, window.Start, window.End) = (title.Trim(), description.Trim(), ids, start, end);

        if (!FitsSnapshot())
        {
            (window.Title, window.Description, window.ServiceIds, window.Start, window.End) = before;

            return MonitoringResult.Invalid;
        }

        if (notifySubscribers)
            EmailMaintenance(window, "Maintenance updated");

        Record("Updated maintenance", MaintenanceTarget(window), MaintenanceLink, WindowText(window));
        Commit();

        return MonitoringResult.Ok;
    }

    #endregion

    #region Private Methods

    private static string Clip(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];

    [GeneratedRegex(@"^[^@\s]{1,64}@[^@\s]+\.[^@\s]{2,}$")]
    private static partial Regex EmailPattern();

    private static bool IsDemoAccount(string? accountId) => accountId is not null && MonitoringSeedData.DemoAccounts().Any(account => account.Id == accountId);

    private static bool IsEmail(string? value) => value is not null && value.Length <= MaxEmailLength && EmailPattern().IsMatch(value);

    private static bool IsPlausible(DateTime value) => value >= MonitoringSeedData.Now.AddYears(-3) && value <= MonitoringSeedData.Now.AddYears(2);

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

    private static bool IsWeekStart(DateTime value) =>
        value >= MonitoringSeedData.RotationStart.AddDays(-28) && value <= MonitoringSeedData.RotationStart.AddDays(7 * 104)
        && (value - MonitoringSeedData.RotationStart).Ticks % TimeSpan.FromDays(7).Ticks == 0;

    private static string SeverityWord(MonitoringSeverity severity) => severity switch
    {
        MonitoringSeverity.Critical => "Critical",
        MonitoringSeverity.Major => "Major",
        _ => "Minor",
    };

    private static string StageWord(MonitoringPublicStage stage) => stage switch
    {
        MonitoringPublicStage.Investigating => "Investigating",
        MonitoringPublicStage.Identified => "Identified",
        MonitoringPublicStage.Monitoring => "Monitoring",
        _ => "Resolved",
    };

    private static MonitoringPublicStatus StatusOf(MonitoringSeverity severity) => severity switch
    {
        MonitoringSeverity.Critical => MonitoringPublicStatus.MajorOutage,
        MonitoringSeverity.Major => MonitoringPublicStatus.PartialOutage,
        _ => MonitoringPublicStatus.Degraded,
    };

    [GeneratedRegex("^[0-9a-f]{16}$")]
    private static partial Regex TraceIdPattern();

    private static bool Unique<T>(IEnumerable<T> values)
    {
        var list = values.ToList();

        return list.Distinct().Count() == list.Count;
    }

    private static int WeekIndex(DateTime time) => (int)Math.Floor((time - MonitoringSeedData.RotationStart).TotalDays / 7);

    /// <summary>
    /// What bends a service's metrics at a time: the worst of the incidents active then (from their own start and resolution
    /// times) and the injected failures running then. An outage outranks errors, which outrank latency.
    /// </summary>
    private MonitoringImpact? ImpactAt(string serviceId, DateTime time)
    {
        // An incident a rule opened during a drill is the drill's symptom: the drill bends the metrics, so they recover when it ends.
        var fromIncidents = incidents.Where(incident => incident.ServiceId == serviceId && incident.FaultId is null && incident.StartedAt <= time && (incident.ResolvedAt is null || time < incident.ResolvedAt))
            .Select(incident => incident.Impact);
        var fromFaults = faults.Where(fault => fault.ServiceId == serviceId && fault.Start <= time && time < fault.End).Select(fault => fault.Impact);
        var all = fromIncidents.Concat(fromFaults).ToList();

        return all.Count == 0 ? null
            : all.Contains(MonitoringImpact.Outage) ? MonitoringImpact.Outage
            : all.Contains(MonitoringImpact.Errors) ? MonitoringImpact.Errors
            : MonitoringImpact.Latency;
    }

    private void Commit()
    {
        version++;
        RebuildHistory();
        Changed?.Invoke();
    }

    /// <summary>Puts one email per matching subscription in the outbox; the oldest go when it is full.</summary>
    private void EmailSubscribers(MonitoringIncident incident, MonitoringPublicStage stage, string text)
    {
        var service = FindService(incident.ServiceId);

        SendEmails(new[] { incident.ServiceId }, Clip($"[{StageWord(stage)}] {incident.Title}", 200),
            $"{service?.Name ?? incident.ServiceId}: {StageWord(stage)}\n\n{text}\n\nView the incident on {MonitoringSeedData.StatusPageName}. You get this email because you subscribed to updates in this demo tab.",
            incident.Id, null);
    }

    /// <summary>Emails the subscribers who follow any of the window's services about a scheduled maintenance window.</summary>
    private void EmailMaintenance(MonitoringMaintenance window, string headline)
    {
        // Only the services on the status page are named; a window with none of them sends nothing (see SendEmails).
        var names = string.Join(", ", window.ServiceIds.Where(IsOnStatusPage).Select(id => FindService(id)!.Name));
        var end = window.End.Date == window.Start.Date ? FormattableString.Invariant($"{window.End:HH:mm}") : FormattableString.Invariant($"{window.End:ddd, MMM d, HH:mm}");

        SendEmails(window.ServiceIds, Clip($"[{headline}] {window.Title}", 200),
            FormattableString.Invariant($"{names}: {headline}\n\n{window.Start:ddd, MMM d, HH:mm} to {end} (site time)\n\n{window.Description}\n\nYou get this email because you subscribed to updates in this demo tab."),
            null, window.Id);
    }

    /// <summary>Puts one email per subscription that follows any of the services in the outbox; the oldest go when it is full or the snapshot would be too large.</summary>
    private void SendEmails(IReadOnlyCollection<string> serviceIds, string subject, string body, int? incidentId, int? maintenanceId)
    {
        // Nothing about services the status page doesn't show goes to subscribers.
        var publicIds = serviceIds.Where(IsOnStatusPage).ToList();

        if (publicIds.Count == 0)
            return;

        foreach (var subscription in subscriptions.Where(item => item.ServiceIds.Count == 0 || item.ServiceIds.Any(publicIds.Contains)))
        {
            if (nextEmailId >= NextIdLimit)
                return;

            emails.Add(new MonitoringEmail(nextEmailId++, subscription.Id, subscription.Email, subject, body, Clock.Now, incidentId) { MaintenanceId = maintenanceId });

            while (emails.Count > MaxEmails)
                emails.RemoveAt(0);
        }

        // The outbox gives way first: the oldest emails go until the snapshot fits, so saving never stops.
        while (emails.Count > 0 && !FitsSnapshot())
            emails.RemoveAt(0);
    }

    /// <summary>The signed-in admin, or <see langword="null" />.</summary>
    private ShowcaseAccount? Admin() => Identity.Current is { RoleId: MonitoringSeedData.AdminRole } account ? account : null;

    /// <summary>The signed-in engineer or admin (admins have every engineer action), or <see langword="null" />.</summary>
    private ShowcaseAccount? Engineer() => Identity.Current is { RoleId: MonitoringSeedData.EngineerRole or MonitoringSeedData.AdminRole } account ? account : null;

    /// <summary>Whether a service exists and is a component on the public status page.</summary>
    private bool IsOnStatusPage(string serviceId) => FindService(serviceId) is { IsPublic: true };

    /// <summary>Whether the snapshot, as it would be saved now, still fits under <see cref="MaxSnapshotLength" />.</summary>
    private bool FitsSnapshot()
    {
        try
        {
            return JsonSerializer.Serialize(CaptureData(), snapshotOptions).Length <= MaxSnapshotLength;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            return false;
        }
    }

    /// <summary>The log lines for the shown tick: the seeded two hours, three generated lines per tick since, and incident events; newest first.</summary>
    private IReadOnlyList<MonitoringLogEntry> BuildLogs()
    {
        var lines = seedLogs.Where(entry => FindService(entry.ServiceId) is not null).ToList();
        var now = CurrentTime;

        for (var t = Math.Max(1, displayTick - LiveLogTicks + 1); t <= displayTick; t++)
        {
            for (var i = 0; i < 3; i++)
            {
                var id = 10_001 + ((t - 1) * 3) + i;
                var service = Services[(int)(MonitoringSeedData.Hash(id, 17, i) * 1000) % Services.Count];
                var time = SampleTime(t).AddMilliseconds(-i * 350);
                var level = ImpactAt(service.Id, time) switch
                {
                    MonitoringImpact.Errors or MonitoringImpact.Outage => i == 1 ? MonitoringLogLevel.Warning : MonitoringLogLevel.Error,
                    MonitoringImpact.Latency => MonitoringLogLevel.Warning,
                    _ => MonitoringSeedData.Hash(id, 4, 4) < 0.05 ? MonitoringLogLevel.Warning : MonitoringLogLevel.Info,
                };

                lines.Add(new MonitoringLogEntry(id, time, service.Id, level, MonitoringSeedData.Message(service, level, id), MonitoringSeedData.TraceId(id)));
            }
        }

        // Incident events after the seed time, as the alerting system logs them.
        foreach (var incident in incidents.Where(incident => incident.Id >= FirstIncidentId || incident.StartedAt > MonitoringSeedData.Now || incident.ResolvedAt > MonitoringSeedData.Now))
        {
            var service = FindService(incident.ServiceId)!;

            if (incident.StartedAt > MonitoringSeedData.Now && incident.StartedAt <= now)
                lines.Add(new MonitoringLogEntry(2_000_000 + (incident.Id * 10) + 1, incident.StartedAt, service.Id, incident.Severity == MonitoringSeverity.Critical ? MonitoringLogLevel.Error : MonitoringLogLevel.Warning,
                    $"Incident #{incident.Id} opened: {Clip(incident.Title, 80)}", MonitoringSeedData.TraceId(2_000_000 + incident.Id)));

            if (incident.ResolvedAt is { } resolved && resolved > MonitoringSeedData.Now && resolved <= now)
                lines.Add(new MonitoringLogEntry(2_000_000 + (incident.Id * 10) + 2, resolved, service.Id, MonitoringLogLevel.Info,
                    $"Incident #{incident.Id} resolved; {service.Name} back to normal", MonitoringSeedData.TraceId(3_000_000 + incident.Id)));
        }

        return lines.OrderByDescending(entry => entry.Time).ThenByDescending(entry => entry.Id).Take(MaxLogLines).ToList();
    }

    /// <summary>
    /// Raises a notification for a demo engineer (rule WR-001); nothing for an engineer without a sign-in, or for the person
    /// who caused it unless the monitoring system raised it (<paramref name="fromSystem" />). The newest
    /// <see cref="MaxNotifications" /> are kept.
    /// </summary>
    private void Notify(string? accountId, MonitoringNotificationKind kind, string title, string text, int? incidentId, string? url = null, bool fromSystem = false, DateTime? at = null)
    {
        if (accountId is null || !IsDemoAccount(accountId) || nextNotificationId >= NextIdLimit || (!fromSystem && accountId == Identity.Current?.Id))
            return;

        notifications.Add(new MonitoringNotification
        {
            Id = nextNotificationId++, AccountId = accountId, Kind = kind, Time = at ?? Clock.Now, Title = Clip(title, 200), Text = Clip(text, 500), IncidentId = incidentId, Url = url,
        });

        while (notifications.Count > MaxNotifications)
            notifications.RemoveAt(0);
    }

    /// <summary>
    /// Notifies this week's on-call engineers who have a demo account (unless <paramref name="onCall" /> is off because an
    /// escalation policy pages them), and the demo engineers whose team owns the incident's service (once each). An alert a
    /// rule fires (<paramref name="fromSystem" />) reaches the signed-in person too.
    /// </summary>
    private void NotifyOnCall(MonitoringIncident incident, string title, string text, bool fromSystem = false, bool onCall = true)
    {
        var shift = ShiftForWeek(WeekIndex(Clock.Now));
        var told = new HashSet<string>(StringComparer.Ordinal) { shift.PrimaryId, shift.SecondaryId };

        if (onCall)
        {
            foreach (var engineerId in told)
                Notify(engineerId, MonitoringNotificationKind.Alert, title, text, incident.Id, fromSystem: fromSystem);
        }

        if (FindService(incident.ServiceId) is not { } service)
            return;

        foreach (var owner in Engineers.Where(engineer => engineer.Team == service.Team && !told.Contains(engineer.Id)))
            Notify(owner.Id, MonitoringNotificationKind.Alert, title, $"{text} Your team owns {service.Name}.", incident.Id, fromSystem: fromSystem);
    }

    private void RebuildHistory()
    {
        history.Clear();

        foreach (var service in Services)
        {
            var index = WaveIndex(service.Id);
            history[service.Id] = Enumerable.Range(displayTick - MonitoringSeedData.HistoryLength + 1, MonitoringSeedData.HistoryLength).Select(t => Sample(service, index, t)).ToList();
        }

        logCache = null;
    }

    /// <summary>
    /// A service's place in the metric waves: a seeded service keeps its seed position; an added one gets a stable number from
    /// its ID. So adding or deleting a service never changes another service's metrics.
    /// </summary>
    private static int WaveIndex(string serviceId)
    {
        var seeded = seedServiceIds.IndexOf(serviceId);

        if (seeded >= 0)
            return seeded;

        var hash = 17;

        foreach (var c in serviceId)
            hash = unchecked((hash * 31) + c);

        return 8 + Math.Abs(hash % 89);
    }

    /// <summary>A deterministic sample: a slow wave plus hash noise around the normal range, bent by the incident active at its time.</summary>
    private MonitoringSample Sample(MonitoringService service, int index, int at)
    {
        double Wave(int metric, double amplitude) =>
            1 + (amplitude * Math.Sin((at + (5 * index) + (3 * metric)) / 4.0)) + (amplitude * 0.7 * ((MonitoringSeedData.Hash(at, index, metric) * 2) - 1));

        var time = SampleTime(at);
        var requests = service.BaseRequests * Wave(0, 0.08);
        var latency = service.BaseLatencyMs * Wave(1, 0.07);
        var errors = Math.Max(0, service.BaseErrorRate * Wave(2, 0.3));
        var cpu = Math.Clamp(service.BaseCpu * Wave(3, 0.06), 0, 100);
        var memory = Math.Clamp(service.BaseMemory * Wave(4, 0.02), 0, 100);

        switch (ImpactAt(service.Id, time))
        {
            case MonitoringImpact.Errors:
                errors = 6 + (1.5 * Math.Sin(at / 3.0)) + MonitoringSeedData.Hash(at, 3, 1);
                break;

            case MonitoringImpact.Latency:
                latency *= 2.6;
                cpu = Math.Min(100, cpu * 1.5);
                break;

            case MonitoringImpact.Outage:
                errors = 45 + (8 * Math.Sin(at / 3.0)) + (4 * MonitoringSeedData.Hash(at, 5, 1));
                requests *= 0.4;
                latency *= 4;
                break;
        }

        return new MonitoringSample(at, time, requests, latency, errors, cpu, memory);
    }

    private static DateTime SampleTime(int at) => MonitoringSeedData.Now.AddSeconds(at * MonitoringSeedData.TickSeconds);

    /// <summary>The week's rotation (primary is the week's engineer, secondary the next one), with overrides applied.</summary>
    private MonitoringShift ShiftForWeek(int week)
    {
        var order = MonitoringSeedData.RotationOrder;
        var start = MonitoringSeedData.RotationStart.AddDays(week * 7);
        var slot = ((week % order.Count) + order.Count) % order.Count;
        var primary = overrides.FirstOrDefault(item => item.ShiftStart == start && item.Role == MonitoringOnCallRole.Primary);
        var secondary = overrides.FirstOrDefault(item => item.ShiftStart == start && item.Role == MonitoringOnCallRole.Secondary);

        return new MonitoringShift(start, start.AddDays(7), primary?.EngineerId ?? order[slot], secondary?.EngineerId ?? order[(slot + 1) % order.Count])
        {
            PrimaryOverride = primary,
            SecondaryOverride = secondary,
        };
    }

    /// <summary>Adds a timeline entry if the incident has room and the snapshot still fits; returns whether it was added.</summary>
    private bool TryAddUpdate(MonitoringIncident incident, ShowcaseAccount account, string text, MonitoringUpdateKind kind, MonitoringPublicStage? stage, List<MonitoringIncidentUpdate>? added = null)
    {
        if (incident.Updates.Count >= MaxUpdatesPerIncident || nextUpdateId >= NextIdLimit)
            return false;

        var update = new MonitoringIncidentUpdate(Clock.Now, Clip(account.Name, ShowcaseIdentity.MaxNameLength), text) { Id = nextUpdateId++, Kind = kind, Stage = stage, AccountId = account.Id };
        incident.Updates.Add(update);

        if (!FitsSnapshot())
        {
            incident.Updates.Remove(update);

            return false;
        }

        added?.Add(update);

        return true;
    }

    /// <summary>Evaluates the enabled alert rules on the shown window; returns whether any opened an incident.</summary>
    private bool EvaluateRules()
    {
        var fired = false;

        foreach (var rule in rules.Where(rule => rule.IsEnabled).ToList())
        {
            // An alert doesn't fire on a service that already has an incident, or that is in maintenance.
            if (FindService(rule.ServiceId) is not { } service || ActiveIncident(service.Id) is not null || ActiveMaintenance(service.Id) is not null)
                continue;

            var count = Math.Clamp(rule.ForSeconds / MonitoringSeedData.TickSeconds, 1, MonitoringSeedData.HistoryLength);
            var samples = history[service.Id].TakeLast(count).ToList();

            // After an incident on the service is resolved, the rule needs a full window of new samples before it fires again.
            var lastResolved = incidents.Where(incident => incident.ServiceId == service.Id && incident.ResolvedAt is not null).Select(incident => incident.ResolvedAt!.Value).DefaultIfEmpty(DateTime.MinValue).Max();

            if (samples.Count < count || samples[0].Time <= lastResolved || !samples.All(sample => MetricOf(rule.Metric, sample) > rule.Threshold))
                continue;

            if (incidents.Count >= MaxIncidents || nextIncidentId >= NextIdLimit || nextUpdateId >= NextIdLimit - 2)
                continue;

            var latest = MetricOf(rule.Metric, samples[^1]);
            var drill = faults.FirstOrDefault(fault => fault.ServiceId == service.Id && fault.Start <= samples[^1].Time && samples[^1].Time < fault.End);

            // A hidden service's incidents never go on the status page.
            var publish = rule.AutoPublish && service.IsPublic;
            var impact = ImpactAt(service.Id, samples[^1].Time)
                ?? (rule.Metric == MonitoringAlertMetric.ErrorRate ? (latest >= 20 ? MonitoringImpact.Outage : MonitoringImpact.Errors) : MonitoringImpact.Latency);
            var incident = new MonitoringIncident
            {
                Id = nextIncidentId++, RuleId = rule.Id, FaultId = drill?.Id, ServiceId = service.Id, Severity = rule.Severity, Impact = impact, Status = MonitoringIncidentStatus.Open,
                StartedAt = samples[0].Time, Title = Clip(rule.Name, MaxIncidentTitleLength),
                Summary = Clip($"{MetricLabel(rule.Metric)} stayed above {MetricValue(rule.Metric, rule.Threshold)} for {rule.ForSeconds} s on {service.Name} (latest {MetricValue(rule.Metric, latest)}).", MaxIncidentSummaryLength),
            };

            incident.Updates.Add(new MonitoringIncidentUpdate(samples[^1].Time, MonitoringSeedData.SystemAuthor,
                Clip($"Alert fired: {rule.Name}. {incident.Summary}", MaxUpdateLength)) { Id = nextUpdateId++, Kind = MonitoringUpdateKind.Alert });

            if (publish)
                incident.Updates.Add(new MonitoringIncidentUpdate(samples[^1].Time, MonitoringSeedData.SystemAuthor, rule.PublicMessage) { Id = nextUpdateId++, Kind = MonitoringUpdateKind.Public, Stage = MonitoringPublicStage.Investigating });

            incidents.Add(incident);

            if (!FitsSnapshot())
            {
                incidents.Remove(incident);

                continue;
            }

            rule.LastFiredAt = samples[^1].Time;
            rule.LastIncidentId = incident.Id;

            if (publish)
                EmailSubscribers(incident, MonitoringPublicStage.Investigating, rule.PublicMessage);

            Record(samples[^1].Time, MonitoringSeedData.SystemAuthor, "Alert fired", IncidentTarget(incident.Id), IncidentLink(incident.Id),
                $"{rule.Name}{(publish ? "; posted on the status page" : string.Empty)}");
            // Under an escalation policy the on-call engineers are paged instead (the page is their notification).
            var paged = StartPaging(incident, samples[^1].Time);
            NotifyOnCall(incident, $"{SeverityWord(rule.Severity)} alert: {rule.Name}", $"#{incident.Id} opened on {service.Name}.{(publish ? " It's posted on the status page." : string.Empty)}",
                fromSystem: true, onCall: !paged);
            fired = true;
        }

        return fired;
    }

    /// <summary>The normal ranges a new service gets from its kind: error rate, CPU, memory, disk.</summary>
    private static (double Errors, double Cpu, double Memory, double Disk) KindDefaults(MonitoringServiceKind kind) => kind switch
    {
        MonitoringServiceKind.Api => (0.3, 40, 55, 30),
        MonitoringServiceKind.Worker => (0.5, 45, 60, 25),
        MonitoringServiceKind.Database => (0, 55, 70, 50),
        MonitoringServiceKind.Cache => (0, 20, 70, 10),
        _ => (0.8, 20, 35, 20),
    };

    /// <summary>
    /// Checks a maintenance window's fields; returns the refusal, or <see langword="null" /> when they are valid. A window
    /// may not overlap another scheduled or running window on the same service (<see cref="MonitoringResult.Duplicate" />).
    /// </summary>
    private MonitoringResult? MaintenanceProblem(string? title, string? description, IReadOnlyList<string>? serviceIds, DateTime start, DateTime end, int? exceptId)
    {
        if (serviceIds is null || serviceIds.Count == 0 || serviceIds.Any(id => FindService(id) is null))
            return serviceIds is { Count: > 0 } ? MonitoringResult.NotFound : MonitoringResult.Invalid;

        if (!IsText(title?.Trim(), MaxMaintenanceTitleLength) || !IsText(description?.Trim(), MaxMaintenanceDescriptionLength)
            || start < Clock.Now.AddMinutes(-1) || start > Clock.Now.AddDays(90) || end <= start || end - start > MaxMaintenanceLength)
            return MonitoringResult.Invalid;

        return OverlappingMaintenance(serviceIds, start, end, exceptId) is not null ? MonitoringResult.Duplicate : null;
    }

    private static string MetricLabel(MonitoringAlertMetric metric) => metric switch
    {
        MonitoringAlertMetric.ErrorRate => "Error rate",
        MonitoringAlertMetric.Latency => "p95 latency",
        _ => "CPU",
    };

    private static double MetricOf(MonitoringAlertMetric metric, MonitoringSample sample) => metric switch
    {
        MonitoringAlertMetric.ErrorRate => sample.ErrorRate,
        MonitoringAlertMetric.Latency => sample.LatencyMs,
        _ => sample.Cpu,
    };

    private static string MetricValue(MonitoringAlertMetric metric, double value) =>
        metric == MonitoringAlertMetric.Latency ? FormattableString.Invariant($"{value:0} ms") : FormattableString.Invariant($"{value:0.#}%");

    /// <summary>A service ID from its name: lower-case letters, digits, and hyphens.</summary>
    private static string Slug(string name)
    {
        var slug = SlugPattern().Replace(name.ToLowerInvariant(), "-").Trim('-');

        return slug.Length > 30 ? slug[..30].TrimEnd('-') : slug;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex SlugPattern();

    /// <summary>The thresholds an alert rule may use for a metric: percent for error rate and CPU, milliseconds for latency.</summary>
    public static (double Low, double High) ThresholdRange(MonitoringAlertMetric metric) => metric switch
    {
        MonitoringAlertMetric.Latency => (1, 60_000),
        MonitoringAlertMetric.Cpu => (1, 100),
        _ => (0.1, 100),
    };

    [GeneratedRegex("^[a-z0-9][a-z0-9-]{1,29}$")]
    private static partial Regex ServiceIdPattern();

    /// <summary>A region code like <c>us-east-1</c>: lower-case letters and digits in parts joined by hyphens, starting with a letter.</summary>
    [GeneratedRegex("^[a-z][a-z0-9]*(-[a-z0-9]+)+$")]
    private static partial Regex RegionPattern();

    /// <summary>Checks one stored service: its ID, text limits, kind, and normal ranges.</summary>
    private static bool ValidService(MonitoringService? service) =>
        service is not null && service.Id is not null && ServiceIdPattern().IsMatch(service.Id) && service.Id != "new" && IsText(service.Name, MaxServiceNameLength) && Enum.IsDefined(service.Kind)
        && IsText(service.Region, MaxServiceTagLength) && IsText(service.Team, MaxServiceTagLength) && IsText(service.Description, MaxServiceDescriptionLength)
        && InRange(service.BaseRequests, 1, 100_000) && InRange(service.BaseLatencyMs, 0.1, 10_000) && InRange(service.BaseErrorRate, 0, 10) && InRange(service.BaseCpu, 0, 100)
        && InRange(service.BaseMemory, 0, 100) && InRange(service.DiskUsed, 0, 100) && service.Instances is >= 1 and <= 10_000 && InRange(service.Uptime30Days, 0, 100);

    private static bool InRange(double value, double low, double high) => double.IsFinite(value) && value >= low && value <= high;

    /// <summary>
    /// Checks a stored incident's paging: the policy copy's limits, pages within its schedule (step, round, a known engineer),
    /// a consistent stop (paging runs only while the incident is open, and an acknowledgement names who).
    /// </summary>
    private bool ValidPaging(MonitoringIncident incident, MonitoringPaging paging, MonitoringSnapshot snapshot)
    {
        if (paging.PolicyId < 1 || paging.PolicyId >= snapshot.NextPolicyId || paging.Pages is null || !IsPlausible(paging.StartedAt) || paging.StartedAt < incident.StartedAt.AddMinutes(-1)
            || PolicyProblem(paging.PolicyName, true, null, paging.SecondaryAfterMinutes, paging.ThirdEngineerId, paging.ThirdAfterMinutes, paging.Repeats) is not null)
            return false;

        var plan = PagingSchedule(paging.SecondaryAfterMinutes, paging.ThirdAfterMinutes, paging.Repeats);

        if (paging.Pages.Count > plan.Count || paging.Pages.Select((page, index) => (page, index)).Any(item => item.page is null || item.page.Step != plan[item.index].Step
                || item.page.Round != plan[item.index].Round || FindEngineer(item.page.EngineerId) is null || !IsPlausible(item.page.At) || !IsText(item.page.Channels, 80)))
            return false;

        if ((paging.StopReason is null) != (paging.StoppedAt is null) || (paging.StopReason is { } reason && !Enum.IsDefined(reason))
            || (paging.StoppedAt is { } stopped && !IsPlausible(stopped)) || (paging.StopReason is null && incident.Status != MonitoringIncidentStatus.Open))
            return false;

        return (paging.AcknowledgedBy is null) == (paging.AcknowledgedAt is null) && (paging.AcknowledgedBy is null || (IsText(paging.AcknowledgedBy, ShowcaseIdentity.MaxNameLength)
            && paging.StopReason == MonitoringPagingStop.Acknowledged && IsPlausible(paging.AcknowledgedAt!.Value)));
    }

    /// <summary>Checks one stored incident: references, enums, text limits, times, the timeline, the postmortem, and that a published incident's public stage matches its status.</summary>
    private bool ValidIncident(MonitoringIncident? incident, MonitoringSnapshot snapshot, HashSet<string> serviceIds)
    {
        if (incident is null || incident.Id < 1 || incident.Id >= snapshot.NextIncidentId || !serviceIds.Contains(incident.ServiceId) || !Enum.IsDefined(incident.Severity)
            || !Enum.IsDefined(incident.Impact) || !Enum.IsDefined(incident.Status) || !IsText(incident.Title, MaxIncidentTitleLength) || !IsText(incident.Summary, MaxIncidentSummaryLength)
            || !IsPlausible(incident.StartedAt) || (incident.Assignee is not null && !IsText(incident.Assignee, ShowcaseIdentity.MaxNameLength))
            || (incident.Status == MonitoringIncidentStatus.Resolved) != (incident.ResolvedAt is not null)
            || (incident.ResolvedAt is { } resolved && (!IsPlausible(resolved) || resolved < incident.StartedAt))
            || incident.Updates is not { Count: > 0 and <= MaxUpdatesPerIncident })
            return false;

        if (incident.Updates.Any(update => update is null || update.Id < 1 || update.Id >= snapshot.NextUpdateId || !IsText(update.Author, ShowcaseIdentity.MaxNameLength)
                // Status and severity entries add a prefix to the reason ("Severity changed from Critical to Major: " is 41 characters).
                || !IsText(update.Text, MaxUpdateLength + UpdatePrefixRoom) || !Enum.IsDefined(update.Kind) || !IsPlausible(update.Time)
                || (update.Kind == MonitoringUpdateKind.Public) != (update.Stage is not null) || (update.Stage is { } stage && !Enum.IsDefined(stage))
                || (update.AccountId is not null && !IsDemoAccount(update.AccountId))))
            return false;

        if (incident.IsPublished && (incident.Status == MonitoringIncidentStatus.Resolved) != (incident.PublicStage == MonitoringPublicStage.Resolved))
            return false;

        if (incident.Paging is { } paging && !ValidPaging(incident, paging, snapshot))
            return false;

        return incident.Postmortem is not { } postmortem
            || (IsText(postmortem.Summary, MaxPostmortemLength) && IsText(postmortem.RootCause, MaxPostmortemLength) && IsText(postmortem.Author, ShowcaseIdentity.MaxNameLength)
                && postmortem.ActionItems is { Count: <= MaxActionItems } && postmortem.ActionItems.All(item => IsText(item, MaxActionItemLength))
                && IsPlausible(postmortem.WrittenAt) && (postmortem.PublishedAt is not { } published || (IsPlausible(published) && incident.IsPublished)));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the site's clock, which moves on from the seed time (D-029).</summary>
    public ShowcaseClock Clock { get; } = new(MonitoringSeedData.Now);

    /// <summary>Gets the time of the latest shown sample.</summary>
    public DateTime CurrentTime => SampleTime(displayTick);

    /// <summary>Gets the engineers on the rotation.</summary>
    public IReadOnlyList<MonitoringEngineer> Engineers { get; } = MonitoringSeedData.Engineers();

    /// <summary>Gets the site's demo sign-in.</summary>
    public ShowcaseIdentity Identity { get; }

    /// <summary>Gets every incident (engineers' view), newest first.</summary>
    public IReadOnlyList<MonitoringIncident> Incidents => incidents.OrderByDescending(incident => incident.StartedAt).ToList();

    /// <summary>Gets whether the theme switcher reported the dark color mode.</summary>
    public bool IsDarkTheme => isDarkTheme;

    /// <summary>Gets whether the signed-in account is an admin.</summary>
    public bool IsAdmin => Identity.Current?.RoleId == MonitoringSeedData.AdminRole;

    /// <summary>Gets whether the signed-in account uses the console: an engineer or an admin.</summary>
    public bool IsEngineer => Identity.Current?.RoleId is MonitoringSeedData.EngineerRole or MonitoringSeedData.AdminRole;

    /// <summary>Gets or sets whether live updates are paused; kept here so the choice holds across pages (WCAG 2.2.2).</summary>
    public bool IsPaused { get; set; }

    /// <summary>Gets the clock's latest tick (the window catches up to it on <see cref="Tick" />).</summary>
    public int LiveTick => (int)Math.Floor(Clock.Elapsed.TotalSeconds / MonitoringSeedData.TickSeconds);

    /// <summary>Gets the log lines for the shown window, newest first (the seeded two hours and the lines since; up to 500).</summary>
    public IReadOnlyList<MonitoringLogEntry> Logs
    {
        get
        {
            if (logCache is not { } cache || cache.Tick != displayTick || cache.Version != version)
                logCache = cache = (displayTick, version, BuildLogs());

            return cache.Lines;
        }
    }

    /// <summary>Gets the signed-in engineer's notifications, newest first.</summary>
    public IReadOnlyList<MonitoringNotification> Notifications =>
        Engineer() is { } account ? notifications.Where(notification => notification.AccountId == account.Id).OrderByDescending(notification => notification.Time).ThenByDescending(notification => notification.Id).ToList() : Array.Empty<MonitoringNotification>();

    /// <summary>Gets the injected failures, newest first.</summary>
    public IReadOnlyList<MonitoringFault> Faults => faults.OrderByDescending(fault => fault.Start).ThenByDescending(fault => fault.Id).ToList();

    /// <summary>Gets every maintenance window, the latest start first.</summary>
    public IReadOnlyList<MonitoringMaintenance> MaintenanceWindows => maintenance.OrderByDescending(window => window.Start).ToList();

    /// <summary>Gets the status page's overall status: the worst of its components.</summary>
    public MonitoringPublicStatus OverallPublicStatus => StatusPageServices.Select(service => PublicStatus(service.Id)).DefaultIfEmpty(MonitoringPublicStatus.Operational).Max();

    /// <summary>Gets the on-call overrides, newest first.</summary>
    public IReadOnlyList<MonitoringOverride> Overrides => overrides.OrderByDescending(item => item.CreatedAt).ToList();

    /// <summary>Gets how the signed-in engineer wants to be reached (the defaults until saved).</summary>
    public MonitoringNotificationPreferences Preferences =>
        Engineer() is { } account && preferences.TryGetValue(account.Id, out var saved) ? saved : new MonitoringNotificationPreferences();

    /// <summary>Gets the published incidents that are still ongoing on the status page (on services it shows), newest first.</summary>
    public IReadOnlyList<MonitoringIncident> PublicActiveIncidents =>
        incidents.Where(incident => incident.IsPublished && incident.PublicStage != MonitoringPublicStage.Resolved && IsOnStatusPage(incident.ServiceId)).OrderByDescending(incident => incident.StartedAt).ToList();

    /// <summary>Gets every published incident on a service the status page shows (its history), newest first.</summary>
    public IReadOnlyList<MonitoringIncident> PublicIncidents => incidents.Where(incident => incident.IsPublished && IsOnStatusPage(incident.ServiceId)).OrderByDescending(incident => incident.StartedAt).ToList();

    /// <summary>Gets the maintenance windows the status page announces: in progress, and scheduled within the next 14 days; soonest first.</summary>
    public IReadOnlyList<MonitoringMaintenance> PublicMaintenance =>
        maintenance.Where(window => window.StatusAt(Clock.Now) is MonitoringMaintenanceStatus.InProgress
                || (window.StatusAt(Clock.Now) == MonitoringMaintenanceStatus.Scheduled && window.Start <= Clock.Now.AddDays(14)))
            .Where(window => window.ServiceIds.Any(id => FindService(id) is { IsPublic: true }))
            .OrderBy(window => window.Start).ToList();

    /// <summary>Gets the alert rules, in number order.</summary>
    public IReadOnlyList<MonitoringAlertRule> Rules => rules.OrderBy(rule => rule.Id).ToList();

    /// <summary>Gets the monitored services, in the order they were added.</summary>
    public IReadOnlyList<MonitoringService> Services => services;

    /// <summary>Gets the services shown as components on the public status page.</summary>
    public IReadOnlyList<MonitoringService> StatusPageServices => services.Where(service => service.IsPublic).ToList();

    /// <summary>Gets the site's slug, used in the storage key.</summary>
    public string SiteSlug => "monitoring";

    /// <summary>Gets the snapshot format version (4: escalation policies and the incidents' paging).</summary>
    public int SnapshotVersion => 4;

    /// <summary>Gets the status page subscriptions made in this tab, oldest first.</summary>
    public IReadOnlyList<MonitoringSubscription> Subscriptions => subscriptions;

    /// <summary>Gets how many of the signed-in engineer's notifications are unread.</summary>
    public int UnreadCount => Engineer() is { } account ? notifications.Count(notification => notification.AccountId == account.Id && !notification.IsRead) : 0;

    #endregion

    #region Events

    /// <summary>Raised when anything in the state changes, including sign-in.</summary>
    public event Action? Changed;

    /// <summary>Raised when the theme switcher changes the color mode.</summary>
    public event Action? ThemeChanged;

    /// <summary>Raised after <see cref="Tick" /> moved the shown window.</summary>
    public event Action? Ticked;

    #endregion

    #region Nested Types

    /// <summary>The monitoring site's part of the tab snapshot.</summary>
    private sealed class MonitoringSnapshot
    {
        #region Properties, Indexers

        public List<ShowcaseAuditEntry>? Audit { get; set; }

        public List<MonitoringEmail>? Emails { get; set; }

        public List<MonitoringFault>? Faults { get; set; }

        public List<MonitoringIncident>? Incidents { get; set; }

        public List<MonitoringMaintenance>? Maintenance { get; set; }

        public int NextAuditId { get; set; }

        public int NextEmailId { get; set; }

        public int NextFaultId { get; set; }

        public int NextIncidentId { get; set; }

        public int NextMaintenanceId { get; set; }

        public int NextRuleId { get; set; }

        public int NextNotificationId { get; set; }

        public int NextOverrideId { get; set; }

        public int NextPolicyId { get; set; }

        public int NextSubscriptionId { get; set; }

        public int NextUpdateId { get; set; }

        public List<MonitoringNotification>? Notifications { get; set; }

        public List<MonitoringOverride>? Overrides { get; set; }

        public List<MonitoringEscalationPolicy>? Policies { get; set; }

        public Dictionary<string, MonitoringNotificationPreferences>? Preferences { get; set; }

        public List<MonitoringAlertRule>? Rules { get; set; }

        public List<MonitoringService>? Services { get; set; }

        public List<MonitoringSubscription>? Subscriptions { get; set; }

        #endregion
    }

    #endregion
}
