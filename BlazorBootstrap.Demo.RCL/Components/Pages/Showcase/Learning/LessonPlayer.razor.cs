using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// Lesson player, for learners: the course outline (modules and lessons, each with its completion in text and the current
/// lesson marked <c>aria-current</c>), the lesson body (seeded Markdown), and for a video or reading lesson a
/// "Mark as complete" / "Mark as not complete" toggle; a quiz lesson shows <see cref="LearningQuiz" />, which completes by
/// passing. Previous and next links, the lesson's questions (<see cref="LearningLessonQuestions" />), and the course
/// handout in a PdfViewer follow. Finishing the last lesson here shows a one-time banner with the certificate and a link to
/// review the course. A learner who isn't enrolled is offered to enroll; an unknown course or lesson shows the not-found screen.
/// </summary>
public partial class LessonPlayer : ComponentBase, IDisposable
{
    #region Fields and Constants

    private bool completionIsError;

    private string? completionMessage;

    private bool focusHeading;

    private string? handoutError;

    private ElementReference heading;

    private int? loadedCourseId;

    private int? loadedLessonId;

    /// <summary>The lesson last reported to <see cref="LearningState.OpenedLesson" />, so it's recorded once per visit.</summary>
    private int? openedLessonId;

    /// <summary>Whether the course was finished on this page, which shows the one-time banner.</summary>
    private bool showFinished;

    private bool statusIsError;

    private string? statusMessage;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // "Enroll" is replaced by the lesson; focus goes to its heading instead of falling back to the page (WCAG 2.4.3).
        if (focusHeading)
        {
            focusHeading = false;
            await LearningDisplay.FocusAsync(heading);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        // A new lesson starts without the previous lesson's messages and banner.
        if (loadedLessonId != LessonId)
        {
            loadedLessonId = LessonId;
            statusMessage = null;
            statusIsError = false;
            completionMessage = null;
            completionIsError = false;
            showFinished = false;
        }

        // The handout viewer is keyed by course and reports a load error once, so its warning stays for the course.
        if (loadedCourseId != Id)
        {
            loadedCourseId = Id;
            handoutError = null;
        }

        RecordOpened();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string CertificateUrl(LearningCertificate certificate) => $"{LearningLayout.Base}/certificates/{Uri.EscapeDataString(certificate.Code)}";

    private void Enroll()
    {
        if (Course is not { } course)
            return;

        var result = State.Enroll(course.Id);

        statusIsError = result is not (LearningResult.Ok or LearningResult.NoChange);
        statusMessage = result switch
        {
            LearningResult.Ok => $"You're enrolled in {course.Title}. The lesson is below.",
            LearningResult.NoChange => $"You're already enrolled in {course.Title}. The lesson is below.",
            LearningResult.NotAllowed => "Sign in with a learner account to enroll. Nothing changed.",
            LearningResult.NotFound => "This course isn't in the catalog any more, so you can't enroll.",
            LearningResult.Invalid => "This demo tab holds as many enrollments as it can. Reset the demo data to enroll.",
            LearningResult.WrongState => $"{course.Title} isn't open for new enrollments right now, so you weren't enrolled.",
            _ => "That didn't work, and nothing changed. Try again.",
        };

        if (!statusIsError)
        {
            RecordOpened();
            focusHeading = true;
        }
    }

    /// <summary>A passed quiz finished the course: show the banner.</summary>
    private void OnCourseCompleted() => showFinished = true;

    private void OnHandoutError(string message) => handoutError = "The handout could not be shown here. Use the link above to open it.";

    /// <summary>Re-renders on state changes; a tab opened on a lesson records the visit once its snapshot is restored.</summary>
    private void OnStateChanged() => InvokeAsync(() =>
    {
        RecordOpened();
        StateHasChanged();
    });

    /// <summary>Returns a lesson's line in the outline: its kind, length, and (when enrolled) its completion, in text.</summary>
    private string OutlineMeta(LearningLesson lesson, bool done)
    {
        var meta = $"{LearningDisplay.KindLabel(lesson.Kind)} · {lesson.Minutes} min";

        return IsEnrolled ? $"{meta} · {(done ? "Complete" : "Not complete")}" : meta;
    }

    /// <summary>
    /// Tells the state the lesson was opened ("continue where you left off"), once per lesson and only after the tab's
    /// snapshot is restored, so the seed view before the restore isn't recorded.
    /// </summary>
    private void RecordOpened()
    {
        if (Session?.IsRestored != true || Course is not { } course || Lesson is null || openedLessonId == LessonId || !State.IsEnrolled(course.Id))
            return;

        openedLessonId = LessonId;
        State.OpenedLesson(course.Id);
    }

    private void ToggleComplete()
    {
        if (Course is not { } course || Lesson is not { } lesson)
            return;

        var isComplete = !IsLessonComplete;
        var wasCourseComplete = State.IsComplete(course.Id);
        var result = State.SetLessonComplete(course.Id, lesson.Id, isComplete);

        var done = State.CompletedCount(course.Id);
        var total = course.Lessons.Count;

        completionIsError = result is not (LearningResult.Ok or LearningResult.NoChange);
        completionMessage = result switch
        {
            LearningResult.Ok when !isComplete => $"Marked as not complete. {done} of {total} lessons complete.",
            LearningResult.Ok when !wasCourseComplete && State.IsComplete(course.Id) => $"Lesson complete. That was the last one: all {total} lessons are done.",
            LearningResult.Ok => $"Lesson complete. {done} of {total} lessons complete.",
            LearningResult.NoChange => isComplete ? "This lesson was already complete." : "This lesson was already not complete.",
            LearningResult.NotAllowed => "Sign in with a learner account to track your progress. Nothing changed.",
            LearningResult.NotEnrolled => "Enroll in the course to track your progress. Nothing changed.",
            LearningResult.NotFound => "This lesson isn't part of the course any more. Nothing changed.",
            LearningResult.WrongState => "Quizzes complete when you pass them. Nothing changed.",
            _ => "That didn't work, and nothing changed. Try again.",
        };

        if (result == LearningResult.Ok)
            showFinished = isComplete && !wasCourseComplete && State.IsComplete(course.Id);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Courses", Href = LearningLayout.Base },
        new() { Text = Course?.Title, Href = Course is null ? null : LearningDisplay.CourseUrl(Course.Id) },
        new() { Text = Lesson?.Title, IsCurrentPage = true },
    };

    private LearningCourse? Course => State.FindCourse(Id);

    private string HandoutUrl => Course is null ? string.Empty : Navigation.ToAbsoluteUri(Course.HandoutPath).ToString();

    /// <summary>Gets or sets the course ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    private bool IsEnrolled => Course is not null && State.IsEnrolled(Course.Id);

    private bool IsLessonComplete => Course is not null && Lesson is not null && State.IsLessonComplete(Course.Id, Lesson.Id);

    private LearningLesson? Lesson => Course?.Lessons.FirstOrDefault(lesson => lesson.Id == LessonId);

    /// <summary>Gets or sets the lesson ID from the route.</summary>
    [Parameter] public int LessonId { get; set; }

    private int LessonIndex => Course is null ? -1 : Course.Lessons.ToList().FindIndex(lesson => lesson.Id == LessonId);

    private int ModuleIndex => Course is null ? 0 : Math.Max(0, Course.Modules.ToList().FindIndex(module => module.Lessons.Any(lesson => lesson.Id == LessonId)));

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private LearningLesson? Next => Course is not null && LessonIndex >= 0 && LessonIndex < Course.Lessons.Count - 1 ? Course.Lessons[LessonIndex + 1] : null;

    private LearningLesson? Previous => Course is not null && LessonIndex > 0 ? Course.Lessons[LessonIndex - 1] : null;

    /// <summary>The course page's review section, where the learner writes or edits their review.</summary>
    private string ReviewUrl => Course is null ? LearningLayout.Base : $"{LearningDisplay.CourseUrl(Course.Id)}#your-review";

    [CascadingParameter] private ShowcaseSession? Session { get; set; }

    [Inject] private LearningState State { get; set; } = default!;

    #endregion
}
