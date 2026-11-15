using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;
using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Home of the operator console, the MRR and trials dashboard: KPI tiles, the 12-month revenue chart against its target,
/// the plan mix by MRR or by tenant count, the trials that end soonest, the newest sales leads (read-only), and recent
/// activity across tenants. Charts use <see cref="SaasChartTheme" /> and redraw when the data or the theme changes.
/// </summary>
public partial class AdminOverview : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int MaxLeads = 3;

    private const int MaxTrials = 5;

    private bool chartsReady;

    private DoughnutChart planChart = default!;

    /// <summary>Whether the plan mix counts tenants instead of MRR.</summary>
    private bool planMixByTenants;

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

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static string DaysLeftText(int daysLeft) => daysLeft switch
    {
        0 => "Ends today",
        1 => "1 day left",
        _ => $"{daysLeft} days left",
    };

    /// <summary>Shortens a lead's message to about two lines, at a word break.</summary>
    private static string Excerpt(string message)
    {
        const int Length = 110;

        if (message.Length <= Length)
            return message;

        var cut = message.LastIndexOf(' ', Length);

        return message[..(cut > 40 ? cut : Length)].TrimEnd(',', '.', ';', ':') + "…";
    }

    private static BadgeColor LeadColor(SaasLeadStatus status) => status switch
    {
        SaasLeadStatus.New => BadgeColor.Primary,
        SaasLeadStatus.Contacted => BadgeColor.Info,
        SaasLeadStatus.Qualified => BadgeColor.Success,
        _ => BadgeColor.Secondary,
    };

    private static string Percent(double value) => value.ToString("0", CultureInfo.InvariantCulture) + "%";

    private static string TrialsSubtitle(int count) => count switch
    {
        0 => "Workspaces inside their free trial",
        1 => "1 workspace on trial",
        _ => $"{Count(count)} workspaces on trial, soonest end first",
    };

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
        var mix = PlanMix;

        return new ChartData
        {
            Labels = mix.Select(entry => entry.Plan.Name).ToList(),
            Datasets = new List<IChartDataset>
            {
                new DoughnutChartDataset
                {
                    Label = planMixByTenants ? "Tenants" : "MRR",
                    Data = mix.Select(entry => (double?)entry.Value).ToList(),
                    BackgroundColor = mix.Select(entry => colors[entry.Index % colors.Count]).ToList(),
                    BorderColor = mix.Select(_ => SaasChartTheme.Surface(isDark)).ToList(),
                    BorderWidth = mix.Select(_ => 3d).ToList(),
                    HoverOffset = mix.Select(_ => 6d).ToList(),
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
            Labels = SaasAnalyticsSeedData.RevenueMonths().ToList(),
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

    private async Task SetPlanMixAsync(bool byTenants)
    {
        if (planMixByTenants == byTenants)
            return;

        planMixByTenants = byTenants;

        if (chartsReady)
            await planChart.UpdateAsync(PlanData(), PlanOptions());
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string Greeting
    {
        get
        {
            var hour = State.Clock.Now.Hour;
            var part = hour < 12 ? "morning" : hour < 18 ? "afternoon" : "evening";
            var firstName = State.Identity.Current?.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            return firstName is null ? $"Good {part}" : $"Good {part}, {firstName}";
        }
    }

    /// <summary>Within 5% of this month's target counts as on track (the seeded month sits 4% under it).</summary>
    private bool IsOnTarget => State.RevenueHistory[^1] >= State.RevenueTargets[^1] * .95;

    private IEnumerable<Kpi> Kpis
    {
        get
        {
            var growth = State.MrrGrowthPercent;
            var endingSoon = State.Workspaces.Count(workspace => State.TrialDaysLeft(workspace) is <= 7);
            var paying = State.Workspaces.Count(workspace => State.MonthlyRevenueFor(workspace) > 0);
            var suspended = State.SuspendedCount;

            return new[]
            {
                new Kpi("Monthly recurring revenue", SaasDisplay.Money(State.Mrr), IconName.CurrencyDollar, "vs last month",
                    growth.ToString("+0.0;-0.0", CultureInfo.InvariantCulture) + "%", growth >= 0),
                new Kpi("Active tenants", Count(State.ActiveCount), IconName.Buildings, $"of {Count(State.Workspaces.Count)} tenants"),
                new Kpi("On trial", Count(State.TrialCount), IconName.HourglassSplit,
                    endingSoon switch { 0 => "none end in the next 7 days", 1 => "1 ends in the next 7 days", _ => $"{Count(endingSoon)} end in the next 7 days" }),
                new Kpi("Paid seats", Count(State.PaidSeats), IconName.PersonBadge, $"across {Count(paying)} paying {(paying == 1 ? "tenant" : "tenants")}"),
                new Kpi("Suspended tenants", Count(suspended), IconName.SlashCircle, suspended == 0 ? "none right now" : "their members can't use them"),
            };
        }
    }

    private string PlanChartLabel
    {
        get
        {
            var mix = PlanMix;

            if (mix.Count == 0)
                return planMixByTenants ? "Plan mix by tenants: no tenants yet." : "Plan mix by MRR: no revenue this month.";

            var total = mix.Sum(entry => entry.Value);

            return planMixByTenants
                ? "Doughnut chart of the plan mix by tenants: " + string.Join(", ", mix.Select(entry => $"{entry.Plan.Name} {Count((int)entry.Value)}"))
                : "Doughnut chart of the plan mix by MRR: " + string.Join(", ", mix.Select(entry => $"{entry.Plan.Name} {SaasDisplay.Money((decimal)entry.Value)} ({Percent(entry.Value / total * 100)})"));
        }
    }

    /// <summary>Each plan's share, in plan order, leaving out plans with nothing to show; the index keeps a plan's color in both views.</summary>
    private IReadOnlyList<(SaasPlan Plan, int Index, double Value)> PlanMix => State.Plans
        .Select((plan, index) => (plan, index, planMixByTenants
            ? State.Workspaces.Count(workspace => workspace.PlanId == plan.Id)
            : (double)State.Workspaces.Where(workspace => workspace.PlanId == plan.Id).Sum(State.MonthlyRevenueFor)))
        .Where(entry => entry.Item3 > 0)
        .ToList();

    private string RevenueChartLabel
    {
        get
        {
            var history = State.RevenueHistory;
            var target = State.RevenueTargets;

            return $"Line chart of monthly recurring revenue from {SaasDisplay.Money((decimal)history[0])} to {SaasDisplay.Money((decimal)history[^1])} over 12 months, "
                + $"plotted against a dashed target line that ends at {SaasDisplay.Money((decimal)target[^1])}.";
        }
    }

    [Inject] private SaasState State { get; set; } = default!;

    /// <summary>The workspaces on trial, the soonest end first.</summary>
    private IReadOnlyList<SaasWorkspace> Trials => State.Workspaces
        .Where(workspace => workspace.Status == SaasWorkspaceStatus.Trial)
        .OrderBy(workspace => workspace.TrialEndsOn ?? DateTime.MaxValue)
        .ThenBy(workspace => workspace.Name, StringComparer.OrdinalIgnoreCase)
        .ToList();

    #endregion

    #region Nested Types

    private sealed record Kpi(string Label, string Value, IconName IconName, string Note, string? Trend = null, bool IsUp = true);

    #endregion
}
