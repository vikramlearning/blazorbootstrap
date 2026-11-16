using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// The signed-in instructor's roster (<see cref="LearningState.Roster" />): a summary line ("12 learners across 2 courses"),
/// filters by course, name, and status (All, In progress, Completed, Not started, each with its count), a sort (last active,
/// name, or progress), and a table with a caption and header scopes: learner, course, enrollment date, progress in words and as
/// a bar with the same words as its value text, last activity, and the certificate code linking to the certificate (or a dash).
/// Progress counts the published lessons only. The page re-renders whenever the state changes.
/// </summary>
public partial class TeachLearners : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const string SortActive = "active";

    private const string SortName = "name";

    private const string SortProgress = "progress";

    private const string StatusAll = "all";

    private const string StatusCompleted = "completed";

    private const string StatusInProgress = "in-progress";

    private const string StatusNotStarted = "not-started";

    private static readonly string[] Sorts = { SortActive, SortName, SortProgress };

    private static readonly string[] StatusFilters = { StatusAll, StatusInProgress, StatusCompleted, StatusNotStarted };

    /// <summary>The course the roster is limited to, as an ID, or an empty string for every course.</summary>
    private string courseFilter = string.Empty;

    private string search = string.Empty;

    private string sort = SortActive;

    private string statusFilter = StatusAll;

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

    private static bool MatchesStatus(LearningRosterRow row, string filter) => filter switch
    {
        StatusCompleted => row.IsComplete,
        StatusNotStarted => row.CompletedCount == 0,
        StatusInProgress => row.CompletedCount > 0 && !row.IsComplete,
        _ => true,
    };

    private void OnCourseChanged(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        courseFilter = State.MyCourses.Any(course => course.Id.ToString(CultureInfo.InvariantCulture) == value) ? value : string.Empty;
    }

    private void OnSortChanged(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? SortActive;
        sort = Sorts.Contains(value) ? value : SortActive;
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        // A course deleted in the meantime falls back to every course.
        if (courseFilter.Length > 0 && State.MyCourses.All(course => course.Id.ToString(CultureInfo.InvariantCulture) != courseFilter))
            courseFilter = string.Empty;

        StateHasChanged();
    });

    private void OnStatusChanged(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? StatusAll;
        statusFilter = StatusFilters.Contains(value) ? value : StatusAll;
    }

    private static string Plural(int count, string one, string many) =>
        count == 1 ? $"1 {one}" : $"{count.ToString("#,0", CultureInfo.InvariantCulture)} {many}";

    /// <summary>Progress in words, like "5 of 9 lessons, 55%".</summary>
    private static string ProgressText(LearningRosterRow row) => row.LessonCount == 0
        ? "No published lessons yet"
        : string.Create(CultureInfo.InvariantCulture, $"{row.CompletedCount} of {Plural(row.LessonCount, "lesson", "lessons")}, {row.Percent}%");

    private static string RowKey(LearningRosterRow row) => string.Create(CultureInfo.InvariantCulture, $"{row.Enrollment.AccountId}:{row.Course.Id}");

    private string ScopeText(int shown, int total) => total == 0
        ? "No enrollments yet."
        : shown == total
            ? $"Showing all {Plural(total, "enrollment", "enrollments")}."
            : $"Showing {shown.ToString("#,0", CultureInfo.InvariantCulture)} of {Plural(total, "enrollment", "enrollments")}.";

    /// <summary>The rows that match the course and name filters (the status filter's counts are over these).</summary>
    private List<LearningRosterRow> Scoped(IReadOnlyList<LearningRosterRow> rows)
    {
        var text = search.Trim();

        return rows
            .Where(row => courseFilter.Length == 0 || row.Course.Id.ToString(CultureInfo.InvariantCulture) == courseFilter)
            .Where(row => text.Length == 0 || row.Enrollment.LearnerName.Contains(text, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>Sorts the rows; the roster already comes the most recently active first.</summary>
    private IEnumerable<LearningRosterRow> Sorted(IEnumerable<LearningRosterRow> rows) => sort switch
    {
        SortName => rows.OrderBy(row => row.Enrollment.LearnerName, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.Course.Title, StringComparer.OrdinalIgnoreCase),
        SortProgress => rows.OrderByDescending(row => row.Percent).ThenBy(row => row.Enrollment.LearnerName, StringComparer.OrdinalIgnoreCase),
        _ => rows.OrderByDescending(row => row.Enrollment.LastActivity),
    };

    /// <summary>A status option's text with its count over the course and name filters, such as "Completed (3)".</summary>
    private static string StatusLabel(string filter, List<LearningRosterRow> scoped)
    {
        var name = filter switch
        {
            StatusInProgress => "In progress",
            StatusCompleted => "Completed",
            StatusNotStarted => "Not started",
            _ => "All",
        };

        return string.Create(CultureInfo.InvariantCulture, $"{name} ({scoped.Count(row => MatchesStatus(row, filter))})");
    }

    /// <summary>"12 learners across 2 courses": distinct learners and the courses they're enrolled in.</summary>
    private static string SummaryText(IReadOnlyList<LearningRosterRow> rows)
    {
        if (rows.Count == 0)
            return "No learners yet.";

        var learners = rows.Select(row => row.Enrollment.AccountId).Distinct().Count();
        var courses = rows.Select(row => row.Course.Id).Distinct().Count();

        return $"{Plural(learners, "learner", "learners")} across {Plural(courses, "course", "courses")}";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string SortText => sort switch
    {
        SortName => "sorted by name, A to Z",
        SortProgress => "sorted by progress, furthest first",
        _ => "sorted by last activity, latest first",
    };

    [Inject] private LearningState State { get; set; } = default!;

    #endregion
}
