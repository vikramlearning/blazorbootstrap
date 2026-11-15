using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

/// <summary>
/// Generated traffic and revenue curves for Lumora's charts. Values follow fixed curves, so they never change between loads:
/// Lumora's own site traffic for the operator console, a customer's product traffic for their workspace, and the 12-month
/// revenue history.
/// </summary>
public static class SaasAnalyticsSeedData
{
    #region Fields and Constants

    /// <summary>The pages of Lumora's own public site, for the operator's analytics.</summary>
    public static readonly IReadOnlyList<string> LumoraPages = new[]
    {
        "/pricing", "/features/funnels", "/blog/retention-playbook", "/integrations", "/docs/getting-started", "/customers",
    };

    /// <summary>The pages of a customer's product, for their workspace's analytics.</summary>
    public static readonly IReadOnlyList<string> ProductPages = new[]
    {
        "/products", "/checkout", "/search", "/account/orders", "/collections/new", "/help",
    };

    private static readonly double[] pageShares = { .21, .17, .12, .09, .07, .05 };

    private static readonly double[] revenueFactors = { .58, .61, .66, .69, .74, .78, .83, .86, .90, .94, .97, 1 };

    private static readonly double[] revenueTargetFactors = { .60, .64, .68, .72, .76, .80, .84, .88, .92, .96, 1.0, 1.04 };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Traffic for one period. <paramref name="scale" /> multiplies the volume (1 is Lumora's own site) and
    /// <paramref name="pages" /> names the six top pages.
    /// </summary>
    public static SaasAnalyticsSnapshot Analytics(SaasAnalyticsRange range, double scale, IReadOnlyList<string> pages)
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
        var today = SaasSeedData.Today;

        for (var index = 0; index < count; index++)
        {
            var fromEnd = count - 1 - index;
            var date = stepDays == 0 ? today.AddMonths(-fromEnd) : today.AddDays(-fromEnd * stepDays);
            var label = date.ToString(format, CultureInfo.InvariantCulture);

            var value = Math.Round(unitSessions * scale * (1 + (.16 * Math.Sin(index * .9)) + (.012 * index)));
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

        var topPages = pages.Take(pageShares.Length)
            .Select((path, index) => new SaasTopPage(path, (int)(total * pageShares[index]), Math.Round(6.4 - index + (.2 * shift), 1)))
            .ToList();

        return new SaasAnalyticsSnapshot(sessions, signups, sources, topPages, 41.5 - (1.2 * shift), 3.4 + (.2 * shift));
    }

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
            .Select(index => SaasSeedData.Today.AddMonths(index - (revenueFactors.Length - 1)).ToString("MMM", CultureInfo.InvariantCulture))
            .ToList();

    /// <summary>The revenue target for the last 12 months.</summary>
    public static IReadOnlyList<double> RevenueTargets(decimal seedMrr) =>
        revenueTargetFactors.Select(factor => Math.Round((double)seedMrr * factor)).ToList();

    #endregion

    #endregion
}
