using System.Text.Json;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

/// <summary>
/// State of the "Stackwatch" monitoring site for the current browser tab: incidents with their internal and public
/// timelines, status page subscriptions and the demo email outbox, engineers' notifications and preferences, on-call
/// overrides, and the demo sign-in. It is kept in the tab's <c>sessionStorage</c> through the showcase foundation; nothing
/// is sent.
/// </summary>
/// <remarks>
/// Metrics and log lines are not stored: they are generated deterministically for the sample ticks of the site's moving
/// clock (one every <see cref="MonitoringSeedData.TickSeconds" /> seconds, D-029), bent by the incidents active at each
/// sample's time. <see cref="Tick" /> moves the shown window up to the clock; live pages call it from a timer, and pausing
/// stops it. The public status page reads only what engineers published: an incident appears once it has a public update,
/// and is ongoing there until a public update says it is resolved.
/// </remarks>
public sealed partial class MonitoringState : IShowcaseSiteState
{
    #region Fields and Constants

    /// <summary>The longest postmortem action item.</summary>
    public const int MaxActionItemLength = 300;

    /// <summary>The most postmortem action items.</summary>
    public const int MaxActionItems = 10;

    /// <summary>The longest email address.</summary>
    public const int MaxEmailLength = 120;

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

    private const int MaxIncidents = 300;

    private const int MaxLogLines = 500;

    private const int MaxNotifications = 300;

    private const int MaxOverrides = 100;

    /// <summary>The largest the site's snapshot may grow: the store's limit less room for its envelope.</summary>
    private const int MaxSnapshotLength = ShowcaseSessionStore.MaxSnapshotLength - (48 * 1024);

    private const int MaxUpdatesPerIncident = 200;

    private const int NextIdLimit = 1_000_000;

    /// <summary>Timeline entries kept free on every incident for resolving and reopening (notes and updates stop before them).</summary>
    private const int ReservedUpdates = 4;

    /// <summary>Room for the prefix of status and severity entries on top of <see cref="MaxUpdateLength" />.</summary>
    private const int UpdatePrefixRoom = 64;

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<MonitoringEmail> emails = new();

    private readonly Dictionary<string, List<MonitoringSample>> history = new();

    private readonly List<MonitoringIncident> incidents = MonitoringSeedData.Incidents();

    private readonly List<MonitoringNotification> notifications = MonitoringSeedData.Notifications();

    private readonly List<MonitoringOverride> overrides = new();

    private readonly Dictionary<string, MonitoringNotificationPreferences> preferences = new();

    private readonly List<MonitoringLogEntry> seedLogs;

    private readonly List<MonitoringSubscription> subscriptions = new();

    /// <summary>The tick the shown window ends at; it moves to the clock's tick on <see cref="Tick" />.</summary>
    private int displayTick;

    private bool isDarkTheme;

    /// <summary>The log lines for the shown tick, and the incident version they were built with.</summary>
    private (int Tick, int Version, IReadOnlyList<MonitoringLogEntry> Lines)? logCache;

    private int nextEmailId = 1;

    private int nextIncidentId = FirstIncidentId;

    private int nextNotificationId;

    private int nextOverrideId = 1;

    private int nextSubscriptionId = 1;

    private int nextUpdateId;

    /// <summary>Changes whenever incidents change, so derived metrics and logs are rebuilt.</summary>
    private int version;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out, with a full chart window ending at tick 0.</summary>
    public MonitoringState()
    {
        seedLogs = MonitoringSeedData.Logs(Services);
        nextUpdateId = incidents.SelectMany(incident => incident.Updates).Max(update => update.Id) + 1;
        nextNotificationId = notifications.Max(notification => notification.Id) + 1;
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

        if (!TryAddUpdate(incident, account, "Acknowledged.", MonitoringUpdateKind.Status, null))
            return MonitoringResult.Invalid;

        incident.Status = MonitoringIncidentStatus.Acknowledged;
        incident.Assignee = account.Name;
        Notify(account.Id, MonitoringNotificationKind.Assigned, $"You acknowledged #{incident.Id}", $"{incident.Title} is assigned to you.", incident.Id);
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

        Commit();

        return MonitoringResult.Ok;
    }

    /// <summary>Returns the snapshot: incidents, subscriptions, the outbox, notifications, preferences, overrides, and the ID counters.</summary>
    public object CaptureData() => new MonitoringSnapshot
    {
        Emails = emails.ToList(),
        Incidents = incidents.ToList(),
        NextEmailId = nextEmailId,
        NextIncidentId = nextIncidentId,
        NextNotificationId = nextNotificationId,
        NextOverrideId = nextOverrideId,
        NextSubscriptionId = nextSubscriptionId,
        NextUpdateId = nextUpdateId,
        Notifications = notifications.ToList(),
        Overrides = overrides.ToList(),
        Preferences = preferences.ToDictionary(pair => pair.Key, pair => pair.Value),
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
        NotifyOnCall(created, $"{SeverityWord(severity)} incident declared: {title}", $"{account.Name} declared #{created.Id} on {FindService(serviceId)!.Name}.");
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
    public MonitoringIncident? FindPublicIncident(int incidentId) => incidents.FirstOrDefault(incident => incident.Id == incidentId && incident.IsPublished);

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
        }

        EmailSubscribers(incident, stage, text);
        Commit();

        return MonitoringResult.Ok;
    }

    /// <summary>Returns how a service shows on the status page: from the published, ongoing incidents that affect it.</summary>
    public MonitoringPublicStatus PublicStatus(string serviceId) =>
        PublicActiveIncidents.Where(incident => incident.ServiceId == serviceId).Select(incident => StatusOf(incident.Severity)).DefaultIfEmpty(MonitoringPublicStatus.Operational).Max();

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

        incident.Status = MonitoringIncidentStatus.Resolved;
        incident.ResolvedAt = Clock.Now;
        incident.Assignee ??= account.Name;

        if (incident.IsPublished)
            EmailSubscribers(incident, MonitoringPublicStage.Resolved, publicText);

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

        preferences[account.Id] = new MonitoringNotificationPreferences { Email = value.Email, Push = value.Push, SmsForCritical = value.SmsForCritical, IncidentUpdates = value.IncidentUpdates };
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

        if (!IsEmail(email) || services.Any(id => FindService(id) is null))
            return MonitoringResult.Invalid;

        if (subscriptions.Any(item => item.Email == email))
            return MonitoringResult.Duplicate;

        if (subscriptions.Count >= MaxSubscriptions || nextSubscriptionId >= NextIdLimit)
            return MonitoringResult.Invalid;

        subscription = new MonitoringSubscription { Id = nextSubscriptionId++, Email = email, ServiceIds = services, CreatedAt = Clock.Now };
        subscriptions.Add(subscription);
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
        Changed?.Invoke();

        return MonitoringResult.Ok;
    }

    /// <summary>Moves the shown window up to the clock's latest tick, so charts and logs show the newest samples.</summary>
    public void Tick()
    {
        var target = LiveTick;

        if (target == displayTick)
            return;

        displayTick = target;
        RebuildHistory();
        Ticked?.Invoke();
    }

    /// <summary>Validates a stored snapshot against its own data and applies it only when every part is valid.</summary>
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
                Emails: { Count: <= MaxEmails } storedEmails,
                Incidents: { Count: > 0 and <= MaxIncidents } storedIncidents,
                Notifications: { Count: <= MaxNotifications } storedNotifications,
                Overrides: { Count: <= MaxOverrides } storedOverrides,
                Preferences: { Count: <= 10 } storedPreferences,
                Subscriptions: { Count: <= MaxSubscriptions } storedSubscriptions,
            }
            || snapshot.NextIncidentId is < FirstIncidentId or > NextIdLimit
            || snapshot.NextUpdateId is < 1 or > NextIdLimit
            || snapshot.NextEmailId is < 1 or > NextIdLimit
            || snapshot.NextNotificationId is < 1 or > NextIdLimit
            || snapshot.NextOverrideId is < 1 or > NextIdLimit
            || snapshot.NextSubscriptionId is < 1 or > NextIdLimit)
            return false;

        if (storedIncidents.Any(incident => !ValidIncident(incident, snapshot)) || !Unique(storedIncidents.Select(incident => incident.Id))
            || !Unique(storedIncidents.SelectMany(incident => incident.Updates).Select(update => update.Id)))
            return false;

        var incidentIds = storedIncidents.Select(incident => incident.Id).ToHashSet();

        if (storedSubscriptions.Any(subscription => subscription is null || subscription.Id < 1 || subscription.Id >= snapshot.NextSubscriptionId || !IsEmail(subscription.Email)
                || subscription.ServiceIds is not { Count: <= 20 } || subscription.ServiceIds.Any(id => FindService(id) is null) || !Unique(subscription.ServiceIds)
                || !IsPlausible(subscription.CreatedAt))
            || !Unique(storedSubscriptions.Select(subscription => subscription.Id)) || !Unique(storedSubscriptions.Select(subscription => subscription.Email)))
            return false;

        var subscriptionIds = storedSubscriptions.Select(subscription => subscription.Id).ToHashSet();

        if (storedEmails.Any(email => email is null || email.Id < 1 || email.Id >= snapshot.NextEmailId || !subscriptionIds.Contains(email.SubscriptionId)
                || !incidentIds.Contains(email.IncidentId) || !IsEmail(email.To) || !IsText(email.Subject, 200) || !IsText(email.Body, MaxUpdateLength + 600) || !IsPlausible(email.SentAt))
            || !Unique(storedEmails.Select(email => email.Id)))
            return false;

        if (storedNotifications.Any(notification => notification is null || notification.Id < 1 || notification.Id >= snapshot.NextNotificationId
                || !IsDemoAccount(notification.AccountId) || !Enum.IsDefined(notification.Kind) || !IsText(notification.Title, 200) || !IsText(notification.Text, 500)
                || !IsPlausible(notification.Time) || (notification.IncidentId is { } id && !incidentIds.Contains(id)))
            || !Unique(storedNotifications.Select(notification => notification.Id)))
            return false;

        if (storedPreferences.Any(pair => !IsDemoAccount(pair.Key) || pair.Value is null))
            return false;

        if (storedOverrides.Any(item => item is null || item.Id < 1 || item.Id >= snapshot.NextOverrideId || !IsDemoAccount(item.AccountId)
                || FindEngineer(item.EngineerId) is null || FindEngineer(item.OriginalId) is null || item.EngineerId == item.OriginalId || item.EngineerId != item.AccountId
                || !Enum.IsDefined(item.Role)
                || !IsWeekStart(item.ShiftStart) || item.Reason is null || item.Reason.Length > MaxReasonLength || !IsPlausible(item.CreatedAt))
            || !Unique(storedOverrides.Select(item => item.Id)) || !Unique(storedOverrides.Select(item => (item.ShiftStart, item.Role))))
            return false;

        incidents.Clear();
        incidents.AddRange(storedIncidents);
        subscriptions.Clear();
        subscriptions.AddRange(storedSubscriptions);
        emails.Clear();
        emails.AddRange(storedEmails);
        notifications.Clear();
        notifications.AddRange(storedNotifications);
        overrides.Clear();
        overrides.AddRange(storedOverrides);
        preferences.Clear();

        foreach (var pair in storedPreferences)
            preferences[pair.Key] = pair.Value;

        nextIncidentId = snapshot.NextIncidentId;
        nextUpdateId = snapshot.NextUpdateId;
        nextEmailId = snapshot.NextEmailId;
        nextNotificationId = snapshot.NextNotificationId;
        nextOverrideId = snapshot.NextOverrideId;
        nextSubscriptionId = snapshot.NextSubscriptionId;

        return true;
    }

    /// <summary>Removes a subscription and its emails.</summary>
    public MonitoringResult Unsubscribe(int subscriptionId)
    {
        if (subscriptions.RemoveAll(subscription => subscription.Id == subscriptionId) == 0)
            return MonitoringResult.NotFound;

        emails.RemoveAll(email => email.SubscriptionId == subscriptionId);
        Changed?.Invoke();

        return MonitoringResult.Ok;
    }

    /// <summary>Changes which services a subscription follows (empty for every service).</summary>
    public MonitoringResult UpdateSubscription(int subscriptionId, IReadOnlyList<string> serviceIds)
    {
        if (subscriptions.FirstOrDefault(subscription => subscription.Id == subscriptionId) is not { } subscription)
            return MonitoringResult.NotFound;

        var services = (serviceIds ?? Array.Empty<string>()).Distinct().ToList();

        if (services.Any(id => FindService(id) is null))
            return MonitoringResult.Invalid;

        if (subscription.ServiceIds.Order().SequenceEqual(services.Order()))
            return MonitoringResult.NoChange;

        subscription.ServiceIds = services;
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

    /// <summary>The incident active on a service at a time (the worst one), from its own start and resolution times.</summary>
    private MonitoringIncident? ActiveAt(string serviceId, DateTime time) =>
        incidents.Where(incident => incident.ServiceId == serviceId && incident.StartedAt <= time && (incident.ResolvedAt is null || time < incident.ResolvedAt))
            .OrderByDescending(incident => incident.Impact == MonitoringImpact.Outage).ThenBy(incident => incident.Severity).FirstOrDefault();

    private void Commit()
    {
        version++;
        RebuildHistory();
        Changed?.Invoke();
    }

    /// <summary>Puts one email per matching subscription in the outbox; the oldest go when it is full.</summary>
    private void EmailSubscribers(MonitoringIncident incident, MonitoringPublicStage stage, string text)
    {
        var service = FindService(incident.ServiceId)!;

        foreach (var subscription in subscriptions.Where(item => item.ServiceIds.Count == 0 || item.ServiceIds.Contains(incident.ServiceId)))
        {
            if (nextEmailId >= NextIdLimit)
                return;

            emails.Add(new MonitoringEmail(nextEmailId++, subscription.Id, subscription.Email, Clip($"[{StageWord(stage)}] {incident.Title}", 200),
                $"{service.Name}: {StageWord(stage)}\n\n{text}\n\nView the incident on {MonitoringSeedData.StatusPageName}. You get this email because you subscribed to updates in this demo tab.",
                Clock.Now, incident.Id));

            while (emails.Count > MaxEmails)
                emails.RemoveAt(0);
        }

        // The outbox gives way first: the oldest emails go until the snapshot fits, so saving never stops.
        while (emails.Count > 0 && !FitsSnapshot())
            emails.RemoveAt(0);
    }

    /// <summary>The signed-in engineer, or <see langword="null" />.</summary>
    private ShowcaseAccount? Engineer() => Identity.Current is { RoleId: MonitoringSeedData.EngineerRole } account ? account : null;

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
        var lines = new List<MonitoringLogEntry>(seedLogs);
        var now = CurrentTime;

        for (var t = Math.Max(1, displayTick - LiveLogTicks + 1); t <= displayTick; t++)
        {
            for (var i = 0; i < 3; i++)
            {
                var id = 10_001 + ((t - 1) * 3) + i;
                var service = Services[(int)(MonitoringSeedData.Hash(id, 17, i) * 1000) % Services.Count];
                var time = SampleTime(t).AddMilliseconds(-i * 350);
                var level = ActiveAt(service.Id, time)?.Impact switch
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

    private void Notify(string accountId, MonitoringNotificationKind kind, string title, string text, int? incidentId)
    {
        if (!IsDemoAccount(accountId) || nextNotificationId >= NextIdLimit)
            return;

        notifications.Add(new MonitoringNotification { Id = nextNotificationId++, AccountId = accountId, Kind = kind, Time = Clock.Now, Title = Clip(title, 200), Text = Clip(text, 500), IncidentId = incidentId });

        while (notifications.Count > MaxNotifications)
            notifications.RemoveAt(0);
    }

    /// <summary>Notifies this week's on-call engineers who have a demo account.</summary>
    private void NotifyOnCall(MonitoringIncident incident, string title, string text)
    {
        var shift = ShiftForWeek(WeekIndex(Clock.Now));

        foreach (var engineerId in new[] { shift.PrimaryId, shift.SecondaryId }.Distinct())
            Notify(engineerId, MonitoringNotificationKind.Alert, title, text, incident.Id);
    }

    private void RebuildHistory()
    {
        for (var index = 0; index < Services.Count; index++)
        {
            var service = Services[index];
            history[service.Id] = Enumerable.Range(displayTick - MonitoringSeedData.HistoryLength + 1, MonitoringSeedData.HistoryLength).Select(t => Sample(service, index, t)).ToList();
        }

        logCache = null;
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

        switch (ActiveAt(service.Id, time)?.Impact)
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

    /// <summary>Checks one stored incident: references, enums, text limits, times, the timeline, the postmortem, and that a published incident's public stage matches its status.</summary>
    private bool ValidIncident(MonitoringIncident? incident, MonitoringSnapshot snapshot)
    {
        if (incident is null || incident.Id < 1 || incident.Id >= snapshot.NextIncidentId || FindService(incident.ServiceId) is null || !Enum.IsDefined(incident.Severity)
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

    /// <summary>Gets whether the signed-in account is an engineer.</summary>
    public bool IsEngineer => Identity.Current?.RoleId == MonitoringSeedData.EngineerRole;

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

    /// <summary>Gets the status page's overall status: the worst of its components.</summary>
    public MonitoringPublicStatus OverallPublicStatus => Services.Select(service => PublicStatus(service.Id)).DefaultIfEmpty(MonitoringPublicStatus.Operational).Max();

    /// <summary>Gets the on-call overrides, newest first.</summary>
    public IReadOnlyList<MonitoringOverride> Overrides => overrides.OrderByDescending(item => item.CreatedAt).ToList();

    /// <summary>Gets how the signed-in engineer wants to be reached (the defaults until saved).</summary>
    public MonitoringNotificationPreferences Preferences =>
        Engineer() is { } account && preferences.TryGetValue(account.Id, out var saved) ? saved : new MonitoringNotificationPreferences();

    /// <summary>Gets the published incidents that are still ongoing on the status page, newest first.</summary>
    public IReadOnlyList<MonitoringIncident> PublicActiveIncidents =>
        incidents.Where(incident => incident.IsPublished && incident.PublicStage != MonitoringPublicStage.Resolved).OrderByDescending(incident => incident.StartedAt).ToList();

    /// <summary>Gets every published incident (the status page's history), newest first.</summary>
    public IReadOnlyList<MonitoringIncident> PublicIncidents => incidents.Where(incident => incident.IsPublished).OrderByDescending(incident => incident.StartedAt).ToList();

    /// <summary>Gets the monitored services.</summary>
    public IReadOnlyList<MonitoringService> Services { get; } = MonitoringSeedData.Services();

    /// <summary>Gets the site's slug, used in the storage key.</summary>
    public string SiteSlug => "monitoring";

    /// <summary>Gets the snapshot format version.</summary>
    public int SnapshotVersion => 1;

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

        public List<MonitoringEmail>? Emails { get; set; }

        public List<MonitoringIncident>? Incidents { get; set; }

        public int NextEmailId { get; set; }

        public int NextIncidentId { get; set; }

        public int NextNotificationId { get; set; }

        public int NextOverrideId { get; set; }

        public int NextSubscriptionId { get; set; }

        public int NextUpdateId { get; set; }

        public List<MonitoringNotification>? Notifications { get; set; }

        public List<MonitoringOverride>? Overrides { get; set; }

        public Dictionary<string, MonitoringNotificationPreferences>? Preferences { get; set; }

        public List<MonitoringSubscription>? Subscriptions { get; set; }

        #endregion
    }

    #endregion
}
