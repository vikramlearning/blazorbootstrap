using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;
using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Revenue in the operator console, from this tab's tenants and invoices (nothing generated): MRR, ARR, ARPA, paid seats,
/// money collected and refunded this month (<see cref="SaasState.CollectedByMonth" />), and open invoices; charts of
/// collected against refunded over 12 months, MRR by plan, and tenants by status, each with a summary and a visually hidden
/// table; the top 10 tenants by MRR; and the trial funnel from the workspaces and their activity. Charts use
/// <see cref="SaasChartTheme" /> and redraw when the data or the theme changes.
/// </summary>
public partial class AdminRevenue : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The activity title of a trial the customer upgraded in Billing before it ended.</summary>
    private const string ConvertedTitle = "Trial converted";

    /// <summary>The activity title of a trial that ran out and was billed.</summary>
    private const string EndedTitle = "Trial ended";

    private const int Months = 12;

    private const int TopCount = 10;

    private bool chartsReady;

    private BarChart collectionChart = default!;

    private DoughnutChart planChart = default!;

    private BarChart statusChart = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var initializedDark = State.IsDarkTheme;

            await collectionChart.InitializeAsync(CollectionData(), CollectionOptions());
            await planChart.InitializeAsync(PlanData(), PlanOptions());
            await statusChart.InitializeAsync(StatusData(), StatusOptions());
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

    /// <summary>A short month label for the chart axis, such as "Dec '25".</summary>
    private static string MonthLabel(DateTime month) => month.ToString("MMM \\'yy", CultureInfo.InvariantCulture);

    private static string MonthName(DateTime month) => month.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>What the ended trials are now, such as "3 paying, 1 past due, 0 suspended or archived".</summary>
    private static string OutcomeText(TrialFunnel trials)
    {
        var text = $"{Count(trials.PayingNow)} paying, {Count(trials.PastDueNow)} past due, {Count(trials.ClosedNow)} suspended or archived";

        return trials.FreeNow > 0 ? $"{text}, {Count(trials.FreeNow)} on a free plan" : text;
    }

    private static string Percent(double value) => value.ToString("0", CultureInfo.InvariantCulture) + "%";

    private ChartData CollectionData()
    {
        var colors = SaasChartTheme.Series(State.IsDarkTheme);
        var months = CollectedMonths;

        return new ChartData
        {
            Labels = months.Select(month => MonthLabel(month.Month)).ToList(),
            Datasets = new List<IChartDataset>
            {
                Bars("Collected", months.Select(month => (double)month.Collected), colors[0]),
                Bars("Refunded", months.Select(month => (double)month.Refunded), colors[3]),
            },
        };
    }

    private BarChartOptions CollectionOptions()
    {
        var options = new BarChartOptions { Responsive = true, MaintainAspectRatio = false };

        SaasChartTheme.Apply(options, State.IsDarkTheme);

        return options;
    }

    private static BarChartDataset Bars(string label, IEnumerable<double> values, string color) => new()
    {
        Label = label,
        Data = values.Select(value => (double?)value).ToList(),
        BackgroundColor = new List<string> { color },
        BorderColor = new List<string> { color },
        BorderWidth = new List<double> { 0 },
        BorderRadius = new List<double> { 6 },
        MaxBarThickness = 22,
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
        var mix = PlanMrr.Where(entry => entry.Mrr > 0).ToList();

        return new ChartData
        {
            Labels = mix.Select(entry => entry.Plan.Name).ToList(),
            Datasets = new List<IChartDataset>
            {
                new DoughnutChartDataset
                {
                    Label = "MRR",
                    Data = mix.Select(entry => (double?)entry.Mrr).ToList(),
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

        await collectionChart.UpdateAsync(CollectionData(), CollectionOptions());
        await planChart.UpdateAsync(PlanData(), PlanOptions());
        await statusChart.UpdateAsync(StatusData(), StatusOptions());
    }

    /// <summary>A status's bar color: green for active, amber for trials, the brand color for past due, gray for archived.</summary>
    private string StatusBarColor(SaasWorkspaceStatus status)
    {
        var isDark = State.IsDarkTheme;
        var colors = SaasChartTheme.Series(isDark);

        return status switch
        {
            SaasWorkspaceStatus.Active => colors[2],
            SaasWorkspaceStatus.Trial => colors[3],
            SaasWorkspaceStatus.PastDue => colors[0],
            SaasWorkspaceStatus.Suspended => colors[4],
            _ => SaasChartTheme.Muted(isDark),
        };
    }

    private ChartData StatusData()
    {
        var counts = StatusCounts;

        return new ChartData
        {
            Labels = counts.Select(entry => SaasDisplay.StatusName(entry.Status)).ToList(),
            Datasets = new List<IChartDataset>
            {
                new BarChartDataset
                {
                    Label = "Tenants",
                    Data = counts.Select(entry => (double?)entry.Tenants).ToList(),
                    BackgroundColor = counts.Select(entry => StatusBarColor(entry.Status)).ToList(),
                    BorderColor = counts.Select(entry => StatusBarColor(entry.Status)).ToList(),
                    BorderWidth = new List<double> { 0 },
                    BorderRadius = new List<double> { 6 },
                    MaxBarThickness = 36,
                },
            },
        };
    }

    private BarChartOptions StatusOptions()
    {
        var options = new BarChartOptions { Responsive = true, MaintainAspectRatio = false };

        SaasChartTheme.Apply(options, State.IsDarkTheme);
        options.Plugins.Legend.Display = false;

        return options;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Money collected and refunded per month, oldest first, from the invoices.</summary>
    private IReadOnlyList<SaasMonthRevenue> CollectedMonths => State.CollectedByMonth(Months);

    private string CollectionChartLabel
    {
        get
        {
            var months = CollectedMonths;
            var collected = months.Sum(month => month.Collected);
            var refunded = months.Sum(month => month.Refunded);
            var best = months.OrderByDescending(month => month.Collected).First();

            return collected == 0
                ? $"Bar chart of money collected and refunded per month from {MonthName(months[0].Month)} to {MonthName(months[^1].Month)}: nothing collected yet."
                : $"Bar chart of money collected and refunded per month from {MonthName(months[0].Month)} to {MonthName(months[^1].Month)}: "
                  + $"{SaasDisplay.Money(collected)} collected and {SaasDisplay.Money(refunded)} refunded in all; the most was collected in {MonthName(best.Month)}, {SaasDisplay.Money(best.Collected)}.";
        }
    }

    private IEnumerable<Kpi> Kpis
    {
        get
        {
            var mrr = State.Mrr;
            var paying = PayingCount;
            var thisMonth = CollectedMonths[^1];
            var open = OpenInvoices;
            var openTenants = open.Select(invoice => invoice.WorkspaceId).Distinct().Count();

            return new[]
            {
                new Kpi("MRR", SaasDisplay.Money(mrr), IconName.CurrencyDollar, paying == 0 ? "no paying tenants" : $"from {SaasState.Plural(paying, "paying tenant", "paying tenants")}"),
                new Kpi("ARR", SaasDisplay.Money(mrr * 12), IconName.GraphUpArrow, "MRR × 12"),
                new Kpi("ARPA", paying == 0 ? "—" : SaasDisplay.Money(mrr / paying), IconName.Calculator, paying == 0 ? "no paying tenants" : "MRR per paying tenant"),
                new Kpi("Paid seats", Count(State.PaidSeats), IconName.PersonBadge, $"across {SaasState.Plural(paying, "paying tenant", "paying tenants")}"),
                new Kpi("Collected this month", SaasDisplay.Money(thisMonth.Collected), IconName.CashStack, $"{MonthName(thisMonth.Month)}, before refunds"),
                new Kpi("Refunded this month", SaasDisplay.Money(thisMonth.Refunded), IconName.ArrowCounterclockwise, $"on invoices issued in {MonthName(thisMonth.Month)}"),
                new Kpi("Open invoices", Count(open.Count), IconName.Receipt,
                    open.Count == 0 ? "none waiting for payment" : $"{SaasDisplay.MoneyCents(open.Sum(invoice => invoice.Amount))} due from {SaasState.Plural(openTenants, "tenant", "tenants")}"),
            };
        }
    }

    private IReadOnlyList<SaasInvoice> OpenInvoices => State.AllInvoices.Where(invoice => invoice.Status == SaasInvoiceStatus.Open).ToList();

    private int PayingCount => State.Workspaces.Count(workspace => State.MonthlyRevenueFor(workspace) > 0);

    private string PlanChartLabel
    {
        get
        {
            var mix = PlanMrr.Where(entry => entry.Mrr > 0).ToList();
            var total = mix.Sum(entry => entry.Mrr);

            return total == 0
                ? "Doughnut chart of MRR by plan: no MRR this month."
                : "Doughnut chart of MRR by plan: " + string.Join(", ", mix.Select(entry => $"{entry.Plan.Name} {SaasDisplay.Money(entry.Mrr)} ({Percent((double)(entry.Mrr / total * 100))})")) + ".";
        }
    }

    /// <summary>Each plan in plan order with its MRR and paying tenants; the index keeps a plan's color.</summary>
    private IReadOnlyList<PlanRevenue> PlanMrr => State.Plans
        .Select((plan, index) =>
        {
            var paying = State.Workspaces.Where(workspace => workspace.PlanId == plan.Id && State.MonthlyRevenueFor(workspace) > 0).ToList();

            return new PlanRevenue(plan, index, paying.Sum(State.MonthlyRevenueFor), paying.Count);
        })
        .ToList();

    [Inject] private SaasState State { get; set; } = default!;

    private string StatusChartLabel => "Bar chart of tenants by status: "
        + string.Join(", ", StatusCounts.Select(entry => $"{SaasDisplay.StatusName(entry.Status)} {Count(entry.Tenants)}")) + ".";

    /// <summary>How many tenants have each status, and their MRR, in the order of the statuses.</summary>
    private IReadOnlyList<StatusCount> StatusCounts => Enum.GetValues<SaasWorkspaceStatus>()
        .Select(status =>
        {
            var tenants = State.Workspaces.Where(workspace => workspace.Status == status).ToList();

            return new StatusCount(status, tenants.Count, tenants.Sum(State.MonthlyRevenueFor));
        })
        .ToList();

    /// <summary>The paying tenants with the highest MRR, then by name.</summary>
    private IReadOnlyList<SaasWorkspace> TopTenants => State.Workspaces
        .Where(workspace => State.MonthlyRevenueFor(workspace) > 0)
        .OrderByDescending(State.MonthlyRevenueFor)
        .ThenBy(workspace => workspace.Name, StringComparer.OrdinalIgnoreCase)
        .Take(TopCount)
        .ToList();

    /// <summary>
    /// The trial funnel from the workspaces and their activity. Running trials come from the workspaces' status; ended trials
    /// from each workspace's latest "Trial converted" or "Trial ended" activity, and what they are now from their status.
    /// </summary>
    private TrialFunnel Trials
    {
        get
        {
            var running = State.Workspaces.Where(workspace => workspace.Status == SaasWorkspaceStatus.Trial).ToList();
            var endingSoon = running.Where(workspace => State.TrialDaysLeft(workspace) is <= 7).OrderBy(workspace => workspace.TrialEndsOn ?? DateTime.MaxValue).ToList();
            var outcomes = State.Workspaces
                .Where(workspace => workspace.Status != SaasWorkspaceStatus.Trial)
                .Select(workspace => (Workspace: workspace, Title: State.ActivityOf(workspace.Id).FirstOrDefault(activity => activity.Title is ConvertedTitle or EndedTitle)?.Title))
                .Where(entry => entry.Title is not null)
                .ToList();
            var ended = outcomes.Select(entry => entry.Workspace).ToList();

            return new TrialFunnel(
                State.TrialCount,
                running.Count(workspace => workspace.CardLast4 is not null),
                endingSoon.Count,
                endingSoon.FirstOrDefault(),
                outcomes.Count(entry => entry.Title == ConvertedTitle),
                outcomes.Count(entry => entry.Title == EndedTitle),
                ended.Count(workspace => State.MonthlyRevenueFor(workspace) > 0 && workspace.Status == SaasWorkspaceStatus.Active),
                ended.Count(workspace => workspace.Status == SaasWorkspaceStatus.PastDue),
                ended.Count(workspace => workspace.Status is SaasWorkspaceStatus.Suspended or SaasWorkspaceStatus.Archived));
        }
    }

    #endregion

    #region Nested Types

    private sealed record Kpi(string Label, string Value, IconName IconName, string Note);

    private sealed record PlanRevenue(SaasPlan Plan, int Index, decimal Mrr, int Paying);

    private sealed record StatusCount(SaasWorkspaceStatus Status, int Tenants, decimal Mrr);

    /// <summary>The trial funnel.</summary>
    /// <param name="Running">Workspaces on trial now.</param>
    /// <param name="RunningWithCard">Of those, the ones with a card on file (billed automatically when the trial ends).</param>
    /// <param name="EndingSoon">Trials ending within 7 days.</param>
    /// <param name="Soonest">The trial that ends first, if one ends within 7 days.</param>
    /// <param name="Converted">Ended trials whose latest outcome is "Trial converted" (upgraded in Billing during the trial).</param>
    /// <param name="RanOut">Ended trials whose latest outcome is "Trial ended" (the trial ran out and the plan was billed).</param>
    /// <param name="PayingNow">Ended trials that are active and count toward MRR now.</param>
    /// <param name="PastDueNow">Ended trials that are past due now (no card when the trial ended, or a failed renewal).</param>
    /// <param name="ClosedNow">Ended trials that are suspended or archived now.</param>
    private sealed record TrialFunnel(int Running, int RunningWithCard, int EndingSoon, SaasWorkspace? Soonest, int Converted, int RanOut, int PayingNow, int PastDueNow, int ClosedNow)
    {
        /// <summary>Gets how many trials ended, converted or run out.</summary>
        public int Ended => Converted + RanOut;

        /// <summary>Gets the ended trials that are active on a free plan now (the operator moved them).</summary>
        public int FreeNow => Ended - PayingNow - PastDueNow - ClosedNow;
    }

    #endregion
}
