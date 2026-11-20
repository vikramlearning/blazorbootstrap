using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

/// <summary>
/// Fixed, fictitious content for the "Stackwatch" monitoring site: services, incidents (with their internal and public
/// timelines, about two hundred over the last year), escalation policies and the pages they sent, maintenance windows,
/// engineers and the on-call rotation, notifications, the audit log, log lines, and traces. Metric values, log lines, and the older incidents come from
/// deterministic wave-and-hash generators (no random numbers), so every fresh tab starts the same way.
/// </summary>
public static class MonitoringSeedData
{
    #region Fields and Constants

    /// <summary>The demo admin's name (also on the rotation).</summary>
    public const string AdminName = "Marcus Osei";

    /// <summary>The role of an admin: everything an engineer does, plus alert rules, failure drills, escalation policies, maintenance, and services.</summary>
    public const string AdminRole = "admin";

    /// <summary>The roles that use the console: engineers and admins.</summary>
    public const string ConsoleRoles = EngineerRole + "," + AdminRole;

    /// <summary>The demo engineer's name.</summary>
    public const string EngineerName = "Priya Nair";

    /// <summary>The role of an engineer (works incidents, publishes status page updates, reads logs and traces, goes on call).</summary>
    public const string EngineerRole = "engineer";

    /// <summary>How many samples the charts keep.</summary>
    public const int HistoryLength = 30;

    /// <summary>The fictitious product name.</summary>
    public const string ProductName = "Stackwatch";

    /// <summary>The ID of the demo admin account (also his ID on the rotation).</summary>
    public const string SeededAdminId = "marcus";

    /// <summary>The ID of the demo engineer account (also her ID on the rotation).</summary>
    public const string SeededEngineerId = "priya";

    /// <summary>The name of the fictitious company whose status page this is.</summary>
    public const string StatusPageName = "Stackwatch Status";

    /// <summary>The author of entries the monitoring system writes.</summary>
    public const string SystemAuthor = "Stackwatch alerts";

    /// <summary>Seconds between live samples (finalize OI-09).</summary>
    public const int TickSeconds = 2;

    /// <summary>How many log lines before the seed time the tab starts with (about six hours of them).</summary>
    public const int SeedLogLines = 360;

    /// <summary>The number of the oldest generated incident; the generated history runs up to the hand-written incidents.</summary>
    public const int FirstHistoryIncidentId = 101;

    /// <summary>The site's base route (the same as <c>MonitoringLayout.Base</c>); audit and notification links stay under it.</summary>
    private const string SiteBase = DemoRouteConstants.Showcase_URL_Monitoring;

    /// <summary>The console's base route (the same as <c>ConsoleLayout.Base</c>).</summary>
    private const string ConsoleBase = SiteBase + "/console";

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

    /// <summary>The alert rules in a fresh tab: one per important signal; two post on the status page on their own.</summary>
    public static List<MonitoringAlertRule> AlertRules() => new()
    {
        new() { Id = 1, Name = "Payments error rate above 2%", ServiceId = "payments", Metric = MonitoringAlertMetric.ErrorRate, Threshold = 2, ForSeconds = 20, Severity = MonitoringSeverity.Critical, AutoPublish = true, PublicMessage = "Some card payments are failing at checkout. We're investigating.", LastFiredAt = Now.AddMinutes(-18), LastIncidentId = 301 },
        new() { Id = 2, Name = "API gateway error rate above 2%", ServiceId = "api-gateway", Metric = MonitoringAlertMetric.ErrorRate, Threshold = 2, ForSeconds = 20, Severity = MonitoringSeverity.Major },
        new() { Id = 3, Name = "Search p95 latency above 300 ms", ServiceId = "search", Metric = MonitoringAlertMetric.Latency, Threshold = 300, ForSeconds = 30, Severity = MonitoringSeverity.Major, LastFiredAt = Now.AddMinutes(-52), LastIncidentId = 300 },
        new() { Id = 4, Name = "Orders database CPU above 90%", ServiceId = "postgres", Metric = MonitoringAlertMetric.Cpu, Threshold = 90, ForSeconds = 30, Severity = MonitoringSeverity.Major },
        new() { Id = 5, Name = "Notification worker error rate above 5%", ServiceId = "notifications", Metric = MonitoringAlertMetric.ErrorRate, Threshold = 5, ForSeconds = 20, Severity = MonitoringSeverity.Minor },
        new() { Id = 6, Name = "Warehouse sensors not reporting", ServiceId = "sensors", Metric = MonitoringAlertMetric.ErrorRate, Threshold = 20, ForSeconds = 20, Severity = MonitoringSeverity.Major, AutoPublish = true, PublicMessage = "Readings from some warehouse sensors aren't arriving. We're investigating." },
    };

    /// <summary>
    /// The audit log a fresh tab starts with: what people did on the seeded incidents (declared, acknowledged, assigned,
    /// posted on the status page, added a note, resolved, wrote and published a postmortem), the alerts the rules fired, and
    /// the admins' setup changes (alert rules, maintenance windows, a service). The newest 150 entries are kept, oldest first.
    /// </summary>
    public static List<ShowcaseAuditEntry> AuditEntries(IReadOnlyList<MonitoringIncident> incidents, IReadOnlyList<MonitoringAlertRule> rules, IReadOnlyList<MonitoringMaintenance> windows,
        IReadOnlyList<MonitoringEscalationPolicy> policies)
    {
        var lines = new List<(DateTime At, string Actor, string Action, string Target, string? Url, string? Details)>();

        foreach (var incident in incidents)
        {
            var target = FormattableString.Invariant($"Incident #{incident.Id}");
            var url = FormattableString.Invariant($"{ConsoleBase}/incidents/{incident.Id}");

            foreach (var update in incident.Updates.Where(update => update.Time <= Now))
            {
                if (update.Author == SystemAuthor)
                {
                    if (update.Kind == MonitoringUpdateKind.Alert && incident.RuleId is not null)
                        lines.Add((update.Time, SystemAuthor, "Alert fired", target, url, incident.Title));
                    else if (update.Kind == MonitoringUpdateKind.Page)
                        lines.Add((update.Time, SystemAuthor, "Paged", target, url, Clip(update.Text, 120)));

                    continue;
                }

                var entry = update.Kind switch
                {
                    MonitoringUpdateKind.Alert => ("Declared", incident.Title),
                    MonitoringUpdateKind.Public => ("Posted on the status page", update.Stage is { } stage ? $"{stage}: {Clip(update.Text, 120)}" : null),
                    MonitoringUpdateKind.Note => ("Added a note", Clip(update.Text, 120)),
                    MonitoringUpdateKind.Severity => ("Changed severity", Clip(update.Text, 120)),
                    _ when update.Text.StartsWith("Acknowledged", StringComparison.Ordinal) => ("Acknowledged", incident.Title),
                    _ when update.Text.StartsWith("Assigned to ", StringComparison.Ordinal) => ("Assigned", update.Text.TrimEnd('.')),
                    _ when update.Text.StartsWith("Resolved", StringComparison.Ordinal) => ("Resolved", incident.Title),
                    _ => ((string?)null, (string?)null),
                };

                if (entry.Item1 is { } action)
                    lines.Add((update.Time, update.Author, action, target, url, entry.Item2));
            }

            if (incident.Postmortem is { } postmortem)
            {
                lines.Add((postmortem.WrittenAt, postmortem.Author, "Wrote the postmortem", target, url, null));

                if (postmortem.PublishedAt is { } published)
                    lines.Add((published, postmortem.Author, "Published the postmortem", target, url, null));
            }
        }

        foreach (var rule in rules)
            lines.Add((Now.AddDays(-40 - (rule.Id * 6)).AddHours(-rule.Id), AdminName, "Created alert rule", $"Alert rule: {rule.Name}", $"{ConsoleBase}/admin/alerts", null));

        foreach (var policy in policies)
            lines.Add((Now.AddDays(-75 + (policy.Id * 3)).AddHours(-2), AdminName, "Created escalation policy", $"Escalation policy: {policy.Name}", $"{ConsoleBase}/admin/escalation", null));

        lines.Add((Now.AddDays(-20).AddHours(-4), AdminName, "Updated escalation policy", "Escalation policy: Checkout path: page fast", $"{ConsoleBase}/admin/escalation", "Secondary after 2 min (was 5 min)"));

        foreach (var window in windows)
        {
            var scheduledAt = window.Start <= Now ? window.Start.AddDays(-4) : Now.AddDays(-1).AddHours(-2);
            lines.Add((scheduledAt, window.CreatedBy, "Scheduled maintenance", $"Maintenance: {window.Title}", $"{ConsoleBase}/admin/maintenance", null));

            if (window.CancelledAt is { } cancelled)
                lines.Add((cancelled, window.CreatedBy, "Cancelled maintenance", $"Maintenance: {window.Title}", $"{ConsoleBase}/admin/maintenance", null));
        }

        lines.Add((Now.AddDays(-12).AddHours(-3), AdminName, "Updated service", "Service: Search", $"{ConsoleBase}/admin/services/search", "Normal p95 latency 140 ms"));
        lines.Add((Now.AddDays(-9).AddHours(-6), AdminName, "Updated alert rule", "Alert rule: Search p95 latency above 300 ms", $"{ConsoleBase}/admin/alerts", "Threshold 300 ms for 30 s"));

        return lines
            .Where(line => line.At <= Now)
            .OrderByDescending(line => line.At)
            .Take(150)
            .OrderBy(line => line.At)
            .Select((line, index) => new ShowcaseAuditEntry
            {
                Id = index + 1,
                At = line.At,
                Actor = line.Actor,
                Action = line.Action,
                Target = line.Target,
                TargetUrl = line.Url,
                Details = line.Details,
            })
            .ToList();
    }

    /// <summary>
    /// The escalation policies in a fresh tab: the checkout path pages fast (the secondary after 2 minutes, the payments
    /// lead after 20 more, twice over), the data stores page their owners' team, and a default covers everything else.
    /// </summary>
    public static List<MonitoringEscalationPolicy> EscalationPolicies() => new()
    {
        new() { Id = 1, Name = "Checkout path: page fast", ServiceIds = { "payments", "api-gateway", "auth" }, SecondaryAfterMinutes = 2, ThirdEngineerId = "daniel", ThirdAfterMinutes = 20, Repeats = 1 },
        new() { Id = 2, Name = "Data stores", ServiceIds = { "postgres", "redis" }, SecondaryAfterMinutes = 5, ThirdEngineerId = "aiko", ThirdAfterMinutes = 10 },
        new() { Id = 3, Name = "Default: everything else", IsDefault = true, SecondaryAfterMinutes = 10, Repeats = 2 },
    };

    /// <summary>The demo accounts: the on-call engineer, and the admin who runs the monitoring setup.</summary>
    public static IReadOnlyList<ShowcaseDemoAccount> DemoAccounts() => new List<ShowcaseDemoAccount>
    {
        new(SeededEngineerId, EngineerRole, EngineerName, "priya.nair@example.com", "watch-demo-26", "PN"),
        new(SeededAdminId, AdminRole, AdminName, "marcus.osei@example.com", "watch-admin-26", "MO"),
    };

    /// <summary>
    /// Maintenance windows in a fresh tab: a database upgrade in two days, a finished cache resize, and the year's earlier
    /// windows (one cancelled). The on-call engineers were told about the ones that already started.
    /// </summary>
    public static List<MonitoringMaintenance> Maintenance()
    {
        var windows = new List<MonitoringMaintenance>
        {
            new()
            {
                Id = 1, Title = "Orders database upgrade", CreatedBy = AdminName, ServiceIds = { "postgres" },
                Start = Now.Date.AddDays(2).AddHours(2), End = Now.Date.AddDays(2).AddHours(3),
                Description = "We're upgrading the orders database. Placing orders may be unavailable for a few minutes during the window.",
            },
            new()
            {
                Id = 2, Title = "Session cache resize", CreatedBy = AdminName, ServiceIds = { "redis" }, StartNotified = true,
                Start = Now.Date.AddDays(-5).AddHours(1), End = Now.Date.AddDays(-5).AddHours(1.5),
                Description = "We added memory to the session cache. Some customers may have had to sign in again.",
            },
        };

        // Earlier windows, oldest first: (days ago, start hour, hours, title, services, description, cancelled).
        var past = new (int DaysAgo, int Hour, double Hours, string Title, string[] ServiceIds, string Description, bool Cancelled)[]
        {
            (330, 2, 2, "API gateway TLS certificate rotation", new[] { "api-gateway" }, "We're rotating certificates. A few requests may need a retry.", false),
            (301, 3, 1, "Search index rebuild", new[] { "search" }, "Search results may be incomplete for up to an hour.", false),
            (274, 2, 3, "Orders database storage upgrade", new[] { "postgres" }, "Placing orders may pause for a few minutes.", false),
            (240, 1, 1, "Auth service key rotation", new[] { "auth" }, "You may need to sign in again.", false),
            (211, 2, 2, "Warehouse gateway firmware update", new[] { "sensors" }, "Sensor readings arrive late during the update; none are lost.", false),
            (183, 2, 1.5, "Notification worker queue migration", new[] { "notifications" }, "Emails and push notifications may arrive a few minutes late.", true),
            (176, 2, 1.5, "Notification worker queue migration", new[] { "notifications" }, "Emails and push notifications may arrive a few minutes late.", false),
            (149, 3, 2, "Payments processor failover test", new[] { "payments" }, "Card payments may take longer than usual.", false),
            (118, 2, 4, "Data center network maintenance", new[] { "api-gateway", "auth", "postgres" }, "Short interruptions are possible while we move traffic.", false),
            (86, 1, 1, "Session cache version upgrade", new[] { "redis" }, "Some customers may have to sign in again.", false),
            (51, 2, 2, "Search cluster resize", new[] { "search" }, "Search may be slower than usual.", false),
            (23, 2, 1, "Orders database minor version upgrade", new[] { "postgres" }, "Placing orders may pause for a minute.", false),
        };

        var id = 3;

        foreach (var (daysAgo, hour, hours, title, serviceIds, description, cancelled) in past)
        {
            var start = Now.Date.AddDays(-daysAgo).AddHours(hour);

            windows.Add(new MonitoringMaintenance
            {
                Id = id++, Title = title, CreatedBy = AdminName, ServiceIds = serviceIds.ToList(), Start = start, End = start.AddHours(hours), Description = description,
                CancelledAt = cancelled ? start.AddDays(-2) : null, StartNotified = true,
            });
        }

        return windows;
    }

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
                Id = 301, RuleId = 1, ServiceId = "payments", Severity = MonitoringSeverity.Critical, Impact = MonitoringImpact.Errors, Status = MonitoringIncidentStatus.Open, StartedAt = Now.AddMinutes(-18),
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
                Id = 300, RuleId = 3, ServiceId = "search", Severity = MonitoringSeverity.Major, Impact = MonitoringImpact.Latency, Status = MonitoringIncidentStatus.Acknowledged, StartedAt = Now.AddMinutes(-52),
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

        // About six months more: the alerts of an ordinary year, numbered before the hand-written ones (see History).
        incidents.AddRange(History(Entry));

        // The escalation policies paged the on-call engineers: #301 is still paging (the primary and the secondary were
        // paged, step 3 is due in a few minutes), #300 was acknowledged right after the secondary's page, and the recent
        // past incidents show their pages and acknowledgements.
        var policies = EscalationPolicies();

        foreach (var incident in incidents.Where(incident => incident.Id >= 271))
        {
            var policy = policies.FirstOrDefault(item => item.IsActive && !item.IsDefault && item.ServiceIds.Contains(incident.ServiceId)) ?? policies.First(item => item.IsDefault);
            var acknowledged = incident.Updates.FirstOrDefault(update => update.Kind == MonitoringUpdateKind.Status && update.Text.StartsWith("Acknowledged", StringComparison.Ordinal));

            // The hand-written past incidents have no acknowledgement on their timeline: their assignee took them three minutes in.
            var ackAt = incident.Id == 301 ? (DateTime?)null : acknowledged?.Time ?? incident.StartedAt.AddMinutes(3);
            var ackBy = acknowledged?.Author ?? incident.Assignee ?? EngineerName;
            AddPaging(incident, policy, ackAt, ackBy, acknowledged, Entry);
        }

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

    /// <summary>About six hours of log lines before the seed time, newest first. Payments errors and search warnings match the active incidents.</summary>
    public static List<MonitoringLogEntry> Logs(IReadOnlyList<MonitoringService> services)
    {
        var entries = new List<MonitoringLogEntry>(SeedLogLines);

        for (var k = 0; k < SeedLogLines; k++)
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

    /// <summary>The demo engineer's and admin's notifications in a fresh tab; the two newest of each are unread.</summary>
    public static List<MonitoringNotification> Notifications() => new()
    {
        new() { Id = 1, AccountId = SeededEngineerId, Kind = MonitoringNotificationKind.Update, Time = Now.AddDays(-5), IsRead = true, IncidentId = 297, Title = "Postmortem drafted: Orders database failover", Text = "Marcus Osei wrote the postmortem. It isn't published on the status page yet." },
        new() { Id = 2, AccountId = SeededEngineerId, Kind = MonitoringNotificationKind.OnCall, Time = RotationStart, IsRead = true, Title = "Your on-call week started", Text = "You're primary on call until Mon, Nov 30 at 09:00. Marcus Osei is secondary." },
        new() { Id = 11, AccountId = SeededEngineerId, Kind = MonitoringNotificationKind.Page, Time = Now.AddMinutes(-52), IsRead = true, IncidentId = 300, Title = "Paged: incident #300 Search latency above 300 ms", Text = "Step 1 of 2, round 1 of 3: you're primary on call. Acknowledge to stop the escalation." },
        new() { Id = 3, AccountId = SeededEngineerId, Kind = MonitoringNotificationKind.Assigned, Time = Now.AddMinutes(-41), IsRead = true, IncidentId = 300, Title = "You acknowledged #300", Text = "Search latency above 300 ms is assigned to you." },
        new() { Id = 4, AccountId = SeededEngineerId, Kind = MonitoringNotificationKind.Page, Time = Now.AddMinutes(-18), IncidentId = 301, Title = "Paged: incident #301 Payments API returning 5xx errors", Text = "Step 1 of 3, round 1 of 2: you're primary on call. Acknowledge to stop the escalation." },
        new() { Id = 5, AccountId = SeededEngineerId, Kind = MonitoringNotificationKind.Update, Time = Now.AddMinutes(-12), IncidentId = 301, Title = "Marcus Osei posted on the status page", Text = "#301 is now public: Investigating." },
        new() { Id = 6, AccountId = SeededAdminId, Kind = MonitoringNotificationKind.Update, Time = Now.AddDays(-6).AddHours(-1.75), IsRead = true, IncidentId = 297, Title = "Priya Nair resolved #297", Text = "Orders database failover is resolved. It needs a postmortem." },
        new() { Id = 7, AccountId = SeededAdminId, Kind = MonitoringNotificationKind.Update, Time = Now.AddDays(-2).AddHours(-1), IsRead = true, IncidentId = 298, Title = "Priya Nair resolved #298", Text = "Warehouse sensors offline in bay 4 is resolved." },
        new() { Id = 8, AccountId = SeededAdminId, Kind = MonitoringNotificationKind.OnCall, Time = RotationStart, IsRead = true, Title = "You're secondary on call this week", Text = "Until Mon, Nov 30 at 09:00. Priya Nair is primary." },
        new() { Id = 12, AccountId = SeededAdminId, Kind = MonitoringNotificationKind.Page, Time = Now.AddMinutes(-42), IsRead = true, IncidentId = 300, Title = "Paged: incident #300 Search latency above 300 ms", Text = "Step 2 of 2, round 1 of 3: you're secondary on call. Acknowledge to stop the escalation." },
        new() { Id = 9, AccountId = SeededAdminId, Kind = MonitoringNotificationKind.Page, Time = Now.AddMinutes(-41), IncidentId = 300, Title = "Your page for #300 is covered", Text = "Priya Nair acknowledged Search latency above 300 ms. You don't need to respond." },
        new() { Id = 10, AccountId = SeededAdminId, Kind = MonitoringNotificationKind.Page, Time = Now.AddMinutes(-16), IncidentId = 301, Title = "Paged: incident #301 Payments API returning 5xx errors", Text = "Step 2 of 3, round 1 of 2: you're secondary on call. Acknowledge to stop the escalation." },
    };

    /// <summary>The roles of the site.</summary>
    public static IReadOnlyList<ShowcaseRole> Roles() => new List<ShowcaseRole>
    {
        new(EngineerRole, "Engineer", "/showcase/monitoring/console", "/showcase/monitoring/console/account", "Works the incident queue, gets paged and acknowledges, posts status page updates, reads logs and traces, and goes on call."),
        new(AdminRole, "Admin", "/showcase/monitoring/console", "/showcase/monitoring/console/account", "Everything an engineer does, plus alert rules and failure drills, escalation policies, maintenance windows, and the monitored services."),
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

    /// <summary>
    /// Gives a seeded incident the paging its policy would have done (the same schedule and wording as the state's engine):
    /// each page due before the acknowledgement, on the rotation of its time, with a timeline entry; then the
    /// acknowledgement stops it, or (with none) it is still paging.
    /// </summary>
    private static void AddPaging(MonitoringIncident incident, MonitoringEscalationPolicy policy, DateTime? ackAt, string ackBy, MonitoringIncidentUpdate? acknowledged,
        Func<DateTime, string, string, MonitoringUpdateKind, MonitoringPublicStage?, string?, MonitoringIncidentUpdate> entry)
    {
        var start = incident.Updates[0].Time;
        var paging = new MonitoringPaging
        {
            PolicyId = policy.Id, PolicyName = policy.Name, SecondaryAfterMinutes = policy.SecondaryAfterMinutes, ThirdEngineerId = policy.ThirdEngineerId,
            ThirdAfterMinutes = policy.ThirdAfterMinutes, Repeats = policy.Repeats, StartedAt = start,
        };
        var plan = MonitoringState.PagingSchedule(policy.SecondaryAfterMinutes, policy.ThirdAfterMinutes, policy.Repeats);
        var steps = plan.Max(item => item.Step);
        var rounds = plan.Max(item => item.Round) + 1;
        var names = Engineers().ToDictionary(engineer => engineer.Id, engineer => engineer.Name);
        var until = ackAt ?? Now;

        foreach (var (step, round, offset) in plan)
        {
            var at = start + offset;

            if (at > until || (ackAt is not null && at >= ackAt))
                break;

            var week = (int)Math.Floor((at - RotationStart).TotalDays / 7);
            var slot = ((week % RotationOrder.Count) + RotationOrder.Count) % RotationOrder.Count;
            var engineerId = step switch { 1 => RotationOrder[slot], 2 => RotationOrder[(slot + 1) % RotationOrder.Count], _ => policy.ThirdEngineerId! };
            var channels = incident.Severity == MonitoringSeverity.Critical ? "in-app, email, push, and text message" : "in-app, email, and push";
            var where = FormattableString.Invariant($"step {step} of {steps}") + (rounds > 1 ? FormattableString.Invariant($", round {round + 1} of {rounds}") : string.Empty);
            var role = step switch { 1 => "primary on call", 2 => "secondary on call", _ => "named in the policy" };

            paging.Pages.Add(new MonitoringPage { Step = step, Round = round, EngineerId = engineerId, At = at, Channels = channels });
            Insert(incident, entry(at, SystemAuthor, $"Paged {names[engineerId]} ({role}, {where}) by {channels}.", MonitoringUpdateKind.Page, null, null));
        }

        if (ackAt is { } acked)
        {
            var text = $"Acknowledged by {ackBy}; paging stopped after {(paging.Pages.Count == 1 ? "1 page" : FormattableString.Invariant($"{paging.Pages.Count} pages"))}.";
            var accountId = DemoAccounts().FirstOrDefault(account => account.Name == ackBy)?.Id;

            if (acknowledged is not null)
                incident.Updates[incident.Updates.IndexOf(acknowledged)] = acknowledged with { Text = text };
            else
                Insert(incident, entry(acked, ackBy, text, MonitoringUpdateKind.Status, null, accountId));

            paging.StopReason = MonitoringPagingStop.Acknowledged;
            paging.StoppedAt = acked;
            paging.AcknowledgedBy = ackBy;
            paging.AcknowledgedAt = acked;
        }

        incident.Paging = paging;
    }

    private static string Clip(string text, int maxLength) => text.Length <= maxLength ? text : string.Concat(text.AsSpan(0, maxLength - 1), "…");

    /// <summary>Puts a timeline entry in time order (after the entries at the same time).</summary>
    private static void Insert(MonitoringIncident incident, MonitoringIncidentUpdate update) =>
        incident.Updates.Insert(incident.Updates.FindLastIndex(item => item.Time <= update.Time) + 1, update);

    /// <summary>
    /// The incidents of an ordinary year before the hand-written ones (numbers <see cref="FirstHistoryIncidentId" /> to 290,
    /// from a year ago to about 80 days ago, in number order): each fired an alert, was acknowledged by that week's primary
    /// on call, and resolved. Most stayed engineers-only; the critical ones and some major ones were posted on the status page.
    /// Times are whole minutes, from <see cref="Hash" />.
    /// </summary>
    private static IEnumerable<MonitoringIncident> History(Func<DateTime, string, string, MonitoringUpdateKind, MonitoringPublicStage?, string?, MonitoringIncidentUpdate> entry)
    {
        var templates = new (string ServiceId, MonitoringImpact Impact, string Title, string Summary, string Investigating)[]
        {
            ("api-gateway", MonitoringImpact.Errors, "Elevated API errors", "Some API requests failed with 503 responses.", "Some requests to our API are failing. We're looking into it."),
            ("api-gateway", MonitoringImpact.Latency, "Slow API responses", "p95 latency behind the gateway doubled.", "Our API is responding slower than usual."),
            ("auth", MonitoringImpact.Latency, "Slow sign-ins", "Sign-ins took several seconds.", "Signing in is slower than usual."),
            ("auth", MonitoringImpact.Errors, "Sign-in errors", "Some sign-ins failed with a server error.", "Some customers can't sign in. We're investigating."),
            ("payments", MonitoringImpact.Errors, "Card payments failing for some customers", "The card processor returned errors for part of the traffic.", "Some card payments are failing at checkout."),
            ("payments", MonitoringImpact.Latency, "Slow checkout", "Charges took over a second.", "Checkout is slower than usual."),
            ("search", MonitoringImpact.Latency, "Slow search results", "Search queries took two to three times longer.", "Search is slower than usual."),
            ("search", MonitoringImpact.Errors, "Search errors", "Some searches returned an error.", "Some searches show an error. Please try again."),
            ("notifications", MonitoringImpact.Outage, "Delayed emails", "The email queue stopped draining for a while.", "Emails are arriving late. Your orders are not affected."),
            ("notifications", MonitoringImpact.Errors, "Push notifications failing", "The push provider rejected some notifications.", "Some push notifications aren't arriving."),
            ("postgres", MonitoringImpact.Latency, "Orders database slow writes", "Lock contention slowed writes to orders.", "Placing orders is slower than usual."),
            ("postgres", MonitoringImpact.Outage, "Orders database unreachable", "The primary stopped accepting connections.", "Customers can't place orders right now."),
            ("redis", MonitoringImpact.Latency, "Session cache evictions", "Memory pressure evicted sessions early.", "Some customers are being signed out."),
            ("redis", MonitoringImpact.Errors, "Session cache timeouts", "Cache calls timed out under load.", "Some pages are slow to load."),
            ("sensors", MonitoringImpact.Outage, "Warehouse sensors offline", "Sensors in one bay stopped reporting.", "Readings from some warehouse sensors aren't arriving."),
            ("sensors", MonitoringImpact.Errors, "Sensor readings rejected", "The gateway rejected malformed readings.", "Some sensor readings arrive late."),
        };
        var names = Engineers().ToDictionary(engineer => engineer.Id, engineer => engineer.Name);
        var count = 291 - FirstHistoryIncidentId;

        for (var index = 0; index < count; index++)
        {
            var id = FirstHistoryIncidentId + index;

            // A year ago up to about 80 days ago, every day and a half, give or take five hours: still in number order.
            var start = Minute(Now.AddDays(-365 + (index * 1.5)).AddMinutes((Hash(id, 2, 9) * 600) - 300));
            var (serviceId, impact, title, summary, investigating) = templates[(int)(Hash(id, 4, 1) * templates.Length) % templates.Length];
            var roll = Hash(id, 6, 3);
            var severity = roll < 0.12 ? MonitoringSeverity.Critical : roll < 0.45 ? MonitoringSeverity.Major : MonitoringSeverity.Minor;
            var minutes = severity switch
            {
                MonitoringSeverity.Critical => 10 + (int)(Hash(id, 7, 2) * 80),
                MonitoringSeverity.Major => 20 + (int)(Hash(id, 7, 2) * 130),
                _ => 8 + (int)(Hash(id, 7, 2) * 52),
            };
            var published = severity == MonitoringSeverity.Critical || (severity == MonitoringSeverity.Major && Hash(id, 8, 5) < 0.5);

            // The primary on call that week picked it up.
            var week = (int)Math.Floor((start - RotationStart).TotalDays / 7);
            var assigneeId = RotationOrder[((week % RotationOrder.Count) + RotationOrder.Count) % RotationOrder.Count];
            var assignee = names[assigneeId];
            var accountId = assigneeId is SeededEngineerId or SeededAdminId ? assigneeId : null;
            var acknowledged = start.AddMinutes(2 + (int)(Hash(id, 9, 4) * 7));
            var end = start.AddMinutes(Math.Max(minutes, (int)(acknowledged - start).TotalMinutes + 3));

            var incident = new MonitoringIncident
            {
                Id = id, ServiceId = serviceId, Severity = severity, Impact = impact, Status = MonitoringIncidentStatus.Resolved, StartedAt = start, ResolvedAt = end, Assignee = assignee,
                Title = title, Summary = summary,
            };

            incident.Updates.Add(entry(start, SystemAuthor, "Alert fired.", MonitoringUpdateKind.Alert, null, null));
            incident.Updates.Add(entry(acknowledged, assignee, "Acknowledged.", MonitoringUpdateKind.Status, null, accountId));

            if (published)
            {
                incident.Updates.Add(entry(acknowledged.AddMinutes(2), assignee, investigating, MonitoringUpdateKind.Public, MonitoringPublicStage.Investigating, accountId));
                incident.Updates.Add(entry(end, assignee, "This is resolved, and everything works normally again.", MonitoringUpdateKind.Public, MonitoringPublicStage.Resolved, accountId));
            }

            incident.Updates.Add(entry(end, assignee, "Resolved.", MonitoringUpdateKind.Status, null, accountId));

            yield return incident;
        }
    }

    /// <summary>Drops the seconds and below, so a generated time is a whole minute.</summary>
    private static DateTime Minute(DateTime time) => new(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0, time.Kind);

    private static string Endpoint(MonitoringService service, int seed) => service.Kind switch
    {
        MonitoringServiceKind.Api => new[] { "GET /v1/products", "GET /v1/orders", "POST /v1/sessions", "GET /v1/users/me" }[seed % 4],
        MonitoringServiceKind.Worker => "job send-email",
        _ => service.Name,
    };

    #endregion

    #endregion
}
