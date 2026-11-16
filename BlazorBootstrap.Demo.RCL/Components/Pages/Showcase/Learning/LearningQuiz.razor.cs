using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// A quiz lesson's scored quiz. Each question is a <c>fieldset</c> with a <c>legend</c> and native radios. Submitting
/// needs every question answered (otherwise a message names the missing ones and focus goes to the first); the score is
/// then announced in a status region ("You got 2 of 3 right — passed"), and each question is marked Right or Wrong in text
/// with the correct option and the explanation. "Try again" clears the answers and focuses the first question. The best
/// score and number of attempts come from <see cref="LearningState.QuizResult" />; passing completes the lesson, and a
/// passed quiz can be retaken. Key it by lesson, so another lesson starts fresh.
/// </summary>
public partial class LearningQuiz : ComponentBase
{
    #region Fields and Constants

    private int?[] answers = Array.Empty<int?>();

    private string? errorMessage;

    private ElementReference[] firstOptions = Array.Empty<ElementReference>();

    /// <summary>The question whose first option takes focus after the next render, if any.</summary>
    private int? focusQuestion;

    private bool focusResult;

    /// <summary>The submission shown, or <see langword="null" /> while answering.</summary>
    private QuizOutcome? result;

    private ElementReference resultRegion;

    /// <summary>Whether a submission was refused for missing answers, which marks the unanswered questions.</summary>
    private bool showMissing;

    private bool submitting;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusResult)
        {
            focusResult = false;
            await LearningDisplay.FocusAsync(resultRegion);
        }
        else if (focusQuestion is int question && question < firstOptions.Length)
        {
            focusQuestion = null;
            await LearningDisplay.FocusAsync(firstOptions[question]);
        }
    }

    protected override void OnParametersSet()
    {
        if (answers.Length == Lesson.Questions.Count)
            return;

        answers = new int?[Lesson.Questions.Count];
        firstOptions = new ElementReference[Lesson.Questions.Count];
    }

    #endregion

    #region Private Methods

    private static string AttemptsText(int attempts) =>
        attempts == 1 ? "1 attempt" : string.Create(CultureInfo.InvariantCulture, $"{attempts} attempts");

    /// <summary>Joins question numbers like "1", "1 and 3", or "1, 2, and 3".</summary>
    private static string NumberList(IReadOnlyList<int> numbers) => numbers.Count switch
    {
        1 => numbers[0].ToString(CultureInfo.InvariantCulture),
        2 => string.Create(CultureInfo.InvariantCulture, $"{numbers[0]} and {numbers[1]}"),
        _ => string.Create(CultureInfo.InvariantCulture, $"{string.Join(", ", numbers.Take(numbers.Count - 1))}, and {numbers[^1]}"),
    };

    private void Choose(int question, int option)
    {
        answers[question] = option;

        // The message about missing answers updates as they're filled in.
        if (showMissing && answers.All(answer => answer is not null))
        {
            showMissing = false;
            errorMessage = null;
        }
    }

    private string QuestionId(int question) => string.Create(CultureInfo.InvariantCulture, $"qp-quiz-{Lesson.Id}-q{question}");

    private string ResultSentence(QuizOutcome outcome)
    {
        var total = Lesson.Questions.Count;

        return outcome.Passed
            ? string.Create(CultureInfo.InvariantCulture, $"You got {outcome.Score} of {total} right — passed.")
            : string.Create(CultureInfo.InvariantCulture, $"You got {outcome.Score} of {total} right — you need {Lesson.PassMark} to pass.");
    }

    private async Task SubmitAsync()
    {
        // A second press while the first is handled would count an extra attempt.
        if (submitting)
            return;

        var missing = answers.Select((answer, index) => (answer, number: index + 1)).Where(pair => pair.answer is null).Select(pair => pair.number).ToList();

        if (missing.Count > 0)
        {
            showMissing = true;
            errorMessage = missing.Count == 1
                ? $"Answer question {NumberList(missing)} before you submit."
                : $"Answer questions {NumberList(missing)} before you submit.";
            focusQuestion = missing[0] - 1;
            return;
        }

        submitting = true;

        try
        {
            var chosen = answers.Select(answer => answer!.Value).ToList();
            var wasCourseComplete = State.IsComplete(Course.Id);
            var wasPassed = IsPassed;
            var outcome = State.SubmitQuiz(Course.Id, Lesson.Id, chosen, out var score);

            if (outcome != LearningResult.Ok)
            {
                errorMessage = outcome switch
                {
                    LearningResult.NotAllowed => "Sign in with a learner account to take the quiz. Nothing was saved.",
                    LearningResult.NotEnrolled => "Enroll in the course to take its quizzes. Nothing was saved.",
                    LearningResult.NotFound => "This quiz isn't part of the course any more. Nothing was saved.",
                    LearningResult.Invalid => "Choose one of the options for every question, then submit again.",
                    _ => "That didn't work, and nothing was saved. Try again.",
                };
                focusResult = true;
                return;
            }

            var passed = score >= Lesson.PassMark;
            var courseCompleted = !wasCourseComplete && State.IsComplete(Course.Id);

            string? note = passed
                ? courseCompleted
                    ? "This lesson is complete, and so is the course."
                    : wasPassed ? "You'd already passed; this lesson stays complete." : "This lesson is now complete."
                : wasPassed
                    ? "You passed before, so this lesson stays complete. Your best score counts."
                    : "Read the explanations below, then try again.";

            errorMessage = null;
            showMissing = false;
            result = new QuizOutcome(score, passed, chosen, note);
            focusResult = true;

            if (courseCompleted)
                await OnCourseCompleted.InvokeAsync();
        }
        finally
        {
            submitting = false;
        }
    }

    private void TryAgain()
    {
        result = null;
        errorMessage = null;
        showMissing = false;
        answers = new int?[Lesson.Questions.Count];
        focusQuestion = 0;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private LearningQuizAttempt? Attempt => State.QuizResult(Lesson.Id);

    /// <summary>Gets or sets the course the quiz belongs to.</summary>
    [Parameter, EditorRequired] public LearningCourse Course { get; set; } = default!;

    private bool IsPassed => Attempt is { } attempt && attempt.BestScore >= Lesson.PassMark;

    /// <summary>Gets or sets the quiz lesson.</summary>
    [Parameter, EditorRequired] public LearningLesson Lesson { get; set; } = default!;

    /// <summary>Gets or sets the callback raised when a passed quiz finished the course (its last lesson).</summary>
    [Parameter] public EventCallback OnCourseCompleted { get; set; }

    /// <summary>The status region's box: passed, not passed, or a refusal; none while it's empty.</summary>
    private string? ResultClass => result is { } shown
        ? shown.Passed ? "qp-quiz-result-pass" : "qp-quiz-result-fail"
        : errorMessage is null ? null : "qp-quiz-result-error";

    [Inject] private LearningState State { get; set; } = default!;

    private string TitleId => string.Create(CultureInfo.InvariantCulture, $"qp-quiz-{Lesson.Id}-title");

    #endregion

    #region Nested Types

    /// <summary>A scored submission: right answers, whether it passed, the options chosen, and a note about the lesson.</summary>
    private sealed record QuizOutcome(int Score, bool Passed, IReadOnlyList<int> Answers, string? Note);

    #endregion
}
