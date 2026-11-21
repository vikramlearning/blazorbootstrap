using System.Globalization;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;
using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// The agent desk's SLA report: for the last 7, 14, or 30 days and all teams or one, the share of tickets resolved and first
/// replied to within their targets, the open tickets breached and at risk now, a stacked bar chart of tickets resolved per day
/// (within the SLA and late, with a summary label and a visually hidden table), and tables by priority, agent, and team. The
/// figures come from <see cref="HelpDeskReports.Build" />; the page rebuilds them when the range or team changes and whenever
/// the state changes (an action in another tab, or the site's clock).
/// </summary>
public partial class AgentReportSla : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultDays = 30;

    private static readonly int[] RangeOptions = { 7, 14, 30 };

    private bool chartReady;

    private BarChart dailyChart = default!;

    private int days = DefaultDays;

    private HelpDeskReport report = new();

    /// <summary>The team the report is limited to, or an empty string for every team.</summary>
    private string teamId = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await dailyChart.InitializeAsync(ChartData(), ChartOptions());
            chartReady = true;
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        Rebuild();
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    /// <summary>A day like "Nov 24".</summary>
    private static string DayLabel(DateOnly date) => date.ToString("MMM d", CultureInfo.InvariantCulture);

    /// <summary>An SLA target like "4 h" or "3 days".</summary>
    private static string Hours(int hours) => hours >= 24 && hours % 24 == 0
        ? (hours == 24 ? "1 day" : FormattableString.Invariant($"{hours / 24} days"))
        : FormattableString.Invariant($"{hours} h");

    /// <summary>A share like "92%", or "—" when there is nothing to divide.</summary>
    private static string PercentText(int part, int whole) => HelpDeskReport.Percent(part, whole) is { } percent ? FormattableString.Invariant($"{percent}%") : "—";

    private ChartData ChartData() => new()
    {
        Labels = report.Days.Select(day => DayLabel(day.Date)).ToList(),
        Datasets = new List<IChartDataset>
        {
            HelpDeskChartTheme.Bars("Within SLA", report.Days.Select(day => (double)day.ResolvedInSla), HelpDeskChartTheme.Good, "resolved"),
            HelpDeskChartTheme.Bars("Late", report.Days.Select(day => (double)(day.Resolved - day.ResolvedInSla)), HelpDeskChartTheme.Bad, "resolved"),
        },
    };

    private static BarChartOptions ChartOptions() => HelpDeskChartTheme.DailyBarOptions("Tickets resolved", stacked: true);

    private async Task OnRangeChangedAsync(ChangeEventArgs args)
    {
        if (int.TryParse(args.Value?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && RangeOptions.Contains(value))
            days = value;

        Rebuild();
        await RedrawChartAsync();
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        Rebuild();
        StateHasChanged();
        await RedrawChartAsync();
    });

    private async Task OnTeamChangedAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        teamId = State.FindTeam(value) is null ? string.Empty : value;

        Rebuild();
        await RedrawChartAsync();
    }

    /// <summary>Builds the report again; a team deleted in the meantime falls back to all teams.</summary>
    private void Rebuild()
    {
        if (teamId.Length > 0 && State.FindTeam(teamId) is null)
            teamId = string.Empty;

        report = HelpDeskReports.Build(State, days, teamId.Length == 0 ? null : teamId);
    }

    private async Task RedrawChartAsync()
    {
        if (!chartReady)
            return;

        await dailyChart.UpdateAsync(ChartData(), ChartOptions());
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string ChartLabel
    {
        get
        {
            var late = report.Resolved - report.ResolvedInSla;
            var busiest = report.Days.OrderByDescending(day => day.Resolved).ThenByDescending(day => day.Date).FirstOrDefault();

            return report.Resolved == 0
                ? $"Stacked bar chart of tickets resolved per day, {RangeText}: none resolved."
                : $"Stacked bar chart of tickets resolved per day, {RangeText}: {Count(report.ResolvedInSla)} within the SLA and {Count(late)} late; "
                  + $"the most were resolved on {DayLabel(busiest!.Date)}, {Count(busiest.Resolved)}.";
        }
    }

    private string FirstReplyNote => report.FirstReplyDue == 0
        ? "No first reply was due in this range."
        : $"{Count(report.FirstReplyInSla)} of {HelpDeskDisplay.Plural(report.FirstReplyDue, "ticket", "tickets")} answered in time";

    private string RangeText => $"{DayLabel(report.From)} to {DayLabel(report.To)}";

    private string ResolutionNote => report.Resolved == 0
        ? "No ticket was resolved in this range."
        : $"{Count(report.ResolvedInSla)} of {HelpDeskDisplay.Plural(report.Resolved, "resolved ticket", "resolved tickets")}";

    /// <summary>What the figures cover, such as "Oct 18 to Nov 16 · All teams".</summary>
    private string ScopeText => $"{RangeText} · {(teamId.Length == 0 ? "All teams" : State.FindTeam(teamId)?.Name ?? "All teams")}";

    [Inject] private HelpDeskState State { get; set; } = default!;

    #endregion
}
