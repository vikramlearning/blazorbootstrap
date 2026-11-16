using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// Instructor quiz builder for a quiz lesson of one of the instructor's courses: an ordered list of questions, each a
/// fieldset with its prompt, <see cref="LearningState.MinOptions" /> to <see cref="LearningState.MaxOptions" /> options
/// (one radio group per question picks the correct answer), and an optional explanation. Questions move up and down, are
/// added (up to <see cref="LearningState.MaxQuestionsPerQuiz" />) and removed; the pass mark shows as the list changes.
/// Each field is checked before saving, with its message tied to the input and focus on the first problem after the
/// render that shows it. When learners have results for the quiz, saving a change asks first, naming how many go. A
/// read-only preview shows the questions as learners see them. A lesson that isn't a quiz, or isn't the instructor's,
/// shows a not-found panel.
/// </summary>
public partial class TeachQuizEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How many empty options a new question starts with.</summary>
    private const int NewQuestionOptions = 3;

    private ElementReference addQuestionButton;

    private Button? cancelSaveButton;

    /// <summary>Set by "Save and remove results"; the save runs once the dialog is hidden.</summary>
    private bool confirmSave;

    /// <summary>The questions being edited, in order.</summary>
    private List<QuestionDraft> drafts = new();

    /// <summary>The element to focus after the next render (the render that shows an error, a new field, or a moved question).</summary>
    private Func<ElementReference?>? focusAfterRender;

    private int formErrors;

    private string? formError;

    /// <summary>The course and lesson the drafts were filled from; <see langword="null" /> before the first load.</summary>
    private string? loadedKey;

    /// <summary>The next key for a draft question or option, so rows keep their elements and focus while the list changes.</summary>
    private int nextKey = 1;

    /// <summary>The inputs and buttons of the questions, by element ID, for moving focus.</summary>
    private readonly Dictionary<string, ElementReference> refs = new();

    private Button? saveButton;

    private Modal saveModal = default!;

    private int statusCount;

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

    protected override void OnParametersSet() => EnsureLoaded();

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string CounterText(int length, int max) =>
        length > max
            ? string.Create(CultureInfo.InvariantCulture, $"{length:#,0} of {max:#,0} characters: {length - max:#,0} over the limit")
            : string.Create(CultureInfo.InvariantCulture, $"{length:#,0} of {max:#,0} characters");

    private static string DraftId(QuestionDraft draft) => string.Create(CultureInfo.InvariantCulture, $"qp-quiz-q{draft.Key}");

    private static string? ExplanationProblem(string value) =>
        value.Trim().Length > LearningState.MaxExplanationLength
            ? string.Create(CultureInfo.InvariantCulture, $"Shorten the explanation to {LearningState.MaxExplanationLength} characters or fewer (now {value.Trim().Length}).")
            : null;

    /// <summary>Says how many learners' results there are, like "1 learner's results" or "3 learners' results".</summary>
    private static string LearnerResultsText(int count) =>
        count == 1 ? "1 learner's results" : string.Create(CultureInfo.InvariantCulture, $"{count:#,0} learners' results");

    /// <summary>How many right answers pass a quiz of <paramref name="count" /> questions: two thirds, rounded up (as <see cref="LearningLesson.PassMark" />).</summary>
    private static int PassMarkFor(int count) => (int)Math.Ceiling(count * 2 / 3.0);

    private static string? PromptProblem(string value) =>
        string.IsNullOrWhiteSpace(value) ? "Write the question."
        : value.Trim().Length > LearningState.MaxPromptLength
            ? string.Create(CultureInfo.InvariantCulture, $"Shorten the question to {LearningState.MaxPromptLength} characters or fewer (now {value.Trim().Length}).")
        : null;

    private static string QuestionCountText(int count) =>
        count == 1 ? "1 question" : string.Create(CultureInfo.InvariantCulture, $"{count} questions");

    private static string ResultsSummary(int count) => count == 0 ? "None yet" : LearnerResultsText(count);

    /// <summary>Checks the options of a question: each written, within the limit, and different from the ones before it.</summary>
    private static void ValidateOptions(QuestionDraft draft)
    {
        for (var index = 0; index < draft.Options.Count; index++)
        {
            var text = draft.Options[index].Text.Trim();
            var earlier = draft.Options.Take(index).ToList().FindIndex(option => text.Length > 0 && string.Equals(option.Text.Trim(), text, StringComparison.OrdinalIgnoreCase));

            draft.Options[index].Error = text.Length == 0 ? string.Create(CultureInfo.InvariantCulture, $"Write option {index + 1}, or remove it.")
                : text.Length > LearningState.MaxOptionLength ? string.Create(CultureInfo.InvariantCulture, $"Shorten option {index + 1} to {LearningState.MaxOptionLength} characters or fewer (now {text.Length}).")
                : earlier >= 0 ? string.Create(CultureInfo.InvariantCulture, $"Option {index + 1} is the same as option {earlier + 1}. Make each option different.")
                : null;
        }
    }

    private void AddOption(QuestionDraft draft)
    {
        if (draft.Options.Count >= LearningState.MaxOptions)
            return;

        var option = new OptionDraft { Key = nextKey++ };

        draft.Options.Add(option);
        FocusAfterRender($"{DraftId(draft)}-o{option.Key}");
    }

    private void AddQuestion()
    {
        if (drafts.Count >= LearningState.MaxQuestionsPerQuiz)
            return;

        var draft = new QuestionDraft { Key = nextKey++ };

        for (var index = 0; index < NewQuestionOptions; index++)
            draft.Options.Add(new OptionDraft { Key = nextKey++ });

        drafts.Add(draft);
        SetStatus(string.Create(CultureInfo.InvariantCulture, $"Question {drafts.Count} added. {PassText}. Not saved yet."));
        FocusAfterRender($"{DraftId(draft)}-prompt");
    }

    /// <summary>Re-checks the options of a question that shows option errors, keeping only the ones still true.</summary>
    private void ClearFixedOptionErrors(QuestionDraft draft)
    {
        var shown = draft.Options.Where(option => option.Error is not null).Select(option => option.Key).ToHashSet();

        ValidateOptions(draft);

        // Only messages that were shown stay (updated); a new problem waits for the next save.
        foreach (var option in draft.Options.Where(option => !shown.Contains(option.Key)))
            option.Error = null;
    }

    /// <summary>Saves the drafts; the results confirmation (when it was needed) has been given.</summary>
    private void Commit()
    {
        var questions = drafts.Select(draft => new LearningQuestion(
            draft.Id,
            draft.Prompt.Trim(),
            draft.Options.Select(option => option.Text.Trim()).ToList(),
            draft.Options.FindIndex(option => option.Key == draft.CorrectKey),
            draft.Explanation.Trim())).ToList();

        var result = State.SaveQuiz(CourseId, LessonId, questions, out var cleared);

        switch (result)
        {
            case LearningResult.Ok:
                // The new questions got their numbers: the drafts take them, so the next save updates them instead of adding them again.
                if (Lesson is { } saved && saved.Questions.Count == drafts.Count)
                {
                    for (var index = 0; index < drafts.Count; index++)
                        drafts[index].Id = saved.Questions[index].Id;
                }

                formErrors = 0;
                var message = drafts.Count == 0
                    ? "The quiz is saved with no questions. Add some before you publish it."
                    : $"The quiz is saved: {QuestionCountText(drafts.Count)}; learners pass with {PassMarkFor(drafts.Count)} of {drafts.Count}.";

                if (cleared > 0)
                    message += $" {LearnerResultsText(cleared)} were removed; lessons they completed stay complete.";

                if (Lesson is { IsPublished: false })
                    message += " The lesson is still a draft: publish it in the lesson editor when it's ready.";

                SetStatus(message);
                break;

            case LearningResult.NoChange:
                SetStatus("Nothing changed, so there was nothing to save.");
                break;

            case LearningResult.WrongState:
                ShowFormError("A published quiz needs at least one question. Add a question, or unpublish the lesson in the lesson editor first. Nothing was saved.");
                break;

            case LearningResult.Invalid:
                ShowFormError("The questions couldn't be saved: check each one for missing, repeated, or too-long text. If another tab changed this quiz, reload the page; if the questions look right, this demo tab is full, so reset the demo data. Nothing was saved.");
                break;

            case LearningResult.NotFound:
                ShowFormError("The questions couldn't be saved: the lesson was deleted or is no longer a quiz. Nothing was saved.");
                break;

            case LearningResult.NotAllowed:
                ShowFormError("Only the course's instructor can edit its quiz. Sign in with the instructor account. Nothing was saved.");
                break;

            default:
                ShowFormError("The questions couldn't be saved. Try again.");
                break;
        }
    }

    private async Task ConfirmSaveAsync()
    {
        confirmSave = true;

        await saveModal.HideAsync();
    }

    /// <summary>Fills the drafts for the route once the quiz lesson can be found; a later state change can complete it.</summary>
    private void EnsureLoaded()
    {
        var key = string.Create(CultureInfo.InvariantCulture, $"{CourseId}/{LessonId}");

        if (loadedKey == key || Lesson is not { } lesson)
            return;

        loadedKey = key;
        Load(lesson);
    }

    private void FocusAfterRender(string elementId) =>
        focusAfterRender = () => refs.TryGetValue(elementId, out var element) ? element : null;

    /// <summary>Fills the drafts from the stored questions.</summary>
    private void Load(LearningLesson lesson)
    {
        drafts = lesson.Questions.Select(question =>
        {
            var options = question.Options.Select(text => new OptionDraft { Key = nextKey++, Text = text }).ToList();

            return new QuestionDraft
            {
                Id = question.Id,
                Key = nextKey++,
                Prompt = question.Prompt,
                Options = options,
                CorrectKey = question.CorrectIndex >= 0 && question.CorrectIndex < options.Count ? options[question.CorrectIndex].Key : null,
                Explanation = question.Explanation,
            };
        }).ToList();
        refs.Clear();
        formError = null;
        formErrors = 0;
        statusMessage = null;
        statusText = null;
        statusCount = 0;
    }

    /// <summary>Moves a question up (−1) or down (+1); focus stays on the pressed button, or the other one at an end.</summary>
    private void MoveQuestion(QuestionDraft draft, int offset)
    {
        var index = drafts.IndexOf(draft);
        var target = index + offset;

        if (index < 0 || target < 0 || target >= drafts.Count)
            return;

        (drafts[index], drafts[target]) = (drafts[target], drafts[index]);

        var atEnd = offset < 0 ? target == 0 : target == drafts.Count - 1;
        var button = (offset < 0) != atEnd ? "up" : "down";

        SetStatus(string.Create(CultureInfo.InvariantCulture, $"Question moved {(offset < 0 ? "up" : "down")}: it's now question {target + 1} of {drafts.Count}. Not saved yet."));
        FocusAfterRender($"{DraftId(draft)}-{button}");
    }

    private void OnCorrectChosen(QuestionDraft draft, OptionDraft option)
    {
        draft.CorrectKey = option.Key;
        draft.CorrectError = null;
    }

    private void OnExplanationInput(QuestionDraft draft, ChangeEventArgs args)
    {
        draft.Explanation = args.Value?.ToString() ?? string.Empty;

        if (draft.ExplanationError is not null)
            draft.ExplanationError = ExplanationProblem(draft.Explanation);
    }

    private void OnOptionInput(QuestionDraft draft, OptionDraft option, ChangeEventArgs args)
    {
        option.Text = args.Value?.ToString() ?? string.Empty;

        // A fix here can also clear a "same as" message on another option of the question.
        if (draft.Options.Any(item => item.Error is not null))
            ClearFixedOptionErrors(draft);
    }

    private void OnPromptInput(QuestionDraft draft, ChangeEventArgs args)
    {
        draft.Prompt = args.Value?.ToString() ?? string.Empty;

        if (draft.PromptError is not null)
            draft.PromptError = PromptProblem(draft.Prompt);
    }

    /// <summary>Asks first when saving would remove learners' results; returns focus to Save after "Keep editing".</summary>
    private async Task OnSaveHiddenAsync()
    {
        if (!confirmSave)
        {
            await LearningDisplay.FocusAsync(saveButton?.Element);

            return;
        }

        confirmSave = false;
        Commit();
        focusAfterRender = () => statusRegion;
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        EnsureLoaded();
        StateHasChanged();
    });

    /// <summary>Removes an option (a question keeps <see cref="LearningState.MinOptions" />); focus goes to the option now in its place.</summary>
    private void RemoveOption(QuestionDraft draft, OptionDraft option)
    {
        var index = draft.Options.IndexOf(option);

        if (index < 0 || draft.Options.Count <= LearningState.MinOptions)
            return;

        draft.Options.RemoveAt(index);

        var wasCorrect = draft.CorrectKey == option.Key;

        if (wasCorrect)
            draft.CorrectKey = null;

        if (draft.Options.Any(item => item.Error is not null))
            ClearFixedOptionErrors(draft);

        var number = drafts.IndexOf(draft) + 1;
        var next = draft.Options[Math.Min(index, draft.Options.Count - 1)];

        SetStatus(string.Create(CultureInfo.InvariantCulture,
            $"Option {index + 1} removed from question {number}.{(wasCorrect ? " It was the correct answer: choose another one." : null)} Not saved yet."));
        FocusAfterRender($"{DraftId(draft)}-o{next.Key}");
    }

    /// <summary>Removes a question; focus goes to the question now in its place, the one before it, or Add question.</summary>
    private void RemoveQuestion(QuestionDraft draft)
    {
        var index = drafts.IndexOf(draft);

        if (index < 0)
            return;

        drafts.RemoveAt(index);

        SetStatus(drafts.Count == 0
            ? string.Create(CultureInfo.InvariantCulture, $"Question {index + 1} removed. No questions left. Not saved yet.")
            : string.Create(CultureInfo.InvariantCulture, $"Question {index + 1} removed. {PassText}. Not saved yet."));

        if (drafts.Count == 0)
            focusAfterRender = () => addQuestionButton;
        else
            FocusAfterRender($"{DraftId(drafts[Math.Min(index, drafts.Count - 1)])}-prompt");
    }

    /// <summary>
    /// Checks every field, then saves. Nothing changed: says so. Learners have results for the quiz: asks first, naming
    /// how many go.
    /// </summary>
    private async Task SaveAsync()
    {
        formError = null;

        if (Validate() is { } firstProblem)
        {
            FocusAfterRender(firstProblem);

            return;
        }

        if (IsUnchanged)
        {
            SetStatus("Nothing changed, so there was nothing to save.");

            return;
        }

        if (State.LessonImpact(LessonId).QuizResults > 0)
        {
            confirmSave = false;
            await saveModal.ShowAsync();

            return;
        }

        Commit();
    }

    private string SaveConsequence() =>
        $"{LearnerResultsText(State.LessonImpact(LessonId).QuizResults)} for this quiz will be removed, because they answered other questions; lessons they completed stay complete.";

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>Shows a form-level error; the same error again gets a counter, so a screen reader announces it again.</summary>
    private void ShowFormError(string message)
    {
        formErrors++;
        formError = formErrors == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({formErrors})");
    }

    /// <summary>Checks every field and marks the problems; returns the element ID of the first one, or <see langword="null" />.</summary>
    private string? Validate()
    {
        string? first = null;

        foreach (var draft in drafts)
        {
            var prefix = DraftId(draft);

            draft.PromptError = PromptProblem(draft.Prompt);
            ValidateOptions(draft);
            draft.CorrectError = draft.Options.Any(option => option.Key == draft.CorrectKey) ? null : "Choose the correct answer for this question.";
            draft.ExplanationError = ExplanationProblem(draft.Explanation);

            if (first is not null)
                continue;

            if (draft.PromptError is not null)
                first = $"{prefix}-prompt";
            else if (draft.Options.FirstOrDefault(option => option.Error is not null) is { } invalid)
                first = $"{prefix}-o{invalid.Key}";
            else if (draft.CorrectError is not null)
                first = $"{prefix}-o{draft.Options[0].Key}-correct";
            else if (draft.ExplanationError is not null)
                first = $"{prefix}-explanation";
        }

        return first;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Courses", Href = CoursesUrl },
        new() { Text = Course?.Title ?? "Course", Href = CourseEditUrl },
        new() { Text = Lesson?.Title ?? "Lesson", Href = LessonEditUrl },
        new() { Text = "Questions", IsCurrentPage = true },
    };

    /// <summary>The course (every lesson, drafts included), or <see langword="null" /> when it isn't one of the instructor's.</summary>
    private LearningCourse? Course => State.FindCourseForEdit(CourseId);

    private string CourseEditUrl => string.Create(CultureInfo.InvariantCulture, $"{CoursesUrl}/{CourseId}");

    /// <summary>Gets or sets the course ID from the route.</summary>
    [Parameter] public int CourseId { get; set; }

    private static string CoursesUrl => $"{TeachLayout.Base}/courses";

    /// <summary>Whether the drafts match the stored questions, compared as the state compares them (trimmed).</summary>
    private bool IsUnchanged =>
        Lesson is { } lesson && lesson.Questions.Count == drafts.Count && drafts.Zip(lesson.Questions).All(pair =>
            pair.First.Id == pair.Second.Id
            && pair.First.Prompt.Trim() == pair.Second.Prompt
            && pair.First.Options.Select(option => option.Text.Trim()).SequenceEqual(pair.Second.Options)
            && pair.First.Options.FindIndex(option => option.Key == pair.First.CorrectKey) == pair.Second.CorrectIndex
            && pair.First.Explanation.Trim() == pair.Second.Explanation);

    /// <summary>The quiz lesson, or <see langword="null" /> when it is unknown or another kind.</summary>
    private LearningLesson? Lesson => Course?.Lessons.FirstOrDefault(lesson => lesson.Id == LessonId && lesson.Kind == LearningLessonKind.Quiz);

    private string LessonEditUrl => string.Create(CultureInfo.InvariantCulture, $"{CourseEditUrl}/lessons/{LessonId}");

    /// <summary>Gets or sets the lesson ID from the route.</summary>
    [Parameter] public int LessonId { get; set; }

    /// <summary>The pass mark as the list stands, like "Learners pass with 2 of 3".</summary>
    private string PassText => drafts.Count == 0
        ? "Add a question: learners need at least one to take the quiz"
        : string.Create(CultureInfo.InvariantCulture, $"Learners pass with {PassMarkFor(drafts.Count)} of {drafts.Count}");

    [Inject] private LearningState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>An option being edited.</summary>
    private sealed class OptionDraft
    {
        #region Properties, Indexers

        /// <summary>Gets or sets the message for this option, or <see langword="null" />.</summary>
        public string? Error { get; set; }

        /// <summary>Gets the key that keeps the option's row while the list changes.</summary>
        public int Key { get; init; }

        /// <summary>Gets or sets the option as typed.</summary>
        public string Text { get; set; } = string.Empty;

        #endregion
    }

    /// <summary>A question being edited: its stored ID (0 for a new one), text, options, and correct option.</summary>
    private sealed class QuestionDraft
    {
        #region Properties, Indexers

        /// <summary>Gets or sets the key of the correct option, or <see langword="null" /> when none is chosen.</summary>
        public int? CorrectKey { get; set; }

        /// <summary>Gets or sets the "choose the correct answer" message, or <see langword="null" />.</summary>
        public string? CorrectError { get; set; }

        /// <summary>Gets or sets the explanation as typed (optional).</summary>
        public string Explanation { get; set; } = string.Empty;

        /// <summary>Gets or sets the explanation's message, or <see langword="null" />.</summary>
        public string? ExplanationError { get; set; }

        /// <summary>Gets or sets the stored question ID, 0 until the question is saved.</summary>
        public int Id { get; set; }

        /// <summary>Gets the key that keeps the question's fieldset while the list changes.</summary>
        public int Key { get; init; }

        /// <summary>Gets or sets the options, in order.</summary>
        public List<OptionDraft> Options { get; set; } = new();

        /// <summary>Gets or sets the question as typed.</summary>
        public string Prompt { get; set; } = string.Empty;

        /// <summary>Gets or sets the question's message, or <see langword="null" />.</summary>
        public string? PromptError { get; set; }

        #endregion
    }

    #endregion
}
