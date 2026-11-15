using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;
using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// The workspace home: a greeting, usage this month against the plan's limits, the plan and its renewal, a product-traffic
/// chart with a period switch (with a summary sentence and a hidden table as its text alternative), the top pages and
/// traffic sources, the newest activity and notifications, and quick links to invite a teammate or change plan. A workspace
/// with no events yet gets a "Send your first events" hint in place of the traffic.
/// </summary>
/// <remarks>
/// The chosen period stays in the page. The chart draws on a canvas, so it re-applies <see cref="SaasChartTheme" /> when
/// <see cref="SaasState.ThemeChanged" /> reports a new color mode.
/// </remarks>
public partial class AppDashboard : ComponentBase, IDisposable
{
    #region Fields and Constants

    private static readonly (SaasAnalyticsRange Value, string Text, string Description)[] ranges =
    {
        (SaasAnalyticsRange.Last7Days, "7 days", "the last 7 days"),
        (SaasAnalyticsRange.Last30Days, "30 days", "the last 30 days"),
        (SaasAnalyticsRange.Last90Days, "90 days", "the last 90 days, by week"),
        (SaasAnalyticsRange.Last12Months, "12 months", "the last 12 months, by month"),
    };

    private bool chartReady;

    /// <summary>The workspace, event count, and period the chart last drew, so a state change redraws only when they differ.</summary>
    private (int WorkspaceId, long Events, SaasAnalyticsRange Range)? drawn;

    private SaasAnalyticsRange range = SaasAnalyticsRange.Last30Days;

    private string rangeMessage = string.Empty;

    private LineChart trafficChart = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // The chart is only in the page while a workspace is shown (not while the layout's guard is), so it starts when it
        // first appears and starts again if it was taken away (a sign-out) and came back.
        if (State.CurrentWorkspace is not { } workspace)
        {
            chartReady = false;
            drawn = null;
        }
        else if (!chartReady && trafficChart is not null)
        {
            chartReady = true;
            var initializedDark = State.IsDarkTheme;

            await trafficChart.InitializeAsync(ChartDataFor(workspace), ChartOptions());
            drawn = (workspace.Id, workspace.EventsThisMonth, range);

            // The theme switcher can report the color mode while the chart is still starting; catch up once ready.
            if (State.IsDarkTheme != initializedDark)
                await RedrawChartAsync(force: true);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;
        State.ThemeChanged += OnThemeChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and theme changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        State.ThemeChanged -= OnThemeChanged;
    }

    #endregion

    #region Private Methods

    private static string CardText(SaasWorkspace workspace) =>
        workspace is { CardBrand: { } brand, CardLast4: { } last4 }
            ? FormattableString.Invariant($"{brand} ending in {last4}, expires {workspace.CardExpiryMonth:00}/{workspace.CardExpiryYear}")
            : "No card on file";

    private static string Count(double value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static string Percent(double value) => value.ToString("0", CultureInfo.InvariantCulture) + "%";

    private static Quota QuotaOf(string id, string label, string used, long usedCount, long limit, string unitPlural, Func<long, string> format)
    {
        if (limit <= 0)
            return new Quota(id, label, used, null, $"No limit on your plan · {used} {unitPlural} so far", false);

        var percent = usedCount * 100d / limit;
        var note = percent switch
        {
            >= 100 => $"Limit reached · {Percent(percent)} used",
            >= 90 => $"Almost at the limit · {Percent(percent)} used",
            >= 75 => $"Getting close to the limit · {Percent(percent)} used",
            _ => $"{Percent(percent)} used",
        };

        return new Quota(id, label, $"{used} of {format(limit)}", percent, note, percent >= 90);
    }

    private static string SeatText(int seats) => seats == 1 ? "1 seat" : $"{seats.ToString(CultureInfo.InvariantCulture)} seats";

    private static string TrafficSummary(SaasAnalyticsSnapshot snapshot)
    {
        var sessions = snapshot.Sessions;

        if (sessions.Count == 0)
            return "No sessions in this period.";

        var lowest = sessions.MinBy(point => point.Value)!;
        var highest = sessions.MaxBy(point => point.Value)!;

        return $"{Count(sessions.Sum(point => point.Value))} sessions in total, from {Count(lowest.Value)} at the lowest ({lowest.Label}) "
            + $"to {Count(highest.Value)} at the highest ({highest.Label}). Bounce rate {snapshot.BounceRatePercent.ToString("0.0", CultureInfo.InvariantCulture)}%, "
            + $"average session {snapshot.AverageSessionMinutes.ToString("0.0", CultureInfo.InvariantCulture)} minutes.";
    }

    private ChartData ChartDataFor(SaasWorkspace workspace)
    {
        var isDark = State.IsDarkTheme;
        var snapshot = Snapshot(workspace);
        var sessions = new LineChartDataset
        {
            Label = "Sessions",
            Data = snapshot.Sessions.Select(point => (double?)point.Value).ToList(),
            BorderColor = SaasChartTheme.Primary(isDark),
            BackgroundColor = SaasChartTheme.Primary(isDark, .12),
            BorderWidth = 2.5,
            Tension = 0,
            PointRadius = new List<double> { range == SaasAnalyticsRange.Last30Days ? 2 : 3 },
            PointHoverRadius = new List<double> { 5 },
            PointBackgroundColor = new List<string> { SaasChartTheme.Primary(isDark) },
            PointBorderColor = new List<string> { SaasChartTheme.Primary(isDark) },
        };
        sessions.FillToOrigin();

        return new ChartData { Labels = snapshot.Sessions.Select(point => point.Label).ToList(), Datasets = new List<IChartDataset> { sessions } };
    }

    private LineChartOptions ChartOptions()
    {
        var options = new LineChartOptions
        {
            Responsive = true,
            MaintainAspectRatio = false,
            Locale = "en-US",
            Interaction = new Interaction { Mode = InteractionMode.Index, Intersect = false },
        };

        SaasChartTheme.Apply(options, State.IsDarkTheme);
        options.Plugins.Legend.Display = false;
        options.Scales.Y!.BeginAtZero = true;

        return options;
    }

    private string Greeting(SaasMember member)
    {
        var firstName = member.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? member.Name;
        var partOfDay = State.Clock.Now.Hour switch
        {
            < 12 => "Good morning",
            < 18 => "Good afternoon",
            _ => "Good evening",
        };

        return $"{partOfDay}, {firstName}";
    }

    private async Task OnRangeChangedAsync(SaasAnalyticsRange value)
    {
        if (value == range)
            return;

        range = value;
        rangeMessage = $"Showing traffic over {RangeDescription}.";
        await RedrawChartAsync(force: false);
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        StateHasChanged();
        await RedrawChartAsync(force: false);
    });

    private void OnThemeChanged() => InvokeAsync(() => RedrawChartAsync(force: true));

    /// <summary>Redraws the chart when its data changed, or always when <paramref name="force" /> is set (a new color mode).</summary>
    private async Task RedrawChartAsync(bool force)
    {
        if (!chartReady || State.CurrentWorkspace is not { } workspace)
            return;

        var key = (workspace.Id, workspace.EventsThisMonth, range);

        if (!force && drawn == key)
            return;

        drawn = key;
        await trafficChart.UpdateAsync(ChartDataFor(workspace), ChartOptions());
    }

    private string RenewalText(SaasWorkspace workspace, SaasPlan plan)
    {
        if (State.TrialDaysLeft(workspace) is { } daysLeft)
        {
            var ends = workspace.TrialEndsOn is { } endsOn ? SaasDisplay.Date(endsOn) : "soon";

            return daysLeft switch
            {
                0 => $"Ends today ({ends})",
                1 => $"Ends {ends}, 1 day left",
                _ => $"Ends {ends}, {daysLeft} days left",
            };
        }

        return workspace.Status switch
        {
            SaasWorkspaceStatus.Suspended => "Paused while the workspace is suspended",
            SaasWorkspaceStatus.Archived => "None: the workspace is archived",
            SaasWorkspaceStatus.PastDue => $"The payment due {SaasDisplay.Date(workspace.RenewsOn)} failed; update the card in Billing",
            _ when plan.MonthlyPricePerSeat == 0 => "Free plan, nothing to pay",
            _ => $"Renews on {SaasDisplay.Date(workspace.RenewsOn)}",
        };
    }

    private IEnumerable<Quota> Quotas(SaasWorkspace workspace, SaasPlan plan)
    {
        var seatsUsed = State.SeatsUsed(workspace.Id);

        yield return QuotaOf("events", "Events tracked", SaasDisplay.Events(workspace.EventsThisMonth), workspace.EventsThisMonth, plan.EventLimit, "events", SaasDisplay.Events);
        yield return QuotaOf("seats", "Seats used", seatsUsed.ToString(CultureInfo.InvariantCulture), seatsUsed, workspace.Seats, "seats",
            value => value.ToString(CultureInfo.InvariantCulture));
        yield return QuotaOf("dashboards", "Dashboards", workspace.Dashboards.ToString(CultureInfo.InvariantCulture), workspace.Dashboards, plan.DashboardLimit, "dashboards",
            value => value.ToString(CultureInfo.InvariantCulture));
    }

    private SaasAnalyticsSnapshot Snapshot(SaasWorkspace workspace) => State.WorkspaceAnalytics(workspace, range);

    #endregion

    #endregion

    #region Properties, Indexers

    private string RangeDescription => ranges.First(option => option.Value == range).Description;

    [Inject] private SaasState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One usage meter: the amount used against the limit, the percentage (none when unlimited), and a note in words.</summary>
    private sealed record Quota(string Id, string Label, string Amount, double? Percent, string Note, bool IsHigh);

    #endregion
}
