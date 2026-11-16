using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// The instructor's Q&amp;A inbox: learners' questions on her courses, newest first, filtered by status (Needs an answer,
/// the default; Answered, meaning it has an instructor answer; All) and course, with counts in each filter. Each thread
/// shows its course and lesson (links to the lesson as learners see it and to the lesson editor), the learner, when, the
/// plain text (kept in <c>pre-wrap</c>), and the answers (instructor answers marked "Instructor", her own "You"). She
/// answers through a form with a character counter; a posted answer is announced, and when the question leaves the
/// "Needs an answer" list that is said too and focus moves to the status message. Her own answers can be deleted after a
/// confirmation that names the consequence.
/// </summary>
public partial class TeachQuestions : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int ExcerptLength = 160;

    /// <summary>The status filters, in the order they are offered.</summary>
    private static readonly InboxFilter[] filters = { InboxFilter.NeedsAnswer, InboxFilter.Answered, InboxFilter.All };

    /// <summary>The answer typed for each question, by question number.</summary>
    private readonly Dictionary<int, string> answerDrafts = new();

    /// <summary>The message under each answer field, by question number.</summary>
    private readonly Dictionary<int, string?> answerErrors = new();

    /// <summary>The answer field of each question, for moving focus to it.</summary>
    private readonly Dictionary<int, ElementReference> answerInputs = new();

    private Button? cancelDeleteButton;

    /// <summary>The course shown (0 for every course).</summary>
    private int courseFilter;

    /// <summary>The Delete button of each of her answers, to return focus after Cancel.</summary>
    private readonly Dictionary<int, ElementReference> deleteButtons = new();

    /// <summary>Whether the dialog closed after a delete was tried, so focus goes to the status message.</summary>
    private bool deleteDone;

    private bool deleting;

    private Modal deleteModal = default!;

    /// <summary>The element to focus after the next render (the render that shows an error or removes the question).</summary>
    private Func<ElementReference?>? focusAfterRender;

    /// <summary>The answer waiting for the delete confirmation: its question and answer numbers.</summary>
    private (int ThreadId, int AnswerId)? pendingDelete;

    private InboxFilter statusFilter = InboxFilter.NeedsAnswer;

    private int statusCount;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

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

    private static string? AnswerProblem(string text) =>
        text.Trim().Length == 0 ? "Write your answer first."
        : text.Trim().Length > LearningState.MaxQuestionLength ? $"Shorten your answer to {MaxLengthText} characters or fewer (now {text.Trim().Length.ToString("#,0", CultureInfo.InvariantCulture)})."
        : null;

    private static string Excerpt(string text) => text.Length <= ExcerptLength ? text : $"{text[..ExcerptLength].TrimEnd()}…";

    private static string FilterLabel(InboxFilter filter) => filter switch
    {
        InboxFilter.NeedsAnswer => "Needs an answer",
        InboxFilter.Answered => "Answered",
        _ => "All",
    };

    /// <summary>Whether a question has an answer from the instructor ("Answered").</summary>
    private static bool IsAnswered(LearningQuestionThread thread) => thread.Answers.Any(answer => answer.IsInstructor);

    private static string IsoTime(DateTime value) => value.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);

    private static string LessonEditUrl(int courseId, int lessonId) =>
        string.Create(CultureInfo.InvariantCulture, $"{TeachLayout.Base}/courses/{courseId}/lessons/{lessonId}");

    private static bool Matches(LearningQuestionThread thread, InboxFilter filter) => filter switch
    {
        InboxFilter.NeedsAnswer => !IsAnswered(thread),
        InboxFilter.Answered => IsAnswered(thread),
        _ => true,
    };

    private static string ShowingText(int count) =>
        count == 1 ? "1 question, newest first." : string.Create(CultureInfo.InvariantCulture, $"{count} questions, newest first.");

    /// <summary>Formats a time like "Nov 22, 2026 at 9:00 AM".</summary>
    private static string When(DateTime value) => $"{LearningDisplay.Date(value)} at {value.ToString("h:mm tt", CultureInfo.InvariantCulture)}";

    /// <summary>Says how long ago something happened on the site's clock, like "3 hours ago" or "2 days ago".</summary>
    private string Ago(DateTime value)
    {
        var elapsed = State.Clock.Now - value;

        return elapsed.TotalHours < 1 ? "less than an hour ago"
            : elapsed.TotalDays < 1 ? Plural((int)elapsed.TotalHours, "hour") + " ago"
            : Plural((int)elapsed.TotalDays, "day") + " ago";

        static string Plural(int count, string unit) => string.Create(CultureInfo.InvariantCulture, $"{count} {unit}{(count == 1 ? string.Empty : "s")}");
    }

    private async Task AskDeleteAsync(LearningQuestionThread thread, LearningAnswer answer)
    {
        pendingDelete = (thread.Id, answer.Id);
        deleteDone = false;

        await deleteModal.ShowAsync();
    }

    private int CharacterCount(int threadId) => answerDrafts.GetValueOrDefault(threadId)?.Length ?? 0;

    private async Task ConfirmDeleteAsync()
    {
        // A second press while the dialog closes would try the delete twice.
        if (pendingDelete is not { } pending || deleting)
            return;

        deleting = true;

        var thread = State.InstructorQuestions.FirstOrDefault(item => item.Id == pending.ThreadId);
        var wasAnswered = thread is not null && IsAnswered(thread);
        var result = State.DeleteAnswer(pending.ThreadId, pending.AnswerId);

        if (result == LearningResult.Ok)
        {
            deleteButtons.Remove(pending.AnswerId);

            var reopened = wasAnswered && thread is not null && !IsAnswered(thread);
            var message = "Your answer was deleted. Learners no longer see it.";

            if (reopened)
                message += statusFilter == InboxFilter.Answered
                    ? $" {thread!.Author}'s question needs an answer again, so it moved to Needs an answer."
                    : $" {thread!.Author}'s question needs an answer again.";

            SetStatus(message, isError: false);
        }
        else
        {
            SetStatus(result switch
            {
                LearningResult.NotFound => "That answer was already deleted, or its question is gone. Nothing else changed.",
                LearningResult.NotAllowed => "Sign in with the instructor account to delete your answers. Nothing was deleted.",
                _ => "That didn't work, and nothing was deleted. Try again.",
            }, isError: true);
        }

        deleteDone = true;

        await deleteModal.HideAsync();
    }

    /// <summary>How many questions the course option holds under the status filter (0 for every course).</summary>
    private int CourseCount(int courseId) =>
        State.InstructorQuestions.Count(thread => (courseId == 0 || thread.CourseId == courseId) && Matches(thread, statusFilter));

    /// <summary>Says what deleting the pending answer does: learners stop seeing it, and the question may need an answer again.</summary>
    private string DeleteConsequence()
    {
        if (pendingDelete is not { } pending || State.InstructorQuestions.FirstOrDefault(item => item.Id == pending.ThreadId) is not { } thread)
            return "This can't be undone.";

        var lastInstructorAnswer = thread.Answers.Count(answer => answer.IsInstructor && answer.Id != pending.AnswerId) == 0;
        var reopens = lastInstructorAnswer ? $" {thread.Author}'s question goes back to Needs an answer." : string.Empty;

        return $"Learners no longer see it on the lesson.{reopens} This can't be undone.";
    }

    private bool IsMine(LearningAnswer answer) => answer.AccountId is not null && answer.AccountId == State.Identity.Current?.Id;

    private void OnAnswerInput(int threadId, ChangeEventArgs args)
    {
        answerDrafts[threadId] = args.Value?.ToString() ?? string.Empty;

        if (answerErrors.GetValueOrDefault(threadId) is not null && AnswerProblem(answerDrafts[threadId]) is null)
            answerErrors.Remove(threadId);
    }

    private void OnCourseChanged(ChangeEventArgs args) =>
        courseFilter = int.TryParse(args.Value?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;

    /// <summary>
    /// The dialog opens from code, so Bootstrap does not return focus: after a delete, focus goes to the status message
    /// (the answer and its button are gone); after Cancel, back to the Delete button that opened it.
    /// </summary>
    private async Task OnDeleteHiddenAsync()
    {
        deleting = false;
        var pending = pendingDelete;
        pendingDelete = null;

        if (deleteDone)
        {
            deleteDone = false;
            focusAfterRender = () => statusRegion;
        }
        else if (pending is { } opened && deleteButtons.TryGetValue(opened.AnswerId, out var opener))
        {
            await LearningDisplay.FocusAsync(opener);
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>
    /// Posts an answer. When the question then leaves the "Needs an answer" list, that is said and focus moves to the status
    /// message, because the form is gone.
    /// </summary>
    private void PostAnswer(LearningQuestionThread thread)
    {
        var text = answerDrafts.GetValueOrDefault(thread.Id) ?? string.Empty;

        if (AnswerProblem(text) is { } problem)
        {
            answerErrors[thread.Id] = problem;
            focusAfterRender = () => answerInputs.TryGetValue(thread.Id, out var input) ? input : null;

            return;
        }

        var leaves = statusFilter == InboxFilter.NeedsAnswer && !IsAnswered(thread);
        var result = State.AnswerQuestion(thread.Id, text, out _);

        switch (result)
        {
            case LearningResult.Ok:
                answerDrafts.Remove(thread.Id);
                answerErrors.Remove(thread.Id);

                if (leaves)
                {
                    SetStatus($"Your answer to {thread.Author}'s question is posted. The question is answered now, so it moved to Answered.", isError: false);
                    focusAfterRender = () => statusRegion;
                }
                else
                {
                    SetStatus($"Your answer to {thread.Author}'s question is posted. Learners see it on the lesson, marked Instructor.", isError: false);
                }

                break;

            case LearningResult.Invalid:
                answerErrors[thread.Id] = $"Write an answer of 1 to {MaxLengthText} characters. If it still won't post, this question has as many answers as it can hold, or this demo tab is full; reset the demo data.";
                focusAfterRender = () => answerInputs.TryGetValue(thread.Id, out var input) ? input : null;
                break;

            default:
                SetStatus(result switch
                {
                    LearningResult.NotFound => $"{thread.Author}'s question is gone: the learner deleted it, or its lesson was deleted. Nothing was posted.",
                    LearningResult.NotAllowed => "Sign in with the instructor account to answer questions. Nothing was posted.",
                    _ => "That didn't work, and nothing was posted. Try again.",
                }, isError: true);
                focusAfterRender = () => statusRegion;
                break;
        }
    }

    private void SetStatusFilter(InboxFilter filter) => statusFilter = filter;

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message, bool isError)
    {
        statusIsError = isError;
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>How many questions the status filter holds in the chosen course.</summary>
    private int StatusCount(InboxFilter filter) =>
        State.InstructorQuestions.Count(thread => (courseFilter == 0 || thread.CourseId == courseFilter) && Matches(thread, filter));

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>The chosen course's title, or <see langword="null" /> for every course.</summary>
    private string? CourseName => courseFilter == 0 ? null : State.MyCourses.FirstOrDefault(course => course.Id == courseFilter)?.Title;

    private string EmptyText => statusFilter switch
    {
        InboxFilter.NeedsAnswer => CourseName is { } name ? $"Every question about {name} has your answer." : "Every question on your courses has your answer. New ones show here.",
        InboxFilter.Answered => CourseName is { } name ? $"You haven't answered a question about {name} yet." : "You haven't answered a question yet.",
        _ => CourseName is { } name ? $"No learner has asked about {name} yet." : "No learner has asked about your courses yet.",
    };

    private string EmptyTitle => statusFilter switch
    {
        InboxFilter.NeedsAnswer => "You're all caught up",
        InboxFilter.Answered => "No answered questions",
        _ => "No questions yet",
    };

    private string LeadText => State.UnansweredCount switch
    {
        0 => "Learners' questions on your courses, newest first. None is waiting for an answer.",
        1 => "Learners' questions on your courses, newest first. 1 is waiting for your answer.",
        var count => string.Create(CultureInfo.InvariantCulture, $"Learners' questions on your courses, newest first. {count} are waiting for your answer."),
    };

    private static string MaxLengthText => LearningState.MaxQuestionLength.ToString("#,0", CultureInfo.InvariantCulture);

    /// <summary>The answer waiting for the delete confirmation, or <see langword="null" />.</summary>
    private LearningAnswer? PendingAnswer =>
        pendingDelete is { } pending
            ? State.InstructorQuestions.FirstOrDefault(thread => thread.Id == pending.ThreadId)?.Answers.FirstOrDefault(answer => answer.Id == pending.AnswerId)
            : null;

    [Inject] private LearningState State { get; set; } = default!;

    /// <summary>The questions shown: the instructor's, newest first, through the status and course filters.</summary>
    private IReadOnlyList<LearningQuestionThread> Threads =>
        State.InstructorQuestions.Where(thread => (courseFilter == 0 || thread.CourseId == courseFilter) && Matches(thread, statusFilter)).ToList();

    #endregion

    #region Nested Types

    /// <summary>Which questions the inbox shows.</summary>
    private enum InboxFilter
    {
        /// <summary>Questions without an instructor answer.</summary>
        NeedsAnswer,

        /// <summary>Questions with an instructor answer.</summary>
        Answered,

        /// <summary>Every question.</summary>
        All,
    }

    #endregion
}
