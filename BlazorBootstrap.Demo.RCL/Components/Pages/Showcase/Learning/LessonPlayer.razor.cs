using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// Lesson player: the lesson list with progress, the lesson body (seeded Markdown), a "Mark as complete" button that
/// advances the course progress bar, previous and next links, and the course handout in a PdfViewer. A lesson that is
/// not part of the course renders the not-found screen; a learner who has not enrolled is offered to enroll.
/// </summary>
public partial class LessonPlayer : ComponentBase, IDisposable
{
    #region Fields and Constants

    private bool focusHeading;

    private string? handoutError;

    private ElementReference heading;

    private int? loadedLessonId;

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
        // A new lesson starts without the previous lesson's status text.
        if (loadedLessonId == LessonId)
            return;

        loadedLessonId = LessonId;
        statusMessage = null;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void Enroll()
    {
        if (Course is null)
            return;

        State.Enroll(Course.Id);
        statusMessage = $"You're enrolled in {Course.Title}.";
        focusHeading = true;
    }

    private void OnHandoutError(string message) => handoutError = "The handout could not be shown here. Use the link above to open it.";

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void ToggleComplete()
    {
        if (Course is null || Lesson is null)
            return;

        var isComplete = !IsLessonComplete;
        State.SetLessonComplete(Course.Id, Lesson.Id, isComplete);

        var done = State.CompletedCount(Course.Id);
        var total = Course.Lessons.Count;

        statusMessage = !isComplete
            ? $"Marked as not complete. {done} of {total} lessons complete."
            : done == total
                ? $"Course complete! All {total} lessons done. Your certificate is ready."
                : $"Lesson complete. {done} of {total} lessons complete.";
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

    [Inject] private LearningState State { get; set; } = default!;

    #endregion
}
