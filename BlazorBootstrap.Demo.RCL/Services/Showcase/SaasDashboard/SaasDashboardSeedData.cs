using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.SaasDashboard;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.SaasDashboard;

/// <summary>
/// Fixed, fictitious content for the "Pulseboard" SaaS admin dashboard. Every value is derived from
/// constants and index arithmetic, so each load shows the same data.
/// </summary>
public static class SaasDashboardSeedData
{
    #region Fields and Constants

    /// <summary>Number of seeded customer accounts.</summary>
    public const int AccountCount = 48;

    /// <summary>The fictitious product name.</summary>
    public const string ProductName = "Pulseboard";

    /// <summary>The fixed "today" every date on the site is relative to.</summary>
    public static readonly DateTime Today = new(2026, 11, 9);

    /// <summary>Time zones offered on the settings screen.</summary>
    public static readonly IReadOnlyList<string> TimeZones = new[]
    {
        "UTC", "America/New_York", "America/Los_Angeles", "Europe/London", "Europe/Berlin", "Asia/Kolkata", "Asia/Singapore", "Australia/Sydney",
    };

    private static readonly string[] companies =
    {
        "Northwind Outfitters", "Brightlane Health", "Orbitworks", "Fable & Finch", "Cinderpeak Labs", "Harborline Freight", "Mossgrove Studio", "Quillstone Media",
        "Tidewater Robotics", "Lanternfield Co", "Pinecrest Foods", "Violet Harbor", "Stonebridge Legal", "Skylark Travel", "Emberwick Games", "Ridgeview Energy",
    };

    private static readonly string[] countries =
    {
        "United States", "Germany", "India", "United Kingdom", "Canada", "Australia", "Brazil", "Japan", "Netherlands", "Singapore",
    };

    private static readonly string[] firstNames =
    {
        "Avery", "Jordan", "Riley", "Morgan", "Casey", "Quinn", "Rowan", "Skyler", "Emerson", "Finley", "Harper", "Kendall",
        "Logan", "Parker", "Reese", "Sawyer", "Tatum", "Blake", "Dakota", "Ellis", "Marlow", "Noor", "Sasha", "Wren",
    };

    private static readonly string[] lastNames =
    {
        "Ashdown", "Bellweather", "Calloway", "Dunmore", "Eastlake", "Fairbank", "Greenhollow", "Hartwell", "Ironside", "Juniper", "Kingsley", "Larkspur",
        "Merriweather", "Northcott", "Oakhurst", "Pemberton", "Quillfeather", "Ravenscroft", "Silverton", "Thornbury", "Underhill", "Vanterpool", "Whitlock", "Yarrow",
    };

    private static readonly double[] revenueFactors = { .58, .61, .66, .69, .74, .78, .83, .86, .90, .94, .97, 1 };

    private static readonly double[] revenueTargetFactors = { .60, .64, .68, .72, .76, .80, .84, .88, .92, .96, 1.0, 1.04 };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>The seeded customer accounts.</summary>
    public static List<SaasAccount> Accounts()
    {
        var accounts = new List<SaasAccount>(AccountCount);

        for (var index = 0; index < AccountCount; index++)
        {
            var firstName = firstNames[index % firstNames.Length];
            var lastName = lastNames[((index * 7) + 3 + (index / firstNames.Length * 5)) % lastNames.Length];
            var company = companies[((index * 5) + 1) % companies.Length];

            var status = index % 11 == 7 ? SaasAccountStatus.Suspended
                : index % 5 == 4 ? SaasAccountStatus.Trial
                : SaasAccountStatus.Active;

            var planId = status == SaasAccountStatus.Trial ? "pro"
                : (index % 6) switch
                {
                    0 or 3 => "business",
                    5 => "free",
                    _ => "pro",
                };

            var seats = planId switch
            {
                "free" => 1 + (index % 3),
                "pro" => 4 + ((index * 3) % 22),
                _ => 20 + ((index * 7) % 60),
            };

            var role = (index % 8) switch
            {
                0 or 4 => SaasAccountRole.Owner,
                1 or 5 or 6 => SaasAccountRole.Admin,
                2 => SaasAccountRole.Billing,
                _ => SaasAccountRole.Member,
            };

            var lastSeenOn = status == SaasAccountStatus.Suspended
                ? Today.AddDays(-(30 + (index % 40)))
                : Today.AddDays(-(index % 9)).AddHours(9 + (index % 8)).AddMinutes((index * 13) % 60);

            accounts.Add(new SaasAccount(
                Id: 1001 + index,
                Name: $"{firstName} {lastName}",
                Email: $"{firstName}.{lastName}@{Slug(company)}.example".ToLowerInvariant(),
                Company: company,
                Role: role,
                PlanId: planId,
                Seats: seats,
                Status: status,
                Country: countries[((index * 3) + 2) % countries.Length],
                JoinedOn: Today.AddDays(-(20 + ((index * 37) % 700))),
                LastSeenOn: lastSeenOn,
                ApiUsagePercent: ((index * 17) + 23) % 101,
                StorageUsagePercent: ((index * 29) + 11) % 101));
        }

        return accounts;
    }

    /// <summary>Four history entries per seeded account, newest first.</summary>
    public static List<SaasActivity> Activities(IEnumerable<SaasAccount> accounts)
    {
        var activities = new List<SaasActivity>();

        foreach (var account in accounts)
        {
            var offset = account.Id % 5;

            activities.Add(new(account.Id, account.LastSeenOn, "Signed in", $"From {account.Country}", IconName.BoxArrowInRight));
            activities.Add(new(account.Id, account.LastSeenOn.AddDays(-(2 + offset)), "Exported a report", "Weekly usage summary (CSV)", IconName.FileEarmarkArrowDown));
            activities.Add(new(account.Id, account.LastSeenOn.AddDays(-(9 + (offset * 2))), "Invited teammates", $"{1 + offset} invitations sent", IconName.PersonPlus));
            activities.Add(new(account.Id, account.JoinedOn, "Account created", $"Started on the {PlanName(account.PlanId)} plan", IconName.Flag));
        }

        return activities;
    }

    /// <summary>Traffic for one period. Values follow a fixed curve, so they never change between loads.</summary>
    public static SaasAnalyticsSnapshot Analytics(SaasAnalyticsRange range)
    {
        var (count, unitSessions, stepDays, format) = range switch
        {
            SaasAnalyticsRange.Last7Days => (7, 4200d, 1, "ddd"),
            SaasAnalyticsRange.Last30Days => (30, 4100d, 1, "MMM d"),
            SaasAnalyticsRange.Last90Days => (13, 28600d, 7, "MMM d"),
            _ => (12, 123000d, 0, "MMM"),
        };

        var sessions = new List<SaasSeriesPoint>(count);
        var signups = new List<SaasSeriesPoint>(count);

        for (var index = 0; index < count; index++)
        {
            var fromEnd = count - 1 - index;
            var date = stepDays == 0 ? Today.AddMonths(-fromEnd) : Today.AddDays(-fromEnd * stepDays);
            var label = date.ToString(format, CultureInfo.InvariantCulture);

            var value = Math.Round(unitSessions * (1 + (.16 * Math.Sin(index * .9)) + (.012 * index)));
            sessions.Add(new(label, value));
            signups.Add(new(label, Math.Round(value * (.021 + (.004 * Math.Cos(index * .7))))));
        }

        var total = sessions.Sum(point => point.Value);
        var shift = (int)range;

        var sources = new List<SaasTrafficSource>
        {
            new("Organic search", Math.Round(total * (.38 + (.01 * shift)))),
            new("Direct", Math.Round(total * (.24 - (.01 * shift)))),
            new("Referral", Math.Round(total * .16)),
            new("Social", Math.Round(total * (.13 - (.005 * shift)))),
            new("Email", Math.Round(total * (.09 + (.005 * shift)))),
        };

        var topPages = new List<SaasTopPage>
        {
            new("/pricing", (int)(total * .21), 6.4 + (.3 * shift)),
            new("/features/funnels", (int)(total * .17), 4.1 + (.2 * shift)),
            new("/blog/retention-playbook", (int)(total * .12), 2.8),
            new("/integrations", (int)(total * .09), 3.6 - (.1 * shift)),
            new("/docs/getting-started", (int)(total * .07), 1.9),
            new("/customers", (int)(total * .05), 3.2 + (.1 * shift)),
        };

        return new SaasAnalyticsSnapshot(sessions, signups, sources, topPages, 41.5 - (1.2 * shift), 3.4 + (.2 * shift));
    }

    /// <summary>The last six monthly invoices of the workspace.</summary>
    public static IReadOnlyList<SaasInvoice> Invoices()
    {
        var invoices = new List<SaasInvoice>();

        for (var index = 0; index < 6; index++)
        {
            var issuedOn = new DateTime(Today.Year, Today.Month, 1).AddMonths(-index);
            invoices.Add(new($"PB-{issuedOn:yyyyMM}-0{42 + index}", issuedOn, 1185m - (index * 79m), index > 0));
        }

        return invoices;
    }

    /// <summary>The seeded header notifications, newest first.</summary>
    public static List<SaasNotification> Notifications() => new()
    {
        new(1, "Usage spike detected", "API calls are 38% above the weekly average.", IconName.GraphUpArrow, Today.AddHours(8).AddMinutes(12), false),
        new(2, "3 trials end this week", "Reach out before they expire on Friday.", IconName.HourglassSplit, Today.AddHours(7).AddMinutes(40), false),
        new(3, "Invoice paid", "Orbitworks paid the October invoice.", IconName.Receipt, Today.AddDays(-1).AddHours(16), false),
        new(4, "New integration available", "Warehouse sync now supports hourly runs.", IconName.Plug, Today.AddDays(-2).AddHours(11), true),
        new(5, "Weekly digest sent", "The summary went to 4 workspace admins.", IconName.Envelope, Today.AddDays(-3).AddHours(9), true),
    };

    /// <summary>The subscription plans.</summary>
    public static IReadOnlyList<SaasPlan> Plans() => new List<SaasPlan>
    {
        new("free", "Free", 0m, 10_000, 5),
        new("pro", "Pro", 29m, 250_000, 100),
        new("business", "Business", 79m, 2_000_000, 1_000),
    };

    /// <summary>Monthly recurring revenue for the last 12 months; the current month is <paramref name="currentMrr" />.</summary>
    public static IReadOnlyList<double> RevenueHistory(decimal seedMrr, decimal currentMrr)
    {
        var history = revenueFactors.Select(factor => Math.Round((double)seedMrr * factor)).ToList();
        history[^1] = (double)currentMrr;

        return history;
    }

    /// <summary>The month labels of the 12-month revenue history, oldest first.</summary>
    public static IReadOnlyList<string> RevenueMonths() =>
        Enumerable.Range(0, revenueFactors.Length)
            .Select(index => Today.AddMonths(index - (revenueFactors.Length - 1)).ToString("MMM", CultureInfo.InvariantCulture))
            .ToList();

    /// <summary>The revenue target for the last 12 months.</summary>
    public static IReadOnlyList<double> RevenueTargets(decimal seedMrr) =>
        revenueTargetFactors.Select(factor => Math.Round((double)seedMrr * factor)).ToList();

    /// <summary>The workspace settings a fresh session starts with.</summary>
    public static SaasWorkspaceSettings Settings() =>
        new("Pulseboard HQ", "support@pulseboard.example", "UTC", "Monday", WeeklyDigest: true, SecurityAlerts: true, BillingEmails: true, ProductUpdates: false);

    #endregion

    #region Private Methods

    private static string PlanName(string planId) => Plans().First(plan => plan.Id == planId).Name;

    private static string Slug(string company) => new(company.Where(char.IsLetter).ToArray());

    #endregion

    #endregion
}
