using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;
using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// The instructor's dashboard: a greeting by first name; the learners enrolled (roster rows), those active in the last 7 days,
/// the courses completed, the average rating of her published courses (visible reviews), and the questions waiting for an
/// answer (with a link to the Q&amp;A inbox); a completion funnel bar chart (enrolled, started, halfway, and completed for each
/// course with learners) and a ratings bar chart (visible reviews per star rating, for all her courses or one), each with a
/// heading, a summary in words, a label on the chart, and a visually hidden table; then the three newest questions waiting,
/// the three newest reviews (stars in words), and her courses with their status, learners, and rating. The page rebuilds the
/// figures and redraws both charts whenever the state changes (another tab, the site's clock).
/// </summary>
public partial class TeachDashboard : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How many days back a learner counts as active.</summary>
    private const int ActiveDays = 7;

    /// <summary>How many questions and reviews the lists show.</summary>
    private const int ListSize = 3;

    /// <summary>How many characters of a question or review the lists show.</summary>
    private const int ExcerptLength = 160;

    private bool chartsReady;

    private BarChart funnelChart = default!;

    /// <summary>The funnel of each of the instructor's courses with learners, in the order of <see cref="LearningState.MyCourses" />.</summary>
    private List<CourseFunnel> funnels = new();

    /// <summary>The visible reviews of the instructor's published courses (the average rating).</summary>
    private List<LearningReview> publishedReviews = new();

    private BarChart ratingChart = default!;

    /// <summary>The visible reviews per star rating for the course chosen (or every course); index 0 is one star.</summary>
    private int[] ratingCounts = new int[5];

    /// <summary>The course the ratings chart is limited to, as an ID, or an empty string for every course.</summary>
    private string ratingCourse = string.Empty;

    private List<LearningRosterRow> roster = new();

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await funnelChart.InitializeAsync(FunnelData(), FunnelOptions());
            await ratingChart.InitializeAsync(RatingData(), RatingOptions());
            chartsReady = true;
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

    private string CourseTitle(int courseId) => State.MyCourses.FirstOrDefault(course => course.Id == courseId)?.Title ?? "a removed course";

    /// <summary>The start of a plain-text question or review, cut at a word with an ellipsis when it is long.</summary>
    private static string Excerpt(string text)
    {
        text = text.ReplaceLineEndings(" ").Trim();

        if (text.Length <= ExcerptLength)
            return text;

        var cut = text.LastIndexOf(' ', ExcerptLength);

        return $"{text[..(cut > ExcerptLength / 2 ? cut : ExcerptLength)].TrimEnd()}…";
    }

    private ChartData FunnelData() => new()
    {
        Labels = funnels.Select(row => row.Course.Title).ToList(),
        Datasets = new List<IChartDataset>
        {
            LearningChartTheme.Bars("Enrolled", funnels.Select(row => (double)row.Funnel.Enrolled), LearningChartTheme.FunnelEnrolled),
            LearningChartTheme.Bars("Started", funnels.Select(row => (double)row.Funnel.Started), LearningChartTheme.FunnelStarted),
            LearningChartTheme.Bars("Halfway", funnels.Select(row => (double)row.Funnel.Halfway), LearningChartTheme.FunnelHalfway),
            LearningChartTheme.Bars("Completed", funnels.Select(row => (double)row.Funnel.Completed), LearningChartTheme.FunnelCompleted),
        },
    };

    private static BarChartOptions FunnelOptions() => LearningChartTheme.CountBarOptions("Learners", showLegend: true);

    /// <summary>The lesson a question is about, as a title, from the instructor's full view of the course (drafts included).</summary>
    private string LessonTitle(LearningQuestionThread thread) =>
        State.FindCourseForEdit(thread.CourseId)?.Lessons.FirstOrDefault(lesson => lesson.Id == thread.LessonId)?.Title ?? "a removed lesson";

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        Rebuild();
        StateHasChanged();
        await RedrawChartsAsync();
    });

    private async Task OnRatingCourseChangedAsync(ChangeEventArgs args)
    {
        ratingCourse = args.Value?.ToString() ?? string.Empty;

        Rebuild();
        await RedrawChartsAsync();
    }

    /// <summary>A share like "25%", rounded down.</summary>
    private static string PercentText(int part, int whole) => whole == 0 ? "0%" : FormattableString.Invariant($"{part * 100 / whole}%");

    private static string Plural(int count, string one, string many) => count == 1 ? $"1 {one}" : $"{Count(count)} {many}";

    private ChartData RatingData() => new()
    {
        Labels = Enumerable.Range(1, 5).Select(StarLabel).ToList(),
        Datasets = new List<IChartDataset>
        {
            LearningChartTheme.Bars("Reviews", ratingCounts.Select(count => (double)count), LearningChartTheme.Ratings),
        },
    };

    private static BarChartOptions RatingOptions() => LearningChartTheme.CountBarOptions("Reviews", showLegend: false);

    /// <summary>Works the figures out again; a course removed in the meantime falls back to every course in the ratings chart.</summary>
    private void Rebuild()
    {
        var courses = State.MyCourses;

        if (ratingCourse.Length > 0 && courses.All(course => course.Id.ToString(CultureInfo.InvariantCulture) != ratingCourse))
            ratingCourse = string.Empty;

        roster = State.Roster.ToList();

        funnels = courses
            .Select(course => State.Funnel(course.Id) is { Enrolled: > 0 } funnel ? new CourseFunnel(course, funnel) : null)
            .OfType<CourseFunnel>()
            .ToList();

        var published = courses.Where(course => course.IsPublished).Select(course => course.Id).ToHashSet();
        publishedReviews = State.InstructorReviews.Where(review => review.HiddenReason is null && published.Contains(review.CourseId)).ToList();

        ratingCounts = new int[5];

        foreach (var course in courses.Where(course => ratingCourse.Length == 0 || course.Id.ToString(CultureInfo.InvariantCulture) == ratingCourse))
        {
            var counts = State.RatingCounts(course.Id);

            for (var index = 0; index < ratingCounts.Length; index++)
                ratingCounts[index] += counts[index];
        }
    }

    private async Task RedrawChartsAsync()
    {
        if (!chartsReady)
            return;

        await funnelChart.UpdateAsync(FunnelData(), FunnelOptions());
        await ratingChart.UpdateAsync(RatingData(), RatingOptions());
    }

    /// <summary>A course's average rating and review count for the courses table, or "No reviews".</summary>
    private string RatingCell(int courseId)
    {
        var reviews = State.RatingCounts(courseId).Sum();

        return reviews == 0 ? "No reviews" : $"{LearningDisplay.Rating(State.AverageRating(courseId))} from {Plural(reviews, "review", "reviews")}";
    }

    private static string StarLabel(int stars) => stars == 1 ? "1 star" : string.Create(CultureInfo.InvariantCulture, $"{stars} stars");

    #endregion

    #endregion

    #region Properties, Indexers

    private int ActiveCount => roster.Count(row => row.Enrollment.LastActivity >= State.Clock.Now.AddDays(-ActiveDays));

    private int CompletedCount => roster.Count(row => row.IsComplete);

    private string EnrolledNote
    {
        get
        {
            if (roster.Count == 0)
                return "No one has enrolled yet.";

            var learners = roster.Select(row => row.Enrollment.AccountId).Distinct().Count();
            var courses = roster.Select(row => row.Course.Id).Distinct().Count();

            return $"{Plural(learners, "learner", "learners")} across {Plural(courses, "course", "courses")}";
        }
    }

    private string FunnelLabel
    {
        get
        {
            if (funnels.Count == 0)
                return "Bar chart of the completion funnel: no learners yet.";

            var parts = funnels.Select(row =>
                $"{row.Course.Title}: {row.Funnel.Enrolled} enrolled, {row.Funnel.Started} started, {row.Funnel.Halfway} halfway, {row.Funnel.Completed} completed");

            return $"Bar chart of the completion funnel by course. {string.Join("; ", parts)}.";
        }
    }

    private string FunnelSummary
    {
        get
        {
            if (funnels.Count == 0)
                return "No learners yet. Once learners enroll, this shows how far they get through each course.";

            var enrolled = funnels.Sum(row => row.Funnel.Enrolled);
            var completed = funnels.Sum(row => row.Funnel.Completed);

            return $"How far learners got through the published lessons of each course with learners: {Count(completed)} of {Count(enrolled)} enrolled ({PercentText(completed, enrolled)}) have completed.";
        }
    }

    /// <summary>"Good morning, Avery", by the site's clock and the signed-in instructor's first name.</summary>
    private string Greeting
    {
        get
        {
            var name = State.Identity.Current?.Name.Trim() ?? string.Empty;
            var first = name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            var hour = State.Clock.Now.Hour;
            var part = hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening";

            return first is null ? part : $"{part}, {first}";
        }
    }

    private List<LearningReview> LatestReviews => State.InstructorReviews.Take(ListSize).ToList();

    private string QuestionsUrl => $"{TeachLayout.Base}/questions";

    private string RatingLabel
    {
        get
        {
            var total = ratingCounts.Sum();

            if (total == 0)
                return $"Bar chart of reviews by star rating, {RatingScopeText}: no reviews.";

            var parts = Enumerable.Range(1, 5).Reverse().Select(stars => $"{StarLabel(stars)}: {ratingCounts[stars - 1]}");

            return $"Bar chart of reviews by star rating, {RatingScopeText}. {string.Join(", ", parts)}.";
        }
    }

    private string RatingScopeText => ratingCourse.Length == 0
        ? "all your courses"
        : State.MyCourses.FirstOrDefault(course => course.Id.ToString(CultureInfo.InvariantCulture) == ratingCourse)?.Title ?? "all your courses";

    private string RatingSummary
    {
        get
        {
            var total = ratingCounts.Sum();

            if (total == 0)
                return $"No visible reviews of {RatingScopeText} yet.";

            var average = Enumerable.Range(1, 5).Sum(stars => stars * ratingCounts[stars - 1]) / (double)total;

            return $"{Plural(total, "visible review", "visible reviews")} of {RatingScopeText}, {LearningDisplay.Rating(average)} out of 5 on average. Hidden reviews don't count.";
        }
    }

    private string ReviewsUrl => $"{TeachLayout.Base}/reviews";

    [Inject] private LearningState State { get; set; } = default!;

    /// <summary>The newest questions with no answer from the instructor yet.</summary>
    private List<LearningQuestionThread> Waiting =>
        State.InstructorQuestions.Where(thread => thread.Answers.All(answer => !answer.IsInstructor)).Take(ListSize).ToList();

    #endregion

    #region Nested Types

    /// <summary>A course with learners and how far they got.</summary>
    private sealed record CourseFunnel(LearningCourse Course, LearningFunnel Funnel);

    #endregion
}
