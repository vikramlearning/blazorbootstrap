using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// Reports (project managers): KPI cards (open tasks, overdue, done this week, done in the last four weeks), tasks done per
/// week for the last eight weeks (<see cref="PbState.CompletedPerWeek" />), open tasks by status, each project's progress
/// (tasks and estimated hours done, overdue tasks, next milestone, status), and the overdue tasks with how long they are
/// overdue. Each chart is a <see cref="ReportsChart" /> that draws on its own first render and redraws when the theme or
/// its values change, with a text summary and a data table. The page re-renders on every state change, so a task moved to
/// Done on a board counts here at once.
/// </summary>
public partial class Reports : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How many weeks the throughput chart shows.</summary>
    private const int ThroughputWeeks = 8;

    /// <summary>The statuses of open tasks, in board order.</summary>
    private static readonly PbTaskStatus[] openStatuses = { PbTaskStatus.Todo, PbTaskStatus.InProgress, PbTaskStatus.Review };

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Share(int part, int whole) => whole <= 0 ? "0%" : (part * 100.0 / whole).ToString("0", CultureInfo.InvariantCulture) + "%";

    private static string ShortDate(DateTime date) => date.ToString("MMM d", CultureInfo.InvariantCulture);

    private static IReadOnlyList<ReportsChartSeries> StatusSeries(IReadOnlyList<(PbTaskStatus Status, int Count)> items) => new[]
    {
        new ReportsChartSeries("Open tasks", items.Select(item => (double)item.Count).ToList(), 0),
    };

    private static string StatusSummary(IReadOnlyList<(PbTaskStatus Status, int Count)> items)
    {
        var total = items.Sum(item => item.Count);

        return $"{PbDisplay.Plural(total, "open task")}: "
               + string.Join(", ", items.Select(item => $"{PbDisplay.StatusLabel(item.Status).ToLowerInvariant()} {item.Count} ({Share(item.Count, total)})")) + ".";
    }

    private static IReadOnlyList<ReportsChartSeries> ThroughputSeries(IReadOnlyList<PbWeekThroughput> weeks) => new[]
    {
        new ReportsChartSeries("Tasks done", weeks.Select(week => (double)week.Tasks).ToList(), 0),
    };

    private static string ThroughputSummary(IReadOnlyList<PbWeekThroughput> weeks)
    {
        var total = weeks.Sum(week => week.Tasks);

        if (total == 0)
            return $"No tasks were finished in the last {weeks.Count} weeks.";

        var busiest = weeks.MaxBy(week => week.Tasks)!;
        var thisWeek = weeks[^1];
        var average = (total / (double)weeks.Count).ToString("0.#", CultureInfo.InvariantCulture);

        return $"{PbDisplay.Plural(total, "task")} ({weeks.Sum(week => week.Hours).ToString(CultureInfo.InvariantCulture)} estimated hours) done in the last {weeks.Count} weeks, about {average} a week. "
               + $"The busiest was the week of {ShortDate(busiest.WeekStart)} with {busiest.Tasks}. This week so far: {thisWeek.Tasks}.";
    }

    private IEnumerable<Kpi> Kpis(IReadOnlyList<PbTask> tasks, IReadOnlyList<PbWeekThroughput> weeks, int overdue, DateTime today)
    {
        var open = tasks.Count(task => task.Status != PbTaskStatus.Done);
        var unassigned = tasks.Count(task => task.Status != PbTaskStatus.Done && task.AssigneeId is null);
        var weekStart = PbState.WeekStart(today);
        var doneThisWeek = tasks.Count(task => task.CompletedAt >= weekStart);
        var lastFour = weeks.TakeLast(4).Sum(week => week.Tasks);
        var activeProjects = State.Projects.Count(project => project.Status == PbProjectStatus.Active);

        return new[]
        {
            new Kpi("Open tasks", Count(open), $"{Count(unassigned)} unassigned · {PbDisplay.Plural(activeProjects, "active project")}"),
            new Kpi("Overdue", Count(overdue), overdue == 0 ? "Nothing is past its due date" : $"{Share(overdue, open)} of open tasks"),
            new Kpi("Done this week", Count(doneThisWeek), $"Since Monday, {PbDisplay.Date(weekStart)}"),
            new Kpi("Done in the last 4 weeks", Count(lastFour), "This week and the three before"),
        };

        static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);
    }

    /// <summary>The next milestone due today or later, with its progress, or why there is none.</summary>
    private string NextMilestoneText(PbProject project, DateTime today)
    {
        if (project.Milestones.Count == 0)
            return "No milestones";

        if (project.Milestones.Where(item => item.Due.Date >= today).OrderBy(item => item.Due).FirstOrDefault() is not { } next)
            return "None upcoming";

        var (done, total) = State.MilestoneProgress(project.Id, next.Id);

        return $"{next.Title} · {PbDisplay.Date(next.Due)} · {ManageProjects.ProgressText(done, total)}";
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>A KPI card.</summary>
    private sealed record Kpi(string Label, string Value, string Note);

    #endregion
}
