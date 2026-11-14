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

/// <summary>Where an incident is in its life.</summary>
public enum MonitoringIncidentStatus
{
    /// <summary>Not yet picked up.</summary>
    Open,

    /// <summary>Someone is working on it.</summary>
    Acknowledged,

    /// <summary>Fixed.</summary>
    Resolved,
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

/// <summary>A dated note on an incident's timeline.</summary>
public sealed record MonitoringIncidentUpdate(DateTime Time, string Author, string Text);

/// <summary>An incident.</summary>
public sealed class MonitoringIncident
{
    #region Properties, Indexers

    /// <summary>Gets the incident number.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets when the incident was resolved.</summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>Gets the affected service ID.</summary>
    public string ServiceId { get; init; } = string.Empty;

    /// <summary>Gets the severity.</summary>
    public MonitoringSeverity Severity { get; init; }

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

/// <summary>A log line.</summary>
public sealed record MonitoringLogEntry(int Id, DateTime Time, string ServiceId, MonitoringLogLevel Level, string Message, string TraceId);
