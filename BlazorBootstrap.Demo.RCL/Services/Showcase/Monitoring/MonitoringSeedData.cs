using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

/// <summary>
/// Fixed, fictitious content for the "Stackwatch" monitoring site: services, incidents, and log lines. Metric values
/// come from deterministic wave-and-hash generators (no random numbers), so every load starts the same way.
/// </summary>
public static class MonitoringSeedData
{
    #region Fields and Constants

    /// <summary>How many samples the charts keep.</summary>
    public const int HistoryLength = 30;

    /// <summary>The fictitious product name.</summary>
    public const string ProductName = "Stackwatch";

    /// <summary>Seconds between live samples (finalize OI-09).</summary>
    public const int TickSeconds = 2;

    /// <summary>The fictitious engineer shown as signed in.</summary>
    public const string UserName = "Jordan Ashdown";

    /// <summary>The fixed "now" of tick 0; later ticks add <see cref="TickSeconds" /> each.</summary>
    public static readonly DateTime Now = new(2026, 11, 24, 10, 30, 0);

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>The incidents: one open, one acknowledged, three resolved.</summary>
    public static List<MonitoringIncident> Incidents() => new()
    {
        new MonitoringIncident
        {
            Id = 301, ServiceId = "payments", Severity = MonitoringSeverity.Critical, Status = MonitoringIncidentStatus.Open, StartedAt = Now.AddMinutes(-18),
            Title = "Payments API returning 5xx errors",
            Summary = "About 6% of payment requests fail with 502 responses from the card processor.",
            Updates = { new(Now.AddMinutes(-18), ProductName, "Alert fired: error rate above 2% for 5 minutes.") },
        },
        new MonitoringIncident
        {
            Id = 300, ServiceId = "search", Severity = MonitoringSeverity.Major, Status = MonitoringIncidentStatus.Acknowledged, StartedAt = Now.AddMinutes(-52),
            Title = "Search latency above 300 ms",
            Summary = "Search results take two to three times longer than usual.",
            Updates =
            {
                new(Now.AddMinutes(-52), ProductName, "Alert fired: p95 latency above twice the normal range for 10 minutes."),
                new(Now.AddMinutes(-41), "Priya Nair", "Acknowledged. The nightly reindex job is competing for CPU; throttling it now."),
            },
        },
        new MonitoringIncident
        {
            Id = 299, ServiceId = "redis", Severity = MonitoringSeverity.Minor, Status = MonitoringIncidentStatus.Resolved, StartedAt = Now.AddHours(-20), ResolvedAt = Now.AddHours(-19.4),
            Title = "Session cache evictions",
            Summary = "Memory pressure evicted sessions early; some users had to sign in again.",
            Updates =
            {
                new(Now.AddHours(-20), ProductName, "Alert fired: eviction rate above 500 per minute."),
                new(Now.AddHours(-19.4), "Marcus Osei", "Raised the memory limit and shortened the TTL of guest sessions. Resolved."),
            },
        },
        new MonitoringIncident
        {
            Id = 298, ServiceId = "sensors", Severity = MonitoringSeverity.Major, Status = MonitoringIncidentStatus.Resolved, StartedAt = Now.AddDays(-2).AddHours(-3), ResolvedAt = Now.AddDays(-2).AddHours(-1),
            Title = "Warehouse sensors offline in bay 4",
            Summary = "38 temperature sensors stopped reporting after a gateway firmware update.",
            Updates =
            {
                new(Now.AddDays(-2).AddHours(-3), ProductName, "Alert fired: 38 devices missed 3 heartbeats."),
                new(Now.AddDays(-2).AddHours(-1), UserName, "Rolled the bay 4 gateway back to the previous firmware. All devices reporting. Resolved."),
            },
        },
        new MonitoringIncident
        {
            Id = 297, ServiceId = "postgres", Severity = MonitoringSeverity.Critical, Status = MonitoringIncidentStatus.Resolved, StartedAt = Now.AddDays(-6).AddHours(-2), ResolvedAt = Now.AddDays(-6).AddHours(-1.75),
            Title = "Orders database failover",
            Summary = "The primary database restarted; writes failed for about 15 minutes until the replica took over.",
            Updates =
            {
                new(Now.AddDays(-6).AddHours(-2), ProductName, "Alert fired: primary unreachable."),
                new(Now.AddDays(-6).AddHours(-1.75), "Priya Nair", "Failover to the replica finished; writes recovered. Resolved; follow-up review scheduled."),
            },
        },
    };

    /// <summary>About two hours of log lines, newest first. Payments errors and search warnings match the open incidents.</summary>
    public static List<MonitoringLogEntry> Logs(IReadOnlyList<MonitoringService> services)
    {
        var entries = new List<MonitoringLogEntry>(120);

        for (var k = 0; k < 120; k++)
        {
            var time = Now.AddSeconds(-(k * 61) - (k * 7 % 23));
            var service = services[(int)(Hash(k, 11, 3) * 1000) % services.Count];
            var minutesAgo = (Now - time).TotalMinutes;

            var level = service.Id switch
            {
                "payments" when minutesAgo <= 18 => k % 2 == 0 ? MonitoringLogLevel.Error : MonitoringLogLevel.Warning,
                "search" when minutesAgo <= 52 => k % 2 == 0 ? MonitoringLogLevel.Warning : MonitoringLogLevel.Info,
                _ => k % 13 == 0 ? MonitoringLogLevel.Warning : MonitoringLogLevel.Info,
            };

            entries.Add(new MonitoringLogEntry(10_000 - k, time, service.Id, level, Message(service, level, k), TraceId(k)));
        }

        return entries;
    }

    /// <summary>Returns a log message for a service and level from fixed templates.</summary>
    public static string Message(MonitoringService service, MonitoringLogLevel level, int seed) => (service.Id, level) switch
    {
        ("payments", MonitoringLogLevel.Error) => $"POST /v1/charges failed: upstream card processor returned 502 after {420 + (seed * 37 % 300)} ms",
        ("payments", MonitoringLogLevel.Warning) => "Retrying charge on the secondary processor (attempt 2 of 3)",
        ("search", MonitoringLogLevel.Warning) => $"Slow query: q=\"winter boots\" took {310 + (seed * 53 % 240)} ms (threshold 300 ms)",
        ("sensors", MonitoringLogLevel.Warning) => $"Device bay2-temp-{(seed * 7 % 60) + 1:00} missed one heartbeat",
        ("redis", MonitoringLogLevel.Warning) => "Memory use above 80%; evicting least recently used keys",
        ("postgres", MonitoringLogLevel.Warning) => $"Autovacuum on orders took {12 + (seed % 9)} s",
        (_, MonitoringLogLevel.Warning) => $"Response time above target on {Endpoint(service, seed)}",
        (_, MonitoringLogLevel.Error) => $"Unhandled error on {Endpoint(service, seed)}",
        ("sensors", _) => $"Batch of {180 + (seed * 13 % 90)} readings stored from {(seed % 4) + 1} gateways",
        ("postgres", _) => $"Checkpoint complete: wrote {200 + (seed * 17 % 600)} buffers",
        ("redis", _) => $"Snapshot saved in {40 + (seed % 30)} ms",
        ("notifications", _) => $"Sent {20 + (seed % 40)} emails and {5 + (seed % 12)} push notifications",
        _ => $"{Endpoint(service, seed)} 200 in {(int)(service.BaseLatencyMs * (0.7 + Hash(seed, 3, 9) * 0.6))} ms",
    };

    /// <summary>The monitored services.</summary>
    public static IReadOnlyList<MonitoringService> Services() => new List<MonitoringService>
    {
        new("api-gateway", "API gateway", MonitoringServiceKind.Api, "us-east-1", "Platform", "Routes every public request to the services behind it.", 1240, 85, 0.3, 46, 58, 41, 6, 99.98),
        new("auth", "Auth service", MonitoringServiceKind.Api, "us-east-1", "Identity", "Signs users in and issues access tokens.", 310, 62, 0.2, 31, 49, 37, 3, 99.99),
        new("payments", "Payments API", MonitoringServiceKind.Api, "us-east-1", "Payments", "Charges cards and issues refunds through the card processor.", 140, 180, 0.4, 38, 61, 44, 4, 99.95),
        new("search", "Search", MonitoringServiceKind.Api, "eu-west-1", "Discovery", "Product search and suggestions.", 420, 140, 0.2, 52, 64, 58, 4, 99.97),
        new("notifications", "Notification worker", MonitoringServiceKind.Worker, "us-east-1", "Platform", "Sends emails and push notifications from a queue.", 75, 410, 0.6, 54, 66, 33, 2, 99.9),
        new("postgres", "Orders database", MonitoringServiceKind.Database, "us-east-1", "Data", "PostgreSQL primary for orders and customers.", 2100, 4, 0.0, 63, 78, 72, 2, 99.99),
        new("redis", "Session cache", MonitoringServiceKind.Cache, "us-east-1", "Identity", "Redis cache for sessions and rate limits.", 5400, 0.8, 0.0, 22, 81, 12, 3, 100),
        new("sensors", "Warehouse sensors", MonitoringServiceKind.Devices, "eu-west-1", "Logistics", "Temperature and door sensors in four warehouse bays, reporting through gateways.", 260, 120, 0.9, 18, 34, 29, 412, 99.8),
    };

    /// <summary>Returns a stable pseudo-random number from 0 to 1 for the given inputs (no shared random state).</summary>
    public static double Hash(int a, int b, int c)
    {
        unchecked
        {
            var h = (uint)((a * 73_856_093) ^ (b * 19_349_663) ^ (c * 83_492_791));
            h ^= h >> 13;
            h *= 0x5bd1e995;
            h ^= h >> 15;

            return (h % 10_000) / 10_000.0;
        }
    }

    #endregion

    #region Private Methods

    private static string Endpoint(MonitoringService service, int seed) => service.Kind switch
    {
        MonitoringServiceKind.Api => new[] { "GET /v1/products", "GET /v1/orders", "POST /v1/sessions", "GET /v1/users/me" }[seed % 4],
        MonitoringServiceKind.Worker => "job send-email",
        _ => service.Name,
    };

    private static string TraceId(int seed) => ((uint)(Hash(seed, 5, 7) * uint.MaxValue)).ToString("x8", CultureInfo.InvariantCulture) + ((uint)(Hash(seed, 9, 2) * uint.MaxValue)).ToString("x8", CultureInfo.InvariantCulture);

    #endregion

    #endregion
}
