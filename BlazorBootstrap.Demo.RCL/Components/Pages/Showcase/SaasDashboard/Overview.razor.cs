using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.SaasDashboard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.SaasDashboard;
using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.SaasDashboard;

/// <summary>
/// Dashboard home: KPI tiles, the revenue and plan-mix charts, the newest accounts, quotas, and recent activity.
/// </summary>
public partial class Overview : ComponentBase, IDisposable
{
    #region Fields and Constants

    private bool chartsReady;

    private DoughnutChart planChart = default!;

    private LineChart revenueChart = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var initializedDark = State.IsDarkTheme;

            await revenueChart.InitializeAsync(RevenueData(), RevenueOptions());
            await planChart.InitializeAsync(PlanData(), PlanOptions());
            chartsReady = true;

            // The theme switcher can report the color mode while the charts are still initializing; catch up once ready.
            if (State.IsDarkTheme != initializedDark)
                await RedrawChartsAsync();
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

    private string NameOf(int accountId) => State.FindAccount(accountId)?.Name ?? "Unknown account";

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        StateHasChanged();
        await RedrawChartsAsync();
    });

    private void OnThemeChanged() => InvokeAsync(RedrawChartsAsync);

    private ChartData PlanData()
    {
        var isDark = State.IsDarkTheme;
        var colors = SaasChartTheme.Series(isDark);
        var revenue = PlanRevenue;

        return new ChartData
        {
            Labels = revenue.Select(entry => entry.Plan.Name).ToList(),
            Datasets = new List<IChartDataset>
            {
                new DoughnutChartDataset
                {
                    Label = "MRR",
                    Data = revenue.Select(entry => (double?)entry.Revenue).ToList(),
                    BackgroundColor = revenue.Select((_, index) => colors[index % colors.Count]).ToList(),
                    BorderColor = revenue.Select(_ => SaasChartTheme.Surface(isDark)).ToList(),
                    BorderWidth = revenue.Select(_ => 3d).ToList(),
                    HoverOffset = revenue.Select(_ => 6d).ToList(),
                },
            },
        };
    }

    private DoughnutChartOptions PlanOptions()
    {
        var options = new DoughnutChartOptions { Responsive = true, MaintainAspectRatio = false };
        SaasChartTheme.Apply(options, State.IsDarkTheme);

        return options;
    }

    private async Task RedrawChartsAsync()
    {
        if (!chartsReady)
            return;

        await revenueChart.UpdateAsync(RevenueData(), RevenueOptions());
        await planChart.UpdateAsync(PlanData(), PlanOptions());
    }

    private ChartData RevenueData()
    {
        var isDark = State.IsDarkTheme;

        var mrr = new LineChartDataset
        {
            Label = "MRR",
            Data = State.RevenueHistory.Select(value => (double?)value).ToList(),
            BorderColor = SaasChartTheme.Primary(isDark),
            BackgroundColor = SaasChartTheme.Primary(isDark, .14),
            BorderWidth = 3,
            Tension = .35,
            PointRadius = new List<double> { 0 },
            PointHoverRadius = new List<double> { 5 },
            PointBackgroundColor = new List<string> { SaasChartTheme.Primary(isDark) },
        };
        mrr.FillToOrigin();

        return new ChartData
        {
            Labels = SaasDashboardSeedData.RevenueMonths().ToList(),
            Datasets = new List<IChartDataset>
            {
                mrr,
                new LineChartDataset
                {
                    Label = "Target",
                    Data = State.RevenueTargets.Select(value => (double?)value).ToList(),
                    BorderColor = SaasChartTheme.Muted(isDark),
                    BackgroundColor = SaasChartTheme.Muted(isDark),
                    BorderWidth = 2,
                    BorderDash = new List<double> { 6, 6 },
                    Tension = .35,
                    PointRadius = new List<double> { 0 },
                    PointHoverRadius = new List<double> { 4 },
                },
            },
        };
    }

    private LineChartOptions RevenueOptions()
    {
        var options = new LineChartOptions
        {
            Responsive = true,
            MaintainAspectRatio = false,
            Interaction = new Interaction { Mode = InteractionMode.Index, Intersect = false },
        };

        SaasChartTheme.Apply(options, State.IsDarkTheme);

        return options;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Within 5% of this month's target counts as on track (the seed data sits 4% under it).</summary>
    private bool IsOnTarget => State.RevenueHistory[^1] >= State.RevenueTargets[^1] * .95;

    private IEnumerable<Kpi> Kpis
    {
        get
        {
            var growth = State.MrrGrowthPercent;

            return new[]
            {
                new Kpi("Monthly recurring revenue", SaasDisplay.Money(State.Mrr), IconName.CurrencyDollar, $"{growth:+0.0;-0.0}%", "vs last month", growth >= 0),
                new Kpi("Active accounts", State.ActiveCount.ToString(CultureInfo.InvariantCulture), IconName.People, "+6", "this month", true),
                new Kpi("Accounts in trial", State.TrialCount.ToString(CultureInfo.InvariantCulture), IconName.HourglassSplit, "3", "end this week", true),
                new Kpi("Paid seats", State.PaidSeats.ToString("#,0", CultureInfo.InvariantCulture), IconName.PersonBadge, $"{State.SuspendedCount} suspended", "accounts", State.SuspendedCount == 0),
            };
        }
    }

    private IEnumerable<SaasAccount> NewestAccounts => State.Accounts.OrderByDescending(account => account.JoinedOn).ThenByDescending(account => account.Id).Take(5);

    private string PlanChartLabel => "Revenue by plan: " + string.Join(", ", PlanRevenue.Select(entry => $"{entry.Plan.Name} {SaasDisplay.Money(entry.Revenue)}"));

    private IReadOnlyList<(SaasPlan Plan, decimal Revenue)> PlanRevenue => State.Plans
        .Where(plan => plan.PricePerSeat > 0)
        .Select(plan => (plan, State.Accounts.Where(account => account.PlanId == plan.Id).Sum(State.MonthlyRevenueFor)))
        .ToList();

    private IEnumerable<Quota> Quotas => new[]
    {
        new Quota("API calls", 72, "1.44M of 2M"),
        new Quota("Storage", 41, "410 GB of 1 TB"),
        new Quota("Seats", Math.Min(100, Math.Round(State.PaidSeats / 15d)), $"{State.PaidSeats:#,0} of 1,500"),
    };

    private string RevenueChartLabel
    {
        get
        {
            var history = State.RevenueHistory;

            return $"Line chart of monthly recurring revenue from {SaasDisplay.Money((decimal)history[0])} to {SaasDisplay.Money((decimal)history[^1])} over 12 months, plotted against a dashed target line.";
        }
    }

    [Inject] private SaasDashboardState State { get; set; } = default!;

    #endregion

    #region Nested Types

    private sealed record Kpi(string Label, string Value, IconName IconName, string Trend, string TrendNote, bool IsUp);

    private sealed record Quota(string Label, double Percent, string Text);

    #endregion
}
