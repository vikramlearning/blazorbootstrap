using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// "Questions about this lesson": the lesson's questions newest first, each with its author, time, plain text (kept in
/// <c>pre-wrap</c>), and answers (the instructor's marked "Instructor" in text). Enrolled learners ask through a form with a
/// character counter (up to <see cref="LearningState.MaxQuestionLength" />); a posted question is announced and shows at
/// the top. The learner's own question can be deleted while it has no answers, after a confirmation that names the
/// consequence. Others are told that asking needs an enrollment. Key it by lesson, so another lesson starts fresh.
/// </summary>
public partial class LearningLessonQuestions : ComponentBase
{
    #region Fields and Constants

    private const int ExcerptLength = 160;

    private string? askError;

    private Button? cancelDeleteButton;

    /// <summary>The Delete button of each of the learner's unanswered questions, to return focus after Cancel.</summary>
    private readonly Dictionary<int, ElementReference> deleteButtons = new();

    /// <summary>Whether the dialog closed after a delete was tried, so focus goes to the status message.</summary>
    private bool deleteDone;

    private bool deleting;

    private Modal deleteModal = default!;

    private bool focusQuestion;

    private bool focusStatus;

    private int? pendingDeleteId;

    /// <summary>The question posted from this form, marked as new at the top of the list.</summary>
    private int? postedId;

    private ElementReference questionInput;

    private string? questionText;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusQuestion)
        {
            focusQuestion = false;
            await LearningDisplay.FocusAsync(questionInput);
        }
        else if (focusStatus)
        {
            focusStatus = false;
            await LearningDisplay.FocusAsync(statusRegion);
        }
    }

    #endregion

    #region Private Methods

    private static string Excerpt(string text) => text.Length <= ExcerptLength ? text : $"{text[..ExcerptLength].TrimEnd()}…";

    private static string IsoTime(DateTime value) => value.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);

    /// <summary>Formats a time like "Nov 22, 2026 at 9:00 AM".</summary>
    private static string When(DateTime value) => $"{LearningDisplay.Date(value)} at {value.ToString("h:mm tt", CultureInfo.InvariantCulture)}";

    private void Ask()
    {
        var text = questionText?.Trim() ?? string.Empty;

        if (text.Length == 0)
        {
            askError = "Write your question first.";
            focusQuestion = true;
            return;
        }

        if (text.Length > LearningState.MaxQuestionLength)
        {
            askError = $"Shorten your question to {MaxLengthText} characters or fewer.";
            focusQuestion = true;
            return;
        }

        var result = State.AskQuestion(Course.Id, Lesson.Id, text, out var thread);

        if (result == LearningResult.Ok && thread is not null)
        {
            askError = null;
            questionText = string.Empty;
            postedId = thread.Id;
            statusIsError = false;
            statusMessage = $"Your question is posted at the top of the list. {Course.Instructor} usually answers within a day.";
            return;
        }

        if (result == LearningResult.Invalid)
        {
            askError = $"Write a question of 1 to {MaxLengthText} characters. If it still won't post, this demo tab holds as many questions as it can; reset the demo data.";
            focusQuestion = true;
            return;
        }

        statusIsError = true;
        statusMessage = result switch
        {
            LearningResult.NotAllowed => "Sign in with a learner account to ask a question. Nothing was posted.",
            LearningResult.NotEnrolled => $"Enroll in {Course.Title} to ask about its lessons. Nothing was posted.",
            LearningResult.NotFound => "This lesson isn't part of the course any more. Nothing was posted.",
            _ => "That didn't work, and nothing was posted. Try again.",
        };
        focusStatus = true;
    }

    private async Task AskDeleteAsync(LearningQuestionThread thread)
    {
        pendingDeleteId = thread.Id;
        deleteDone = false;

        await deleteModal.ShowAsync();
    }

    private async Task ConfirmDeleteAsync()
    {
        // A second press while the dialog closes would try the delete twice.
        if (pendingDeleteId is not int threadId || deleting)
            return;

        deleting = true;

        var result = State.DeleteQuestion(threadId);

        statusIsError = result != LearningResult.Ok;
        statusMessage = result switch
        {
            LearningResult.Ok => "Your question was deleted.",
            LearningResult.NotFound => "That question was already deleted.",
            LearningResult.WrongState => "Your question has an answer now, so it stays for other learners to read. Nothing was deleted.",
            LearningResult.NotAllowed => "Sign in with the learner account that asked it to delete a question. Nothing was deleted.",
            _ => "That didn't work, and nothing was deleted. Try again.",
        };

        if (result == LearningResult.Ok)
        {
            deleteButtons.Remove(threadId);

            if (postedId == threadId)
                postedId = null;
        }

        deleteDone = true;

        await deleteModal.HideAsync();
    }

    private bool IsMine(LearningQuestionThread thread) => thread.AccountId is not null && thread.AccountId == State.Identity.Current?.Id;

    /// <summary>
    /// The dialog opens from code, so Bootstrap does not return focus: after a delete, focus goes to the status message
    /// (the question and its button are gone); after Cancel, back to the Delete button that opened it.
    /// </summary>
    private async Task OnDeleteHiddenAsync()
    {
        deleting = false;
        var threadId = pendingDeleteId;
        pendingDeleteId = null;

        if (deleteDone)
        {
            deleteDone = false;
            StateHasChanged();
            await LearningDisplay.FocusAsync(statusRegion);
        }
        else if (threadId is int id && deleteButtons.TryGetValue(id, out var opener))
        {
            await LearningDisplay.FocusAsync(opener);
        }
    }

    /// <summary>Focus starts on Cancel, the choice that keeps the question.</summary>
    private Task OnDeleteShownAsync() => LearningDisplay.FocusAsync(cancelDeleteButton?.Element);

    #endregion

    #endregion

    #region Properties, Indexers

    private int CharacterCount => questionText?.Length ?? 0;

    private string CountText => Threads.Count switch
    {
        0 => "Ask the instructor about anything in this lesson.",
        1 => "1 question, newest first.",
        _ => string.Create(CultureInfo.InvariantCulture, $"{Threads.Count} questions, newest first."),
    };

    /// <summary>Gets or sets the course the lesson belongs to.</summary>
    [Parameter, EditorRequired] public LearningCourse Course { get; set; } = default!;

    private string IdPrefix => string.Create(CultureInfo.InvariantCulture, $"qp-qa-{Lesson.Id}");

    /// <summary>Gets or sets the lesson whose questions are shown.</summary>
    [Parameter, EditorRequired] public LearningLesson Lesson { get; set; } = default!;

    private static string MaxLengthText => LearningState.MaxQuestionLength.ToString("#,0", CultureInfo.InvariantCulture);

    private LearningQuestionThread? PendingDelete => pendingDeleteId is int id ? Threads.FirstOrDefault(thread => thread.Id == id) : null;

    [Inject] private LearningState State { get; set; } = default!;

    private IReadOnlyList<LearningQuestionThread> Threads => State.QuestionsFor(Lesson.Id);

    #endregion
}
