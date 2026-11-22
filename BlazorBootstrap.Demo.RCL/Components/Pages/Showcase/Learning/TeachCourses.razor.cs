using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// The signed-in instructor's courses, published and draft, as cards: the title (a link to its editor), Published or Draft
/// in words, the category and level, the learners, the published lessons of all lessons, and the average rating with the
/// count of visible reviews. Each card offers Edit, "View in school", Publish or Unpublish, and Delete. Publishing a course
/// without a published lesson is explained; unpublishing asks first and names the effect (it leaves the catalog and takes
/// no new enrollments; its learners keep access). A course with learners can't be deleted, which the card says up front;
/// deleting another one asks first, and a refusal (certificates) says why. Every <see cref="LearningResult" /> maps to a
/// message in the status region.
/// </summary>
public partial class TeachCourses : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<int, ElementReference> deleteButtons = new();

    private readonly Dictionary<int, ElementReference> publishButtons = new();

    private readonly StatusMessage status = new();

    /// <summary>Set by "Delete course"; the delete runs once the dialog is hidden, so nothing changes under an open dialog.</summary>
    private bool confirmDelete;

    /// <summary>Set by "Unpublish"; the change runs once the dialog is hidden.</summary>
    private bool confirmUnpublish;

    private Modal deleteModal = default!;

    /// <summary>The element to focus after the next render, read then (it may only exist after that render).</summary>
    private Func<ElementReference?>? focusAfterRender;

    /// <summary>The course a dialog is about.</summary>
    private LearningCourse? pending;

    private ElementReference statusRegion;

    private Modal unpublishModal = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusAfterRender is { } target)
        {
            focusAfterRender = null;
            await LearningDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string CountText(IReadOnlyList<LearningCourse> courses)
    {
        var published = courses.Count(course => course.IsPublished);
        var total = courses.Count == 1 ? "1 course" : string.Create(CultureInfo.InvariantCulture, $"{courses.Count} courses");

        return string.Create(CultureInfo.InvariantCulture, $"{total}: {published} published, {courses.Count - published} draft.");
    }

    /// <summary>Says why a course with learners can't be deleted, and what to do instead: "… while 3 learners are enrolled …".</summary>
    private static string DeleteBlockedDetail(LearningCourse course, int learners) =>
        (learners == 1 ? "1 learner is" : string.Create(CultureInfo.InvariantCulture, $"{learners} learners are"))
        + " enrolled: they keep the course, their progress, and their certificates. "
        + (course.IsPublished ? "Unpublish it instead to take it out of the catalog." : "It's already out of the catalog and closed to new enrollments.");

    /// <summary>The note under a course with learners, next to its unavailable Delete button.</summary>
    private static string DeleteUnavailableText(LearningCourse course, int learners) => $"Delete isn't available while {DeleteBlockedDetail(course, learners)}";

    /// <summary>Says who keeps access when a course is unpublished.</summary>
    private static string KeepAccessText(int learners) => learners switch
    {
        0 => "No learners are enrolled, so nobody loses access.",
        1 => "Its 1 learner keeps access to the course, their progress, and any certificate.",
        _ => string.Create(CultureInfo.InvariantCulture, $"Its {learners} learners keep access to the course, their progress, and their certificates."),
    };

    private static string LearnersText(int count) => count switch
    {
        0 => "No learners yet",
        1 => "1 learner",
        _ => string.Create(CultureInfo.InvariantCulture, $"{count} learners"),
    };

    private static string PublishedLessonsText(int published, int total) => total switch
    {
        0 => "No lessons yet",
        1 => published == 1 ? "1 of 1 lesson published" : "0 of 1 lesson published",
        _ => string.Create(CultureInfo.InvariantCulture, $"{published} of {total} lessons published"),
    };

    private static string ResultText(LearningResult result, LearningCourse course) => result switch
    {
        LearningResult.NotFound => $"\"{course.Title}\" isn't one of your courses any more, so nothing changed.",
        LearningResult.NotAllowed => "Sign in as an instructor to do that. Nothing changed.",
        _ => "That didn't work, and nothing changed. Try again.",
    };

    private static string ReviewsText(int count) => count == 1 ? "1 review" : string.Create(CultureInfo.InvariantCulture, $"{count} reviews");

    private async Task AskDeleteAsync(LearningCourse course)
    {
        pending = course;
        confirmDelete = false;

        await deleteModal.ShowAsync();
    }

    private async Task ConfirmDeleteAsync()
    {
        confirmDelete = true;

        await deleteModal.HideAsync();
    }

    private async Task ConfirmUnpublishAsync()
    {
        confirmUnpublish = true;

        await unpublishModal.HideAsync();
    }

    /// <summary>Says what a delete takes with it: the modules and lessons, the reviews, and the learners' questions.</summary>
    private string DeleteConsequenceText(LearningCourse course)
    {
        var lessons = course.Lessons.Count;
        var reviews = State.InstructorReviews.Count(review => review.CourseId == course.Id);
        var questions = State.InstructorQuestions.Count(thread => thread.CourseId == course.Id);
        var content = string.Create(CultureInfo.InvariantCulture,
            $"{(course.Modules.Count == 1 ? "1 module" : $"{course.Modules.Count} modules")} and {(lessons == 1 ? "1 lesson" : $"{lessons} lessons")}");
        var extras = new List<string>(2);

        if (reviews > 0)
            extras.Add(reviews == 1 ? "1 review" : string.Create(CultureInfo.InvariantCulture, $"{reviews} reviews"));

        if (questions > 0)
            extras.Add(questions == 1 ? "1 learner question" : string.Create(CultureInfo.InvariantCulture, $"{questions} learner questions"));

        return extras.Count == 0
            ? $"The course is removed with its {content}."
            : $"The course is removed with its {content}, and its {string.Join(" and ", extras)} go with it.";
    }

    /// <summary>A course with learners can't be deleted: the button says why instead of opening a dialog.</summary>
    private void ExplainDelete(LearningCourse course, int learners) =>
        status.Set($"\"{course.Title}\" can't be deleted while {DeleteBlockedDetail(course, learners)}", isError: true);

    private int LearnerCount(int courseId) => State.Roster.Count(row => row.Course.Id == courseId);

    /// <summary>
    /// Runs a confirmed delete once the dialog is hidden; focus goes to the status message (the card is gone), or back to
    /// the Delete button after Cancel or a refusal.
    /// </summary>
    private async Task OnDeleteHiddenAsync()
    {
        var course = pending;
        pending = null;
        var opener = course is not null && deleteButtons.TryGetValue(course.Id, out var button) ? button : (ElementReference?)null;

        if (!confirmDelete || course is null)
        {
            await LearningDisplay.FocusAsync(opener);

            return;
        }

        confirmDelete = false;
        var result = State.DeleteCourse(course.Id);

        switch (result)
        {
            case LearningResult.Ok:
                deleteButtons.Remove(course.Id);
                publishButtons.Remove(course.Id);
                status.Set($"\"{course.Title}\" is deleted.");
                focusAfterRender = () => statusRegion;
                break;

            case LearningResult.WrongState:
                status.Set($"\"{course.Title}\" can't be deleted: it has learners or certificates, which need the course. "
                    + (course.IsPublished ? "Unpublish it instead to take it out of the catalog." : "It's already out of the catalog."), isError: true);
                focusAfterRender = () => statusRegion;
                break;

            default:
                status.Set(ResultText(result, course), isError: true);
                focusAfterRender = () => statusRegion;
                break;
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Runs a confirmed unpublish once the dialog is hidden, then focuses the status message; Cancel returns focus to the button.</summary>
    private async Task OnUnpublishHiddenAsync()
    {
        var course = pending;
        pending = null;
        var opener = course is not null && publishButtons.TryGetValue(course.Id, out var button) ? button : (ElementReference?)null;

        if (!confirmUnpublish || course is null)
        {
            await LearningDisplay.FocusAsync(opener);

            return;
        }

        confirmUnpublish = false;
        var result = State.SetCoursePublished(course.Id, false);

        status.Set(result switch
        {
            LearningResult.Ok => $"\"{course.Title}\" is unpublished: it's out of the catalog and takes no new enrollments. {KeepAccessText(LearnerCount(course.Id))}",
            LearningResult.NoChange => $"\"{course.Title}\" was already unpublished.",
            _ => ResultText(result, course),
        }, isError: result is not (LearningResult.Ok or LearningResult.NoChange));
        focusAfterRender = () => statusRegion;
    }

    /// <summary>Unpublishing asks first; publishing happens at once (a course needs a published lesson) and is reported.</summary>
    private async Task TogglePublishedAsync(LearningCourse course)
    {
        if (course.IsPublished)
        {
            pending = course;
            confirmUnpublish = false;

            await unpublishModal.ShowAsync();

            return;
        }

        var result = State.SetCoursePublished(course.Id, true);

        status.Set(result switch
        {
            LearningResult.Ok => $"\"{course.Title}\" is published: it's in the catalog, and learners can enroll.",
            LearningResult.NoChange => $"\"{course.Title}\" was already published.",
            LearningResult.WrongState => $"\"{course.Title}\" can't be published yet: it has no published lesson. Open the course editor, publish at least one lesson, then publish the course.",
            _ => ResultText(result, course),
        }, isError: result is not (LearningResult.Ok or LearningResult.NoChange));

        // Published courses list first, so the card may move, and a moved element loses focus: keep it on the course's button.
        focusAfterRender = () => publishButtons.TryGetValue(course.Id, out var button) ? button : null;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the route of the course list.</summary>
    public static string ListUrl => $"{TeachLayout.Base}/courses";

    [Inject] private LearningState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>A status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private sealed class StatusMessage
    {
        #region Fields and Constants

        private int count;

        private string? text;

        #endregion

        #region Methods

        #region Public Methods

        public void Set(string message, bool isError = false)
        {
            count = message == text ? count + 1 : 1;
            text = message;
            IsError = isError;
            Message = count == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({count})");
        }

        #endregion

        #endregion

        #region Properties, Indexers

        public bool IsError { get; private set; }

        public string? Message { get; private set; }

        #endregion
    }

    #endregion
}
