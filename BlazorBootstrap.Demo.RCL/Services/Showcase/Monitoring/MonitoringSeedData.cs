using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

/// <summary>
/// Fixed, fictitious content for the "Stackwatch" monitoring site: services, incidents (with their internal and public
/// timelines), engineers and the on-call rotation, notifications, log lines, and traces. Metric values and log lines come
/// from deterministic wave-and-hash generators (no random numbers), so every fresh tab starts the same way.
/// </summary>
public static class MonitoringSeedData
{
    #region Fields and Constants

    /// <summary>The demo engineer's name.</summary>
    public const string EngineerName = "Priya Nair";

    /// <summary>The role of an engineer (works incidents, publishes status page updates, reads logs and traces, goes on call).</summary>
    public const string EngineerRole = "engineer";

    /// <summary>How many samples the charts keep.</summary>
    public const int HistoryLength = 30;

    /// <summary>The fictitious product name.</summary>
    public const string ProductName = "Stackwatch";

    /// <summary>The ID of the demo engineer account (also her ID on the rotation).</summary>
    public const string SeededEngineerId = "priya";

    /// <summary>The name of the fictitious company whose status page this is.</summary>
    public const string StatusPageName = "Stackwatch Status";

    /// <summary>The author of entries the monitoring system writes.</summary>
    public const string SystemAuthor = "Stackwatch alerts";

    /// <summary>Seconds between live samples (finalize OI-09).</summary>
    public const int TickSeconds = 2;

    /// <summary>When a fresh tab's clock starts (tick 0); the clock moves on from there (D-029).</summary>
    public static readonly DateTime Now = new(2026, 11, 24, 10, 30, 0);

    /// <summary>The start of the on-call week that contains <see cref="Now" /> (Mondays at 09:00).</summary>
    public static readonly DateTime RotationStart = new(2026, 11, 23, 9, 0, 0);

    /// <summary>The order of the weekly rotation: week k's primary is entry k, its secondary the next one.</summary>
    public static readonly IReadOnlyList<string> RotationOrder = new[] { "priya", "marcus", "aiko", "daniel" };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Builds a trace for a log line, deterministically from its trace ID: the gateway (for APIs), auth, the service, and its data stores.</summary>
    public static MonitoringTrace BuildTrace(MonitoringLogEntry entry, IReadOnlyList<MonitoringService> services)
    {
        var seed = int.Parse(entry.TraceId[..7], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var service = services.First(item => item.Id == entry.ServiceId);
        var spans = new List<MonitoringSpan>();
        var number = 0;
        double Jitter(int salt, double low, double high) => low + (Hash(seed, salt, 7) * (high - low));
        string NextId() => FormattableString.Invariant($"{entry.TraceId[..6]}{++number:x2}");

        var isError = entry.Level == MonitoringLogLevel.Error;
        var isApi = service.Kind == MonitoringServiceKind.Api && service.Id != "api-gateway";

        if (isApi)
        {
            var rootId = NextId();
            var authId = NextId();
            var serviceId = NextId();
            var authMs = Jitter(1, 3, 9);
            var work = new List<MonitoringSpan>();
            var at = 1 + authMs + 1;

            spans.Add(new MonitoringSpan(authId, rootId, "auth", "POST /internal/tokens/verify", 1, authMs, false));

            // The service's own calls: the session cache, then the database (and the card processor for payments).
            var cacheMs = Jitter(2, 0.4, 1.6);
            work.Add(new MonitoringSpan(NextId(), serviceId, "redis", "GET session:*", at + 1, cacheMs, false));
            var dbStart = at + 2 + cacheMs;
            var dbMs = Jitter(3, 2, 14);
            work.Add(new MonitoringSpan(NextId(), serviceId, "postgres", service.Id == "search" ? "SELECT products (full-text)" : "SELECT … FROM orders", dbStart, dbMs, false));

            var end = dbStart + dbMs;

            if (service.Id == "payments")
            {
                var processorMs = isError ? Jitter(4, 420, 700) : Jitter(4, 90, 160);
                work.Add(new MonitoringSpan(NextId(), serviceId, "payments", "POST card-processor /charges", end + 1, processorMs, isError));
                end += 1 + processorMs;
            }

            var serviceMs = end - at + Jitter(5, 2, 6);
            spans.Insert(1, new MonitoringSpan(serviceId, rootId, service.Id, entry.Message.Length > 48 ? entry.Message[..48].TrimEnd() + "…" : entry.Message, at, serviceMs, isError));
            spans.AddRange(work);
            spans.Insert(0, new MonitoringSpan(rootId, null, "api-gateway", "HTTP request", 0, at + serviceMs + Jitter(6, 1, 3), isError));
        }
        else
        {
            var rootId = NextId();
            var ownMs = Jitter(1, service.BaseLatencyMs * 0.6, service.BaseLatencyMs * 1.4);
            spans.Add(new MonitoringSpan(rootId, null, service.Id, entry.Message.Length > 48 ? entry.Message[..48].TrimEnd() + "…" : entry.Message, 0, Math.Max(ownMs, 1), isError));

            if (service.Kind is MonitoringServiceKind.Worker or MonitoringServiceKind.Devices)
                spans.Add(new MonitoringSpan(NextId(), rootId, "postgres", "INSERT … batch", Math.Max(ownMs, 1) * 0.3, Jitter(2, 2, 9), false));
        }

        return new MonitoringTrace(entry.TraceId, entry.Time, spans[0].ServiceId, spans);
    }

    /// <summary>The demo accounts: the on-call engineer.</summary>
    public static IReadOnlyList<ShowcaseDemoAccount> DemoAccounts() => new List<ShowcaseDemoAccount>
    {
        new(SeededEngineerId, EngineerRole, EngineerName, "priya.nair@example.com", "watch-demo-26", "PN"),
    };

    /// <summary>The engineers on the on-call rotation.</summary>
    public static IReadOnlyList<MonitoringEngineer> Engineers() => new List<MonitoringEngineer>
    {
        new("priya", EngineerName, "PN", "Platform"),
        new("marcus", "Marcus Osei", "MO", "Platform"),
        new("aiko", "Aiko Tanaka", "AT", "Identity"),
        new("daniel", "Daniel Okafor", "DO", "Payments"),
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

    /// <summary>
    /// The incidents: two active (a published critical one, and an acknowledged major one the public hasn't heard about),
    /// and resolved ones over the last 90 days, most published, some with a postmortem.
    /// </summary>
    public static List<MonitoringIncident> Incidents()
    {
        var updateId = 1;

        MonitoringIncidentUpdate Entry(DateTime time, string author, string text, MonitoringUpdateKind kind, MonitoringPublicStage? stage = null, string? accountId = null) =>
            new(time, author, text) { Id = updateId++, Kind = kind, Stage = stage, AccountId = accountId };

        var incidents = new List<MonitoringIncident>
        {
            new()
            {
                Id = 301, ServiceId = "payments", Severity = MonitoringSeverity.Critical, Impact = MonitoringImpact.Errors, Status = MonitoringIncidentStatus.Open, StartedAt = Now.AddMinutes(-18),
                Title = "Payments API returning 5xx errors",
                Summary = "About 6% of payment requests fail with 502 responses from the card processor.",
                Updates =
                {
                    Entry(Now.AddMinutes(-18), SystemAuthor, "Alert fired: error rate above 2% for 5 minutes.", MonitoringUpdateKind.Alert),
                    Entry(Now.AddMinutes(-12), "Marcus Osei", "We're investigating failed card payments at checkout. Some customers may need to try again.", MonitoringUpdateKind.Public, MonitoringPublicStage.Investigating),
                },
            },
            new()
            {
                Id = 300, ServiceId = "search", Severity = MonitoringSeverity.Major, Impact = MonitoringImpact.Latency, Status = MonitoringIncidentStatus.Acknowledged, StartedAt = Now.AddMinutes(-52),
                Assignee = EngineerName,
                Title = "Search latency above 300 ms",
                Summary = "Search results take two to three times longer than usual.",
                Updates =
                {
                    Entry(Now.AddMinutes(-52), SystemAuthor, "Alert fired: p95 latency above twice the normal range for 10 minutes.", MonitoringUpdateKind.Alert),
                    Entry(Now.AddMinutes(-41), EngineerName, "Acknowledged.", MonitoringUpdateKind.Status, accountId: SeededEngineerId),
                    Entry(Now.AddMinutes(-40), EngineerName, "The nightly reindex job is competing for CPU; throttling it now.", MonitoringUpdateKind.Note, accountId: SeededEngineerId),
                },
            },
            new()
            {
                Id = 299, ServiceId = "redis", Severity = MonitoringSeverity.Minor, Impact = MonitoringImpact.Latency, Status = MonitoringIncidentStatus.Resolved, StartedAt = Now.AddHours(-20), ResolvedAt = Now.AddHours(-19.4),
                Assignee = "Marcus Osei",
                Title = "Session cache evictions",
                Summary = "Memory pressure evicted sessions early; some users had to sign in again.",
                Updates =
                {
                    Entry(Now.AddHours(-20), SystemAuthor, "Alert fired: eviction rate above 500 per minute.", MonitoringUpdateKind.Alert),
                    Entry(Now.AddHours(-19.9), "Marcus Osei", "Some customers are being signed out unexpectedly. We're looking into it.", MonitoringUpdateKind.Public, MonitoringPublicStage.Investigating),
                    Entry(Now.AddHours(-19.5), "Marcus Osei", "Raised the memory limit and shortened the TTL of guest sessions.", MonitoringUpdateKind.Note),
                    Entry(Now.AddHours(-19.4), "Marcus Osei", "Sign-ins are staying active again. If you were signed out, signing in once more fixes it.", MonitoringUpdateKind.Public, MonitoringPublicStage.Resolved),
                    Entry(Now.AddHours(-19.4), "Marcus Osei", "Resolved.", MonitoringUpdateKind.Status),
                },
            },
            new()
            {
                Id = 298, ServiceId = "sensors", Severity = MonitoringSeverity.Major, Impact = MonitoringImpact.Outage, Status = MonitoringIncidentStatus.Resolved, StartedAt = Now.AddDays(-2).AddHours(-3), ResolvedAt = Now.AddDays(-2).AddHours(-1),
                Assignee = EngineerName,
                Title = "Warehouse sensors offline in bay 4",
                Summary = "38 temperature sensors stopped reporting after a gateway firmware update.",
                Updates =
                {
                    Entry(Now.AddDays(-2).AddHours(-3), SystemAuthor, "Alert fired: 38 devices missed 3 heartbeats.", MonitoringUpdateKind.Alert),
                    Entry(Now.AddDays(-2).AddHours(-2.7), EngineerName, "Temperature readings from warehouse bay 4 aren't arriving. We've found the cause: a gateway update, which we're rolling back.", MonitoringUpdateKind.Public, MonitoringPublicStage.Identified, SeededEngineerId),
                    Entry(Now.AddDays(-2).AddHours(-1), EngineerName, "All bay 4 sensors are reporting again. Readings from the gap were buffered and have been stored.", MonitoringUpdateKind.Public, MonitoringPublicStage.Resolved, SeededEngineerId),
                    Entry(Now.AddDays(-2).AddHours(-1), EngineerName, "Resolved.", MonitoringUpdateKind.Status, accountId: SeededEngineerId),
                },
                Postmortem = new MonitoringPostmortem
                {
                    Author = EngineerName,
                    Summary = "For two hours, 38 sensors in warehouse bay 4 sent no readings. Readings were buffered on the devices and stored once they reconnected, so no data was lost.",
                    RootCause = "Gateway firmware 4.2 changed the default heartbeat port. The bay 4 gateway was updated first, and its sensors could no longer reach it.",
                    ActionItems = { "Roll out gateway firmware to one gateway at a time, with a 30-minute soak.", "Add a heartbeat check to the firmware release checklist." },
                    WrittenAt = Now.AddDays(-1).AddHours(-4),
                    PublishedAt = Now.AddDays(-1).AddHours(-3),
                },
            },
            new()
            {
                Id = 297, ServiceId = "postgres", Severity = MonitoringSeverity.Critical, Impact = MonitoringImpact.Outage, Status = MonitoringIncidentStatus.Resolved, StartedAt = Now.AddDays(-6).AddHours(-2), ResolvedAt = Now.AddDays(-6).AddHours(-1.75),
                Assignee = EngineerName,
                Title = "Orders database failover",
                Summary = "The primary database restarted; writes failed for about 15 minutes until the replica took over.",
                Updates =
                {
                    Entry(Now.AddDays(-6).AddHours(-2), SystemAuthor, "Alert fired: primary unreachable.", MonitoringUpdateKind.Alert),
                    Entry(Now.AddDays(-6).AddHours(-1.95), EngineerName, "Customers can't place orders right now. We're investigating.", MonitoringUpdateKind.Public, MonitoringPublicStage.Investigating, SeededEngineerId),
                    Entry(Now.AddDays(-6).AddHours(-1.8), EngineerName, "Orders are going through again on our standby database. We're watching closely.", MonitoringUpdateKind.Public, MonitoringPublicStage.Monitoring, SeededEngineerId),
                    Entry(Now.AddDays(-6).AddHours(-1.75), EngineerName, "Everything is back to normal. Orders that failed can be placed again.", MonitoringUpdateKind.Public, MonitoringPublicStage.Resolved, SeededEngineerId),
                    Entry(Now.AddDays(-6).AddHours(-1.75), EngineerName, "Resolved; failover finished and writes recovered.", MonitoringUpdateKind.Status, accountId: SeededEngineerId),
                },
                Postmortem = new MonitoringPostmortem
                {
                    Author = "Marcus Osei",
                    Summary = "For 15 minutes customers couldn't place orders, because the orders database stopped accepting writes.",
                    RootCause = "A kernel update restarted the primary database host outside the maintenance window; automatic failover waited for a health check that took 12 minutes to time out.",
                    ActionItems = { "Pin kernel updates to the maintenance window.", "Lower the failover health-check timeout to 60 seconds." },
                    WrittenAt = Now.AddDays(-5),
                },
            },
        };

        // Older incidents, all resolved and published, so the 90-day uptime bars and the history have something to show.
        void Past(int id, string serviceId, MonitoringSeverity severity, MonitoringImpact impact, double daysAgo, int minutes, string title, string summary, string investigating, string resolved, string author)
        {
            var start = Now.AddDays(-daysAgo);
            var end = start.AddMinutes(minutes);

            incidents.Add(new MonitoringIncident
            {
                Id = id, ServiceId = serviceId, Severity = severity, Impact = impact, Status = MonitoringIncidentStatus.Resolved, StartedAt = start, ResolvedAt = end, Assignee = author,
                Title = title, Summary = summary,
                Updates =
                {
                    Entry(start, SystemAuthor, "Alert fired.", MonitoringUpdateKind.Alert),
                    Entry(start.AddMinutes(Math.Min(8, minutes / 3.0)), author, investigating, MonitoringUpdateKind.Public, MonitoringPublicStage.Investigating),
                    Entry(end, author, resolved, MonitoringUpdateKind.Public, MonitoringPublicStage.Resolved),
                    Entry(end, author, "Resolved.", MonitoringUpdateKind.Status),
                },
            });
        }

        Past(296, "api-gateway", MonitoringSeverity.Major, MonitoringImpact.Errors, 14.2, 40, "Elevated API errors", "Some API requests failed with 503 responses during a deploy.",
            "Some requests to our API are failing. We're looking into it.", "A faulty deploy was rolled back and the API is working normally.", "Aiko Tanaka");
        Past(295, "auth", MonitoringSeverity.Minor, MonitoringImpact.Latency, 23.6, 60, "Slow sign-ins", "Sign-ins took up to 5 seconds.",
            "Signing in is slower than usual. We're investigating.", "Sign-in times are back to normal.", "Aiko Tanaka");
        Past(294, "notifications", MonitoringSeverity.Major, MonitoringImpact.Outage, 31.1, 120, "Delayed emails and push notifications", "Order confirmations arrived up to two hours late.",
            "Order confirmation emails are delayed. Your orders are not affected.", "All delayed emails have been sent.", "Marcus Osei");
        Past(293, "payments", MonitoringSeverity.Critical, MonitoringImpact.Outage, 47.4, 25, "Card payments failing", "Card payments failed for 25 minutes after the card processor rotated its certificates.",
            "Card payments are failing at checkout. We're investigating.", "Card payments are working again.", "Daniel Okafor");
        Past(292, "search", MonitoringSeverity.Minor, MonitoringImpact.Latency, 60.3, 180, "Slow search results", "Search took about a second longer than usual.",
            "Search is slower than usual.", "Search speed is back to normal.", EngineerName);
        Past(291, "sensors", MonitoringSeverity.Major, MonitoringImpact.Outage, 78.8, 300, "Warehouse sensors offline", "Sensors in bays 1 and 2 stopped reporting during a network change.",
            "Sensor readings from two warehouse bays aren't arriving.", "All sensors are reporting again; buffered readings were stored.", "Marcus Osei");

        incidents.First(incident => incident.Id == 293).Postmortem = new MonitoringPostmortem
        {
            Author = "Daniel Okafor",
            Summary = "For 25 minutes no card payment went through. Customers saw a \"payment declined\" message; no card was charged.",
            RootCause = "The card processor rotated its TLS certificates, and our pinned certificate list didn't include the new one.",
            ActionItems = { "Pin the processor's certificate authority instead of its leaf certificates.", "Alert on TLS handshake failures to the processor." },
            WrittenAt = Now.AddDays(-45),
            PublishedAt = Now.AddDays(-44),
        };

        return incidents;
    }

    /// <summary>About two hours of log lines before the seed time, newest first. Payments errors and search warnings match the active incidents.</summary>
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

    /// <summary>The demo engineer's notifications in a fresh tab.</summary>
    public static List<MonitoringNotification> Notifications() => new()
    {
        new() { Id = 1, AccountId = SeededEngineerId, Kind = MonitoringNotificationKind.Update, Time = Now.AddDays(-5), IsRead = true, IncidentId = 297, Title = "Postmortem drafted: Orders database failover", Text = "Marcus Osei wrote the postmortem. It isn't published on the status page yet." },
        new() { Id = 2, AccountId = SeededEngineerId, Kind = MonitoringNotificationKind.OnCall, Time = RotationStart, IsRead = true, Title = "Your on-call week started", Text = "You're primary on call until Mon, Nov 30 at 09:00. Marcus Osei is secondary." },
        new() { Id = 3, AccountId = SeededEngineerId, Kind = MonitoringNotificationKind.Assigned, Time = Now.AddMinutes(-41), IsRead = true, IncidentId = 300, Title = "You acknowledged #300", Text = "Search latency above 300 ms is assigned to you." },
        new() { Id = 4, AccountId = SeededEngineerId, Kind = MonitoringNotificationKind.Alert, Time = Now.AddMinutes(-18), IncidentId = 301, Title = "Critical alert: Payments API returning 5xx errors", Text = "Error rate above 2% for 5 minutes. You're primary on call." },
        new() { Id = 5, AccountId = SeededEngineerId, Kind = MonitoringNotificationKind.Update, Time = Now.AddMinutes(-12), IncidentId = 301, Title = "Marcus Osei posted on the status page", Text = "#301 is now public: Investigating." },
    };

    /// <summary>The roles of the site.</summary>
    public static IReadOnlyList<ShowcaseRole> Roles() => new List<ShowcaseRole>
    {
        new(EngineerRole, "Engineer", "/showcase/monitoring/console", "/showcase/monitoring/console/account", "Works incidents, posts status page updates, reads logs and traces, and goes on call."),
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

    /// <summary>Returns a trace ID (16 hex characters) for a seed number.</summary>
    public static string TraceId(int seed) =>
        ((uint)(Hash(seed, 5, 7) * uint.MaxValue)).ToString("x8", CultureInfo.InvariantCulture) + ((uint)(Hash(seed, 9, 2) * uint.MaxValue)).ToString("x8", CultureInfo.InvariantCulture);

    #endregion

    #region Private Methods

    private static string Endpoint(MonitoringService service, int seed) => service.Kind switch
    {
        MonitoringServiceKind.Api => new[] { "GET /v1/products", "GET /v1/orders", "POST /v1/sessions", "GET /v1/users/me" }[seed % 4],
        MonitoringServiceKind.Worker => "job send-email",
        _ => service.Name,
    };

    #endregion

    #endregion
}
