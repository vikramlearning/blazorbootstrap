using System.Text.Json.Serialization;

namespace BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;

/// <summary>What a monitored service is.</summary>
public enum MonitoringServiceKind
{
    /// <summary>An HTTP API.</summary>
    Api,

    /// <summary>A background worker.</summary>
    Worker,

    /// <summary>A database.</summary>
    Database,

    /// <summary>A cache.</summary>
    Cache,

    /// <summary>A fleet of IoT devices reporting through a gateway.</summary>
    Devices,
}

/// <summary>Health of a service, from its latest sample.</summary>
public enum MonitoringStatus
{
    /// <summary>Within its normal ranges.</summary>
    Operational,

    /// <summary>Slow or with elevated errors.</summary>
    Degraded,

    /// <summary>Failing most requests.</summary>
    Down,
}

/// <summary>A component's status as the public status page shows it, set by the published incidents that affect it.</summary>
public enum MonitoringPublicStatus
{
    /// <summary>Working normally.</summary>
    Operational,

    /// <summary>Slower than usual (a published minor incident).</summary>
    Degraded,

    /// <summary>Partly failing (a published major incident).</summary>
    PartialOutage,

    /// <summary>Unavailable (a published critical incident).</summary>
    MajorOutage,
}

/// <summary>How bad an incident is.</summary>
public enum MonitoringSeverity
{
    /// <summary>Customers can't use a core feature.</summary>
    Critical,

    /// <summary>A feature is slow or partly failing.</summary>
    Major,

    /// <summary>Little or no customer impact.</summary>
    Minor,
}

/// <summary>What an incident does to its service's metrics while it is active.</summary>
public enum MonitoringImpact
{
    /// <summary>Requests fail at an elevated rate.</summary>
    Errors,

    /// <summary>Requests are slow and CPU runs hot.</summary>
    Latency,

    /// <summary>Most requests fail.</summary>
    Outage,
}

/// <summary>Where an incident is in its life, for the engineers.</summary>
public enum MonitoringIncidentStatus
{
    /// <summary>Not yet picked up.</summary>
    Open,

    /// <summary>Someone is working on it.</summary>
    Acknowledged,

    /// <summary>Fixed.</summary>
    Resolved,
}

/// <summary>The stage a public status page update announces (the usual status page wording).</summary>
public enum MonitoringPublicStage
{
    /// <summary>We are looking into it.</summary>
    Investigating,

    /// <summary>The cause is known and a fix is underway.</summary>
    Identified,

    /// <summary>A fix is in place and we are watching the results.</summary>
    Monitoring,

    /// <summary>The incident is over.</summary>
    Resolved,
}

/// <summary>What an entry on an incident's timeline is.</summary>
public enum MonitoringUpdateKind
{
    /// <summary>The monitoring system (an alert fired).</summary>
    Alert,

    /// <summary>An engineers-only note.</summary>
    Note,

    /// <summary>An update published on the status page (and emailed to subscribers).</summary>
    Public,

    /// <summary>A status change: acknowledged, resolved, or reopened.</summary>
    Status,

    /// <summary>A severity change.</summary>
    Severity,
}

/// <summary>Severity of a log line.</summary>
public enum MonitoringLogLevel
{
    /// <summary>Normal operation.</summary>
    Info,

    /// <summary>Something unusual that did not fail.</summary>
    Warning,

    /// <summary>A failure.</summary>
    Error,
}

/// <summary>What an engineer's notification is about.</summary>
public enum MonitoringNotificationKind
{
    /// <summary>An alert fired or an incident was declared.</summary>
    Alert,

    /// <summary>An incident was assigned to the engineer.</summary>
    Assigned,

    /// <summary>Someone updated an incident the engineer follows.</summary>
    Update,

    /// <summary>An on-call shift starts, or someone took one over.</summary>
    OnCall,
}

/// <summary>Which on-call position a shift fills.</summary>
public enum MonitoringOnCallRole
{
    /// <summary>Paged first.</summary>
    Primary,

    /// <summary>Paged when the primary doesn't acknowledge within 5 minutes.</summary>
    Secondary,
}

/// <summary>Outcome of an action on the monitoring site.</summary>
public enum MonitoringResult
{
    /// <summary>Done.</summary>
    Ok,

    /// <summary>Sign in with an engineer account first, or it isn't yours.</summary>
    NotAllowed,

    /// <summary>The incident, service, subscription, shift, or notification doesn't exist.</summary>
    NotFound,

    /// <summary>A value is missing, too long, or out of range (or the tab is full).</summary>
    Invalid,

    /// <summary>The item's state doesn't allow it (for example, a postmortem before the incident is resolved).</summary>
    WrongState,

    /// <summary>Nothing changed.</summary>
    NoChange,

    /// <summary>It already exists (for example, a subscription for the same email).</summary>
    Duplicate,
}

/// <summary>A monitored service with its normal ranges. Names and numbers are fictitious.</summary>
public sealed record MonitoringService(
    string Id,
    string Name,
    MonitoringServiceKind Kind,
    string Region,
    string Team,
    string Description,
    double BaseRequests,
    double BaseLatencyMs,
    double BaseErrorRate,
    double BaseCpu,
    double BaseMemory,
    double DiskUsed,
    int Instances,
    double Uptime30Days);

/// <summary>One reading of a service's metrics.</summary>
public sealed record MonitoringSample(int Tick, DateTime Time, double Requests, double LatencyMs, double ErrorRate, double Cpu, double Memory);

/// <summary>A dated entry on an incident's timeline. Text is plain. Only <see cref="MonitoringUpdateKind.Public" /> entries reach the status page.</summary>
public sealed record MonitoringIncidentUpdate(DateTime Time, string Author, string Text)
{
    #region Properties, Indexers

    /// <summary>Gets the author's demo account, or <see langword="null" /> for a seeded entry or the monitoring system.</summary>
    public string? AccountId { get; init; }

    /// <summary>Gets the entry number.</summary>
    public int Id { get; init; }

    /// <summary>Gets whether the entry is published on the status page.</summary>
    [JsonIgnore]
    public bool IsPublic => Kind == MonitoringUpdateKind.Public;

    /// <summary>Gets what the entry is.</summary>
    public MonitoringUpdateKind Kind { get; init; } = MonitoringUpdateKind.Note;

    /// <summary>Gets the stage a public update announces; <see langword="null" /> for other entries.</summary>
    public MonitoringPublicStage? Stage { get; init; }

    #endregion
}

/// <summary>The review written after an incident: what happened, why, and what changes. Text is plain.</summary>
public sealed class MonitoringPostmortem
{
    #region Properties, Indexers

    /// <summary>Gets or sets the follow-up actions, in order.</summary>
    public List<string> ActionItems { get; set; } = new();

    /// <summary>Gets or sets the author's name.</summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>Gets or sets when it was published on the status page, or <see langword="null" /> while engineers-only.</summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>Gets or sets the root cause.</summary>
    public string RootCause { get; set; } = string.Empty;

    /// <summary>Gets or sets what happened and its impact.</summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>Gets or sets when it was last saved.</summary>
    public DateTime WrittenAt { get; set; }

    #endregion
}

/// <summary>An incident.</summary>
public sealed class MonitoringIncident
{
    #region Properties, Indexers

    /// <summary>Gets or sets the engineer working on it (their name), if someone acknowledged it.</summary>
    public string? Assignee { get; set; }

    /// <summary>Gets the incident number.</summary>
    public int Id { get; init; }

    /// <summary>Gets what the incident does to its service's metrics while it is active.</summary>
    public MonitoringImpact Impact { get; init; }

    /// <summary>Gets whether any update was published on the status page.</summary>
    [JsonIgnore]
    public bool IsPublished => Updates.Any(update => update.IsPublic);

    /// <summary>Gets or sets the postmortem, once one is written.</summary>
    public MonitoringPostmortem? Postmortem { get; set; }

    /// <summary>Gets the stage of the latest public update, or <see langword="null" /> when nothing is published.</summary>
    [JsonIgnore]
    public MonitoringPublicStage? PublicStage => Updates.LastOrDefault(update => update.IsPublic)?.Stage;

    /// <summary>Gets or sets when the incident was resolved; <see langword="null" /> while it is active.</summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>Gets the affected service ID.</summary>
    public string ServiceId { get; init; } = string.Empty;

    /// <summary>Gets or sets the severity.</summary>
    public MonitoringSeverity Severity { get; set; }

    /// <summary>Gets when the incident started.</summary>
    public DateTime StartedAt { get; init; }

    /// <summary>Gets or sets the status.</summary>
    public MonitoringIncidentStatus Status { get; set; }

    /// <summary>Gets the one-line summary.</summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>Gets the title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the timeline, oldest first.</summary>
    public List<MonitoringIncidentUpdate> Updates { get; init; } = new();

    #endregion
}

/// <summary>A log line. <see cref="TraceId" /> links to its trace.</summary>
public sealed record MonitoringLogEntry(int Id, DateTime Time, string ServiceId, MonitoringLogLevel Level, string Message, string TraceId);

/// <summary>One operation inside a trace, timed from the start of the trace.</summary>
public sealed record MonitoringSpan(string Id, string? ParentId, string ServiceId, string Operation, double StartMs, double DurationMs, bool IsError);

/// <summary>A request's path through the services: its spans in start order, the root first.</summary>
public sealed record MonitoringTrace(string TraceId, DateTime Time, string RootServiceId, IReadOnlyList<MonitoringSpan> Spans)
{
    #region Properties, Indexers

    /// <summary>Gets the whole trace's length in milliseconds.</summary>
    public double DurationMs => Spans.Count == 0 ? 0 : Spans.Max(span => span.StartMs + span.DurationMs);

    /// <summary>Gets whether any span failed.</summary>
    public bool HasError => Spans.Any(span => span.IsError);

    #endregion
}

/// <summary>A public subscription to status page updates, kept in this tab. The email is only used for the demo outbox.</summary>
public sealed class MonitoringSubscription
{
    #region Properties, Indexers

    /// <summary>Gets when it was created.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets the subscriber's email address.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Gets the subscription number.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the services the subscriber follows; empty means every service.</summary>
    public List<string> ServiceIds { get; set; } = new();

    #endregion
}

/// <summary>An email "sent" to a subscriber when a status page update was published (nothing is sent; this is the demo outbox).</summary>
public sealed record MonitoringEmail(int Id, int SubscriptionId, string To, string Subject, string Body, DateTime SentAt, int IncidentId);

/// <summary>An in-app notification for an engineer.</summary>
public sealed class MonitoringNotification
{
    #region Properties, Indexers

    /// <summary>Gets the engineer's demo account.</summary>
    public string AccountId { get; init; } = string.Empty;

    /// <summary>Gets the notification number.</summary>
    public int Id { get; init; }

    /// <summary>Gets the incident it is about, if any.</summary>
    public int? IncidentId { get; init; }

    /// <summary>Gets or sets whether the engineer read it.</summary>
    public bool IsRead { get; set; }

    /// <summary>Gets what it is about.</summary>
    public MonitoringNotificationKind Kind { get; init; }

    /// <summary>Gets the details.</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>Gets when it arrived.</summary>
    public DateTime Time { get; init; }

    /// <summary>Gets the headline.</summary>
    public string Title { get; init; } = string.Empty;

    #endregion
}

/// <summary>How an engineer wants to be reached.</summary>
public sealed class MonitoringNotificationPreferences
{
    #region Properties, Indexers

    /// <summary>Gets or sets whether alerts also arrive by email.</summary>
    public bool Email { get; set; } = true;

    /// <summary>Gets or sets whether alerts also arrive as push notifications on the phone app.</summary>
    public bool Push { get; set; } = true;

    /// <summary>Gets or sets whether critical alerts also arrive by text message.</summary>
    public bool SmsForCritical { get; set; } = true;

    /// <summary>Gets or sets whether updates to incidents the engineer is working on arrive too (not only alerts).</summary>
    public bool IncidentUpdates { get; set; } = true;

    #endregion
}

/// <summary>An engineer on the on-call rotation (most have no sign-in in the demo).</summary>
public sealed record MonitoringEngineer(string Id, string Name, string Initials, string Team);

/// <summary>A week of on-call: who is primary and secondary, after any overrides.</summary>
public sealed record MonitoringShift(DateTime Start, DateTime End, string PrimaryId, string SecondaryId)
{
    #region Properties, Indexers

    /// <summary>Gets the override that changed the primary, if any.</summary>
    public MonitoringOverride? PrimaryOverride { get; init; }

    /// <summary>Gets the override that changed the secondary, if any.</summary>
    public MonitoringOverride? SecondaryOverride { get; init; }

    #endregion
}

/// <summary>An engineer taking over someone's on-call week (primary or secondary).</summary>
public sealed class MonitoringOverride
{
    #region Properties, Indexers

    /// <summary>Gets the demo account that made it.</summary>
    public string AccountId { get; init; } = string.Empty;

    /// <summary>Gets when it was made.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets the engineer who takes the shift.</summary>
    public string EngineerId { get; init; } = string.Empty;

    /// <summary>Gets the override number.</summary>
    public int Id { get; init; }

    /// <summary>Gets the engineer the rotation had scheduled.</summary>
    public string OriginalId { get; init; } = string.Empty;

    /// <summary>Gets why (plain text, optional).</summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>Gets the position taken.</summary>
    public MonitoringOnCallRole Role { get; init; }

    /// <summary>Gets the start of the week taken.</summary>
    public DateTime ShiftStart { get; init; }

    #endregion
}

/// <summary>One day of a component's 90-day uptime bar: its worst published status that day.</summary>
public sealed record MonitoringUptimeDay(DateTime Date, MonitoringPublicStatus Status, IReadOnlyList<int> IncidentIds);
