using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;
using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// The agent desk's satisfaction report: for the last 7, 14, or 30 days and all teams or one, the CSAT (good ratings out of all
/// ratings), the ratings given, the response rate (rated out of resolved tickets), and the bad ratings; a stacked bar chart of
/// good and bad ratings per day (a day without ratings shows no bar rather than a 0% point), with a summary label and a
/// visually hidden table; ratings by agent; and the customers' recent comments with a Good, Bad, or All filter. The figures
/// come from <see cref="HelpDeskReports.Build" />; the page rebuilds them when the range or team changes and whenever the state
/// changes (an action in another tab, or the site's clock).
/// </summary>
public partial class AgentReportSatisfaction : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultDays = 30;

    private static readonly CommentFilter[] CommentFilters = { CommentFilter.All, CommentFilter.Good, CommentFilter.Bad };

    private static readonly int[] RangeOptions = { 7, 14, 30 };

    private bool chartReady;

    private CommentFilter commentFilter = CommentFilter.All;

    private ElementReference commentsHeading;

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

    private static BarChartOptions ChartOptions() => HelpDeskChartTheme.DailyBarOptions("Ratings", stacked: true);

    private static string CommentCountText(int count) => count == 1 ? "Showing 1 comment" : $"Showing {Count(count)} comments";

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    /// <summary>A day like "Nov 24".</summary>
    private static string DayLabel(DateOnly date) => date.ToString("MMM d", CultureInfo.InvariantCulture);

    /// <summary>A share like "92%", or "—" when there is nothing to divide.</summary>
    private static string PercentText(int part, int whole) => HelpDeskReport.Percent(part, whole) is { } percent ? FormattableString.Invariant($"{percent}%") : "—";

    private static string Plural(int count, string one, string many) => count == 1 ? $"1 {one}" : $"{Count(count)} {many}";

    private ChartData ChartData() => new()
    {
        Labels = report.Days.Select(day => DayLabel(day.Date)).ToList(),
        Datasets = new List<IChartDataset>
        {
            HelpDeskChartTheme.Bars("Good", report.Days.Select(day => (double)day.Good), HelpDeskChartTheme.Good, "ratings"),
            HelpDeskChartTheme.Bars("Bad", report.Days.Select(day => (double)day.Bad), HelpDeskChartTheme.Bad, "ratings"),
        },
    };

    /// <summary>A filter button's text with its count, such as "Bad (3)".</summary>
    private string CommentFilterLabel(CommentFilter filter) => FormattableString.Invariant($"{filter} ({report.Comments.Count(comment => Matches(comment, filter))})");

    private static bool Matches(HelpDeskRatingComment comment, CommentFilter filter) => filter switch
    {
        CommentFilter.Good => comment.Satisfaction == HelpDeskSatisfaction.Good,
        CommentFilter.Bad => comment.Satisfaction == HelpDeskSatisfaction.Bad,
        _ => true,
    };

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

    /// <summary>Shows only the bad ratings' comments and moves focus to the list's heading.</summary>
    private async Task ShowBadCommentsAsync()
    {
        commentFilter = CommentFilter.Bad;
        await HelpDeskDisplay.FocusAsync(commentsHeading);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string ChartLabel
    {
        get
        {
            var ratings = report.Good + report.Bad;
            var rated = report.Days.Count(day => day.Good + day.Bad > 0);

            return ratings == 0
                ? $"Stacked bar chart of good and bad ratings per day, {RangeText}: no ratings."
                : $"Stacked bar chart of good and bad ratings per day, {RangeText}: {Count(report.Good)} good and {Count(report.Bad)} bad, "
                  + $"over {Plural(rated, "day", "days")} with ratings; CSAT {PercentText(report.Good, ratings)}.";
        }
    }

    private string EmptyCommentsText => commentFilter switch
    {
        CommentFilter.Good => "No comments with a good rating in this range.",
        CommentFilter.Bad => "No comments with a bad rating in this range.",
        _ => "No customer left a comment with their rating in this range.",
    };

    private IReadOnlyList<HelpDeskRatingComment> FilteredComments => report.Comments.Where(comment => Matches(comment, commentFilter)).ToList();

    private string RangeText => $"{DayLabel(report.From)} to {DayLabel(report.To)}";

    /// <summary>What the figures cover, such as "Oct 18 to Nov 16 · All teams".</summary>
    private string ScopeText => $"{RangeText} · {(teamId.Length == 0 ? "All teams" : State.FindTeam(teamId)?.Name ?? "All teams")}";

    [Inject] private HelpDeskState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>Which comments the list shows.</summary>
    private enum CommentFilter
    {
        All,
        Good,
        Bad,
    }

    #endregion
}
